using API.Commons;
using API.Helper;
using API.Hubs;
using API.Models;
using API.Services.Interfaces;
using API.ViewModels;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace API.Services
{
    public class NotificationService : INotificationService
    {
        private readonly Sep490Context _context;
        private readonly ILog _logger;
        private readonly IHubContext<NotifyHub> _hubContext;
        public NotificationService(Sep490Context context, ILog log, IHubContext<NotifyHub> hubContext)
        {
            _hubContext = hubContext;
            _logger = log;
            _context = context;
        }

        public async Task<(string, SearchResult?)> GetAlert(NotifySearchVM search, string usertoken)
        {
            var query = _context.Notifications.Include(x => x.CreatedUser).Include(x => x.User)
                .Where(x => x.SendToId == usertoken).AsQueryable();

            if (search.IsRead.HasValue)
                query = query.Where(x => x.IsRead == search.IsRead.Value);

            if (search.DateFrom.HasValue)
                query = query.Where(x => x.CreatedAt >= search.DateFrom.Value);
            if (search.DateTo.HasValue)
                query = query.Where(x => x.CreatedAt <= search.DateTo.Value);

            if (!string.IsNullOrEmpty(search.TextSearch)) 
                query = query.Where(x => x.Message.Contains(search.TextSearch));

            var totalCount = await query.CountAsync();
            var totalPage = (int)Math.Ceiling(totalCount / (double)search.PageSize);

            var data = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((search.CurrentPage - 1) * search.PageSize)
                .Take(search.PageSize)
                .Select(n => new NotificationVM
                {
                    Id = n.NotifyId,
                    Message = n.Message,
                    SendToId = n.SendToId,
                    CreatedAvatar = n.CreatedUser!.AvatarUrl ?? "",
                    CreatedName = n.CreatedUser.FullName!,
                    CreatedEmail = n.CreatedUser.Email!,
                    CreatedBy = n.CreatedBy,
                    IsRead = n.IsRead,
                    Type = n.Type,
                    CreatedAt = n.CreatedAt
                }).ToListAsync();

            if (data.IsObjectEmpty()) return ("No notifications found.", null);
            return ("", new SearchResult
            {
                Result = data,
                TotalPage = totalPage,
                CurrentPage = search.CurrentPage,
                PageSize = search.PageSize,
                Total = totalCount
            });
        }

        public async Task<(string, NotifyDetailVM?)> GetOne(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return ("Notification ID cannot be null or empty.", null);

            var notification = await _context.Notifications.Where(x => x.NotifyId == id)
                    .Include(x=> x.CreatedUser)
                    .Include(x => x.User)
                .AsNoTracking()
                .Select(x => new NotifyDetailVM
                {
                    Id = x.NotifyId,
                    Message = x.Message,
                    CreatedName = x.CreatedUser!.FullName!,
                    CreatedAvatar = x.CreatedUser.AvatarUrl ?? "",
                    CreatedUserCode = x.CreatedUser.UserCode ?? "",
                    StudentName = x.User!.FullName!,
                    StudentAvatar = x.User.AvatarUrl ?? "",
                    StudentUserCode = x.User.UserCode ?? "",
                    IsRead = x.IsRead,
                    Type = x.Type,
                    CreatedAt = x.CreatedAt
                }).FirstOrDefaultAsync();
            if (notification == null) return ("Notification not found.", null);

            return ("", notification);
        }

        public async Task<string> Create(NotificationCreateVM notificationVM, string usertoken)
        {
            if (notificationVM == null) return "Notification data is null";
            var notification = new Notification
            {
                NotifyId = Guid.NewGuid().ToString(),
                Message = notificationVM.Message,
                SendToId = notificationVM.SendToId,
                CreatedBy = usertoken,
                Type = notificationVM.Type,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
            };
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();

            // 🔽 Truy vấn thông tin người tạo
            var createdUser = await _context.Users.Where(u => u.UserId == usertoken)
                .Select(u => new
                {
                    u.FullName,
                    u.Email,
                    u.AvatarUrl
                }).AsNoTracking().FirstOrDefaultAsync();

            await _hubContext.Clients.Group(notification.SendToId)
            .SendAsync(NotifyHub.RECEIVE_NEW_NOTIFY, new NotificationVM
            {
                Id = notification.NotifyId,
                CreatedAvatar = createdUser?.AvatarUrl ?? "",
                CreatedEmail = createdUser?.Email ?? "",
                CreatedName = createdUser?.FullName ?? "",
                Message = notification.Message,
                SendToId = notification.SendToId,
                CreatedBy = notification.CreatedBy,
                Type = notification.Type,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            });
            //var msg = await _logger.WriteActivity(new AddUserLogVM
            //{
            //    UserId = usertoken,
            //    ObjectId = notification.NotifyId,
            //    ActionType = "Create Notification",
            //    Description = $"Notification created with Type: {notification.Type}, Message: {notification.Message}",
            //    Metadata = "",
            //    Status = (int)LogStatus.Success
            //});
            //if (msg.Length > 0) return msg;
            return "";
        }

        public async Task<string> MarkAsRead(string id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null) return "Notification not found";

            notification.IsRead = true;
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();

            var totalUnread = await GetTotalUnread(notification.SendToId);

            await _hubContext.Clients.Group(notification.SendToId)
                .SendAsync(NotifyHub.READ_NOTIFY, new
                {
                    NotificationId = notification.NotifyId,
                    TotalUnread = totalUnread.Item2
                });
            return "";
        }

        public async Task<string> MarkAllAsRead(string usertoken)
        {
            var notifications = await _context.Notifications
                .Where(n => n.SendToId == usertoken && !n.IsRead).ToListAsync();
            if (!notifications.Any()) return "No unread notifications found";

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }
            _context.Notifications.UpdateRange(notifications);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group(usertoken)
                .SendAsync(NotifyHub.READ_ALL_NOTIFY, new
                {
                    TotalUnread = 0
                });
            return "";
        }

        public async Task<(string, int)> GetTotalUnread(string usertoken)
        {
            var count = await _context.Notifications
                .CountAsync(n => n.SendToId == usertoken && !n.IsRead);
            return ("", count);
        }

        public async Task<string> Delete(List<string> ids)
        {
            if (ids.IsObjectEmpty()) return "Notification IDs cannot be null or empty.";

            var notifications = await _context.Notifications.Where(n => ids.Contains(n.NotifyId)).ToListAsync();
            if (notifications.IsObjectEmpty()) return "No notifications found for the provided IDs.";

            _context.Notifications.RemoveRange(notifications);
            await _context.SaveChangesAsync();

            //var msg = await _logger.WriteActivity(new AddUserLogVM
            //{
            //    UserId = notifications.First().CreatedBy,
            //    ObjectId = string.Join(", ", notifications.Select(n => n.NotifyId)),
            //    ActionType = "Delete Notifications",
            //    Description = $"Deleted {notifications.Count} notifications.",
            //    Metadata = string.Join(", ", notifications.Select(n => n.Message)),
            //    Status = (int)LogStatus.Success
            //});
            //if (msg.Length > 0) return msg;
            return "";
        }

        public async Task SendToAdmins(string usertoken, string message)
        {
            var adminIds = await _context.Users
                .Include(x => x.UserRoles)
                .Where(x => x.UserRoles.Any(r => r.RoleId == (int)RoleEnum.Admin))
                .Select(x => x.UserId)
                .ToListAsync();

            var notifications = adminIds.Select(adminId => new Notification
            {
                NotifyId = Guid.NewGuid().ToString(),
                Message = $"New Feedback: {message}",
                SendToId = adminId,
                CreatedBy = usertoken,
                Type = "Feedback",
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
            }).ToList();

            await _context.Notifications.AddRangeAsync(notifications);
            await _context.SaveChangesAsync();

            foreach (var notify in notifications)
            {
                await _hubContext.Clients.Group(notify.SendToId)
                    .SendAsync(NotifyHub.RECEIVE_NEW_NOTIFY, new NotificationVM
                    {
                        Id = notify.NotifyId,
                        Message = notify.Message,
                        SendToId = notify.SendToId,
                        CreatedBy = notify.CreatedBy,
                        Type = notify.Type,
                        IsRead = notify.IsRead,
                        CreatedAt = notify.CreatedAt
                    });
            }
        }

    }
}

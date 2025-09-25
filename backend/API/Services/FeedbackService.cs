using API.Commons;
using API.Models;
using API.Services.Interfaces;
using API.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly Sep490Context _context;
        private readonly INotificationService _notificationService;
        public FeedbackService(Sep490Context context, INotificationService notificationService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _notificationService = notificationService;
        }

        public async Task<(string, SearchResult?)> GetList(FeedbackSearchVM search)
        {
            var query = _context.Feedbacks.Include(fb => fb.User).AsQueryable();
            if (!search.TextSearch.IsEmpty())
            {
                var loweredText = search.TextSearch!.ToLower();
                query = query.Where(a =>
                    a.Title.ToLower().Contains(loweredText) ||
                    a.Content.ToLower().Contains(loweredText) ||
                    (a.User.FullName ?? "").ToLower().Contains(loweredText));
            }
            if (search.DateFrom.HasValue)
                query = query.Where(fb => fb.CreatedAt >= search.DateFrom.Value);
            if (search.DateTo.HasValue)
                query = query.Where(fb => fb.CreatedAt <= search.DateTo.Value);

            var totalCount = await query.CountAsync();
            var totalPage = (int)Math.Ceiling(totalCount / (double)search.PageSize);

            if (!query.Any()) return ("No feedbacks found.", null);
            var data = await query
                .OrderByDescending(fb => fb.CreatedAt)
                .Skip((search.CurrentPage - 1) * search.PageSize)
                .Take(search.PageSize)
                .Select(fb => new
                {
                    FeedbackId = fb.Id,
                    fb.Title,
                    fb.Content,
                    StudentId = fb.UserId,
                    StudentName = fb.User.FullName,
                    StudentEmail = fb.User.Email,
                    StudentCode = fb.User.UserCode,
                    fb.CreatedAt
                }).ToListAsync();

            return ("", new SearchResult
            {
                Result = data,
                TotalPage = totalPage,
                CurrentPage = search.CurrentPage,
                PageSize = search.PageSize,
                Total = totalCount
            });
        }

        public async Task<(string, object?)> GetOne(string id)
        {
            var fb = await _context.Feedbacks.Include(fb => fb.User)
                .Select(fb => new
                {
                    FeedbackId = fb.Id,
                    fb.Title,
                    fb.Content,
                    StudentId = fb.UserId,
                    StudentName = fb.User.FullName,
                    StudentEmail = fb.User.Email,
                    StudentCode = fb.User.UserCode,
                    fb.CreatedAt
                }).FirstOrDefaultAsync(fb => fb.FeedbackId == id);

            if (fb == null) return ("No found feedback", null);
            return ("", fb);
        }

        public async Task<string> CreateUpdate(CreateUpdateFeedbackVM model, string usertoken)
        {
            if (model.Id.IsEmpty())
            {
                var feedback = new Feedback
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = model.Title,
                    Content = model.Content,
                    UserId = usertoken,
                    CreatedAt = DateTime.UtcNow,
                };
                await _context.Feedbacks.AddAsync(feedback);
                await _context.SaveChangesAsync();
                // Gọi gửi thông báo cho admin
                await _notificationService.SendToAdmins(usertoken, feedback.Title);
            }
            else
            {
                var feedback = await _context.Feedbacks.FindAsync(model.Id);
                if (feedback == null) return "Feedback not found.";

                feedback.Title = model.Title;
                feedback.Content = model.Content;

                _context.Update(feedback);
                await _context.SaveChangesAsync();
            }
            return "";
        }

        //public async Task<string> Resolve(string id, string responseContent, string usertoken)
        //{
        //    if (id.IsEmpty()) return "Feedback ID cannot be null or empty.";
        //    if (responseContent.IsEmpty()) return "Response content cannot be empty.";

        //    var feedback = await _context.Feedbacks.FindAsync(id);
        //    if (feedback == null) return "Feedback not found.";

        //    feedback.IsResolved = true;
        //    feedback.ResponseContent = responseContent;
        //    feedback.ResponseAt = DateTime.UtcNow;

        //    _context.Feedbacks.Update(feedback);
        //    await _context.SaveChangesAsync();
        //    return "";
        //}


        public async Task<string> Delete(string id)
        {
            if (id.IsEmpty()) return "Feedback ID cannot be null or empty.";
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null) return "Feedback not found.";

            _context.Feedbacks.Remove(feedback);
            await _context.SaveChangesAsync();
            return "";
        }
    }
}

using API.Commons;
using API.Helper;
using API.Models;
using API.Services.Interfaces;
using API.Utilities;
using API.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class RoomUserService : IRoomUserService
    {
        private readonly Sep490Context _context;
        private readonly ILog _logger;

        public RoomUserService(Sep490Context context, ILog log)
        {
            _logger = log ?? throw new ArgumentNullException(nameof(log));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<(string, SearchResult?)> GetUsersNotInRoom(SearchUserRoomExamVM search)
        {
            // Lấy danh sách userId đã có trong RoomUser
            var existingUserIds = await _context.RoomUsers.Where(ru => ru.RoomId == search.RoomId)
                .Select(ru => ru.UserId).AsNoTracking().ToListAsync();

            // Query tất cả học sinh chưa có mặt trong Room
            var query = _context.Users.Include(u => u.UserRoles).Include(u => u.Major)
                .Where(u => !existingUserIds.Contains(u.UserId) && u.UserRoles.Any(r => r.RoleId == (int)RoleEnum.Student))
                .AsNoTracking().AsQueryable();

            if (!search.TextSearch.IsEmpty())
            {
                var text = search.TextSearch!.ToLower();
                query = query.Where(u => u.FullName!.ToLower().Contains(text) ||
                    u.UserCode.ToLower().Contains(text) || u.Email!.ToLower().Contains(text));
            }
            var totalCount = await query.CountAsync();
            var totalPage = (int)Math.Ceiling(totalCount / (double)search.PageSize);

            var results = await query
                .OrderBy(u => u.CreateAt)
                .Skip((search.CurrentPage - 1) * search.PageSize)
                .Take(search.PageSize)
                .Select(x => new RoomUserVM
                {
                    UserId = x.UserId,
                    FullName = x.FullName,
                    UserCode = x.UserCode,
                    Major = x.Major!.Name,
                    RoleId = x.UserRoles.FirstOrDefault()!.RoleId,
                }).ToListAsync();
            if (results.IsObjectEmpty()) return ("No users found.", null);

            return ("", new SearchResult
            {
                Result = results,
                TotalPage = totalPage,
                PageSize = search.PageSize,
                CurrentPage = search.CurrentPage,
                Total = totalCount
            });
        }

        public async Task<(string, SearchResult?)> GetUsersInRoom(SearchRoomUserVM search)
        {
            if (search.RoomId.IsEmpty()) return ("RoomId is required.", null);

            var query = _context.RoomUsers.Include(ru => ru.User).ThenInclude(x=> x.UserRoles).Include(ru => ru.Room)
                .Where(ru => ru.RoomId == search.RoomId).AsNoTracking()
                .AsQueryable();

            if (search.Role != null && search.Role.Any())
                query = query.Where(ru => search.Role.Select(r => (int)r).ToList().Contains(ru.RoleId));

            if (search.Status.HasValue)
                query = query.Where(ru => ru.Status == (int)search.Status.Value);

            if (!search.TextSearch.IsEmpty())
            {
                var loweredText = search.TextSearch!.ToLower();
                query = query.Where(ru => ru.User != null && (ru.User.FullName.ToLower().Contains(loweredText)
                || ru.User.Email.ToLower().Contains(loweredText) || ru.User.UserCode.ToLower().Contains(loweredText)));
            }

            var totalCount = await query.CountAsync();
            var totalPage = (int)Math.Ceiling(totalCount / (double)search.PageSize);

            var roomUserList = await query
                .OrderByDescending(ru => ru.JoinTime)
                .Skip((search.CurrentPage - 1) * search.PageSize)
                .Take(search.PageSize)
                .ToListAsync();
            if (!roomUserList.Any()) return ("No users found.", null);

            var room = roomUserList.First().Room;
            var result = new RoomWithUserVM
            {
                RoomId = room.RoomId,
                RoomCode = room.RoomCode,
                Description = room.Description,
                IsRoomActive = room.IsActive,
                Users = roomUserList.Select(ru => new UserInRoomVM
                {
                    RoomId = ru.RoomId,
                    RoomUserId = ru.RoomUserId,
                    UserId = ru.UserId,
                    Role = ru.User.UserRoles.FirstOrDefault()!.RoleId,
                    UserStatus = ru.Status,
                    JoinTime = ru.JoinTime,
                    UpdatedAt = ru.UpdatedAt,
                    User = new UserVM
                    {
                        UserId = ru.User.UserId,
                        Fullname = ru.User.Email,
                        UserCode = ru.User.UserCode,
                        Email = ru.User.Email,
                        Avatar = ru.User.AvatarUrl,
                        Sex = ru.User.Sex,
                        CreateAt = ru.User.CreateAt,
                        UpdateAt = ru.User.UpdateAt,
                    }
                }).ToList()
            };

            return ("", new SearchResult
            {
                Result = result,
                TotalPage = totalPage,
                CurrentPage = search.CurrentPage,
                PageSize = search.PageSize,
                Total = totalCount
            });
        }

        public async Task<string> RemoveUsersFromRoom(string roomId, List<string> userIds, string usertoken)
        {
            if (roomId.IsEmpty() || userIds.IsObjectEmpty())
                return "RoomId and list of UserIds cannot be null or empty.";
            // Lấy Room để lấy các ExamId thuộc room 
            var examsInRoom = await _context.Exams.Where(e => e.RoomId == roomId).Select(e => e.ExamId).ToListAsync();
            if (examsInRoom.Any())
            {
                // Nếu có exam trong phòng thì mới check user đang thi
                var inProgressUserIds = await _context.StudentExams
                    .Where(se => examsInRoom.Contains(se.ExamId)
                        && userIds.Contains(se.StudentId)
                        && se.Status == (int)StudentExamStatus.InProgress
                        )
                    .Select(se => se.StudentId)
                    .Distinct()
                    .ToListAsync();
                if (inProgressUserIds.Any())
                {
                    var names = await _context.Users
                        .Where(u => inProgressUserIds.Contains(u.UserId))
                        .Select(u => u.FullName ?? u.UserCode)
                        .ToListAsync();
                    return $"Cannot remove users who are currently taking an exam: {string.Join(", ", names)}";
                }
            }
            var roomUsers = await _context.RoomUsers.Where(ru => ru.RoomId == roomId && userIds.Contains(ru.UserId)).ToListAsync();
            if (roomUsers.IsObjectEmpty()) return "No matching users found in the room.";

            _context.RoomUsers.RemoveRange(roomUsers);
            await _context.SaveChangesAsync();

            var msg = await _logger.WriteActivity(new AddUserLogVM
            {
                ActionType = "Remove",
                Description = "Users have been removed from the room.",
                ObjectId = roomId,
                UserId = usertoken,
                Metadata = string.Join(", ", userIds),
                Status = (int)LogStatus.Success,
            });
            if (msg.Length > 0) return msg;
            return "";
        }

        public async Task<(string, List<string>, List<string>, List<string>)> AddStudentsToRoom(string roomId, List<string> userCodesOrIds, string usertoken)
        {
            if (roomId.IsEmpty() || userCodesOrIds.IsObjectEmpty())
                return ("Room ID and User Codes/IDs cannot be null or empty.", new(), new(), new());

            var roomExists = await _context.Rooms.FindAsync(roomId);
            if (roomExists == null) return ("Room not found.", new(), new(), new());

            // Tìm tất cả user có UserId hoặc UserCode khớp với input
            var users = await _context.Users.Where(u => userCodesOrIds.Contains(u.UserId) || userCodesOrIds.Contains(u.UserCode!))
                .ToListAsync();

            // Tạo map để tra UserId từ bất kỳ đầu vào nào
            var inputToUserId = new Dictionary<string, string>();
            foreach (var user in users)
            {
                if (user.UserCode != null)
                    inputToUserId[user.UserCode] = user.UserId;
                inputToUserId[user.UserId] = user.UserId;
            }

            // Tìm các input không hợp lệ
            var invalidInputs = userCodesOrIds.Where(input => !inputToUserId.ContainsKey(input)).ToList();
            var allUserIds = inputToUserId.Values.Distinct().ToList();
            // Tìm user đã có trong room
            var duplicatedUserIds = await _context.RoomUsers.Where(ru => ru.RoomId == roomId && allUserIds.Contains(ru.UserId))
                .Select(ru => ru.UserId).ToListAsync();

            var duplicatedInputs = inputToUserId.Where(kvp => duplicatedUserIds.Contains(kvp.Value))
                .Select(kvp => kvp.Key).Distinct().ToList();

            // Danh sách user hợp lệ cần thêm
            var validNewInputs = userCodesOrIds.Except(invalidInputs).Except(duplicatedInputs).ToList();

            // Kiểm tra capacity
            var currentCount = await _context.RoomUsers.CountAsync(ru => ru.RoomId == roomId);
            var availableSlots = roomExists.Capacity - currentCount;

            if (availableSlots <= 0)
                return ($"Room is full. Capacity = {roomExists.Capacity}, Current = {currentCount}.", new(), invalidInputs, duplicatedInputs);

            if (validNewInputs.Count > availableSlots)
                return ($"Not enough capacity. Available slots: {availableSlots}, Requested: {validNewInputs.Count}.", new(), invalidInputs, duplicatedInputs);

            var newRoomUsers = new List<RoomUser>();
            var addedInputs = new List<string>();
            foreach (var input in validNewInputs)
            {
                var uid = inputToUserId[input]; 
                newRoomUsers.Add(new RoomUser
                {
                    RoomUserId = Guid.NewGuid().ToString(),
                    RoomId = roomId,
                    UserId = uid,
                    RoleId = (int)RoleEnum.Student,
                    Status = (int)ActiveStatus.Active,
                    JoinTime = DateTime.UtcNow,
                });
                addedInputs.Add(input);
            }

            if (newRoomUsers.Any())
            {
                await _context.RoomUsers.AddRangeAsync(newRoomUsers);
                await _context.SaveChangesAsync();
            }
            else return ("No new users to add.", addedInputs, invalidInputs, duplicatedInputs);

            var msg = await _logger.WriteActivity(new AddUserLogVM
            {
                ActionType = "Add",
                Description = $"User(s) [{string.Join(", ", newRoomUsers)}] have been added to the room.",
                ObjectId = roomId,
                UserId = usertoken,
                Metadata = string.Join(", ", addedInputs),
                Status = (int)LogStatus.Success,
            });
            if (msg.Length > 0) return (msg, addedInputs, invalidInputs, duplicatedInputs);
            return ("", addedInputs, invalidInputs, duplicatedInputs);
        }

        public async Task<string> AssignTeacherToRoom(string roomId, string userId, string usertoken)
        {
            if (string.IsNullOrEmpty(roomId) || string.IsNullOrEmpty(userId))
                return "Room ID and User ID cannot be null or empty.";

            var roomExists = await _context.Rooms.FindAsync(roomId);
            if (roomExists == null) return "Room does not exist.";

            var existingRoomUser = await _context.RoomUsers.AnyAsync(ru => ru.RoomId == roomId && ru.UserId == userId);
            if (existingRoomUser) return "User is already assigned to this room.";

            var userExist = await _context.Users.Include(u => u.UserRoles)
                .AnyAsync(u => u.UserId == userId && u.UserRoles.Any(ur => ur.RoleId == (int)RoleEnum.Lecture));
            if (!userExist) return "This user is not a lecture.";

            var roomUser = new RoomUser
            {
                RoomUserId = Guid.NewGuid().ToString(),
                RoomId = roomId,
                UserId = userId,
                RoleId = (int)RoleEnum.Lecture,
                Status = (int)ActiveStatus.Active,
                JoinTime = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.RoomUsers.AddAsync(roomUser);
            await _context.SaveChangesAsync();

            var msg = await _logger.WriteActivity(new AddUserLogVM
            {
                ActionType = "Assign",
                Description = "A teacher has been assigned to the room.",
                ObjectId = roomId,
                UserId = usertoken,
                Metadata = roomExists.RoomCode,
                Status = (int)LogStatus.Success,
            });
            if (msg.Length > 0) return msg;
            return "";
        }

        public async Task<string> UpdateRoomUser(UpdateRoomUserVM input, string usertoken)
        {
            var roomUser = await _context.RoomUsers.Include(x=> x.Room)
                .FirstOrDefaultAsync(ru => ru.RoomUserId == input.RoomUserId && ru.RoomId == input.RoomId && ru.UserId == input.UserId);
            if (roomUser == null) return "RoomUser not found.";

            roomUser.RoleId = (int)input.RoleId;
            roomUser.Status = (int)input.Status;
            roomUser.UpdatedAt = DateTime.UtcNow;

            _context.RoomUsers.Update(roomUser);
            await _context.SaveChangesAsync();

            var msg = await _logger.WriteActivity(new AddUserLogVM
            {
                ActionType = "Update",
                Description = $"RoomUser has been updated in RoomCode: [{roomUser.Room.RoomCode}].",
                ObjectId = input.RoomId,
                UserId = usertoken,
                Metadata = $"UserId: {input.UserId} - RoleId: {input.RoleId} - Status: {input.Status} - RoomId: {roomUser.RoomId}",
                Status = (int)LogStatus.Success,
            });
            if (msg.Length > 0) return msg;
            return "";
        }

        public async Task<(string, MemoryStream?)> Export(string? roomId)
        {
            var list = await _context.RoomUsers.Include(x => x.User)
                 .Include(x => x.Room).ThenInclude(r => r.Subject)
                 .Include(x => x.Room).ThenInclude(r => r.Class)
                 .Where(x => string.IsNullOrEmpty(roomId) || x.RoomId == roomId)
                 .Select(x => new ExportUserRoom
                 {
                     RoomId = x.RoomId,
                     RoomCode = x.Room.RoomCode,
                     ClassName = x.Room.Class.ClassCode,
                     SubjectName = x.Room.Subject.SubjectName,
                     UserId = x.UserId,
                     UserCode = x.User.UserCode,
                     FullName = x.User.FullName,
                     Email = x.User.Email,
                     Sex = x.User.Sex,
                     Status = x.Status,
                     Avatar = x.User.AvatarUrl,
                 })
                 .ToListAsync();
            if (list.IsObjectEmpty()) return ("No user in room found.", null);
            var file = FileHandler.GenerateExcelFile(list);
            if (file == null) return ("Error export data.", null);

            return ("", file);
        }

        public async Task<(string, object?)> Import(IFormFile fileData, string roomId, string usertoken)
        {
            var msg = FileHandler.ImportFromExcel(fileData, out List<ImportRoomUser> importedData);
            if (!msg.IsEmpty()) return (msg, null);
            if (importedData == null || importedData.Count == 0)
                return ("No data found in the file.", null);

            var codeOrIds = importedData.Where(x => !x.UserCode.IsEmpty())
                .Select(x => x.UserCode!.Trim()).Distinct().ToList();
            if (codeOrIds.Count == 0) return ("No valid user codes or IDs found.", null);

            var (msgError, added, invalid, duplicated) = await AddStudentsToRoom(roomId, codeOrIds, usertoken);
            if (msgError.Length > 0) return (msgError, null);

            return ("", new
            {
                addedInputs = added,
                invalidInputs = invalid,
                duplicatedInputs = duplicated
            });
        }

        public async Task<string> ToggleActive(ToggleActiveRoomUserVM input, string usertoken)
        {
            var students = await _context.RoomUsers.Where(a => a.RoomId == input.RoomId && input.StudentId.Contains(a.UserId)).ToListAsync();
            if (students.IsObjectEmpty()) return ("No student found.");

            foreach (var stu in students)
            {
                stu.Status = (int)input.Status;
                stu.UpdatedAt = DateTime.UtcNow;
            }
            _context.RoomUsers.UpdateRange(students);
            await _context.SaveChangesAsync();

            var msg = await _logger.WriteActivity(new AddUserLogVM
            {
                ActionType = "Toggle",
                Description = "Room user status has been toggled.",
                ObjectId = input.RoomId,
                UserId = usertoken,
                Metadata = $"{string.Join(", ", input.StudentId)} - {input.Status}",
                Status = (int)LogStatus.Success,
            });
            if (msg.Length > 0) return msg;
            return ("");
        }
    }
}

using API.Commons;
using API.Helper;
using API.Models;
using API.Services.Interfaces;
using API.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class RoomService : IRoomService
    {
        private readonly Sep490Context _context;
        private readonly ILog _log;
        public RoomService(Sep490Context context, ILog log)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _log = log;
        }

        public async Task<(string, SearchResult?)> GetAllRooms(SearchRoomVM search)
        {
            var query = _context.Rooms.Include(r => r.Class).Include(r => r.Subject)
               .Include(r => r.RoomUsers).ThenInclude(ru => ru.User).AsQueryable();

            if (!search.ClassId.IsEmpty())
                query = query.Where(r => r.ClassId == search.ClassId);

            if (!search.SubjectId.IsEmpty())
                query = query.Where(r => r.SubjectId == search.SubjectId);

            if (search.IsActive.HasValue)
                query = query.Where(r => r.IsActive == search.IsActive.Value);

            if (!search.TextSearch.IsEmpty())
            {
                string keyword = search.TextSearch.ToLower();
                query = query.Where(r => r.Subject.SubjectName.ToLower().Contains(keyword)
                || r.Class.ClassCode.ToLower().Contains(keyword)
                || r.RoomCode.ToLower().Contains(keyword)
                );
            }
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / search.PageSize);

            var rooms = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((search.CurrentPage - 1) * search.PageSize)
                .Take(search.PageSize)
                .Select(r => new RoomVM
                {
                    RoomId = r.RoomId,
                    RoomCode = r.RoomCode,
                    Capacity = r.Capacity,
                    RoomDescription = r.Description,
                    IsRoomActive = r.IsActive,
                    RoomCreatedAt = r.CreatedAt,
                    RoomUpdatedAt = r.UpdatedAt,

                    ClassId = r.Class.ClassId,
                    ClassCode = r.Class.ClassCode,
                    ClassDescription = r.Class.Description,
                    //  ClassMaxStudent = r.Class.MaxStudent,
                    IsClassActive = r.Class.IsActive,
                    ClassCreatedBy = r.Class.CreatedBy,
                    ClassStartDate = r.Class.StartDate,
                    ClassEndDate = r.Class.EndDate,

                    SubjectId = r.Subject.SubjectId,
                    SubjectName = r.Subject.SubjectName,
                    SubjectCode = r.Subject.SubjectCode,
                    SubjectContent = r.Subject.SubjectContent,
                    SubjectDescription = r.Subject.SubjectDescription,
                    SubjectStatus = r.Subject.Status,

                    TotalUsers = r.RoomUsers.Count,
                    RoomTeachers = r.RoomUsers.Where(ru => ru.RoleId == (int)RoleEnum.Lecture
                        && ru.Status == (int)ActiveStatus.Active && ru.User != null).Select(ru => ru.User!.Email).ToList()
                }).ToListAsync();
            if (rooms.Count == 0) return ("No rooms found.", null);

            return ("", new SearchResult
            {
                Result = rooms,
                TotalPage = totalPages,
                CurrentPage = search.CurrentPage,
                PageSize = search.PageSize,
                Total = totalItems
            });
        }

        public async Task<(string, RoomVM?)> GetRoomByIdAsync(string roomId)
        {
            if (roomId.IsEmpty()) return ("Room Id not found!", null);

            var room = await _context.Rooms
                .Include(r => r.Class).Include(r => r.Subject)
                .Include(r => r.RoomUsers).ThenInclude(ru => ru.User)
                .Select(r => new RoomVM
                {
                    RoomId = r.RoomId,
                    RoomCode = r.RoomCode,
                    Capacity = r.Capacity,
                    RoomDescription = r.Description,
                    IsRoomActive = r.IsActive,
                    RoomCreatedAt = r.CreatedAt,
                    RoomUpdatedAt = r.UpdatedAt,

                    ClassId = r.Class.ClassId,
                    ClassCode = r.Class.ClassCode,
                    // ClassMaxStudent = r.Class.MaxStudent,
                    ClassCreatedBy = r.Class.CreatedBy,
                    IsClassActive = r.Class.IsActive,
                    ClassStartDate = r.Class.StartDate,
                    ClassEndDate = r.Class.EndDate,

                    SubjectContent = r.Subject.SubjectContent,
                    SubjectDescription = r.Subject.SubjectDescription,
                    SubjectStatus = r.Subject.Status,
                    SubjectId = r.Subject.SubjectId,
                    SubjectName = r.Subject.SubjectName,
                    SubjectCode = r.Subject.SubjectCode,

                    TotalUsers = r.RoomUsers.Count,
                    RoomTeachers = r.RoomUsers.Where(ru => ru.RoleId == (int)RoleEnum.Lecture
                    && ru.Status == (int)ActiveStatus.Active && ru.User != null).Select(ru => ru.User!.FullName).ToList()
                })
                .FirstOrDefaultAsync(r => r.RoomId == roomId);

            if (room == null) return ("Room not found.", null);
            return ("", room);
        }

        public async Task<string> ChangeActivateRoom(string roomId, string usertoken)
        {
            if (roomId.IsEmpty()) return "Room Id cannot be null or empty.";

            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null) return "Room not found.";

            room.IsActive = !room.IsActive;
            room.UpdatedAt = DateTime.UtcNow;

            _context.Rooms.Update(room);
            await _context.SaveChangesAsync();

            var msg = await _log.WriteActivity(new AddUserLogVM
            {
                ActionType = room.IsActive ? "Activate Room" : "Deactivate Room",
                Description = $"Room {room.RoomCode} has been {(room.IsActive ? "activated" : "deactivated")}.",
                ObjectId = room.RoomId,
                Metadata = room.RoomCode,
                UserId = usertoken,
                Status = (int)LogStatus.Success,
            });
            if (msg.Length > 0) return msg;
            return "";
        }

        public async Task<string> DoRemoveRoom(string roomId, string usertoken)
        {
            if (roomId.IsEmpty()) return "Room Id cannot be null or empty.";
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null) return "Room not found.";

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

            var msg = await _log.WriteActivity(new AddUserLogVM
            {
                ActionType = "Remove Room",
                Description = $"Room {room.RoomCode} has been removed.",
                ObjectId = room.RoomId,
                Metadata = room.RoomCode,
                UserId = usertoken, // Assuming no user is associated with this action
                Status = (int)LogStatus.Success,
            });
            if (msg.Length > 0) return msg;
            return "";
        }

        public async Task<string> CreateUpdateRoomVM(CreateUpdateRoomVM roomVM, string usertoken)
        {
            if (roomVM == null) return "Room data cannot be null.";

            var existingRoom = await _context.Rooms.AnyAsync(r => r.RoomCode == roomVM.RoomCode && (roomVM.RoomId.IsEmpty() || r.RoomId != roomVM.RoomId));
            if (existingRoom) return "This RoomCode is already in use. Please enter a different one.";

            var existingSubject = await _context.Subjects.AnyAsync(x => x.SubjectId == roomVM.SubjectId);
            if (!existingSubject) return "Please select a subject valid.";

            var existingClass = await _context.Classes.AnyAsync(x => x.ClassId == roomVM.ClassId);
            if (!existingClass) return "Please select a class valid";

            if (roomVM.RoomId.IsEmpty())
            {
                var newRoom = new Room
                {
                    RoomId = Guid.NewGuid().ToString(),
                    ClassId = roomVM.ClassId,
                    SubjectId = roomVM.SubjectId,
                    IsActive = roomVM.IsActive,
                    Description = roomVM.RoomDescription,
                    RoomCode = roomVM.RoomCode,
                    Capacity = roomVM.Capacity,
                    CreatedAt = DateTime.UtcNow,
                };
                await _context.Rooms.AddAsync(newRoom);
            }
            else
            {
                var room = await _context.Rooms.FindAsync(roomVM.RoomId);
                if (room == null) return "Room not found.";

                room.ClassId = roomVM.ClassId;
                room.RoomCode = roomVM.RoomCode;
                room.Description = roomVM.RoomDescription;
                room.Capacity = roomVM.Capacity;
                room.SubjectId = roomVM.SubjectId;
                room.IsActive = roomVM.IsActive;
                room.UpdatedAt = DateTime.UtcNow;

                _context.Rooms.Update(room);
            }
            await _context.SaveChangesAsync();
            var msg = await _log.WriteActivity(new AddUserLogVM
            {
                ActionType = roomVM.RoomId.IsEmpty() ? "Create" : "Update",
                Description = $"A room has been {(roomVM.RoomId.IsEmpty() ? "created" : "updated")}.",
                ObjectId = roomVM.RoomId,
                Metadata = roomVM.RoomCode,
                UserId = usertoken,
                Status = (int)LogStatus.Success,
            });
            return "";
        }
    }

}

using API.Commons;
using API.Helper;
using API.Models;
using API.Services.Interfaces;
using API.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;

namespace API.Services
{
    public class ExamSupervisorService : IExamSupervisorService
    {
        private readonly Sep490Context _context;
        private readonly ILog _log;
        public ExamSupervisorService(Sep490Context context, ILog log)
        {
            _context = context;
            _log = log;
        }

        public async Task<(string, object?)> GetAll(string examId, string usertoken)
        {
            var isAdmin = await _context.Users.Include(u => u.UserRoles).AnyAsync(u => u.UserId == usertoken && u.UserRoles.Any(r => r.RoleId == (int)RoleEnum.Admin));
            if (!isAdmin)
            {
                var isCreated = await _context.ExamSupervisors.AnyAsync(x => x.ExamId == examId && x.CreatedBy == usertoken);
                if (!isCreated) return ("You do not have permission to view this exam's supervisors.", null);
            }

            var exam = await _context.Exams.Include(x => x.Room.Subject).Include(x => x.Room.Class)
                .Where(e => e.ExamId == examId)
                .Select(e => new
                {
                    e.ExamId,
                    e.Title,
                    e.Room.Subject.SubjectName,
                    e.Room.Class.ClassCode,
                    e.Room.RoomCode,
                    e.StartTime,
                    e.EndTime,

                }).AsNoTracking().FirstOrDefaultAsync();
            if (exam == null) return ("Exam not found.", null);

            var supervisors = await _context.ExamSupervisors
              .Where(x => x.ExamId == examId)
              .Include(x => x.User.Department)
              .Include(x => x.User.Specialization)
              .Include(x => x.User.Major)
              .Include(x => x.CreatedUser.Department)
              .Include(x => x.CreatedUser.Specialization)
              .Include(x => x.CreatedUser.Major)
              .AsSplitQuery()
              .AsNoTracking()
              .Select(x => new SupervisorVM
              {
                  UserId = (x.User != null ? x.User.UserId : x.CreatedUser != null ? x.CreatedUser.UserId : ""),
                  FullName = (x.User != null ? x.User.FullName : x.CreatedUser != null ? x.CreatedUser.FullName : "") ?? "",
                  Email = (x.User != null ? x.User.Email : x.CreatedUser != null ? x.CreatedUser.Email : "") ?? "",
                  UserCode = (x.User != null ? x.User.UserCode : x.CreatedUser != null ? x.CreatedUser.UserCode : "") ?? "",
                  Phone = (x.User != null ? x.User.Phone : x.CreatedUser != null ? x.CreatedUser.Phone : "") ?? "",
                  AvatarUrl = x.User != null ? x.User.AvatarUrl : x.CreatedUser != null ? x.CreatedUser.AvatarUrl : null,
                  Department = (x.User != null ? x.User.Department.Name : x.CreatedUser != null ? x.CreatedUser.Department.Name : "") ?? "",
                  Major = (x.User != null ? x.User.Major.Name : x.CreatedUser != null ? x.CreatedUser.Major.Name : "") ?? "",
                  Specialization = (x.User != null ? x.User.Specialization.Name : x.CreatedUser != null ? x.CreatedUser.Specialization.Name : "") ?? "",
                  AssignAt = x.CreatedAt
              }).ToListAsync();
            if (supervisors.Count == 0) return ("No supervisor found for this exam.", null);

            var result = new ExamSupervisorVM
            {
                ExamId = exam.ExamId,
                ExamTitle = exam.Title ?? "",
                SubjectName = exam.SubjectName ?? "",
                ClassCode = exam.ClassCode ?? "",
                RoomCode = exam.RoomCode ?? "",
                StartTime = exam.StartTime,
                EndTime = exam.EndTime,
                SupervisorVMs = supervisors
            };
            return ("", result);
        }

        public async Task<(string, SearchResult?)> GetSupervisors(SearchRequestVM search)
        {
            var query = _context.Users.Include(u => u.UserRoles).Include(u => u.RoomUsers)
               .ThenInclude(ru => ru.Room).ThenInclude(r => r.Subject)
                .Where(u => u.UserRoles.Any(ur => ur.RoleId != (int)RoleEnum.Student) && u.Status == (int)UserStatus.Active)
                .AsQueryable();
            if (!string.IsNullOrWhiteSpace(search.TextSearch))
            {
                var text = search.TextSearch.ToLower();
                query = query.Where(u => u.Email.ToLower().Contains(text)
                    || u.FullName.ToLower().Contains(text)
                    || u.UserCode.ToLower().Contains(text)
                    || u.RoomUsers.Any(ru => ru.Room.Subject.SubjectName.ToLower().Contains(text)));
            }

            var totalCount = await query.CountAsync();
            var totalPage = (int)Math.Ceiling(totalCount / (double)search.PageSize);

            var users = await query
                .OrderBy(u => u.CreateAt)
                .Skip((search.CurrentPage - 1) * search.PageSize)
                .Take(search.PageSize)
                .Select(u => new
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    UserCode = u.UserCode,
                    Email = u.Email,
                    SubjectNames = u.RoomUsers
                        .Where(ru => ru.RoleId == (int)RoleEnum.Lecture && ru.Room != null && ru.Room.Subject != null)
                        .Select(ru => ru.Room.Subject.SubjectName)
                        .Distinct()
                        .ToList()
                }).ToListAsync();
            if (!users.Any()) return ("No users found.", null);

            return ("", new SearchResult
            {
                Result = users,
                TotalPage = totalPage,
                PageSize = search.PageSize,
                CurrentPage = search.CurrentPage,
                Total = totalCount
            });
        }

        public async Task<(string, SearchResult?)> GetExams(SearchRequestVM search, string usertoken)
        {
            var now = DateTime.UtcNow;
            var isAdmin = await _context.Users.Include(u => u.UserRoles).AsNoTracking()
                .AnyAsync(u => u.UserId == usertoken && u.UserRoles.Any(r => r.RoleId != (int)RoleEnum.Student));

            var query = _context.Exams.Include(e => e.Room!.Subject).Include(e => e.Room!.Class)
                .Where(e => e.Status == (int)ExamStatus.Published
                 && (e.StartTime > now || e.StartTime <= now && e.EndTime >= now))
                .AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search.TextSearch))
            {
                var text = search.TextSearch.ToLower();
                query = query.Where(e => e.Title.ToLower().Contains(text)
                    || e.Room.Subject.SubjectName.ToLower().Contains(text)
                    || e.Room.Class.ClassCode.ToLower().Contains(text));
            }
            var totalCount = await query.CountAsync();
            var totalPage = (int)Math.Ceiling(totalCount / (double)search.PageSize);

            var examIdsWithSupervisors = await _context.ExamSupervisors.Where(es => es.SupervisorId != null)
                .Select(es => es.ExamId).Distinct().AsNoTracking().ToListAsync();

            var exams = await query
                .OrderBy(e => e.CreatedAt)
                .Skip((search.CurrentPage - 1) * search.PageSize)
                .Take(search.PageSize)
                .Select(e => new
                {
                    ExamId = e.ExamId,
                    Title = e.Title,
                    SubjectName = e.Room.Subject.SubjectName,
                    ClassCode = e.Room.Class.ClassCode,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    e.CreatedAt,
                    e.Status,
                    e.LiveStatus,
                    StudentCount = e.Room.RoomUsers.Count(ru => ru.Status == (int)ActiveStatus.Active),
                    HasSupervisor = examIdsWithSupervisors.Contains(e.ExamId)
                }).ToListAsync();
            if (!exams.Any()) return ("No exams found.", null);

            return ("", new SearchResult
            {
                Result = exams,
                TotalPage = totalPage,
                PageSize = search.PageSize,
                CurrentPage = search.CurrentPage,
                Total = totalCount
            });
        }
        public async Task<(string, object?)> AssignSupervisor(EditExamSupervisorVM input, string usertoken)
        {
            using var trans = await _context.Database.BeginTransactionAsync();
            try
            {
                var currentSupervisors = await _context.ExamSupervisors.Include(x => x.Exam).Where(x => x.ExamId == input.ExamId).ToListAsync();
                if (currentSupervisors.IsNullOrEmpty()) return ("No ExamSupervisor found for this exam.", null);

                var users = await _context.Users.Include(x => x.UserRoles).Where(x => input.SupervisorId.Contains(x.UserId)).ToListAsync();
                if (!users.Any()) return ("No users found with the provided SupervisorId.", null);

                var invalidUsers = users.Where(u => u.UserRoles.All(ur => ur.RoleId == (int)RoleEnum.Student)).ToList();
                if (invalidUsers.Any())
                    return ($"Users {string.Join(", ", invalidUsers.Select(u => u.Email))} have Student role.", null);

                var currentUserIds = currentSupervisors.Select(x => x.SupervisorId).ToList();
                var toAddUserIds = input.SupervisorId.Where(id => !currentUserIds.Contains(id)).ToList();
                var toAdd = toAddUserIds.Select(userId => new ExamSupervisor
                {
                    ExamSupervisorId = Guid.NewGuid().ToString(),
                    ExamId = input.ExamId,
                    SupervisorId = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Note = input.Note,
                    CreatedBy = currentSupervisors.First().CreatedBy,
                }).ToList();
                if (toAdd.IsNullOrEmpty()) return ("No new supervisor to add", null);

                await _context.ExamSupervisors.AddRangeAsync(toAdd);
                await _context.SaveChangesAsync();

                var msg = await _log.WriteActivity(new AddUserLogVM
                {
                    ActionType = "Update",
                    Description = $"New supervisors has been added to exam",
                    ObjectId = input.ExamId,
                    Metadata = toAdd.Select(x => x.ExamSupervisorId).ToList().ToString(),
                    UserId = usertoken,
                    Status = (int)LogStatus.Success
                });
                if (msg.Length > 0) return (msg, null);

                await trans.CommitAsync();
                return ("", input.SupervisorId.ToList());
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                return ($"Error updating supervisor list: {ex.Message}", null);
            }
        }
        public async Task<(string, object?)> Remove(EditExamSupervisorVM edit, string usertoken)
        {
            using var trans = await _context.Database.BeginTransactionAsync();
            try
            {
                var examExists = await _context.Exams.AsNoTracking().AnyAsync(x => x.ExamId == edit.ExamId);
                if (!examExists) return ($"Exam with ID: {edit.ExamId} does not exist.", null);

                var isAdmin = await _context.Users.Include(u => u.UserRoles).AsNoTracking()
                    .AnyAsync(u => u.UserId == usertoken && u.UserRoles.Any(r => r.RoleId == (int)RoleEnum.Admin));
                var supervisorsQuery = _context.ExamSupervisors.Include(x => x.User).Where(x => x.ExamId == edit.ExamId);
                if (!isAdmin) supervisorsQuery = supervisorsQuery.Where(x => x.CreatedBy == usertoken);

                var supervisors = await supervisorsQuery.ToListAsync();
                if (!supervisors.Any()) return ("No supervisors found to delete.", null);

                var existingIds = supervisors.Select(x => x.SupervisorId).ToList();
                var idsToDelete = edit.SupervisorId.Intersect(existingIds).ToList();
                var idsNotFound = edit.SupervisorId.Except(existingIds).ToList();

                if (!idsToDelete.Any()) return ("No matching supervisor id(s) found to delete.", idsNotFound);

                var toRemove = supervisors.Where(x => idsToDelete.Contains(x.SupervisorId)).ToList();

                _context.ExamSupervisors.RemoveRange(toRemove);
                await _context.SaveChangesAsync();

                var msg = await _log.WriteActivity(new AddUserLogVM
                {
                    ActionType = "Remove",
                    Description = "Supervisors have been removed from exam.",
                    ObjectId = edit.ExamId,
                    Metadata = string.Join(", ", toRemove.Select(x => $"{x.SupervisorId} - {x.User?.FullName}")),
                    UserId = usertoken,
                    Status = (int)LogStatus.Success
                });
                if (msg.Length > 0)
                {
                    await trans.RollbackAsync();
                    return (msg, null);
                }
                await trans.CommitAsync();
                return ("", idsNotFound);
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                return ($"Error deleting supervisors: {ex.Message}", null);
            }
        }

    }
}

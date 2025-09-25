using API.Commons;
using API.Helper;
using API.Models;
using API.Services.Interfaces;
using API.ViewModels;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class SubjectService : ISubjectService
    {
        private readonly Sep490Context _context;
        private readonly IMapper _mapper;
        private readonly ILog _log;

        public SubjectService(Sep490Context context, IMapper mapper, ILog log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _context = context;
            _mapper = mapper;
        }
        public async Task<(string, SearchResult?)> GetAllSubjects(SearchSubjectVM search)
        {
            var query = _context.Subjects.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search.TextSearch))
                query = query.Where(s => s.SubjectName.Contains(search.TextSearch) || s.SubjectCode.Contains(search.TextSearch));

            if (search.Status.HasValue)
                query = query.Where(s => s.Status == search.Status.Value);

            var totalCount = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalCount / search.PageSize);

            if (totalCount == 0) return ("No subjects found.", null);

            var subjects = await query
                .OrderByDescending(s => s.CreatedDate)
                .Skip((search.CurrentPage - 1) * search.PageSize)
                .Take(search.PageSize)
                .ToListAsync();

            return ("", new SearchResult
            {
                Result = _mapper.Map<List<SubjectVM>>(subjects),
                TotalPage = totalPages,
                CurrentPage = search.CurrentPage,
                PageSize = search.PageSize,
                Total = totalCount
            });
        }

        public async Task<(string, SubjectVM?)> GetSubjectById(string subjectId)
        {
            if (string.IsNullOrEmpty(subjectId))
                return ("Subject ID cannot be null or empty.", null);

            var subject = await _context.Subjects.FindAsync(subjectId);
            if (subject == null) return ("Subject not found.", null);

            var subjectVM = _mapper.Map<SubjectVM>(subject);
            return ("", subjectVM);
        }

        public async Task<string> ChangeActivateSubject(string subjectId, string usertoken)
        {
            if (string.IsNullOrEmpty(subjectId))
                return "Subject ID cannot be null or empty.";

            var subject = await _context.Subjects.FindAsync(subjectId);
            if (subject == null) return "Subject not found.";

            subject.Status = !subject.Status;
            _context.Subjects.Update(subject);
            await _context.SaveChangesAsync();

            var msg = await _log.WriteActivity(new AddUserLogVM
            {
                ActionType = subject.Status ? "Activate" : "Deactivate",
                ObjectId = subject.SubjectId,
                Status = (int)LogStatus.Success,
                Description = $"{subject.SubjectName} has been {(subject.Status ? "activated" : "deactivated")}.",
                UserId = usertoken,
                Metadata = "SubjectName: " + subject.SubjectName + "SubjectCode: " + subject.SubjectCode,
            });
            if (msg.Length > 0) return msg;
            return "";
        }

        public async Task<string> CreateUpdateSubject(CreateUpdateSubjectVM subject, string usertoken)
        {
            if (subject == null)
                return "Subject cannot be null.";

            if (string.IsNullOrEmpty(subject.SubjectId))
            {
                var existingSubject = await _context.Subjects.AnyAsync(s => s.SubjectCode == subject.SubjectCode);
                if (existingSubject) return "This SubjectCode is already in use. Please enter a different one.";

                var existName = await _context.Subjects.AnyAsync(s => s.SubjectName == subject.SubjectName);
                if (existName) return "This SubjectName is already in use. Please enter a different one.";

                subject.SubjectId = Guid.NewGuid().ToString();
                var newSubject = new Subject
                {
                    SubjectId = subject.SubjectId,
                    SubjectName = subject.SubjectName,
                    SubjectDescription = subject.SubjectDescription,
                    SubjectContent = subject.SubjectContent,
                    SubjectCode = subject.SubjectCode,
                    Status = subject.Status,
                    Credits = subject.Credits,
                    CreatedDate = DateTime.UtcNow,
                };
                await _context.Subjects.AddAsync(newSubject);
            }
            else
            {
                var existingSubject = await _context.Subjects.FindAsync(subject.SubjectId);
                if (existingSubject == null) return "Subject not found!";

                existingSubject.SubjectName = subject.SubjectName;
                existingSubject.SubjectDescription = subject.SubjectDescription;
                existingSubject.SubjectCode = subject.SubjectCode;
                existingSubject.SubjectContent = subject.SubjectContent;
                existingSubject.Status = subject.Status;
                existingSubject.Credits = subject.Credits;
                existingSubject.UpdatedDate = DateTime.UtcNow;

                _context.Subjects.Update(existingSubject);
            }
            await _context.SaveChangesAsync();
            
            var msg = await _log.WriteActivity(new AddUserLogVM
            {
                ActionType = string.IsNullOrEmpty(subject.SubjectId) ? "Create" : "Update",
                ObjectId = subject.SubjectId,
                Status = (int)LogStatus.Success,
                Description = $"{subject.SubjectCode} has been {(string.IsNullOrEmpty(subject.SubjectId) ? "created" : "updated")}.",
                UserId = usertoken,
                Metadata = "SubjectName: " + subject.SubjectName + "SubjectCode: " + subject.SubjectCode,
            });
            if (msg.Length > 0) return msg;
            return "";
        }
    }
}

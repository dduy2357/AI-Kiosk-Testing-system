using API.Commons;
using API.Helper;
using API.Models;
using API.Services.Interfaces;
using API.ViewModels;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class ClassesService : IClassesService
    {
        private readonly Sep490Context _context;
        private readonly IMapper _mapper;
        private readonly ILog _logger;
        public ClassesService(Sep490Context context, IMapper mapper, ILog logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<(string, SearchResult?)> GetAllClasses(SearchClassVM search)
        {
            var query = _context.Classes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search.TextSearch))
                query = query.Where(c => c.ClassCode.Contains(search.TextSearch));

            if (search.IsActive.HasValue)
                query = query.Where(c => c.IsActive == search.IsActive.Value);

            if (search.FromDate.HasValue)
                query = query.Where(c => c.StartDate >= search.FromDate.Value);

            if (search.ToDate.HasValue)
                query = query.Where(c => c.StartDate <= search.ToDate.Value);

            query = query.OrderByDescending(c => c.CreatedAt);

            var classes = await query.Skip((search.CurrentPage - 1) * search.PageSize).Take(search.PageSize).ToListAsync();
            if (classes == null || classes.Count == 0) return ("No classes found.", null);

            var classVMs = _mapper.Map<List<ClassVM>>(classes);
            return ("", new SearchResult
            {
                CurrentPage = search.CurrentPage,
                PageSize = search.PageSize,
                TotalPage = (int)Math.Ceiling((double)await query.CountAsync() / search.PageSize),
                Result = classVMs,
                Total = await query.CountAsync()
            });
        }

        public async Task<(string, ClassVM?)> GetClassById(string classId)
        {
            if (string.IsNullOrEmpty(classId))
                return ("Class ID cannot be null or empty.", null);

            var classEntity = await _context.Classes.Include(c => c.Rooms).FirstOrDefaultAsync(c => c.ClassId == classId);
            if (classEntity == null) return ("Class not found.", null);

            var mapper = _mapper.Map<ClassVM>(classEntity);

            return ("", mapper);
        }

        public async Task<string> DoDeactivateClass(string classId, string usertoken)
        {
            if (string.IsNullOrEmpty(classId)) return "Class ID cannot be null or empty.";

            var classEntity = await _context.Classes.FirstOrDefaultAsync(c => c.ClassId == classId);
            if (classEntity == null) return "Class not found.";

            classEntity.IsActive = !classEntity.IsActive;
            _context.Classes.Update(classEntity);
            await _context.SaveChangesAsync();

            var msg = await _logger.WriteActivity(new AddUserLogVM
            {
                ActionType = "Deactivate",
                Description = "A class has been deactivate.",
                ObjectId = classId,
                UserId = usertoken,
                Metadata = classEntity.ClassCode,
                Status = (int)LogStatus.Success,    
            });
            if (msg.Length > 0) return msg;
            return "";
        }

        public async Task<string> DoCreateUpdateClass(CreateUpdateClassVM input, string usertoken)
        {
            if (input == null) return "Data input cannot be null.";

            if (input.ClassId.IsEmpty())
            {
                input.ClassId = Guid.NewGuid().ToString();

                var existingClass = await _context.Classes.AnyAsync(c => c.ClassCode == input.ClassCode);
                if (existingClass) return "This ClassCode is already in use. Please enter a different one.";

                var newClass = new Class
                {
                    ClassCode = input.ClassCode,
                    Description = input.Description,
                    CreatedBy = usertoken,
                    IsActive = input.IsActive,
                    StartDate = input.StartDate,
                    EndDate = input.EndDate,
                    CreatedAt = DateTime.UtcNow,
                    ClassId = input.ClassId,
                };
                await _context.Classes.AddAsync(newClass);
            }
            else
            {
                var existingClass = await _context.Classes.FindAsync(input.ClassId);
                if (existingClass == null) return "Class not found.";

                existingClass.ClassCode = input.ClassCode;
                existingClass.Description = input.Description;
                existingClass.CreatedBy = usertoken;
                existingClass.IsActive = input.IsActive;
                existingClass.StartDate = input.StartDate;
                existingClass.EndDate = input.EndDate;
                existingClass.UpdatedAt = DateTime.UtcNow;

                _context.Classes.Update(existingClass);
            }
            await _context.SaveChangesAsync();
            var msg = await _logger.WriteActivity(new AddUserLogVM
            {
                ActionType = (input.ClassId.IsEmpty() ? "created" : "updated."),
                Description = "A class has been " + (input.ClassId.IsEmpty() ? "created" : "updated."),
                Metadata = input.ClassCode,
                ObjectId = input.ClassId,
                Status = (int)LogStatus.Success,
                UserId = usertoken,
            });
            if (msg.Length > 0) return msg;
            return "";
        }
    }
}

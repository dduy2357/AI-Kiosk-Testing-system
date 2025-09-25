using API.Models;
using API.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace API.Repository
{
    public class SubjectRepository : ISubjectRepository
    {
        private readonly Sep490Context _context;
        public SubjectRepository(Sep490Context context)
        {
            _context = context;
        }
        //Validate subject with AnyAsync linq
        public async Task<bool> GetSubjectByIdAsync(string subjectId)
        {
            return await _context.Subjects.AnyAsync(s => s.SubjectId.ToLower() == subjectId.ToLower());
        }
    }
}

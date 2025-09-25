using API.Models;
using API.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace API.Repository
{
    public class FaceCaptureRepository : IFaceCaptureRepository
    {
        private readonly DbContext _context;

        public FaceCaptureRepository(DbContext context)
        {
            _context = context;
        }

        public async Task<List<FaceCapture>> GetByStudentExamId(string studentExamId)
        {
            return await _context.Set<FaceCapture>()
                .Where(fc => fc.StudentExamId == studentExamId)
                .ToListAsync();
        }

        public void DeleteRange(List<FaceCapture> faceCaptures)
        {
            _context.Set<FaceCapture>().RemoveRange(faceCaptures);
        }
    }
}

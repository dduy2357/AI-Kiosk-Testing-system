using API.Helper;
using API.Models;
using API.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace API.Repository
{
    public class StudentExamRepository : IStudentExamRepository
    {
        private readonly Sep490Context _context;

        public StudentExamRepository(Sep490Context context)
        {
            _context = context;
        }

        public async Task Add(StudentExam studentExam)
        {
            await _context.StudentExams.AddAsync(studentExam);
        }

        public void Update(StudentExam studentExam)
        {
            _context.StudentExams.Update(studentExam);
        }

        public void Remove(StudentExam studentExam)
        {
            _context.StudentExams.Remove(studentExam);
        }
        public void Detach(StudentExam studentExam)
        {
            _context.Entry(studentExam).State = EntityState.Detached;
        }
        public async Task<List<string>> GetSubmittedExamIds(string studentId, DateTime from, DateTime to)
        {
            return await _context.StudentExams.AsNoTracking()
                .Where(se => se.StudentId == studentId &&
                             se.Status == (int)StudentExamStatus.Submitted &&
                             se.SubmitTime >= from && se.SubmitTime <= to)
                .Select(se => se.ExamId)
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<StudentExam>> GetByExamIds(string studentId, List<string> examIds)
        {
            return await _context.StudentExams.AsNoTracking()
                .Where(se => examIds.Contains(se.ExamId) && se.StudentId == studentId)
                .ToListAsync();
        }

        public async Task<StudentExam?> GetByExamAndStudent(string examId, string studentId)
        {
            return await _context.StudentExams.FirstOrDefaultAsync(se => se.ExamId == examId && se.StudentId == studentId);
        }

        public async Task<StudentExam?> GetByIdWithExam(string studentExamId, string examId, string studentId, bool asNoTracking = false)
        {
            var query = _context.StudentExams.Include(se => se.Exam)
                .Where(se => se.StudentExamId == studentExamId && se.ExamId == examId && se.StudentId == studentId);

            if (asNoTracking) query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync();
        }
  
        public async Task<StudentExam?> GetByStudentExamId(string studentExamId, string examId, string studentId)
        {
            return await _context.StudentExams.Include(x => x.Exam).FirstOrDefaultAsync(se => se.StudentExamId == studentExamId
                             && se.StudentId == studentId && se.ExamId == examId);
        }

        public async Task<StudentExam?> GetExamInProgress(string examId, string usertoken)
        {
            return await _context.StudentExams.Include(x => x.Exam).AsNoTracking()
                .Where(se => se.ExamId == examId && se.StudentId == usertoken && se.Status == (int)StudentExamStatus.InProgress)
                .FirstOrDefaultAsync();
        }
        public async Task<StudentExam?> GetStudentExamWithExamUser(string studentExamId, StudentExamStatus? status = null, bool asNoTracking = false)
        {
            var query = _context.StudentExams.Include(se => se.Exam).Include(x => x.User)
                    .Where(se => se.StudentExamId == studentExamId && (status == null || se.Status == (int)status));

            if (asNoTracking) query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync();
        }
    }
}

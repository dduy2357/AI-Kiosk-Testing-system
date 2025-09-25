using API.Commons;
using API.Helper;
using API.Models;

namespace API.Builders
{
    public class StudentExamBuilder
    {
        private readonly StudentExam _studentExam = new();

        public StudentExamBuilder WithExam(Exam exam)
        {
            _studentExam.ExamId = exam.ExamId;
            _studentExam.SubmitTime = DateTime.UtcNow.AddMinutes(exam.Duration);
            return this;
        }

        public StudentExamBuilder WithStudent(string studentId)
        {
            _studentExam.StudentId = studentId;
            _studentExam.StudentExamId = Guid.NewGuid().ToString();
            return this;
        }

        public StudentExamBuilder WithHttpContext(HttpContext context)
        {
            _studentExam.IpAddress = Utils.GetClientIpAddress(context);
            _studentExam.BrowserInfo = Utils.GetClientBrowser(context);
            return this;
        }

        public StudentExamBuilder WithScore(decimal score)
        {
            _studentExam.Score = score;
            return this;
        }

        public StudentExamBuilder AsNew()
        {
            var now = DateTime.UtcNow;
            _studentExam.CreatedAt = now;
            _studentExam.StartTime = now;
            _studentExam.Status = (int)StudentExamStatus.InProgress;
            return this;
        }

        public StudentExamBuilder WithStatus(int status)
        {
            _studentExam.Status = status;
            return this;
        }

        public StudentExamBuilder WithTotalQuestions(int totalQuestions)
        {
            _studentExam.TotalQuestions = totalQuestions;
            return this;
        }

        public StudentExamBuilder AsResume(StudentExam existing)
        {
            _studentExam.StudentExamId = existing.StudentExamId;
            _studentExam.ExamId = existing.ExamId;
            _studentExam.StudentId = existing.StudentId;
            _studentExam.StartTime = existing.StartTime;
            _studentExam.SubmitTime = existing.SubmitTime;
            _studentExam.CreatedAt = existing.CreatedAt;    
            _studentExam.Status = (int)StudentExamStatus.InProgress;
            _studentExam.UpdatedAt = DateTime.UtcNow;
            return this;
        }

        public StudentExam Build() => _studentExam;
    }

}

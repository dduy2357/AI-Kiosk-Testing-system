using API.Models;

namespace API.Builders
{
    public class ExamSupervisorBuilder
    {
        private readonly ExamSupervisor _examSupervisor;

        public ExamSupervisorBuilder()
        {
            _examSupervisor = new ExamSupervisor
            {
                ExamSupervisorId = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow
            };
        }

        public ExamSupervisorBuilder SetExamId(string examId)
        {
            _examSupervisor.ExamId = examId;
            return this;
        }

        public ExamSupervisorBuilder SetSupervisorId(string? supervisorId)
        {
            _examSupervisor.SupervisorId = supervisorId;
            return this;
        }

        public ExamSupervisorBuilder SetCreatedBy(string userId)
        {
            _examSupervisor.CreatedBy = userId;
            return this;
        }

        public ExamSupervisor Build() => _examSupervisor;
    }

}

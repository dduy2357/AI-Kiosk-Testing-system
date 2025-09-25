using API.Repository.Interface;
using API.Services.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace API.Repository
{
    public interface IUnitOfWork
    {
        IExamRepository Exams { get; }
        IRoomRepository Rooms { get; }
        IQuestionBankRepository QuestionBanks { get; }
        IExamQuestionRepository ExamQuestions { get; }
        IExamSupervisorRepository ExamSupervisors { get; }
        IExamOtpRepository ExamOTPs { get; }
        IStudentExamRepository StudentExams { get; }
        IStudentAnswerRepository StudentAnswers { get; }
        IFaceCaptureRepository FaceCaptures { get; }
        IUserRepository Users { get; }
        ISubjectRepository Subjects { get; }
        IQuestionRepository Questions { get; }

        Task<int> SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}

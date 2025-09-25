using API.Models;
using API.Repository.Interface;
using Microsoft.EntityFrameworkCore.Storage;

namespace API.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly Sep490Context _context;

        private IExamRepository _examRepository = null!;
        private IRoomRepository _roomRepository = null!;
        private IQuestionBankRepository _questionBankRepository = null!;
        private IExamQuestionRepository _examQuestionRepository = null!;
        private IExamSupervisorRepository _examSupervisorRepository = null!;
        private IExamOtpRepository _examOtpRepository = null!;
        private IStudentExamRepository _studentExamRepository = null!;
        private IStudentAnswerRepository _studentAnswerRepository = null!;
        private IFaceCaptureRepository _faceCaptureRepository = null!;
        private IUserRepository _userRepository = null!;
        private ISubjectRepository subjectRepository = null!;
        private IQuestionRepository _questionRepository = null!;

        public UnitOfWork(Sep490Context context) => _context = context;
        public IExamRepository Exams => _examRepository ??= new ExamRepository(_context);
        public IRoomRepository Rooms => _roomRepository ??= new RoomRepository(_context);
        public IQuestionBankRepository QuestionBanks => _questionBankRepository ??= new QuestionBankRepository(_context);
        public IExamQuestionRepository ExamQuestions => _examQuestionRepository ??= new ExamQuestionRepository(_context);
        public IExamSupervisorRepository ExamSupervisors => _examSupervisorRepository ??= new ExamSupervisorRepository(_context);
        public IExamOtpRepository ExamOTPs => _examOtpRepository ??= new ExamOtpRepository(_context);
        public IStudentExamRepository StudentExams => _studentExamRepository ??= new StudentExamRepository(_context);
        public IStudentAnswerRepository StudentAnswers => _studentAnswerRepository ??= new StudentAnswerRepository(_context);
        public IFaceCaptureRepository FaceCaptures => _faceCaptureRepository ??= new FaceCaptureRepository(_context);
        public IUserRepository Users => _userRepository ??= new UserRepository(_context);
        public ISubjectRepository Subjects => subjectRepository ??= new SubjectRepository(_context);
        public IQuestionRepository Questions => _questionRepository ??= new QuestionRepository(_context);
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }
        public async Task CommitTransactionAsync()
        {
            await _context.Database.CommitTransactionAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            await _context.Database.RollbackTransactionAsync();
        }
    }
}
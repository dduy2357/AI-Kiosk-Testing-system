using Amazon.S3;
using API.Cached;
using API.Commons;
using API.Factory;
using API.Models;
using API.Observers;
using API.Observers.Interface;
using API.Repository;
using API.Repository.Interface;
using API.Services;
using API.Services.Interfaces;
using API.Strategy;
using API.Subjects;
using API.Tasks;
using API.Validators;
using API.Validators.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Configurations
{
    public static class ServicesConfiguration
    {
        public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register the DbContext with connection string from the configuration
            services.AddDbContext<Sep490Context>(options =>
                options.UseSqlServer(configuration.GetConnectionString("MyDB")));

            //services.AddSingleton<JwtAuthentication>(); 
            services.AddSingleton<IAmazonS3>(sp => new AmazonS3Client(
                  ConfigManager.gI().AWSAccessKey,
                  ConfigManager.gI().AWSSecretKey,
                  Amazon.RegionEndpoint.GetBySystemName(ConfigManager.gI().AWSRegion)
            ));
            services.AddScoped<IDataCached, DataCached>();
            services.AddScoped<ILog, Log>();
            services.AddScoped<IAuthenticateService, AuthenticateService>();
            services.AddScoped<IAuthorizeService, AuthorizeService>();
            services.AddScoped<IAmazonS3Service, AmazonS3Service>();
            services.AddScoped<ICacheProvider, MemoryCacheProvider>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IClassesService, ClassesService>();
            services.AddScoped<ISubjectService, SubjectService>();
            services.AddScoped<IRoomService, RoomService>();
            services.AddScoped<IRoomUserService, RoomUserService>();
            services.AddScoped<IDisabledKeyService, DisabledKeyService>();
            services.AddScoped<IProhibitedAppService, ProhibitedAppService>();
            services.AddScoped<ICampusService, CampusService>();
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<IPositionService, PositionService>();
            services.AddScoped<IMajorService, MajorService>();
            services.AddScoped<ISpecializationService, SpecializationService>();
            services.AddScoped<IQuestionBankService, QuestionBankService>();
            services.AddScoped<IQuestionService, QuestionService>();
            services.AddScoped<IConfigUrlService, ConfigUrlService>();
            services.AddScoped<IConfigDesktopService, ConfigDesktopService>();
            services.AddScoped<IExamService, ExamService>();
            services.AddScoped<IStudentExamService, StudentExamService>();
            services.AddScoped<IMonitoringService, MonitoringService>();
            services.AddScoped<IExamSupervisorService, ExamSupervisorService>();
            services.AddScoped<IFaceCaptureService, FaceCaptureService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IStudentViolationService, StudentViolationService>();
            services.AddHostedService<AutoSubmitExam>();
            services.AddScoped<IFeedbackService, FeedbackService>();
            services.AddScoped<IAddExamValidator, AddExamValidator>();


            services.AddScoped<IExamRepository, ExamRepository>();
            services.AddScoped<IRoomRepository, RoomRepository>();
            services.AddScoped<IQuestionBankRepository, QuestionBankRepository>();
            services.AddScoped<IExamQuestionRepository, ExamQuestionRepository>();
            services.AddScoped<IExamSupervisorRepository, ExamSupervisorRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ISubjectRepository, SubjectRepository>();
            services.AddScoped<IQuestionRepository, QuestionRepository>();

            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IExamObserver, ExamLogObserver>();
            services.AddScoped<IExamSubject>(sp =>
            {
                var subject = new ExamSubject();
                var observer = sp.GetRequiredService<IExamObserver>();
                subject.Attach(observer);
                return subject;
            });

            services.AddScoped<IMonitoringObserver, LoggerObserver>();
            services.AddScoped<IMonitoringObserver, TimerDisplayObserver>();
            services.AddScoped<IMonitoringSubject>(sp =>
            {
                var subject = new MonitoringSubject();
                var observers = sp.GetServices<IMonitoringObserver>();
                foreach (var observer in observers)
                {
                    subject.Attach(observer);
                }
                return subject;
            });

            services.AddScoped<EssayScoringStrategy>();
            services.AddScoped<MultichoiceScoringStrategy>(); 
            services.AddScoped<IScoringStrategyFactory, ScoringStrategyFactory>();
        }
    }
}
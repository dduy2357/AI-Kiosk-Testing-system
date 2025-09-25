using DocumentFormat.OpenXml;
using Microsoft.EntityFrameworkCore;

namespace API.Models
{
    public class Sep490Context : DbContext
    {
        public Sep490Context() { }

        public Sep490Context(DbContextOptions<Sep490Context> options) : base(options) { }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            try
            {
                return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                throw new Exception(ex.Message + ex.InnerException);
            }
            catch (Exception ex)
            {
                throw new Exception("A system error occurred." + ex.Message + ex.InnerException);
            }
        }
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return SaveChangesAsync(true, cancellationToken);
        }

        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Permission> Permissions { get; set; }
        public virtual DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<PermissionCategory> PermissionCategories { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<User> Users { get; set; }
        public virtual DbSet<Class> Classes { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<RoomUser> RoomUsers { get; set; }
        public DbSet<DisabledKey> DisabledKeys { get; set; }
        public DbSet<ProhibitedApp> ProhibitedApps { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<Major> Majors { get; set; }
        public DbSet<Specialization> Specializations { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Campus> Campuses { get; set; }
        public DbSet<QuestionBank> QuestionBanks { get; set; }
        public DbSet<QuestionShare> QuestionShares { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<ExamQuestion> ExamQuestions { get; set; }
        public DbSet<StudentAnswer> StudentAnswers { get; set; }
        public DbSet<StudentExam> StudentExams { get; set; }
        public DbSet<StudentViolation> StudentViolations { get; set; }
        public DbSet<ExamSupervisor> ExamSupervisors { get; set; }
        public DbSet<ExamOtp> ExamOtps { get; set; }
        //public DbSet<ExamResult> ExamResults { get; set; }
        public DbSet<UserLog> UserLogs { get; set; }
        public DbSet<ExamLog> ExamLogs { get; set; }
        public DbSet<FaceCapture> FaceCaptures { get; set; }
        public DbSet<ConfigUrl> ConfigUrls { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Ignore<MarkupCompatibilityAttributes>(); //

            // Configure composite keys
            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            // Configure composite unique indexes
            modelBuilder.Entity<RoomUser>()
                .HasIndex(e => new { e.RoomId, e.UserId })
                .IsUnique();

            // Configure cascade delete behaviors
            modelBuilder.Entity<RoomUser>()
                .HasOne(e => e.Room)
                .WithMany(e => e.RoomUsers)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RoomUser>()
                .HasOne(e => e.User)
                .WithMany(e => e.RoomUsers)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Permission>()
                .HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Position>()
                .HasOne(p => p.Department)
                .WithMany(d => d.Positions)
                .HasForeignKey(p => p.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade); // Restrict delete parent and child| Restrict block delete parent if child exist 

            modelBuilder.Entity<StudentExam>()
                .HasOne(se => se.User)
                .WithMany()
                .HasForeignKey(se => se.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudentExam>()
                .HasOne(se => se.Exam)
                .WithMany()
                .HasForeignKey(se => se.ExamId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ExamLog>()
                .HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict); // hoặc .NoAction

            modelBuilder.Entity<Question>()
                .HasOne(q => q.Subject)
                .WithMany(s => s.Questions)
                .HasForeignKey(q => q.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Question>()
                .HasOne(q => q.CreatedByUser)
                .WithMany()
                .HasForeignKey(q => q.CreateUser)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Question>()
                .HasOne(q => q.UpdatedByUser)
                .WithMany()
                .HasForeignKey(q => q.UpdateUser)
                .OnDelete(DeleteBehavior.Restrict); // hoặc .NoAction

            modelBuilder.Entity<Question>()
                .HasOne(q => q.QuestionBank)
                .WithMany(qb => qb.Questions)
                .HasForeignKey(q => q.QuestionBankId)
                .OnDelete(DeleteBehavior.Restrict);

            //modelBuilder.Entity<ExamQuestion>()
            //  .HasOne(eq => eq.Exam)
            //  .WithMany()
            //  .HasForeignKey(eq => eq.ExamId)
            //  .OnDelete(DeleteBehavior.Restrict);

            //modelBuilder.Entity<ExamQuestion>()
            //    .HasOne(eq => eq.Question)
            //    .WithMany()
            //    .HasForeignKey(eq => eq.QuestionId)
            //    .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ExamOtp>()
                .HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ExamOtp>()
                .HasOne(e => e.Exam)
                .WithMany()
                .HasForeignKey(e => e.ExamId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ExamSupervisor>(entity =>
            {
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.SupervisorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CreatedUser)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Exam)
                    .WithMany(e => e.ExamSupervisors)
                    .HasForeignKey(e => e.ExamId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<QuestionShare>()
              .HasOne(qs => qs.SharedWithUser)
              .WithMany()
              .HasForeignKey(qs => qs.SharedWithUserId)
              .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<QuestionShare>()
                .HasOne(qs => qs.QuestionBank)
                .WithMany()
                .HasForeignKey(qs => qs.QuestionBankId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.SendToId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.CreatedUser)
                .WithMany()
                .HasForeignKey(n => n.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
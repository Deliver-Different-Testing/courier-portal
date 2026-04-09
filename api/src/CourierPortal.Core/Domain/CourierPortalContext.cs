using Microsoft.EntityFrameworkCore;
using CourierPortal.Core.Domain.Entities;

namespace CourierPortal.Core.Domain;

public partial class CourierPortalContext : DbContext
{
    public CourierPortalContext(DbContextOptions<CourierPortalContext> options) : base(options) { }

    // Applicant flow entities
    public DbSet<DocumentType> DocumentTypes => Set<DocumentType>();
    public DbSet<PortalStep> PortalSteps => Set<PortalStep>();
    public DbSet<ApplicantStepData> ApplicantStepData => Set<ApplicantStepData>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<TenantSettings> TenantSettings => Set<TenantSettings>();

    // Quiz entities  
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();
    public DbSet<QuizOption> QuizOptions => Set<QuizOption>();
    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
    public DbSet<QuizAttemptAnswer> QuizAttemptAnswers => Set<QuizAttemptAnswer>();

    // Recruitment stages
    public DbSet<RecruitmentStage> RecruitmentStages => Set<RecruitmentStage>();
    public DbSet<RecruitmentNote> RecruitmentNotes => Set<RecruitmentNote>();
    public DbSet<JobPosting> JobPostings => Set<JobPosting>();
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}

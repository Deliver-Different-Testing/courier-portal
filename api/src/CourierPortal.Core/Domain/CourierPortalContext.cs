using CourierPortal.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Core.Domain;

public partial class CourierPortalContext : DbContext
{
    public CourierPortalContext(DbContextOptions<CourierPortalContext> options) : base(options) { }

    // Compliance
    public DbSet<ComplianceProfile> ComplianceProfiles => Set<ComplianceProfile>();
    public DbSet<ComplianceProfileRequirement> ComplianceProfileRequirements => Set<ComplianceProfileRequirement>();
    public DbSet<DocumentType> DocumentTypes => Set<DocumentType>();

    // Quiz
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();
    public DbSet<QuizOption> QuizOptions => Set<QuizOption>();
    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
    public DbSet<QuizAttemptAnswer> QuizAttemptAnswers => Set<QuizAttemptAnswer>();

    // Recruitment
    public DbSet<JobPosting> JobPostings => Set<JobPosting>();
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();

    // Registration
    public DbSet<RegistrationField> RegistrationFields => Set<RegistrationField>();
    public DbSet<RegistrationFieldOption> RegistrationFieldOptions => Set<RegistrationFieldOption>();

    // Driver Approval
    public DbSet<DriverApproval> DriverApprovals => Set<DriverApproval>();

    // Messenger
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ConversationMessage> ConversationMessages => Set<ConversationMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ComplianceProfile
        modelBuilder.Entity<ComplianceProfile>(e =>
        {
            e.ToTable("ComplianceProfiles");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Description).HasMaxLength(1000);
            e.Property(x => x.CourierType).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<ComplianceProfileRequirement>(e =>
        {
            e.ToTable("ComplianceProfileRequirements");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Profile).WithMany(p => p.Requirements).HasForeignKey(x => x.ProfileId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.DocumentType).WithMany(d => d.ComplianceRequirements).HasForeignKey(x => x.DocumentTypeId).OnDelete(DeleteBehavior.Restrict);
        });

        // DocumentType
        modelBuilder.Entity<DocumentType>(e =>
        {
            e.ToTable("DocumentTypes");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Description).HasMaxLength(1000);
            e.Property(x => x.Category).HasMaxLength(100);
        });

        // Quiz
        modelBuilder.Entity<Quiz>(e =>
        {
            e.ToTable("Quizzes");
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(300).IsRequired();
            e.Property(x => x.Description).HasMaxLength(2000);
            e.Property(x => x.Category).HasMaxLength(100);
        });

        modelBuilder.Entity<QuizQuestion>(e =>
        {
            e.ToTable("QuizQuestions");
            e.HasKey(x => x.Id);
            e.Property(x => x.QuestionText).HasMaxLength(2000).IsRequired();
            e.Property(x => x.QuestionType).HasMaxLength(50).IsRequired();
            e.HasOne(x => x.Quiz).WithMany(q => q.Questions).HasForeignKey(x => x.QuizId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QuizOption>(e =>
        {
            e.ToTable("QuizOptions");
            e.HasKey(x => x.Id);
            e.Property(x => x.OptionText).HasMaxLength(500).IsRequired();
            e.HasOne(x => x.Question).WithMany(q => q.Options).HasForeignKey(x => x.QuestionId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QuizAttempt>(e =>
        {
            e.ToTable("QuizAttempts");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Quiz).WithMany(q => q.Attempts).HasForeignKey(x => x.QuizId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<QuizAttemptAnswer>(e =>
        {
            e.ToTable("QuizAttemptAnswers");
            e.HasKey(x => x.Id);
            e.Property(x => x.TextAnswer).HasMaxLength(2000);
            e.HasOne(x => x.Attempt).WithMany(a => a.Answers).HasForeignKey(x => x.AttemptId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Question).WithMany().HasForeignKey(x => x.QuestionId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.SelectedOption).WithMany().HasForeignKey(x => x.SelectedOptionId).OnDelete(DeleteBehavior.Restrict);
        });

        // JobPosting
        modelBuilder.Entity<JobPosting>(e =>
        {
            e.ToTable("JobPostings");
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(300).IsRequired();
            e.Property(x => x.Description).HasMaxLength(5000);
            e.Property(x => x.Location).HasMaxLength(200);
            e.Property(x => x.VehicleType).HasMaxLength(100);
            e.Property(x => x.PayRate).HasColumnType("decimal(10,2)");
            e.Property(x => x.Status).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<JobApplication>(e =>
        {
            e.ToTable("JobApplications");
            e.HasKey(x => x.Id);
            e.Property(x => x.Status).HasMaxLength(50).IsRequired();
            e.HasOne(x => x.Posting).WithMany(p => p.Applications).HasForeignKey(x => x.PostingId).OnDelete(DeleteBehavior.Cascade);
        });

        // RegistrationField
        modelBuilder.Entity<RegistrationField>(e =>
        {
            e.ToTable("RegistrationFields");
            e.HasKey(x => x.Id);
            e.Property(x => x.FieldName).HasMaxLength(200).IsRequired();
            e.Property(x => x.FieldType).HasMaxLength(50).IsRequired();
            e.Property(x => x.Section).HasMaxLength(100);
            e.Property(x => x.HelpText).HasMaxLength(500);
        });

        modelBuilder.Entity<RegistrationFieldOption>(e =>
        {
            e.ToTable("RegistrationFieldOptions");
            e.HasKey(x => x.Id);
            e.Property(x => x.OptionText).HasMaxLength(300).IsRequired();
            e.HasOne(x => x.Field).WithMany(f => f.Options).HasForeignKey(x => x.FieldId).OnDelete(DeleteBehavior.Cascade);
        });

        // DriverApproval
        modelBuilder.Entity<DriverApproval>(e =>
        {
            e.ToTable("DriverApprovals");
            e.HasKey(x => x.Id);
            e.Property(x => x.Status).HasMaxLength(50).IsRequired();
            e.Property(x => x.ReviewedBy).HasMaxLength(200);
            e.Property(x => x.Notes).HasMaxLength(2000);
        });

        // Conversation
        modelBuilder.Entity<Conversation>(e =>
        {
            e.ToTable("Conversations");
            e.HasKey(x => x.Id);
            e.Property(x => x.Subject).HasMaxLength(300);
            e.Property(x => x.Status).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<ConversationMessage>(e =>
        {
            e.ToTable("ConversationMessages");
            e.HasKey(x => x.Id);
            e.Property(x => x.SenderType).HasMaxLength(50).IsRequired();
            e.Property(x => x.SenderName).HasMaxLength(200).IsRequired();
            e.Property(x => x.MessageText).HasMaxLength(5000).IsRequired();
            e.HasOne(x => x.Conversation).WithMany(c => c.Messages).HasForeignKey(x => x.ConversationId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}

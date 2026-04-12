using CourierPortal.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Infrastructure.Data;

/// <summary>
/// EF Core database context for the Agents &amp; Partners module.
/// </summary>
public class AgentsDbContext : DbContext
{
    public AgentsDbContext(DbContextOptions<AgentsDbContext> options) : base(options) { }

    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<AgentVehicleRate> AgentVehicleRates => Set<AgentVehicleRate>();
    public DbSet<AgentCourierRate> AgentCourierRates => Set<AgentCourierRate>();
    public DbSet<NpCourier> NpCouriers => Set<NpCourier>();
    public DbSet<NpUser> NpUsers => Set<NpUser>();
    public DbSet<MarketplacePosting> MarketplacePostings => Set<MarketplacePosting>();
    public DbSet<MarketplaceQuote> MarketplaceQuotes => Set<MarketplaceQuote>();
    public DbSet<ProspectAgent> ProspectAgents => Set<ProspectAgent>();
    public DbSet<AgentOnboarding> AgentOnboardings => Set<AgentOnboarding>();
    public DbSet<NpFeatureConfig> NpFeatureConfigs => Set<NpFeatureConfig>();
    public DbSet<CourierDocumentType> CourierDocumentTypes => Set<CourierDocumentType>();
    public DbSet<CourierDocument> CourierDocuments => Set<CourierDocument>();
    public DbSet<CourierDocumentAudit> CourierDocumentAudits => Set<CourierDocumentAudit>();
    public DbSet<CourierApplicant> CourierApplicants => Set<CourierApplicant>();
    public DbSet<RecruitmentStageConfig> RecruitmentStageConfigs => Set<RecruitmentStageConfig>();
    public DbSet<CourierContract> CourierContracts => Set<CourierContract>();
    public DbSet<TucCourierFleet> TucCourierFleets => Set<TucCourierFleet>();
    public DbSet<CourierSchedule> CourierSchedules => Set<CourierSchedule>();
    public DbSet<ScheduleTemplate> ScheduleTemplates => Set<ScheduleTemplate>();
    public DbSet<TucManualMessage> TucManualMessages => Set<TucManualMessage>();
    public DbSet<TrainingItem> TrainingItems => Set<TrainingItem>();
    public DbSet<TrainingCompletion> TrainingCompletions => Set<TrainingCompletion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Agent>(e =>
        {
            e.HasKey(a => a.AgentId);
            e.HasIndex(a => new { a.TenantId, a.Name });
            e.HasIndex(a => new { a.TenantId, a.IsNetworkPartner });
        });

        modelBuilder.Entity<AgentVehicleRate>(e =>
        {
            e.HasKey(r => r.AgentVehicleRateId);
            e.HasOne(r => r.Agent).WithMany(a => a.VehicleRates).HasForeignKey(r => r.AgentId);
        });

        modelBuilder.Entity<AgentCourierRate>(e =>
        {
            e.HasKey(r => r.AgentCourierRateId);
            e.HasOne(r => r.Agent).WithMany(a => a.CourierRates).HasForeignKey(r => r.AgentId);
            e.HasOne(r => r.Courier).WithMany().HasForeignKey(r => r.CourierId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<NpCourier>(e =>
        {
            e.HasKey(c => c.CourierId);
            e.HasOne(c => c.Agent).WithMany(a => a.Couriers).HasForeignKey(c => c.AgentId);
            e.HasOne(c => c.MasterCourier).WithMany(c => c.SubCouriers).HasForeignKey(c => c.MasterId).OnDelete(DeleteBehavior.NoAction);
            e.HasIndex(c => new { c.AgentId, c.Code }).IsUnique();
        });

        modelBuilder.Entity<NpUser>(e =>
        {
            e.HasKey(u => u.NpUserId);
            e.HasOne(u => u.Agent).WithMany(a => a.Users).HasForeignKey(u => u.AgentId);
            e.HasIndex(u => new { u.AgentId, u.Email }).IsUnique();
        });

        modelBuilder.Entity<MarketplacePosting>(e =>
        {
            e.HasKey(p => p.PostingId);
            e.HasIndex(p => new { p.TenantId, p.Status });
        });

        modelBuilder.Entity<MarketplaceQuote>(e =>
        {
            e.HasKey(q => q.QuoteId);
            e.HasOne(q => q.Posting).WithMany(p => p.Quotes).HasForeignKey(q => q.PostingId);
            e.HasOne(q => q.Agent).WithMany().HasForeignKey(q => q.AgentId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<ProspectAgent>(e =>
        {
            e.HasKey(p => p.ProspectAgentId);
            e.HasIndex(p => p.Association);
            e.HasIndex(p => new { p.City, p.State });
        });

        modelBuilder.Entity<AgentOnboarding>(e =>
        {
            e.HasKey(o => o.OnboardingId);
            e.HasIndex(o => new { o.TenantId, o.CurrentStep });
        });

        modelBuilder.Entity<NpFeatureConfig>(e =>
        {
            e.HasKey(f => f.NpFeatureConfigId);
            e.HasOne(f => f.Agent).WithMany().HasForeignKey(f => f.AgentId);
            e.HasIndex(f => f.AgentId).IsUnique();
        });

        modelBuilder.Entity<CourierDocumentType>(e =>
        {
            e.HasKey(dt => dt.Id);
            e.HasIndex(dt => new { dt.TenantId, dt.Active });
            e.Property(dt => dt.Category).HasConversion<string>();
            e.Property(dt => dt.AppliesTo).HasConversion<string>();
        });

        modelBuilder.Entity<CourierDocument>(e =>
        {
            e.HasKey(d => d.Id);
            e.HasOne(d => d.Courier).WithMany().HasForeignKey(d => d.CourierId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(d => d.DocumentType).WithMany(dt => dt.Documents).HasForeignKey(d => d.DocumentTypeId);
            e.HasIndex(d => new { d.CourierId, d.DocumentTypeId });
            e.Property(d => d.Status).HasConversion<string>();
            e.HasQueryFilter(d => !d.IsDeleted);
        });

        modelBuilder.Entity<CourierDocumentAudit>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasOne(a => a.Document).WithMany().HasForeignKey(a => a.DocumentId);
            e.Property(a => a.Action).HasConversion<string>();
            e.HasIndex(a => a.DocumentId);
        });

        modelBuilder.Entity<CourierApplicant>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasIndex(a => new { a.TenantId, a.PipelineStage });
            e.HasIndex(a => new { a.TenantId, a.Email });
            e.HasOne(a => a.ApprovedAsCourier).WithMany().HasForeignKey(a => a.ApprovedAsCourierId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<RecruitmentStageConfig>(e =>
        {
            e.HasKey(s => s.Id);
            e.HasIndex(s => new { s.TenantId, s.SortOrder });
        });

        modelBuilder.Entity<CourierContract>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasIndex(c => new { c.TenantId, c.IsActive });
        });

        modelBuilder.Entity<TucCourierFleet>(e =>
        {
            e.HasKey(f => f.UccfId);
            e.HasIndex(f => f.TenantId);
        });

        modelBuilder.Entity<CourierSchedule>(e =>
        {
            e.HasKey(s => s.Id);
            e.HasOne(s => s.Courier).WithMany().HasForeignKey(s => s.CourierId).OnDelete(DeleteBehavior.NoAction);
            e.HasIndex(s => new { s.LocationId, s.Date });
            e.HasIndex(s => new { s.CourierId, s.Date });
        });

        modelBuilder.Entity<ScheduleTemplate>(e =>
        {
            e.HasKey(t => t.Id);
            e.HasIndex(t => t.TenantId);
        });

        modelBuilder.Entity<TucManualMessage>(e =>
        {
            e.HasKey(m => m.UcmmId);
            e.HasIndex(m => m.UcmmSendToCourierId);
            e.HasIndex(m => m.UcmmSendFromCourierId);
            e.HasIndex(m => m.UcmmDate);
        });

        modelBuilder.Entity<TrainingItem>(e =>
        {
            e.HasKey(i => i.Id);
            e.HasIndex(i => new { i.TenantId, i.Active });
        });

        modelBuilder.Entity<TrainingCompletion>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasOne(c => c.TrainingItem).WithMany(i => i.Completions).HasForeignKey(c => c.TrainingItemId);
            e.HasOne(c => c.Courier).WithMany().HasForeignKey(c => c.CourierId).OnDelete(DeleteBehavior.NoAction);
            e.HasIndex(c => new { c.CourierId, c.TrainingItemId }).IsUnique();
        });
    }
}

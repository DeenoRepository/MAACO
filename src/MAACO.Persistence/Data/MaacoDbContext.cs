using MAACO.Core.Domain.Entities;
using MAACO.Core.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MAACO.Persistence.Data;

public sealed class MaacoDbContext(DbContextOptions<MaacoDbContext> options) : DbContext(options)
{
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TaskItem> TaskItems => Set<TaskItem>();
    public DbSet<Workflow> Workflows => Set<Workflow>();
    public DbSet<WorkflowStep> WorkflowSteps => Set<WorkflowStep>();
    public DbSet<AgentDefinition> AgentDefinitions => Set<AgentDefinition>();
    public DbSet<ToolExecution> ToolExecutions => Set<ToolExecution>();
    public DbSet<LogEvent> LogEvents => Set<LogEvent>();
    public DbSet<Artifact> Artifacts => Set<Artifact>();
    public DbSet<GitOperation> GitOperations => Set<GitOperation>();
    public DbSet<BuildRun> BuildRuns => Set<BuildRun>();
    public DbSet<MemoryRecord> MemoryRecords => Set<MemoryRecord>();
    public DbSet<ApprovalRequest> ApprovalRequests => Set<ApprovalRequest>();
    public DbSet<LlmCallLog> LlmCallLogs => Set<LlmCallLog>();
    public DbSet<ProjectContextSnapshot> ProjectContextSnapshots => Set<ProjectContextSnapshot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var repositoryPathConverter = new ValueConverter<RepositoryPath, string>(
            value => value.Value,
            value => new RepositoryPath(value));

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RepositoryPath).HasConversion(repositoryPathConverter);
            entity.Property(x => x.Version).IsConcurrencyToken();
            entity.HasIndex(x => x.RepositoryPath).IsUnique();
            entity.HasMany(x => x.Tasks).WithOne().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Version).IsConcurrencyToken();
            entity.HasIndex(x => x.ProjectId);
        });

        modelBuilder.Entity<Workflow>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Version).IsConcurrencyToken();
            entity.HasIndex(x => x.TaskId);
        });

        modelBuilder.Entity<WorkflowStep>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Version).IsConcurrencyToken();
            entity.HasIndex(x => x.WorkflowId);
            entity.HasOne<Workflow>().WithMany().HasForeignKey(x => x.WorkflowId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ToolExecution>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Version).IsConcurrencyToken();
            entity.HasIndex(x => x.WorkflowId);
            entity.HasOne<Workflow>().WithMany().HasForeignKey(x => x.WorkflowId).OnDelete(DeleteBehavior.Cascade);
            entity.OwnsOne(x => x.Result);
        });

        modelBuilder.Entity<LogEvent>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Version).IsConcurrencyToken();
            entity.HasIndex(x => x.WorkflowId);
            entity.HasOne<Workflow>().WithMany().HasForeignKey(x => x.WorkflowId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Artifact>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Version).IsConcurrencyToken();
            entity.HasIndex(x => x.TaskId);
            entity.HasOne<TaskItem>().WithMany().HasForeignKey(x => x.TaskId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GitOperation>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Version).IsConcurrencyToken();
            entity.HasIndex(x => x.TaskId);
            entity.HasOne<TaskItem>().WithMany().HasForeignKey(x => x.TaskId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BuildRun>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Version).IsConcurrencyToken();
            entity.HasIndex(x => x.WorkflowId);
            entity.HasOne<Workflow>().WithMany().HasForeignKey(x => x.WorkflowId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MemoryRecord>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Version).IsConcurrencyToken();
            entity.Property(x => x.EmbeddingProvider).HasMaxLength(128);
            entity.Property(x => x.EmbeddingModel).HasMaxLength(256);
            entity.Property(x => x.EmbeddingHash).HasMaxLength(128);
            entity.Property(x => x.VectorRef).HasMaxLength(512);
            entity.Property(x => x.ContentHash).HasMaxLength(128);
            entity.HasIndex(x => x.ProjectId);
            entity.HasOne<Project>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ApprovalRequest>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Version).IsConcurrencyToken();
            entity.HasIndex(x => x.Status);
            entity.HasOne<Workflow>().WithMany().HasForeignKey(x => x.WorkflowId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LlmCallLog>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Version).IsConcurrencyToken();
            entity.OwnsOne(x => x.Usage);
            entity.HasOne<Workflow>().WithMany().HasForeignKey(x => x.WorkflowId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ProjectContextSnapshot>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Version).IsConcurrencyToken();
            entity.HasOne<Project>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.OwnsOne(x => x.Stack);
        });

        modelBuilder.Entity<AgentDefinition>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Version).IsConcurrencyToken();
        });
    }
}

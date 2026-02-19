using Microsoft.EntityFrameworkCore;
using UniTask.Api.Projects;
using UniTask.Api.PullRequests;
using UniTask.Api.Tasks;
using UniTask.Api.Shared;
using UniTask.Api.Users;

namespace UniTask.Api.Shared;

public class TaskDbContext : DbContext
{
    public TaskDbContext(DbContextOptions<TaskDbContext> options)
        : base(options)
    {
    }

    public DbSet<TaskItem> Tasks { get; set; } = null!;
    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<Organisation> Organisations { get; set; } = null!;
    public DbSet<OrganisationMember> OrganisationMembers { get; set; } = null!;
    public DbSet<ProjectMember> ProjectMembers { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<TaskType> TaskTypes { get; set; } = null!;
    public DbSet<Status> Statuses { get; set; } = null!;
    public DbSet<Comment> Comments { get; set; } = null!;
    public DbSet<Label> Labels { get; set; } = null!;
    public DbSet<LabelValue> LabelValues { get; set; } = null!;
    public DbSet<Board> Boards { get; set; } = null!;
    public DbSet<TaskChange> TaskChanges { get; set; } = null!;
    public DbSet<TaskItemRelation> TaskItemRelations { get; set; } = null!;
    public DbSet<Attachment> Attachments { get; set; } = null!;
    public DbSet<PullRequest> PullRequests { get; set; } = null!;
    public DbSet<MergeStatus> MergeStatuses { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Project configuration
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.ExternalId).HasMaxLength(100);
            
            entity.HasOne(e => e.Organisation)
                .WithMany(e => e.Projects)
                .HasForeignKey(e => e.OrganisationId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasMany(e => e.Tasks)
                .WithOne(e => e.Project)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasMany(e => e.Members)
                .WithOne(e => e.Project)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(e => e.Boards)
                .WithOne(e => e.Project)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Organisation configuration
        modelBuilder.Entity<Organisation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ExternalId).HasMaxLength(100);
        });

        // OrganisationMember configuration
        modelBuilder.Entity<OrganisationMember>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Role).HasMaxLength(50);

            entity.HasOne(e => e.Organisation)
                .WithMany(e => e.Members)
                .HasForeignKey(e => e.OrganisationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ProjectMember configuration
        modelBuilder.Entity<ProjectMember>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Role).HasMaxLength(50);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DisplayName).HasMaxLength(200);
            entity.Property(e => e.ExternalId).HasMaxLength(100);
        });

        // TaskType configuration
        modelBuilder.Entity<TaskType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ExternalId).HasMaxLength(100);
            
            entity.HasOne(e => e.Project)
                .WithMany(e => e.TaskTypes)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasMany(e => e.Tasks)
                .WithOne(e => e.TaskType)
                .HasForeignKey(e => e.TaskTypeId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasMany(e => e.Statuses)
                .WithOne(e => e.TaskType)
                .HasForeignKey(e => e.TaskTypeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Status configuration
        modelBuilder.Entity<Status>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ExternalId).HasMaxLength(100);
            
            entity.HasMany(e => e.Tasks)
                .WithOne(e => e.Status)
                .HasForeignKey(e => e.StatusId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Comment configuration
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired().HasMaxLength(5000);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
            
            entity.HasOne(e => e.TaskItem)
                .WithMany(e => e.Comments)
                .HasForeignKey(e => e.TaskItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Attachment configuration
        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Url).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.InternalName).IsRequired().HasMaxLength(500);
            entity.Property(e => e.FileType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ExternalId).HasMaxLength(100);
            
            entity.HasOne(e => e.TaskItem)
                .WithMany(e => e.Attachments)
                .HasForeignKey(e => e.TaskItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Label configuration
        modelBuilder.Entity<Label>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Color).HasMaxLength(7); // For hex colors like #FFFFFF
            entity.Property(e => e.ExternalId).HasMaxLength(100);
            
            entity.HasMany(e => e.Tasks)
                .WithMany(e => e.Labels)
                .UsingEntity(j => j.ToTable("TaskLabels"));
            
            entity.HasMany(e => e.Values)
                .WithOne(e => e.Label)
                .HasForeignKey(e => e.LabelId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // LabelValue configuration
        modelBuilder.Entity<LabelValue>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Value).IsRequired().HasMaxLength(500);
        });

        // Board configuration
        modelBuilder.Entity<Board>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Goal).HasMaxLength(1000);
            entity.Property(e => e.ExternalId).HasMaxLength(100);
            
            entity.HasMany(e => e.Tasks)
                .WithOne(e => e.Board)
                .HasForeignKey(e => e.BoardId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // TaskChange configuration
        modelBuilder.Entity<TaskChange>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Field).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Value).HasMaxLength(2000);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
            
            entity.HasOne(e => e.TaskItem)
                .WithMany(e => e.Changes)
                .HasForeignKey(e => e.TaskItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TaskItem configuration
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Source).HasConversion<string>();
            entity.Property(e => e.AssignedTo).HasMaxLength(100);
            entity.Property(e => e.ExternalId).HasMaxLength(100);
            
            // Parent-Child relationship (self-referencing)
            entity.HasOne(e => e.Parent)
                .WithMany(e => e.Children)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // TaskItemRelation configuration
        modelBuilder.Entity<TaskItemRelation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FromRelationType).HasConversion<string>();
            entity.Property(e => e.ToRelationType).HasConversion<string>();
            
            entity.HasOne(e => e.FromTask)
                .WithMany(e => e.RelationsFrom)
                .HasForeignKey(e => e.FromTaskId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.ToTask)
                .WithMany(e => e.RelationsTo)
                .HasForeignKey(e => e.ToTaskId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // MergeStatus configuration
        modelBuilder.Entity<MergeStatus>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            
            entity.HasMany(e => e.PullRequests)
                .WithOne(e => e.MergeStatus)
                .HasForeignKey(e => e.MergeStatusId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // PullRequest configuration
        modelBuilder.Entity<PullRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.ExternalId).HasMaxLength(100);
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.Repository).HasMaxLength(200);
            entity.Property(e => e.SourceBranch).HasMaxLength(200);
            entity.Property(e => e.TargetBranch).HasMaxLength(200);
            
            entity.HasOne(e => e.TaskItem)
                .WithMany(e => e.PullRequests)
                .HasForeignKey(e => e.TaskItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

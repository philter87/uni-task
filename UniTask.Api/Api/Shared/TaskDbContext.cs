using Microsoft.EntityFrameworkCore;
using UniTask.Api.Projects;
using UniTask.Api.Tasks;

namespace UniTask.Api.Shared;

public class TaskDbContext : DbContext
{
    public TaskDbContext(DbContextOptions<TaskDbContext> options)
        : base(options)
    {
    }

    public DbSet<TaskItem> Tasks { get; set; } = null!;
    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<ProjectMember> ProjectMembers { get; set; } = null!;
    public DbSet<TaskType> TaskTypes { get; set; } = null!;
    public DbSet<Status> Statuses { get; set; } = null!;
    public DbSet<Comment> Comments { get; set; } = null!;
    public DbSet<Label> Labels { get; set; } = null!;
    public DbSet<Sprint> Sprints { get; set; } = null!;
    public DbSet<TaskChange> TaskChanges { get; set; } = null!;
    public DbSet<TaskItemRelation> TaskItemRelations { get; set; } = null!;

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
            
            entity.HasMany(e => e.Tasks)
                .WithOne(e => e.Project)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasMany(e => e.Members)
                .WithOne(e => e.Project)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(e => e.Sprints)
                .WithOne(e => e.Project)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ProjectMember configuration
        modelBuilder.Entity<ProjectMember>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Role).HasMaxLength(50);
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
        });

        // Status configuration
        modelBuilder.Entity<Status>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ExternalId).HasMaxLength(100);
            
            entity.HasOne(e => e.Project)
                .WithMany(e => e.Statuses)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.SetNull);
            
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
        });

        // Sprint configuration
        modelBuilder.Entity<Sprint>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Goal).HasMaxLength(1000);
            entity.Property(e => e.ExternalId).HasMaxLength(100);
            
            entity.HasMany(e => e.Tasks)
                .WithOne(e => e.Sprint)
                .HasForeignKey(e => e.SprintId)
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
            entity.Property(e => e.OldStatus).HasConversion<string>().HasColumnName("OldStatus");
            entity.Property(e => e.Priority).HasConversion<string>();
            entity.Property(e => e.AssignedTo).HasMaxLength(100);
            entity.Property(e => e.Source).HasMaxLength(50);
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
    }
}

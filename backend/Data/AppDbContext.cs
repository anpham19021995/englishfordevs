using EnglishForDevs.Api.Data.Entities;
using EnglishForDevs.Api.Shared;
using Microsoft.EntityFrameworkCore;

namespace EnglishForDevs.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<PracticeAttempt> PracticeAttempts => Set<PracticeAttempt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(user => user.Id);
            entity.Property(user => user.Email).HasMaxLength(ValidationLimits.EmailMaxLength).IsRequired();
            entity.Property(user => user.PasswordHash).HasMaxLength(512).IsRequired();
            entity.HasIndex(user => user.Email).IsUnique();
        });

        modelBuilder.Entity<PracticeAttempt>(entity =>
        {
            entity.HasKey(attempt => attempt.Id);
            entity.Property(attempt => attempt.Mode).HasMaxLength(32).IsRequired();
            entity.Property(attempt => attempt.Message).HasMaxLength(ValidationLimits.PracticeMessageMaxLength).IsRequired();
            entity.Property(attempt => attempt.Source).HasMaxLength(64).IsRequired();
            entity.Property(attempt => attempt.DirectReply).HasMaxLength(ValidationLimits.PracticeFeedbackMaxLength).IsRequired();
            entity.Property(attempt => attempt.CorrectedVersion).HasMaxLength(ValidationLimits.PracticeFeedbackMaxLength).IsRequired();
            entity.Property(attempt => attempt.NaturalVersion).HasMaxLength(ValidationLimits.PracticeFeedbackMaxLength).IsRequired();
            entity.Property(attempt => attempt.ConfidenceFeedback).HasMaxLength(ValidationLimits.PracticeFeedbackMaxLength).IsRequired();
            entity.Property(attempt => attempt.FollowUpQuestion).HasMaxLength(ValidationLimits.PracticeFollowUpQuestionMaxLength).IsRequired();
            entity.HasOne(attempt => attempt.User)
                .WithMany()
                .HasForeignKey(attempt => attempt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(attempt => attempt.CreatedAt);
            entity.HasIndex(attempt => new { attempt.UserId, attempt.CreatedAt });
        });
    }
}

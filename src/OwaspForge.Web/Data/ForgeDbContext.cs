using Microsoft.EntityFrameworkCore;

namespace OwaspForge.Web.Data;

public sealed class ForgeDbContext(DbContextOptions<ForgeDbContext> options) : DbContext(options)
{
    public DbSet<ProgressRecord> Progress => Set<ProgressRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProgressRecord>(entity =>
        {
            entity.HasIndex(record => record.ChallengeId).IsUnique();
            entity.Property(record => record.ChallengeId).HasMaxLength(80);
            entity.Property(record => record.PenaltyPoints).HasDefaultValue(0);
            entity.Property(record => record.HintsUsed).HasDefaultValue(0);
            entity.Property(record => record.Attempts).HasDefaultValue(0);
        });
    }
}

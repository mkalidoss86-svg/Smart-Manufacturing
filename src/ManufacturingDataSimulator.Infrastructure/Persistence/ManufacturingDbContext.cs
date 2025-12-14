using ManufacturingDataSimulator.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ManufacturingDataSimulator.Infrastructure.Persistence;

public class ManufacturingDbContext : DbContext
{
    public ManufacturingDbContext(DbContextOptions<ManufacturingDbContext> options)
        : base(options)
    {
    }

    public DbSet<ManufacturingEvent> ManufacturingEvents { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ManufacturingEvent>(entity =>
        {
            entity.ToTable("ManufacturingEvents");
            
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.ProductionLine)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.BatchId)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.ProductId)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.Timestamp)
                .IsRequired();
            
            entity.Property(e => e.DefectType)
                .IsRequired()
                .HasConversion<string>();
            
            entity.Property(e => e.Severity)
                .IsRequired()
                .HasConversion<string>();
            
            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<string>();
            
            entity.Property(e => e.ConfidenceScore)
                .HasPrecision(5, 4);
            
            entity.Property(e => e.Metadata)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, object>()
                );
            
            entity.HasIndex(e => e.ProductionLine);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.ProductionLine, e.Timestamp });
        });
    }
}

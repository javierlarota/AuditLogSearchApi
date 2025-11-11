using AuditLogSearchApi.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using NpgsqlTypes;

namespace AuditLogSearchApi.Data
{
    public class AuditLogDbContext : DbContext
    {
        public AuditLogDbContext(DbContextOptions<AuditLogDbContext> options)
            : base(options)
        {
        }

        public DbSet<AuditLog> AuditLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var ipAddressConverter = new ValueConverter<string?, NpgsqlInet?>(
                v => v == null ? null : new NpgsqlInet(v),
                v => v == null ? null : v.ToString()
            );

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.ToTable("audit_logs");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Timestamp)
                    .HasColumnType("timestamptz")
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamptz")
                    .HasDefaultValueSql("NOW()")
                    .IsRequired();

                entity.Property(e => e.IpAddress)
                    .HasColumnType("inet")
                    .HasConversion(ipAddressConverter);

                entity.Property(e => e.Metadata)
                    .HasColumnType("jsonb");

                // Indexes are created via SQL scripts
            });
        }
    }
}

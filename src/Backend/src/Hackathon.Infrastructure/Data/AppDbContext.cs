using Microsoft.EntityFrameworkCore;
using Hackathon.Domain.Entities;

namespace Hackathon.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Server> Servers => Set<Server>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);

                entity.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255);

                entity.Property(u => u.Tg)
                .HasMaxLength(255);

                entity.HasIndex(u => u.Email)
                .IsUnique();

                entity.HasIndex(u => u.Tg)
                .IsUnique();
            }
        );

        modelBuilder.Entity<Server>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.Property(u => u.Ip)
            .HasMaxLength(255).IsRequired();

            entity.Property(u => u.IntervalMinutes)
            .HasDefaultValue(20);

            entity.Property(u => u.Protocol);

            entity.Property(u => u.Host)
            .HasMaxLength(255).IsRequired();

        });
    }
}
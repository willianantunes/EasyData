using Microsoft.EntityFrameworkCore;

using EasyData.AspNetCore.AdminDashboard.Authentication.Entities;

namespace EasyData.AspNetCore.AdminDashboard.Authentication
{
    internal class AuthDbContext : DbContext
    {
        public DbSet<AuthUser> Users { get; set; }
        public DbSet<AuthGroup> Groups { get; set; }
        public DbSet<AuthPermission> Permissions { get; set; }
        public DbSet<AuthGroupPermission> GroupPermissions { get; set; }
        public DbSet<AuthUserGroup> UserGroups { get; set; }

        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuthUser>(entity =>
            {
                entity.HasIndex(e => e.Username).IsUnique();
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.IsSuperuser).HasDefaultValue(false);
                entity.Property(e => e.DateJoined).HasDefaultValueSql("GETUTCDATE()");
            });

            modelBuilder.Entity<AuthGroup>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
            });

            modelBuilder.Entity<AuthPermission>(entity =>
            {
                entity.HasIndex(e => e.Codename).IsUnique();
            });

            modelBuilder.Entity<AuthGroupPermission>(entity =>
            {
                entity.HasIndex(e => new { e.GroupId, e.PermissionId }).IsUnique();
                entity.HasOne(e => e.Group).WithMany().HasForeignKey(e => e.GroupId);
                entity.HasOne(e => e.Permission).WithMany().HasForeignKey(e => e.PermissionId);
            });

            modelBuilder.Entity<AuthUserGroup>(entity =>
            {
                entity.HasIndex(e => new { e.UserId, e.GroupId }).IsUnique();
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);
                entity.HasOne(e => e.Group).WithMany().HasForeignKey(e => e.GroupId);
            });
        }
    }
}

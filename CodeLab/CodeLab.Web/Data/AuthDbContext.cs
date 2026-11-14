using CodeLab.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CodeLab.Web.Data;

// Mapping 7 bảng Identity đã tạo bằng SQL; không gọi Migrate/EnsureCreated.
public class AuthDbContext : IdentityDbContext<ApplicationUser>
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema("dbo");

        // Script SQL dùng khóa chuỗi 128 ký tự cho nhất quán và giới hạn kích thước index.
        builder.Entity<ApplicationUser>().Property(x => x.Id).HasMaxLength(128);
        builder.Entity<IdentityRole>().Property(x => x.Id).HasMaxLength(128);
        builder.Entity<IdentityUserRole<string>>().Property(x => x.UserId).HasMaxLength(128);
        builder.Entity<IdentityUserRole<string>>().Property(x => x.RoleId).HasMaxLength(128);
        builder.Entity<IdentityUserClaim<string>>().Property(x => x.UserId).HasMaxLength(128);
        builder.Entity<IdentityRoleClaim<string>>().Property(x => x.RoleId).HasMaxLength(128);
        builder.Entity<IdentityUserLogin<string>>().Property(x => x.UserId).HasMaxLength(128);
        builder.Entity<IdentityUserLogin<string>>().Property(x => x.LoginProvider).HasMaxLength(128);
        builder.Entity<IdentityUserLogin<string>>().Property(x => x.ProviderKey).HasMaxLength(128);
        builder.Entity<IdentityUserToken<string>>().Property(x => x.UserId).HasMaxLength(128);
        builder.Entity<IdentityUserToken<string>>().Property(x => x.LoginProvider).HasMaxLength(128);
        builder.Entity<IdentityUserToken<string>>().Property(x => x.Name).HasMaxLength(128);

        builder.Entity<ApplicationUser>().Property(x => x.HoTen).HasMaxLength(100);
        builder.Entity<ApplicationUser>().Property(x => x.DaKhoa).HasDefaultValue(false);
        builder.Entity<ApplicationUser>().Property(x => x.NgayTao).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Entity<ApplicationUser>().HasIndex(x => x.NormalizedEmail)
            .HasDatabaseName("EmailIndex").IsUnique().HasFilter("[NormalizedEmail] IS NOT NULL");
    }
}

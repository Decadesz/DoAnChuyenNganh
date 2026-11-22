using CodeLab.Web.Models;
using Microsoft.AspNetCore.Identity;
namespace CodeLab.Web.Services;
public static class AdminSeed
{
    public static async Task RunAsync(IServiceProvider services, IConfiguration config)
    {
        var email = config["SeedAdmin:Email"];
        var password = config["SeedAdmin:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;
        // Tạo phạm vi để dùng UserManager và RoleManager khi ứng dụng khởi động.
        using var scope = services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { "Admin", "HocVien" })
            if (!await roles.RoleExistsAsync(role))
            {
                var r = await roles.CreateAsync(new IdentityRole(role));
                if (!r.Succeeded)
                    throw new InvalidOperationException("Không tạo được vai trò.");
            }
        // Dùng email để kiểm tra lần chạy trước đã tạo tài khoản hay chưa.
        var existing = await users.FindByEmailAsync(email);
        if (existing != null)
        {
            if (!await users.IsInRoleAsync(existing, "Admin"))
                throw new InvalidOperationException("Email SeedAdmin đã thuộc tài khoản thường. Hãy dùng email mới; không tự nâng quyền.");
            return;
        }
        var u = new ApplicationUser { UserName = email, Email = email, HoTen = config["SeedAdmin:HoTen"] ?? "Quản trị viên", EmailConfirmed = true };
        var result = await users.CreateAsync(u, password);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
        // Gán vai trò Admin cho tài khoản vừa tạo. Nếu gán thất bại, dọn tài khoản mới.
        var added = await users.AddToRoleAsync(u, "Admin");
        if (!added.Succeeded)
        {
            await users.DeleteAsync(u);
            throw new InvalidOperationException("Không gán được quyền Admin.");
        }
    }
}

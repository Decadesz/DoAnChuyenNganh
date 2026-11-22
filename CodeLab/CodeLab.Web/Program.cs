using CodeLab.Web.Data;
using CodeLab.Web.Models;
using CodeLab.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
var builder = WebApplication.CreateBuilder(args);
// Đọc chuỗi kết nối từ cấu hình để có thể đổi SQL Server mà không sửa controller.
var connection = builder.Configuration.GetConnectionString("CodeLab") ?? throw new InvalidOperationException("Thiếu kết nối CodeLab.");
// Đăng ký DbContext để ASP.NET Core tự truyền đối tượng db vào constructor của controller.
builder.Services.AddDbContext<CodeLabContext>(o => o.UseSqlServer(connection));
// Context này ánh xạ các bảng tài khoản Identity trong cùng database CodeLab.
builder.Services.AddDbContext<AuthDbContext>(o => o.UseSqlServer(connection));
// Dùng chung dịch vụ kiểm tra quyền học trong một yêu cầu HTTP.
builder.Services.AddScoped<AccessService>();
// Bài 6 dùng Session. CodeLab dùng Identity để băm mật khẩu, quản lý vai trò và đăng nhập bằng cookie.
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(o =>
{
    o.User.RequireUniqueEmail = true;
    o.Stores.MaxLengthForKeys = 128;
    o.Password.RequiredLength = 8;
    // Identity khóa đăng nhập tạm thời sau nhiều lần nhập sai, riêng với cờ DaKhoa do quản trị viên đặt.
    o.Lockout.MaxFailedAccessAttempts = 5;
    o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
}).AddEntityFrameworkStores<AuthDbContext>().AddDefaultTokenProviders();
// Chỉ định trang đăng nhập, trang từ chối và yêu cầu cookie đăng nhập được gửi qua HTTPS.
builder.Services.ConfigureApplicationCookie(o => { o.LoginPath = "/TaiKhoan/DangNhap"; o.AccessDeniedPath = "/Home/TuChoi"; o.Cookie.HttpOnly = true; o.Cookie.SecurePolicy = CookieSecurePolicy.Always; });
// Tự kiểm tra token chống giả mạo cho form ghi dữ liệu. Validation của các ô dựa vào attribute tường minh.
builder.Services.AddControllersWithViews(o => { o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()); o.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true; });
// Giới hạn số lần gửi yêu cầu vào action tài khoản theo IP trong từng phút.
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = 429;
    o.AddPolicy("login", ctx => RateLimitPartition.GetFixedWindowLimiter(ctx.Connection.RemoteIpAddress?.ToString() ?? "guest", _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});
var app = builder.Build();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Home/Loi"); app.UseHsts(); }
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
// Đọc cookie và tạo User trước khi kiểm tra trạng thái khóa và quyền truy cập.
app.UseAuthentication();
// Khóa tài khoản có hiệu lực ngay cả với cookie đang đăng nhập.
app.Use(async (ctx, next) =>
{
    if (ctx.User.Identity?.IsAuthenticated == true)
    {
        // Lấy UserManager thuộc yêu cầu hiện tại để đọc lại tài khoản từ database.
        var users = ctx.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        var u = await users.GetUserAsync(ctx.User);
        if (u == null || u.DaKhoa)
        {
            await ctx.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>().SignOutAsync();
            ctx.Response.Redirect("/TaiKhoan/DangNhap");
            return;
        }
    }
    // Tài khoản hợp lệ thì chuyển yêu cầu sang bước xử lý tiếp theo.
    await next();
});
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
// Tạo tài khoản quản trị ban đầu khi cấu hình SeedAdmin có email và mật khẩu.
await AdminSeed.RunAsync(app.Services, app.Configuration);
app.Run();
// Khai báo Program công khai để dự án kiểm thử khởi động ứng dụng MVC.
public partial class Program
{
}

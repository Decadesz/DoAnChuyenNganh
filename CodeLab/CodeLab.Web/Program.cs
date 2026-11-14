using CodeLab.Web.Data;
using CodeLab.Web.Models;
using CodeLab.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
var builder = WebApplication.CreateBuilder(args);
var connection = builder.Configuration.GetConnectionString("CodeLab") ?? throw new InvalidOperationException("Thiếu kết nối CodeLab.");
builder.Services.AddDbContext<CodeLabContext>(o => o.UseSqlServer(connection));
builder.Services.AddDbContext<AuthDbContext>(o => o.UseSqlServer(connection));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(o =>
{
    o.User.RequireUniqueEmail = true;
    o.Stores.MaxLengthForKeys = 128;
    o.Password.RequiredLength = 8;
    o.Lockout.MaxFailedAccessAttempts = 5;
    o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
}).AddEntityFrameworkStores<AuthDbContext>().AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(o => { o.LoginPath = "/TaiKhoan/DangNhap"; o.AccessDeniedPath = "/Home/TuChoi"; o.Cookie.HttpOnly = true; o.Cookie.SecurePolicy = CookieSecurePolicy.Always; });
builder.Services.AddControllersWithViews(o => { o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()); o.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true; });
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
app.UseAuthentication();
// Khóa tài khoản có hiệu lực ngay cả với cookie đang đăng nhập.
app.Use(async (ctx, next) =>
{
    if (ctx.User.Identity?.IsAuthenticated == true)
    {
        var users = ctx.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        var u = await users.GetUserAsync(ctx.User);
        if (u == null || u.DaKhoa)
        {
            await ctx.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>().SignOutAsync();
            ctx.Response.Redirect("/TaiKhoan/DangNhap");
            return;
        }
    }
    await next();
});
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
await AdminSeed.RunAsync(app.Services, app.Configuration);
app.Run();
public partial class Program
{
}

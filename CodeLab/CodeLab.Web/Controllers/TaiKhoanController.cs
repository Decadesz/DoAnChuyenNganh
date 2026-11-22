using CodeLab.Web.Models;
using CodeLab.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
namespace CodeLab.Web.Controllers;
public class TaiKhoanController : Controller
{
    // UserManager đọc và ghi tài khoản. SignInManager quản lý cookie đăng nhập.
    private readonly UserManager<ApplicationUser> users;
    private readonly SignInManager<ApplicationUser> signIn;

    // ASP.NET Core truyền các dịch vụ đã đăng ký vào constructor để action sử dụng.
    public TaiKhoanController(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn)
    {
        this.users = users;
        this.signIn = signIn;
    }

    public IActionResult DangNhap(string? returnUrl) => View(new LoginInput { ReturnUrl = returnUrl });
    // Áp dụng giới hạn tần suất login đã đăng ký trong Program.cs.
    [HttpPost, EnableRateLimiting("login")]
    // async/await chờ thao tác I/O hoàn thành mà không giữ luồng xử lý trong lúc chờ.
    public async Task<IActionResult> DangNhap(LoginInput input)
    {
        if (!ModelState.IsValid)
            return View(input);
        // Tìm tài khoản qua Identity rồi kiểm tra cờ khóa do quản trị viên đặt.
        var user = await users.FindByEmailAsync(input.Email.Trim());
        if (user != null && !user.DaKhoa)
        {
            // Identity kiểm tra mật khẩu và cấp cookie. false là phiên trình duyệt, true bật đếm lần sai để khóa tạm.
            var r = await signIn.PasswordSignInAsync(user, input.Password, false, true);
            if (r.Succeeded)
                // Chỉ quay về địa chỉ trong CodeLab để tránh chuyển hướng đến trang bên ngoài.
                return Url.IsLocalUrl(input.ReturnUrl) ? LocalRedirect(input.ReturnUrl!) : RedirectToAction("Index", "Home");
        }
        ModelState.AddModelError("", "Thông tin đăng nhập không đúng hoặc tài khoản đang bị khóa.");
        return View(input);
    }
    public IActionResult DangKy() => View(new RegisterInput());
    [HttpPost, EnableRateLimiting("login")]
    public async Task<IActionResult> DangKy(RegisterInput input)
    {
        if (!ModelState.IsValid)
            return View(input);
        var u = new ApplicationUser { UserName = input.Email.Trim(), Email = input.Email.Trim(), HoTen = input.HoTen.Trim() };
        // Identity tự băm mật khẩu khi tạo tài khoản.
        var r = await users.CreateAsync(u, input.Password);
        if (r.Succeeded)
        {
            // Tài khoản tự đăng ký chỉ nhận vai trò HocVien.
            var role = await users.AddToRoleAsync(u, "HocVien");
            if (!role.Succeeded)
            {
                // Nếu gán vai trò thất bại, xóa tài khoản vừa tạo để tránh bỏ lại tài khoản thiếu quyền.
                await users.DeleteAsync(u);
                ModelState.AddModelError("", "Chưa có vai trò HocVien. Kiểm tra SQL khởi tạo.");
                return View(input);
            }
            // Cấp cookie để học viên đăng nhập ngay sau khi đăng ký.
            await signIn.SignInAsync(u, false);
            return RedirectToAction("Index", "Home");
        }
        foreach (var e in r.Errors)
            ModelState.AddModelError("", e.Description);
        return View(input);
    }
    [Authorize, HttpPost]
    public async Task<IActionResult> DangXuat()
    {
        // Xóa cookie xác thực của phiên hiện tại.
        await signIn.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    public IActionResult DoiMatKhau() => View(new ChangePasswordInput());

    [Authorize, HttpPost, EnableRateLimiting("login")]
    public async Task<IActionResult> DoiMatKhau(ChangePasswordInput input)
    {
        if (!ModelState.IsValid) return View(input);
        var user = await users.GetUserAsync(User);
        if (user == null) return Challenge();
        // Identity kiểm tra mật khẩu cũ trước khi băm và lưu mật khẩu mới.
        var result = await users.ChangePasswordAsync(user, input.OldPassword, input.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            return View(input);
        }
        // Refresh this login after Identity changes the password and security stamp.
        // Cấp lại cookie vì đổi mật khẩu làm thay đổi mã xác thực phiên.
        await signIn.RefreshSignInAsync(user);
        // Giữ thông báo qua lần chuyển trang kế tiếp để layout hiển thị.
        TempData["Message"] = "Đã đổi mật khẩu.";
        return RedirectToAction(nameof(DoiMatKhau));
    }
}

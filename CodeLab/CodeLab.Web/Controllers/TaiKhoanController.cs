using CodeLab.Web.Models;
using CodeLab.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
namespace CodeLab.Web.Controllers;
public class TaiKhoanController(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn) : Controller
{
    public IActionResult DangNhap(string? returnUrl) => View(new LoginInput { ReturnUrl = returnUrl });
    [HttpPost, EnableRateLimiting("login")]
    public async Task<IActionResult> DangNhap(LoginInput input)
    {
        if (!ModelState.IsValid)
            return View(input);
        var user = await users.FindByEmailAsync(input.Email.Trim());
        if (user != null && !user.DaKhoa)
        {
            var r = await signIn.PasswordSignInAsync(user, input.Password, false, true);
            if (r.Succeeded)
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
        var r = await users.CreateAsync(u, input.Password);
        if (r.Succeeded)
        {
            var role = await users.AddToRoleAsync(u, "HocVien");
            if (!role.Succeeded)
            {
                await users.DeleteAsync(u);
                ModelState.AddModelError("", "Chưa có vai trò HocVien. Kiểm tra SQL khởi tạo.");
                return View(input);
            }
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
        var result = await users.ChangePasswordAsync(user, input.OldPassword, input.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            return View(input);
        }
        // Refresh this login after Identity changes the password and security stamp.
        await signIn.RefreshSignInAsync(user);
        TempData["Message"] = "Đã đổi mật khẩu.";
        return RedirectToAction(nameof(DoiMatKhau));
    }
}

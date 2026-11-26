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
        if (user != null)
        {
            if(user.DaKhoa==true)
            {
                ModelState.AddModelError("", "Tài khoản đang bị khóa.");
                return View(input);
            }
            // Identity kiểm tra mật khẩu và cấp cookie. false là phiên trình duyệt, true bật đếm lần sai để khóa tạm.
            var r = await signIn.PasswordSignInAsync(user, input.Password, false, true);
            if (r.Succeeded)
                // Chỉ quay về địa chỉ trong CodeLab để tránh chuyển hướng đến trang bên ngoài.
            return Url.IsLocalUrl(input.ReturnUrl) ? LocalRedirect(input.ReturnUrl!) : RedirectToAction("Index", "Home");
            else
            {
                ModelState.AddModelError("", "Thông tin đăng nhập không đúng.");
                return View(input);
            }
        }
        else
        {
            ModelState.AddModelError("", "Thông tin đăng nhập không đúng.");
            return View(input);
        }
    }
    public IActionResult DangKy() => View(new RegisterInput());

[HttpPost, EnableRateLimiting("login")]
public async Task<IActionResult> DangKy(RegisterInput input)
{
    if (!ModelState.IsValid)
        return View(input);

    var email = input.Email.Trim();
    var user = new ApplicationUser
    {
        UserName = email,
        Email = email,
        HoTen = input.HoTen.Trim()
    };

    // Identity kiểm tra dữ liệu và băm mật khẩu khi tạo tài khoản.
    var result = await users.CreateAsync(user, input.Password);

    if (!result.Succeeded)
    {
        if (result.Errors.Any(e => e.Code == "DuplicateUserName"
                                || e.Code == "DuplicateEmail"))
        {
            ModelState.AddModelError("", "Email này đã được đăng ký.");
            return View(input);
        }

        foreach (var error in result.Errors)
        {
            if (error.Code == "PasswordTooShort")
            {
                int doDaiToiThieu = users.Options.Password.RequiredLength;
                ModelState.AddModelError("", $"Mật khẩu phải có ít nhất {doDaiToiThieu} ký tự.");
            }
            else if (error.Code == "PasswordRequiresDigit")
            {
                ModelState.AddModelError("", "Mật khẩu phải có ít nhất một chữ số.");
            }
            else if (error.Code == "PasswordRequiresLower")
            {
                ModelState.AddModelError("", "Mật khẩu phải có ít nhất một chữ thường.");
            }
            else if (error.Code == "PasswordRequiresUpper")
            {
                ModelState.AddModelError("", "Mật khẩu phải có ít nhất một chữ hoa.");
            }
            else if (error.Code == "PasswordRequiresNonAlphanumeric")
            {
                ModelState.AddModelError("", "Mật khẩu phải có ít nhất một ký tự đặc biệt.");
            }
            else if (error.Code == "InvalidUserName" || error.Code == "InvalidEmail")
            {
                ModelState.AddModelError("", "Email không hợp lệ. Vui lòng kiểm tra lại.");
            }
            else
            {
                ModelState.AddModelError("", error.Description);
            }
        }

        return View(input);
    }

    var roleResult = await users.AddToRoleAsync(user, "HocVien");
    if (!roleResult.Succeeded)
    {
        await users.DeleteAsync(user);
        ModelState.AddModelError("", "Không thể hoàn tất đăng ký. Vui lòng thử lại sau.");
        return View(input);
    }

    await signIn.SignInAsync(user, false);
    return RedirectToAction("Index", "Home");
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
            foreach (var error in result.Errors) ModelState.AddModelError("","Mật khẩu cũ không đúng!");
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

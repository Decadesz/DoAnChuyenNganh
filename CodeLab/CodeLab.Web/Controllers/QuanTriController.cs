using CodeLab.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace CodeLab.Web.Controllers;
// Chỉ tài khoản có vai trò Admin mới truy cập được phần này.
[Authorize(Roles = "Admin")]
public class QuanTriController : Controller
{
    private readonly UserManager<ApplicationUser> users;

    // ASP.NET Core truyền các dịch vụ đã đăng ký vào constructor để action sử dụng.
    public QuanTriController(UserManager<ApplicationUser> users)
    {
        this.users = users;
    }

    public IActionResult Index() => RedirectToAction(nameof(TaiKhoan));
    // async/await chờ thao tác I/O hoàn thành mà không giữ luồng xử lý trong lúc chờ.
    public async Task<IActionResult> TaiKhoan(string? q)
    {
        // Identity lấy danh sách tài khoản thuộc vai trò HocVien.
        var rows = await users.GetUsersInRoleAsync("HocVien");
        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            rows = rows.Where(x => x.HoTen.Contains(q, StringComparison.OrdinalIgnoreCase)
                || (x.Email ?? "").Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        return View(rows.OrderBy(x => x.Email).ToList());
    }
    [HttpPost]
    public async Task<IActionResult> Khoa(string id)
    {
        var user = await users.FindByIdAsync(id);
        if (user == null) return NotFound();
        // Chỉ đổi trạng thái học viên. Kiểm tra cả vai trò Admin và ID tài khoản đang đăng nhập.
        if (!await users.IsInRoleAsync(user, "HocVien") || await users.IsInRoleAsync(user, "Admin")
            || user.Id == User.FindFirstValue(ClaimTypes.NameIdentifier)) return Forbid();
        user.DaKhoa = !user.DaKhoa;
        var result = await users.UpdateAsync(user);
        // Đổi mã xác thực phiên để Identity có thể vô hiệu hóa cookie phát trước đó.
        if (result.Succeeded) await users.UpdateSecurityStampAsync(user);
        // Giữ thông báo qua lần chuyển trang kế tiếp để layout hiển thị.
        TempData["Message"] = result.Succeeded ? "Đã cập nhật tài khoản." : "Chưa lưu được thay đổi.";
        return RedirectToAction(nameof(TaiKhoan));
    }
}

using CodeLab.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace CodeLab.Web.Controllers;
[Authorize(Roles = "Admin")]
public class QuanTriController(UserManager<ApplicationUser> users) : Controller
{
    public IActionResult Index() => RedirectToAction(nameof(TaiKhoan));
    public async Task<IActionResult> TaiKhoan(string? q)
    {
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
        if (!await users.IsInRoleAsync(user, "HocVien") || await users.IsInRoleAsync(user, "Admin")
            || user.Id == User.FindFirstValue(ClaimTypes.NameIdentifier)) return Forbid();
        user.DaKhoa = !user.DaKhoa;
        var result = await users.UpdateAsync(user);
        if (result.Succeeded) await users.UpdateSecurityStampAsync(user);
        TempData["Message"] = result.Succeeded ? "Đã cập nhật tài khoản." : "Chưa lưu được thay đổi.";
        return RedirectToAction(nameof(TaiKhoan));
    }
}

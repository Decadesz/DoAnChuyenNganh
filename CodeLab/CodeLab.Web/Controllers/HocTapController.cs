using CodeLab.Web.Data;
using CodeLab.Web.Models;
using CodeLab.Web.ViewModels;
using CodeLab.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
namespace CodeLab.Web.Controllers;
[Authorize]
public class HocTapController : Controller
{
    private readonly CodeLabContext db;
    private readonly AccessService access;

    // ASP.NET Core truyền các dịch vụ đã đăng ký vào constructor để action sử dụng.
    public HocTapController(CodeLabContext db, AccessService access)
    {
        this.db = db;
        this.access = access;
    }

    // NameIdentifier là mã tài khoản do Identity đặt khi đăng nhập.
    private string U => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    // async/await chờ thao tác I/O hoàn thành mà không giữ luồng xử lý trong lúc chờ.
    public async Task<IActionResult> BaiGiang(int id)
    {
        // Quản trị viên có thể xem trước. Học viên phải đạt điều kiện kiểm tra bài trong AccessService.
        if (!User.IsInRole("Admin") && !await access.Lesson(U, id))
            return Forbid();
        var l = await db.BaiGiang.FindAsync(id);
        if (l == null)
            return NotFound();
        // Gom bài giảng, tài liệu và trạng thái hoàn thành để view nhận qua một Model.
        return View(new LessonPage(l, await db.TaiLieu.Where(x => x.MaBaiGiang == id).ToListAsync(), await db.HoanThanhBaiGiang.AnyAsync(x => x.MaHocVien == U && x.MaBaiGiang == id && x.DaHoanThanh)));
    }
    [HttpPost]
    public async Task<IActionResult> HoanThanh(int id)
    {
        if (!await access.Lesson(U, id))
            return Forbid();
        var l = await db.BaiGiang.FindAsync(id);
        if (l == null)
            return NotFound();
        // FindAsync đọc theo khóa ghép gồm mã học viên và mã bài.
        var r = await db.HoanThanhBaiGiang.FindAsync(U, id);
        // Gửi xác nhận lần nữa thì quay lại bài và giữ thời điểm hoàn thành cũ.
        if (r?.DaHoanThanh == true) return RedirectToAction(nameof(BaiGiang), new { id });
        // Lưu theo giờ UTC để các mốc thời gian nhất quán giữa các máy chủ.
        var now = DateTime.UtcNow;
        if (r == null)
        {
            r = new()
            {
                MaHocVien = U,
                MaBaiGiang = id,
                MaKhoaHoc = l.MaKhoaHoc,
                TruyCapDauLuc = now
            };
            db.Add(r);
        }
        r.TruyCapCuoiLuc = now;
        r.DaHoanThanh = true;
        r.HoanThanhLuc = now;
        try
        {
            await db.SaveChangesAsync();
        }
        // Giữ thông báo qua lần chuyển trang kế tiếp để layout hiển thị.
        catch (DbUpdateException) { TempData["Message"] = "Bài giảng vừa được cập nhật. Hãy tải lại trang."; }
        return RedirectToAction(nameof(BaiGiang), new
        {
            id
        });
    }
}

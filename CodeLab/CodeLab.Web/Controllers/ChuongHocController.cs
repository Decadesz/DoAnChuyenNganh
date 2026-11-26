using CodeLab.Web.Data;
using CodeLab.Web.Models;
using CodeLab.Web.ViewModels;
using CodeLab.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
namespace CodeLab.Web.Controllers;
// Chỉ tài khoản có vai trò Admin mới truy cập được phần này.
[Authorize(Roles = "Admin")]
public class ChuongHocController : Controller
{
    private readonly CodeLabContext db;

    // ASP.NET Core truyền các dịch vụ đã đăng ký vào constructor để action sử dụng.
    public ChuongHocController(CodeLabContext db)
    {
        this.db = db;
    }

    // async/await chờ thao tác I/O hoàn thành mà không giữ luồng xử lý trong lúc chờ.
    // Truy vấn chỉ đọc dùng AsNoTracking để EF Core không theo dõi thay đổi của dữ liệu trả về.
    public async Task<IActionResult> Index(string? tuKhoa, int? maKhoaHoc)
    {
        var query = db.ChuongHoc.AsNoTracking();

        if (maKhoaHoc.HasValue)
        {
            query = query.Where(chuong => chuong.MaKhoaHoc == maKhoaHoc.Value);
        }

        var tuKhoaTim = tuKhoa?.Trim() ?? "";

        if (tuKhoaTim.Length > 0)
        {
            query = query.Where(chuong =>
                chuong.TenChuong.Contains(tuKhoaTim) ||
                db.KhoaHoc.Any(khoa =>
                    khoa.MaKhoaHoc == chuong.MaKhoaHoc &&
                    khoa.TenKhoaHoc.Contains(tuKhoaTim)));
        }

        ViewBag.TenKhoaHoc = await db.KhoaHoc.AsNoTracking()
            .ToDictionaryAsync(khoa => khoa.MaKhoaHoc, khoa => khoa.TenKhoaHoc);

        ViewBag.TuKhoa = tuKhoaTim;
        ViewBag.MaKhoaHocDaChon = maKhoaHoc;

        var danhSach = await query
            .OrderBy(chuong => chuong.MaKhoaHoc)
            .ThenBy(chuong => chuong.ThuTu)
            .ToListAsync();

        return View(danhSach);
    }
    public async Task<IActionResult> Edit(int id = 0)
    {
        await Lists();
        if (id == 0)
            return View(new ChuongHocInput());
        var row = await db.ChuongHoc.FindAsync(id);
        if (row == null)
            return NotFound();
        return View(new ChuongHocInput { MaChuong = row.MaChuong, MaKhoaHoc = row.MaKhoaHoc, TenChuong = row.TenChuong, ThuTu = row.ThuTu });
    }
    [HttpPost]
    public async Task<IActionResult> Edit(int id, ChuongHocInput input)
    {
        await Lists();
        if (id != input.MaChuong)
            return BadRequest();
        if (id != 0 && input.ThuTu is null)
        {
            ModelState.AddModelError(nameof(input.ThuTu), "Vui lòng nhập thứ tự.");
        }
        if (!ModelState.IsValid)
            return View(input);
        var row = id == 0 ? new ChuongHoc() : await db.ChuongHoc.FindAsync(id);
        if (row == null)
            return NotFound();
        // Khi sửa chương, giữ khóa hiện tại để các bài bên trong vẫn thuộc đúng khóa.
        if (id != 0 && row.MaKhoaHoc != input.MaKhoaHoc)
        {
            ModelState.AddModelError("", "Không chuyển bản ghi sang chương/khóa/bài tập khác. Hãy tạo bản mới.");
            return View(input);
        }

        row.MaKhoaHoc = input.MaKhoaHoc;
        row.TenChuong = input.TenChuong;
        row.ThuTu = id == 0
            ? (await db.ChuongHoc
                .Where(x => x.MaKhoaHoc == row.MaKhoaHoc)
                .MaxAsync(x => (int?)x.ThuTu) ?? 0) + 1
            : input.ThuTu.GetValueOrDefault();

        if (!await db.KhoaHoc.AnyAsync(x => x.MaKhoaHoc == row.MaKhoaHoc))
        {
            ModelState.AddModelError("", "Khóa học không tồn tại.");
            return View(input);
        }
        if (await db.ChuongHoc.AnyAsync(x => x.MaKhoaHoc == row.MaKhoaHoc && x.ThuTu == row.ThuTu && x.MaChuong != row.MaChuong))
        {
            ModelState.AddModelError("", "Thứ tự chương đã tồn tại.");
            return View(input);
        }
        if (id == 0)
            db.ChuongHoc.Add(row);
        try
        {
            await db.SaveChangesAsync();
            // Giữ thông báo qua lần chuyển trang kế tiếp để layout hiển thị.
            TempData["Message"] = "Đã lưu chương học.";
            return RedirectToAction(nameof(Index));
        }
        // EF Core báo xung đột khi bản ghi đã thay đổi kể từ phiên bản được gửi.
        catch (DbUpdateConcurrencyException) { ModelState.AddModelError("", "Dữ liệu đã thay đổi. Mở lại trang sửa trước khi lưu."); }
        catch (DbUpdateException) { ModelState.AddModelError("", "Không lưu được: kiểm tra mã liên kết, thứ tự trùng hoặc ràng buộc dữ liệu."); }
        return View(input);
    }
    private async Task Lists()
    {
        ViewBag.Courses = new SelectList(await db.KhoaHoc.AsNoTracking().ToListAsync(), "MaKhoaHoc", "TenKhoaHoc");
    }
}

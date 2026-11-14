using CodeLab.Web.Data;
using CodeLab.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace CodeLab.Web.Controllers;
// Mốc 13/11: chỉ xem danh sách, tìm kiếm, chi tiết và lộ trình.
public class HomeController(CodeLabContext db) : Controller
{
    public async Task<IActionResult> Index(string? q, int? language)
    {
        ViewBag.Languages = await db.NgonNguLapTrinh.AsNoTracking().ToListAsync();
        var rows = db.KhoaHoc.AsNoTracking().Where(x => x.HienThi);
        if (!string.IsNullOrWhiteSpace(q)) rows = rows.Where(x => x.TenKhoaHoc.Contains(q.Trim()));
        if (language.HasValue) rows = rows.Where(x => x.MaNgonNgu == language);
        return View(await rows.OrderBy(x => x.TenKhoaHoc).ToListAsync());
    }
    public async Task<IActionResult> ChiTiet(int id)
    {
        var course = await db.KhoaHoc.AsNoTracking().FirstOrDefaultAsync(x => x.MaKhoaHoc == id && x.HienThi);
        if (course == null) return NotFound();
        var chapters = await db.ChuongHoc.AsNoTracking().Where(x => x.MaKhoaHoc == id)
            .OrderBy(x => x.ThuTu).ToListAsync();
        var lessons = await db.BaiGiang.AsNoTracking().Where(x => x.MaKhoaHoc == id && x.HienThi)
            .OrderBy(x => x.ThuTu)
            .Select(x => new LessonOutline(x.MaBaiGiang, x.MaChuong, x.TenBaiGiang, x.ThuTu)).ToListAsync();
        return View(new CoursePage(course, chapters, lessons));
    }
    public IActionResult TuChoi() { Response.StatusCode = 403; return View(); }
    public IActionResult Loi() { Response.StatusCode = 500; return View(); }
}

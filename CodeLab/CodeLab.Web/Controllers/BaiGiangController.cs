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
public class BaiGiangController : Controller
{
    private readonly CodeLabContext db;

    // ASP.NET Core truyền các dịch vụ đã đăng ký vào constructor để action sử dụng.
    public BaiGiangController(CodeLabContext db)
    {
        this.db = db;
    }

    // async/await chờ thao tác I/O hoàn thành mà không giữ luồng xử lý trong lúc chờ.
    // Truy vấn chỉ đọc dùng AsNoTracking để EF Core không theo dõi thay đổi của dữ liệu trả về.
    public async Task<IActionResult> Index()
    {
        ViewBag.TenKhoaHoc = await db.KhoaHoc
            .AsNoTracking()
            .ToDictionaryAsync(k => k.MaKhoaHoc, k => k.TenKhoaHoc);

        // Tạo bảng tra: mã chương → tên chương.
        ViewBag.TenChuong = await db.ChuongHoc
            .AsNoTracking()
            .ToDictionaryAsync(c => c.MaChuong, c => c.TenChuong);

        var danhSach = await db.BaiGiang
            .AsNoTracking()
            .OrderBy(b => b.MaKhoaHoc)
            .ThenBy(b => b.MaChuong)
            .ThenBy(b => b.ThuTu)
            .ToListAsync();

        return View(danhSach);
    }
    public async Task<IActionResult> Edit(int id = 0)
    {
        if (id == 0)
        {
            await Lists();
            return View(new BaiGiangInput());
        }

        var row = await db.BaiGiang.FindAsync(id);
        if (row == null)
            return NotFound();

        await Lists(row.MaKhoaHoc);
        return View(new BaiGiangInput { MaBaiGiang = row.MaBaiGiang, MaChuong = row.MaChuong, MaKhoaHoc = row.MaKhoaHoc, TenBaiGiang = row.TenBaiGiang, NoiDung = row.NoiDung, DuongDanVideo = row.DuongDanVideo, HocThu = row.HocThu, BatBuoc = row.BatBuoc, HienThi = row.HienThi, ThuTu = row.ThuTu, Version = Convert.ToBase64String(row.RowVersion) });
    }
    [HttpPost]
    public async Task<IActionResult> Edit(int id, BaiGiangInput input)
    {
        await Lists(input.MaKhoaHoc);
        if (id != input.MaBaiGiang)
            return BadRequest();
        if (id != 0 && input.ThuTu is null)
        {
            ModelState.AddModelError(nameof(input.ThuTu), "Vui lòng nhập thứ tự.");
        }
        if (!ModelState.IsValid)
            return View(input);
        var row = id == 0 ? new BaiGiang() : await db.BaiGiang.FindAsync(id);
        if (row == null)
            return NotFound();
        if (id != 0 && row.MaChuong != input.MaChuong)
        {
            ModelState.AddModelError("", "Không chuyển bản ghi sang chương/khóa/bài tập khác. Hãy tạo bản mới.");
            return View(input);
        }
        if (id != 0 && row.MaKhoaHoc != input.MaKhoaHoc)
        {
            ModelState.AddModelError("", "Không chuyển bản ghi sang chương/khóa/bài tập khác. Hãy tạo bản mới.");
            return View(input);
        }
        if (id != 0)
        {
            try
            {
                // Giải mã phiên bản từ form để phát hiện nếu người khác đã lưu bài trong lúc sửa.
                db.Entry(row).Property(x => x.RowVersion).OriginalValue = Convert.FromBase64String(input.Version ?? "");
            }
            catch (FormatException) { ModelState.AddModelError("", "Phiên bản dữ liệu không hợp lệ."); return View(input); }
        }
        row.MaChuong = input.MaChuong;
        row.MaKhoaHoc = input.MaKhoaHoc;
        row.TenBaiGiang = input.TenBaiGiang;
        row.NoiDung = input.NoiDung ?? "";
        // Giữ đường dẫn video đã lưu hoặc kiểm tra địa chỉ HTTPS khi nhập liên kết mới.
        if (input.DuongDanVideo != row.DuongDanVideo && !string.IsNullOrWhiteSpace(input.DuongDanVideo)
            && (!Uri.TryCreate(input.DuongDanVideo, UriKind.Absolute, out var url) || url.Scheme != "https"))
        {
            ModelState.AddModelError("DuongDanVideo", "Nhập liên kết HTTPS hoặc tải video tại danh sách bài giảng.");
            return View(input);
        }
        row.DuongDanVideo = input.DuongDanVideo;
        row.HocThu = input.HocThu;
        row.BatBuoc = input.BatBuoc;
        row.HienThi = input.HienThi;
        row.ThuTu = id == 0
            ? (await db.BaiGiang
                .Where(x => x.MaChuong == row.MaChuong)
                .MaxAsync(x => (int?)x.ThuTu) ?? 0) + 1
            : input.ThuTu.GetValueOrDefault();
        if (id == 0)
            row.NgayTao = DateTime.UtcNow;
        else
            row.NgayCapNhat = DateTime.UtcNow;
        // Kiểm tra chương thực sự nằm trong khóa đã chọn trước khi lưu.
        if (!await db.ChuongHoc.AnyAsync(x => x.MaChuong == row.MaChuong && x.MaKhoaHoc == row.MaKhoaHoc))
        {
            ModelState.AddModelError("", "Chương không thuộc khóa học đã chọn.");
            return View(input);
        }
        if (await db.BaiGiang.AnyAsync(x => x.MaChuong == row.MaChuong && x.ThuTu == row.ThuTu && x.MaBaiGiang != row.MaBaiGiang))
        {
            ModelState.AddModelError("", "Thứ tự bài giảng đã tồn tại.");
            return View(input);
        }
        if (!row.HienThi && await db.KhoaHoc.AnyAsync(x => x.MaKhoaHoc == row.MaKhoaHoc && x.MoBan) && !await db.BaiGiang.AnyAsync(x => x.MaKhoaHoc == row.MaKhoaHoc && x.MaBaiGiang != row.MaBaiGiang && x.HienThi))
        {
            ModelState.AddModelError("", "Ngừng mở bán khóa học trước khi ẩn bài giảng hiển thị cuối cùng.");
            return View(input);
        }
        if (id == 0)
            db.BaiGiang.Add(row);
        try
        {
            await db.SaveChangesAsync();
            // Giữ thông báo qua lần chuyển trang kế tiếp để layout hiển thị.
            TempData["Message"] = "Đã lưu bài giảng.";
            return RedirectToAction(nameof(Index));
        }
        // EF Core báo xung đột khi bản ghi đã thay đổi kể từ phiên bản được gửi.
        catch (DbUpdateConcurrencyException) { ModelState.AddModelError("", "Dữ liệu đã thay đổi. Mở lại trang sửa trước khi lưu."); }
        catch (DbUpdateException) { ModelState.AddModelError("", "Không lưu được: kiểm tra mã liên kết, thứ tự trùng hoặc ràng buộc dữ liệu."); }
        return View(input);
    }
    private async Task Lists(int maKhoaHoc = 0)
    {
        var khoaHoc = await db.KhoaHoc.AsNoTracking()
            .OrderBy(k => k.TenKhoaHoc)
            .ToListAsync();

        var chuongHoc = await db.ChuongHoc.AsNoTracking()
            .Where(c => c.MaKhoaHoc == maKhoaHoc)
            .OrderBy(c => c.ThuTu)
            .ToListAsync();

        ViewBag.Courses = new SelectList(khoaHoc, "MaKhoaHoc", "TenKhoaHoc");
        ViewBag.Chapters = new SelectList(chuongHoc, "MaChuong", "TenChuong");
    }
    [HttpGet]
    public async Task<IActionResult> LayChuongTheoKhoa(int maKhoaHoc)
    {
        var chuongHoc = await db.ChuongHoc.AsNoTracking()
            .Where(c => c.MaKhoaHoc == maKhoaHoc)
            .OrderBy(c => c.ThuTu)
            .Select(c => new { maChuong = c.MaChuong, tenChuong = c.TenChuong })
            .ToListAsync();

        return Json(chuongHoc);
    }
}

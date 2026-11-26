using System.Security.Claims;
using CodeLab.Web.Data;
using CodeLab.Web.Models;
using CodeLab.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CodeLab.Web.Controllers;

// Chỉ tài khoản có vai trò Admin mới truy cập được phần này.
[Authorize(Roles = "Admin")]
[AutoValidateAntiforgeryToken]
public class KhoaHocController : Controller
{
    private readonly CodeLabContext db;
    private readonly ILogger<KhoaHocController> logger;

    // ASP.NET Core truyền các dịch vụ đã đăng ký vào constructor để action sử dụng.
    public KhoaHocController(CodeLabContext db, ILogger<KhoaHocController> logger)
    {
        this.db = db;
        this.logger = logger;
    }

    // async/await chờ thao tác I/O hoàn thành mà không giữ luồng xử lý trong lúc chờ.
    public async Task<IActionResult> Index(string? q, string? status)
    {
        // Truy vấn chỉ đọc dùng AsNoTracking để EF Core không theo dõi thay đổi của dữ liệu trả về.
        var query = db.KhoaHoc.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(x => x.TenKhoaHoc.Contains(q.Trim()));
        // Chọn biểu thức lọc LINQ theo trạng thái nhận từ ô lọc.
        query = status switch
        {
            "hidden" => query.Where(x => !x.HienThi),
            "open" => query.Where(x => x.HienThi && x.MoBan),
            "closed" => query.Where(x => x.HienThi && !x.MoBan),
            _ => query
        };
        // Tạo bảng tra tên ngôn ngữ theo mã để view lấy tên cho từng khóa.
        ViewBag.Languages = await db.NgonNguLapTrinh.AsNoTracking().ToDictionaryAsync(x => x.MaNgonNgu, x => x.TenNgonNgu);
        return View(await query.OrderBy(x => x.TenKhoaHoc).ToListAsync());
    }

    public async Task<IActionResult> Create()
    {
        await Languages();
        return View("Form", new KhoaHocInput());
    }

    [HttpPost]
    public async Task<IActionResult> Create(KhoaHocInput input)
    {
        // ID từ request không được dùng khi tạo mới.
        input.MaKhoaHoc = 0;
        // Bỏ giá trị mã khóa đã ghi nhận từ form tạo mới vì SQL sẽ sinh mã.
        ModelState.Remove(nameof(input.MaKhoaHoc));
        await ValidateInput(input);

        if (!ModelState.IsValid) return await Form(input);
        var course = new KhoaHoc
        {
            NgayTao = DateTime.UtcNow,
            NguoiTaoId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            HienThi = false,
            MoBan = false
        };
        Apply(input, course);
        db.KhoaHoc.Add(course);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Không thêm được khóa học");
            ModelState.AddModelError("", "Chưa lưu được khóa học. Kiểm tra dữ liệu rồi thử lại.");
            return await Form(input);
        }
        // Giữ thông báo qua lần chuyển trang kế tiếp để layout hiển thị.
        TempData["Message"] = "Đã thêm khóa học ở trạng thái ẩn, chưa mở bán. Chọn Trạng thái để công bố.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var c = await db.KhoaHoc.AsNoTracking().FirstOrDefaultAsync(x => x.MaKhoaHoc == id);
        if (c == null) return NotFound();
        return await Form(new KhoaHocInput
        {
            MaKhoaHoc = c.MaKhoaHoc, MaNgonNgu = c.MaNgonNgu, TenKhoaHoc = c.TenKhoaHoc,
            MoTaNgan = c.MoTaNgan, MoTaChiTiet = c.MoTaChiTiet, HocPhi = c.HocPhi,
            TrinhDo = c.TrinhDo, HinhDaiDien = c.HinhDaiDien, RowVersion = Convert.ToBase64String(c.RowVersion)
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, KhoaHocInput input)
    {
        if (id != input.MaKhoaHoc) return BadRequest();
        var c = await db.KhoaHoc.FindAsync(id);
        if (c == null) return NotFound();
        await ValidateInput(input);
        if (!TryVersion(input.RowVersion, out var version))
            ModelState.AddModelError("", "Phiên bản dữ liệu không hợp lệ. Mở lại trang Sửa.");
        if (!ModelState.IsValid) return await Form(input);
        // Dùng phiên bản lúc mở form để EF Core phát hiện bản ghi đã bị thay đổi.
        db.Entry(c).Property(x => x.RowVersion).OriginalValue = version;
        Apply(input, c);
        c.NgayCapNhat = DateTime.UtcNow;
        try { await db.SaveChangesAsync(); }
        // EF Core báo xung đột khi bản ghi đã thay đổi kể từ phiên bản được gửi.
        catch (DbUpdateConcurrencyException)
        {
            TempData["Message"] = "Khóa học đã thay đổi ở phiên khác. Chưa lưu nội dung bạn gửi; hãy đối chiếu dữ liệu mới và sửa lại.";
            return RedirectToAction(nameof(Edit), new { id });
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Không sửa được khóa học {Id}", id);
            ModelState.AddModelError("", "Chưa lưu được khóa học. Kiểm tra dữ liệu rồi thử lại.");
            return await Form(input);
        }
        TempData["Message"] = "Đã cập nhật thông tin khóa học.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> TrangThai(int id)
    {
        var c = await db.KhoaHoc.AsNoTracking().FirstOrDefaultAsync(x => x.MaKhoaHoc == id);
        if (c == null) return NotFound();
        ViewBag.CourseName = c.TenKhoaHoc;
        return View(new TrangThaiKhoaHocInput { MaKhoaHoc = id, HienThi = c.HienThi, MoBan = c.MoBan, RowVersion = Convert.ToBase64String(c.RowVersion) });
    }

    [HttpPost]
    public async Task<IActionResult> TrangThai(int id, TrangThaiKhoaHocInput input)
    {
        if (id != input.MaKhoaHoc) return BadRequest();
        var c = await db.KhoaHoc.FindAsync(id);
        if (c == null) return NotFound();
        ViewBag.CourseName = c.TenKhoaHoc;
        if (input.MoBan && !input.HienThi)
            ModelState.AddModelError("", "Muốn mở bán cần bật hiển thị. Muốn ẩn cần tắt mở bán trước hoặc tắt cả hai cùng lúc.");
        if (!TryVersion(input.RowVersion, out var version))
            ModelState.AddModelError("", "Phiên bản dữ liệu không hợp lệ. Mở lại trang Trạng thái.");
        if (!ModelState.IsValid) return View(input);
        // Dùng phiên bản lúc mở form để EF Core phát hiện bản ghi đã bị thay đổi.
        db.Entry(c).Property(x => x.RowVersion).OriginalValue = version;
        c.HienThi = input.HienThi;
        c.MoBan = input.MoBan;
        c.NgayCapNhat = DateTime.UtcNow;
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException)
        {
            TempData["Message"] = "Khóa học vừa được thay đổi ở phiên khác. Kiểm tra trạng thái mới trước khi lưu lại.";
            return RedirectToAction(nameof(TrangThai), new { id });
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Không lưu được trạng thái khóa học {Id}", id);
            ModelState.AddModelError("", "Chưa lưu được trạng thái. Vui lòng thử lại.");
            return View(input);
        }
        TempData["Message"] = "Đã cập nhật trạng thái khóa học.";
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateInput(KhoaHocInput input)
    {
        if (!await db.NgonNguLapTrinh.AnyAsync(x => x.MaNgonNgu == input.MaNgonNgu))
            ModelState.AddModelError(nameof(input.MaNgonNgu), "Ngôn ngữ không tồn tại.");
    }
    private async Task Languages() => ViewBag.Languages = new SelectList(
        await db.NgonNguLapTrinh.AsNoTracking().OrderBy(x => x.TenNgonNgu).ToListAsync(), "MaNgonNgu", "TenNgonNgu");
    private async Task<IActionResult> Form(KhoaHocInput input) { await Languages(); return View("Form", input); }
    // Mã phiên bản đi qua form dưới dạng Base64 và được giải mã về 8 byte của SQL Server.
    private static bool TryVersion(string? value, out byte[] version)
    {
        version = [];
        try { version = Convert.FromBase64String(value ?? ""); return version.Length == 8; }
        catch (FormatException) { return false; }
    }
    // Chép các trường được phép sửa từ form vào bản ghi đang được EF Core theo dõi.
    private static void Apply(KhoaHocInput input, KhoaHoc c)
    {
        c.MaNgonNgu = input.MaNgonNgu;
        c.TenKhoaHoc = input.TenKhoaHoc.Trim();
        c.MoTaNgan = input.MoTaNgan.Trim();
        c.MoTaChiTiet = input.MoTaChiTiet?.Trim();
        c.HocPhi = input.HocPhi;
        c.TrinhDo = input.TrinhDo;
        c.HinhDaiDien = string.IsNullOrWhiteSpace(input.HinhDaiDien) ? null : input.HinhDaiDien.Trim();
    }
}

using CodeLab.Web.Data;
using CodeLab.Web.Models;
using CodeLab.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
namespace CodeLab.Web.Controllers;
[Authorize]
public class TaiLieuController : Controller
{
    private readonly CodeLabContext db;
    private readonly AccessService access;
    private readonly IWebHostEnvironment env;

    // ASP.NET Core truyền các dịch vụ đã đăng ký vào constructor để action sử dụng.
    public TaiLieuController(CodeLabContext db, AccessService access, IWebHostEnvironment env)
    {
        this.db = db;
        this.access = access;
        this.env = env;
    }

    // App_Data nằm ngoài thư mục tệp tĩnh. Tải tài liệu phải qua action TaiVe để xét quyền.
    private string Folder => Path.Combine(env.ContentRootPath, "App_Data", "TaiLieu");
    // Chỉ tài khoản có vai trò Admin mới truy cập được phần này.
    [Authorize(Roles = "Admin")]
    // async/await chờ thao tác I/O hoàn thành mà không giữ luồng xử lý trong lúc chờ.
    public async Task<IActionResult> Index(int id)
    {
        if (!await db.BaiGiang.AnyAsync(x => x.MaBaiGiang == id))
            return NotFound();
        ViewBag.LessonId = id;
        return View(await db.TaiLieu.Where(x => x.MaBaiGiang == id).ToListAsync());
    }
    [HttpPost, Authorize(Roles = "Admin"), RequestSizeLimit(11_000_000)]
    // IFormFile là tệp ASP.NET Core nhận từ form multipart/form-data. Yêu cầu có giới hạn kích thước.
    public async Task<IActionResult> Upload(int id, string title, IFormFile? file)
    {
        if (!await db.BaiGiang.AnyAsync(x => x.MaBaiGiang == id))
            return NotFound();
        if (file == null || file.Length == 0 || file.Length > 10_000_000 || string.IsNullOrWhiteSpace(title) || title.Length > 150 || !new[] { ".pdf", ".txt", ".docx", ".xlsx", ".pptx", ".zip" }.Contains(Path.GetExtension(file.FileName).ToLowerInvariant()))
        {
            // Giữ thông báo qua lần chuyển trang kế tiếp để layout hiển thị.
            TempData["Message"] = "Chọn PDF/TXT/DOCX/XLSX/PPTX/ZIP tối đa 10 MB và tên tối đa 150 ký tự.";
            return RedirectToAction(nameof(Index), new
            {
                id
            });
        }
        Directory.CreateDirectory(Folder);
        // Sinh tên tệp riêng để tránh ghi đè khi nhiều tài liệu dùng cùng tên gốc.
        var name = Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName).ToLowerInvariant();
        var path = Path.Combine(Folder, name);
        // Luồng ghi tệp được đóng khi thoát khối, sau khi dữ liệu tải lên được sao chép.
        await using (var stream = System.IO.File.Create(path))
        {
            await file.CopyToAsync(stream);
        }
        var original = Path.GetFileName(file.FileName);
        if (original.Length > 260)
            original = original[^260..];
        db.TaiLieu.Add(new()
        {
            MaBaiGiang = id,
            TenTaiLieu = title.Trim(),
            TenTepGoc = original,
            DuongDanLuu = name,
            LoaiTep = "application/octet-stream",
            KichThuocBytes = file.Length,
            NgayTaiLen = DateTime.UtcNow
        });
        try
        {
            await db.SaveChangesAsync();
        }
        // Nếu ghi thông tin vào SQL thất bại, dọn tệp vừa ghi trên ổ đĩa.
        catch { System.IO.File.Delete(path); throw; }
        return RedirectToAction(nameof(Index), new
        {
            id
        });
    }
    public async Task<IActionResult> TaiVe(int id)
    {
        var row = await db.TaiLieu.FindAsync(id);
        if (row == null)
            return NotFound();
        if (!User.IsInRole("Admin") && !await access.Lesson(User.FindFirstValue(ClaimTypes.NameIdentifier)!, row.MaBaiGiang))
            return Forbid();
        // Chỉ chấp nhận tên tệp để việc tải về luôn đọc trong thư mục đã cấu hình.
        if (row.DuongDanLuu != Path.GetFileName(row.DuongDanLuu))
            return NotFound();
        var path = Path.Combine(Folder, row.DuongDanLuu);
        if (!System.IO.File.Exists(path))
            return NotFound();
        // Trả dữ liệu tệp sau khi kiểm tra quyền và đặt tên tải về theo tên gốc.
        return PhysicalFile(path, "application/octet-stream", row.TenTepGoc);
    }
}

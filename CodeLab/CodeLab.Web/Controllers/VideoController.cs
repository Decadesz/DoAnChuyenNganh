using System.Security.Claims;
using CodeLab.Web.Data;
using CodeLab.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CodeLab.Web.Controllers;

[Authorize]
public class VideoController : Controller
{
    private readonly CodeLabContext db;
    private readonly IWebHostEnvironment env;
    private readonly AccessService access;

    // ASP.NET Core truyền các dịch vụ đã đăng ký vào constructor để action sử dụng.
    public VideoController(CodeLabContext db, IWebHostEnvironment env, AccessService access)
    {
        this.db = db;
        this.env = env;
        this.access = access;
    }

    // Lưu video trong App_Data để trình phát phải gọi action Xem có kiểm tra quyền.
    private string ThuMuc => Path.Combine(env.ContentRootPath, "App_Data", "Video");

    // Chỉ tài khoản có vai trò Admin mới truy cập được phần này.
    [Authorize(Roles = "Admin")]
    // async/await chờ thao tác I/O hoàn thành mà không giữ luồng xử lý trong lúc chờ.
    public async Task<IActionResult> Them(int id)
    {
        var bai = await db.BaiGiang.FindAsync(id);
        return bai == null ? NotFound() : View(bai);
    }

    private const long DungLuongVideoToiDa = 2_000_000_000L;
    private const long DungLuongRequestToiDa = 2_010_000_000L;

    [HttpPost, Authorize(Roles = "Admin")]
    [RequestSizeLimit(DungLuongRequestToiDa)]
    [RequestFormLimits(MultipartBodyLengthLimit = DungLuongRequestToiDa)]
    // IFormFile là tệp ASP.NET Core nhận từ form multipart/form-data. Yêu cầu có giới hạn kích thước.
    public async Task<IActionResult> Them(int id, IFormFile? video)
    {
        var bai = await db.BaiGiang.FindAsync(id);
        if (bai == null) return NotFound();

        if (video == null || video.Length == 0 ||
            video.Length > DungLuongVideoToiDa ||
            !string.Equals(Path.GetExtension(video.FileName), ".mp4",
                StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("", "Chọn video MP4 tối đa 2 GB.");
            return View(bai);
        }
        Directory.CreateDirectory(ThuMuc);
        // Sinh tên MP4 riêng cho mỗi lượt tải để tránh trùng tên khi ghi tệp.
        var ten = Guid.NewGuid().ToString("N") + ".mp4";
        var tep = Path.Combine(ThuMuc, ten);
        var videoCu = bai.DuongDanVideo;
        try
        {
            // Sao chép tệp từ form vào luồng ghi và đóng luồng sau khi xong.
            await using (var stream = System.IO.File.Create(tep))
            {
                await video.CopyToAsync(stream);
            }
            // Lưu dấu nhận biết video nội bộ và tên tệp vào cột đường dẫn của bài.
            bai.DuongDanVideo = "video/" + ten;
            bai.NgayCapNhat = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }
        // EF Core báo xung đột khi bản ghi đã thay đổi kể từ phiên bản được gửi.
        catch (DbUpdateConcurrencyException)
        {
            System.IO.File.Delete(tep);
            ModelState.AddModelError("", "Bài vừa thay đổi. Tải lại trang rồi thử lại.");
            return View(bai);
        }
        catch
        {
            System.IO.File.Delete(tep);
            throw;
        }
        // Dọn video cũ sau khi SQL đã lưu thành công đường dẫn mới.
        if (TepNoiBo(videoCu))
            System.IO.File.Delete(Path.Combine(ThuMuc, videoCu![6..]));
        return RedirectToAction("Index", "BaiGiang");
    }

    public async Task<IActionResult> Xem(int id)
    {
        var bai = await db.BaiGiang.FindAsync(id);
        if (bai == null || !TepNoiBo(bai.DuongDanVideo)) return NotFound();
        if (!User.IsInRole("Admin") && !await access.Lesson(User.FindFirstValue(ClaimTypes.NameIdentifier)!, id))
            return Forbid();
        var tep = Path.Combine(ThuMuc, bai.DuongDanVideo![6..]);
        if (!System.IO.File.Exists(tep)) return NotFound();
        // Cho phép trình duyệt yêu cầu từng đoạn byte để phát và tua video.
        return PhysicalFile(tep, "video/mp4", enableRangeProcessing: true);
    }

    // Kiểm tra đúng tên GUID và đuôi MP4 do hệ thống tạo trước khi thao tác với tệp.
    private static bool TepNoiBo(string? path)
    {
        return path != null && path.StartsWith("video/") && path.EndsWith(".mp4") &&
            Guid.TryParseExact(Path.GetFileNameWithoutExtension(path[6..]), "N", out _) &&
            Path.GetFileName(path[6..]) == path[6..];
    }
}

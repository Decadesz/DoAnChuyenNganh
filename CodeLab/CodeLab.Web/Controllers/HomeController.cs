using CodeLab.Web.Data;
using CodeLab.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace CodeLab.Web.Controllers;
// Mốc 13/11: chỉ xem danh sách, tìm kiếm, chi tiết và lộ trình.
public class HomeController : Controller
{
    private readonly CodeLabContext db;

    // ASP.NET Core truyền các dịch vụ đã đăng ký vào constructor để action sử dụng.
    public HomeController(CodeLabContext db)
    {
        this.db = db;
    }

    // async/await chờ thao tác I/O hoàn thành mà không giữ luồng xử lý trong lúc chờ.
    public async Task<IActionResult> Index(string? q, int? language)
    {
        // Truy vấn chỉ đọc dùng AsNoTracking để EF Core không theo dõi thay đổi của dữ liệu trả về.
        ViewBag.Languages = await db.NgonNguLapTrinh.AsNoTracking().ToListAsync();
        var rows = db.KhoaHoc.AsNoTracking().Where(x => x.HienThi);
        if (!string.IsNullOrWhiteSpace(q)) rows = rows.Where(x => x.TenKhoaHoc.Contains(q.Trim()));
        if (language.HasValue) rows = rows.Where(x => x.MaNgonNgu == language);
        ViewBag.DanhSach = await rows.OrderBy(x => x.TenKhoaHoc).ToListAsync();
        return View();
    }
    public async Task<IActionResult> ChiTiet(int id)
    {
        var course = await db.KhoaHoc.AsNoTracking().FirstOrDefaultAsync(x => x.MaKhoaHoc == id && x.HienThi);
        if (course == null) return NotFound();
        ViewBag.Chuong = await db.ChuongHoc.AsNoTracking().Where(x => x.MaKhoaHoc == id)
            .OrderBy(x => x.ThuTu).ToListAsync();
        ViewBag.Bai = await db.BaiGiang.AsNoTracking().Where(x => x.MaKhoaHoc == id && x.HienThi)
            .OrderBy(x => x.ThuTu)
            // Chỉ lấy trường cần cho lộ trình. Nội dung và video được tải ở action bài giảng sau khi xét quyền.
            .Select(x => new LessonOutline(x.MaBaiGiang, x.MaChuong, x.TenBaiGiang,x.ThuTu, x.HocThu)).ToListAsync();
        // Đọc ID từ thông tin xác thực. Khách chưa đăng nhập sẽ không có ID học viên.
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        // AnyAsync chỉ hỏi database có dòng quyền học ứng với tài khoản và khóa này hay không.
        ViewBag.CoQuyen = await db.QuyenHoc.AnyAsync(x => x.MaHocVien == userId && x.MaKhoaHoc == id);
        return View(course);
    }
    public IActionResult TuChoi() { Response.StatusCode = 403; return View(); }
    public IActionResult Loi() { Response.StatusCode = 500; return View(); }
}

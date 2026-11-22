using CodeLab.Web.Data;
using Microsoft.EntityFrameworkCore;
namespace CodeLab.Web.Services;
// Dùng chung quy tắc xét quyền cho trang bài giảng, video và tài liệu.
public class AccessService(CodeLabContext db)
{
    // AnyAsync kiểm tra học viên có dòng cấp quyền cho khóa hay không.
    public Task<bool> Enrolled(string user, int course) => db.QuyenHoc.AnyAsync(x => x.MaHocVien == user && x.MaKhoaHoc == course);
    public async Task<bool> Lesson(string user, int id)
    {
        var l = await db.BaiGiang.AsNoTracking().FirstOrDefaultAsync(x => x.MaBaiGiang == id);
        if (l == null || !l.HienThi)
            return false;
        if (await Enrolled(user, l.MaKhoaHoc))
            return true;
        // Nếu chưa có quyền khóa, chỉ cho xem bài học thử khi khóa đang hiển thị.
        return l.HocThu && l.HienThi && await db.KhoaHoc.AnyAsync(x => x.MaKhoaHoc == l.MaKhoaHoc && x.HienThi);
    }
}

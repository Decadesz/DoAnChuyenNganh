using CodeLab.Web.Models;
namespace CodeLab.Web.ViewModels;
public record LessonOutline(int MaBaiGiang, int MaChuong, string TenBaiGiang, int ThuTu);
public record CoursePage(KhoaHoc Course, List<ChuongHoc> Chapters, List<LessonOutline> Lessons);

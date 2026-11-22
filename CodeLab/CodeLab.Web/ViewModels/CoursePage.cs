using CodeLab.Web.Models;
namespace CodeLab.Web.ViewModels;
public record CoursePage(KhoaHoc Course, List<ChuongHoc> Chapters, List<LessonOutline> Lessons);

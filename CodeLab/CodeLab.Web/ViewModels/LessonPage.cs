using CodeLab.Web.Models;
namespace CodeLab.Web.ViewModels;
// Record gom bài, danh sách tài liệu và trạng thái đã học để truyền sang Razor.
public record LessonPage(BaiGiang Lesson, List<TaiLieu> Documents, bool Completed);

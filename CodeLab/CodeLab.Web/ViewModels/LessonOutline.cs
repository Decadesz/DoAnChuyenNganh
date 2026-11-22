namespace CodeLab.Web.ViewModels;
// Record mang các trường cần hiển thị trong lộ trình, gồm cờ học thử.
public record LessonOutline(
    int MaBaiGiang,
    int MaChuong,
    string TenBaiGiang,
    int ThuTu,
    bool HocThu);

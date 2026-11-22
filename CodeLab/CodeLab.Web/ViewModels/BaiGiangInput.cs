using System.ComponentModel.DataAnnotations;
namespace CodeLab.Web.ViewModels;
// Form gửi ViewModel này để controller kiểm tra và chép các trường được phép sửa.
public class BaiGiangInput
{
    public int MaBaiGiang
    {
        get; set;
    }
    public string? Version
    {
        get; set;
    }
    [Display(Name = "Chương")]
    [Range(1, int.MaxValue)]
    public int MaChuong
    {
        get; set;
    }
    [Display(Name = "Khóa học")]
    [Range(1, int.MaxValue)]
    public int MaKhoaHoc
    {
        get; set;
    }
    [Display(Name = "Tên bài giảng")]
    [Required, MaxLength(150)] public string TenBaiGiang { get; set; } = "";
    [Display(Name = "Nội dung")]
    public string NoiDung { get; set; } = "";
    [Display(Name = "URL video HTTPS")]
    [MaxLength(500)] public string? DuongDanVideo { get; set; } = "";
    [Display(Name = "Cho học thử")]
    public bool HocThu
    {
        get; set;
    }
    [Display(Name = "Bắt buộc")]
    public bool BatBuoc { get; set; } = true;
    [Display(Name = "Hiển thị")]
    public bool HienThi
    {
        get; set;
    }
    [Display(Name = "Thứ tự")]
    [Range(1, int.MaxValue)] public int ThuTu { get; set; } = 1;
}

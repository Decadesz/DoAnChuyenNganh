using System.ComponentModel.DataAnnotations;
namespace CodeLab.Web.ViewModels;
// Model dành cho form chương học. Controller kiểm tra dữ liệu trước khi cập nhật bảng.
public class ChuongHocInput
{
    public int MaChuong
    {
        get; set;
    }
    public string? Version
    {
        get; set;
    }
    [Display(Name = "Khóa học")]
    [Range(1, int.MaxValue)]
    public int MaKhoaHoc
    {
        get; set;
    }
    [Display(Name = "Tên chương")]
    [Required, MaxLength(150)] 
    public string TenChuong { get; set; } = "";
    [Display(Name = "Thứ tự")]
    [Range(1, int.MaxValue, ErrorMessage = "Thứ tự phải lớn hơn 0.")]
    public int? ThuTu { get; set; }
}

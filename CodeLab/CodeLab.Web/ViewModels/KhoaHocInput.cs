using System.ComponentModel.DataAnnotations;
namespace CodeLab.Web.ViewModels;

// Chỉ nhận những trường Admin được phép sửa; không bind trực tiếp entity Scaffold.
// ViewModel nhận các trường của form. IValidatableObject bổ sung quy tắc kiểm tra ngoài attribute.
public class KhoaHocInput : IValidatableObject
{
    public int MaKhoaHoc { get; set; }
    [Required(ErrorMessage = "Nhập tên khóa học."), StringLength(150)]
    [Display(Name = "Tên khóa học")]
    public string TenKhoaHoc { get; set; } = "";
    [Range(1, int.MaxValue, ErrorMessage = "Chọn ngôn ngữ lập trình.")]
    [Display(Name = "Ngôn ngữ lập trình")]
    public int MaNgonNgu { get; set; }
    [Required(ErrorMessage = "Nhập mô tả ngắn."), StringLength(500)]
    [Display(Name = "Mô tả ngắn")]
    public string MoTaNgan { get; set; } = "";
    [StringLength(30000), Display(Name = "Mô tả chi tiết")]
    public string? MoTaChiTiet { get; set; }
    [Range(typeof(decimal), "0", "999999999999999999", ErrorMessage = "Học phí phải từ 0 đến 999999999999999999 đồng.")]
    [Display(Name = "Học phí (đồng)")]
    public decimal HocPhi { get; set; }
    [Required, RegularExpression("^(Nền tảng|Cơ bản|Nâng cao)$", ErrorMessage = "Chọn trình độ hợp lệ.")]
    [Display(Name = "Trình độ")]
    public string TrinhDo { get; set; } = "Cơ bản";
    [StringLength(500), Display(Name = "URL ảnh đại diện (HTTPS, không bắt buộc)")]
    public string? HinhDaiDien { get; set; }
    // Biểu diễn phiên bản bản ghi bằng chuỗi để truyền qua ô ẩn trong form.
    public string? RowVersion { get; set; }
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (HocPhi != decimal.Truncate(HocPhi))
            yield return new ValidationResult("Nhập học phí là số nguyên đồng.", [nameof(HocPhi)]);
        if (!string.IsNullOrWhiteSpace(HinhDaiDien) &&
            (!Uri.TryCreate(HinhDaiDien.Trim(), UriKind.Absolute, out var uri) || uri.Scheme != "https" || !string.IsNullOrEmpty(uri.UserInfo)))
            yield return new ValidationResult("Ảnh đại diện phải là URL HTTPS hợp lệ.", [nameof(HinhDaiDien)]);
    }
}

public class TrangThaiKhoaHocInput
{
    public int MaKhoaHoc { get; set; }
    [Display(Name = "Hiển thị khóa học cho khách và học viên")]
    public bool HienThi { get; set; }
    [Display(Name = "Mở bán khóa học")]
    public bool MoBan { get; set; }
    [Required] public string RowVersion { get; set; } = "";
}

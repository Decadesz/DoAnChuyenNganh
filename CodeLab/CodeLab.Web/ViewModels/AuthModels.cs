using System.ComponentModel.DataAnnotations;
namespace CodeLab.Web.ViewModels;
public class LoginInput
{
    [Display(Name = "Email")]
    [Required, EmailAddress, MaxLength(256)] 
    public string Email { get; set; } = "";
    [Display(Name = "Mật khẩu")]
    [Required, DataType(DataType.Password)] 
    public string Password { get; set; } = "";
    public string? ReturnUrl
    {
        get; set;
    }
}
public class RegisterInput
{
    [Display(Name = "Họ tên")]
    [Required, MaxLength(100)] 
    public string HoTen { get; set; } = "";
    [Display(Name = "Email")]
    [Required, EmailAddress, MaxLength(256)] 
    public string Email { get; set; } = "";
    [Display(Name = "Mật khẩu")]
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu."), MinLength(8), DataType(DataType.Password),]

    public string Password { get; set; } = "";

    [Display(Name = "Nhập lại mật khẩu")]
    [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu.")]
    [Compare(nameof(Password), ErrorMessage = "Mật khẩu nhập lại không khớp."), DataType(DataType.Password),] 
    public string ConfirmPassword { get; set; } = "";
}

public class ChangePasswordInput
{
    [Display(Name = "Mật khẩu hiện tại")]
    [Required, DataType(DataType.Password)]
    public string OldPassword { get; set; } = "";
    [Display(Name = "Mật khẩu mới")]
    [Required, MinLength(8), DataType(DataType.Password)]
    public string NewPassword { get; set; } = "";
    [Display(Name = "Nhập lại mật khẩu mới")]
    [Compare(nameof(NewPassword)), DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = "";
}

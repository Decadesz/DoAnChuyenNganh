using System.ComponentModel.DataAnnotations;
namespace CodeLab.Web.ViewModels;
public class LoginInput
{
    [Required, EmailAddress, MaxLength(256)] public string Email { get; set; } = ""; [Required, DataType(DataType.Password)] public string Password { get; set; } = ""; public string? ReturnUrl
    {
        get; set;
    }
}
public class RegisterInput
{
    [Required, MaxLength(100)] public string HoTen { get; set; } = ""; [Required, EmailAddress, MaxLength(256)] public string Email { get; set; } = ""; [Required, MinLength(8), DataType(DataType.Password)] public string Password { get; set; } = ""; [Compare(nameof(Password)), DataType(DataType.Password)] public string ConfirmPassword { get; set; } = "";
}

public class ChangePasswordInput
{
    [Required, DataType(DataType.Password)]
    public string OldPassword { get; set; } = "";
    [Required, MinLength(8), DataType(DataType.Password)]
    public string NewPassword { get; set; } = "";
    [Compare(nameof(NewPassword)), DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = "";
}

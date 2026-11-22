using Microsoft.AspNetCore.Identity;

namespace CodeLab.Web.Models;

// Kế thừa thông tin tài khoản Identity và thêm các trường dùng riêng trong CodeLab.
public class ApplicationUser : IdentityUser
{
    public string HoTen { get; set; } = "";
    public bool DaKhoa
    {
        get; set;
    }
    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
}

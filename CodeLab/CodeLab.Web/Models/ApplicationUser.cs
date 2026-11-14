using Microsoft.AspNetCore.Identity;

namespace CodeLab.Web.Models;

public class ApplicationUser : IdentityUser
{
    public string HoTen { get; set; } = "";
    public bool DaKhoa
    {
        get; set;
    }
    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
}

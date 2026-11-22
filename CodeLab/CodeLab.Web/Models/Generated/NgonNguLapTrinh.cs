namespace CodeLab.Web.Models;
public partial class NgonNguLapTrinh
{
    public int MaNgonNgu
    {
        get; set;
    }
    public string TenNgonNgu { get; set; } = "";
    public string MaJDoodle { get; set; } = "";
    public int PhienBanJDoodle
    {
        get; set;
    }
    public bool DangHoatDong
    {
        get; set;
    }
}

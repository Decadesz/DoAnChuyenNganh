namespace CodeLab.Web.Models;
public partial class TaiLieu
{
    public int MaTaiLieu
    {
        get; set;
    }
    public int MaBaiGiang
    {
        get; set;
    }
    public string TenTaiLieu { get; set; } = "";
    public string TenTepGoc { get; set; } = "";
    public string DuongDanLuu { get; set; } = "";
    public string? LoaiTep
    {
        get; set;
    }
    public long KichThuocBytes
    {
        get; set;
    }
    public DateTime NgayTaiLen
    {
        get; set;
    }
}

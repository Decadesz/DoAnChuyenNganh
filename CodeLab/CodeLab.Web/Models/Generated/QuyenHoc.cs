namespace CodeLab.Web.Models;
public partial class QuyenHoc
{
    public long MaQuyenHoc
    {
        get; set;
    }
    public string MaHocVien { get; set; } = "";
    public int MaKhoaHoc
    {
        get; set;
    }
    public long MaDonHang
    {
        get; set;
    }
    public DateTime NgayCap
    {
        get; set;
    }
    public string NguonCap { get; set; } = "";
    public string? NguoiCapId
    {
        get; set;
    }
    public string? GhiChu
    {
        get; set;
    }
}

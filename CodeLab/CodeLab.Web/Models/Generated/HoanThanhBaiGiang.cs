namespace CodeLab.Web.Models;
public partial class HoanThanhBaiGiang
{
    public string MaHocVien { get; set; } = "";
    public int MaBaiGiang
    {
        get; set;
    }
    public int MaKhoaHoc
    {
        get; set;
    }
    public DateTime TruyCapDauLuc
    {
        get; set;
    }
    public DateTime TruyCapCuoiLuc
    {
        get; set;
    }
    public bool DaHoanThanh
    {
        get; set;
    }
    public DateTime? HoanThanhLuc
    {
        get; set;
    }
}

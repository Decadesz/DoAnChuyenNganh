namespace CodeLab.Web.Models;
public partial class ChuongHoc
{
    public int MaChuong
    {
        get; set;
    }
    public int MaKhoaHoc
    {
        get; set;
    }
    public string TenChuong { get; set; } = "";
    public int ThuTu
    {
        get; set;
    }
}

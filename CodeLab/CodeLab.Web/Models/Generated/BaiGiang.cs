using System;
using System.Collections.Generic;

namespace CodeLab.Web.Models;

public partial class BaiGiang
{
    public int MaBaiGiang { get; set; }

    public int MaChuong { get; set; }

    public int MaKhoaHoc { get; set; }

    public string TenBaiGiang { get; set; } = null!;

    public string NoiDung { get; set; } = null!;

    public string? DuongDanVideo { get; set; }

    public bool HocThu { get; set; }

    public bool BatBuoc { get; set; }

    public bool HienThi { get; set; }

    public int ThuTu { get; set; }

    public DateTime NgayTao { get; set; }

    public DateTime? NgayCapNhat { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual ChuongHoc ChuongHoc { get; set; } = null!;
}

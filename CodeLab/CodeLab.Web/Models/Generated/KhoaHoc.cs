using System;
using System.Collections.Generic;

namespace CodeLab.Web.Models;

public partial class KhoaHoc
{
    public int MaKhoaHoc { get; set; }

    public int MaNgonNgu { get; set; }

    public string TenKhoaHoc { get; set; } = null!;

    public string MoTaNgan { get; set; } = null!;

    public string? MoTaChiTiet { get; set; }

    public decimal HocPhi { get; set; }

    public string TrinhDo { get; set; } = null!;

    public string? HinhDaiDien { get; set; }

    public bool HienThi { get; set; }

    public bool MoBan { get; set; }

    public string? NguoiTaoId { get; set; }

    public DateTime NgayTao { get; set; }

    public DateTime? NgayCapNhat { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual ICollection<ChuongHoc> ChuongHoc { get; set; } = new List<ChuongHoc>();

    public virtual NgonNguLapTrinh MaNgonNguNavigation { get; set; } = null!;
}

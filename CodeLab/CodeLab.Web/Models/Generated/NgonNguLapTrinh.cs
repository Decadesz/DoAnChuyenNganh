using System;
using System.Collections.Generic;

namespace CodeLab.Web.Models;

public partial class NgonNguLapTrinh
{
    public int MaNgonNgu { get; set; }

    public string TenNgonNgu { get; set; } = null!;

    public string MaJDoodle { get; set; } = null!;

    public int PhienBanJDoodle { get; set; }

    public bool DangHoatDong { get; set; }

    public virtual ICollection<KhoaHoc> KhoaHoc { get; set; } = new List<KhoaHoc>();
}

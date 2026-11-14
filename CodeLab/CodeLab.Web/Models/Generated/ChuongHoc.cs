using System;
using System.Collections.Generic;

namespace CodeLab.Web.Models;

public partial class ChuongHoc
{
    public int MaChuong { get; set; }

    public int MaKhoaHoc { get; set; }

    public string TenChuong { get; set; } = null!;

    public int ThuTu { get; set; }

    public virtual ICollection<BaiGiang> BaiGiang { get; set; } = new List<BaiGiang>();

    public virtual KhoaHoc MaKhoaHocNavigation { get; set; } = null!;
}

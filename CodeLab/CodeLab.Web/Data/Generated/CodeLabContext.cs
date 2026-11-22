using Microsoft.EntityFrameworkCore;
using CodeLab.Web.Models;
namespace CodeLab.Web.Data;
// Ánh xạ từ script SQL đính kèm; có thể thay bằng Scaffold theo hướng dẫn.
// Constructor nhận cấu hình kết nối từ Program.cs. Các DbSet bên dưới tương ứng với bảng SQL.
public partial class CodeLabContext(DbContextOptions<CodeLabContext> options) : DbContext(options)
{
    public virtual DbSet<NgonNguLapTrinh> NgonNguLapTrinh { get; set; } = null!;
    public virtual DbSet<KhoaHoc> KhoaHoc { get; set; } = null!;
    public virtual DbSet<ChuongHoc> ChuongHoc { get; set; } = null!;
    public virtual DbSet<BaiGiang> BaiGiang { get; set; } = null!;
    public virtual DbSet<TaiLieu> TaiLieu { get; set; } = null!;
    public virtual DbSet<QuyenHoc> QuyenHoc { get; set; } = null!;
    public virtual DbSet<HoanThanhBaiGiang> HoanThanhBaiGiang { get; set; } = null!;
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<NgonNguLapTrinh>(e =>
        {
            e.ToTable("NgonNguLapTrinh", "dbo");
            e.HasKey(x => new { x.MaNgonNgu });
            e.Property(x => x.MaNgonNgu).HasColumnType("int").IsRequired().ValueGeneratedOnAdd();
            e.Property(x => x.TenNgonNgu).HasColumnType("nvarchar(50)").IsRequired();
            e.Property(x => x.MaJDoodle).HasColumnType("varchar(30)").IsRequired();
            e.Property(x => x.PhienBanJDoodle).HasColumnType("int").IsRequired();
            e.Property(x => x.DangHoatDong).HasColumnType("bit").IsRequired();
        });
        b.Entity<KhoaHoc>(e =>
        {
            e.ToTable("KhoaHoc", "dbo");
            e.HasKey(x => new { x.MaKhoaHoc });
            e.Property(x => x.MaKhoaHoc).HasColumnType("int").IsRequired().ValueGeneratedOnAdd();
            e.Property(x => x.MaNgonNgu).HasColumnType("int").IsRequired();
            e.Property(x => x.TenKhoaHoc).HasColumnType("nvarchar(150)").IsRequired();
            e.Property(x => x.MoTaNgan).HasColumnType("nvarchar(500)").IsRequired();
            e.Property(x => x.MoTaChiTiet).HasColumnType("nvarchar(max)");
            e.Property(x => x.HocPhi).HasColumnType("decimal(18,0)").IsRequired();
            e.Property(x => x.TrinhDo).HasColumnType("nvarchar(30)").IsRequired();
            e.Property(x => x.HinhDaiDien).HasColumnType("nvarchar(500)");
            e.Property(x => x.HienThi).HasColumnType("bit").IsRequired();
            e.Property(x => x.MoBan).HasColumnType("bit").IsRequired();
            e.Property(x => x.NguoiTaoId).HasColumnType("nvarchar(128)");
            e.Property(x => x.NgayTao).HasColumnType("datetime2").IsRequired();
            e.Property(x => x.NgayCapNhat).HasColumnType("datetime2");
            // Khai báo cột phiên bản do SQL Server cấp để EF Core so sánh khi cập nhật.
            e.Property(x => x.RowVersion).HasColumnType("rowversion").IsRequired().IsRowVersion();
        });
        b.Entity<ChuongHoc>(e =>
        {
            e.ToTable("ChuongHoc", "dbo");
            e.HasKey(x => new { x.MaChuong });
            e.Property(x => x.MaChuong).HasColumnType("int").IsRequired().ValueGeneratedOnAdd();
            e.Property(x => x.MaKhoaHoc).HasColumnType("int").IsRequired();
            e.Property(x => x.TenChuong).HasColumnType("nvarchar(150)").IsRequired();
            e.Property(x => x.ThuTu).HasColumnType("int").IsRequired();
        });
        b.Entity<BaiGiang>(e =>
        {
            e.ToTable("BaiGiang", "dbo");
            e.HasKey(x => new { x.MaBaiGiang });
            e.Property(x => x.MaBaiGiang).HasColumnType("int").IsRequired().ValueGeneratedOnAdd();
            e.Property(x => x.MaChuong).HasColumnType("int").IsRequired();
            e.Property(x => x.MaKhoaHoc).HasColumnType("int").IsRequired();
            e.Property(x => x.TenBaiGiang).HasColumnType("nvarchar(150)").IsRequired();
            e.Property(x => x.NoiDung).HasColumnType("nvarchar(max)").IsRequired();
            e.Property(x => x.DuongDanVideo).HasColumnType("nvarchar(500)");
            e.Property(x => x.HocThu).HasColumnType("bit").IsRequired();
            e.Property(x => x.BatBuoc).HasColumnType("bit").IsRequired();
            e.Property(x => x.HienThi).HasColumnType("bit").IsRequired();
            e.Property(x => x.ThuTu).HasColumnType("int").IsRequired();
            e.Property(x => x.NgayTao).HasColumnType("datetime2").IsRequired();
            e.Property(x => x.NgayCapNhat).HasColumnType("datetime2");
            // Khai báo cột phiên bản do SQL Server cấp để EF Core so sánh khi cập nhật.
            e.Property(x => x.RowVersion).HasColumnType("rowversion").IsRequired().IsRowVersion();
        });
        b.Entity<TaiLieu>(e =>
        {
            e.ToTable("TaiLieu", "dbo");
            e.HasKey(x => new { x.MaTaiLieu });
            e.Property(x => x.MaTaiLieu).HasColumnType("int").IsRequired().ValueGeneratedOnAdd();
            e.Property(x => x.MaBaiGiang).HasColumnType("int").IsRequired();
            e.Property(x => x.TenTaiLieu).HasColumnType("nvarchar(150)").IsRequired();
            e.Property(x => x.TenTepGoc).HasColumnType("nvarchar(260)").IsRequired();
            e.Property(x => x.DuongDanLuu).HasColumnType("nvarchar(500)").IsRequired();
            e.Property(x => x.LoaiTep).HasColumnType("nvarchar(100)");
            e.Property(x => x.KichThuocBytes).HasColumnType("bigint").IsRequired();
            e.Property(x => x.NgayTaiLen).HasColumnType("datetime2").IsRequired();
        });
        b.Entity<QuyenHoc>(e =>
        {
            e.ToTable("QuyenHoc", "dbo");
            e.HasKey(x => new { x.MaQuyenHoc });
            e.Property(x => x.MaQuyenHoc).HasColumnType("bigint").IsRequired().ValueGeneratedOnAdd();
            e.Property(x => x.MaHocVien).HasColumnType("nvarchar(128)").IsRequired();
            e.Property(x => x.MaKhoaHoc).HasColumnType("int").IsRequired();
            e.Property(x => x.MaDonHang).HasColumnType("bigint").IsRequired();
            e.Property(x => x.NgayCap).HasColumnType("datetime2").IsRequired();
            e.Property(x => x.NguonCap).HasColumnType("varchar(20)").IsRequired();
            e.Property(x => x.NguoiCapId).HasColumnType("nvarchar(128)");
            e.Property(x => x.GhiChu).HasColumnType("nvarchar(1000)");
        });
        b.Entity<HoanThanhBaiGiang>(e =>
        {
            e.ToTable("HoanThanhBaiGiang", "dbo");
            // Dùng khóa ghép để mỗi học viên có một bản ghi hoàn thành cho mỗi bài.
            e.HasKey(x => new { x.MaHocVien, x.MaBaiGiang });
            e.Property(x => x.MaHocVien).HasColumnType("nvarchar(128)").IsRequired().ValueGeneratedNever();
            e.Property(x => x.MaBaiGiang).HasColumnType("int").IsRequired().ValueGeneratedNever();
            e.Property(x => x.MaKhoaHoc).HasColumnType("int").IsRequired();
            e.Property(x => x.TruyCapDauLuc).HasColumnType("datetime2").IsRequired();
            e.Property(x => x.TruyCapCuoiLuc).HasColumnType("datetime2").IsRequired();
            e.Property(x => x.DaHoanThanh).HasColumnType("bit").IsRequired();
            e.Property(x => x.HoanThanhLuc).HasColumnType("datetime2");
        });
        OnModelCreatingPartial(b);
    }
    partial void OnModelCreatingPartial(ModelBuilder b);
}

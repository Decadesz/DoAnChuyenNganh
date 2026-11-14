using System;
using System.Collections.Generic;
using CodeLab.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeLab.Web.Data;

public partial class CodeLabContext : DbContext
{
    public CodeLabContext(DbContextOptions<CodeLabContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BaiGiang> BaiGiang { get; set; }

    public virtual DbSet<ChuongHoc> ChuongHoc { get; set; }

    public virtual DbSet<KhoaHoc> KhoaHoc { get; set; }

    public virtual DbSet<NgonNguLapTrinh> NgonNguLapTrinh { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BaiGiang>(entity =>
        {
            entity.HasKey(e => e.MaBaiGiang);

            entity.HasIndex(e => new { e.MaKhoaHoc, e.BatBuoc }, "IX_BaiGiang_KhoaHoc");

            entity.HasIndex(e => new { e.MaBaiGiang, e.MaKhoaHoc }, "UQ_BaiGiang_KhoaHoc").IsUnique();

            entity.HasIndex(e => new { e.MaChuong, e.ThuTu }, "UQ_BaiGiang_ThuTu").IsUnique();

            entity.Property(e => e.BatBuoc).HasDefaultValue(true);
            entity.Property(e => e.DuongDanVideo).HasMaxLength(500);
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.NoiDung).HasDefaultValue("");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.TenBaiGiang).HasMaxLength(150);

            entity.HasOne(d => d.ChuongHoc).WithMany(p => p.BaiGiang)
                .HasPrincipalKey(p => new { p.MaChuong, p.MaKhoaHoc })
                .HasForeignKey(d => new { d.MaChuong, d.MaKhoaHoc })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BaiGiang_ChuongHoc");
        });

        modelBuilder.Entity<ChuongHoc>(entity =>
        {
            entity.HasKey(e => e.MaChuong);

            entity.HasIndex(e => new { e.MaChuong, e.MaKhoaHoc }, "UQ_ChuongHoc_KhoaHoc").IsUnique();

            entity.HasIndex(e => new { e.MaKhoaHoc, e.ThuTu }, "UQ_ChuongHoc_ThuTu").IsUnique();

            entity.Property(e => e.TenChuong).HasMaxLength(150);

            entity.HasOne(d => d.MaKhoaHocNavigation).WithMany(p => p.ChuongHoc)
                .HasForeignKey(d => d.MaKhoaHoc)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChuongHoc_KhoaHoc");
        });

        modelBuilder.Entity<KhoaHoc>(entity =>
        {
            entity.HasKey(e => e.MaKhoaHoc);

            entity.HasIndex(e => e.NguoiTaoId, "IX_KhoaHoc_NguoiTao");

            entity.HasIndex(e => new { e.HienThi, e.MaNgonNgu, e.TrinhDo }, "IX_KhoaHoc_TraCuu");

            entity.Property(e => e.HinhDaiDien).HasMaxLength(500);
            entity.Property(e => e.HocPhi).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.MoTaNgan).HasMaxLength(500);
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.NguoiTaoId).HasMaxLength(128);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.TenKhoaHoc).HasMaxLength(150);
            entity.Property(e => e.TrinhDo).HasMaxLength(30);

            entity.HasOne(d => d.MaNgonNguNavigation).WithMany(p => p.KhoaHoc)
                .HasForeignKey(d => d.MaNgonNgu)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_KhoaHoc_NgonNgu");
        });

        modelBuilder.Entity<NgonNguLapTrinh>(entity =>
        {
            entity.HasKey(e => e.MaNgonNgu);

            entity.HasIndex(e => e.MaJDoodle, "UQ_NgonNgu_JDoodle").IsUnique();

            entity.HasIndex(e => e.TenNgonNgu, "UQ_NgonNgu_Ten").IsUnique();

            entity.Property(e => e.DangHoatDong).HasDefaultValue(true);
            entity.Property(e => e.MaJDoodle)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.TenNgonNgu).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

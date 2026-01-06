namespace SixOSDatKhamAppMobile.Models
{
    public class LichHenGoiDTO
    {
        public long Id { get; set; }
        public DateTime? NgayDangKy { get; set; }
        public long? IdGoi { get; set; }
        public string TenGoi { get; set; }
        public bool? DaDen { get; set; }
        public DateTime? Ngay { get; set; }
        public string Tu { get; set; }
        public string Den { get; set; }
        public long? IdbacSi { get; set; }
        public string MaDatLich { get; set; }
        public int? Sttngay { get; set; }
        public int? Sttphong { get; set; }
        public string TenPhong { get; set; }
        public string TenCoSoNgan { get; set; }
        public decimal? TongTien { get; set; }
        public string QrCode { get; set; }
        public string TenChuyenGia { get; set; }
        public List<DichVuChiTietDTO> ChiTietDichVu { get; set; }
    }

    public class DichVuChiTietDTO
    {
        public long Id { get; set; }
        public long? IddichVu { get; set; }
        public string TenDichVu { get; set; }
        public int? SoLuong { get; set; }
        public decimal? DonGiaDichVu { get; set; }
        public string MaLoaiCls { get; set; }
        public string Tgbd { get; set; }
        public string Tgkt { get; set; }
    }

    public class LichHenNgayDTO
    {
        public long Id { get; set; }
        public long? IdbacSi { get; set; }
        public bool? DaDen { get; set; }
        public DateTime? NgayDangKy { get; set; }
        public DateTime? Ngay { get; set; }
        public string Tu { get; set; }
        public string Den { get; set; }
        public string MaDatLich { get; set; }
        public string QrCode { get; set; }
        public string TenBacSi { get; set; }
    }

    public class LichSuDatHenResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public List<LichHenGoiDTO> DanhSachTheoGoi { get; set; }
        public List<LichHenNgayDTO> DanhSachTheoNgay { get; set; }
        public List<LichHenNgayDTO> DanhSachTheoChuyenGia { get; set; }
        public List<GoiKemTheoDTO> DanhSachGoiKemTheo { get; set; }
    }

    public class GoiKemTheoDTO
    {
        public long Id { get; set; }
        public string TenGoi { get; set; }
    }
}

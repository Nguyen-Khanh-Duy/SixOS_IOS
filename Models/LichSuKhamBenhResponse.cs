namespace SixOSDatKhamAppMobile.Models
{
    public class LichSuKhamBenhResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<LichHenTheoGoi> DanhSachTheoGoi { get; set; } = new();
        public List<LichHenTheoNgay> DanhSachTheoNgay { get; set; } = new();
        public List<LichHenTheoChuyenGia> DanhSachTheoChuyenGia { get; set; } = new();
        public List<GoiKemTheo> DanhSachGoiKemTheo { get; set; } = new();
    }
    public class LichHenTheoGoi
    {
        public DateTime NgayDangKy { get; set; }
        public long? IdGoi { get; set; }
        public string TenGoi { get; set; } = string.Empty;
        public bool? DaDen { get; set; }
        public long? IdDoiTac { get; set; }
        public long Id { get; set; }
        public string Ngay { get; set; } = string.Empty;
        public string Tu { get; set; } = string.Empty;
        public string Den { get; set; } = string.Empty;

        public long? IdbacSi { get; set; }
        public string MaDatLich { get; set; } = string.Empty;
        public int? Sttngay { get; set; }
        public int? Sttphong { get; set; }
        public string TenPhong { get; set; } = string.Empty;
        public string TenCoSoNgan { get; set; } = string.Empty;
        public bool ThanhToan { get; set; }
        public decimal TongTien { get; set; }
        public List<ChiTietDichVu> ChiTietDichVu { get; set; } = new();
        public string QrCode { get; set; } = string.Empty;
        public string TenChuyenGia { get; set; } = string.Empty;
        public DateTime? NgayDateTime
        {
            get
            {
                if (DateTime.TryParseExact(Ngay, "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var result))
                    return result;
                return null;
            }
        }
    }


    public class AppointmentGoi
    {
        public DateTime NgayDangKy { get; set; }
        public long? IdGoi { get; set; }
        public string TenGoi { get; set; } = string.Empty;
        public bool DaDen { get; set; }
        public long? IdDoiTac { get; set; }
        public long Id { get; set; }
        public string Ngay { get; set; } = string.Empty;
        public string Tu { get; set; } = string.Empty;
        public string Den { get; set; } = string.Empty;

        public long? IdbacSi { get; set; }
        public string MaDatLich { get; set; } = string.Empty;
        public int? Sttngay { get; set; }
        public int? Sttphong { get; set; }
        public string TenPhong { get; set; } = string.Empty;
        public string TenCoSoNgan { get; set; } = string.Empty;
        public bool ThanhToan { get; set; }
        public decimal TongTien { get; set; }
        public List<ChiTietDichVu> ChiTietDichVu { get; set; } = new();
        public string QrCode { get; set; } = string.Empty;
        public string TenChuyenGia { get; set; } = string.Empty;
        public DateTime? NgayDateTime => DateTime.TryParse(Ngay, out var result) ? result : null;
    }

    public class AppointmentNgay
    {
        public long Id { get; set; }
        public long? IdbacSi { get; set; }
        public bool DaDen { get; set; }
        public DateTime NgayDangKy { get; set; }
        public string Ngay { get; set; } = string.Empty;
        public string Tu { get; set; } = string.Empty;
        public string Den { get; set; } = string.Empty;

        public string MaDatLich { get; set; } = string.Empty;
        public bool ThanhToan { get; set; }
        public string QrCode { get; set; } = string.Empty;

        public DateTime? NgayDateTime => DateTime.TryParse(Ngay, out var result) ? result : null;
    }

    public class AppointmentChuyenGia
    {
        public long Id { get; set; }
        public long? IdbacSi { get; set; }
        public bool DaDen { get; set; }
        public DateTime NgayDangKy { get; set; }
        public string Ngay { get; set; } = string.Empty;
        public string Tu { get; set; } = string.Empty;
        public string Den { get; set; } = string.Empty;

        public string MaDatLich { get; set; } = string.Empty;
        public bool ThanhToan { get; set; }
        public string QrCode { get; set; } = string.Empty;
        public string TenChuyenGia { get; set; } = string.Empty;
        public DateTime? NgayDateTime => DateTime.TryParse(Ngay, out var result) ? result : null;
    }

    public class ChiTietDichVu
    {
        public long Id { get; set; }
        public long? IddichVu { get; set; }
        public string TenDichVu { get; set; } = string.Empty;
        public int? SoLuong { get; set; }
        public decimal? DonGiaDichVu { get; set; }
        public int? SoPhutTkq { get; set; }
        public int? SoPhutCd { get; set; }
        public bool NhomPhauThuat { get; set; }
        public bool NhomThuThuat { get; set; }
        public bool NhomCanLamSang { get; set; }
        public bool NhomCongKham { get; set; }
        public string MaLoaiCls { get; set; } = string.Empty;
        public long? IdnhomDv { get; set; }
        public long? IdphongBuong { get; set; }
        public int? Stt { get; set; }
        public long? IdgoiChiDinh { get; set; }
        public string Tgbd { get; set; } = string.Empty;
        public string Tgkt { get; set; } = string.Empty;
    }

    public class LichHenTheoNgay
    {
        public long Id { get; set; }
        public long? IdbacSi { get; set; }
        public bool DaDen { get; set; }
        public DateTime NgayDangKy { get; set; }

        public string Ngay { get; set; } = string.Empty;
        public string Tu { get; set; } = string.Empty;
        public string Den { get; set; } = string.Empty;

        public string MaDatLich { get; set; } = string.Empty;
        public bool ThanhToan { get; set; }
        public string QrCode { get; set; } = string.Empty;
        public DateTime? NgayDateTime
        {
            get
            {
                if (DateTime.TryParseExact(Ngay, "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var result))
                    return result;
                return null;
            }
        }
    }

    public class LichHenTheoChuyenGia
    {
        public long Id { get; set; }
        public long? IdbacSi { get; set; }
        public bool DaDen { get; set; }
        public DateTime NgayDangKy { get; set; }

        public string Ngay { get; set; } = string.Empty;
        public string Tu { get; set; } = string.Empty;
        public string Den { get; set; } = string.Empty;

        public string MaDatLich { get; set; } = string.Empty;
        public bool ThanhToan { get; set; }
        public string QrCode { get; set; } = string.Empty;
        public string TenChuyenGia { get; set; } = string.Empty;
        public DateTime? NgayDateTime
        {
            get
            {
                if (DateTime.TryParseExact(Ngay, "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var result))
                    return result;
                return null;
            }
        }
    }

    public class GoiKemTheo
    {
        public long Id { get; set; }
        public string TenGoi { get; set; } = string.Empty;
    }

    public class DeleteAppointmentRequest
    {
        public long Id { get; set; }
    }

    public class DeleteAppointmentGoiRequest
    {
        public string Ngay { get; set; }
        public long IdGoi { get; set; }
    }

    public class DeleteAppointmentResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
    }
}

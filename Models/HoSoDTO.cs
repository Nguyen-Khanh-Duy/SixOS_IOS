using SixOSDatKhamAppMobile.Models.Response;

namespace SixOSDatKhamAppMobile.Models
{
    public class HoSoListResponse : BaseResponse
    {
        public List<HoSoBenhNhan>? Data { get; set; }
    }

    public class HoSoDetailResponse : BaseResponse
    {
        public HoSoDetail? Data { get; set; }
    }

    public class HoSoBenhNhan
    {
        public long IdBenhNhan { get; set; }
        public string? TenBenhNhan { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string? DienThoai { get; set; }
        public string? SoCccd { get; set; }
        public long IdTk { get; set; }
        public string? Email { get; set; }
        public string? GioiTinh { get; set; }
        public string? DiaChi { get; set; }
    }

    public class HoSoDetail
    {
        public long Id { get; set; }
        public string? HoTen { get; set; }
        public string? NgaySinh { get; set; }
        public string? GioiTinh { get; set; }
        public string? Cccd { get; set; }
        public string? QuocTich { get; set; }
        public string? NgheNghiep { get; set; }
        public string? DanToc { get; set; }
        public string? DienThoai { get; set; }
        public string? DiaChi { get; set; }
        public string? TinhThanh { get; set; }
        public long? IdPhuongXa { get; set; }
        public string? PhuongXa { get; set; }
        public string? Email { get; set; }
    }
}

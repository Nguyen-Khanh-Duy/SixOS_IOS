using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SixOSDatKhamAppMobile.Models
{
    public class LuuDatHenRequestDTO
    {
        public long IdBenhNhan { get; set; }
        public long IdDoiTac { get; set; }
        public long IdGoi { get; set; }
        public string NgayMuonDatHen { get; set; }
        public string GioHienTai { get; set; }
        public string LisIdGoiKemTheo { get; set; }
        public long IdChuyenGia { get; set; }
    }

    public class LuuDatHenResponseDTO
    {
        public string LoaiDangKy { get; set; }
        public string TenGoi { get; set; }
        public string MaHen { get; set; }
        public long IdHen { get; set; }
        public string LisIdGoiKemTheo { get; set; }
        public string Thoigian { get; set; }
        public List<object> Congviec { get; set; }
        public long IdGoi { get; set; }
        public long IdBenhNhan { get; set; }
        public string NgayDangKy { get; set; }
        public string KhuKham { get; set; }
    }

    public class ChotLichHenRequestDTO
    {
        public long IdBenhNhan { get; set; }
        public string MaHen { get; set; }
        public long IDLichHenBN { get; set; }
        public bool TrangThai { get; set; }
    }

    public class LichHenCuDTO
    {
        public int StatusCode { get; set; }
        public string LoaiDangKy { get; set; }
        public string TenGoi { get; set; }
        public string Thoigian { get; set; }
        public List<List<string>> Congviec { get; set; }
        public long? IdGoi { get; set; }
        public long? IdBenhNhan { get; set; }
        public string NgayDangKy { get; set; }
    }
}

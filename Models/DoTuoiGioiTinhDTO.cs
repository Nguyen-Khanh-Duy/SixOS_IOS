using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SixOSDatKhamAppMobile.Models
{
    public class DoTuoiGioiTinhDTO
    {
        public int Tuoi { get; set; }
        public long? GioiTinh { get; set; }
    }

    public class KiemTraTinhTrangKhamBenhDTO
    {
        public bool Success { get; set; }
        public bool Data { get; set; } // true = cho phép đăng ký, false = không cho phép
        public string Hotline { get; set; }
    }
}

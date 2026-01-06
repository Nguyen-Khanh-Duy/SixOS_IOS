using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SixOSDatKhamAppMobile.Services.S0305
{
    public class S0305_QRCCCDParserService
    {
        public class CCCDInfo
        {
            public string SoCCCD { get; set; }
            public string HoTen { get; set; }
            public DateTime? NgaySinh { get; set; }
            public string GioiTinh { get; set; }
            public string DiaChi { get; set; }
            public string TinhThanh { get; set; }
            public string NgayCap { get; set; }
        }

        // danh sách 34 tỉnh/thành phố trực thuộc Trung ương (cập nhật từ 1/7/2025)
        private static readonly Dictionary<string, string> _provinceMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // 6 thành phố trực thuộc Trung ương
            {"Hà Nội", "Thành phố Hà Nội"},
            {"Hồ Chí Minh", "Thành phố Hồ Chí Minh"},
            {"HCM", "Thành phố Hồ Chí Minh"},
            {"TPHCM", "Thành phố Hồ Chí Minh"},
            {"TP.HCM", "Thành phố Hồ Chí Minh"},
            {"Đà Nẵng", "Thành phố Đà Nẵng"},
            {"Hải Phòng", "Thành phố Hải Phòng"},
            {"Cần Thơ", "Thành phố Cần Thơ"},
            {"Nha Trang", "Thành phố Nha Trang"},
            
            // 28 tỉnh
            // Miền Bắc (10 tỉnh)
            {"Quảng Ninh", "Tỉnh Quảng Ninh"},
            {"Lạng Sơn", "Tỉnh Lạng Sơn"},
            {"Cao Bằng", "Tỉnh Cao Bằng"},
            {"Hà Giang", "Tỉnh Hà Giang"},
            {"Tuyên Quang", "Tỉnh Tuyên Quang"},
            {"Thái Nguyên", "Tỉnh Thái Nguyên"},
            {"Bắc Giang", "Tỉnh Bắc Giang"},
            {"Phú Thọ", "Tỉnh Phú Thọ"},
            {"Hòa Bình", "Tỉnh Hòa Bình"},
            {"Sơn La", "Tỉnh Sơn La"},
            {"Lai Châu", "Tỉnh Lai Châu"},
            {"Điện Biên", "Tỉnh Điện Biên"},
            
            // Đồng bằng Bắc Bộ (7 tỉnh)
            {"Bắc Ninh", "Tỉnh Bắc Ninh"},
            {"Hưng Yên", "Tỉnh Hưng Yên"},
            {"Hải Dương", "Tỉnh Hải Dương"},
            {"Thái Bình", "Tỉnh Thái Bình"},
            {"Nam Định", "Tỉnh Nam Định"},
            {"Ninh Bình", "Tỉnh Ninh Bình"},
            {"Thanh Hóa", "Tỉnh Thanh Hóa"},
            
            // Miền Trung (7 tỉnh)
            {"Nghệ An", "Tỉnh Nghệ An"},
            {"Hà Tĩnh", "Tỉnh Hà Tĩnh"},
            {"Quảng Bình", "Tỉnh Quảng Bình"},
            {"Quảng Trị", "Tỉnh Quảng Trị"},
            {"Thừa Thiên Huế", "Tỉnh Thừa Thiên Huế"},
            {"Huế", "Tỉnh Thừa Thiên Huế"},
            {"Quảng Nam", "Tỉnh Quảng Nam"},
            {"Quảng Ngãi", "Tỉnh Quảng Ngãi"},
            {"Bình Định", "Tỉnh Bình Định"},
            {"Phú Yên", "Tỉnh Phú Yên"},
            {"Khánh Hòa", "Tỉnh Khánh Hòa"},
            
            // Tây Nguyên (3 tỉnh)
            {"Gia Lai", "Tỉnh Gia Lai"},
            {"Đắk Lắk", "Tỉnh Đắk Lắk"},
            {"Lâm Đồng", "Tỉnh Lâm Đồng"},
            
            // Đông Nam Bộ (4 tỉnh)
            {"Bình Dương", "Tỉnh Bình Dương"},
            {"Đồng Nai", "Tỉnh Đồng Nai"},
            {"Bà Rịa - Vũng Tàu", "Tỉnh Bà Rịa - Vũng Tàu"},
            {"BR-VT", "Tỉnh Bà Rịa - Vũng Tàu"},
            {"BRVT", "Tỉnh Bà Rịa - Vũng Tàu"},
            {"Tây Ninh", "Tỉnh Tây Ninh"},
            {"Bình Phước", "Tỉnh Bình Phước"},
            
            // Đồng bằng sông Cửu Long (5 tỉnh)
            {"Long An", "Tỉnh Long An"},
            {"Tiền Giang", "Tỉnh Tiền Giang"},
            {"Bến Tre", "Tỉnh Bến Tre"},
            {"Vĩnh Long", "Tỉnh Vĩnh Long"},
            {"Đồng Tháp", "Tỉnh Đồng Tháp"},
            {"An Giang", "Tỉnh An Giang"},
            {"Kiên Giang", "Tỉnh Kiên Giang"},
            //{"Cần Thơ", "Thành phố Cần Thơ"},
            {"Sóc Trăng", "Tỉnh Sóc Trăng"},
            {"Bạc Liêu", "Tỉnh Bạc Liêu"},
            {"Cà Mau", "Tỉnh Cà Mau"}
        };

        // mapping các tỉnh cũ đã sáp nhập vào tỉnh mới (từ 1/7/2025)
        private static readonly Dictionary<string, string> _mergedProvincesMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Các tỉnh sáp nhập vào tỉnh khác
            {"Bắc Kạn", "Tỉnh Cao Bằng"},           // Bắc Kạn sáp nhập vào Cao Bằng
            {"Lào Cai", "Tỉnh Lào Cai - Yên Bái"},    // Lào Cai + Yên Bái
            {"Yên Bái", "Tỉnh Lào Cai - Yên Bái"},
            {"Hà Nam", "Tỉnh Nam Định"},             // Hà Nam sáp nhập vào Nam Định
            {"Vĩnh Phúc", "Tỉnh Phú Thọ"},           // Vĩnh Phúc sáp nhập vào Phú Thọ
            {"Ninh Thuận", "Tỉnh Khánh Hòa"},        // Ninh Thuận sáp nhập vào Khánh Hòa
            {"Bình Thuận", "Tỉnh Bình Thuận - Ninh Thuận"},
            {"Kon Tum", "Tỉnh Gia Lai"},             // Kon Tum sáp nhập vào Gia Lai
            {"Đắk Nông", "Tỉnh Đắk Lắk"},           // Đắk Nông sáp nhập vào Đắk Lắk
            {"Hậu Giang", "Tỉnh Cần Thơ"},           // Hậu Giang sáp nhập vào Cần Thơ
            {"Trà Vinh", "Tỉnh Bến Tre"},            // Trà Vinh sáp nhập vào Bến Tre
            
            // Các thành phố trở thành tỉnh lỵ của tỉnh mới
            {"Buôn Ma Thuột", "Tỉnh Đắk Lắk"},
            {"Pleiku", "Tỉnh Gia Lai"},
            {"Đà Lạt", "Tỉnh Lâm Đồng"},
            {"Vũng Tàu", "Tỉnh Bà Rịa - Vũng Tàu"},
            {"Biên Hòa", "Tỉnh Đồng Nai"},
            {"Thủ Dầu Một", "Tỉnh Bình Dương"},
            {"Rạch Giá", "Tỉnh Kiên Giang"},
            {"Long Xuyên", "Tỉnh An Giang"}
        };

        // Danh sách các từ khóa đơn vị hành chính cấp tỉnh
        private static readonly HashSet<string> _provinceKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "tỉnh", "thành phố", "tp", "thành phố trực thuộc trung ương"
        };

        public static CCCDInfo ParseQRData(string qrData)
        {
            if (string.IsNullOrWhiteSpace(qrData))
                return null;

            try
            {
                var parts = qrData.Split('|');

                if (parts.Length < 7)
                    return null;

                var info = new CCCDInfo();

                info.SoCCCD = parts[0]?.Trim();
                info.HoTen = parts[2]?.Trim();

                // Parse ngày sinh
                if (!string.IsNullOrWhiteSpace(parts[3]))
                {
                    var ngaySinhStr = parts[3].Trim();
                    if (ngaySinhStr.Length == 8)
                    {
                        if (DateTime.TryParseExact(ngaySinhStr, "ddMMyyyy",
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None,
                            out DateTime ngaySinh))
                        {
                            info.NgaySinh = ngaySinh;
                        }
                    }
                }

                info.GioiTinh = parts[4]?.Trim();

                // Xử lý địa chỉ với tỉnh thành mới
                if (!string.IsNullOrWhiteSpace(parts[5]))
                {
                    info.DiaChi = parts[5].Trim();
                    info.TinhThanh = ExtractProvinceFromAddress(info.DiaChi);
                }

                info.NgayCap = parts[6]?.Trim();

                return info;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing QR data: {ex.Message}");
                return null;
            }
        }

        /// Trích xuất tỉnh từ địa chỉ theo danh sách 34 tỉnh thành mới
        private static string ExtractProvinceFromAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return "Không xác định";

            // Chuẩn hóa địa chỉ
            address = NormalizeAddress(address);

            // Tách địa chỉ thành các phần
            var addressParts = address.Split(',')
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToArray();

            if (addressParts.Length == 0)
                return "Không xác định";

            // Chiến lược 1: Tìm trực tiếp trong mapping tỉnh mới
            foreach (var part in addressParts.Reverse())
            {
                var normalizedPart = NormalizeText(part);

                // Kiểm tra trong mapping tỉnh mới
                if (_provinceMapping.ContainsKey(normalizedPart))
                {
                    return _provinceMapping[normalizedPart];
                }
            }

            // Chiến lược 2: Kiểm tra mapping tỉnh cũ đã sáp nhập
            foreach (var part in addressParts.Reverse())
            {
                var normalizedPart = NormalizeText(part);

                if (_mergedProvincesMapping.ContainsKey(normalizedPart))
                {
                    return _mergedProvincesMapping[normalizedPart];
                }
            }

            // Chiến lược 3: Tìm bằng từ khóa tỉnh/thành phố
            foreach (var part in addressParts.Reverse())
            {
                foreach (var keyword in _provinceKeywords)
                {
                    if (part.StartsWith(keyword, StringComparison.OrdinalIgnoreCase) ||
                        part.Contains($" {keyword} ", StringComparison.OrdinalIgnoreCase))
                    {
                        var extracted = ExtractProvinceNameFromPart(part);
                        if (!string.IsNullOrEmpty(extracted))
                            return extracted;
                    }
                }
            }

            // Chiến lược 4: Tìm tên thành phố lớn
            var bigCities = new[] { "Hà Nội", "Hồ Chí Minh", "Đà Nẵng", "Hải Phòng", "Cần Thơ", "Nha Trang" };
            foreach (var part in addressParts.Reverse())
            {
                foreach (var city in bigCities)
                {
                    if (part.Contains(city, StringComparison.OrdinalIgnoreCase))
                    {
                        return _provinceMapping.ContainsKey(city)
                            ? _provinceMapping[city]
                            : $"Thành phố {city}";
                    }
                }
            }

            // Chiến lược 5: Lấy phần cuối cùng và cố gắng chuẩn hóa
            var lastPart = addressParts.Last();
            var normalizedLastPart = NormalizeTinhName(lastPart);

            if (!string.IsNullOrEmpty(normalizedLastPart))
                return normalizedLastPart;

            return lastPart; // Trả về phần cuối nếu không tìm thấy
        }

        /// Trích xuất tên tỉnh từ phần địa chỉ có từ khóa
        private static string ExtractProvinceNameFromPart(string part)
        {
            if (string.IsNullOrWhiteSpace(part))
                return string.Empty;

            // Loại bỏ từ khóa
            foreach (var keyword in _provinceKeywords)
            {
                if (part.StartsWith(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    var name = part.Substring(keyword.Length).Trim();
                    return NormalizeTinhName(name);
                }
            }

            return NormalizeTinhName(part);
        }

        /// Chuẩn hóa tên tỉnh
        public static string NormalizeTinhName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var normalized = NormalizeText(input);

            // Loại bỏ các tiền tố không cần thiết
            normalized = Regex.Replace(normalized,
                @"^(Tỉnh|Thành phố|TP\.?|Thị xã|TX\.?|Huyện|Quận|Phường|Xã|Thị trấn|Thôn|Ấp|Tổ|Khu phố)\s+",
                "",
                RegexOptions.IgnoreCase);

            normalized = normalized.Trim();

            // Kiểm tra trong mapping
            if (_provinceMapping.ContainsKey(normalized))
            {
                return _provinceMapping[normalized];
            }

            // Kiểm tra mapping tỉnh cũ
            if (_mergedProvincesMapping.ContainsKey(normalized))
            {
                return _mergedProvincesMapping[normalized];
            }

            // Kiểm tra các biến thể
            foreach (var mapping in _provinceMapping)
            {
                if (normalized.Contains(mapping.Key, StringComparison.OrdinalIgnoreCase))
                {
                    return mapping.Value;
                }
            }

            // Chuẩn hóa viết hoa
            normalized = System.Globalization.CultureInfo.CurrentCulture.TextInfo
                .ToTitleCase(normalized.ToLower());

            return normalized;
        }

        /// Chuẩn hóa địa chỉ
        private static string NormalizeAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return string.Empty;

            // chuẩn hóa khoảng trắng
            address = Regex.Replace(address, @"\s+", " ").Trim();

            // chuẩn hóa dấu gạch nối
            address = address.Replace(" - ", "-")
                            .Replace(" – ", "-")
                            .Replace(" — ", "-")
                            .Replace(" / ", "/")
                            .Replace(" \\ ", "/");

            // xử lý các trường hợp đặc biệt
            address = address.Replace("TP.", "Thành phố")
                            .Replace("TP ", "Thành phố ")
                            .Replace("Tp.", "Thành phố")
                            .Replace("Tp ", "Thành phố ");

            return address;
        }

        /// Chuẩn hóa văn bản cơ bản
        private static string NormalizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            text = text.Trim();
            text = Regex.Replace(text, @"\s+", " ");

            // Chuẩn hóa viết thường để so sánh
            return text.ToLower();
        }

        /// Validate dữ liệu CCCD
        public static bool ValidateCCCDInfo(CCCDInfo info)
        {
            if (info == null)
                return false;

            // Kiểm tra số CCCD (9 hoặc 12 số)
            if (string.IsNullOrWhiteSpace(info.SoCCCD) ||
                !Regex.IsMatch(info.SoCCCD, @"^\d{9}$|^\d{12}$"))
                return false;

            // Kiểm tra họ tên
            if (string.IsNullOrWhiteSpace(info.HoTen))
                return false;

            // Kiểm tra ngày sinh
            if (!info.NgaySinh.HasValue ||
                info.NgaySinh.Value.Year < 1900 ||
                info.NgaySinh.Value > DateTime.Now)
                return false;

            return true;
        }

        /// Kiểm tra xem tên tỉnh có hợp lệ theo danh sách 34 tỉnh thành mới
        public static bool IsValidProvince(string provinceName)
        {
            if (string.IsNullOrWhiteSpace(provinceName))
                return false;

            var normalized = NormalizeTinhName(provinceName);

            // Kiểm tra trong danh sách giá trị đã chuẩn hóa
            return _provinceMapping.Values.Any(v =>
                v.Equals(normalized, StringComparison.OrdinalIgnoreCase)) ||
                   _mergedProvincesMapping.Values.Any(v =>
                v.Equals(normalized, StringComparison.OrdinalIgnoreCase));
        }

        /// Lấy danh sách 34 tỉnh/thành phố hợp lệ (từ 1/7/2025)
        public static List<string> GetValidProvinces()
        {
            var provinces = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var value in _provinceMapping.Values)
            {
                provinces.Add(value);
            }

            foreach (var value in _mergedProvincesMapping.Values)
            {
                provinces.Add(value);
            }

            return provinces.OrderBy(p => p).ToList();
        }

        /// Chuyển đổi địa chỉ cũ sang địa chỉ mới theo tỉnh thành mới
        public static string ConvertOldAddressToNew(string oldAddress)
        {
            if (string.IsNullOrWhiteSpace(oldAddress))
                return oldAddress;

            var province = ExtractProvinceFromAddress(oldAddress);
            var addressParts = oldAddress.Split(',')
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToArray();

            if (addressParts.Length > 1)
            {
                // Giữ phần địa chỉ chi tiết, thay thế phần tỉnh
                var detailParts = addressParts.Take(addressParts.Length - 1);
                return $"{string.Join(", ", detailParts)}, {province}";
            }

            return province;
        }
    }
}
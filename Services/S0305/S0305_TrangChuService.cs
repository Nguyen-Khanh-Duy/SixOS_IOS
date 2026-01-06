using SixOSDatKhamAppMobile.Configurations;
using SixOSDatKhamAppMobile.Helpers;
using SixOSDatKhamAppMobile.Models;
using System.Net.Http.Json;

namespace SixOSDatKhamAppMobile.Services.S0305
{
    public class S0305_TrangChuService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = S0305_ApiConfig.BaseUrl;

        public S0305_TrangChuService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(S0305_ApiConfig.DefaultTimeoutSeconds);
        }

        public async Task<ApiResponse<List<ChuyenGiaTrangChuDTO>>> LayNgauNhienChuyenGiaAsync(int soLuong = 3)
        {
            try
            {
                var token = await S0305_SecureStorage.GetTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync(
                    $"{_baseUrl}/TrangChu/lay-ngau-nhien-chuyen-gia?soLuong={soLuong}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponseWrapper<List<ChuyenGiaTrangChuDTO>>>();
                    return new ApiResponse<List<ChuyenGiaTrangChuDTO>>
                    {
                        Success = result.StatusCode == 200,
                        Message = result.Message,
                        Data = result.Data
                    };
                }

                return new ApiResponse<List<ChuyenGiaTrangChuDTO>>
                {
                    Success = false,
                    Message = "Không thể kết nối đến server"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<ChuyenGiaTrangChuDTO>>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }

        /// Lấy gói khám ngẫu nhiên theo độ tuổi và giới tính
        public async Task<ApiResponse<List<GoiKhamTrangChuDTO>>> LayGoiKhamTheoTuoiGioiTinhAsync(
            int tuoi = 1,
            long idGioiTinh = 0,
            int soLuong = 3)
        {
            try
            {
                var token = await S0305_SecureStorage.GetTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync(
                    $"{_baseUrl}/TrangChu/lay-goi-kham-theo-tuoi-gioi-tinh?tuoi={tuoi}&idGioiTinh={idGioiTinh}&soLuong={soLuong}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponseWrapper<List<GoiKhamTrangChuDTO>>>();
                    return new ApiResponse<List<GoiKhamTrangChuDTO>>
                    {
                        Success = result.StatusCode == 200,
                        Message = result.Message,
                        Data = result.Data
                    };
                }

                return new ApiResponse<List<GoiKhamTrangChuDTO>>
                {
                    Success = false,
                    Message = "Không thể kết nối đến server"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<GoiKhamTrangChuDTO>>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }

        // Giữ lại method cũ để tương thích ngược
        public async Task<ApiResponse<List<GoiKhamTrangChuDTO>>> LayNgauNhienGoiKhamAsync(int soLuong = 3)
        {
            try
            {
                var token = await S0305_SecureStorage.GetTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync(
                    $"{_baseUrl}/TrangChu/lay-ngau-nhien-goi-kham?soLuong={soLuong}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponseWrapper<List<GoiKhamTrangChuDTO>>>();
                    return new ApiResponse<List<GoiKhamTrangChuDTO>>
                    {
                        Success = result.StatusCode == 200,
                        Message = result.Message,
                        Data = result.Data
                    };
                }

                return new ApiResponse<List<GoiKhamTrangChuDTO>>
                {
                    Success = false,
                    Message = "Không thể kết nối đến server"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<GoiKhamTrangChuDTO>>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }
    }

    #region DTOs
    public class ChuyenGiaTrangChuDTO
    {
        public long ID { get; set; }
        public string HocVi { get; set; }
        public string TenChuyenGia { get; set; }
        public string ChucDanh { get; set; }
        public string MoTaNgan { get; set; }
        public string DuongDanHinh { get; set; }
    }

    public class GoiKhamTrangChuDTO
    {
        public long Id { get; set; }
        public string TenGoi { get; set; }
        public string TenGoiHienThi
        {
            get
            {
                if (string.IsNullOrWhiteSpace(TenGoi))
                    return string.Empty;

                int index = TenGoi.IndexOf("[");
                return index > 0
                    ? TenGoi.Substring(0, index).Trim()
                    : TenGoi.Trim();
            }
        }
        public string MoTa { get; set; }
        public int? TuoiMin { get; set; }
        public int? TuoiMax { get; set; }
        public decimal? TongTien { get; set; }
        public int? IDGioiTinh { get; set; }
        public bool GoiCoBan { get; set; }
        public bool GoiNangCao { get; set; }
        public string HuongDan { get; set; }
        public List<GoiKhamChiTietTrangChuDTO> ChiTiet { get; set; } = new();
    }

    public class GoiKhamChiTietTrangChuDTO
    {
        public long Id { get; set; }
        public int? Stt { get; set; }
        public int? SoLuong { get; set; }
        public decimal? DonGiaDichVu { get; set; }
        public string TenDichVu { get; set; }
        public decimal ThanhTien { get; set; }
    }
    #endregion
}
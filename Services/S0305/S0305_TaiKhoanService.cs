using System.Text.Json;
using SixOSDatKhamAppMobile.Configurations;
using SixOSDatKhamAppMobile.Models;

namespace SixOSDatKhamAppMobile.Services.S0305
{
    public class S0305_TaiKhoanService
    {
        private readonly string _baseUrl = S0305_ApiConfig.BaseUrl;
        private readonly HttpClient _httpClient;

        public S0305_TaiKhoanService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(S0305_ApiConfig.DefaultTimeoutSeconds);

            // Cấu hình headers mặc định
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<TaiKhoanDTO> LayThongTinTaiKhoanHienTai()
        {
            try
            {
                // lấy token từ SecureStorage
                var token = await S0305_SecureStorage.GetTokenAsync();

                if (string.IsNullOrEmpty(token))
                {
                    throw new Exception("Người dùng chưa đăng nhập");
                }

                // thêm token vào header
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync($"{_baseUrl}/TaiKhoan/thong-tin-hien-tai");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<TaiKhoanDTO>>(
                        content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    if (apiResponse?.Success == true)
                    {
                        return apiResponse.Data;
                    }
                    else
                    {
                        throw new Exception(apiResponse?.Message ?? "Không thể lấy thông tin tài khoản");
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new Exception("Phiên đăng nhập đã hết hạn");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Lỗi khi gọi API: {response.StatusCode} - {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Lỗi kết nối: {ex.Message}");
            }
            catch (TaskCanceledException)
            {
                throw new Exception("Yêu cầu bị quá thời gian");
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi: {ex.Message}");
            }
        }

        public async Task<TaiKhoanDTO> LayThongTinTaiKhoanBangCCCD(string cccd)
        {
            try
            {
                var token = await S0305_SecureStorage.GetTokenAsync();

                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.GetAsync($"{_baseUrl}/api/TaiKhoan/cccd/{cccd}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<TaiKhoanDTO>>(
                        content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    if (apiResponse?.Success == true)
                    {
                        return apiResponse.Data;
                    }
                    else
                    {
                        throw new Exception(apiResponse?.Message ?? "Không thể lấy thông tin tài khoản");
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Lỗi khi gọi API: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi: {ex.Message}");
            }
        }
    }

    // Model cho thông tin tài khoản
    public class TaiKhoanDTO
    {
        public long IdTK { get; set; }
        public string HoTen { get; set; }
        public string DienThoai { get; set; } = null!;

    }
}
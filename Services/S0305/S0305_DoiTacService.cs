using SixOSDatKhamAppMobile.Configurations;
using SixOSDatKhamAppMobile.Models;
using SixOSDatKhamAppMobile.Models.Response;
using SixOSDatKhamAppMobile.Pages;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SixOSDatKhamAppMobile.Services.S0305
{
    public class S0305_DoiTacService
    {
        private readonly string _baseUrl = S0305_ApiConfig.BaseUrl;
        private readonly HttpClient _httpClient;

        public S0305_DoiTacService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(S0305_ApiConfig.DefaultTimeoutSeconds);
        }

        public async Task<bool> SetDoiTacAsync(long idDoiTac)
        {
            try
            {
                var refreshToken = await S0305_SecureStorage.GetRefreshTokenAsync();
                if (string.IsNullOrEmpty(refreshToken))
                    return false;
                var accessToken = await S0305_SecureStorage.GetTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                if (string.IsNullOrEmpty(accessToken))
                {
                    return false;
                }
                var requestBody = new
                {
                    idDoiTac = idDoiTac,
                    refreshToken = refreshToken
                };

                var response = await _httpClient.PostAsJsonAsync(
                    $"{_baseUrl}/Auth/SetDoiTac",
                    requestBody
                );

                if (!response.IsSuccessStatusCode)
                    return false;

                var result = await response.Content.ReadFromJsonAsync<SetDoiTacResponseDTO>();

                if (result != null && result.Success)
                {
                    // Lưu lại token mới
                    await S0305_SecureStorage.SaveTokenAsync(result.AccessToken);
                    await S0305_SecureStorage.SaveRefreshTokenAsync(result.RefreshToken);
                    await S0305_SecureStorage.SaveIdDoiTacAsync(idDoiTac.ToString());

                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
        public async Task<BenhVien> GetBenhVienAsync()
        {
            try
            {
                var token = await S0305_SecureStorage.GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    return new BenhVien
                    {
                        Success = false,
                        Message = "Chưa đăng nhập"
                    };
                }

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync($"{_baseUrl}/DoiTac");

                if (!response.IsSuccessStatusCode)
                {
                    return new BenhVien
                    {
                        Success = false,
                        Message = $"Lỗi API: {response.StatusCode}"
                    };
                }

                var content = await response.Content.ReadAsStringAsync();

                var data = JsonSerializer.Deserialize<BenhVien>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (data == null)
                {
                    return new BenhVien
                    {
                        Success = false,
                        Message = "Không đọc được dữ liệu từ server"
                    };
                }
                data.Success = true;
                return data;
            }
            catch (Exception ex)
            {
                return new BenhVien
                {
                    Success = false,
                    Message = "Lỗi: " + ex.Message
                };
            }
        }
    }

    public class SetDoiTacResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public long IdDoiTac { get; set; }
    }
}

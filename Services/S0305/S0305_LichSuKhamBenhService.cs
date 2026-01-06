using SixOSDatKhamAppMobile.Configurations;
using SixOSDatKhamAppMobile.Models.LichSuKhamBenh;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace SixOSDatKhamAppMobile.Services.S0305
{
    public class S0305_LichSuKhamBenhService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = S0305_ApiConfig.BaseUrl;

        public S0305_LichSuKhamBenhService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(S0305_ApiConfig.DefaultTimeoutSeconds);
        }

        public async Task<SendOtpResponse> SendOTPAsync(SendOtpRequest request)
        {
            try
            {
                var url = $"{_baseUrl}/LichSuKhamBenh/SendOTP";
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonSerializer.Deserialize<SendOtpResponse>(responseContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }

                return new SendOtpResponse
                {
                    StatusCode = (int)response.StatusCode,
                    Message = "Gửi OTP thất bại"
                };
            }
            catch (Exception ex)
            {
                return new SendOtpResponse
                {
                    StatusCode = 500,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }

        public async Task<VerifyOtpResponse> VerifyOTPAsync(VerifyOtpRequest request)
        {
            try
            {
                var url = $"{_baseUrl}/LichSuKhamBenh/VerifyOTP";
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<VerifyOtpResponse>(responseContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (result?.StatusCode == 200 && !string.IsNullOrEmpty(result.Token))
                    {
                        await S0305_SecureStorage.SaveTokenAsync(result.Token);
                    }

                    return result;
                }

                return new VerifyOtpResponse
                {
                    StatusCode = (int)response.StatusCode,
                    Message = "Xác thực OTP thất bại"
                };
            }
            catch (Exception ex)
            {
                return new VerifyOtpResponse
                {
                    StatusCode = 500,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }

        public async Task<List<DotDieuTriModel>> GetDotDieuTriAsync()
        {
            try
            {
                var token = await S0305_SecureStorage.GetTokenAsync();

                if (string.IsNullOrEmpty(token))
                {
                    return null;
                }

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var url = $"{_baseUrl}/LichSuKhamBenh/GetDotDieuTri";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<DotDieuTriModel>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }

                return new List<DotDieuTriModel>();
            }
            catch (Exception ex)
            {
                return new List<DotDieuTriModel>();
            }
        }

        public async Task<List<FileKhamBenhModel>> GetFilesTheoDotDieuTriAsync(long idVv, string ngay)
        {
            try
            {
                var token = await S0305_SecureStorage.GetTokenAsync();

                if (string.IsNullOrEmpty(token))
                {
                    return null;
                }

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var url = $"{_baseUrl}/LichSuKhamBenh/GetFilesTheoDotDieuTri?idVV={idVv}&ngay={ngay}";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<FileKhamBenhModel>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }

                return new List<FileKhamBenhModel>();
            }
            catch (Exception ex)
            {
                return new List<FileKhamBenhModel>();
            }
        }

        public async Task<byte[]> GetFileAsync(long id)
        {
            try
            {
                var token = await S0305_SecureStorage.GetTokenAsync();

                if (string.IsNullOrEmpty(token))
                {
                    return null;
                }

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var url = $"{_baseUrl}/LichSuKhamBenh/GetFile/{id}";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
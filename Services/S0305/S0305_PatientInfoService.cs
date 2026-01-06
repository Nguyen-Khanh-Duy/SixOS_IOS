using SixOSDatKhamAppMobile.Configurations;
using SixOSDatKhamAppMobile.Models;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SixOSDatKhamAppMobile.Services.S0305
{
    public class S0305_PatientInfoService
    {
        private readonly string _baseUrl = S0305_ApiConfig.BaseUrl;
        private readonly HttpClient _httpClient;

        public S0305_PatientInfoService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(S0305_ApiConfig.DefaultTimeoutSeconds);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<bool> CheckPatientInfoExistsAsync(long userId)
        {
            try
            {
                // Lấy token từ SecureStorage
                var token = await S0305_SecureStorage.GetTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.GetAsync($"{_baseUrl}/Patient/check-info?userId={userId}");

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<CheckPatientInfoResponse>(
                        responseString,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return result?.HasPatientInfo ?? false;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<long?> LayIdBNAsync()
        {
            try
            {
                var userId = await S0305_SecureStorage.GetUserIdAsync();
                if (string.IsNullOrEmpty(userId))
                {
                    return null;
                }

                var token = await S0305_SecureStorage.GetTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.GetAsync($"{_baseUrl}/Patient/lay-id-bn?userId={userId}");

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    if (long.TryParse(responseString, out long idBN))
                    {
                        return idBN;
                    }
                }

                return null;
            } catch
            {
                return null;
            }
        }

        public async Task<CheckGoiKhamBNResponse> CheckGoiKhamBNAsync(long idBN)
        {
            try
            {
                var token = await S0305_SecureStorage.GetTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.GetAsync($"{_baseUrl}/Patient/check-goi-kham?idBN={idBN}");

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<CheckGoiKhamBNResponse>(
                        responseString,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return result ?? new CheckGoiKhamBNResponse
                    {
                        HasGoiKham = false,
                        Message = "Không thể xử lý phản hồi từ server"
                    };
                }
                else
                {
                    return new CheckGoiKhamBNResponse
                    {
                        HasGoiKham = false,
                        Message = $"Lỗi {response.StatusCode}: Không thể kiểm tra gói khám"
                    };
                }
            }
            catch (Exception ex)
            {
                return new CheckGoiKhamBNResponse
                {
                    HasGoiKham = false,
                    Message = $"Lỗi kết nối: {ex.Message}"
                };
            }
        }

        public async Task<SavePatientInfoResponse> SavePatientInfoAsync(PatientInfoRequest request, string flag = "LuuHoSo")
        {
            try
            {
                // Lấy token từ SecureStorage
                var token = await S0305_SecureStorage.GetTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var jsonContent = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                dynamic response;
                if (flag == "LuuHoSo") { 
                    response = await _httpClient.PostAsync($"{_baseUrl}/Patient/save-info", content);   
                } else response = await _httpClient.PostAsync($"{_baseUrl}/Patient/save-info/edit", content);

                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<SavePatientInfoResponse>(
                        responseString,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return result ?? new SavePatientInfoResponse
                    {
                        Success = false,
                        Message = "Không thể xử lý phản hồi từ server"
                    };
                }
                else
                {
                    return JsonSerializer.Deserialize<SavePatientInfoResponse>(responseString,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                        ?? new SavePatientInfoResponse
                        {
                            Success = false,
                            Message = $"Lỗi {response.StatusCode}: Không thể lưu thông tin"
                        };
                }
            }
            catch (Exception ex)
            {
                return new SavePatientInfoResponse
                {
                    Success = false,
                    Message = $"Lỗi kết nối: {ex.Message}"
                };
            }
        }
    }

    public class CheckPatientInfoResponse
    {
        public bool HasPatientInfo { get; set; }
        public string Message { get; set; }
    }

    public class SavePatientInfoResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public long? PatientId { get; set; }
        public long? DemTK {  get; set; }
    }
}
using Microsoft.Maui.Storage;

namespace SixOSDatKhamAppMobile.Services
{
    public static class S0305_SecureStorage
    {
        private const string TokenKey = "auth_token";
        private const string RefreshTokenKey = "refresh_token";
        private const string UserIdKey = "user_id";
        private const string UserCccdKey = "user_cccd";
        private const string UserPhoneKey = "user_phone";
        private const string IdDoiTacKey = "id_doi_tac";
        private const string DaQuaTrangChuKey = "da_qua_trang_chu";

        public static async Task SaveTokenAsync(string token)
        {
            await Microsoft.Maui.Storage.SecureStorage.SetAsync(TokenKey, token);
        }

        public static async Task SaveRefreshTokenAsync(string refreshToken)
        {
            await Microsoft.Maui.Storage.SecureStorage.SetAsync(RefreshTokenKey, refreshToken);
        }

        public static async Task SaveUserIdAsync(string userId)
        {
            await Microsoft.Maui.Storage.SecureStorage.SetAsync(UserIdKey, userId);
        }

        public static async Task SaveUserCCCDAsync(string cccd)
        {
            await Microsoft.Maui.Storage.SecureStorage.SetAsync(UserCccdKey, cccd);
        }

        public static async Task SaveUserPhoneAsync(string phone)
        {
            await Microsoft.Maui.Storage.SecureStorage.SetAsync(UserPhoneKey, phone);
        }

        public static async Task SaveIdDoiTacAsync(string idDoiTac)
        {
            await Microsoft.Maui.Storage.SecureStorage.SetAsync(IdDoiTacKey, idDoiTac);
        }

        public static async Task SaveDaQuaTrangChuAsync(bool daQuaTrangChu)
        {
            await Microsoft.Maui.Storage.SecureStorage.SetAsync(DaQuaTrangChuKey, daQuaTrangChu.ToString());
        }

        public static async Task<string> GetTokenAsync()
        {
            try
            {
                return await Microsoft.Maui.Storage.SecureStorage.GetAsync(TokenKey);
            }
            catch
            {
                return null;
            }
        }

        public static async Task<string> GetRefreshTokenAsync()
        {
            try
            {
                return await Microsoft.Maui.Storage.SecureStorage.GetAsync(RefreshTokenKey);
            }
            catch
            {
                return null;
            }
        }

        public static async Task<string> GetUserIdAsync()
        {
            try
            {
                return await Microsoft.Maui.Storage.SecureStorage.GetAsync(UserIdKey);
            }
            catch
            {
                return null;
            }
        }

        public static async Task<string> GetUserCCCDAsync()
        {
            try
            {
                return await Microsoft.Maui.Storage.SecureStorage.GetAsync(UserCccdKey);
            }
            catch
            {
                return null;
            }
        }

        public static async Task<string> GetUserPhoneAsync()
        {
            try
            {
                return await Microsoft.Maui.Storage.SecureStorage.GetAsync(UserPhoneKey);
            }
            catch
            {
                return null;
            }
        }

        public static async Task<string> GetIdDoiTacAsync()
        {
            try
            {
                return await Microsoft.Maui.Storage.SecureStorage.GetAsync(IdDoiTacKey);
            }
            catch
            {
                return null;
            }
        }

        public static async Task<string> GetDaQuaTrangChuAsync()
        {
            try
            {
                return await Microsoft.Maui.Storage.SecureStorage.GetAsync(DaQuaTrangChuKey);
            }
            catch
            {
                return null;
            }
        }

        public static void ClearAllData()
        {
            try
            {
                Preferences.Remove(TokenKey);
                Preferences.Remove(RefreshTokenKey);
                Preferences.Remove(UserIdKey);
                Preferences.Remove(UserPhoneKey);
                Preferences.Remove(UserCccdKey);
                Preferences.Remove(IdDoiTacKey);
                Preferences.Remove(DaQuaTrangChuKey);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing storage: {ex.Message}");
            }
        }

        public static async Task<bool> IsUserLoggedInAsync()
        {
            var token = await GetTokenAsync();
            return !string.IsNullOrEmpty(token);
        }
    }
}
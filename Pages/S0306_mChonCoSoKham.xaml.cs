using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using SixOSDatKhamAppMobile.Services;
using SixOSDatKhamAppMobile.Services.S0305;
using System;
using System.Threading.Tasks;

namespace SixOSDatKhamAppMobile.Pages
{
    public partial class S0306_mChonCoSoKham : ContentPage
    {
        private Color colorSelected = Color.FromArgb("#E8F0FE");
        private Color colorNormal = Colors.White;

        private string coSoDuocChonId = "";
        private string tenCoSoDuocChon = "";

        private readonly S0305_DoiTacService _doiTacService;
        private readonly S0305_PatientInfoService _patientInfoService;

        #region Constructors
        public S0306_mChonCoSoKham()
        {
            InitializeComponent();

            _doiTacService = new S0305_DoiTacService();
            _patientInfoService = new S0305_PatientInfoService();

            ResetAllCoSo();
            SetupSafeArea();
        }
        private void SetupSafeArea()
        {
            // Xử lý cho Android
#if ANDROID
    var mainDisplayInfo = DeviceDisplay.Current.MainDisplayInfo;
    var density = mainDisplayInfo.Density;
    
    // Tính toán status bar height
    var statusBarHeight = density > 0 ? density * 15 : 15;

            // Tính toán navigation bar height (khoảng 48-56dp)
            var bottomPadding = density > 0 ? density * 15 : 15;

            // Set padding cho cả trên và dưới
            this.BackgroundColor = Colors.Black;
    this.Padding = new Thickness(0, statusBarHeight, 0, bottomPadding);
    
    // Đảm bảo content bắt đầu sau status bar
    if (Content is Layout layout)
    {
        layout.BackgroundColor = Colors.White;
    }
#endif

            // Xử lý cho iOS
#if IOS
    // Sử dụng SafeAreaInsets để lấy chính xác hơn
    var topPadding = 40; // hoặc tính toán dựa trên SafeAreaInsets
    var bottomPadding = 34; // cho iPhone có Home indicator
    
    this.BackgroundColor = Colors.White;
    this.Padding = new Thickness(0, topPadding, 0, bottomPadding);
#endif
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            // Đảm bảo status bar có màu trắng
            if (Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.GetUseSafeArea(this))
            {
                // Sử dụng safe area
                On<iOS>().SetUseSafeArea(true);
            }
            var idBN = await _patientInfoService.LayIdBNAsync();
            await S0305_SecureStorage.SaveUserIdAsync((idBN ?? 0).ToString());
        }

        #endregion

        #region Chọn cơ sở
        private async void OnChonCoSo1Clicked(object sender, EventArgs e)
        {
            await ChonCoSo("1");
        }

        private async void OnChonCoSo2Clicked(object sender, EventArgs e)
        {
            await ChonCoSo("2");
        }

        private async Task ChonCoSo(string coSoId)
        {
            if (coSoDuocChonId == coSoId) return;

            ResetAllCoSo();
            coSoDuocChonId = coSoId;

            if (coSoId == "1")
            {
                frameCoSo1.BackgroundColor = colorSelected;
                frameSelected1.IsVisible = true;
                btnChonCoSo1.IsVisible = false;
                tenCoSoDuocChon = "Bệnh viện Ung Bướu TP.HCM - Cơ Sở 1";
            }
            else if (coSoId == "2")
            {
                frameCoSo2.BackgroundColor = colorSelected;
                frameSelected2.IsVisible = true;
                btnChonCoSo2.IsVisible = false;
                tenCoSoDuocChon = "Bệnh viện Ung Bướu TP.HCM - Cơ Sở 2";
            }

            // Tự động lưu và chuyển trang sau khi chọn
            await XuLyChonCoSo();
        }

        private void ResetAllCoSo()
        {
            // Reset cơ sở 1
            frameCoSo1.BackgroundColor = colorNormal;
            frameSelected1.IsVisible = false;
            btnChonCoSo1.IsVisible = true;

            // Reset cơ sở 2
            frameCoSo2.BackgroundColor = colorNormal;
            frameSelected2.IsVisible = false;
            btnChonCoSo2.IsVisible = true;
        }
        #endregion

        #region Xử lý chọn cơ sở
        private async Task XuLyChonCoSo()
        {
            if (string.IsNullOrEmpty(coSoDuocChonId))
                return;

            bool ok = await _doiTacService.SetDoiTacAsync(long.Parse(coSoDuocChonId));
            if (!ok)
            {
                await DisplayAlert("Lỗi", "Không thể lưu cơ sở khám", "OK");
                ResetAllCoSo();
                coSoDuocChonId = "";
                return;
            }

            var idBNStr = await S0305_SecureStorage.GetUserIdAsync();
            if (string.IsNullOrEmpty(idBNStr) || !long.TryParse(idBNStr, out long idBN))
            {
                await DisplayAlert("Lỗi", "Không tìm thấy hồ sơ bệnh nhân", "OK");
                ResetAllCoSo();
                coSoDuocChonId = "";
                return;
            }

            var checkResult = await _patientInfoService.CheckGoiKhamBNAsync(idBN);
            if (checkResult == null)
            {
                await DisplayAlert("Lỗi", "Không thể kiểm tra gói khám", "OK");
                ResetAllCoSo();
                coSoDuocChonId = "";
                return;
            }

            // Chuyển trang dựa trên kết quả kiểm tra gói khám
            if (checkResult.HasGoiKham)
                await Navigation.PushAsync(new S0306_mTrangChu());
            else
                await Navigation.PushAsync(new S0306_mKhamTheoGoi());
        }
        #endregion

        #region Navigation
        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
        #endregion
    }
}
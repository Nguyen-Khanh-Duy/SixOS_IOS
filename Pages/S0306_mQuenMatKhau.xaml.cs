using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using SixOSDatKhamAppMobile.Services.S0305;
using System.Text.RegularExpressions;

namespace SixOSDatKhamAppMobile.Pages
{
    public partial class S0306_mQuenMatKhau : ContentPage
    {
        private readonly S0305_ForgotPasswordService _forgotPasswordService;
        private string _currentCccd;
        private string _currentPhone;
        private int _wrongPhoneAttempts = 0;
        private string _lastCheckedCccd = "";

        public S0306_mQuenMatKhau()
        {
            InitializeComponent();
            _forgotPasswordService = new S0305_ForgotPasswordService();

            CccdEntry.TextChanged += OnCccdTextChanged;
            SetupSafeArea();
            // Subscribe to OTP popup events
            if (OtpPopup != null)
            {
                OtpPopup.OtpVerified += OnOtpVerified;
                OtpPopup.OtpCancelled += OnOtpCancelled;
                OtpPopup.ResendOtpRequested += OnResendOtpRequested;
            }
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
        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.GetUseSafeArea(this))
            {
                // Sử dụng safe area
                On<iOS>().SetUseSafeArea(true);
            }
        }
        // Xử lý khi CCCD thay đổi
        private void OnCccdTextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.NewTextValue))
            {
                HintContainer.IsVisible = false;
                PhoneEntry.Placeholder = "Nhập số điện thoại";
                _wrongPhoneAttempts = 0;
                _lastCheckedCccd = "";
            }
            else if (_lastCheckedCccd != e.NewTextValue)
            {
                HintContainer.IsVisible = false;
                PhoneEntry.Placeholder = "Nhập số điện thoại";
                _wrongPhoneAttempts = 0;
            }
        }

        private void ShowPhoneHint(string phoneHint)
        {
            if (!string.IsNullOrEmpty(phoneHint))
            {
                HintContainer.IsVisible = true;
                HintLabel.Text = $"💡 Gợi ý: SĐT đã đăng ký có đuôi {phoneHint}";
                PhoneEntry.Text = "";
                PhoneEntry.Placeholder = $"Nhập số có đuôi {phoneHint}";

                // Animation
                AnimateHintContainer();
            }
        }

        private async void AnimateHintContainer()
        {
            await HintContainer.ScaleTo(1.05, 200, Easing.CubicOut);
            await HintContainer.ScaleTo(1.0, 200, Easing.CubicIn);
        }

        private async void OnContinueClicked(object sender, EventArgs e)
        {
            string cccd = CccdEntry.Text?.Trim();
            string phone = PhoneEntry.Text?.Trim();

            // Validate input
            if (string.IsNullOrWhiteSpace(cccd))
            {
                await DisplayAlert("Lỗi", "Vui lòng nhập số CCCD", "OK");
                CccdEntry.Focus();
                return;
            }

            if (cccd.Length < 9 || cccd.Length > 12)
            {
                await DisplayAlert("Lỗi", "Số CCCD phải từ 9 đến 12 ký tự", "OK");
                CccdEntry.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(phone))
            {
                await DisplayAlert("Lỗi", "Vui lòng nhập số điện thoại", "OK");
                PhoneEntry.Focus();
                return;
            }

            // Kiểm tra số điện thoại hợp lệ (10-11 số)
            phone = Regex.Replace(phone, @"[^\d]", ""); // Chỉ giữ số
            if (phone.Length < 10 || phone.Length > 11)
            {
                await DisplayAlert("Lỗi", "Số điện thoại không hợp lệ", "OK");
                PhoneEntry.Focus();
                return;
            }

            // Show loading
            ContinueButton.IsEnabled = false;
            ContinueButton.Text = "Đang xử lý...";

            try
            {
                // Gọi API gửi OTP
                var result = await _forgotPasswordService.SendForgotPasswordOtpAsync(cccd, phone);

                if (result.Success)
                {
                    // Lưu thông tin để dùng sau
                    _currentCccd = cccd;
                    _currentPhone = phone;

                    // Vô hiệu hóa form chính
                    CccdEntry.IsEnabled = false;
                    PhoneEntry.IsEnabled = false;

                    // Hiển thị OTP popup với đúng API (2 tham số)
                    if (OtpPopup != null)
                    {
                        await OtpPopup.ShowAsync(phone, 60);
                    }
                }
                else
                {
                    // Tracking attempts
                    if (_lastCheckedCccd == cccd)
                    {
                        _wrongPhoneAttempts++;
                    }
                    else
                    {
                        _wrongPhoneAttempts = 1;
                        _lastCheckedCccd = cccd;
                    }

                    // Hiển thị hint nếu BE trả về
                    if (!string.IsNullOrEmpty(result.PhoneHint))
                    {
                        ShowPhoneHint(result.PhoneHint);
                    }

                    // Thông báo lỗi
                    string errorMessage = _wrongPhoneAttempts == 1
                        ? result.Message
                        : $"{result.Message} (Lần {_wrongPhoneAttempts})";

                    await DisplayAlert("Lỗi", errorMessage, "OK");
                    PhoneEntry.Focus();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", $"Có lỗi xảy ra: {ex.Message}", "OK");
            }
            finally
            {
                ContinueButton.IsEnabled = true;
                ContinueButton.Text = "TIẾP TỤC";
            }
        }

        private async void OnOtpVerified(object sender, string otp)
        {
            try
            {
                if (OtpPopup != null)
                {
                    // Có thể thêm indicator loading vào OTP popup nếu cần
                }

                // Verify OTP với backend
                var verifyResult = await _forgotPasswordService.VerifyForgotPasswordOtpAsync(
                    _currentCccd,
                    _currentPhone,
                    otp);

                if (verifyResult.Success)
                {
                    // OTP đúng, ẩn popup và chuyển trang
                    if (OtpPopup != null)
                    {
                        await OtpPopup.HideAsync();
                    }

                    var resetPage = new S0306_mDatLaiMatKhau(_currentCccd, _currentPhone, otp);
                    await Navigation.PushAsync(resetPage);
                }
                else
                {
                    // OTP sai, hiển thị lỗi và reset OTP fields
                    await DisplayAlert("Lỗi", verifyResult.Message, "OK");

                    if (OtpPopup != null)
                    {
                        OtpPopup.ResetOtpFields();
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", $"Có lỗi xảy ra: {ex.Message}", "OK");
            }
        }

        private async void OnOtpCancelled(object sender, EventArgs e)
        {
            // User hủy nhập OTP, bật lại form
            if (OtpPopup != null)
            {
                await OtpPopup.HideAsync();
            }

            CccdEntry.IsEnabled = true;
            PhoneEntry.IsEnabled = true;
        }

        private async void OnResendOtpRequested(object sender, EventArgs e)
        {
            // Gửi lại OTP
            try
            {
                var result = await _forgotPasswordService.SendForgotPasswordOtpAsync(_currentCccd, _currentPhone);

                if (result.Success)
                {
                    // Không cần hiển thị alert nữa vì OTP component đã tự reset timer
                    // Component sẽ tự động reset timer khi ResendOtpRequested được gọi
                }
                else
                {
                    await DisplayAlert("Lỗi", result.Message, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", $"Không thể gửi lại OTP: {ex.Message}", "OK");
            }
        }

        private async void OnCallSupportClicked(object sender, EventArgs e)
        {
            bool call = await DisplayAlert("Hỗ trợ", "Gọi đến tổng đài hỗ trợ: 028 3841 2637?", "Gọi", "Hủy");

            if (call)
            {
                try
                {
                    if (PhoneDialer.Default.IsSupported)
                    {
                        PhoneDialer.Default.Open("02838412637");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Lỗi", $"Không thể thực hiện cuộc gọi: {ex.Message}", "OK");
                }
            }
        }

        private async void OnBackToLoginClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        protected override bool OnBackButtonPressed()
        {
            // Nếu OTP popup đang hiển thị, ẩn nó đi
            if (OtpPopup != null && IsOtpPopupVisible())
            {
                OnOtpCancelled(this, EventArgs.Empty);
                return true;
            }

            Navigation.PopAsync();
            return true;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Cleanup OTP popup
            if (OtpPopup != null)
            {
                OtpPopup.Cleanup();
                OtpPopup.OtpVerified -= OnOtpVerified;
                OtpPopup.OtpCancelled -= OnOtpCancelled;
                OtpPopup.ResendOtpRequested -= OnResendOtpRequested;
            }
        }

        // Helper method để kiểm tra OTP popup có đang hiển thị không
        private bool IsOtpPopupVisible()
        {
            // Kiểm tra thông qua visual state (nếu component không có property IsVisible)
            // Có thể cần thêm property public IsVisible trong component OTP
            return OtpPopup?.IsVisible == true;
        }
    }
}
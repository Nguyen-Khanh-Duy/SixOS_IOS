using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using SixOSDatKhamAppMobile.Services.S0305;
using System.Text.RegularExpressions;

namespace SixOSDatKhamAppMobile.Pages
{
    public partial class S0306_mTaoTaiKhoan : ContentPage
    {
        private bool _isPasswordVisible = false;
        private bool _isConfirmPasswordVisible = false;
        private readonly S0305_AuthService _authService;
        private string _currentCccd = "";
        private string _currentPhone = "";
        private string _currentPassword = "";

        public S0306_mTaoTaiKhoan()
        {
            InitializeComponent();
            _authService = new S0305_AuthService();

            CccdEntry.TextChanged += OnFieldTextChanged;
            PhoneEntry.TextChanged += OnFieldTextChanged;
            PasswordEntry.TextChanged += OnPasswordTextChanged;
            ConfirmPasswordEntry.TextChanged += OnConfirmPasswordTextChanged;
            TermsCheckBox.CheckedChanged += OnTermsCheckedChanged;
            SetupSafeArea();
            // Subscribe to OTP popup events
            if (OtpPopup != null)
            {
                OtpPopup.OtpVerified += OnOtpVerified;
                OtpPopup.OtpCancelled += OnOtpCancelled;
                OtpPopup.ResendOtpRequested += OnResendOtpRequested;
            }
        }

        // ================ OTP EVENT HANDLERS ================
        private async void OnOtpVerified(object sender, string otp)
        {
            if (OtpPopup != null)
            {
                await OtpPopup.HideAsync();
            }

            // Xác thực OTP và đăng ký
            await VerifyOtpAndRegister(otp);
        }

        private async void OnOtpCancelled(object sender, EventArgs e)
        {
            if (OtpPopup != null)
            {
                await OtpPopup.HideAsync();
            }

            // Bật lại form đăng ký
            EnableRegistrationForm(true);
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
        private async void OnResendOtpRequested(object sender, EventArgs e)
        {
            // Gửi lại OTP
            try
            {
                var result = await _authService.SendOtpAsync(_currentCccd, _currentPhone);

                if (!result.Success)
                {
                    await DisplayAlert("Lỗi", result.Message, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", $"Không thể gửi lại OTP: {ex.Message}", "OK");
            }
        }

        // ================ FORM VALIDATION ================
        private void OnFieldTextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateForm();
        }

        private void OnPasswordTextChanged(object sender, TextChangedEventArgs e)
        {
            string password = e.NewTextValue;

            if (string.IsNullOrEmpty(password))
            {
                PasswordHintLabel.IsVisible = false;
            }
            else if (password.Length < 6)
            {
                PasswordHintLabel.IsVisible = true;
                PasswordHintLabel.Text = $"Cần thêm {6 - password.Length} ký tự nữa";
                PasswordHintLabel.TextColor = Color.FromArgb("#F44336");
            }
            else
            {
                PasswordHintLabel.IsVisible = true;
                PasswordHintLabel.Text = "✓ Mật khẩu đủ độ dài";
                PasswordHintLabel.TextColor = Color.FromArgb("#4CAF50");
            }

            CheckPasswordConfirmation();
            ValidateForm();
        }

        private void OnConfirmPasswordTextChanged(object sender, TextChangedEventArgs e)
        {
            CheckPasswordConfirmation();
            ValidateForm();
        }

        private void CheckPasswordConfirmation()
        {
            string password = PasswordEntry.Text;
            string confirmPassword = ConfirmPasswordEntry.Text;

            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                ValidationLabel.IsVisible = false;
                return;
            }

            if (password == confirmPassword)
            {
                ValidationLabel.Text = "✓ Mật khẩu khớp";
                ValidationLabel.TextColor = Color.FromArgb("#4CAF50");
                ValidationLabel.IsVisible = true;
            }
            else
            {
                ValidationLabel.Text = "✗ Mật khẩu không khớp";
                ValidationLabel.TextColor = Color.FromArgb("#F44336");
                ValidationLabel.IsVisible = true;
            }
        }

        private void OnTermsCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            ValidateForm();
        }

        private void ValidateForm()
        {
            string cccd = CccdEntry.Text?.Trim();
            string phone = PhoneEntry.Text?.Trim();
            string password = PasswordEntry.Text;
            string confirmPassword = ConfirmPasswordEntry.Text;
            bool termsAccepted = TermsCheckBox.IsChecked;

            bool isCccdValid = !string.IsNullOrEmpty(cccd) && cccd.Length == 12 && cccd.All(char.IsDigit);
            bool isPhoneValid = !string.IsNullOrEmpty(phone) && IsValidPhoneNumber(phone);
            bool isPasswordValid = !string.IsNullOrEmpty(password) && password.Length >= 6;
            bool isPasswordMatch = password == confirmPassword;
            bool isConfirmNotEmpty = !string.IsNullOrEmpty(confirmPassword);

            RegisterButton.IsEnabled = isCccdValid && isPhoneValid &&
                                      isPasswordValid && isPasswordMatch && isConfirmNotEmpty &&
                                      termsAccepted;
        }

        // ================ REGISTRATION LOGIC ================
        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            string cccd = CccdEntry.Text?.Trim();
            string phone = PhoneEntry.Text?.Trim();
            string password = PasswordEntry.Text;
            string confirmPassword = ConfirmPasswordEntry.Text;

            // Validation
            if (string.IsNullOrEmpty(cccd))
            {
                await DisplayAlert("Lỗi", "Vui lòng nhập số CCCD", "OK");
                CccdEntry.Focus();
                return;
            }

            if (cccd.Length != 12 || !cccd.All(char.IsDigit))
            {
                await DisplayAlert("Lỗi", "Số CCCD phải có đúng 12 chữ số", "OK");
                CccdEntry.Focus();
                return;
            }

            if (string.IsNullOrEmpty(phone))
            {
                await DisplayAlert("Lỗi", "Vui lòng nhập số điện thoại", "OK");
                PhoneEntry.Focus();
                return;
            }

            if (!IsValidPhoneNumber(phone))
            {
                await DisplayAlert("Lỗi", "Số điện thoại không hợp lệ", "OK");
                PhoneEntry.Focus();
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                await DisplayAlert("Lỗi", "Vui lòng nhập mật khẩu", "OK");
                PasswordEntry.Focus();
                return;
            }

            if (password.Length < 6)
            {
                await DisplayAlert("Lỗi", "Mật khẩu phải có tối thiểu 6 ký tự", "OK");
                PasswordEntry.Focus();
                return;
            }

            if (password != confirmPassword)
            {
                await DisplayAlert("Lỗi", "Mật khẩu xác nhận không khớp", "OK");
                ConfirmPasswordEntry.Text = "";
                ConfirmPasswordEntry.Focus();
                return;
            }

            if (!TermsCheckBox.IsChecked)
            {
                await DisplayAlert("Lỗi", "Vui lòng đồng ý với điều khoản sử dụng", "OK");
                return;
            }

            // Lưu thông tin
            _currentCccd = cccd;
            _currentPhone = NormalizePhoneNumber(phone);
            _currentPassword = password;

            // Hiển thị loading
            RegisterButton.IsEnabled = false;
            RegisterButton.Text = "ĐANG GỬI OTP...";

            // Gửi OTP
            var result = await _authService.SendOtpAsync(_currentCccd, _currentPhone);

            if (result.Success)
            {
                // Vô hiệu hóa form đăng ký
                EnableRegistrationForm(false);

                // Hiển thị OTP popup
                if (OtpPopup != null)
                {
                    await OtpPopup.ShowAsync(_currentPhone, 60);
                }
            }
            else
            {
                await DisplayAlert("Lỗi", result.Message, "OK");
                RegisterButton.IsEnabled = true;
                RegisterButton.Text = "TẠO TÀI KHOẢN";
            }
        }

        private async Task VerifyOtpAndRegister(string otp)
        {
            RegisterButton.Text = "ĐANG ĐĂNG KÝ...";

            var registerResult = await _authService.VerifyOtpAndRegisterAsync(
                _currentCccd,
                _currentPhone,
                otp,
                _currentPassword
            );

            if (registerResult.Success)
            {
                await DisplayAlert(
                    "Thành công",
                    "Đăng ký tài khoản thành công!\n\n" +
                    $"Số CCCD: {_currentCccd}\n" +
                    $"SĐT: {_currentPhone}\n\n" +
                    "Bạn có thể đăng nhập ngay bây giờ.",
                    "OK"
                );

                // Quay lại trang login
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Lỗi", registerResult.Message, "OK");
                EnableRegistrationForm(true);

                // Reset OTP fields nếu nhập sai
                OtpPopup?.ResetOtpFields();
            }
        }

        // ================ HELPER METHODS ================
        private void EnableRegistrationForm(bool enable)
        {
            CccdEntry.IsEnabled = enable;
            PhoneEntry.IsEnabled = enable;
            PasswordEntry.IsEnabled = enable;
            ConfirmPasswordEntry.IsEnabled = enable;
            TermsCheckBox.IsEnabled = enable;
            RegisterButton.IsEnabled = enable;
            RegisterButton.Text = "TẠO TÀI KHOẢN";
        }

        private bool IsValidPhoneNumber(string phone)
        {
            if (string.IsNullOrEmpty(phone))
                return false;

            string digitsOnly = Regex.Replace(phone, @"[^\d]", "");
            return digitsOnly.Length >= 10 && digitsOnly.Length <= 11 && digitsOnly.StartsWith("0");
        }

        private string NormalizePhoneNumber(string phone)
        {
            if (string.IsNullOrEmpty(phone))
                return phone;

            phone = Regex.Replace(phone, @"[^\d]", "");

            if (phone.StartsWith("84"))
            {
                phone = "0" + phone.Substring(2);
            }

            if (!phone.StartsWith("0") && phone.Length > 0)
            {
                phone = "0" + phone;
            }

            return phone;
        }

        // ================ UI EVENT HANDLERS ================
        private void OnTogglePasswordClicked(object sender, EventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;
            PasswordEntry.IsPassword = !_isPasswordVisible;
            TogglePasswordButton.Source = _isPasswordVisible ? "dong.png" : "mo.png";
        }

        private void OnToggleConfirmPasswordClicked(object sender, EventArgs e)
        {
            _isConfirmPasswordVisible = !_isConfirmPasswordVisible;
            ConfirmPasswordEntry.IsPassword = !_isConfirmPasswordVisible;
            ToggleConfirmPasswordButton.Source = _isConfirmPasswordVisible ? "dong.png" : "mo.png";
        }

        private async void OnTermsClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Điều khoản sử dụng",
                "ĐIỀU KHOẢN SỬ DỤNG ỨNG DỤNG\n\n" +
                "1. Thông tin cá nhân của bạn sẽ được bảo mật và chỉ sử dụng cho mục đích đăng ký khám bệnh.\n\n" +
                "2. Bạn cam kết cung cấp thông tin chính xác và trung thực.\n\n" +
                "3. Ứng dụng không thu thập thông tin không cần thiết cho dịch vụ.\n\n" +
                "4. Bạn có trách nhiệm bảo mật thông tin tài khoản của mình.\n\n" +
                "5. Bệnh viện có quyền từ chối dịch vụ nếu phát hiện thông tin giả mạo.",
                "Tôi hiểu");
        }

        private async void OnGoToLoginClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnCallSupportClicked(object sender, EventArgs e)
        {
            bool call = await DisplayAlert("Hỗ trợ",
                "Gọi đến tổng đài hỗ trợ: 028 3841 2637?",
                "Gọi", "Hủy");

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

        private async void OnEmailSupportClicked(object sender, EventArgs e)
        {
            bool sendEmail = await DisplayAlert("Email hỗ trợ",
                "Gửi email đến hotro@benhvienungbuou.vn?",
                "Mở Email", "Hủy");

            if (sendEmail)
            {
                try
                {
                    if (Email.Default.IsComposeSupported)
                    {
                        var message = new EmailMessage
                        {
                            Subject = "Yêu cầu hỗ trợ đăng ký tài khoản",
                            Body = $"Xin chào,\n\nTôi cần hỗ trợ về việc đăng ký tài khoản trên ứng dụng.\n\nThông tin của tôi:\n- Số điện thoại: {PhoneEntry.Text}\n\nNội dung cần hỗ trợ:",
                            To = new List<string> { "hotro@benhvienungbuou.vn" }
                        };

                        await Email.Default.ComposeAsync(message);
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Lỗi", $"Không thể mở ứng dụng email: {ex.Message}", "OK");
                }
            }
        }

        // ================ LIFECYCLE METHODS ================
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

        private bool IsOtpPopupVisible()
        {
            // Kiểm tra OTP popup có đang hiển thị không
            // Có thể cần thêm property IsVisible trong component OTP
            return OtpPopup?.IsVisible == true;
        }
    }
}
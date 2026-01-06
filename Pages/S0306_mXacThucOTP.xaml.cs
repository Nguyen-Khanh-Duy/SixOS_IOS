using System.Text;
using System.Timers;

namespace SixOSDatKhamAppMobile.Pages
{
    public partial class S0306_mXacThucOTP : ContentView
    {
        private int _otpCountdown = 60;
        private System.Timers.Timer _otpTimer;
        private string _phoneNumber;

        // Events để parent page lắng nghe
        public event EventHandler<string> OtpVerified;
        public event EventHandler OtpCancelled;
        public event EventHandler ResendOtpRequested;

        // Property để kiểm tra popup có đang hiển thị không
        public bool IsVisible => OtpFormContainer.IsVisible;

        public S0306_mXacThucOTP()
        {
            InitializeComponent();
        }

        // Public method để show popup từ parent page
        public async Task ShowAsync(string phoneNumber, int countdownSeconds = 60)
        {
            _phoneNumber = phoneNumber;
            _otpCountdown = countdownSeconds;

            // Hiển thị số điện thoại (ẩn một phần)
            string maskedPhone = MaskPhoneNumber(phoneNumber);
            OtpPhoneInfo.Text = $"Mã OTP 4 số đã gửi đến: {maskedPhone}";

            // Hiển thị overlay mờ
            Overlay.IsVisible = true;
            await Overlay.FadeTo(1, 300, Easing.CubicIn);

            // Hiển thị form OTP với animation
            OtpFormContainer.IsVisible = true;

            // Animation: scale và fade in
            await Task.WhenAll(
                OtpFormContainer.ScaleTo(1, 400, Easing.SpringOut),
                OtpFormContainer.FadeTo(1, 300, Easing.CubicIn)
            );

            // Khởi động timer OTP
            StartOtpTimer();

            // Focus vào ô OTP đầu tiên
            await Task.Delay(100);
            OtpDigit1.Focus();
        }

        // Public method để hide popup
        public async Task HideAsync()
        {
            // Ẩn form OTP với animation
            await Task.WhenAll(
                OtpFormContainer.ScaleTo(0.9, 300, Easing.SpringIn),
                OtpFormContainer.FadeTo(0, 250, Easing.CubicOut)
            );

            OtpFormContainer.IsVisible = false;

            // Ẩn overlay
            await Overlay.FadeTo(0, 250, Easing.CubicOut);
            Overlay.IsVisible = false;

            // Dừng timer
            StopOtpTimer();

            // Xóa OTP
            ClearOtpFields();
        }

        private void StartOtpTimer()
        {
            OtpTimerLabel.Text = $"Mã OTP có hiệu lực trong: {_otpCountdown}s";
            ResendOtpButton.IsEnabled = false;

            _otpTimer = new System.Timers.Timer(1000);
            _otpTimer.Elapsed += OnOtpTimerElapsed;
            _otpTimer.Start();
        }

        private void StopOtpTimer()
        {
            if (_otpTimer != null)
            {
                _otpTimer.Stop();
                _otpTimer.Dispose();
                _otpTimer = null;
            }
        }

        private void OnOtpTimerElapsed(object sender, ElapsedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _otpCountdown--;

                if (_otpCountdown > 0)
                {
                    OtpTimerLabel.Text = $"Mã OTP có hiệu lực trong: {_otpCountdown}s";
                    ResendOtpButton.IsEnabled = false;
                }
                else
                {
                    OtpTimerLabel.Text = "Mã OTP đã hết hạn";
                    ResendOtpButton.IsEnabled = true;
                    VerifyOtpButton.IsEnabled = false;
                    _otpTimer?.Stop();
                }
            });
        }

        private async void OnResendOtpClicked(object sender, EventArgs e)
        {
            // Trigger event để parent xử lý việc gửi lại OTP
            ResendOtpRequested?.Invoke(this, EventArgs.Empty);

            // Reset timer
            _otpCountdown = 60;
            OtpTimerLabel.Text = $"Mã OTP có hiệu lực trong: {_otpCountdown}s";
            ResendOtpButton.IsEnabled = false;
            VerifyOtpButton.IsEnabled = false;

            // Khởi động lại timer
            if (_otpTimer != null)
            {
                _otpTimer.Stop();
                _otpTimer.Start();
            }

            // Clear OTP fields
            ClearOtpFields();
            OtpDigit1.Focus();
        }

        private void OnOtpDigitChanged(object sender, TextChangedEventArgs e)
        {
            var entry = sender as Entry;

            if (entry == null) return;

            // Chỉ cho phép nhập số
            if (!string.IsNullOrEmpty(e.NewTextValue) && !char.IsDigit(e.NewTextValue[0]))
            {
                entry.Text = e.OldTextValue;
                return;
            }

            // Tự động chuyển focus đến ô tiếp theo khi nhập đủ 1 số
            if (!string.IsNullOrEmpty(e.NewTextValue) && e.NewTextValue.Length == 1)
            {
                MoveToNextOtpBox(entry);
            }
            // Xóa số thì quay lại ô trước
            else if (string.IsNullOrEmpty(e.NewTextValue) && !string.IsNullOrEmpty(e.OldTextValue))
            {
                MoveToPreviousOtpBox(entry);
            }

            // Highlight ô đang focus
            UpdateOtpBoxBorder(entry);

            // Kiểm tra xem đã nhập đủ 4 số chưa
            CheckOtpComplete();
        }

        private void MoveToNextOtpBox(Entry currentEntry)
        {
            Entry nextEntry = null;

            if (currentEntry == OtpDigit1) nextEntry = OtpDigit2;
            else if (currentEntry == OtpDigit2) nextEntry = OtpDigit3;
            else if (currentEntry == OtpDigit3) nextEntry = OtpDigit4;

            if (nextEntry != null)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    nextEntry.Focus();
                    UpdateOtpBoxBorder(nextEntry);
                });
            }
        }

        private void MoveToPreviousOtpBox(Entry currentEntry)
        {
            Entry previousEntry = null;

            if (currentEntry == OtpDigit2) previousEntry = OtpDigit1;
            else if (currentEntry == OtpDigit3) previousEntry = OtpDigit2;
            else if (currentEntry == OtpDigit4) previousEntry = OtpDigit3;

            if (previousEntry != null)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    previousEntry.Focus();
                    UpdateOtpBoxBorder(previousEntry);
                });
            }
        }

        private void UpdateOtpBoxBorder(Entry focusedEntry)
        {
            // Reset tất cả borders về màu xám
            OtpBox1.Stroke = Color.FromArgb("#E0E0E0");
            OtpBox1.StrokeThickness = 1;

            OtpBox2.Stroke = Color.FromArgb("#E0E0E0");
            OtpBox2.StrokeThickness = 1;

            OtpBox3.Stroke = Color.FromArgb("#E0E0E0");
            OtpBox3.StrokeThickness = 1;

            OtpBox4.Stroke = Color.FromArgb("#E0E0E0");
            OtpBox4.StrokeThickness = 1;

            // Set border màu xanh cho ô đang focus
            if (focusedEntry == OtpDigit1)
            {
                OtpBox1.Stroke = Color.FromArgb("#007bff");
                OtpBox1.StrokeThickness = 2;
            }
            else if (focusedEntry == OtpDigit2)
            {
                OtpBox2.Stroke = Color.FromArgb("#007bff");
                OtpBox2.StrokeThickness = 2;
            }
            else if (focusedEntry == OtpDigit3)
            {
                OtpBox3.Stroke = Color.FromArgb("#007bff");
                OtpBox3.StrokeThickness = 2;
            }
            else if (focusedEntry == OtpDigit4)
            {
                OtpBox4.Stroke = Color.FromArgb("#007bff");
                OtpBox4.StrokeThickness = 2;
            }
        }

        private void CheckOtpComplete()
        {
            string enteredOtp = GetEnteredOtp();
            VerifyOtpButton.IsEnabled = enteredOtp.Length == 4;
        }

        private string GetEnteredOtp()
        {
            StringBuilder otpBuilder = new StringBuilder();

            if (!string.IsNullOrEmpty(OtpDigit1.Text)) otpBuilder.Append(OtpDigit1.Text);
            if (!string.IsNullOrEmpty(OtpDigit2.Text)) otpBuilder.Append(OtpDigit2.Text);
            if (!string.IsNullOrEmpty(OtpDigit3.Text)) otpBuilder.Append(OtpDigit3.Text);
            if (!string.IsNullOrEmpty(OtpDigit4.Text)) otpBuilder.Append(OtpDigit4.Text);

            return otpBuilder.ToString();
        }

        private void ClearOtpFields()
        {
            OtpDigit1.Text = "";
            OtpDigit2.Text = "";
            OtpDigit3.Text = "";
            OtpDigit4.Text = "";

            VerifyOtpButton.IsEnabled = false;

            // Reset borders
            UpdateOtpBoxBorder(OtpDigit1);
        }

        private void OnVerifyOtpClicked(object sender, EventArgs e)
        {
            string enteredOtp = GetEnteredOtp();

            if (enteredOtp.Length != 4)
            {
                return;
            }

            // Trigger event để parent xử lý
            OtpVerified?.Invoke(this, enteredOtp);
        }

        private async void OnHideOtpForm(object sender, EventArgs e)
        {
            OtpCancelled?.Invoke(this, EventArgs.Empty);
            await HideAsync();
        }

        private string MaskPhoneNumber(string phone)
        {
            if (string.IsNullOrEmpty(phone) || phone.Length < 7)
                return phone;

            return phone.Substring(0, 4) + "***" + phone.Substring(phone.Length - 3);
        }

        // Public method để cleanup khi page dispose
        public void Cleanup()
        {
            StopOtpTimer();
        }

        // Public method để reset OTP fields khi nhập sai
        public void ResetOtpFields()
        {
            ClearOtpFields();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                OtpDigit1.Focus();
            });
        }
    }
}
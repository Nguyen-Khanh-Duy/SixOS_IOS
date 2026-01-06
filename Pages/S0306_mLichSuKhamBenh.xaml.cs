using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Controls.Shapes;
using SixOSDatKhamAppMobile.Models.LichSuKhamBenh;
using SixOSDatKhamAppMobile.Services;
using SixOSDatKhamAppMobile.Services.S0305;

namespace SixOSDatKhamAppMobile.Pages
{
    public partial class S0306_mLichSuKhamBenh : ContentPage
    {
        private readonly S0305_LichSuKhamBenhService _lichSuService;
        private List<DotDieuTriModel> _dotDieuTriList;
        private DotDieuTriModel _selectedDotDieuTri;

        public S0306_mLichSuKhamBenh()
        {
            InitializeComponent();
            _lichSuService = new S0305_LichSuKhamBenhService();
            SetupSafeArea();
            // Đăng ký event handlers cho OTP popup
            var otpPopup = this.FindByName<S0306_mXacThucOTP>("OtpPopup");
            if (otpPopup != null)
            {
                otpPopup.OtpVerified += OnOtpVerified;
                otpPopup.OtpCancelled += OnOtpCancelled;
                otpPopup.ResendOtpRequested += OnResendOtpRequested;
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
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.GetUseSafeArea(this))
            {
                // Sử dụng safe area
                On<iOS>().SetUseSafeArea(true);
            }
            var token = await S0305_SecureStorage.GetTokenAsync();

            if (string.IsNullOrEmpty(token))
            {
                // Chưa xác thực, hiển thị popup OTP
                await ShowOtpVerification();
            }
            else
            {
                // Đã xác thực, load dữ liệu
                await LoadDotDieuTri();
            }
        }

        private async Task ShowOtpVerification()
        {
            // Lấy thông tin từ preferences hoặc form đăng nhập trước đó
            var cccd = Preferences.Get("TempCccd", "");
            var dienThoai = Preferences.Get("TempDienThoai", "");

            if (string.IsNullOrEmpty(cccd) || string.IsNullOrEmpty(dienThoai))
            {
                await DisplayAlert("Thông báo",
                    "Vui lòng đăng nhập trước khi xem lịch sử khám bệnh", "OK");
                await Navigation.PopAsync();
                return;
            }

            // Gửi OTP
            var sendResult = await _lichSuService.SendOTPAsync(new SendOtpRequest
            {
                SoCccd = cccd,
                DienThoai = dienThoai
            });

            if (sendResult?.StatusCode != 200)
            {
                await DisplayAlert("Lỗi", sendResult?.Message ?? "Gửi OTP thất bại", "OK");
                await Navigation.PopAsync();
                return;
            }

            // Hiển thị popup OTP
            var otpPopup = this.FindByName<S0306_mXacThucOTP>("OtpPopup");
            await otpPopup.ShowAsync(dienThoai, 60);
        }

        private async void OnOtpVerified(object sender, string otp)
        {
            var cccd = Preferences.Get("TempCccd", "");
            var dienThoai = Preferences.Get("TempDienThoai", "");

            var verifyResult = await _lichSuService.VerifyOTPAsync(new VerifyOtpRequest
            {
                Cccd = cccd,
                DienThoai = dienThoai,
                Code = otp
            });

            if (verifyResult?.StatusCode == 200)
            {
                var otpPopup = sender as S0306_mXacThucOTP;
                await otpPopup.HideAsync();

                await DisplayAlert("Thành công", "Xác thực thành công", "OK");

                // Load dữ liệu
                await LoadDotDieuTri();
            }
            else
            {
                await DisplayAlert("Lỗi", verifyResult?.Message ?? "Xác thực thất bại", "OK");

                var otpPopup = sender as S0306_mXacThucOTP;
                otpPopup?.ResetOtpFields();
            }
        }

        private async void OnOtpCancelled(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnResendOtpRequested(object sender, EventArgs e)
        {
            var cccd = Preferences.Get("TempCccd", "");
            var dienThoai = Preferences.Get("TempDienThoai", "");

            var result = await _lichSuService.SendOTPAsync(new SendOtpRequest
            {
                SoCccd = cccd,
                DienThoai = dienThoai
            });

            if (result?.StatusCode == 200)
            {
                await DisplayAlert("Thành công", "Đã gửi lại mã OTP", "OK");
            }
            else
            {
                await DisplayAlert("Lỗi", result?.Message ?? "Gửi lại OTP thất bại", "OK");
            }
        }

        private async Task LoadDotDieuTri()
        {
            try
            {
                _dotDieuTriList = await _lichSuService.GetDotDieuTriAsync();

                if (_dotDieuTriList == null || !_dotDieuTriList.Any())
                {
                    await DisplayAlert("Thông báo",
                        "Không có lịch sử khám bệnh", "OK");
                    return;
                }

                RenderDotDieuTri();

                // Tự động chọn đợt đầu tiên
                if (_dotDieuTriList.Any())
                {
                    await LoadFilesForDot(_dotDieuTriList.First());
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi",
                    $"Không thể tải dữ liệu: {ex.Message}", "OK");
            }
        }

        private void RenderDotDieuTri()
        {
            DateListContainer.Children.Clear();

            foreach (var dot in _dotDieuTriList)
            {
                var border = new Border
                {
                    Stroke = Color.FromArgb("#E2E8F0"),
                    BackgroundColor = Colors.White,
                    Padding = new Thickness(15, 12),
                    StrokeThickness = 1,
                    AutomationId = $"{dot.IdVv}_{dot.NgayVao}"
                };

                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += async (s, e) => await OnDateItemTapped(s, e, dot);
                border.GestureRecognizers.Add(tapGesture);

                var stackLayout = new VerticalStackLayout { Spacing = 5 };

                var ngayDate = DateTime.ParseExact(dot.NgayVao, "dd-MM-yyyy", null);
                var dayOfWeek = ngayDate.ToString("dddd",
                    new System.Globalization.CultureInfo("vi-VN"));

                // Màu sắc cho text
                var dayOfWeekColor = Color.FromArgb("#64748B"); // Màu xám cho thứ
                var dateColor = dot.CoBatThuong ? Color.FromArgb("#EF4444") : Color.FromArgb("#333333");

                stackLayout.Children.Add(new Label
                {
                    Text = char.ToUpper(dayOfWeek[0]) + dayOfWeek.Substring(1),
                    FontSize = 14,
                    TextColor = dayOfWeekColor
                });

                stackLayout.Children.Add(new Label
                {
                    Text = dot.NgayVao,
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = dateColor
                });

                border.Content = stackLayout;
                DateListContainer.Children.Add(border);
            }
        }

        private async Task OnDateItemTapped(object sender, EventArgs e, DotDieuTriModel dot)
        {
            var border = sender as Border;
            if (border == null) return;

            ResetAllDateItems();

            // Khi được chọn: thêm viền xanh đậm và background xanh nhạt
            border.Stroke = Color.FromArgb("#1A73E8");
            border.StrokeThickness = 2;
            border.BackgroundColor = Color.FromArgb("#F0F9FF");

            var stackLayout = border.Content as VerticalStackLayout;
            if (stackLayout != null && stackLayout.Children.Count >= 2)
            {
                var dateLabel = stackLayout.Children[1] as Label;
                if (dateLabel != null)
                {
                    // Khi selected: giữ nguyên màu đỏ nếu bất thường, đổi sang xanh đậm nếu bình thường
                    dateLabel.TextColor = dot.CoBatThuong ?
                        Color.FromArgb("#EF4444") : // Giữ đỏ nếu bất thường
                        Color.FromArgb("#1A73E8");   // Xanh đậm khi selected
                }
            }

            await LoadFilesForDot(dot);
        }

        private void ResetAllDateItems()
        {
            foreach (var child in DateListContainer.Children)
            {
                if (child is Border border)
                {
                    // Reset về trạng thái mặc định
                    border.Stroke = Color.FromArgb("#E2E8F0");
                    border.StrokeThickness = 1;
                    border.BackgroundColor = Colors.White;

                    var stackLayout = border.Content as VerticalStackLayout;
                    if (stackLayout != null && stackLayout.Children.Count >= 2)
                    {
                        var dateLabel = stackLayout.Children[1] as Label;
                        if (dateLabel != null)
                        {
                            var dot = _dotDieuTriList.FirstOrDefault(d =>
                                $"{d.IdVv}_{d.NgayVao}" == border.AutomationId);

                            // Reset màu: đỏ nếu bất thường, đen nếu bình thường
                            dateLabel.TextColor = dot?.CoBatThuong == true ?
                                Color.FromArgb("#EF4444") :
                                Color.FromArgb("#333333");
                        }
                    }
                }
            }
        }

        private async Task LoadFilesForDot(DotDieuTriModel dot)
        {
            _selectedDotDieuTri = dot;

            var files = await _lichSuService.GetFilesTheoDotDieuTriAsync(dot.IdVv, dot.NgayVao);

            if (files == null || !files.Any())
            {
                PdfFilesContainer.IsVisible = false;
                NoFilesContainer.IsVisible = true;
                return;
            }

            RenderFileList(files);
            PdfFilesContainer.IsVisible = true;
            NoFilesContainer.IsVisible = false;
        }

        private void RenderFileList(List<FileKhamBenhModel> files)
        {
            PdfFilesContainer.Children.Clear();

            foreach (var file in files)
            {
                var border = new Border
                {
                    Stroke = Color.FromArgb("#E2E8F0"),
                    StrokeThickness = 1,
                    BackgroundColor = Colors.White,
                    Padding = new Thickness(12),
                    Margin = new Thickness(0, 6),
                    StrokeShape = new RoundRectangle { CornerRadius = 12 }
                };

                // Thêm viền đỏ nếu file bất thường
                if (file.BatThuong == true)
                {
                    border.Stroke = Color.FromArgb("#EF4444");
                    border.StrokeThickness = 2;
                    border.BackgroundColor = Color.FromArgb("#FEF2F2");
                }

                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += async (s, e) => await OnPdfFileTapped(s, e, file);
                border.GestureRecognizers.Add(tapGesture);

                var grid = new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = GridLength.Auto },
                        new ColumnDefinition { Width = GridLength.Star },
                        new ColumnDefinition { Width = GridLength.Auto }
                    },
                    ColumnSpacing = 12
                };

                // Icon PDF - đổi màu đỏ nếu bất thường
                var iconBorder = new Border
                {
                    WidthRequest = 44,
                    HeightRequest = 44,
                    BackgroundColor = file.BatThuong == true ?
                        Color.FromArgb("#EF4444") :
                        Color.FromArgb("#E53935"),
                    StrokeShape = new RoundRectangle { CornerRadius = 8 }
                };

                iconBorder.Content = new Label
                {
                    Text = "PDF",
                    FontSize = 9,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.White,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };

                Grid.SetColumn(iconBorder, 0);
                grid.Children.Add(iconBorder);

                // Thông tin file
                var infoStack = new VerticalStackLayout
                {
                    Spacing = 2,
                    VerticalOptions = LayoutOptions.Center
                };

                // Tên file - đổi màu đỏ nếu bất thường
                var tenFileLabel = new Label
                {
                    Text = file.TenHienThi,
                    FontSize = 15,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = file.BatThuong == true ?
                        Color.FromArgb("#EF4444") :
                        Color.FromArgb("#1F2937"),
                    LineBreakMode = LineBreakMode.TailTruncation
                };

                infoStack.Children.Add(tenFileLabel);

                // Loại file
                var loaiFileStack = new HorizontalStackLayout
                {
                    Spacing = 4,
                    VerticalOptions = LayoutOptions.Center
                };

                loaiFileStack.Children.Add(new Label
                {
                    Text = "PDF",
                    FontSize = 12,
                    TextColor = Color.FromArgb("#6B7280")
                });

                infoStack.Children.Add(loaiFileStack);

                Grid.SetColumn(infoStack, 1);
                grid.Children.Add(infoStack);

                // Icon mở
                var arrowImage = new Image
                {
                    Source = "ic_arrow_right.png",
                    WidthRequest = 20,
                    HeightRequest = 20,
                    VerticalOptions = LayoutOptions.Center
                };

                Grid.SetColumn(arrowImage, 2);
                grid.Children.Add(arrowImage);

                border.Content = grid;
                PdfFilesContainer.Children.Add(border);
            }
        }

        private async Task OnPdfFileTapped(object sender, EventArgs e, FileKhamBenhModel file)
        {
            try
            {
                var pdfData = await _lichSuService.GetFileAsync(file.Id);

                if (pdfData == null || pdfData.Length == 0)
                {
                    await DisplayAlert("Lỗi", "Không thể tải file PDF", "OK");
                    return;
                }

                var fileName = $"{file.TenHienThi}.pdf";
                var filePath = System.IO.Path.Combine(FileSystem.CacheDirectory, fileName);

                await File.WriteAllBytesAsync(filePath, pdfData);

                // Mở bằng ứng dụng mặc định
                await Launcher.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(filePath)
                });
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", $"Không thể mở file: {ex.Message}", "OK");
            }
        }

        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        #region Bottom Navigation
        private async void OnTrangChuClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new S0306_mTrangChu());
        }

        private async void OnHoSoClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new S0306_mHoSoBenhNhan());
        }

        private async void OnLichHenClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new S0306_mLichSuDatHen());
        }

        private async void OnPhieuKhamClicked(object sender, EventArgs e)
        {
            // Already on this page
        }

        private async void OnTaiKhoanClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new S0306_mTaiKhoan());
        }
        #endregion
    }
}
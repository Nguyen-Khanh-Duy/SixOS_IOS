using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Controls.Shapes;
using SixOSDatKhamAppMobile.Models;
using SixOSDatKhamAppMobile.Services;
using SixOSDatKhamAppMobile.Services.S0305;

namespace SixOSDatKhamAppMobile.Pages
{
    public partial class S0306_mLichSuDatHen : ContentPage
    {
        private DateTime _selectedDate;
        private readonly S0305_LichSuDatHenService _lichSuService;
        private readonly S0305_DKGoiKhamService _dKGoiKhamService;
        private LichSuKhamBenhResponse _currentData;
        private bool _isFirstLoad = true; // Thêm flag để kiểm soát lần load đầu tiên

        public S0306_mLichSuDatHen()
        {
            InitializeComponent();
            _lichSuService = new S0305_LichSuDatHenService();
            _dKGoiKhamService = new S0305_DKGoiKhamService();

            // KHÔNG gán giá trị cho datePicker ở đây
            // Sẽ gán sau khi có ngày từ API
            SetupSafeArea();
            BindingContext = this;
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

            System.Diagnostics.Debug.WriteLine($"OnAppearing called - _isFirstLoad: {_isFirstLoad}");

            // Đảm bảo status bar có màu trắng
            if (Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.GetUseSafeArea(this))
            {
                // Sử dụng safe area
                On<iOS>().SetUseSafeArea(true);
            }

            // Chỉ load dữ liệu lần đầu tiên
            if (_isFirstLoad)
            {
                System.Diagnostics.Debug.WriteLine("First load - fetching data");

                // Lấy ngày đặt hẹn gần nhất
                var ngayDatHenGanNhat = await _lichSuService.LayNgayDatHenGanNhatAsync();
                _selectedDate = ngayDatHenGanNhat.ToDateTime(TimeOnly.MinValue);

                System.Diagnostics.Debug.WriteLine($"Date from API: {_selectedDate:dd-MM-yyyy}");

                // Cập nhật DatePicker TRƯỚC KHI set _isFirstLoad = false
                if (datePicker != null)
                {
                    datePicker.Date = _selectedDate;
                }

                // Set flag sau khi đã set datePicker để OnDateSelected không trigger
                _isFirstLoad = false;

                // Chỉ gọi API 1 lần duy nhất
                System.Diagnostics.Debug.WriteLine("Loading appointments...");
                await LoadAppointmentsByDate(_selectedDate);
            }
        }

        private void OnDateSelected(object sender, DateChangedEventArgs e)
        {
            // Bỏ qua nếu là lần đầu tiên (khi set Date programmatically)
            if (_isFirstLoad)
                return;

            _selectedDate = e.NewDate;
            System.Diagnostics.Debug.WriteLine($"OnDateSelected triggered: {e.NewDate:dd-MM-yyyy}");
            _ = LoadAppointmentsByDate(_selectedDate);
        }

        private void OnCalendarButtonClicked(object sender, EventArgs e)
        {
            if (datePicker != null)
            {
                datePicker.Focus();
            }
        }

        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async Task LoadAppointmentsByDate(DateTime date)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"LoadAppointmentsByDate called with: {date:dd-MM-yyyy}");

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    appointmentsContainer.Children.Clear();
                    appointmentsContainer.IsVisible = false;
                    loadingContainer.IsVisible = true;
                    emptyStateContainer.IsVisible = false;
                });

                string dateString = date.ToString("dd-MM-yyyy");
                System.Diagnostics.Debug.WriteLine($"Calling API with date: {dateString}");
                var response = await _lichSuService.GetLichSuKhamBenhAsync(dateString);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    loadingContainer.IsVisible = false;

                    if (response?.StatusCode == 200)
                    {
                        _currentData = response;
                        RenderAppointments(response);
                    }
                    else
                    {
                        ShowErrorMessage(response?.Message ?? "Lỗi khi tải dữ liệu");
                    }
                });
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    loadingContainer.IsVisible = false;
                    ShowErrorMessage($"Lỗi: {ex.Message}");
                });
            }
        }

        private void RenderAppointments(LichSuKhamBenhResponse data)
        {
            var allAppointments = new List<(string type, object appointmentData, DateTime date)>();

            if (data?.DanhSachTheoGoi?.Any() == true)
            {
                foreach (var goi in data.DanhSachTheoGoi)
                {
                    allAppointments.Add(("goi", goi, goi.NgayDateTime ?? DateTime.MinValue));
                }
            }

            if (data?.DanhSachTheoNgay?.Any() == true)
            {
                foreach (var ngay in data.DanhSachTheoNgay)
                {
                    allAppointments.Add(("ngay", ngay, ngay.NgayDateTime ?? DateTime.MinValue));
                }
            }

            if (data?.DanhSachTheoChuyenGia?.Any() == true)
            {
                foreach (var chuyengia in data.DanhSachTheoChuyenGia)
                {
                    allAppointments.Add(("chuyengia", chuyengia, chuyengia.NgayDateTime ?? DateTime.MinValue));
                }
            }

            allAppointments = allAppointments
                .OrderByDescending(x => x.date)
                .ToList();

            if (!allAppointments.Any())
            {
                emptyStateContainer.IsVisible = true;
                appointmentsContainer.IsVisible = false;
                return;
            }

            appointmentsContainer.Children.Clear();
            appointmentsContainer.IsVisible = true;
            emptyStateContainer.IsVisible = false;

            foreach (var appointment in allAppointments)
            {
                var card = appointment.type switch
                {
                    "goi" => CreateGoiCard((LichHenTheoGoi)appointment.appointmentData, data.DanhSachGoiKemTheo),
                    "ngay" => CreateNgayCard((LichHenTheoNgay)appointment.appointmentData),
                    "chuyengia" => CreateChuyenGiaCard((LichHenTheoChuyenGia)appointment.appointmentData),
                    _ => null
                };

                if (card != null)
                {
                    appointmentsContainer.Add(card);
                }
            }
        }

        // ... (giữ nguyên các phương thức còn lại)

        private Frame CreateGoiCard(LichHenTheoGoi goi, List<GoiKemTheo> goiKemTheo)
        {
            var statusBg = goi.DaDen == true ? Color.FromArgb("#10B981") : Color.FromArgb("#F59E0B");
            var statusText = goi.DaDen == true ? "Đã đến" : "Chưa đến";
            var paymentBg = goi.ThanhToan ? Color.FromArgb("#10B981") : Color.FromArgb("#F59E0B");
            var paymentText = goi.ThanhToan ? "Đã thanh toán" : "Chưa thanh toán";

            var content = new VerticalStackLayout { Spacing = 10 };

            var statusRow = new HorizontalStackLayout { Spacing = 8 };
            statusRow.Add(CreateStatusFrame(statusText, statusBg));
            statusRow.Add(CreateStatusFrame(paymentText, paymentBg));
            content.Add(statusRow);

            content.Add(CreateQRCodeGrid(goi.QrCode));

            var detailsStack = new VerticalStackLayout { Spacing = 8 };
            detailsStack.Add(CreateDetailRow("Gói khám:", goi.TenGoi ?? "N/A", "#1A73E8", true));
            detailsStack.Add(CreateDetailRow("Mã hẹn:", goi.MaDatLich ?? "N/A"));
            detailsStack.Add(CreateDetailRow("Ngày khám:",
                goi.NgayDateTime?.ToString("dd-MM-yyyy") ?? goi.Ngay ?? "N/A"));
            detailsStack.Add(CreateDetailRow("Giờ khám:", $"{goi.Tu} - {goi.Den}"));

            if (!string.IsNullOrEmpty(goi.TenPhong))
            {
                detailsStack.Add(CreateDetailRow("Phòng:", $"{goi.Sttphong} - {goi.TenPhong}"));
            }

            if (goi.TongTien > 0)
            {
                detailsStack.Add(CreateDetailRow("Giá:", FormatPrice(goi.TongTien), "#1A73E8", true));
            }

            content.Add(detailsStack);
            content.Add(CreateActionButtons("goi", goi));

            return new Frame
            {
                Margin = new Thickness(0, 0, 0, 15),
                Padding = 15,
                CornerRadius = 15,
                BorderColor = Color.FromArgb("#E2E8F0"),
                HasShadow = true,
                Content = content
            };
        }

        private Frame CreateNgayCard(LichHenTheoNgay ngay)
        {
            var statusBg = ngay.DaDen == true ? Color.FromArgb("#10B981") : Color.FromArgb("#F59E0B");
            var statusText = ngay.DaDen == true ? "Đã đến" : "Chưa đến";
            var paymentBg = ngay.ThanhToan ? Color.FromArgb("#10B981") : Color.FromArgb("#F59E0B");
            var paymentText = ngay.ThanhToan ? "Đã thanh toán" : "Chưa thanh toán";

            var content = new VerticalStackLayout { Spacing = 10 };

            var statusRow = new HorizontalStackLayout { Spacing = 8 };
            statusRow.Add(CreateStatusFrame(statusText, statusBg));
            statusRow.Add(CreateStatusFrame(paymentText, paymentBg));
            content.Add(statusRow);

            content.Add(CreateQRCodeGrid(ngay.QrCode));

            var detailsStack = new VerticalStackLayout { Spacing = 8 };
            detailsStack.Add(CreateDetailRow("Mã hẹn:", ngay.MaDatLich ?? "N/A"));
            detailsStack.Add(CreateDetailRow("Ngày khám:",
                ngay.NgayDateTime?.ToString("dd-MM-yyyy") ?? ngay.Ngay ?? "N/A"));
            detailsStack.Add(CreateDetailRow("Giờ khám:", $"{ngay.Tu} - {ngay.Den}"));
            content.Add(detailsStack);

            content.Add(CreateActionButtons("ngay", ngay));

            return new Frame
            {
                Margin = new Thickness(0, 0, 0, 15),
                Padding = 15,
                CornerRadius = 15,
                BorderColor = Color.FromArgb("#E2E8F0"),
                HasShadow = true,
                Content = content
            };
        }

        private Frame CreateChuyenGiaCard(LichHenTheoChuyenGia chuyengia)
        {
            var statusBg = chuyengia.DaDen == true ? Color.FromArgb("#10B981") : Color.FromArgb("#F59E0B");
            var statusText = chuyengia.DaDen == true ? "Đã đến" : "Chưa đến";
            var paymentBg = chuyengia.ThanhToan ? Color.FromArgb("#10B981") : Color.FromArgb("#F59E0B");
            var paymentText = chuyengia.ThanhToan ? "Đã thanh toán" : "Chưa thanh toán";

            var content = new VerticalStackLayout { Spacing = 10 };

            var statusRow = new HorizontalStackLayout { Spacing = 8 };
            statusRow.Add(CreateStatusFrame(statusText, statusBg));
            statusRow.Add(CreateStatusFrame(paymentText, paymentBg));
            content.Add(statusRow);

            content.Add(CreateQRCodeGrid(chuyengia.QrCode));

            var detailsStack = new VerticalStackLayout { Spacing = 8 };
            detailsStack.Add(CreateDetailRow("Mã hẹn:", chuyengia.MaDatLich ?? "N/A"));
            detailsStack.Add(CreateDetailRow("Chuyên gia:", chuyengia.TenChuyenGia ?? "N/A", "#1A73E8"));
            detailsStack.Add(CreateDetailRow("Ngày khám:",
                chuyengia.NgayDateTime?.ToString("dd-MM-yyyy") ?? chuyengia.Ngay ?? "N/A"));
            detailsStack.Add(CreateDetailRow("Giờ khám:", $"{chuyengia.Tu} - {chuyengia.Den}"));
            content.Add(detailsStack);

            content.Add(CreateActionButtons("chuyengia", chuyengia));

            return new Frame
            {
                Margin = new Thickness(0, 0, 0, 15),
                Padding = 15,
                CornerRadius = 15,
                BorderColor = Color.FromArgb("#E2E8F0"),
                HasShadow = true,
                Content = content
            };
        }

        private Frame CreateStatusFrame(string text, Color bgColor)
        {
            return new Frame
            {
                Padding = new Thickness(13, 5),
                CornerRadius = 10,
                HasShadow = false,
                BackgroundColor = bgColor,
                Content = new Label
                {
                    Text = text,
                    FontSize = 12,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.White
                }
            };
        }

        private Grid CreateQRCodeGrid(string qrCodeBase64)
        {
            var image = new Image
            {
                Aspect = Aspect.AspectFit,
                WidthRequest = 250,
                HeightRequest = 250
            };

            if (!string.IsNullOrEmpty(qrCodeBase64))
            {
                try
                {
                    if (qrCodeBase64.StartsWith("data:image"))
                    {
                        var base64Data = qrCodeBase64.Substring(qrCodeBase64.IndexOf(',') + 1);
                        var imageBytes = Convert.FromBase64String(base64Data);
                        image.Source = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading QR code: {ex.Message}");
                }
            }

            var border = new Border
            {
                Stroke = Color.FromArgb("#E2E8F0"),
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 10 },
                Padding = 10,
                BackgroundColor = Colors.White,
                WidthRequest = 250,
                HeightRequest = 250,
                Content = image
            };

            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += async (s, e) =>
            {
                await OnQrCodeTapped(qrCodeBase64);
            };
            border.GestureRecognizers.Add(tapGesture);

            return new Grid
            {
                HorizontalOptions = LayoutOptions.Center,
                Children = { border }
            };
        }

        private HorizontalStackLayout CreateDetailRow(string label, string value, string valueColor = "#333333", bool isBold = false)
        {
            return new HorizontalStackLayout
            {
                Spacing = 10,
                Children =
                {
                    new Label
                    {
                        Text = label,
                        FontSize = 14,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#64748B"),
                        WidthRequest = 100
                    },
                    new Label
                    {
                        Text = value,
                        FontSize = 14,
                        TextColor = Color.FromArgb(valueColor),
                        FontAttributes = isBold ? FontAttributes.Bold : FontAttributes.None
                    }
                }
            };
        }

        private Grid CreateActionButtons(string type, object appointmentData)
        {
            var grid = new Grid
            {
                ColumnSpacing = 10,
                Margin = new Thickness(0, 10, 0, 0),
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Star)
                }
            };

            long id = 0;
            string dateString = null;

            if (type == "goi" && appointmentData is LichHenTheoGoi goi)
            {
                id = goi.IdGoi ?? 0;
                dateString = goi.NgayDangKy.ToString("dd-MM-yyyy");
            }
            else if (type == "ngay" && appointmentData is LichHenTheoNgay ngay)
            {
                id = ngay.Id;
            }
            else if (type == "chuyengia" && appointmentData is LichHenTheoChuyenGia chuyengia)
            {
                id = chuyengia.Id;
            }

            var btnDoiLich = new Border
            {
                Stroke = Color.FromArgb("#1A73E8"),
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 8 },
                BackgroundColor = Color.FromArgb("#1A73E8"),
                HeightRequest = 40,
                Content = new Label
                {
                    Text = "Đổi lịch",
                    FontSize = 13,
                    TextColor = Colors.White,
                    FontAttributes = FontAttributes.Bold,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center
                }
            };

            var doiLichTap = new TapGestureRecognizer();
            doiLichTap.Tapped += async (s, e) => await HandleDoiLichTapped(type, id);
            btnDoiLich.GestureRecognizers.Add(doiLichTap);
            grid.Add(btnDoiLich, 0, 0);

            var btnChiTiet = new Border
            {
                Stroke = Color.FromArgb("#1A73E8"),
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 8 },
                BackgroundColor = Colors.White,
                HeightRequest = 40,
                Content = new Label
                {
                    Text = "Chi tiết",
                    FontSize = 13,
                    TextColor = Color.FromArgb("#1A73E8"),
                    FontAttributes = FontAttributes.Bold,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center
                }
            };

            var chiTietTap = new TapGestureRecognizer();
            chiTietTap.Tapped += async (s, e) => await HandleXemChiTietTapped(type, appointmentData);
            btnChiTiet.GestureRecognizers.Add(chiTietTap);
            grid.Add(btnChiTiet, 1, 0);

            var btnXoa = new Border
            {
                Stroke = Color.FromArgb("#EF4444"),
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 8 },
                BackgroundColor = Colors.White,
                HeightRequest = 40,
                Content = new Label
                {
                    Text = "Xóa",
                    FontSize = 13,
                    TextColor = Color.FromArgb("#EF4444"),
                    FontAttributes = FontAttributes.Bold,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center
                }
            };

            var xoaTap = new TapGestureRecognizer();
            xoaTap.Tapped += async (s, e) => await HandleXoaTapped(type, id, dateString);
            btnXoa.GestureRecognizers.Add(xoaTap);
            grid.Add(btnXoa, 2, 0);

            return grid;
        }

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
            await Navigation.PushAsync(new S0306_mLichSuKhamBenh());
        }

        private async void OnTaiKhoanClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new S0306_mTaiKhoan());
        }

        #region Event Handlers

        private async Task HandleDoiLichTapped(string type, long id)
        {
            string title = type switch
            {
                "goi" => "Đổi lịch khám gói",
                "chuyengia" => "Đổi lịch khám chuyên gia",
                _ => "Đổi lịch khám"
            };

            await Navigation.PushAsync(new S0306_mKhamTheoGoi());
        }

        private async Task HandleXemChiTietTapped(string type, object appointmentData)
        {
            if (type == "goi" && appointmentData is LichHenTheoGoi goi)
            {
                await ShowDetailModal(goi);
            }
            else
            {
                await DisplayAlert("Thông báo", "Chỉ lịch hẹn theo gói mới có chi tiết dịch vụ", "OK");
            }
        }

        private async Task ShowDetailModal(LichHenTheoGoi goi)
        {
            var modalPage = new ContentPage
            {
                BackgroundColor = Color.FromArgb("#80000000")
            };

            var mainContainer = new Grid
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Fill,
                Margin = new Thickness(20)
            };

            var modalContent = new Frame
            {
                BackgroundColor = Colors.White,
                CornerRadius = 15,
                Padding = 0,
                HasShadow = true,
                MaximumHeightRequest = 600
            };

            var contentStack = new VerticalStackLayout
            {
                Spacing = 0
            };

            var headerGrid = new Grid
            {
                BackgroundColor = Color.FromArgb("#1A73E8"),
                Padding = new Thickness(20, 15),
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto)
                }
            };

            var titleLabel = new Label
            {
                Text = "DANH SÁCH DỊCH VỤ",
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                VerticalOptions = LayoutOptions.Center
            };
            headerGrid.Add(titleLabel, 0, 0);

            var closeButton = new Button
            {
                Text = "✕",
                FontSize = 20,
                TextColor = Colors.White,
                BackgroundColor = Colors.Transparent,
                WidthRequest = 45,
                HeightRequest = 45,
                CornerRadius = 30
            };
            closeButton.Clicked += async (s, e) =>
            {
                await Navigation.PopModalAsync();
            };
            headerGrid.Add(closeButton, 1, 0);

            contentStack.Add(headerGrid);

            var tableHeader = new Grid
            {
                Padding = new Thickness(20, 10),
                BackgroundColor = Color.FromArgb("#EFEFEF"),
                ColumnDefinitions =
                {
                    new ColumnDefinition(new GridLength(3, GridUnitType.Star)),
                    new ColumnDefinition(new GridLength(1.5, GridUnitType.Star)),
                    new ColumnDefinition(new GridLength(1.5, GridUnitType.Star))
                }
            };

            tableHeader.Add(new Label
            {
                Text = "DỊCH VỤ",
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#1A73E8"),
            }, 0, 0);

            tableHeader.Add(new Label
            {
                Text = "BẮT ĐẦU DỰ KIẾN",
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#1A73E8"),
                HorizontalTextAlignment = TextAlignment.Center
            }, 1, 0);

            tableHeader.Add(new Label
            {
                Text = "KẾT THÚC DỰ KIẾN",
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#1A73E8"),
                HorizontalTextAlignment = TextAlignment.Center
            }, 2, 0);

            contentStack.Add(tableHeader);

            var servicesScroll = new Microsoft.Maui.Controls.ScrollView
            {
                MaximumHeightRequest = 400
            };

            var servicesStack = new VerticalStackLayout
            {
                Padding = new Thickness(20, 10),
                Spacing = 0
            };

            if (goi.ChiTietDichVu?.Any() == true)
            {
                bool isEven = false;
                foreach (var dichVu in goi.ChiTietDichVu)
                {
                    var serviceRow = CreateServiceRow(dichVu, isEven);
                    servicesStack.Add(serviceRow);
                    isEven = !isEven;
                }
            }
            else
            {
                servicesStack.Add(new Label
                {
                    Text = "Không có dịch vụ nào",
                    FontSize = 14,
                    TextColor = Color.FromArgb("#64748B"),
                    HorizontalOptions = LayoutOptions.Center,
                    Margin = new Thickness(0, 20)
                });
            }

            servicesScroll.Content = servicesStack;
            contentStack.Add(servicesScroll);

            if (_currentData?.DanhSachGoiKemTheo?.Any() == true)
            {
                var goiKemTheoSection = new VerticalStackLayout
                {
                    Padding = new Thickness(20, 15),
                    Spacing = 8,
                    BackgroundColor = Color.FromArgb("#FFF9E6")
                };

                goiKemTheoSection.Add(new Label
                {
                    Text = "GÓI KÈM THEO",
                    FontSize = 14,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#F59E0B")
                });

                foreach (var goiKemTheo in _currentData.DanhSachGoiKemTheo)
                {
                    goiKemTheoSection.Add(new Label
                    {
                        Text = $"• {goiKemTheo.TenGoi}",
                        FontSize = 13,
                        TextColor = Color.FromArgb("#64748B")
                    });
                }

                contentStack.Add(goiKemTheoSection);
            }

            modalContent.Content = contentStack;
            mainContainer.Children.Add(modalContent);
            modalPage.Content = mainContainer;

            await Navigation.PushModalAsync(modalPage);
        }

        private Grid CreateServiceRow(ChiTietDichVu dichVu, bool isEven)
        {
            var grid = new Grid
            {
                Padding = new Thickness(0, 10),
                BackgroundColor = isEven ? Color.FromArgb("#F8FAFC") : Colors.White,
                ColumnDefinitions =
                {
                    new ColumnDefinition(new GridLength(3, GridUnitType.Star)),
                    new ColumnDefinition(new GridLength(1.5, GridUnitType.Star)),
                    new ColumnDefinition(new GridLength(1.5, GridUnitType.Star))
                }
            };

            grid.Add(new Label
            {
                Text = dichVu.TenDichVu ?? "N/A",
                FontSize = 13,
                TextColor = Color.FromArgb("#333333"),
                LineBreakMode = LineBreakMode.WordWrap
            }, 0, 0);

            grid.Add(new Label
            {
                Text = dichVu.Tgbd ?? "N/A",
                FontSize = 13,
                TextColor = Color.FromArgb("#333333"),
                HorizontalTextAlignment = TextAlignment.Center
            }, 1, 0);

            grid.Add(new Label
            {
                Text = dichVu.Tgkt ?? "N/A",
                FontSize = 13,
                TextColor = Color.FromArgb("#333333"),
                HorizontalTextAlignment = TextAlignment.Center
            }, 2, 0);

            return grid;
        }

        private async Task HandleXoaTapped(string type, long id, string dateString)
        {
            bool result = await DisplayAlert("Xác nhận xóa",
                "Bạn có chắc chắn muốn xóa lịch hẹn này?", "Xóa", "Hủy");

            if (!result)
                return;

            try
            {
                loadingContainer.IsVisible = true;

                var response = await _dKGoiKhamService.XoaDatHenAsync();

                loadingContainer.IsVisible = false;

                if (response.Success)
                {
                    await DisplayAlert("Thành công", response.Message, "OK");
                    await LoadAppointmentsByDate(_selectedDate);
                }
                else
                {
                    await DisplayAlert("Lỗi", response?.Message ?? "Lỗi xóa lịch hẹn", "OK");
                }
            }
            catch (Exception ex)
            {
                loadingContainer.IsVisible = false;
                await DisplayAlert("Lỗi", $"Lỗi: {ex.Message}", "OK");
            }
        }

        private async Task OnQrCodeTapped(string qrCodeBase64)
        {
            if (string.IsNullOrEmpty(qrCodeBase64))
                return;

            await DisplayAlert("QR Code", "Mã QR dùng để check-in", "OK");
        }

        private void ShowErrorMessage(string message)
        {
            emptyStateContainer.IsVisible = false;
            loadingContainer.IsVisible = false;
            appointmentsContainer.IsVisible = false;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("Lỗi", message, "OK");
            });
        }

        private string FormatPrice(decimal price)
        {
            return price.ToString("N0") + " VND";
        }

        #endregion
    }
}
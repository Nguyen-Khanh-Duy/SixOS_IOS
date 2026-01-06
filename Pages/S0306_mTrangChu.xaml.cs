using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Graphics;
using SixOSDatKhamAppMobile.Services;
using SixOSDatKhamAppMobile.Services.S0305;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Timers;

namespace SixOSDatKhamAppMobile.Pages;

public partial class S0306_mTrangChu : ContentPage
{
    private readonly S0305_TrangChuService _trangChuService;
    private readonly S0305_DKGoiKhamService _dkGoiKhamService;
    private ObservableCollection<ChuyenGiaTrangChuDTO> _danhSachChuyenGia;
    private ObservableCollection<ChuyenGiaTrangChuDTO> _danhSachChuyenGiaGoc;
    private ObservableCollection<GoiKhamTrangChuDTO> _danhSachGoiKham;
    private ObservableCollection<GoiKhamTrangChuDTO> _danhSachGoiKhamGoc;

    // Thông tin người dùng để lọc gói khám
    private int _tuoiNguoiDung = 1;
    private long _gioiTinhNguoiDung = 0;

    private System.Timers.Timer _quangCaoTimer;
    private int _currentQuangCaoIndex = 0;
    private bool _hasMoved;
    private const int QUANG_CAO_INTERVAL_MS = 3000;

    private double _startX, _startY;
    private bool _isDragging = false;
    private const double DRAG_THRESHOLD = 50; // Ngưỡng để phân biệt tap và drag

    private bool _daLoadDuLieuLanDau = false; // biến cờ để kiểm tra đã load dữ liệu lần đầu

    public S0306_mTrangChu()
    {
        InitializeComponent();
        _trangChuService = new S0305_TrangChuService();
        _dkGoiKhamService = new S0305_DKGoiKhamService();
        _danhSachChuyenGia = new ObservableCollection<ChuyenGiaTrangChuDTO>();
        _danhSachChuyenGiaGoc = new ObservableCollection<ChuyenGiaTrangChuDTO>();
        _danhSachGoiKham = new ObservableCollection<GoiKhamTrangChuDTO>();
        _danhSachGoiKhamGoc = new ObservableCollection<GoiKhamTrangChuDTO>();
        SetupSafeArea();
        // Khởi tạo timer
        InitializeQuangCaoTimer();

        // Liên kết Indicator với Carousel
        QuangCaoCarousel.IndicatorView = QuangCaoIndicator;
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
        if (!_daLoadDuLieuLanDau)
        {
            await LoadThongTinNguoiDungAsync();
            await LoadDuLieuTrangChuAsync();
            await S0305_SecureStorage.SaveDaQuaTrangChuAsync(true);
            _daLoadDuLieuLanDau = true;
        }
        StartQuangCaoTimer();
    }

    #region Event cho các nút Xem hướng dẫn và Xem chi tiết
    private async Task ShowHuongDanDetail(string title, string htmlContent)
    {
        try
        {
            var fullHtml = $@"
<!DOCTYPE html>
<html>
<head>
    <meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no'>
    <meta charset='UTF-8'>
    <style>
        html, body {{
            margin: 0;
            padding: 0;
            height: 100%;
            background-color: #fff;
        }}

        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            font-size: 16px;
            line-height: 1.6;
            color: #333;

            /* Lề trên chỉ 1px */
            padding: 20px 20px 20px 20px;

            word-wrap: break-word;
            overflow-wrap: break-word;
        }}

        /* FIX khoảng trắng mặc định 10–16px của phần tử đầu tiên */
        body > div > *:first-child {{
            margin-top: 0 !important;
        }}

        h1, h2, h3, h4, h5, h6 {{
            color: #1A73E8;
            margin-top: 24px;
            margin-bottom: 12px;
            line-height: 1.3;
        }}

        h3 {{
            font-size: 18px;
        }}

        p {{
            margin-bottom: 16px;
        }}

        ul, ol {{
            padding-left: 24px;
            margin-bottom: 16px;
        }}

        li {{
            margin-bottom: 8px;
        }}

        strong, b {{
            font-weight: 600;
            color: #1E293B;
        }}
    </style>
</head>
<body>
    <div>
        {htmlContent}
    </div>
</body>
</html>";

            // Tạo Grid chính với 2 hàng
            var mainGrid = new Grid
            {
                RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = 40 }, // Hàng đen 40px trên cùng
                new RowDefinition { Height = GridLength.Star } // Hàng trắng còn lại
            }
            };

            // Thêm phần màu đen trên cùng
            var blackTop = new BoxView
            {
                Color = Colors.Black,
                VerticalOptions = LayoutOptions.Fill,
                HorizontalOptions = LayoutOptions.Fill
            };
            Grid.SetRow(blackTop, 0);
            mainGrid.Children.Add(blackTop);

            // Tạo container cho nội dung trắng
            var whiteContainer = new Grid
            {
                RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = GridLength.Auto }, // Header
                new RowDefinition { Height = GridLength.Star }, // WebView
                new RowDefinition { Height = GridLength.Auto }  // Button
            },
                BackgroundColor = Colors.White,
                RowSpacing = 0
            };
            Grid.SetRow(whiteContainer, 1);
            mainGrid.Children.Add(whiteContainer);

            // ===== Header =====
            var titleLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                BackgroundColor = Color.FromArgb("#1A73E8"),
                Padding = new Thickness(16, 10),
                HeightRequest = 50
            };

            var titleLabel = new Label
            {
                Text = "HƯỚNG DẪN",
                TextColor = Colors.White,
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.CenterAndExpand
            };

            titleLayout.Children.Add(titleLabel);

            // ===== WebView =====
            var webView = new WebView
            {
                Source = new HtmlWebViewSource { Html = fullHtml },
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Colors.White,

                // 👉 Cách top 10px
                Margin = new Thickness(0, 5, 0, 0)
            };


            // Nút đóng
            var closeButton = new Button
            {
                Text = "ĐÓNG",
                BackgroundColor = Color.FromArgb("#1A73E8"),
                TextColor = Colors.White,
                Margin = new Thickness(20, 5, 20, 47),
                CornerRadius = 20,
                HeightRequest = 44
            };

            // ===== Add to Grid =====
            whiteContainer.Children.Add(titleLayout);
            Grid.SetRow(titleLayout, 0);

            whiteContainer.Children.Add(webView);
            Grid.SetRow(webView, 1);

            whiteContainer.Children.Add(closeButton);
            Grid.SetRow(closeButton, 2);

            var detailPage = new ContentPage
            {
                Title = title,
                Content = mainGrid
            };

            closeButton.Clicked += async (s, e) =>
            {
                await detailPage.Navigation.PopModalAsync();
            };

            await Navigation.PushModalAsync(detailPage);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", $"Không thể hiển thị chi tiết: {ex.Message}", "OK");
        }
    }


    private async Task ShowChiTietGoiKham(GoiKhamTrangChuDTO goiKham)
    {
        try
        {
            var detailPage = new ContentPage
            {
                Title = "Chi tiết gói khám"
            };

            // Tạo Grid chính với 2 hàng
            var mainGrid = new Grid
            {
                RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = 40 }, // Hàng đen 40px trên cùng
                new RowDefinition { Height = GridLength.Star } // Hàng trắng còn lại
            }
            };

            // Thêm phần màu đen trên cùng
            var blackTop = new BoxView
            {
                Color = Colors.Black,
                VerticalOptions = LayoutOptions.Fill,
                HorizontalOptions = LayoutOptions.Fill
            };
            Grid.SetRow(blackTop, 0);
            mainGrid.Children.Add(blackTop);

            // Tạo container cho nội dung trắng
            var whiteContainer = new Grid
            {
                RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = GridLength.Star },
                new RowDefinition { Height = GridLength.Auto }
            },
                BackgroundColor = Colors.White
            };
            Grid.SetRow(whiteContainer, 1);
            mainGrid.Children.Add(whiteContainer);

            var scrollView = new Microsoft.Maui.Controls.ScrollView
            {
                Padding = new Thickness(20)
            };

            var contentStack = new VerticalStackLayout
            {
                Spacing = 20
            };

            // Thông tin gói khám
            var infoFrame = new Frame
            {
                BorderColor = Color.FromArgb("#E2E8F0"),
                BackgroundColor = Colors.White,
                CornerRadius = 12,
                Padding = 15,
                Margin = new Thickness(0, 0, 0, 0)
            };

            var infoStack = new VerticalStackLayout
            {
                Spacing = 10
            };

            infoStack.Children.Add(new Label
            {
                Text = "THÔNG TIN GÓI KHÁM",
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#1A73E8")
            });

            infoStack.Children.Add(CreateInfoRow("Tên gói", goiKham.TenGoi));
            infoStack.Children.Add(CreateInfoRow("Giá tiền", $"{goiKham.TongTien:N0} VNĐ"));

            infoFrame.Content = infoStack;
            contentStack.Children.Add(infoFrame);

            // Danh sách dịch vụ
            if (goiKham.ChiTiet != null && goiKham.ChiTiet.Count > 0)
            {
                var servicesFrame = new Frame
                {
                    BorderColor = Color.FromArgb("#E2E8F0"),
                    BackgroundColor = Colors.White,
                    CornerRadius = 12,
                    Padding = 15
                };

                var servicesStack = new VerticalStackLayout
                {
                    Spacing = 10
                };

                servicesStack.Children.Add(new Label
                {
                    Text = "DANH SÁCH DỊCH VỤ",
                    FontSize = 18,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#1A73E8")
                });

                var tableGrid = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = 50 },
                    new ColumnDefinition { Width = GridLength.Star }
                },
                    RowSpacing = 0
                };

                // Header
                tableGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var headerBackground = new BoxView
                {
                    Color = Color.FromArgb("#1A73E8"),
                    CornerRadius = new CornerRadius(8, 8, 0, 0)
                };

                var sttHeader = new Label
                {
                    Text = "STT",
                    FontSize = 14,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.White,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Padding = new Thickness(0, 15)
                };

                var dichVuHeader = new Label
                {
                    Text = "DỊCH VỤ",
                    FontSize = 14,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.White,
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.Center,
                    Padding = new Thickness(15, 15)
                };

                Grid.SetRow(headerBackground, 0);
                Grid.SetColumnSpan(headerBackground, 2);
                tableGrid.Children.Add(headerBackground);

                Grid.SetRow(sttHeader, 0);
                Grid.SetColumn(sttHeader, 0);
                tableGrid.Children.Add(sttHeader);

                Grid.SetRow(dichVuHeader, 0);
                Grid.SetColumn(dichVuHeader, 1);
                tableGrid.Children.Add(dichVuHeader);

                // Dữ liệu
                int rowIndex = 1;
                foreach (var item in goiKham.ChiTiet.OrderBy(x => x.Stt))
                {
                    tableGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    var rowBackground = new BoxView
                    {
                        Color = rowIndex % 2 == 0 ? Color.FromArgb("#F9FAFB") : Colors.White
                    };

                    Grid.SetRow(rowBackground, rowIndex);
                    Grid.SetColumnSpan(rowBackground, 2);
                    tableGrid.Children.Add(rowBackground);

                    var sttLabel = new Label
                    {
                        Text = item.Stt?.ToString() ?? rowIndex.ToString(),
                        FontSize = 14,
                        TextColor = Color.FromArgb("#374151"),
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        Padding = new Thickness(0, 12)
                    };

                    var dichVuLabel = new Label
                    {
                        Text = item.TenDichVu ?? "Đang cập nhật...",
                        FontSize = 14,
                        TextColor = Color.FromArgb("#1F2937"),
                        VerticalOptions = LayoutOptions.Center,
                        Padding = new Thickness(15, 12),
                        LineBreakMode = LineBreakMode.WordWrap
                    };

                    Grid.SetRow(sttLabel, rowIndex);
                    Grid.SetColumn(sttLabel, 0);
                    tableGrid.Children.Add(sttLabel);

                    Grid.SetRow(dichVuLabel, rowIndex);
                    Grid.SetColumn(dichVuLabel, 1);
                    tableGrid.Children.Add(dichVuLabel);

                    if (rowIndex < goiKham.ChiTiet.Count)
                    {
                        var separator = new BoxView
                        {
                            HeightRequest = 1,
                            Color = Color.FromArgb("#E5E7EB"),
                            VerticalOptions = LayoutOptions.End
                        };

                        Grid.SetRow(separator, rowIndex);
                        Grid.SetColumnSpan(separator, 2);
                        tableGrid.Children.Add(separator);
                    }

                    rowIndex++;
                }

                var tableFrame = new Frame
                {
                    BackgroundColor = Colors.White,
                    BorderColor = Color.FromArgb("#E5E7EB"),
                    CornerRadius = 8,
                    Padding = 0,
                    HasShadow = true,
                    Content = tableGrid
                };

                servicesStack.Children.Add(tableFrame);
                servicesFrame.Content = servicesStack;
                contentStack.Children.Add(servicesFrame);
            }

            scrollView.Content = contentStack;

            var closeButton = new Button
            {
                Text = "ĐÓNG",
                BackgroundColor = Color.FromArgb("#1A73E8"),
                TextColor = Colors.White,
                Margin = new Thickness(20, 5, 20, 47),
                CornerRadius = 20,
                HeightRequest = 44
            };

            closeButton.Clicked += async (s, e) =>
            {
                await detailPage.Navigation.PopModalAsync();
            };

            // Thêm vào whiteContainer thay vì mainGrid
            whiteContainer.Children.Add(scrollView);
            Grid.SetRow(scrollView, 0);

            whiteContainer.Children.Add(closeButton);
            Grid.SetRow(closeButton, 1);

            detailPage.Content = mainGrid;

            await Navigation.PushModalAsync(detailPage);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", $"Không thể hiển thị chi tiết: {ex.Message}", "OK");
        }
    }

    private Grid CreateInfoRow(string label, string value)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
        {
            new ColumnDefinition { Width = new GridLength(100) },
            new ColumnDefinition { Width = GridLength.Star }
        }
        };

        grid.Children.Add(new Label
        {
            Text = label + ":",
            FontSize = 14,
            TextColor = Color.FromArgb("#64748B")
        });

        grid.Children.Add(new Label
        {
            Text = value,
            FontSize = 14,
            TextColor = Color.FromArgb("#1E293B"),
            FontAttributes = FontAttributes.Bold
        });
        Grid.SetColumn((View)grid.Children[1], 1);

        return grid;
    }

  
    #endregion

    #region Quảng Cáo Carousel Timer
    private void InitializeQuangCaoTimer()
    {
        _quangCaoTimer = new System.Timers.Timer(QUANG_CAO_INTERVAL_MS);
        _quangCaoTimer.Elapsed += OnQuangCaoTimerElapsed;
        _quangCaoTimer.AutoReset = true;
    }

    private void StartQuangCaoTimer()
    {
        if (QuangCaoCarousel.ItemsSource != null && _quangCaoTimer != null)
        {
            _quangCaoTimer.Start();
        }
    }

    private void StopQuangCaoTimer()
    {
        if (_quangCaoTimer != null)
        {
            _quangCaoTimer.Stop();
        }
    }

    private async void OnQuangCaoTimerElapsed(object sender, ElapsedEventArgs e)
    {
        Device.BeginInvokeOnMainThread(() =>
        {
            if (QuangCaoCarousel.ItemsSource != null)
            {
                var itemCount = ((System.Collections.IList)QuangCaoCarousel.ItemsSource).Count;
                if (itemCount > 0)
                {
                    _currentQuangCaoIndex = (_currentQuangCaoIndex + 1) % itemCount;
                    QuangCaoCarousel.Position = _currentQuangCaoIndex;
                }
            }
        });
    }

    private void QuangCaoCarousel_PositionChanged(object sender, PositionChangedEventArgs e)
    {
        _currentQuangCaoIndex = e.CurrentPosition;
    }
    #endregion

    #region Các phương thức hiện có
    private async Task LoadThongTinNguoiDungAsync()
    {
        try
        {
            var result = await _dkGoiKhamService.LayDoTuoiGioiTinhBNAsync();

            if (result.Success && result.Data != null)
            {
                _tuoiNguoiDung = result.Data.Tuoi;
                _gioiTinhNguoiDung = result.Data.GioiTinh ?? 0;
            }
            else
            {
                _tuoiNguoiDung = 1;
                _gioiTinhNguoiDung = 0;
            }
        }
        catch (Exception ex)
        {
            _tuoiNguoiDung = 1;
            _gioiTinhNguoiDung = 0;
        }
    }

    private async Task LoadDuLieuTrangChuAsync()
    {
        try
        {
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;

            var resultCG = await _trangChuService.LayNgauNhienChuyenGiaAsync(3);
            if (resultCG.Success && resultCG.Data != null)
            {
                _danhSachChuyenGiaGoc.Clear();
                _danhSachChuyenGia.Clear();
                foreach (var cg in resultCG.Data)
                {
                    _danhSachChuyenGiaGoc.Add(cg);
                    _danhSachChuyenGia.Add(cg);
                }
                ChuyenGiaCollectionView.ItemsSource = _danhSachChuyenGia;
            }

            var resultGK = await _trangChuService.LayGoiKhamTheoTuoiGioiTinhAsync(
                _tuoiNguoiDung,
                _gioiTinhNguoiDung,
                3);

            if (resultGK.Success && resultGK.Data != null)
            {
                _danhSachGoiKhamGoc.Clear();
                _danhSachGoiKham.Clear();
                foreach (var gk in resultGK.Data)
                {
                    _danhSachGoiKhamGoc.Add(gk);
                    _danhSachGoiKham.Add(gk);
                }
                GoiKhamCollectionView.ItemsSource = _danhSachGoiKham;
            }
            else if (!resultGK.Success)
            {
                var fallbackResult = await _trangChuService.LayNgauNhienGoiKhamAsync(3);
                if (fallbackResult.Success && fallbackResult.Data != null)
                {
                    _danhSachGoiKhamGoc.Clear();
                    _danhSachGoiKham.Clear();
                    foreach (var gk in fallbackResult.Data)
                    {
                        _danhSachGoiKhamGoc.Add(gk);
                        _danhSachGoiKham.Add(gk);
                    }
                    GoiKhamCollectionView.ItemsSource = _danhSachGoiKham;
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", $"Không thể tải dữ liệu: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    // Các phương thức hiện có khác...
    #endregion

    #region Các phương thức mới - Xử lý 2 nút
    // Xử lý nút "Xem hướng dẫn"
    private async void OnXemHuongDanClicked(object sender, EventArgs e)
    {
        try
        {
            if (sender is Label label && label.BindingContext is GoiKhamTrangChuDTO goiKham)
            {
                if (!string.IsNullOrEmpty(goiKham.HuongDan))
                {
                    await ShowHuongDanDetail("Hướng dẫn sử dụng", goiKham.HuongDan);
                }
                else
                {
                    await DisplayAlert("Hướng dẫn",
                        !string.IsNullOrEmpty(goiKham.MoTa) ? goiKham.MoTa : "Chưa có hướng dẫn",
                        "Đóng");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", $"Không thể hiển thị hướng dẫn: {ex.Message}", "OK");
        }
    }

    // Xử lý nút "Xem chi tiết"
    private async void OnXemChiTietClicked(object sender, EventArgs e)
    {
        try
        {
            if (sender is Label label && label.BindingContext is GoiKhamTrangChuDTO goiKham)
            {
                if (goiKham.ChiTiet != null && goiKham.ChiTiet.Count > 0)
                {
                    await ShowChiTietGoiKham(goiKham);
                }
                else
                {
                    await DisplayAlert("Chi tiết", "Chưa có thông tin chi tiết", "Đóng");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", $"Không thể hiển thị chi tiết: {ex.Message}", "OK");
        }
    }
    #endregion

    #region Các phương thức hiện có khác (giữ nguyên)
    private async void OnDangNhapClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new S0306_mDangNhap());
    }

    private async void OnViewAllNewsClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Tin tức", "Tạm thời chưa có tin tức nào.", "OK");
    }

    private async void OnProfileClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new S0306_mHoSoBenhNhan());
    }

    private async void OnViewAllDoctorsClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new S0306_mDanhSachChuyenGia());
    }

    private async void OnViewAllServicesClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new S0306_mKhamTheoGoi());
    }

    private async void OnBookDoctorClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new S0306_mHoSoBenhNhan());
    }

    private async void OnBookServiceClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is long idGoi)
        {
            await Navigation.PushAsync(new S0306_mKhamTheoGoi());
        }
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

    private async void OpenCS1Map(object sender, EventArgs e)
    {
        var url = "https://www.google.com/maps?q=47 Nguyễn Huy Lượng, Phường Bình Thạnh, TP.HCM";
        await Launcher.OpenAsync(url);
    }

    private async void OpenCS2Map(object sender, EventArgs e)
    {
        var url = "https://www.google.com/maps?q=Số 12 Đường 400, Phường Tăng Nhơn Phú,+TPHCM";
        await Launcher.OpenAsync(url);
    }

    private async void OnThongBaoClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Thông báo", "Chưa có thông báo mới", "OK");
    }

    //private void OnSearchCompleted(object sender, EventArgs e)
    //{
    //    var searchText = SearchEntry.Text?.Trim().ToLower() ?? "";

    //    if (string.IsNullOrWhiteSpace(searchText))
    //    {
    //        // Reset về danh sách gốc
    //        _danhSachChuyenGia.Clear();
    //        foreach (var item in _danhSachChuyenGiaGoc)
    //            _danhSachChuyenGia.Add(item);

    //        _danhSachGoiKham.Clear();
    //        foreach (var item in _danhSachGoiKhamGoc)
    //            _danhSachGoiKham.Add(item);
    //    }
    //    else
    //    {
    //        // Lọc chuyên gia
    //        _danhSachChuyenGia.Clear();
    //        var filteredCG = _danhSachChuyenGiaGoc.Where(cg =>
    //            cg.TenChuyenGia?.ToLower().Contains(searchText) == true ||
    //            cg.ChucDanh?.ToLower().Contains(searchText) == true ||
    //            cg.MoTaNgan?.ToLower().Contains(searchText) == true
    //        );
    //        foreach (var item in filteredCG)
    //            _danhSachChuyenGia.Add(item);

    //        // Lọc gói khám
    //        _danhSachGoiKham.Clear();
    //        var filteredGK = _danhSachGoiKhamGoc.Where(gk =>
    //            gk.TenGoi?.ToLower().Contains(searchText) == true ||
    //            gk.MoTa?.ToLower().Contains(searchText) == true
    //        );
    //        foreach (var item in filteredGK)
    //            _danhSachGoiKham.Add(item);
    //    }
    //}

    // Phương thức xử lý kéo thả
    private void OnPanGestureUpdated(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                // Lưu vị trí ban đầu
                _startX = FloatingCallButton.TranslationX;
                _startY = FloatingCallButton.TranslationY;
                _isDragging = true;
                _hasMoved = false;
                break;

            case GestureStatus.Running:
                // Di chuyển button theo cử chỉ
                FloatingCallButton.TranslationX = _startX + e.TotalX;
                FloatingCallButton.TranslationY = _startY + e.TotalY;

                // Kiểm tra nếu di chuyển đủ xa để coi là drag
                if (Math.Abs(e.TotalX) > DRAG_THRESHOLD || Math.Abs(e.TotalY) > DRAG_THRESHOLD)
                {
                    _hasMoved = true;
                }
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                _isDragging = false;

                // Nếu đã kéo thả, không thực hiện sự kiện tap
                if (_hasMoved)
                {
                    // Tự động "dính" vào cạnh gần nhất
                    SnapToNearestEdge();
                    _hasMoved = false;
                }
                break;
        }
    }

    // Phương thức tự động dính vào cạnh gần nhất
    private void SnapToNearestEdge()
    {
        var screenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
        var screenHeight = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;

        var buttonX = FloatingCallButton.X + FloatingCallButton.TranslationX;
        var buttonY = FloatingCallButton.Y + FloatingCallButton.TranslationY;
        var buttonWidth = FloatingCallButton.Width;
        var buttonHeight = FloatingCallButton.Height;

        // Tính khoảng cách đến các cạnh
        var distanceToLeft = buttonX;
        var distanceToRight = screenWidth - (buttonX + buttonWidth);
        var distanceToTop = buttonY;
        var distanceToBottom = screenHeight - (buttonY + buttonHeight);

        // Tìm cạnh gần nhất
        var minHorizontal = Math.Min(distanceToLeft, distanceToRight);
        var minVertical = Math.Min(distanceToTop, distanceToBottom);

        if (minHorizontal < minVertical)
        {
            // Dính vào cạnh trái hoặc phải
            if (distanceToLeft < distanceToRight)
            {
                // Dính vào cạnh trái
                FloatingCallButton.TranslateTo(0, FloatingCallButton.TranslationY, 250, Easing.SpringOut);
            }
            else
            {
                // Dính vào cạnh phải
                var targetX = screenWidth - buttonWidth - FloatingCallButton.X;
                FloatingCallButton.TranslateTo(targetX, FloatingCallButton.TranslationY, 250, Easing.SpringOut);
            }
        }
        else
        {
            // Dính vào cạnh trên hoặc dưới
            if (distanceToTop < distanceToBottom)
            {
                // Dính vào cạnh trên
                FloatingCallButton.TranslateTo(FloatingCallButton.TranslationX, 0, 250, Easing.SpringOut);
            }
            else
            {
                // Dính vào cạnh dưới
                var targetY = screenHeight - buttonHeight - FloatingCallButton.Y;
                FloatingCallButton.TranslateTo(FloatingCallButton.TranslationX, targetY, 250, Easing.SpringOut);
            }
        }
    }

    // Sửa phương thức OnCallHospitalClicked để không block khi đang kéo
    private async void OnCallHospitalClicked(object sender, EventArgs e)
    {
        // Nếu đang kéo thì không gọi
        if (_isDragging || _hasMoved)
            return;

        // Hiệu ứng nhấn
        await FloatingCallButton.ScaleTo(0.9, 50);
        await FloatingCallButton.ScaleTo(1.0, 50);

        // GỌI ĐIỆN
        try
        {
            PhoneDialer.Open("19001234");
        }
        catch
        {
            await DisplayAlert("Lỗi", "Thiết bị không hỗ trợ gọi điện", "OK");
        }
    }
    #endregion
}
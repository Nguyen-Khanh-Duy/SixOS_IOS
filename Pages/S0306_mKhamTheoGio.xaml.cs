using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Graphics;
using SixOSDatKhamAppMobile.Models;
using SixOSDatKhamAppMobile.Services.S0305;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SixOSDatKhamAppMobile.Pages
{
    public partial class S0306_mKhamTheoGioPage : ContentPage, INotifyPropertyChanged
    {
        #region Properties
        private readonly S0305_DKGoiKhamService _service;
        private ObservableCollection<KhungGioKham> _danhSachKhungGio = new();
        private ObservableCollection<KhungGioKham> _filteredKhungGio = new();
        private KhungGioKham _selectedKhungGio;
        private DateTime _selectedDate = DateTime.Today;
        private string _currentBuoiFilter = "Sang";
        private bool _isLoading = false;
        private long _selectedGoiKhamId = 0;
        private long _selectedChuyenGiaId = 0;
        private long _idGioiTinh = 0;
        private DateTime _currentTime = DateTime.Now;

        public ObservableCollection<KhungGioKham> FilteredKhungGio
        {
            get => _filteredKhungGio;
            set
            {
                _filteredKhungGio = value;
                OnPropertyChanged();
            }
        }

        public KhungGioKham SelectedKhungGio
        {
            get => _selectedKhungGio;
            set
            {
                _selectedKhungGio = value;
                OnPropertyChanged();
                UpdateSelectedInfo();
                UpdateXacNhanButton();
            }
        }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged();
                UpdateSelectedDateLabel();
                _currentTime = DateTime.Now; // Cập nhật thời gian hiện tại khi thay đổi ngày
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowLoadingOverlay));
                OnPropertyChanged(nameof(ShowContent));
            }
        }

        public bool ShowLoadingOverlay => IsLoading;
        public bool ShowContent => !IsLoading;
        #endregion

        #region Constructor
        public S0306_mKhamTheoGioPage()
        {
            InitializeComponent();
            _service = new S0305_DKGoiKhamService();
            BindingContext = this;
            UpdateSelectedDateLabel();
            UpdateFilterButtons();
            UpdateXacNhanButton();
            SetupSafeArea();
            Task.Run(async () => await LoadDataAsync());
        }

        public S0306_mKhamTheoGioPage(DateTime selectedDate) : this()
        {
            SelectedDate = selectedDate;
        }
        #endregion

        #region Private Methods
        private async Task LoadDataAsync()
        {
            try
            {
                MainThread.BeginInvokeOnMainThread(() => IsLoading = true);

                // Cập nhật thời gian hiện tại
                _currentTime = DateTime.Now;

                // Lấy thông tin từ SecureStorage
                var goiKhamIdStr = await SecureStorage.GetAsync("SelectedGoiKhamId");
                if (string.IsNullOrEmpty(goiKhamIdStr) || !long.TryParse(goiKhamIdStr, out _selectedGoiKhamId))
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await DisplayAlert("Lỗi", "Vui lòng chọn gói khám trước", "OK");
                        await Navigation.PopAsync();
                    });
                    return;
                }

                var chuyenGiaIdStr = await SecureStorage.GetAsync("SelectedChuyenGiaId");
                _selectedChuyenGiaId = !string.IsNullOrEmpty(chuyenGiaIdStr) && long.TryParse(chuyenGiaIdStr, out var cgId) ? cgId : 0;

                var ngayKhamStr = await SecureStorage.GetAsync("SelectedNgayKham");
                if (!string.IsNullOrEmpty(ngayKhamStr) && DateTime.TryParse(ngayKhamStr, out var ngayKham))
                {
                    SelectedDate = ngayKham;
                }

                // Lấy thông tin giới tính
                var doTuoiGioiTinhResult = await _service.LayDoTuoiGioiTinhBNAsync();
                if (!doTuoiGioiTinhResult.Success || doTuoiGioiTinhResult.Data == null)
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                        await DisplayAlert("Lỗi", doTuoiGioiTinhResult.Message ?? "Không thể lấy thông tin bệnh nhân", "OK"));
                    return;
                }

                _idGioiTinh = doTuoiGioiTinhResult.Data.GioiTinh ?? 0;

                // Gọi API lấy thời gian hẹn
                string ngayFormatted = SelectedDate.ToString("dd-MM-yyyy");
                var result = await _service.LayThoiGianHenAsync(ngayFormatted, _selectedChuyenGiaId, _idGioiTinh);

                if (!result.Success || result.Data == null)
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await DisplayAlert("Thông báo", result.Message ?? "Không có khung giờ khả dụng", "OK");
                        _danhSachKhungGio.Clear();
                        FilteredKhungGio = new ObservableCollection<KhungGioKham>(_danhSachKhungGio);
                        RenderKhungGioList();
                        IsLoading = false;
                    });
                    return;
                }

                var tempList = new ObservableCollection<KhungGioKham>();

                foreach (var item in result.Data)
                {
                    // Xác định buổi dựa vào IdBuoi
                    string buoi = item.IdBuoi == 1 ? "Sang" : "Chieu";

                    // Parse thời gian bắt đầu
                    TimeSpan thoiGianBatDau = item.ThoiGianBatDau ?? TimeSpan.Zero;

                    // Tạo DateTime từ ngày được chọn và thời gian bắt đầu
                    DateTime gioBatDauKhungGio = SelectedDate.Date.Add(thoiGianBatDau);

                    // Kiểm tra xem khung giờ này đã qua chưa (so với thời gian hiện tại)
                    bool daQuaThoiGian = gioBatDauKhungGio < _currentTime;

                    // Nếu trạng thái từ API là "còn chỗ" nhưng đã qua thời gian, thì đánh dấu là "đã qua"
                    string trangThai = item.TrangThai == 1
                        ? (daQuaThoiGian ? "DaQua" : "ConCho")
                        : "HetCho";

                    var khungGio = new KhungGioKham
                    {
                        Id = (int)item.ID,
                        Gio = item.ThoiGianBatDau.HasValue
                            ? item.ThoiGianBatDau.Value.ToString(@"hh\:mm")
                            : "",
                        Buoi = buoi,
                        TrangThai = trangThai,
                        SoLuongConLai = item.TrangThai == 1 ? 5 : 0,
                        Ngay = SelectedDate,
                        GiaTri = $"{SelectedDate:yyyy-MM-dd} {item.ThoiGianBatDau:hh\\:mm}",
                        GioBatDauDateTime = gioBatDauKhungGio,
                        DaQuaThoiGian = daQuaThoiGian
                    };

                    tempList.Add(khungGio);
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _danhSachKhungGio = tempList;
                    FilteredKhungGio = new ObservableCollection<KhungGioKham>(
                        _danhSachKhungGio.Where(k => k.Buoi == _currentBuoiFilter)
                    );

                    RenderKhungGioList();
                    UpdateSelectedInfo();
                    UpdateXacNhanButton();
                    IsLoading = false;
                });
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Lỗi", $"Không thể tải dữ liệu: {ex.Message}", "OK");
                    IsLoading = false;
                });
            }
        }

        private void UpdateSelectedDateLabel()
        {
            string dayOfWeek = SelectedDate.DayOfWeek switch
            {
                DayOfWeek.Monday => "Thứ Hai",
                DayOfWeek.Tuesday => "Thứ Ba",
                DayOfWeek.Wednesday => "Thứ Tư",
                DayOfWeek.Thursday => "Thứ Năm",
                DayOfWeek.Friday => "Thứ Sáu",
                DayOfWeek.Saturday => "Thứ Bảy",
                DayOfWeek.Sunday => "Chủ Nhật",
                _ => ""
            };

            SelectedDateLabel.Text = $"{dayOfWeek}, {SelectedDate:dd/MM/yyyy}";
        }

        private void UpdateFilterButtons()
        {
            BtnSang.BackgroundColor = _currentBuoiFilter == "Sang"
                ? Color.FromArgb("#1A73E8")
                : Color.FromArgb("#E2E8F0");
            BtnSang.TextColor = _currentBuoiFilter == "Sang" ? Colors.White : Color.FromArgb("#475569");

            BtnChieu.BackgroundColor = _currentBuoiFilter == "Chieu"
                ? Color.FromArgb("#1A73E8")
                : Color.FromArgb("#E2E8F0");
            BtnChieu.TextColor = _currentBuoiFilter == "Chieu" ? Colors.White : Color.FromArgb("#475569");
        }

        private void UpdateSelectedInfo()
        {
            if (SelectedKhungGio != null && SelectedKhungGio.TrangThai == "ConCho")
            {
                SelectedInfoContainer.IsVisible = true;
                string dayOfWeek = SelectedDate.DayOfWeek switch
                {
                    DayOfWeek.Monday => "Thứ Hai",
                    DayOfWeek.Tuesday => "Thứ Ba",
                    DayOfWeek.Wednesday => "Thứ Tư",
                    DayOfWeek.Thursday => "Thứ Năm",
                    DayOfWeek.Friday => "Thứ Sáu",
                    DayOfWeek.Saturday => "Thứ Bảy",
                    DayOfWeek.Sunday => "Chủ Nhật",
                    _ => ""
                };
                SelectedInfoLabel.Text = $"Đã chọn: {dayOfWeek}, {SelectedDate:dd/MM/yyyy} lúc {SelectedKhungGio.Gio}";
            }
            else
            {
                SelectedInfoContainer.IsVisible = false;
                SelectedInfoLabel.Text = "";
            }
        }

        private void UpdateXacNhanButton()
        {
            // Chỉ cho phép xác nhận nếu khung giờ còn chỗ và chưa qua thời gian
            BtnXacNhan.IsEnabled = SelectedKhungGio != null &&
                                  SelectedKhungGio.TrangThai == "ConCho" &&
                                  !SelectedKhungGio.DaQuaThoiGian;

            if (BtnXacNhan.IsEnabled)
            {
                BtnXacNhan.BackgroundColor = Color.FromArgb("#1A73E8");
                BtnXacNhan.TextColor = Colors.White;
            }
            else
            {
                BtnXacNhan.BackgroundColor = Color.FromArgb("#E2E8F0");
                BtnXacNhan.TextColor = Color.FromArgb("#94A3B8");
            }
        }

        private void RenderKhungGioList()
        {
            GioKhamGridContainer.Children.Clear();
            GioKhamGridContainer.RowDefinitions.Clear();
            GioKhamGridContainer.ColumnDefinitions.Clear();

            GioKhamGridContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            GioKhamGridContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            GioKhamGridContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            GioKhamGridContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

            GioKhamGridContainer.RowSpacing = 10;
            GioKhamGridContainer.ColumnSpacing = 10;

            int rowIndex = 0;
            int colIndex = 0;

            // Lọc các khung giờ còn chỗ và chưa qua thời gian
            var khungGioHienThi = FilteredKhungGio.ToList();

            foreach (var khungGio in khungGioHienThi)
            {
                if (colIndex >= 4)
                {
                    colIndex = 0;
                    rowIndex++;
                }

                if (rowIndex >= GioKhamGridContainer.RowDefinitions.Count)
                {
                    GioKhamGridContainer.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                }

                var frame = CreateKhungGioItem(khungGio);
                Grid.SetRow(frame, rowIndex);
                Grid.SetColumn(frame, colIndex);
                GioKhamGridContainer.Children.Add(frame);

                colIndex++;
            }

            // Đếm số khung giờ còn chỗ và chưa qua thời gian
            int soKhungGioConCho = khungGioHienThi.Count(k => k.TrangThai == "ConCho" && !k.DaQuaThoiGian);
            int soKhungGioDaQua = khungGioHienThi.Count(k => k.DaQuaThoiGian);
            int soKhungGioHetCho = khungGioHienThi.Count(k => k.TrangThai == "HetCho");

            bool hasAvailableSlots = soKhungGioConCho > 0;
            bool allSlotsPassed = soKhungGioConCho == 0 && soKhungGioDaQua > 0;
            bool allSlotsFull = soKhungGioHetCho == khungGioHienThi.Count;

            if (!hasAvailableSlots)
            {
                NoResultsContainer.IsVisible = true;
                var noResultsLabel = NoResultsContainer.Children.OfType<Label>().FirstOrDefault();
                if (noResultsLabel != null)
                {
                    if (allSlotsPassed)
                    {
                        noResultsLabel.Text = "Các khung giờ trong buổi này đã qua thời gian hiện tại";
                    }
                    else if (allSlotsFull)
                    {
                        noResultsLabel.Text = "Buổi này đã hết khung giờ trống";
                    }
                    else
                    {
                        noResultsLabel.Text = "Không có khung giờ khả dụng";
                    }
                }
            }
            else
            {
                NoResultsContainer.IsVisible = false;
            }

            GioKhamGridContainer.IsVisible = hasAvailableSlots;

            if (hasAvailableSlots)
            {
                SearchResultsLabel.Text = $"Có {soKhungGioConCho} khung giờ trống";
                SearchResultsLabel.IsVisible = true;
            }
            else
            {
                SearchResultsLabel.IsVisible = false;
            }
        }

        private Frame CreateKhungGioItem(KhungGioKham khungGio)
        {
            bool isSelected = khungGio.Id == SelectedKhungGio?.Id;
            bool isAvailable = khungGio.TrangThai == "ConCho" && !khungGio.DaQuaThoiGian;
            bool isPassedTime = khungGio.DaQuaThoiGian;
            bool isFull = khungGio.TrangThai == "HetCho";

            Color backgroundColor = Colors.White;
            Color borderColor = Color.FromArgb("#E2E8F0");
            Color textColor = Color.FromArgb("#1A73E8");
            float opacity = 1.0f;

            if (isSelected && isAvailable)
            {
                backgroundColor = Color.FromArgb("#F0F7FF");
                borderColor = Color.FromArgb("#1A73E8");
                textColor = Color.FromArgb("#1A73E8");
            }
            else if (!isAvailable)
            {
                if (isPassedTime)
                {
                    // Khung giờ đã qua thời gian
                    backgroundColor = Color.FromArgb("#F8FAFC");
                    borderColor = Color.FromArgb("#CBD5E1");
                    textColor = Color.FromArgb("#94A3B8");
                    opacity = 0.6f;
                }
                else if (isFull)
                {
                    // Khung giờ hết chỗ
                    backgroundColor = Color.FromArgb("#F8FAFC");
                    borderColor = Color.FromArgb("#CBD5E1");
                    textColor = Color.FromArgb("#94A3B8");
                    opacity = 0.7f;
                }
            }

            var frame = new Frame
            {
                BackgroundColor = backgroundColor,
                BorderColor = borderColor,
                CornerRadius = 8,
                HasShadow = false,
                Padding = 12,
                HeightRequest = 60,
                Opacity = opacity
            };

            var label = new Label
            {
                Text = khungGio.Gio,
                FontSize = 14,
                FontAttributes = isSelected ? FontAttributes.Bold : FontAttributes.None,
                TextColor = textColor,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            // Thêm indicator nếu khung giờ đã qua thời gian
            if (isPassedTime)
            {
                var stackLayout = new VerticalStackLayout
                {
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Spacing = 2
                };

                stackLayout.Children.Add(label);

                var indicatorLabel = new Label
                {
                    Text = "Đã qua",
                    FontSize = 10,
                    TextColor = Color.FromArgb("#DC2626"),
                    HorizontalOptions = LayoutOptions.Center
                };

                stackLayout.Children.Add(indicatorLabel);
                frame.Content = stackLayout;
            }
            else if (isFull)
            {
                var stackLayout = new VerticalStackLayout
                {
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Spacing = 2
                };

                stackLayout.Children.Add(label);

                var indicatorLabel = new Label
                {
                    Text = "Hết chỗ",
                    FontSize = 10,
                    TextColor = Color.FromArgb("#94A3B8"),
                    HorizontalOptions = LayoutOptions.Center
                };

                stackLayout.Children.Add(indicatorLabel);
                frame.Content = stackLayout;
            }
            else
            {
                frame.Content = label;
            }

            // Chỉ cho phép tap nếu khung giờ khả dụng
            if (isAvailable)
            {
                frame.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(() =>
                    {
                        SelectedKhungGio = khungGio;
                        RenderKhungGioList();
                    })
                });
            }

            return frame;
        }

        private void FilterKhungGio(string buoi)
        {
            _currentBuoiFilter = buoi;
            UpdateFilterButtons();

            var filtered = _danhSachKhungGio
                .Where(k => k.Buoi == buoi)
                .OrderBy(k => k.GioBatDauDateTime)
                .ToList();

            FilteredKhungGio = new ObservableCollection<KhungGioKham>(filtered);
            RenderKhungGioList();
            UpdateSelectedInfo();
            UpdateXacNhanButton();
        }

        /// Tạo và hiển thị modal xác nhận lịch hẹn cũ
        private async Task ShowLichHenCuModalAsync(LichHenCuData lichHenCu, string messageFromApi)
        {
            var thoiGian = lichHenCu?.Thoigian ?? "Không xác định";
            var tenGoi = lichHenCu?.TenGoi ?? "Gói khám";

            var stackLayout = new VerticalStackLayout
            {
                Spacing = 10,
                Padding = new Thickness(20),
                Children =
                {
                    new Label
                    {
                        Text = "Thông báo",
                        FontSize = 18,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#1A73E8"),
                        HorizontalOptions = LayoutOptions.Center
                    },
                    new Label
                    {
                        Text = messageFromApi ?? "Bạn đã có lịch hẹn trước đây",
                        FontSize = 14,
                        TextColor = Color.FromArgb("#1F2937"),
                        LineBreakMode = LineBreakMode.WordWrap
                    },
                    new Frame
                    {
                        BorderColor = Color.FromArgb("#E5E7EB"),
                        BackgroundColor = Color.FromArgb("#F3F4F6"),
                        CornerRadius = 8,
                        Padding = new Thickness(12),
                        HasShadow = false,
                        Content = new VerticalStackLayout
                        {
                            Spacing = 8,
                            Children =
                            {
                                new Label
                                {
                                    Text = $"Gói khám cũ: {tenGoi}",
                                    FontSize = 13,
                                    TextColor = Color.FromArgb("#374151")
                                },
                                new Label
                                {
                                    Text = $"Thời gian: {thoiGian}",
                                    FontSize = 13,
                                    TextColor = Color.FromArgb("#374151")
                                }
                            }
                        }
                    },
                    new Label
                    {
                        Text = "Bạn có muốn tiếp tục?",
                        FontSize = 13,
                        TextColor = Color.FromArgb("#6B7280"),
                        LineBreakMode = LineBreakMode.WordWrap
                    }
                }
            };

            var xacNhanButton = new Button
            {
                Text = "Xác Nhận",
                BackgroundColor = Color.FromArgb("#E53935"),
                TextColor = Colors.White,
                CornerRadius = 8,
                Padding = new Thickness(16, 12)
            };

            var huyButton = new Button
            {
                Text = "Huỷ",
                BackgroundColor = Color.FromArgb("#E2E8F0"),
                TextColor = Color.FromArgb("#475569"),
                CornerRadius = 8,
                Padding = new Thickness(16, 12)
            };

            var buttonLayout = new HorizontalStackLayout
            {
                Spacing = 12,
                HorizontalOptions = LayoutOptions.Center,
                Padding = new Thickness(20),
                Children = { huyButton, xacNhanButton }
            };

            var mainLayout = new VerticalStackLayout
            {
                Spacing = 20,
                Children = { stackLayout, buttonLayout }
            };

            var popup = new ContentPage
            {
                BackgroundColor = Color.FromArgb("#80000000"),
                Content = new Frame
                {
                    BackgroundColor = Colors.White,
                    CornerRadius = 12,
                    Padding = 20,
                    Margin = new Thickness(20),
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    Content = mainLayout
                }
            };

            bool isConfirmed = false;
            xacNhanButton.Clicked += async (s, e) =>
            {
                isConfirmed = true;
                await Navigation.PopModalAsync();
            };

            huyButton.Clicked += async (s, e) =>
            {
                isConfirmed = false;
                await Navigation.PopModalAsync();
            };

            await Navigation.PushModalAsync(popup);

            // đợi modal đóng
            while (Navigation.ModalStack.Count > 0)
            {
                await Task.Delay(100);
            }

            if (isConfirmed)
            {
                // tiếp tục quy trình đặt hẹn
                await SecureStorage.SetAsync("SelectedGioKham", SelectedKhungGio.Gio);
                await SecureStorage.SetAsync("SelectedKhungGioId", SelectedKhungGio.Id.ToString());
                var xoaDatHenCu = await _service.XoaDatHenAsync();
                await Navigation.PushAsync(new S0306_mXacNhan());
            }
        }
        #endregion

        #region Event Handlers
        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private void OnFilterBuoiClicked(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                string buoi = button.Text == "Buổi sáng" ? "Sang" : "Chieu";
                FilterKhungGio(buoi);
            }
        }

        private async void OnXacNhanClicked(object sender, EventArgs e)
        {
            if (SelectedKhungGio == null || SelectedKhungGio.TrangThai != "ConCho" || SelectedKhungGio.DaQuaThoiGian)
            {
                await DisplayAlert("Thông báo", "Vui lòng chọn một khung giờ khám hợp lệ", "OK");
                return;
            }

            try
            {
                IsLoading = true;

                var lichHenCuResult = await _service.KiemTraLichHenCuAsync();

                if (lichHenCuResult.Data != null && !lichHenCuResult.Success)
                {
                    // có lịch hẹn cũ (statusCode = 400)
                    IsLoading = false;
                    await ShowLichHenCuModalAsync(lichHenCuResult.Data, lichHenCuResult.Message);
                }
                else if (lichHenCuResult.Success)
                {
                    // không có lịch hẹn cũ (statusCode = 200), tiếp tục bình thường
                    await SecureStorage.SetAsync("SelectedGioKham", SelectedKhungGio.Gio);
                    await SecureStorage.SetAsync("SelectedKhungGioId", SelectedKhungGio.Id.ToString());
                    IsLoading = false;
                    await Navigation.PushAsync(new S0306_mXacNhan());
                }
                else
                {
                    await DisplayAlert("Lỗi", lichHenCuResult.Message ?? "Không thể kiểm tra lịch hẹn", "OK");
                    IsLoading = false;
                }
            }
            catch (Exception ex)
            {
                IsLoading = false;
                await DisplayAlert("Lỗi", $"Có lỗi xảy ra: {ex.Message}", "OK");
            }
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.GetUseSafeArea(this))
            {
                // Sử dụng safe area
                On<iOS>().SetUseSafeArea(true);
            }

            // Cập nhật thời gian hiện tại khi page xuất hiện
            _currentTime = DateTime.Now;

            // Nếu ngày được chọn là hôm nay, cần cập nhật lại danh sách khung giờ
            if (SelectedDate.Date == DateTime.Today)
            {
                Task.Run(async () => await LoadDataAsync());
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
    }

    #region Model Classes
    public class KhungGioKham
    {
        public int Id { get; set; }
        public string Gio { get; set; }
        public string Buoi { get; set; }
        public string TrangThai { get; set; }
        public int SoLuongConLai { get; set; }
        public DateTime Ngay { get; set; }
        public string GiaTri { get; set; }
        public DateTime GioBatDauDateTime { get; set; }
        public bool DaQuaThoiGian { get; set; }
    }
    #endregion
}
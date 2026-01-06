using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using SixOSDatKhamAppMobile.Models;
using SixOSDatKhamAppMobile.Services.S0305;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace SixOSDatKhamAppMobile.Pages
{
    public partial class S0306_mKhamTheoGoi : ContentPage, INotifyPropertyChanged
    {
        #region Properties
        private readonly S0305_DKGoiKhamService _service;
        private ObservableCollection<GoiKham> _danhSachGoiKham = new();
        private ObservableCollection<GoiKham> _filteredGoiKham = new();
        private GoiKham _selectedGoiKham;
        private string _currentSearchText = "";
        private bool _isLoading = false;
        private bool _isFiltering = false;
        private bool _isSearching = false;
        private bool _isLoadingMore = false;

        // Pagination
        private int _currentPage = 1;
        private const int _pageSize = 3;
        private bool _hasMoreData = true;
        private bool _isInitialLoad = true;

        // Thông tin bệnh nhân
        private int _tuoiBenhNhan = 0;
        private long _idGioiTinh = 0;

        // Biến lọc
        private string _selectedGender = "Tất cả";
        private string _selectedAge = "Tất cả";
        private string _selectedType = "Tất cả";

        public ObservableCollection<GoiKham> FilteredGoiKham
        {
            get => _filteredGoiKham;
            set
            {
                _filteredGoiKham = value;
                OnPropertyChanged();
            }
        }
        #region Properties
        // Thêm thuộc tính mới để xác định flow
        private bool _isFromChuyenGiaFlow = false;
        public bool IsFromChuyenGiaFlow
        {
            get => _isFromChuyenGiaFlow;
            set

            {
                _isFromChuyenGiaFlow = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentStepIndex));
                OnPropertyChanged(nameof(ShowChuyenGiaAsStep1));
                OnPropertyChanged(nameof(ShowGoiKhamAsStep2));
            }
        }

        public int CurrentStepIndex => IsFromChuyenGiaFlow ? 2 : 1;
        public bool ShowChuyenGiaAsStep1 => IsFromChuyenGiaFlow;
        public bool ShowGoiKhamAsStep2 => IsFromChuyenGiaFlow;
        #endregion

        #region Constructor
        // Thêm constructor có tham số
        public S0306_mKhamTheoGoi(bool isFromChuyenGiaFlow = false)
        {
            InitializeComponent();
            _service = new S0305_DKGoiKhamService();
            BindingContext = this;
            ResetFilters();

            // Thiết lập flow
            IsFromChuyenGiaFlow = isFromChuyenGiaFlow;

            // Load dữ liệu
            Task.Run(async () => await LoadDataAsync());
        }

        // Giữ lại constructor không tham số để tương thích
        //public S0306_mKhamTheoGoi() : this(false)
        //{
        //}

        protected override bool OnBackButtonPressed()
        {
            try
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        SecureStorage.Remove("SelectedChuyenGiaId");
                        SecureStorage.Remove("SelectedChuyenGiaName");

                        // Chuyển trang an toàn
                        if (Shell.Current != null)
                        {
                            await Shell.Current.GoToAsync("..");
                        }
                        else if (Navigation != null)
                        {
                            await Navigation.PopAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                       
                        // Vẫn cho phép quay lại
                        if (Navigation != null)
                        {
                            await Navigation.PopAsync();
                        }
                    }
                });
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ngoại lệ chuyển trang: {ex.Message}");
                return false;
            }
        }
        #endregion
        public GoiKham SelectedGoiKham
        {
            get => _selectedGoiKham;
            set
            {
                _selectedGoiKham = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedGoiKham));
            }
        }

        public bool HasSelectedGoiKham => SelectedGoiKham != null;

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

        public bool IsFiltering
        {
            get => _isFiltering;
            set
            {
                _isFiltering = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowFilterLoading));
                OnPropertyChanged(nameof(ShowFilterContent));
            }
        }

        public bool IsSearching
        {
            get => _isSearching;
            set
            {
                _isSearching = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowSearchLoading));
            }
        }

        public bool IsLoadingMore
        {
            get => _isLoadingMore;
            set
            {
                _isLoadingMore = value;
                OnPropertyChanged();
            }
        }

        public bool ShowLoadingOverlay => IsLoading;
        public bool ShowContent => !IsLoading;
        public bool ShowFilterLoading => IsFiltering;
        public bool ShowFilterContent => !IsFiltering;
        public bool ShowSearchLoading => IsSearching;
        #endregion

        #region Helper Methods

        private string ConvertHtmlToPlainText(string html, int maxLength = 0)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            try
            {
                html = Regex.Replace(html, @"<script[^>]*>[\s\S]*?</script>", "", RegexOptions.IgnoreCase);
                html = Regex.Replace(html, @"<style[^>]*>[\s\S]*?</style>", "", RegexOptions.IgnoreCase);
                html = html.Replace("<br>", "\n")
                          .Replace("<br/>", "\n")
                          .Replace("<br />", "\n")
                          .Replace("</p>", "\n\n")
                          .Replace("</div>", "\n");
                html = Regex.Replace(html, "<.*?>", " ", RegexOptions.Singleline);
                html = System.Net.WebUtility.HtmlDecode(html);
                html = Regex.Replace(html, @"\s+", " ");
                html = html.Trim();

                if (maxLength > 0 && html.Length > maxLength)
                {
                    html = html.Substring(0, Math.Min(maxLength, html.Length)) + "...";
                }

                return html;
            }
            catch
            {
                return maxLength > 0 && html.Length > maxLength
                    ? html.Substring(0, Math.Min(maxLength, html.Length)) + "..."
                    : html;
            }
        }

        private async Task ShowMoTaDetail(string title, string htmlContent)
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
                new RowDefinition { Height = GridLength.Auto }, // Thanh tiêu đề
                new RowDefinition { Height = GridLength.Star }, // WebView
                new RowDefinition { Height = GridLength.Auto }  // Nút đóng
            },
                    BackgroundColor = Colors.White,
                    RowSpacing = 0
                };
                Grid.SetRow(whiteContainer, 1);
                mainGrid.Children.Add(whiteContainer);

                // Tạo thanh tiêu đề "HƯỚNG DẪN"
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

                // Tạo WebView
                var webView = new WebView
                {
                    Source = new HtmlWebViewSource { Html = fullHtml },
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    BackgroundColor = Colors.White
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

                // Thêm các control vào whiteContainer
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

        #endregion

        #region Constructor
        public S0306_mKhamTheoGoi()
        {
            InitializeComponent();
            _service = new S0305_DKGoiKhamService();
            BindingContext = this;
            ResetFilters();

            // Setup scroll listener cho lazy loading
            SetupScrollListener();
            SetupSafeArea();
            // Load dữ liệu
            Task.Run(async () => await LoadDataAsync());
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
        private void SetupScrollListener()
        {
            var scrollView = this.FindByName<Microsoft.Maui.Controls.ScrollView>("MainScrollView");
            if (scrollView != null)
            {
                scrollView.Scrolled += async (s, e) =>
                {
                    var scrollY = e.ScrollY;
                    var contentHeight = scrollView.ContentSize.Height;
                    var scrollViewHeight = scrollView.Height;

                    // Khi scroll đến 80% chiều cao, load thêm
                    if (scrollY + scrollViewHeight >= contentHeight * 0.8)
                    {
                        await LoadMoreGoiKhamAsync();
                    }
                };
            }
        }
        #endregion

        #region Private Methods
        private async Task LoadDataAsync()
        {
            try
            {
                MainThread.BeginInvokeOnMainThread(() => IsLoading = true);

                // Bước 1: Lấy thông tin tuổi và giới tính bệnh nhân
                var doTuoiGioiTinhResult = await _service.LayDoTuoiGioiTinhBNAsync();

                if (!doTuoiGioiTinhResult.Success || doTuoiGioiTinhResult.Data == null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                        DisplayAlert("Lỗi", doTuoiGioiTinhResult.Message ?? "Không thể lấy thông tin bệnh nhân", "OK"));
                    return;
                }

                _tuoiBenhNhan = doTuoiGioiTinhResult.Data.Tuoi;
                _idGioiTinh = doTuoiGioiTinhResult.Data.GioiTinh ?? 0;

                // Bước 2: Lấy batch đầu tiên của gói khám
                _currentPage = 1;
                _hasMoreData = true;
                _isInitialLoad = true;
                await LoadGoiKhamAsync();
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                    DisplayAlert("Lỗi", $"Không thể tải dữ liệu: {ex.Message}", "OK"));
            }
            finally
            {
                MainThread.BeginInvokeOnMainThread(() => IsLoading = false);
            }
        }

        private async Task LoadGoiKhamAsync(long? loaiGoi = 0)
        {
            try
            {
                // Pagination: Lấy dữ liệu theo trang
                var result = await _service.GetGoiChiDinhTheoTuoiAsync(
                    _tuoiBenhNhan,
                    (int)_idGioiTinh,
                    loaiGoi,
                    _currentPage,
                    _pageSize);

                if (!result.Success || result.Data == null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        if (_isInitialLoad)
                        {
                            DisplayAlert("Thông báo", result.Message ?? "Không có gói khám phù hợp", "OK");
                            _danhSachGoiKham.Clear();
                            ApplyFilters();
                        }
                        _hasMoreData = false;
                    });
                    return;
                }

                // Chuyển đổi dữ liệu từ API sang model GoiKham
                var tempList = new List<GoiKham>();
                foreach (var item in result.Data)
                {
                    var goiKham = new GoiKham
                    {
                        Id = item.Id,
                        TenGoiKham = item.TenGoi ?? "",
                        MoTa = ConvertHtmlToPlainText(item.HuongDan ?? "", 150),
                        HuongDanHtml = item.HuongDan ?? "",
                        GiaTien = item.TongTien,
                        ThoiGianHieuLuc = 30,
                        DanhMuc = item.GoiCoBan == true ? "Cơ bản" : (item.GoiNangCao == true ? "Nâng cao" : "Chuyên sâu"),
                        GioiTinh = _idGioiTinh switch
                        {
                            1 => "Nam",
                            2 => "Nữ",
                            _ => "Tất cả"
                        },
                        DoTuoi = _tuoiBenhNhan < 45 ? "Dưới 45" : "Trên 45",
                        LoaiGoi = item.GoiCoBan == true ? "Cơ bản" : (item.GoiNangCao == true ? "Nâng cao" : "Chuyên sâu"),
                        BadgeText = item.GoiCoBan == true && item.Stt == 1 ? "GÓI CƠ BẢN" :
                                   (item.GoiNangCao == true ? "GÓI NÂNG CAO" : ""),
                        BadgeColor = item.GoiCoBan == true && item.Stt == 1 ? "#FF6B35" :
                                    (item.GoiNangCao == true ? "#10B981" : "#8B5CF6"),
                        CacXetNghiem = item.ChiTiet?
                                        .Select(ct => ct.TenDichVu?.ToString() ?? "")
                                        .ToList() ?? new List<string>()
                    };

                    tempList.Add(goiKham);
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (_isInitialLoad)
                    {
                        _danhSachGoiKham = new ObservableCollection<GoiKham>(tempList);
                        _isInitialLoad = false;
                    }
                    else
                    {
                        foreach (var item in tempList)
                        {
                            _danhSachGoiKham.Add(item);
                        }
                    }

                    // Kiểm tra còn dữ liệu không
                    _hasMoreData = tempList.Count >= _pageSize;

                    ApplyFilters();
                });
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                    DisplayAlert("Lỗi", $"Không thể tải gói khám: {ex.Message}", "OK"));
            }
        }

        private async Task LoadMoreGoiKhamAsync()
        {
            // Tránh load nhiều lần cùng lúc
            if (_isLoadingMore || !_hasMoreData || _isLoading)
                return;

            try
            {
                MainThread.BeginInvokeOnMainThread(() => IsLoadingMore = true);

                _currentPage++;
                await LoadGoiKhamAsync(
                    _selectedType switch
                    {
                        "Cơ bản" => 1,
                        "Nâng cao" => 2,
                        _ => 0
                    }
                );
            }
            finally
            {
                MainThread.BeginInvokeOnMainThread(() => IsLoadingMore = false);
            }
        }

        private void ResetFilters()
        {
            _selectedGender = "Tất cả";
            _selectedAge = "Tất cả";
            _selectedType = "Tất cả";
            UpdateFilterUI();
        }

        private void UpdateFilterUI()
        {
            // Giới tính
            UpdateBorderState(GenderAllBorder, _selectedGender == "Tất cả");
            UpdateBorderState(GenderMaleBorder, _selectedGender == "Nam");
            UpdateBorderState(GenderFemaleBorder, _selectedGender == "Nữ");

            // Độ tuổi
            UpdateBorderState(AgeUnder45Border, _selectedAge == "Dưới 45");
            UpdateBorderState(AgeOver45Border, _selectedAge == "Trên 45");

            // Loại gói
            UpdateBorderState(TypeAllBorder, _selectedType == "Tất cả");
            UpdateBorderState(TypeBasicBorder, _selectedType == "Cơ bản");
            UpdateBorderState(TypeAdvancedBorder, _selectedType == "Nâng cao");
        }

        private void UpdateBorderState(Border border, bool isSelected)
        {
            if (border.Content is Label label)
            {
                border.BackgroundColor = isSelected ? Color.FromArgb("#1A73E8") : Colors.White;
                border.Stroke = isSelected ? Color.FromArgb("#1A73E8") : Color.FromArgb("#E2E8F0");
                label.TextColor = isSelected ? Colors.White : Color.FromArgb("#475569");
                label.FontAttributes = isSelected ? FontAttributes.Bold : FontAttributes.None;
            }
        }

        private void ApplyFilters()
        {
            try
            {
                IsFiltering = true;

                var filtered = _danhSachGoiKham.AsEnumerable();

                // Lọc theo giới tính
                if (_selectedGender != "Tất cả")
                {
                    filtered = filtered.Where(g => g.GioiTinh == _selectedGender || g.GioiTinh == "Tất cả");
                }

                // Lọc theo độ tuổi
                if (_selectedAge != "Tất cả")
                {
                    filtered = filtered.Where(g => g.DoTuoi == _selectedAge || g.DoTuoi == "Tất cả");
                }

                // Lọc theo loại gói
                if (_selectedType != "Tất cả")
                {
                    filtered = filtered.Where(g => g.LoaiGoi == _selectedType);
                }

                // Lọc theo tìm kiếm
                if (!string.IsNullOrWhiteSpace(_currentSearchText))
                {
                    var searchText = _currentSearchText.ToLower();
                    filtered = filtered.Where(g =>
                        g.TenGoiKham.ToLower().Contains(searchText) ||
                        g.MoTa.ToLower().Contains(searchText) ||
                        g.DanhMuc.ToLower().Contains(searchText));
                }

                FilteredGoiKham = new ObservableCollection<GoiKham>(filtered);

                RenderGoiKhamList();
                IsFiltering = false;
            }
            catch (Exception ex)
            {
                IsFiltering = false;
                Console.WriteLine($"Lỗi khi áp dụng filter: {ex.Message}");
            }
        }

        private void RenderGoiKhamList()
        {
            try
            {
                // 1. Tạo tất cả items trước khi gán vào container
                var itemsToAdd = new List<View>();
                bool needsToClearContainer = true;

                // 2. Kiểm tra nếu chỉ cần thêm item mới (load more)
                if (_isLoadingMore && _hasMoreData && GoiKhamListContainer.Children.Count > 0)
                {
                    // Khi load more, không xóa items cũ
                    needsToClearContainer = false;

                    // Chỉ lấy items mới (từ vị trí hiện tại đến hết)
                    int startIndex = FilteredGoiKham.Count - _pageSize; // Giả sử _pageSize là số items mỗi trang
                    if (startIndex < 0) startIndex = 0;

                    for (int i = startIndex; i < FilteredGoiKham.Count; i++)
                    {
                        var border = CreateGoiKhamItem(FilteredGoiKham[i]);
                        itemsToAdd.Add(border);
                    }
                }
                else
                {
                    // Render tất cả items
                    foreach (var goiKham in FilteredGoiKham)
                    {
                        var border = CreateGoiKhamItem(goiKham);
                        itemsToAdd.Add(border);
                    }
                }

                // 3. Batch update container để tránh render nhiều lần
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (needsToClearContainer)
                    {
                        GoiKhamListContainer.Children.Clear();
                    }

                    // Thêm tất cả items cùng lúc
                    foreach (var item in itemsToAdd)
                    {
                        GoiKhamListContainer.Children.Add(item);
                    }

                    // 4. Thêm loading indicator nếu cần
                    if (_isLoadingMore && _hasMoreData)
                    {
                        // Xóa loading indicator cũ nếu có
                        RemoveExistingLoadingIndicator();

                        var loadingIndicator = new ActivityIndicator
                        {
                            IsRunning = true,
                            Color = Color.FromArgb("#1A73E8"),
                            Margin = new Thickness(0, 20, 0, 20),
                            HorizontalOptions = LayoutOptions.Center
                        };
                        GoiKhamListContainer.Children.Add(loadingIndicator);
                    }
                    else
                    {
                        RemoveExistingLoadingIndicator();
                    }

                    // 5. Cập nhật visibility
                    UpdateVisibility();

                    // 6. Cập nhật search results label
                    UpdateSearchResultsLabel();
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RenderGoiKhamList: {ex.Message}");
            }
        }

        private void RemoveExistingLoadingIndicator()
        {
            // Tìm và xóa loading indicator cũ
            var existingLoading = GoiKhamListContainer.Children
                .FirstOrDefault(c => c is ActivityIndicator indicator && indicator.IsRunning);

            if (existingLoading != null)
            {
                GoiKhamListContainer.Children.Remove(existingLoading);
            }
        }

        private void UpdateVisibility()
        {
            bool hasResults = FilteredGoiKham.Any();
            GoiKhamListContainer.IsVisible = hasResults;
            NoResultsContainer.IsVisible = !hasResults;
        }

        private void UpdateSearchResultsLabel()
        {
            if (!string.IsNullOrEmpty(_currentSearchText) && FilteredGoiKham.Any())
            {
                SearchResultsLabel.Text = $"Tìm thấy {FilteredGoiKham.Count} kết quả cho '{_currentSearchText}'";
                SearchResultsLabel.IsVisible = true;
            }
            else
            {
                SearchResultsLabel.IsVisible = false;
            }
        }

        private Border CreateGoiKhamItem(GoiKham goiKham)
        {
            bool isSelected = goiKham.Id == SelectedGoiKham?.Id;

            var border = new Border
            {
                Stroke = isSelected ? Color.FromArgb("#1A73E8") : Color.FromArgb("#E2E8F0"),
                StrokeThickness = isSelected ? 3 : 1,
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                BackgroundColor = isSelected ? Color.FromArgb("#EBF5FF") : Colors.White,
                Padding = 0,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var grid = new Grid
            {
                RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                },
                Padding = 20
            };

            // Hàng 1: Tiêu đề và badge
            var headerGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };

            var titleStack = new VerticalStackLayout { Spacing = 5 };

            // Badge nếu có
            if (!string.IsNullOrEmpty(goiKham.BadgeText))
            {
                var badgeFrame = new Frame
                {
                    BackgroundColor = Color.FromArgb(goiKham.BadgeColor),
                    Padding = new Thickness(8, 4),
                    CornerRadius = 4,
                    HorizontalOptions = LayoutOptions.Start,
                    HasShadow = false
                };

                badgeFrame.Content = new Label
                {
                    Text = goiKham.BadgeText,
                    FontSize = 11,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.White
                };
                titleStack.Children.Add(badgeFrame);
            }

            titleStack.Children.Add(new Label
            {
                Text = goiKham.TenGoiKham,
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#1E293B"),
                LineBreakMode = LineBreakMode.WordWrap
            });

            Grid.SetColumn(titleStack, 0);
            headerGrid.Children.Add(titleStack);

            grid.Children.Add(headerGrid);
            Grid.SetRow(headerGrid, 0);

            // Hàng 2: Mô tả
            var descriptionLabel = new Label
            {
                Text = goiKham.MoTa,
                FontSize = 13,
                TextColor = Color.FromArgb("#475569"),
                LineBreakMode = LineBreakMode.WordWrap,
                Margin = new Thickness(0, 8, 0, 0)
            };
            grid.Children.Add(descriptionLabel);
            Grid.SetRow(descriptionLabel, 1);

            // Hàng 3: Icon loại gói
            var typeGenderGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star }
                },
                ColumnSpacing = 8,
                Margin = new Thickness(0, 12, 0, 0)
            };

            var typeStack = new HorizontalStackLayout { Spacing = 6 };

            typeStack.Children.Add(new Image
            {
                Source = "tt1.png",
                WidthRequest = 16,
                HeightRequest = 16,
                VerticalOptions = LayoutOptions.Center
            });

            typeStack.Children.Add(new Label
            {
                Text = goiKham.LoaiGoi,
                FontSize = 13,
                TextColor = Color.FromArgb("#475569")
            });

            Grid.SetColumn(typeStack, 0);
            typeGenderGrid.Children.Add(typeStack);

            grid.Children.Add(typeGenderGrid);
            Grid.SetRow(typeGenderGrid, 2);

            // Hàng 4: Hướng dẫn + Xem chi tiết
            var linksGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star }
                },
                ColumnSpacing = 10,
                Margin = new Thickness(0, 12, 0, 0)
            };

            var huongDanLabel = new Label
            {
                Text = "Xem hướng dẫn",
                FontSize = 13,
                TextColor = Color.FromArgb("#1A73E8"),
                TextDecorations = TextDecorations.Underline,
                HorizontalOptions = LayoutOptions.Start
            };
            huongDanLabel.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => OnXemHuongDanClicked(goiKham))
            });

            var chiTietLabel = new Label
            {
                Text = "Xem chi tiết",
                FontSize = 13,
                TextColor = Color.FromArgb("#1A73E8"),
                TextDecorations = TextDecorations.Underline,
                HorizontalOptions = LayoutOptions.End
            };
            chiTietLabel.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => OnXemChiTietClicked(goiKham))
            });

            Grid.SetColumn(huongDanLabel, 0);
            Grid.SetColumn(chiTietLabel, 1);
            linksGrid.Children.Add(huongDanLabel);
            linksGrid.Children.Add(chiTietLabel);

            grid.Children.Add(linksGrid);
            Grid.SetRow(linksGrid, 3);

            // Hàng 5: Giá + Nút chọn
            var bottomGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                Margin = new Thickness(0, 15, 0, 0)
            };

            var priceLabel = new Label
            {
                Text = $"{goiKham.GiaTien:N0} đ",
                FontSize = 20,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#FF6B35"),
                VerticalOptions = LayoutOptions.Center
            };
            Grid.SetColumn(priceLabel, 0);
            bottomGrid.Children.Add(priceLabel);

            var chonButton = new Button
            {
                Text = isSelected ? "ĐÃ CHỌN ✓" : "CHỌN GÓI NÀY",
                BackgroundColor = isSelected ? Color.FromArgb("#10B981") : Color.FromArgb("#1A73E8"),
                TextColor = Colors.White,
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                CornerRadius = 20,
                HeightRequest = 40,
                WidthRequest = 160,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Center,
                Padding = new Thickness(20, 0)
            };

            chonButton.Clicked += async (s, e) =>
            {
                await OnChonGoiKhamClicked(goiKham);
            };

            Grid.SetColumn(chonButton, 1);
            bottomGrid.Children.Add(chonButton);

            grid.Children.Add(bottomGrid);
            Grid.SetRow(bottomGrid, 4);

            border.Content = grid;
            return border;
        }

        private async void OnXemHuongDanClicked(GoiKham goiKham)
        {
            if (!string.IsNullOrEmpty(goiKham.HuongDanHtml))
            {
                await ShowMoTaDetail("Hướng dẫn sử dụng", goiKham.HuongDanHtml);
            }
            else
            {
                await DisplayAlert("Hướng dẫn sử dụng",
                    !string.IsNullOrEmpty(goiKham.MoTa) ? goiKham.MoTa : "Không có hướng dẫn",
                    "Đóng");
            }
        }

        private async void OnXemChiTietClicked(GoiKham goiKham)
        {
            try
            {
                var result = await _service.LayGoiKhamChiTietAsync(goiKham.Id);

                if (result.Success && result.Data != null)
                {
                    await ShowDetailModal(goiKham, result.Data);
                }
                else
                {
                    await DisplayAlert("Thông báo", "Không có chi tiết cho gói khám này", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", $"Không thể tải chi tiết: {ex.Message}", "OK");
            }
        }

        private async Task ShowDetailModal(GoiKham goiKham, dynamic apiData)
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
                new RowDefinition { Height = 40 }, // Hàng đen 40px
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

                // Tạo ScrollView và nội dung
                var scrollView = new Microsoft.Maui.Controls.ScrollView
                {
                    Padding = new Thickness(20)
                };

                var contentStack = new VerticalStackLayout
                {
                    Spacing = 20
                };

                // Tạo frame thông tin gói khám
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
                    BackgroundColor = Colors.White,
                    TextColor = Color.FromArgb("#1A73E8"),
                    FontFamily = "Roboto-Bold"
                });

                infoStack.Children.Add(CreateInfoRow("Tên gói", goiKham.TenGoiKham));
                infoStack.Children.Add(CreateInfoRow("Giá tiền", $"{goiKham.GiaTien:N0} VNĐ"));
                infoStack.Children.Add(CreateInfoRow("Danh mục", goiKham.DanhMuc));
                infoStack.Children.Add(CreateInfoRow("Loại gói", goiKham.LoaiGoi));

                infoFrame.Content = infoStack;
                contentStack.Children.Add(infoFrame);

                // Tạo danh sách dịch vụ nếu có
                var chiTietList = apiData?.ChiTiet ?? new List<dynamic>();
                if (chiTietList != null && chiTietList.Count > 0)
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
                        BackgroundColor = Colors.White,
                        TextColor = Color.FromArgb("#1A73E8"),
                        FontFamily = "Roboto-Bold"
                    });

                    // Tạo Grid cho bảng
                    var tableGrid = new Grid
                    {
                        ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = 50 }, // Cột STT
                    new ColumnDefinition { Width = GridLength.Star } // Cột DỊCH VỤ
                },
                        RowSpacing = 0,
                        Margin = new Thickness(0, 0, 0, 0)
                    };

                    // Thêm hàng đầu tiên cho tiêu đề
                    tableGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    // Tạo tiêu đề
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

                    // Thêm header vào Grid
                    Grid.SetRow(headerBackground, 0);
                    Grid.SetColumnSpan(headerBackground, 2);
                    tableGrid.Children.Add(headerBackground);

                    Grid.SetRow(sttHeader, 0);
                    Grid.SetColumn(sttHeader, 0);
                    tableGrid.Children.Add(sttHeader);

                    Grid.SetRow(dichVuHeader, 0);
                    Grid.SetColumn(dichVuHeader, 1);
                    tableGrid.Children.Add(dichVuHeader);

                    // Thêm các hàng dữ liệu
                    int stt = 1;
                    int rowIndex = 1;

                    foreach (var item in chiTietList)
                    {
                        string tenDichVu = item.TenDichVu?.ToString() ?? "Đang cập nhật...";

                        // Thêm định nghĩa hàng mới
                        tableGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                        // Tạo nền xen kẽ
                        var rowBackground = new BoxView
                        {
                            Color = rowIndex % 2 == 0 ? Color.FromArgb("#F9FAFB") : Colors.White
                        };

                        Grid.SetRow(rowBackground, rowIndex);
                        Grid.SetColumnSpan(rowBackground, 2);
                        tableGrid.Children.Add(rowBackground);

                        // Tạo label STT
                        var sttLabel = new Label
                        {
                            Text = stt.ToString(),
                            FontSize = 14,
                            TextColor = Color.FromArgb("#374151"),
                            HorizontalOptions = LayoutOptions.Center,
                            VerticalOptions = LayoutOptions.Center,
                            Padding = new Thickness(0, 12)
                        };

                        // Tạo label Dịch vụ
                        var dichVuLabel = new Label
                        {
                            Text = tenDichVu,
                            FontSize = 14,
                            TextColor = Color.FromArgb("#1F2937"),
                            VerticalOptions = LayoutOptions.Center,
                            Padding = new Thickness(15, 12, 15, 12),
                            LineBreakMode = LineBreakMode.WordWrap
                        };

                        // Thêm vào Grid
                        Grid.SetRow(sttLabel, rowIndex);
                        Grid.SetColumn(sttLabel, 0);
                        tableGrid.Children.Add(sttLabel);

                        Grid.SetRow(dichVuLabel, rowIndex);
                        Grid.SetColumn(dichVuLabel, 1);
                        tableGrid.Children.Add(dichVuLabel);

                        // Thêm đường kẻ ngăn cách (trừ hàng cuối)
                        if (rowIndex < chiTietList.Count)
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

                        stt++;
                        rowIndex++;
                    }

                    // Thêm border cho toàn bộ bảng
                    var tableFrame = new Frame
                    {
                        BackgroundColor = Colors.White,
                        BorderColor = Color.FromArgb("#E5E7EB"),
                        CornerRadius = 8,
                        Padding = 0,
                        HasShadow = true,
                        Shadow = new Shadow
                        {
                            Brush = Brush.Black,
                            Opacity = 0.1f,
                            Radius = 5,
                            Offset = new Point(0, 2)
                        },
                        Content = tableGrid
                    };

                    // Thêm bảng vào servicesStack
                    servicesStack.Children.Add(tableFrame);
                    servicesFrame.Content = servicesStack;
                    contentStack.Children.Add(servicesFrame);
                }

                scrollView.Content = contentStack;

                // Tạo nút đóng
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

                // Thêm vào whiteContainer
                whiteContainer.Children.Add(scrollView);
                Grid.SetRow(scrollView, 0);

                whiteContainer.Children.Add(closeButton);
                Grid.SetRow(closeButton, 1);

                // Đặt nội dung cho page
                detailPage.Content = mainGrid;

                await Navigation.PushModalAsync(detailPage);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", $"Không thể hiển thị chi tiết: {ex.Message}", "OK");
            }
        }

        // Phương thức helper tạo dòng thông tin
        private View CreateInfoRow(string title, string value)
        {
            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
        {
            new ColumnDefinition { Width = 120 },
            new ColumnDefinition { Width = GridLength.Star }
        }
            };

            grid.Children.Add(new Label
            {
                Text = $"{title}:",
                FontSize = 14,
                TextColor = Color.FromArgb("#4B5563"),
                FontAttributes = FontAttributes.Bold
            });

            grid.Children.Add(new Label
            {
                Text = value ?? "N/A",
                FontSize = 14,
                TextColor = Color.FromArgb("#1F2937"),
                VerticalOptions = LayoutOptions.Center
            });
            Grid.SetColumn((Label)grid.Children[1], 1);

            return grid;
        }
        private Grid CreateServiceRow(int stt, string serviceName)
        {
            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = new GridLength(40) },
                    new ColumnDefinition { Width = GridLength.Star }
                }
            };

            grid.Children.Add(new Label
            {
                Text = stt + ".",
                FontSize = 14,
                TextColor = Color.FromArgb("#64748B")
            });

            grid.Children.Add(new Label
            {
                Text = serviceName,
                FontSize = 14,
                TextColor = Color.FromArgb("#1E293B"),
                LineBreakMode = LineBreakMode.WordWrap
            });
            Grid.SetColumn((View)grid.Children[1], 1);

            return grid;
        }

        private async Task OnChonGoiKhamClicked(GoiKham goiKham)
        {
            try
            {
                // Sử dụng Task.Run để thực hiện các tác vụ I/O song song
                var saveTasks = new List<Task>
                {
                    SecureStorage.SetAsync("SelectedGoiKhamId", goiKham.Id.ToString()),
                    SecureStorage.SetAsync("SelectedGoiKhamName", goiKham.TenGoiKham),
                    SecureStorage.SetAsync("SelectedGoiKhamPrice", goiKham.GiaTien.ToString()),
                    SecureStorage.SetAsync("SelectedGoiKhamLoai", goiKham.LoaiGoi)
                };

                // Chạy song song tất cả các tác vụ lưu trữ
                await Task.WhenAll(saveTasks);

                // Cập nhật UI trên MainThread
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    SelectedGoiKham = goiKham;
                    RenderGoiKhamList();
                });

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Navigation.PushAsync(new S0306_mKhamTheoChuyenGia());
                });

            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", $"Không thể chọn gói khám: {ex.Message}", "OK");
            }
        }

        #endregion

        #region Event Handlers
        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            await SecureStorage.SetAsync("SelectedChuyenGiaId", "");
            await SecureStorage.SetAsync("SelectedChuyenGiaName", "");
            await Navigation.PopAsync();
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            _currentSearchText = e.NewTextValue;

            _searchDebounceTimer?.Dispose();

            _searchDebounceTimer = new Timer(_ =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ApplyFilters();
                });
            }, null, 500, Timeout.Infinite);
        }

        private System.Threading.Timer _searchDebounceTimer;

        private void OnFilterButtonClicked(object sender, EventArgs e)
        {
            FilterPanel.IsVisible = !FilterPanel.IsVisible;

            if (FilterPanel.IsVisible)
            {
                BtnFilter.TextColor = Color.FromArgb("#1A73E8");
            }
            else
            {
                BtnFilter.TextColor = Color.FromArgb("#1E293B");
            }
        }

        private void OnGenderAllTapped(object sender, EventArgs e)
        {
            _selectedGender = "Tất cả";
            UpdateFilterUI();
        }

        private void OnGenderMaleTapped(object sender, EventArgs e)
        {
            _selectedGender = "Nam";
            UpdateFilterUI();
        }

        private void OnGenderFemaleTapped(object sender, EventArgs e)
        {
            _selectedGender = "Nữ";
            UpdateFilterUI();
        }

        private void OnAgeUnder45Tapped(object sender, EventArgs e)
        {
            _selectedAge = "Dưới 45";
            UpdateFilterUI();
        }

        private void OnAgeOver45Tapped(object sender, EventArgs e)
        {
            _selectedAge = "Trên 45";
            UpdateFilterUI();
        }

        private void OnTypeAllTapped(object sender, EventArgs e)
        {
            _selectedType = "Tất cả";
            UpdateFilterUI();
        }

        private void OnTypeBasicTapped(object sender, EventArgs e)
        {
            _selectedType = "Cơ bản";
            UpdateFilterUI();
        }

        private void OnTypeAdvancedTapped(object sender, EventArgs e)
        {
            _selectedType = "Nâng cao";
            UpdateFilterUI();
        }

        private async void OnApplyFilterClicked(object sender, EventArgs e)
        {
            long? loaiGoi = _selectedType switch
            {
                "Cơ bản" => 1,
                "Nâng cao" => 2,
                _ => 0
            };

            // Reset pagination và load lại từ đầu
            _currentPage = 1;
            _hasMoreData = true;
            _isInitialLoad = true;
            _danhSachGoiKham.Clear();

            await LoadGoiKhamAsync(loaiGoi);

            FilterPanel.IsVisible = false;
            BtnFilter.TextColor = Color.FromArgb("#1E293B");
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
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
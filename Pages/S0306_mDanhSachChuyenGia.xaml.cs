using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Controls.Shapes;
using SixOSDatKhamAppMobile.Services.S0305;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SixOSDatKhamAppMobile.Pages
{
    public partial class S0306_mDanhSachChuyenGia : ContentPage, INotifyPropertyChanged
    {
        #region Properties
        private readonly S0305_DKGoiKhamService _service;
        private ObservableCollection<ChuyenGiaModel> _danhSachChuyenGia = new();
        private ObservableCollection<ChuyenGiaModel> _filteredChuyenGia = new();
        private ChuyenGiaModel _selectedChuyenGia;
        private string _currentSearchText = "";
        private bool _isLoading = false;
        private bool _isLoadingMore = false;
        private long _selectedGoiKhamId = 0;
        private long _idGioiTinh = 0;

        // Pagination properties
        private int _currentPage = 1;
        private int _pageSize = 6;
        private int _totalRecords = 0;
        private bool _hasMoreData = true;

        public ObservableCollection<ChuyenGiaModel> FilteredChuyenGia
        {
            get => _filteredChuyenGia;
            set
            {
                _filteredChuyenGia = value;
                OnPropertyChanged();
            }
        }

        public ChuyenGiaModel SelectedChuyenGia
        {
            get => _selectedChuyenGia;
            set
            {
                if (_selectedChuyenGia != value)
                {
                    if (_selectedChuyenGia != null)
                    {
                        _selectedChuyenGia.IsSelected = false;
                    }

                    _selectedChuyenGia = value;

                    if (_selectedChuyenGia != null)
                    {
                        _selectedChuyenGia.IsSelected = true;
                    }

                    OnPropertyChanged();
                    RenderChuyenGiaList();

                    //// Cập nhật trạng thái nút đặt lịch
                    //UpdateBookButtonState();
                }
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
        #endregion

        #region Model Class
        public class ChuyenGiaModel
        {
            public long Id { get; set; }
            public string Ten { get; set; }
            public string HocVi { get; set; }
            public string ChucVu { get; set; }
            public string MoTaNgan { get; set; }
            public string MoTaChiTiet { get; set; }
            public string DuongDanHinh { get; set; }
            public bool IsSelected { get; set; }
        }
        #endregion

        #region Constructor
        public S0306_mDanhSachChuyenGia()
        {
            InitializeComponent();
            SetupSafeArea();
            _service = new S0305_DKGoiKhamService();
            BindingContext = this;

            // Thêm gesture recognizer cho nút đặt lịch
            var tapGesture = new TapGestureRecognizer();
            //tapGesture.Tapped += OnBookByExpertButtonTapped;
            //BookByExpertButton.GestureRecognizers.Add(tapGesture);

            // Load dữ liệu trang đầu tiên
            Task.Run(async () => await LoadDataAsync());
        }
        //private async void OnBookByExpertButtonTapped(object sender, EventArgs e)
        //{
        //    if (SelectedChuyenGia != null)
        //    {
        //        // Lưu chuyên gia đã chọn vào SecureStorage
        //        await SecureStorage.SetAsync("SelectedChuyenGiaId", SelectedChuyenGia.Id.ToString());
        //        await SecureStorage.SetAsync("SelectedChuyenGiaName", SelectedChuyenGia.Ten);

        //        // Chuyển đến trang gói khám với tham số true (từ flow chuyên gia)
        //        await Navigation.PushAsync(new S0306_mKhamTheoGoi(true));
        //    }
        //}

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
            // Đảm bảo status bar có màu trắng
            if (Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.GetUseSafeArea(this))
            {
                // Sử dụng safe area
                On<iOS>().SetUseSafeArea(true);
            }
            SearchEntry.Text = string.Empty;
            _currentSearchText = string.Empty;
            SearchResultsLabel.IsVisible = false;
            NoResultsContainer.IsVisible = false;
        }
        #endregion

        #region Private Methods
        private async Task LoadDataAsync(bool isLoadMore = false)
        {
            try
            {
                if (!isLoadMore)
                {
                    MainThread.BeginInvokeOnMainThread(() => IsLoading = true);
                    _currentPage = 1;
                    _hasMoreData = true;
                }
                else
                {
                    if (!_hasMoreData || IsLoadingMore)
                        return;

                    MainThread.BeginInvokeOnMainThread(() => IsLoadingMore = true);
                    _currentPage++;
                }
                var result = await _service.LayDanhSachChuyenGiaAsync(_currentPage, _pageSize);

                if (!result.Success || result.Data == null)
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        if (!isLoadMore)
                        {
                            await DisplayAlert("Thông báo", result.Message ?? "Không có chuyên gia phù hợp", "OK");
                            _danhSachChuyenGia.Clear();
                            FilteredChuyenGia = new ObservableCollection<ChuyenGiaModel>(_danhSachChuyenGia);
                            RenderChuyenGiaList();
                        }
                        _hasMoreData = false;
                        IsLoading = false;
                        IsLoadingMore = false;
                    });
                    return;
                }

                _totalRecords = result.Data.Pagination.TotalRecords;

                _hasMoreData = result.Data.Pagination.HasNextPage && result.Data.Data.Any();

                var tempList = new List<ChuyenGiaModel>();
                foreach (var item in result.Data.Data)
                {
                    var chuyenGia = new ChuyenGiaModel
                    {
                        Id = item.ID,
                        Ten = item.TenChuyenGia ?? "",
                        ChucVu = item.ChucDanh ?? "",
                        MoTaNgan = item.MoTaNgan ?? "",
                        MoTaChiTiet = item.MoTaChiTiet ?? "",
                        DuongDanHinh = !string.IsNullOrEmpty(item.DuongDanHinh)
                            ? $"https://kcg.bvungbuou.vn{item.DuongDanHinh}"
                            : "avatar.png",
                        IsSelected = false
                    };

                    tempList.Add(chuyenGia);
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (!isLoadMore)
                    {
                        _danhSachChuyenGia = new ObservableCollection<ChuyenGiaModel>(tempList);
                        FilteredChuyenGia = new ObservableCollection<ChuyenGiaModel>(_danhSachChuyenGia);
                    }
                    else
                    {
                        foreach (var item in tempList)
                        {
                            _danhSachChuyenGia.Add(item);
                            FilteredChuyenGia.Add(item);
                        }
                    }

                    RenderChuyenGiaList();
                    IsLoading = false;
                    IsLoadingMore = false;
                });
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Lỗi", $"Không thể tải dữ liệu: {ex.Message}", "OK");

                    _hasMoreData = false;
                    IsLoading = false;
                    IsLoadingMore = false;
                });
            }
        }
        private async Task ShowChuyenGiaDetailModal(ChuyenGiaModel chuyenGia)
        {
            try
            {
                var detailPage = new ContentPage
                {
                    Title = "Chi tiết chuyên gia"
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

                var scrollView = new Microsoft.Maui.Controls.ScrollView();

                var contentStack = new VerticalStackLayout
                {
                    Spacing = 15,
                    Padding = new Thickness(20)
                };

                var headerStack = new VerticalStackLayout
                {
                    Spacing = 10,
                    HorizontalOptions = LayoutOptions.Center
                };

                var avatarImage = new Image
                {
                    Source = chuyenGia.DuongDanHinh,
                    WidthRequest = 120,
                    HeightRequest = 120,
                    Aspect = Aspect.AspectFill,
                    Margin = new Thickness(0, 0, 0, 0)
                };
                headerStack.Children.Add(avatarImage);

                var nameLabel = new Label
                {
                    Text = !string.IsNullOrWhiteSpace(chuyenGia.Ten) ? chuyenGia.Ten : "Hệ thống đang cập nhật",
                    FontSize = 20,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = !string.IsNullOrWhiteSpace(chuyenGia.Ten) ? Color.FromArgb("#1E293B") : Color.FromArgb("#9CA3AF"),
                    HorizontalTextAlignment = TextAlignment.Center
                };
                headerStack.Children.Add(nameLabel);

                contentStack.Children.Add(headerStack);

                // Thông tin chi tiết - LUÔN hiển thị cả 2 section
                contentStack.Children.Add(CreateInfoSection("Chức vụ", chuyenGia.ChucVu));
                contentStack.Children.Add(CreateInfoSection("Giới thiệu", chuyenGia.MoTaChiTiet));

                scrollView.Content = contentStack;
                whiteContainer.Children.Add(scrollView);
                Grid.SetRow(scrollView, 0);

                var closeButton = new Button
                {
                    Text = "ĐÓNG",
                    BackgroundColor = Color.FromArgb("#1A73E8"),
                    TextColor = Colors.White,
                    Margin = new Thickness(20, 5, 20, 47),
                    CornerRadius = 10,
                    HeightRequest = 44
                };

                closeButton.Clicked += async (s, e) =>
                {
                    await detailPage.Navigation.PopModalAsync();
                };

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

        private View CreateInfoSection(string title, string content)
        {
            var stack = new VerticalStackLayout
            {
                Spacing = 5
            };

            stack.Children.Add(new Label
            {
                Text = title,
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#1A73E8")
            });

            // Kiểm tra nếu nội dung rỗng hoặc null thì hiển thị "Hệ thống đang cập nhật"
            var displayText = !string.IsNullOrWhiteSpace(content) ? content : "Hệ thống đang cập nhật";

            var contentLabel = new Label
            {
                Text = displayText,
                FontSize = 14,
                TextColor = !string.IsNullOrWhiteSpace(content) ? Color.FromArgb("#374151") : Color.FromArgb("#9CA3AF"),
                LineBreakMode = LineBreakMode.WordWrap
            };

            // Nếu là thông báo "Hệ thống đang cập nhật" thì thêm chữ nghiêng
            if (string.IsNullOrWhiteSpace(content))
            {
                contentLabel.FontAttributes = FontAttributes.Italic;
            }

            stack.Children.Add(contentLabel);

            return stack;
        }

        private void RenderChuyenGiaList()
        {
            ChuyenGiaListContainer.Children.Clear();

            foreach (var chuyenGia in FilteredChuyenGia)
            {
                var itemContainer = CreateChuyenGiaItem(chuyenGia);
                ChuyenGiaListContainer.Children.Add(itemContainer);
            }
            if (IsLoadingMore && _hasMoreData)
            {
                var loadingIndicator = new ActivityIndicator
                {
                    IsRunning = true,
                    Color = Color.FromArgb("#1A73E8"),
                    HeightRequest = 40,
                    Margin = new Thickness(0, 10)
                };
                ChuyenGiaListContainer.Children.Add(loadingIndicator);
            }

            bool hasResults = FilteredChuyenGia.Any();
            ChuyenGiaListContainer.IsVisible = hasResults;
            NoResultsContainer.IsVisible = !hasResults;

            if (!string.IsNullOrEmpty(_currentSearchText) && hasResults)
            {
                SearchResultsLabel.Text = $"Tìm thấy {FilteredChuyenGia.Count} chuyên gia cho '{_currentSearchText}'";
                SearchResultsLabel.IsVisible = true;
            }
            else if (hasResults && string.IsNullOrEmpty(_currentSearchText))
            {
                // hiển thị thông báo khi đã load hết 
                if (!_hasMoreData && FilteredChuyenGia.Count == _totalRecords)
                {
                    SearchResultsLabel.Text = $"Đã hiển thị tất cả {_totalRecords} chuyên gia";
                }
                else
                {
                    SearchResultsLabel.Text = $"Hiển thị {FilteredChuyenGia.Count}/{_totalRecords} chuyên gia";
                }
                SearchResultsLabel.IsVisible = true;
            }
            else
            {
                SearchResultsLabel.IsVisible = false;
            }
        }

        private Border CreateChuyenGiaItem(ChuyenGiaModel chuyenGia)
        {
            var mainBorder = new Border
            {
                Stroke = chuyenGia.IsSelected ? Color.FromArgb("#1A73E8") : Color.FromArgb("#E2E8F0"),
                StrokeThickness = chuyenGia.IsSelected ? 2 : 1,
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                BackgroundColor = chuyenGia.IsSelected ? Color.FromArgb("#EBF5FF") : Colors.White,
                Padding = 0,
                Margin = new Thickness(0, 0, 0, 15)
            };

            var mainGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                RowSpacing = 0,
                Padding = new Thickness(15, 15, 15, 15)
            };

            var avatarFrame = new Frame
            {
                BackgroundColor = Color.FromArgb("#F1F5F9"),
                Padding = 0,
                CornerRadius = 10,
                WidthRequest = 80,
                HeightRequest = 100,
                HasShadow = false,
                BorderColor = Color.FromArgb("#E2E8F0")
            };

            var avatarImage = new Image
            {
                Source = chuyenGia.DuongDanHinh,
                Aspect = Aspect.AspectFill,
                WidthRequest = 80,
                HeightRequest = 100
            };

            avatarFrame.Content = avatarImage;
            mainGrid.Children.Add(avatarFrame);
            Grid.SetColumn(avatarFrame, 0);
            Grid.SetRowSpan(avatarFrame, 2);

            var infoStack = new VerticalStackLayout
            {
                Spacing = 8,
                Margin = new Thickness(15, 0, 0, 0)
            };

            var nameLabel = new Label
            {
                Text = chuyenGia.Ten,
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = chuyenGia.IsSelected ? Color.FromArgb("#1A73E8") : Color.FromArgb("#1E293B"),
                LineBreakMode = LineBreakMode.WordWrap
            };
            infoStack.Children.Add(nameLabel);

            if (!string.IsNullOrEmpty(chuyenGia.ChucVu))
            {
                var chucVuLabel = new Label
                {
                    Text = chuyenGia.ChucVu,
                    FontSize = 14,
                    TextColor = Color.FromArgb("#64748B")
                };
                infoStack.Children.Add(chucVuLabel);
            }

            var detailButton = new Border
            {
                Stroke = Color.FromArgb("#1A73E8"),
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 8 },
                BackgroundColor = Colors.White,
                WidthRequest = 100,
                HeightRequest = 36,
                Padding = new Thickness(10, 0),
                HorizontalOptions = LayoutOptions.Start
            };

            var detailLabel = new Label
            {
                Text = "Xem chi tiết",
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#1A73E8"),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };

            detailButton.Content = detailLabel;

            detailButton.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(async () =>
                {
                    await ShowChuyenGiaDetailModal(chuyenGia);
                })
            });

            infoStack.Children.Add(detailButton);

            mainGrid.Children.Add(infoStack);
            Grid.SetColumn(infoStack, 1);
            Grid.SetRowSpan(infoStack, 2);

            //var selectBorder = new Border
            //{
            //    Stroke = Color.FromArgb("#1A73E8"),
            //    StrokeThickness = 1,
            //    StrokeShape = new RoundRectangle { CornerRadius = 8 },
            //    BackgroundColor = chuyenGia.IsSelected ? Color.FromArgb("#1A73E8") : Colors.White,
            //    WidthRequest = 40,
            //    HeightRequest = 40,
            //    HorizontalOptions = LayoutOptions.Center,
            //    VerticalOptions = LayoutOptions.Center
            //};

            //if (chuyenGia.IsSelected)
            //{
            //    var tickLabel = new Label
            //    {
            //        Text = "✓",
            //        FontSize = 20,
            //        FontAttributes = FontAttributes.Bold,
            //        TextColor = Colors.White,
            //        VerticalOptions = LayoutOptions.Center,
            //        HorizontalOptions = LayoutOptions.Center
            //    };
            //    selectBorder.Content = tickLabel;
            //}

            //mainGrid.Children.Add(selectBorder);
            //Grid.SetColumn(selectBorder, 2);
            //Grid.SetRowSpan(selectBorder, 2);

            mainGrid.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() =>
                {
                    if (chuyenGia.IsSelected)
                    {
                        SelectedChuyenGia = null;
                    }
                    else
                    {
                        SelectedChuyenGia = chuyenGia;
                    }
                })
            });

            mainBorder.Content = mainGrid;
            return mainBorder;
        }

        private void FilterChuyenGia(string searchText)
        {
            _currentSearchText = searchText;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                FilteredChuyenGia = new ObservableCollection<ChuyenGiaModel>(_danhSachChuyenGia);
            }
            else
            {
                var filtered = _danhSachChuyenGia
                    .Where(g => g.Ten.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                               (g.ChucVu?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                               (g.MoTaNgan?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false))
                    .ToList();
                FilteredChuyenGia = new ObservableCollection<ChuyenGiaModel>(filtered);
            }

            RenderChuyenGiaList();
        }

        private void OnScrollViewScrolled(object sender, ScrolledEventArgs e)
        {
            var scrollView = sender as Microsoft.Maui.Controls.ScrollView;
            if (scrollView == null) return;
            if (!string.IsNullOrEmpty(_currentSearchText) || !_hasMoreData || IsLoadingMore)
                return;

            // kiểm tra nếu scroll gần đến cuối (80% chiều cao)
            var scrollingSpace = scrollView.ContentSize.Height - scrollView.Height;
            if (scrollingSpace <= 0) return;

            var threshold = 0.8;
            if (e.ScrollY >= scrollingSpace * threshold)
            {
                Task.Run(async () => await LoadDataAsync(true));
            }
        }
        #endregion

        #region Event Handlers
        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            FilterChuyenGia(e.NewTextValue);
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        //private void UpdateBookButtonState()
        //{
        //    if (SelectedChuyenGia != null)
        //    {
        //        // Khi có chuyên gia được chọn
        //        BookByExpertButton.BackgroundColor = Color.FromArgb("#1A73E8");
        //        BookByExpertButton.Stroke = Color.FromArgb("#1A73E8");

        //        var bookButtonLabel = BookByExpertButton.Content as Grid;
        //        if (bookButtonLabel != null)
        //        {
        //            // Tìm Label trong Grid
        //            var label = bookButtonLabel.Children.OfType<Label>().FirstOrDefault();
        //            if (label != null)
        //            {
        //                label.TextColor = Colors.White;
        //            }

        //            // Hiển thị icon mũi tên
        //            var arrowIcon = bookButtonLabel.Children.OfType<Image>().FirstOrDefault();
        //            if (arrowIcon != null)
        //            {
        //                arrowIcon.IsVisible = true;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        // Khi không có chuyên gia được chọn
        //        BookByExpertButton.BackgroundColor = Color.FromArgb("#E5E7EB");
        //        BookByExpertButton.Stroke = Color.FromArgb("#1A73E8");

        //        var bookButtonLabel = BookByExpertButton.Content as Grid;
        //        if (bookButtonLabel != null)
        //        {
        //            // Tìm Label trong Grid
        //            var label = bookButtonLabel.Children.OfType<Label>().FirstOrDefault();
        //            if (label != null)
        //            {
        //                label.TextColor = Color.FromArgb("#9CA3AF");
        //            }

        //            // Ẩn icon mũi tên
        //            var arrowIcon = bookButtonLabel.Children.OfType<Image>().FirstOrDefault();
        //            if (arrowIcon != null)
        //            {
        //                arrowIcon.IsVisible = false;
        //            }
        //        }
        //    }
        //}
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
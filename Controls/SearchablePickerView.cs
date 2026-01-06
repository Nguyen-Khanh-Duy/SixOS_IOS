using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using SixOSDatKhamAppMobile.Models;
using System.Collections.ObjectModel;

namespace SixOSDatKhamAppMobile.Controls
{
    public class SearchablePickerView : VerticalStackLayout
    {
        private Entry searchEntry;
        private Picker picker;
        private ObservableCollection<PickerItem> originalItems;

        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(nameof(ItemsSource), typeof(ObservableCollection<PickerItem>),
                typeof(SearchablePickerView), null, propertyChanged: OnItemsSourceChanged);

        public static readonly BindableProperty SelectedItemProperty =
            BindableProperty.Create(nameof(SelectedItem), typeof(PickerItem),
                typeof(SearchablePickerView), null, propertyChanged: OnSelectedItemChanged);

        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string),
                typeof(SearchablePickerView), "Chọn");

        public static readonly BindableProperty PlaceholderProperty =
            BindableProperty.Create(nameof(Placeholder), typeof(string),
                typeof(SearchablePickerView), "Tìm kiếm...");

        public ObservableCollection<PickerItem> ItemsSource
        {
            get => (ObservableCollection<PickerItem>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public PickerItem SelectedItem
        {
            get => (PickerItem)GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        public Picker GetPicker() => picker;

        public SearchablePickerView()
        {
            Spacing = 5;
            Padding = 0;
            originalItems = new ObservableCollection<PickerItem>();

            // Tạo search entry
            searchEntry = new Entry
            {
                Placeholder = Placeholder,
                PlaceholderColor = Color.FromArgb("#94A3B8"),
                FontSize = 14,
                TextColor = Color.FromArgb("#1E293B"),
                BackgroundColor = Colors.White, 
                Margin = new Thickness(0),
                //Padding = new Thickness(12, 8)
            };

            searchEntry.TextChanged += OnSearchTextChanged;

            // Wrap search trong Frame
            var searchFrame = new Frame
            {
                Content = searchEntry,
                CornerRadius = 12,
                Padding = 0,
                Margin = new Thickness(0, 0, 0, 8),
                BorderColor = Color.FromArgb("#E2E8F0"),
                BackgroundColor = Colors.White,
                HasShadow = true
            };

            // Tạo picker
            picker = new Picker
            {
                Title = Title,
                FontSize = 14,
                TextColor = Color.FromArgb("#1E293B"),
                BackgroundColor = Colors.Transparent,
                TitleColor = Color.FromArgb("#64748B"),
                VerticalOptions = LayoutOptions.Center,
                ItemDisplayBinding = new Binding("Name")
            };

            picker.SelectedIndexChanged += (s, e) =>
            {
                if (picker.SelectedIndex >= 0)
                {
                    SelectedItem = picker.SelectedItem as PickerItem;
                }
            };

            // Wrap picker trong Frame với dropdown icon
            var pickerGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                Padding = new Thickness(15, 2),
                ColumnSpacing = 0
            };

            pickerGrid.Children.Add(picker);
            var arrowLabel = new Label
            {
                Text = "▼",
                FontSize = 12,
                TextColor = Color.FromArgb("#64748B"),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.End
            };

            Grid.SetColumn(arrowLabel, 1);
            pickerGrid.Children.Add(arrowLabel);

            var pickerFrame = new Frame
            {
                Content = pickerGrid,
                CornerRadius = 12,
                Padding = 0,
                Margin = new Thickness(0),
                BorderColor = Color.FromArgb("#E2E8F0"),
                BackgroundColor = Colors.White,
                HasShadow = true
            };

            this.Children.Add(searchFrame);
            this.Children.Add(pickerFrame);
        }

        private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = bindable as SearchablePickerView;
            if (control?.picker == null) return;

            var items = newValue as ObservableCollection<PickerItem>;
            if (items != null)
            {
                control.originalItems = new ObservableCollection<PickerItem>(items);
                control.picker.ItemsSource = new ObservableCollection<PickerItem>(items);
            }
        }

        private static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = bindable as SearchablePickerView;
            if (control?.picker == null) return;

            var item = newValue as PickerItem;
            if (item != null)
            {
                control.picker.SelectedItem = item;
            }
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            if (picker?.ItemsSource == null) return;

            string searchText = e.NewTextValue?.ToLower() ?? "";

            if (string.IsNullOrWhiteSpace(searchText))
            {
                picker.ItemsSource = new ObservableCollection<PickerItem>(originalItems);
            }
            else
            {
                var filtered = originalItems
                    .Where(x => x.Name?.ToLower().Contains(searchText) ?? false)
                    .ToList();

                picker.ItemsSource = new ObservableCollection<PickerItem>(filtered);
            }
        }
    }
}
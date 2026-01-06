
using SixOSDatKhamAppMobile.Services.S0305;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SixOSDatKhamAppMobile.ViewModels
{
    public class DoiTacViewModel : INotifyPropertyChanged
    {
        private string _tenDoiTac;
        private string _diaChiDoiTac;

        public string TenDoiTac
        {
            get => _tenDoiTac;
            set
            {
                _tenDoiTac = value;
                OnPropertyChanged();
            }
        }

        public string DiaChiDoiTac
        {
            get => _diaChiDoiTac;
            set
            {
                _diaChiDoiTac = value;
                OnPropertyChanged();
            }
        }

        public async Task LoadDataAsync(S0305_DoiTacService _apiService)
        {
            var result = await _apiService.GetBenhVienAsync();

            // Ví dụ API trả về status
            //StatusText = result.status switch
            //{
            //    1 => "Đang hoạt động",
            //    2 => "Đã khóa",
            //    _ => "Không xác định"
            //};

            if (result.Success)
            {
                TenDoiTac = result.TenDoiTac ?? string.Empty;
                DiaChiDoiTac = result.DiaChi ?? string.Empty;
            }
            else
            {
                TenDoiTac = string.Empty;
                DiaChiDoiTac = string.Empty;
            }

        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

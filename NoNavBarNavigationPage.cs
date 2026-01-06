using Microsoft.Maui.Controls;

namespace SixOSDatKhamAppMobile
{
    public class NoNavBarNavigationPage : NavigationPage
    {
        public NoNavBarNavigationPage(Page root) : base(root)
        {
            // Tắt cho trang gốc
            SetHasNavigationBar(root, false);

            // Tắt cho mọi trang push vào NavigationPage
            this.Pushed += (s, e) =>
            {
                SetHasNavigationBar(e.Page, false);
            };
        }
    }
}

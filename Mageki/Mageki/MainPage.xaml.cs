
using Rg.Plugins.Popup.Extensions;

using System;

using Xamarin.Forms;

namespace Mageki
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void ControllerPanel_LogoClickd(object sender, EventArgs args)
        {
            Navigation.PushPopupAsync(new SettingsPopup());
        }
    }
}

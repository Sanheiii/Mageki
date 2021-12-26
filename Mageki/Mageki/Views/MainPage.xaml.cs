using Mageki.Utils;

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
            _= Update.CheckUpdateAsync();
        }

        private SettingsPopup settingPopup;
        private async void ControllerPanel_LogoClickd(object sender, EventArgs args)
        {
            var popup = settingPopup ?? (settingPopup = new SettingsPopup(Controller));
            try
            {
                await Navigation.PushPopupAsync(popup);
            }
            catch { }
        }
    }
}

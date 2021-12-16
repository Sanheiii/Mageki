
using Rg.Plugins.Popup.Extensions;

using System;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Mageki
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private SettingsPopup settingPopup;
        private async void ControllerPanel_LogoClickd(object sender, EventArgs args)
        {
            var popup = settingPopup ?? (settingPopup = new SettingsPopup());
            try
            {
                await Navigation.PushPopupAsync(popup);
            }
            catch { }
        }
    }
}

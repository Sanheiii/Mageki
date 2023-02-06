using Mageki.DependencyServices;

using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Mageki
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPopup : PopupPage
    {

        SettingsViewModel ViewModel => BindingContext as SettingsViewModel;
        public SettingsPopup()
        {
            this.BindingContext = new SettingsViewModel();
            InitializeComponent(); 
        }

        public void Dismiss()
        {
            if (PopupNavigation.Instance.PopupStack.Count > 0)
                PopupNavigation.Instance.RemovePageAsync(this);
        }

        private void Close_Tapped(object sender, EventArgs e)
        {
            Dismiss();
        }

        private void SeparateButtonsAndLever_Tapped(object sender, EventArgs e)
        {
            ViewModel.SeparateButtonsAndLever = !ViewModel.SeparateButtonsAndLever;
        }

        private void HideButtons_Tapped(object sender, EventArgs e)
        {
            ViewModel.HideButtons = !ViewModel.HideButtons;
        }

        private void HapticFeedback_Tapped(object sender, EventArgs e)
        {
            ViewModel.HapticFeedback = !ViewModel.HapticFeedback;
        }

        private void Exit_Tapped(object sender, EventArgs e)
        {
            DependencyService.Get<ICloseApplication>().Close();
        }

        private void ProtocolPicker_Tapped(object sender, EventArgs e)
        {
            ProtocolPicker.Focus();
        }
    }
}
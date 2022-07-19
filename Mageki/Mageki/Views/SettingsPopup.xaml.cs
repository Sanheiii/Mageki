using Mageki.DependencyServices;

using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Mageki
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPopup : PopupPage
    {
        private ControllerPanel controller;

        SettingsViewModel ViewModel => BindingContext as SettingsViewModel;
        public SettingsPopup(ControllerPanel controller)
        {
            this.controller = controller;
            InitializeComponent();
            ProtocolPicker.SelectedIndex = ViewModel.ProtocolIndex;
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

        private void HapticFeedback_Tapped(object sender, EventArgs e)
        {
            ViewModel.HapticFeedback = !ViewModel.HapticFeedback;
        }

        private async void TestButton_Tapped(object sender, EventArgs e)
        {
            if (sender is ViewCell cell)
            {
                cell.IsEnabled = false;
                await controller.PressAndReleaseTestButtonAsync();
                cell.IsEnabled = true;
            }
        }

        private async void ServiceButton_Tapped(object sender, EventArgs e)
        {
            if (sender is ViewCell cell)
            {
                cell.IsEnabled = false;
                await controller.PressAndReleaseServiceButtonAsync();
                cell.IsEnabled = true;
            }
        }

        private void Exit_Tapped(object sender, EventArgs e)
        {
            DependencyService.Get<ICloseApplication>().Close();
        }

        private async void Version_Tapped(object sender, EventArgs e)
        {
            if (sender is ViewCell cell)
            {
                cell.IsEnabled = false;
                await Utils.Update.CheckUpdateAsync(true);
                cell.IsEnabled = true;
            }
        }
    }
}
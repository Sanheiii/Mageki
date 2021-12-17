using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Mageki
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPopup : PopupPage
    {
        SettingsViewModel ViewModel=>BindingContext as SettingsViewModel;
        public SettingsPopup()
        {
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

        private void UseSimplifiedLayout_Tapped(object sender, EventArgs e)
        {
            ViewModel.UseSimplifiedLayout = !ViewModel.UseSimplifiedLayout;
        }
    }
}
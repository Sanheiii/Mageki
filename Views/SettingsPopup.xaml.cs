using Microsoft.Maui.Controls.Xaml;
using Mopups.Pages;

namespace Mageki
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPopup : PopupPage
    {
        public SettingsPopup(SettingsViewModel settingsViewModel)
        {
            InitializeComponent();
            BindingContext = settingsViewModel;
        }

        private void PortEntry_Unfocused(object? sender, FocusEventArgs e)
        {
            PortEntry.Text = ((SettingsViewModel)BindingContext).Port.ToString();
        }

        protected override void OnDisappearing()
        {
            StaticIO.SetOptionButton(OptionButtons.Test, false);
            StaticIO.SetOptionButton(OptionButtons.Service, false);
            ((SettingsViewModel)BindingContext).RaisePropertyChanged(nameof(SettingsViewModel.TestState));
            ((SettingsViewModel)BindingContext).RaisePropertyChanged(nameof(SettingsViewModel.ServiceState));
            base.OnDisappearing();
        }
    }
}

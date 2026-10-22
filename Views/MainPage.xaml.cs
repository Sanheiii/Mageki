using Mageki.Utils;



using System;

using Microsoft.Maui.Controls;

namespace Mageki
{
    public partial class MainPage : ContentPage
    {
        private readonly ControllerPanel controller;
        private bool checkedUpdate;

        public MainPage(Mageki.Services.INfcService nfcService, Mageki.Services.ISettingsPopupService settingsPopupService)
        {
            InitializeComponent();
            Content = controller = new ControllerPanel(nfcService, settingsPopupService);
        }

        public void ReleaseInputs() => controller.ReleaseInputs();

        protected override void OnAppearing()
        {
            base.OnAppearing();
            DeviceDisplay.Current.KeepScreenOn = true;
            if (!checkedUpdate)
            {
                checkedUpdate = true;
                _ = Update.CheckUpdateAsync(page: this);
            }
        }

        protected override void OnDisappearing()
        {
            ReleaseInputs();
            DeviceDisplay.Current.KeepScreenOn = false;
            base.OnDisappearing();
        }
    }
}

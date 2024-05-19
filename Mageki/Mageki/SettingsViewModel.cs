using Mageki.Drawables;
using Mageki.Resources;

using Rg.Plugins.Popup.Services;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;

using Xamarin.CommunityToolkit.Effects;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Mageki
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        // Math.Pow(10, 0.1)
        private const double leverSensitivityBase = 1.2589254117941673;

        public Protocol Protocol
        {
            get => Settings.Protocol;
            set
            {
                Settings.Protocol = value;
                RaisePropertyChanged();
            }
        }

        public ushort Port
        {
            get => Settings.Port;
            set
            {
                Settings.Port = value;
                RaisePropertyChanged();
            }
        }

        public string IP
        {
            get => Settings.IP;
            set
            {
                Settings.IP = value;
                RaisePropertyChanged();
            }
        }

        public Status Status => StaticIO.Status;

        public LeverMoveMode LeverMoveMode
        {
            get => Settings.LeverMoveMode;
            set
            {
                Settings.LeverMoveMode = value;
                RaisePropertyChanged();
            }
        }

        public bool EnableCompositeMode
        {
            get => Settings.EnableCompositeMode;
            set
            {
                Settings.EnableCompositeMode = value;
                RaisePropertyChanged();
            }
        }

        public bool EnableLeverOverflowHandling
        {
            get => Settings.EnableLeverOverflowHandling;
            set
            {
                Settings.EnableLeverOverflowHandling = value;
                RaisePropertyChanged();
            }
        }

        // 0 => 1 , -10 => 0.1 , 10 => 10
        public float LeverSensitivity
        {
            get => (float)Math.Log(Settings.LeverSensitivity, leverSensitivityBase);
            set
            {
                Settings.LeverSensitivity = (float)Math.Pow(leverSensitivityBase, value);
                RaisePropertyChanged();
            }
        }

        public double MaxLeverLinearity => Settings.MaxLeverLinearity;
        public double MinLeverLinearity => Settings.MinLeverLinearity;

        public int LeverLinearity
        {
            get => Settings.LeverLinearity;
            set
            {
                Settings.LeverLinearity = value;
                RaisePropertyChanged();
            }
        }

        public float ButtonBottomMargin
        {
            get => Settings.ButtonBottomMargin;
            set
            {
                Settings.ButtonBottomMargin = value;
                RaisePropertyChanged();
            }
        }

        public bool AntiMisTouch
        {
            get => Settings.AntiMisTouch;
            set
            {
                Settings.AntiMisTouch = value;
                RaisePropertyChanged();
            }
        }

        public string Aimeid
        {
            get => Settings.AimeId;
            set
            {
                Settings.AimeId = value;
                RaisePropertyChanged();
            }
        }

        public bool HideGameButtons
        {
            get => Settings.HideGameButtons;
            set
            {
                Settings.HideGameButtons = value;
                RaisePropertyChanged();
            }
        }

        public bool EnableHapticFeedback
        {
            get => Settings.EnableHapticFeedback;
            set
            {
                Settings.EnableHapticFeedback = value;
                RaisePropertyChanged();
            }
        }

        public Version Version => Version.Parse(VersionTracking.CurrentVersion);

        public TouchState TestState
        {
            get => (TouchState)(StaticIO.Data.OptButtons & OptionButtons.Test);
            set
            {
                StaticIO.SetOptionButton(OptionButtons.Test, value == TouchState.Pressed);
                RaisePropertyChanged();
            }
        }

        public TouchState ServiceState
        {
            get => (TouchState)(StaticIO.Data.OptButtons & OptionButtons.Service);
            set
            {
                StaticIO.SetOptionButton(OptionButtons.Service, value == TouchState.Pressed);
                RaisePropertyChanged();
            }
        }

        public Command GoBack { get; }
        public Command CheckUpdate { get; }
        public Command ToggleSwitch { get; }
        public Command FocusElement { get; }
        public Command SelectProtocol { get; }
        public Command SelectLeverMoveMode { get; }

        public SettingsViewModel()
        {
            GoBack = new Command(GoBackExecute);
            CheckUpdate = new Command(CheckUpdateExecute, (obj) => !checkingUpdate);
            ToggleSwitch = new Command(ToggleSwitchExecute);
            FocusElement = new Command(FocusElementExecute);
            SelectProtocol = new Command(SelectProtocolExecute);
            SelectLeverMoveMode = new Command(SelectLeverMoveModeExecute);
            StaticIO.OnStatusChanged += StaticIO_OnStatusChanged;
        }

        private void StaticIO_OnStatusChanged(object sender, OnStatusChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(Status));
        }

        private void GoBackExecute(object obj)
        {
            SettingsPopup popup = (SettingsPopup)obj;

            if (PopupNavigation.Instance.PopupStack?.Contains(popup) ?? false)
                PopupNavigation.Instance.RemovePageAsync(popup);
        }

        bool checkingUpdate = false;

        private async void CheckUpdateExecute(object obj)
        {
            var popup = (SettingsPopup)obj;
            checkingUpdate = true;
            CheckUpdate.ChangeCanExecute();
            var result = await Utils.Update.CheckUpdateAsync(true);
            VisualElement element = PopupNavigation.Instance.PopupStack.Contains(popup)
                ? popup
                : Application.Current.MainPage;
            if (result == Utils.Update.CheckVersionResult.Latest)
            {
                await element.DisplayToastAsync(AppResources.MagekiIsUpToDate);
            }
            else
            {
                await element.DisplayToastAsync(AppResources.CheckUpdateFailed);
            }

            checkingUpdate = false;
            CheckUpdate.ChangeCanExecute();
        }

        private void ToggleSwitchExecute(object obj)
        {
            if (obj is Switch s)
            {
                s.IsToggled = !s.IsToggled;
            }
        }

        private void FocusElementExecute(object obj)
        {
            if (obj is VisualElement element)
            {
                element.Focus();
            }

            if (obj is Entry entry)
            {
                entry.CursorPosition = entry.Text.Length;
            }
        }

        private void SelectProtocolExecute(object obj)
        {
            var newValue = (Protocol)obj;
            if (Protocol != newValue)
            {
                Protocol = newValue;
            }
        }

        private void SelectLeverMoveModeExecute(object obj)
        {
            var newValue = (LeverMoveMode)obj;
            if (LeverMoveMode != newValue)
            {
                LeverMoveMode = newValue;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public enum Protocol
    {
        UDP = 0,
        TCP = 1
    }

    public enum LeverMoveMode
    {
        Relative,
        Absolute
    }

    public static class Settings
    {
        public const double MaxLeverLinearity = 540;
        public const double MinLeverLinearity = 54;

        public static Protocol Protocol
        {
            get => (Protocol)Preferences.Get("protocol", (int)Protocol.TCP);
            set
            {
                if (value != Protocol)
                {
                    Preferences.Set("protocol", (int)value);
                    OnValueChanged();
                }
            }
        }

        public static ushort Port
        {
            get => (ushort)Preferences.Get("port", 4354);
            set
            {
                if (value != Port)
                {
                    Preferences.Set("port", value);
                    OnValueChanged();
                }
            }
        }

        public static string IP
        {
            get
            {
                return Preferences.Get("ip", string.Empty);
            }
            set
            {
                if (value != IP)
                {
                    Preferences.Set("ip", value);
                    OnValueChanged();
                    OnValueChanged("IPAddress");
                }
            }
        }

        public static IPAddress IPAddress => IPAddress.TryParse(IP, out var ip) ? ip : IPAddress.Broadcast;

        public static bool HideGameButtons
        {
            get => Preferences.Get("hideGameButtons", false);
            set
            {
                if (value != HideGameButtons)
                {
                    Preferences.Set("hideGameButtons", value);
                    OnValueChanged();
                }
            }
        }

        public static bool HideWallActionDevices
        {
            get => Preferences.Get("hideWallActionDevices", false);
            set
            {
                if (value != HideWallActionDevices)
                {
                    Preferences.Set("hideWallActionDevices", value);
                    if (value) EnableCompositeMode = false;
                    OnValueChanged();
                }
            }
        }

        public static bool HideMenuButtons
        {
            get => Preferences.Get("hideMenuButtons", false);
            set
            {
                if (value != HideMenuButtons)
                {
                    Preferences.Set("hideMenuButtons", value);
                    OnValueChanged();
                }
            }
        }

        public static bool EnableHapticFeedback
        {
            get => Preferences.Get("hapticFeedback", false);
            set
            {
                if (value != EnableHapticFeedback)
                {
                    Preferences.Set("hapticFeedback", value);
                    OnValueChanged();
                }
            }
        }

        public static LeverMoveMode LeverMoveMode
        {
            get => (LeverMoveMode)Preferences.Get("leverMoveMode", (int)LeverMoveMode.Relative);
            set
            {
                if (value == LeverMoveMode) return;
                if (value == LeverMoveMode.Absolute) EnableCompositeMode = false;
                Preferences.Set("leverMoveMode", (int)value);
                OnValueChanged();
            }
        }

        public static bool EnableCompositeMode
        {
            get => Preferences.Get("enableCompositeMode", false);
            set
            {
                if (value == EnableCompositeMode) return;
                Preferences.Set("enableCompositeMode", value);
                OnValueChanged();
            }
        }

        public static bool EnableLeverOverflowHandling
        {
            get => Preferences.Get("enableLeverOverflowHandling", false);
            set
            {
                if (value == EnableLeverOverflowHandling) return;
                Preferences.Set("enableLeverOverflowHandling", value);
                OnValueChanged();
            }
        }

        public static float LeverSensitivity
        {
            get => Preferences.Get("leverSensitivity", 1f);
            set
            {
                if (value != LeverSensitivity)
                {
                    Preferences.Set("leverSensitivity", value);
                    OnValueChanged();
                }
            }
        }

        public static int LeverLinearity
        {
            get => Preferences.Get("leverLinearity", (int)MaxLeverLinearity);
            set
            {
                if (value != LeverLinearity)
                {
                    if (value < MinLeverLinearity) value = (int)MinLeverLinearity;
                    else if (value > MaxLeverLinearity) value = (int)MaxLeverLinearity;
                    Preferences.Set("leverLinearity", value);
                    OnValueChanged();
                }
            }
        }

        public static float ButtonBottomMargin
        {
            get => Preferences.Get("buttonBottomMargin", 0.2f);
            set
            {
                if (value != ButtonBottomMargin)
                {
                    Preferences.Set("buttonBottomMargin", value);
                    OnValueChanged();
                }
            }
        }

        public static bool AntiMisTouch
        {
            get => Preferences.Get("antiMisTouch", false);
            set
            {
                if (value != AntiMisTouch)
                {
                    Preferences.Set("antiMisTouch", value);
                    OnValueChanged();
                }
            }
        }

        public static string AimeId
        {
            get => Preferences.Get("aimeId", string.Empty);
            set
            {
                if (value != AimeId)
                {
                    Preferences.Set("aimeId", value);
                    OnValueChanged();
                }
            }
        }

        public static Version IgnoredVersion
        {
            get => Version.Parse(Preferences.Get("ignoredVersion", "0.0.0"));
            set
            {
                if (value != IgnoredVersion)
                {
                    Preferences.Set("ignoredVersion", value.ToString());
                    OnValueChanged();
                }
            }
        }

        private static void OnValueChanged([CallerMemberName] string name = null)
        {
            ValueChanged.Invoke(name);
        }

        public delegate void ValueChangedEventHandler(string name);

        public static event ValueChangedEventHandler ValueChanged;
    }
}
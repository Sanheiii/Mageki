using Mageki.Drawables;
using Mageki.Resources;

using Rg.Plugins.Popup.Services;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

        public Protocol Protocol { get => Settings.Protocol; set { Settings.Protocol = value; RaisePropertyChanged(); } }

        public ushort Port { get => Settings.Port; set { Settings.Port = value; RaisePropertyChanged(); } }

        public LeverMoveMode LeverMoveMode { get => Settings.LeverMoveMode; set { Settings.LeverMoveMode = value; RaisePropertyChanged(); } }
        public List<string> LeverMoveModes => Enum.GetNames(typeof(LeverMoveMode)).Select(name => AppResources.ResourceManager.GetString(name)).ToList();

        // 0 => 1 , -10 => 0.1 , 10 => 10
        public float LeverSensitivity { get => (float)Math.Log(Settings.LeverSensitivity, leverSensitivityBase); set { Settings.LeverSensitivity = (float)Math.Pow(leverSensitivityBase, value); RaisePropertyChanged(); } }

        public double MaxLeverLinearity => Settings.MaxLeverLinearity;
        public double MinLeverLinearity => Settings.MinLeverLinearity;
        public int LeverLinearity { get => Settings.LeverLinearity; set { Settings.LeverLinearity = value; RaisePropertyChanged(); } }

        public float ButtonBottomMargin { get => Settings.ButtonBottomMargin; set { Settings.ButtonBottomMargin = value; RaisePropertyChanged(); } }

        public string Aimeid { get => Settings.AimeId; set { Settings.AimeId = value; RaisePropertyChanged(); } }

        public bool SeparateButtonsAndLever { get => Settings.SeparateButtonsAndLever; set { Settings.SeparateButtonsAndLever = value; RaisePropertyChanged(); } }

        public bool HideButtons { get => Settings.HideButtons; set { Settings.HideButtons = value; RaisePropertyChanged(); } }

        public bool HapticFeedback { get => Settings.HapticFeedback; set { Settings.HapticFeedback = value; RaisePropertyChanged(); } }

        public Version Version => Version.Parse(VersionTracking.CurrentVersion);
        public TouchState TestState { get => (TouchState)(App.CurrentIO.Data.OptButtons & OptionButtons.Test); set { App.CurrentIO.SetOptionButton(OptionButtons.Test, value == TouchState.Pressed); RaisePropertyChanged(); } }
        public TouchState ServiceState { get => (TouchState)(App.CurrentIO.Data.OptButtons & OptionButtons.Service); set { App.CurrentIO.SetOptionButton(OptionButtons.Service, value == TouchState.Pressed); RaisePropertyChanged(); } }

        public Command GoBack { get; }
        public Command CheckUpdate { get; }
        public Command ToggleSwitch { get; }
        public Command FocusElement { get; }
        public Command SelectProtocol { get; }

        public SettingsViewModel()
        {
            GoBack = new Command(GoBackExecute);
            CheckUpdate = new Command(CheckUpdateExecute, (obj) => !checkingUpdate);
            ToggleSwitch = new Command(ToggleSwitchExecute);
            FocusElement = new Command(FocusElementExecute);
            SelectProtocol = new Command(SelectProtocolExecute);
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
            VisualElement element = PopupNavigation.Instance.PopupStack.Contains(popup) ? popup : Application.Current.MainPage;
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
            var newValue = Enum.Parse<Protocol>(obj.ToString());
            if(Protocol!=newValue)
            {
                Protocol = newValue;
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
        public static bool SeparateButtonsAndLever
        {
            get => Preferences.Get("separateButtonsAndLever", false);
            set
            {
                if (value != SeparateButtonsAndLever)
                {
                    Preferences.Set("separateButtonsAndLever", value);
                    OnValueChanged();
                }
            }
        }
        public static bool HideButtons
        {
            get => Preferences.Get("hideButtons", false);
            set
            {
                if (value != HideButtons)
                {
                    Preferences.Set("hideButtons", value);
                    OnValueChanged();
                }
            }
        }
        public static bool HapticFeedback
        {
            get => Preferences.Get("hapticFeedback", false);
            set
            {
                if (value != HapticFeedback)
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
                if (value != LeverMoveMode)
                {
                    Preferences.Set("leverMoveMode", (int)value);
                    OnValueChanged();
                }
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

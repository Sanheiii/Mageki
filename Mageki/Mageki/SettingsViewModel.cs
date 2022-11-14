using PropertyChanged;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;

using Xamarin.Essentials;
using Xamarin.Forms;
using System.Linq;

namespace Mageki
{
    [AddINotifyPropertyChangedInterface]
    public class SettingsViewModel
    {
        // Math.Pow(10, 0.1)
        private const double leverSensitivityBase = 1.2589254117941673;

        public Protocols Protocol { get => Settings.Protocol; set => Settings.Protocol = value; }
        public int ProtocolIndex { get => (int)Protocol; set => Protocol = (Protocols)value; }
        public List<string> Protocols => Enum.GetNames(typeof(Protocols)).ToList();
        public ushort Port { get => Settings.Port; set => Settings.Port = value; }
        // 0 => 1 , -10 => 0.1 , 10 => 10
        public float LeverSensitivity { get => (float)Math.Log(Settings.LeverSensitivity, leverSensitivityBase); set => Settings.LeverSensitivity = (float)Math.Pow(leverSensitivityBase, value); }

        public double MaxLeverLinearity => Settings.MaxLeverLinearity;
        public double MinLeverLinearity => Settings.MinLeverLinearity;
        public int LeverLinearity { get => Settings.LeverLinearity; set => Settings.LeverLinearity = value; }

        public float ButtonBottomMargin { get => Settings.ButtonBottomMargin; set => Settings.ButtonBottomMargin = value; }

        public string Aimeid { get => Settings.AimeId; set => Settings.AimeId = value; }

        public bool SeparateButtonsAndLever { get => Settings.SeparateButtonsAndLever; set => Settings.SeparateButtonsAndLever = value; }

        public bool HideButtons { get => Settings.HideButtons; set => Settings.HideButtons = value; }

        public bool HapticFeedback { get => Settings.HapticFeedback; set => Settings.HapticFeedback = value; }

        public Version Version => Version.Parse(VersionTracking.CurrentVersion);

        public SettingsViewModel()
        {

        }
    }
    public enum Protocols
    {
        UDP = 0,
        TCP = 1
    }
    public static class Settings
    {
        public const double MaxLeverLinearity = 540;
        public const double MinLeverLinearity = 54;
        public static Protocols Protocol
        {
            get => (Protocols)Preferences.Get("protocol", (int)Protocols.TCP);
            set
            {
                Preferences.Set("protocol", (int)value);
                OnValueChanged();
            }
        }
        public static ushort Port
        {
            get => (ushort)Preferences.Get("port", 4354);
            set
            {
                Preferences.Set("port", value);
                OnValueChanged();
            }
        }
        public static bool SeparateButtonsAndLever
        {
            get => Preferences.Get("separateButtonsAndLever", false);
            set
            {
                Preferences.Set("separateButtonsAndLever", value);
                OnValueChanged();
            }
        }
        public static bool HideButtons
        {
            get => Preferences.Get("hideButtons", false);
            set
            {
                Preferences.Set("hideButtons", value);
                OnValueChanged();
            }
        }
        public static bool HapticFeedback
        {
            get => Preferences.Get("hapticFeedback", false);
            set
            {
                Preferences.Set("hapticFeedback", value);
                OnValueChanged();
            }
        }
        public static float LeverSensitivity
        {
            get => Preferences.Get("leverSensitivity", 1f);
            set
            {
                Preferences.Set("leverSensitivity", value);
                OnValueChanged();
            }
        }
        public static int LeverLinearity
        {
            get => Preferences.Get("leverLinearity", (int)MaxLeverLinearity);
            set
            {
                if (value < MinLeverLinearity) value = (int)MinLeverLinearity;
                else if (value > MaxLeverLinearity) value = (int)MaxLeverLinearity;
                Preferences.Set("leverLinearity", value);
                OnValueChanged();
            }
        }
        public static float ButtonBottomMargin
        {
            get => Preferences.Get("buttonBottomMargin", 0.2f);
            set
            {
                Preferences.Set("buttonBottomMargin", value);
                OnValueChanged();
            }
        }
        public static string AimeId
        {
            get => Preferences.Get("aimeId", string.Empty);
            set
            {
                Preferences.Set("aimeId", value);
                OnValueChanged();
            }
        }

        public static Version IgnoredVersion
        {
            get => Version.Parse(Preferences.Get("ignoredVersion", "0.0.0"));
            set
            {
                Preferences.Set("ignoredVersion", value.ToString());
                OnValueChanged();
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

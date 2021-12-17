using PropertyChanged;

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

using Xamarin.Essentials;
using Xamarin.Forms;

namespace Mageki
{
    [AddINotifyPropertyChangedInterface]
    public class SettingsViewModel
    {
        // Math.Pow(10, 0.1)
        private const double leverSensitivityBase = 1.2589254117941673;

        public int Port { get => Settings.Port; set => Settings.Port = value; }
        // 0 => 1 , -10 => 0.1 , 10 => 10
        public float LeverSensitivity { get => (float)Math.Log(Settings.LeverSensitivity, leverSensitivityBase); set => Settings.LeverSensitivity = (float)Math.Pow(leverSensitivityBase, value); }

        public bool UseSimplifiedLayout { get => Settings.UseSimplifiedLayout; set => Settings.UseSimplifiedLayout = value; }

        public SettingsViewModel()
        {

        }
    }
    public class Settings
    {
        public static int Port
        {
            get => Preferences.Get("port", 4354);
            set => Preferences.Set("port", value);
        }
        public static bool UseSimplifiedLayout
        {
            get => Preferences.Get("useSimplifiedLayout", true);
            set => Preferences.Set("useSimplifiedLayout", value);
        }
        public static float LeverSensitivity
        {
            get => Preferences.Get("leverSensitivity", 1f);
            set => Preferences.Set("leverSensitivity", value);
        }
    }
}

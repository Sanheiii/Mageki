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
        public int Port { get => Settings.Port; set => Settings.Port = value; }
        public bool UseSimplifiedLayout { get => Settings.UseSimplifiedLayout; set => Settings.UseSimplifiedLayout = value; }

        public string TestText { get; set; }

        public SettingsViewModel()
        {
            TestText = "Hello Xamarin.Forms!";
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
    }
}

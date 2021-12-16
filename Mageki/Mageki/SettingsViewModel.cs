using PropertyChanged;

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

using Xamarin.Forms;

namespace Mageki
{
    [AddINotifyPropertyChangedInterface]
    public class SettingsViewModel
    {
        public string TestText { get; set; }

        public SettingsViewModel()
        {
            TestText = "Hello Xamarin.Forms!";
        }
    }
}

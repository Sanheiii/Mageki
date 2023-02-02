using Mageki.Drawables;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using Xamarin.Forms;

namespace Mageki.Views
{
    public class PortConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = (string)value;
            EntryCell cell = (EntryCell)parameter;
            SettingsViewModel viewModel = (SettingsViewModel)cell.BindingContext;
            if (ushort.TryParse(str, out ushort num1))
            {
                return num1;
            }
            else
            {
                cell.Text = viewModel.Port.ToString();
                return viewModel.Port;
            }
        }
    }
    public class AimeIdConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = (string)value;
            EntryCell cell = (EntryCell)parameter;
            SettingsViewModel viewModel = (SettingsViewModel)cell.BindingContext;
            if (str.Length > 20)
            {
                cell.Text = viewModel.Aimeid;
                str = viewModel.Aimeid;
            }
            return str;
        }
    }
    public class ProtocolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Protocol protocol)
            {
                return (int)protocol;
            }
            else return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int i)
            {
                return (Protocol)i;
            }
            else return default(Protocol);
        }
    }
    public class LeverMoveModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is LeverMoveMode mode)
            {
                return (int)mode;
            }
            else return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int i)
            {
                return (LeverMoveMode)i;
            }
            else return default(LeverMoveMode);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;

namespace Mageki
{
    public class EntryCell : Xamarin.Forms.EntryCell
    {
        public static readonly BindableProperty TextColorProperty = BindableProperty.Create("TextColor", typeof(Color), typeof(EntryCell), Color.Default, BindingMode.OneWay, null, OnTextColorPropertyChanged);
        public static readonly BindableProperty PlaceholderColorProperty = BindableProperty.Create("PlaceholderColor", typeof(Color), typeof(EntryCell), default(Color));

        private static void OnTextColorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {

        }

        public Color TextColor
        {
            get
            {
                return (Color)GetValue(TextColorProperty);
            }
            set
            {
                SetValue(TextColorProperty, value);
            }
        }
        public Color PlaceholderColor
        {
            get
            {
                return (Color)GetValue(PlaceholderColorProperty);
            }
            set
            {
                SetValue(PlaceholderColorProperty, value);
            }
        }
    }
}

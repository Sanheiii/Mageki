using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Mageki
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NavigationBar : ContentView
    {
        public static readonly BindableProperty TitleProperty = BindableProperty.Create(
               propertyName: nameof(Title),
               returnType: typeof(string),
               declaringType: typeof(NavigationBar),
               defaultValue: "");

        public static readonly BindableProperty GoBackCommandProperty = BindableProperty.Create(
               propertyName: nameof(GoBackCommand),
               returnType: typeof(Command),
               declaringType: typeof(NavigationBar));

        public static readonly BindableProperty GoBackCommandParameterProperty = BindableProperty.Create(
               propertyName: nameof(GoBackCommandParameter),
               returnType: typeof(object),
               declaringType: typeof(NavigationBar));
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set
            {
                SetValue(TitleProperty, value);
                OnPropertyChanged(nameof(Title));
            }
        }
        public Command GoBackCommand
        {
            get => (Command)GetValue(GoBackCommandProperty);
            set
            {
                SetValue(GoBackCommandProperty, value);
                OnPropertyChanged(nameof(GoBackCommand));
            }
        }
        public object GoBackCommandParameter
        {
            get => GetValue(GoBackCommandParameterProperty);
            set
            {
                SetValue(GoBackCommandParameterProperty, value);
                OnPropertyChanged(nameof(GoBackCommandParameter));
            }
        }


        public NavigationBar()
        {
            InitializeComponent();
        }
    }
}

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Widget;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

using Color = Xamarin.Forms.Color;
using View = Android.Views.View;

[assembly: ExportRenderer(typeof(Mageki.EntryCell), typeof(Mageki.Droid.Renderers.EntryCellRenderer))]
namespace Mageki.Droid.Renderers
{
    internal class EntryCellRenderer : Xamarin.Forms.Platform.Android.EntryCellRenderer
    {
        private EntryCellView _view;
        private Mageki.EntryCell Element => _view.Element as Mageki.EntryCell;
        private EditText EditText => _view.EditText;
        private TextColorSwitcher _textColorSwitcher;
        private TextColorSwitcher _hintColorSwitcher;

        protected override View GetCellCore(Xamarin.Forms.Cell item, View convertView, ViewGroup parent, Context context)
        {
            _view = (EntryCellView)base.GetCellCore(item, convertView, parent, context);
            UpdatePlaceholderColor();
            UpdateTextColor();
            return _view;
        }

        protected override void OnCellPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Element.TextColor):
                    UpdateTextColor();
                    break;
                case nameof(Element.PlaceholderColor):
                    UpdatePlaceholderColor();
                    break;
            }
            base.OnCellPropertyChanged(sender, e);
        }

        protected void UpdateTextColor()
        {
            var color = Element.TextColor;
            _textColorSwitcher = (_textColorSwitcher ?? new TextColorSwitcher(EditText.TextColors));
            _textColorSwitcher.UpdateTextColor(EditText, color);
            EditText.Background.Mutate().SetColorFilter(color.ToAndroid(), PorterDuff.Mode.SrcAtop);
        }
        protected void UpdatePlaceholderColor()
        {
            _hintColorSwitcher = (_hintColorSwitcher ?? new TextColorSwitcher(EditText.HintTextColors));
            _hintColorSwitcher.UpdateTextColor(EditText, Element.PlaceholderColor, EditText.SetHintTextColor);
        }
    }
    internal class TextColorSwitcher
    {
        private static readonly int[][] s_colorStates = new int[2][]
        {
            new int[1]
            {
                16842910
            },
            new int[1]
            {
                -16842910
            }
        };

        private readonly ColorStateList _defaultTextColors;

        private readonly bool _useLegacyColorManagement;

        private Color _currentTextColor;

        public TextColorSwitcher(ColorStateList textColors, bool useLegacyColorManagement = true)
        {
            _defaultTextColors = textColors;
            _useLegacyColorManagement = useLegacyColorManagement;
        }

        public void UpdateTextColor(TextView control, Color color, Action<ColorStateList> setColor = null)
        {
            if (!(color == _currentTextColor))
            {
                if (setColor == null)
                {
                    setColor = control.SetTextColor;
                }

                _currentTextColor = color;
                if (color.IsDefault)
                {
                    setColor(_defaultTextColors);
                }
                else if (_useLegacyColorManagement)
                {
                    int colorForState = _defaultTextColors.GetColorForState(s_colorStates[1], color.ToAndroid());
                    setColor(new ColorStateList(s_colorStates, new int[2]
                    {
                        color.ToAndroid().ToArgb(),
                        colorForState
                    }));
                }
                else
                {
                    int num = color.ToAndroid().ToArgb();
                    setColor(new ColorStateList(s_colorStates, new int[2]
                    {
                        num,
                        num
                    }));
                }
            }
        }
    }
}
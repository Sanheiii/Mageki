using Rg.Plugins.Popup.Extensions;

using SkiaSharp;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Timers;

using Xamarin.Forms;

namespace Mageki.Drawables
{
    public class SettingButton : ButtonBase
    {
        private SettingsPopup settingPopup;
        private ControllerPanel controller;

        SKPaint borderPaint = new SKPaint()
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 5,
            Color = new SKColor(0x44222222),
        };

        SKPaint borderPressedPaint = new SKPaint()
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 5,
            Color = new SKColor(0xAA222222),
        };
        SKPaint iconPaint;
        SKPaint iconPressedPaint;
        public SettingButton(ControllerPanel controller)
        {
            this.controller = controller;

            using var fontStream = GetType().Assembly.GetManifestResourceStream("Mageki.Assets.Fonts.MaterialIconsRound-Regular.otf");
            using var typeface = SKTypeface.FromStream(fontStream);
            var font = new SKFont(typeface);
            iconPaint = new SKPaint(font)
            {
                Color = borderPaint.Color,
                TextAlign = SKTextAlign.Center,
                Style = SKPaintStyle.StrokeAndFill
            };
            iconPressedPaint = new SKPaint(font)
            {
                Color = borderPressedPaint.Color,
                TextAlign = iconPaint.TextAlign,
                Style = iconPaint.Style
            };
        }

        public override void Draw(SKCanvas canvas)
        {
            if(!Visible) return;
            var boundingBox = BoundingBox;
            boundingBox.Inflate(-Padding.X, -Padding.Y);
            var fontRect = SKRect.Inflate(boundingBox, boundingBox.Height * -0.1f, boundingBox.Height * -0.1f);
            iconPaint.TextSize = iconPressedPaint.TextSize = fontRect.Height;
            var corner = boundingBox.Height * 0.1f;
            SKRect bounds = default;
            char icon = '';
            iconPaint.MeasureText(icon.ToString(), ref bounds);
            if (TouchCount == 0)
            {
                canvas.DrawRoundRect(boundingBox, corner, corner, borderPaint);
                canvas.DrawText(icon.ToString(), new SKPoint(fontRect.MidX, fontRect.Bottom), iconPaint);
            }
            else
            {
                canvas.DrawRoundRect(boundingBox, corner, corner, borderPressedPaint);
                canvas.DrawText(icon.ToString(), new SKPoint(fontRect.MidX, fontRect.Bottom), iconPressedPaint);
            }
            base.Draw(canvas);
        }

        public override void HandleTouchReleased(long id)
        {
            if (touchPoints.ContainsKey(id) && HitTest(touchPoints[id]))
            {
                Pressed();
            }
            base.HandleTouchReleased(id);
        }

        private void Pressed()
        {
            ShowSettingPopup();
        }
        bool flag = false;
        private async void ShowSettingPopup()
        {
            if (flag) return;
            flag = true;
            var popup = settingPopup ?? (settingPopup = new SettingsPopup());
            try
            {
                await Application.Current.MainPage.Navigation.PushPopupAsync(popup);
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex);
            }
            flag = false;
        }
    }
}

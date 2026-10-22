

using SkiaSharp;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Mageki.Services;
using Mageki.Utils;
using Microsoft.Maui.Controls;


namespace Mageki.Drawables
{
    public class SettingButton : ButtonBase
    {
        private readonly INfcService nfcService;
        private readonly ISettingsPopupService settingsPopupService;
        private bool nfcScanning = false;
        private bool logoHoldUnhandled = false;
        private int holdGeneration;
        DateTime scanTime;

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
        SKFont iconFont;
        SKTypeface? iconTypeface;
        bool materialIconAvailable;
        public SettingButton(INfcService nfcService, ISettingsPopupService settingsPopupService)
        {
            this.nfcService = nfcService;
            this.settingsPopupService = settingsPopupService;
            using var fontStream = GetType().Assembly.GetManifestResourceStream("Mageki.MaterialIconsRound");
            iconTypeface = fontStream is null ? null : SKTypeface.FromStream(fontStream);
            materialIconAvailable = iconTypeface is not null;
            iconFont = new SKFont(iconTypeface ?? SKTypeface.Default)
            {
                Edging = SKFontEdging.Antialias,
                Subpixel = true
            };
            iconPaint = new SKPaint
            {
                Color = borderPaint.Color,
                Style = SKPaintStyle.StrokeAndFill
            };
            iconPressedPaint = new SKPaint
            {
                Color = borderPressedPaint.Color,
                Style = iconPaint.Style
            };

            // 安卓平台可以免提示使用NFC刷卡
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                try
                {
                    nfcService.StartReadAime(ScanFelica, ScanMifare, () => { });
                }
                catch (Exception ex)
                {
                    App.Logger.Error(ex);
                }
            }
        }

        public override void Draw(SKCanvas canvas)
        {
            if (!Visible) return;
            var boundingBox = BoundingBox;
            boundingBox.Inflate(-Padding.X, -Padding.Y);
            var fontRect = SKRect.Inflate(boundingBox, boundingBox.Height * -0.1f, boundingBox.Height * -0.1f);
            iconFont.Size = fontRect.Height;
            var corner = boundingBox.Height * 0.1f;
            SKRect bounds = default;
            char icon = '\ue8b8';
            iconFont.MeasureText(icon.ToString(), out bounds);
            var textPoint = new SKPoint(fontRect.MidX - bounds.MidX, fontRect.Bottom);
            if (TouchCount == 0)
            {
                canvas.DrawRoundRect(boundingBox, corner, corner, borderPaint);
                DrawIcon(canvas, fontRect, icon.ToString(), textPoint, iconPaint);
            }
            else
            {
                canvas.DrawRoundRect(boundingBox, corner, corner, borderPressedPaint);
                DrawIcon(canvas, fontRect, icon.ToString(), textPoint, iconPressedPaint);
            }
            base.Draw(canvas);
        }

        private void DrawIcon(SKCanvas canvas, SKRect fontRect, string icon, SKPoint textPoint, SKPaint paint)
        {
            if (materialIconAvailable)
            {
                canvas.DrawText(icon, textPoint.X, textPoint.Y, iconFont, paint);
                return;
            }

            DrawGearFallback(canvas, fontRect, paint);
        }

        private static void DrawGearFallback(SKCanvas canvas, SKRect rect, SKPaint paint)
        {
            using var gearPaint = new SKPaint
            {
                Color = paint.Color,
                Style = SKPaintStyle.Stroke,
                StrokeCap = SKStrokeCap.Round,
                StrokeWidth = rect.Width * 0.08f,
                IsAntialias = true
            };

            var center = new SKPoint(rect.MidX, rect.MidY);
            var outerRadius = rect.Width * 0.34f;
            var innerRadius = rect.Width * 0.16f;
            canvas.DrawCircle(center, innerRadius, gearPaint);

            for (var i = 0; i < 8; i++)
            {
                var angle = MathF.PI * 2 * i / 8;
                var start = new SKPoint(center.X + MathF.Cos(angle) * outerRadius * 0.72f,
                    center.Y + MathF.Sin(angle) * outerRadius * 0.72f);
                var end = new SKPoint(center.X + MathF.Cos(angle) * outerRadius,
                    center.Y + MathF.Sin(angle) * outerRadius);
                canvas.DrawLine(start, end, gearPaint);
            }
        }
        public override bool HandleTouchPressed(long id, SKPoint point)
        {
            if (touchPoints.ContainsKey(id)) return true;
            var handled = base.HandleTouchPressed(id, point);
            if (!(nfcScanning || logoHoldUnhandled))
            {
                logoHoldUnhandled = true;
                scanTime = DateTime.Now;
                if (DeviceInfo.Platform != DevicePlatform.iOS || !nfcService.ReadingAvailable)
                {
                    StaticIO.SetAime(1, GetSimulatedAimeId());
                }
                //符合条件的iOS机型长按一秒打开读卡菜单，取消后可以继续模拟刷卡
                else
                {
                    _ = ReadAfterHoldAsync(id, ++holdGeneration);
                }
            }
            return handled;
        }

        private async Task ReadAfterHoldAsync(long id, int generation)
        {
            await Task.Delay(1000);
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (generation != holdGeneration || !logoHoldUnhandled ||
                    !touchPoints.TryGetValue(id, out var point) || !HitTest(point)) return;
                HandleTouchReleased(id);
                try
                {
                    nfcService.StartReadAime(ScanFelica, ScanMifare, ScanFelicaInvalidated);
                }
                catch (Exception ex)
                {
                    App.Logger.Error(ex);
                    ScanFelicaInvalidated();
                }
            });
        }

        public override void HandleTouchReleased(long id)
        {
            if (touchPoints.TryGetValue(id, out var point))
            {
                bool showSettings = logoHoldUnhandled && HitTest(point) &&
                    DateTime.Now - scanTime < TimeSpan.FromSeconds(0.3);
                logoHoldUnhandled = false;
                holdGeneration++;
                if (!nfcScanning) StaticIO.SetAime(0, Array.Empty<byte>());
                base.HandleTouchReleased(id);
                if (showSettings) Pressed();
                return;
            }
            base.HandleTouchReleased(id);
        }

        public override void HandleTouchCancelled(long id)
        {
            logoHoldUnhandled = false;
            base.HandleTouchCancelled(id);
        }

        private void Pressed()
        {
            _ = settingsPopupService.ShowAsync();
        }



        public void ScanFelica(byte[] packet)
        {
            MainThread.BeginInvokeOnMainThread(() => _ = ScanAsync(2, packet));
        }

        public void ScanMifare(byte[] packet)
        {
            MainThread.BeginInvokeOnMainThread(() => _ = ScanAsync(1, packet));
        }

        private async Task ScanAsync(byte type, byte[] packet)
        {
            if (nfcScanning) return;
            nfcScanning = true;
            try
            {
                StaticIO.SetAime(type, packet);
                await Task.Delay(3000);
            }
            finally
            {
                StaticIO.SetAime(0, Array.Empty<byte>());
                nfcScanning = false;
            }
        }

        private void ScanFelicaInvalidated()
        {
            MainThread.BeginInvokeOnMainThread(() => ScanMifare(GetSimulatedAimeId()));
        }

        byte[] GetSimulatedAimeId()
        {
            byte[] aimeId = Enumerable.Range(0, 10).Select((i) => (byte)255).ToArray();
            if (Settings.AimeId.Length <= 20 && Settings.AimeId.All(c => c is >= '0' and <= '9') &&
                BigInteger.TryParse(Settings.AimeId, out BigInteger integer))
            {
                var bcd = integer.ToBcd();
                var bytes = new byte[10 - bcd.Length].Concat(bcd);
                aimeId = bytes.ToArray();
            }

            return aimeId;
        }
    }
}

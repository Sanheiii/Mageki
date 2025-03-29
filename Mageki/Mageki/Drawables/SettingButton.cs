using Rg.Plugins.Popup.Extensions;

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
using Mageki.Utils;
using Xamarin.Forms;
using Mageki.DependencyServices;
using Xamarin.Essentials;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using Application = Xamarin.Forms.Application;

namespace Mageki.Drawables
{
    public class SettingButton : ButtonBase
    {
        private SettingsPopup settingPopup;
        private bool nfcScanning = false;
        private bool logoHoldUnhandled = false;
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
        public SettingButton()
        {
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

            // 安卓平台可以免提示使用NFC刷卡
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                try
                {
                    DependencyService.Get<INfcService>().StartReadAime(ScanFelica, ScanMifare, () => { });
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

        public override bool HandleTouchPressed(long id, SKPoint point)
        {
            if (!(nfcScanning || logoHoldUnhandled))
            {
                logoHoldUnhandled = true;
                scanTime = DateTime.Now;
                if (DeviceInfo.Platform != DevicePlatform.iOS || !DependencyService.Get<INfcService>().ReadingAvailable)
                {
                    StaticIO.SetAime(1, GetSimulatedAimeId());
                }
                //符合条件的iOS机型长按一秒打开读卡菜单，取消后可以继续模拟刷卡
                else
                {
                    Timer timer = new Timer(1000) { AutoReset = false };
                    timer.Elapsed += (sender, e) =>
                    {
                        MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            if (logoHoldUnhandled)
                            {
                                HandleTouchReleased(id);
                                logoHoldUnhandled = false;
                                var nfcService = DependencyService.Get<INfcService>();
                                nfcService.StartReadAime(ScanFelica, ScanMifare, ScanFelicaInvalidated);
                                timer.Dispose();
                            }
                        });
                    };
                    timer.Start();
                }
            }
            return base.HandleTouchPressed(id, point);
        }

        public override void HandleTouchReleased(long id)
        {
            if (touchPoints.ContainsKey(id) && HitTest(touchPoints[id]))
            {
                logoHoldUnhandled = false;
                StaticIO.SetAime(0, Array.Empty<byte>());
                // 按下超过一定时长不触发菜单
                if (DateTime.Now - scanTime < TimeSpan.FromSeconds(0.3))
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
            var popup = settingPopup ??= new SettingsPopup();
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



        public async void ScanFelica(byte[] packet)
        {
            if (nfcScanning) return;

            string idmString = "0x" + BitConverter.ToUInt64(packet[0..8].ToArray(), 0).ToString("X16");
            string pmMString = "0x" + BitConverter.ToUInt64(packet[8..16].ToArray(), 0).ToString("X16");
            string systemCodeString = BitConverter.ToUInt16(packet[16..18].ToArray(), 0).ToString("X4");
            App.Logger.Debug(
                $"FeliCa card is present\nIDm: {idmString}\nPMm: {pmMString}\nSystemCode: {systemCodeString}");

            nfcScanning = true;
            StaticIO.SetAime(2, packet);
            await Task.Delay(3000);
            StaticIO.SetAime(0, Array.Empty<byte>());
            nfcScanning = false;
        }

        public async void ScanMifare(byte[] packet)
        {
            if (nfcScanning) return;

            App.Logger.Debug($"Mifare card is present\nAccessCode: {1}");

            nfcScanning = true;
            StaticIO.SetAime(1, packet);
            await Task.Delay(3000);
            StaticIO.SetAime(0, Array.Empty<byte>());
            nfcScanning = false;
        }

        private void ScanFelicaInvalidated()
        {
            ScanMifare(GetSimulatedAimeId());
        }

        byte[] GetSimulatedAimeId()
        {
            byte[] aimeId = Enumerable.Range(0, 10).Select((i) => (byte)255).ToArray();
            if (BigInteger.TryParse(Settings.AimeId, out BigInteger integer))
            {
                var bcd = integer.ToBcd();
                var bytes = new byte[10 - bcd.Length].Concat(bcd);
                aimeId = bytes.ToArray();
            }

            return aimeId;
        }
    }
}

using Mageki.Drawables;
using Mageki.TouchTracking;
using Mageki.Utils;

using Plugin.FelicaReader;
using Plugin.FelicaReader.Abstractions;

using SkiaSharp;
using SkiaSharp.Views.Forms;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

using Xamarin.Essentials;
using Xamarin.Forms;

using Button = Mageki.Drawables.Button;
using DeviceInfo = Xamarin.Essentials.DeviceInfo;
using Slider = Mageki.Drawables.Slider;

namespace Mageki
{
    public class ControllerPanel : Grid
    {
        private IO io;
        private bool requireGenRects = false;
        private ButtonCollection buttons = new ButtonCollection();
        //private Button[] buttons = Enumerable.Range(0, 10).Select((n) => new Button()).ToArray();
        private IFelicaReader felicaReader;
        private IDisposable subscription;
        private IDrawable[] decorations = new IDrawable[]
        {
            new Circles(),
            new Circles(),
        };

        Logo logo = new Logo();

        private Slider slider = new Slider();

        #region 常量
        const float PanelMarginCoef = 0.5f;
        const float LRSpacingCoef = 0.5f;
        const float BMSpacingCoef = 0.75f;
        const float ButtonSpacingCoef = 0.25f;
        const float MenuSizeCoef = 0.5f;
        #endregion

        /// <summary>
        /// 绘图面板
        /// </summary>
        SKCanvasView canvasView = new SKCanvasView();
        /// <summary>
        /// 捕获点击
        /// </summary>
        TouchEffect touchEffect = new TouchEffect { Capture = true };
        /// <summary>
        /// 保存多点触摸的数据
        /// </summary>
        Dictionary<long, (TouchArea touchArea, SKPoint position)> touchPoints = new Dictionary<long, (TouchArea, SKPoint)>();

        bool inRhythmGame;
        private bool nfcScanning = false;
        private bool simulateScanning = false;

        public delegate void ClickEventHandler(object sender, EventArgs args);
        public event ClickEventHandler LogoClickd;


        public ControllerPanel()
        {
            //添加绘图面板
            Children.Add(canvasView);
            //注册绘图方法
            canvasView.PaintSurface += CanvasView_PaintSurface;
            //捕获点击
            touchEffect.TouchAction += TouchEffect_TouchAction;
            Effects.Add(touchEffect);
            // 安卓平台可以使用NFC刷卡
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                try
                {
                    this.felicaReader = CrossFelicaReader.Current;
                    this.subscription = this.felicaReader.WhenCardFound().Subscribe(new FelicaCardMediaObserver(ScanFelica));
                }
                catch (Exception ex) { }
            }
            io = new UdpIO();
            io.Init();
            Settings.ValueChanged += Settings_ValueChanged;

        }

        private void Settings_ValueChanged(string name)
        {
            if (name == nameof(Settings.ButtonBottomMargin))
            {
                ForceGenRects();
            }
            if (name == nameof(Settings.Protocol))
            {
                if (!(io is UdpIO) && Settings.Protocol == Protocols.Udp)
                {
                    io.Dispose();
                    io = new UdpIO();
                    io.Init();
                }
                else if (!(io is TcpIO) && Settings.Protocol == Protocols.Tcp)
                {
                    io.Dispose();
                    io = new TcpIO();
                    io.Init();
                }
            }
        }

        public void ForceGenRects()
        {
            requireGenRects = true;
            MainThread.InvokeOnMainThreadAsync(canvasView.InvalidateSurface);
        }

        public async void ScanFelica(byte[] felicaId)
        {
            if (nfcScanning || simulateScanning) return;
            nfcScanning = true;
            io.SetAime(true, new BigInteger(felicaId).ToBcd());
            await Task.Delay(3000);
            io.SetAime(false, new byte[10]);
            nfcScanning = false;
        }

        public async Task PressAndReleaseTestButtonAsync()
        {
            io.SetOptionButton(OptionButtons.Test, true);
            await Task.Delay(1000);
            io.SetOptionButton(OptionButtons.Test, false);
        }
        public async Task PressAndReleaseServiceButtonAsync()
        {
            io.SetOptionButton(OptionButtons.Service, true);
            await Task.Delay(1000);
            io.SetOptionButton(OptionButtons.Service, false);
        }

        int oldWidth = -1;
        int oldHeight = -1;
        /// <summary>
        /// 绘图
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanvasView_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            //获取绘图面板的信息
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;
            //清空画布
            canvas.Clear(SKColors.White);
            if (oldWidth != info.Width || oldHeight != info.Height || requireGenRects)
            {
                requireGenRects = false;
                GenRects(info.Width, info.Height);
            }
            foreach (IDrawable drawable in decorations)
            {
                drawable.Draw(canvas);
            }
            buttons.Draw(canvas, inRhythmGame && Settings.UseSimplifiedLayout);
            logo.Draw(canvas);
            //canvas.DrawRect(slider.BackRect, slider.BackPaint);
            //canvas.DrawRect(slider.LeverRect, slider.LeverPaint);
            oldWidth = info.Width;
            oldHeight = info.Height;
        }
        /// <summary>
        /// 计算各个元素的位置和大小
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        private void GenRects(int width, int height)
        {
            float buttonWidth = (width / (PanelMarginCoef * 2 + LRSpacingCoef * 1 + ButtonSpacingCoef * 4 + 6));
            float buttonHeight = buttonWidth;
            float menuSideLength = buttonHeight * MenuSizeCoef;
            float panelMargin = buttonHeight * PanelMarginCoef;
            float lrSpacing = buttonHeight * LRSpacingCoef;
            float bmSpacing = buttonHeight * BMSpacingCoef;
            float bottomMargin = (height - buttonHeight - bmSpacing - menuSideLength) * Settings.ButtonBottomMargin;
            float buttonSpacing = buttonHeight * ButtonSpacingCoef;

            float buttonBottom = height - bottomMargin;
            if (inRhythmGame && Settings.UseSimplifiedLayout)
            {
                // 打歌时以特殊布局绘制
                float specialButtonWidth = (width - panelMargin * 2 - buttonSpacing * 2) / 4;
                // Button 1
                buttons.S1.Color = buttons.L1.Color;
                buttons.S1.Height = buttonHeight;
                buttons.S1.Width = specialButtonWidth;
                buttons.S1.Center = new SKPoint(panelMargin + buttonSpacing * 0 + specialButtonWidth * 0.5f, buttonBottom - buttonHeight / 2);
                // Button 2
                buttons.S2.Color = buttons.L2.Color;
                buttons.S2.Height = buttonHeight;
                buttons.S2.Width = specialButtonWidth * 2;
                buttons.S2.Center = new SKPoint(panelMargin + buttonSpacing * 1 + specialButtonWidth * 2f, buttonBottom - buttonHeight / 2);
                // Button 3
                buttons.S3.Color = buttons.L3.Color;
                buttons.S3.Height = buttonHeight;
                buttons.S3.Width = specialButtonWidth;
                buttons.S3.Center = new SKPoint(panelMargin + buttonSpacing * 2 + specialButtonWidth * 3.5f, buttonBottom - buttonHeight / 2);
            }
            // Left 1
            buttons.L1.Width = buttons.L1.Height = buttonWidth;
            buttons.L1.Center = new SKPoint(panelMargin + buttonSpacing * 0 + buttonWidth * 0.5f, buttonBottom - buttonHeight / 2);
            // Left 2
            buttons.L2.Width = buttons.L2.Height = buttonWidth;
            buttons.L2.Center = new SKPoint(panelMargin + buttonSpacing * 1 + buttonWidth * 1.5f, buttonBottom - buttonHeight / 2);
            // Left 3
            buttons.L3.Width = buttons.L3.Height = buttonWidth;
            buttons.L3.Center = new SKPoint(panelMargin + buttonSpacing * 2 + buttonWidth * 2.5f, buttonBottom - buttonHeight / 2);
            // Left side 不作绘制
            //buttons.LSide.Center = default;
            // Left menu
            buttons.LMenu.Width = buttons.LMenu.Height = menuSideLength;
            buttons.LMenu.BorderColor = new SKColor(0xFF880000);
            buttons.LMenu.Color = ButtonColors.Red;
            buttons.LMenu.Center = new SKPoint(panelMargin + buttonWidth - buttonSpacing, buttonBottom - buttonHeight - bmSpacing - menuSideLength / 2);

            //-------------------
            // Right 1
            buttons.R1.Width = buttons.R1.Height = buttonWidth;
            buttons.R1.Center = new SKPoint(panelMargin + buttonSpacing * 2 + buttonWidth * 3.5f + lrSpacing, buttonBottom - buttonHeight / 2);
            // Right 2
            buttons.R2.Width = buttons.R2.Height = buttonWidth;
            buttons.R2.Center = new SKPoint(panelMargin + buttonSpacing * 3 + buttonWidth * 4.5f + lrSpacing, buttonBottom - buttonHeight / 2);
            // Right 3
            buttons.R3.Width = buttons.R3.Height = buttonWidth;
            buttons.R3.Center = new SKPoint(panelMargin + buttonSpacing * 4 + buttonWidth * 5.5f + lrSpacing, buttonBottom - buttonHeight / 2);
            // Right side 不作绘制
            //buttons.RSide.Center = default;
            // Right menu
            buttons.RMenu.Width = buttons.RMenu.Height = menuSideLength;
            buttons.RMenu.BorderColor = new SKColor(0xFF888800);
            buttons.RMenu.Color = ButtonColors.Yellow;
            buttons.RMenu.Center = new SKPoint(width - (panelMargin + buttonWidth - buttonSpacing), buttonBottom - buttonHeight - bmSpacing - menuSideLength / 2);
            // 绘制装饰用的环
            if (decorations[0] is Circles circles0)
            {
                circles0.Center = buttons.L2.Center;
                circles0.Radius = buttonWidth + buttonSpacing;
                circles0.DrawLeftArc = true;
                circles0.DrawRightArc = false;
            }
            if (decorations[1] is Circles circles1)
            {
                circles1.Center = buttons.R2.Center;
                circles1.Radius = buttonWidth + buttonSpacing;
                circles1.DrawLeftArc = false;
                circles1.DrawRightArc = true;
            }
            // logo
            logo.MaxHeight = menuSideLength;
            logo.MaxWidth = width;
            logo.Center = new SKPoint(width / 2, buttonBottom - buttonHeight - bmSpacing - logo.MaxHeight * 1.5f);
        }
        private List<(float value, long touchID)> leverCache = new List<(float value, long touchID)>();
        DateTime scanTime;
        /// <summary>
        /// 处理画布点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void TouchEffect_TouchAction(object sender, TouchActionEventArgs args)
        {
            //点击位置转化为画布的坐标
            SKPoint pixelLocation = new SKPoint(
                (float)(canvasView.CanvasSize.Width * args.Location.X / Width),
                (float)(canvasView.CanvasSize.Height * args.Location.Y / Height));
            TouchArea currentArea = GetArea(pixelLocation, canvasView.CanvasSize.Width, canvasView.CanvasSize.Height);
            //处理点击事件
            switch (args.Type)
            {
                case TouchActionType.Pressed:
                    {
                        if (touchPoints.ContainsKey(args.Id)) break;
                        TouchArea area = currentArea;
                        touchPoints.Add(args.Id, (area, pixelLocation));
                        // 按下按键
                        if ((byte)area < 10)
                        {
                            io.SetGameButton((int)area, true);
                            buttons[(int)area].IsHold = true;
                            if (inRhythmGame && (int)area % 5 < 3) buttons[10..13][(int)area % 5].IsHold = true;
                        }
                        // 按下logo根据放开时间刷卡或显示菜单
                        else if (area == TouchArea.Logo)
                        {
                            if (!(nfcScanning || simulateScanning))
                            {
                                byte[] aimeId = Enumerable.Range(0, 10).Select((i) => (byte)255).ToArray();
                                simulateScanning = true;
                                scanTime = DateTime.Now;
                                if (BigInteger.TryParse(Settings.AimeId, out BigInteger integer))
                                {
                                    var bcd = integer.ToBcd();
                                    var bytes = new byte[10 - bcd.Length].Concat(bcd);
                                    aimeId = bytes.ToArray();
                                }
                                io.SetAime(true, aimeId);
                                if (Settings.HapticFeedback) HapticFeedback.Perform(HapticFeedbackType.Click);
                            }
                        }
                        break;
                    }
                case TouchActionType.Moved:
                    {
                        if (!touchPoints.ContainsKey(args.Id)) break;
                        TouchArea area = touchPoints[args.Id].touchArea;
                        if (Settings.SeparateButtonsAndLever && (int)area < 8 && (int)area % 5 < 3)
                        {
                            TouchArea xArea = GetArea(pixelLocation.X, canvasView.CanvasSize.Width);
                            // 如果按键区不触发摇杆则允许搓
                            if (area != xArea && (int)xArea < 8 && (int)xArea % 5 < 3)
                            {

                                io.SetGameButton((int)xArea, true);
                                io.SetGameButton((int)area, false);
                                buttons[(int)xArea].IsHold = true;
                                buttons[(int)area].IsHold = false;
                                touchPoints.Remove(args.Id);
                                touchPoints.Add(args.Id, (xArea, pixelLocation));
                            }
                        }
                        // 拖动触发摇杆，多个手指在同一帧移动时只会取最大值
                        else if (touchPoints.ContainsKey(args.Id))
                        {
                            lock (leverCache)
                            {
                                bool idDuplicated = leverCache.Any(c => c.touchID == args.Id);
                                leverCache.Add((pixelLocation.X - touchPoints[args.Id].position.X, args.Id));
                                if (idDuplicated)
                                {
                                    var max = leverCache.Max((a) => MathF.Abs(a.value));
                                    var value = leverCache.Find(a => MathF.Abs(a.value) == max).value;
                                    var currentValue = pixelLocation.X - touchPoints[args.Id].position.X;
                                    MoveLever(value);
                                    leverCache.Clear();
                                }
                            }
                            touchPoints.Remove(args.Id);
                            touchPoints.Add(args.Id, (area, pixelLocation));
                        }
                        break;
                    }
                case TouchActionType.Released:
                case TouchActionType.Cancelled:
                    {
                        void ReleaseTouchPoint(long id)
                        {
                            if (!touchPoints.ContainsKey(id)) return;
                            TouchArea area = touchPoints[id].touchArea;
                            if ((int)area < 10 && touchPoints.Count(p => p.Value.touchArea == area) < 2)
                            {
                                io.SetGameButton((int)area, false);
                                buttons[(int)area].IsHold = false;
                                if (inRhythmGame && (int)area % 5 < 3) buttons[10..13][(int)area % 5].IsHold = false;
                            }
                            else if (area == TouchArea.Logo && touchPoints.Count(p => p.Value.touchArea == area) < 2)
                            {
                                simulateScanning = false;
                                io.SetAime(false, new byte[10]);
                                // 按下超过一秒不触发菜单
                                if (DateTime.Now - scanTime < TimeSpan.FromSeconds(0.3) && currentArea == area)
                                    LogoClickd.Invoke(this, EventArgs.Empty);
                                if (Settings.HapticFeedback) HapticFeedback.Perform(HapticFeedbackType.Click);
                            }
                            touchPoints.Remove(id);
                        }
                        if (args.Type == TouchActionType.Cancelled && DeviceInfo.Platform == DevicePlatform.Android)
                        {
                            while (touchPoints.Count > 0)
                            {
                                ReleaseTouchPoint(touchPoints.First().Key);
                            }
                        }
                        else
                        {
                            ReleaseTouchPoint(args.Id);
                        }
                        // 释放按键

                        break;
                    }
            }
            //通知重绘画布
            canvasView.InvalidateSurface();
        }

        private void MoveLever(float x)
        {
            var threshold = short.MaxValue / (Settings.LeverLinearity / 2f);
            int part = (int)(slider.Value / threshold);
            var pixelWidth = Width * Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Density;
            var oldValue = slider.Value;
            slider.Value += (short)(x * (pixelWidth / 30) * Settings.LeverSensitivity);
            // check会导致iOS端崩溃，使用土方法检查溢出
            if (x < 0 && oldValue < slider.Value) slider.Value = short.MinValue;
            else if (x > 0 && oldValue > slider.Value) slider.Value = short.MaxValue;
            // 仅在经过分界的时候发包
            if ((int)(slider.Value / threshold) != part)
            {
                io.SetLever(slider.Value);
                return;
            }
        }

        private TouchArea GetArea(SKPoint pixelLocation, float width, float height)
        {
            TouchArea area;
            // 大概点到中间六键的范围就会触发
            if (pixelLocation.Y >= buttons.L1.BorderRect.Top - buttons.L1.BorderRect.Left)
            {
                return GetArea(pixelLocation.X, width);
            }
            else if (!inRhythmGame && buttons.LMenu.BorderRect.Contains(pixelLocation)) area = TouchArea.LMenu;
            else if (!inRhythmGame && buttons.RMenu.BorderRect.Contains(pixelLocation)) area = TouchArea.RMenu;
            else if (!inRhythmGame && logo.Rect.Contains(pixelLocation)) area = TouchArea.Logo;
            else if (pixelLocation.X < width / 2) area = TouchArea.LSide;
            else area = TouchArea.RSide;
            //Debug.WriteLine(area);
            return area;
        }
        private TouchArea GetArea(float x, float width)
        {
            TouchArea area;
            if (inRhythmGame && Settings.UseSimplifiedLayout)
            {
                if (x < width / 4 * 1) area = buttons.R1.IsHold ? TouchArea.LButton1 : TouchArea.RButton1;
                else if (x < width / 4 * 3) area = buttons.R2.IsHold ? TouchArea.LButton2 : TouchArea.RButton2;
                else area = buttons.R3.IsHold ? TouchArea.LButton3 : TouchArea.RButton3;
            }
            else
            {
                if (x < (buttons.L1.BorderRect.Right + buttons.L2.BorderRect.Left) / 2) area = TouchArea.LButton1;
                else if (x < (buttons.L2.BorderRect.Right + buttons.L3.BorderRect.Left) / 2) area = TouchArea.LButton2;
                else if (x < (buttons.L3.BorderRect.Right + buttons.R1.BorderRect.Left) / 2) area = TouchArea.LButton3;
                else if (x < (buttons.R1.BorderRect.Right + buttons.R2.BorderRect.Left) / 2) area = TouchArea.RButton1;
                else if (x < (buttons.R2.BorderRect.Right + buttons.R3.BorderRect.Left) / 2) area = TouchArea.RButton2;
                else area = TouchArea.RButton3;
            }
            return area;
        }

        public void SetLed(uint data)
        {

            buttons.L1.Color = (ButtonColors)((data >> 23 & 1) << 2 | (data >> 19 & 1) << 1 | (data >> 22 & 1) << 0);
            buttons.L2.Color = (ButtonColors)((data >> 20 & 1) << 2 | (data >> 21 & 1) << 1 | (data >> 18 & 1) << 0);
            buttons.L3.Color = (ButtonColors)((data >> 17 & 1) << 2 | (data >> 16 & 1) << 1 | (data >> 15 & 1) << 0);
            buttons.R1.Color = (ButtonColors)((data >> 14 & 1) << 2 | (data >> 13 & 1) << 1 | (data >> 12 & 1) << 0);
            buttons.R2.Color = (ButtonColors)((data >> 11 & 1) << 2 | (data >> 10 & 1) << 1 | (data >> 9 & 1) << 0);
            buttons.R3.Color = (ButtonColors)((data >> 8 & 1) << 2 | (data >> 7 & 1) << 1 | (data >> 6 & 1) << 0);

            // 判断是否在游戏中
            var midButtons = buttons[0..3].Concat(buttons[5..8]);
            bool temp =
                midButtons.Count(b => (b as Button).Color == ButtonColors.Red) == 2 &&
                midButtons.Count(b => (b as Button).Color == ButtonColors.Blue) == 2 &&
                midButtons.Count(b => (b as Button).Color == ButtonColors.Green) == 2;
            if (temp != inRhythmGame) ForceGenRects();
            inRhythmGame = temp;

            if (inRhythmGame)
            {
                buttons.LMenu.Visible = buttons.RMenu.Visible = false;
            }
            else
            {
                buttons.LMenu.Visible = buttons.RMenu.Visible = true;
            }
            MainThread.InvokeOnMainThreadAsync(canvasView.InvalidateSurface);
        }


        enum TouchArea : byte
        {
            LButton1 = 0,
            LButton2 = 1,
            LButton3 = 2,
            LSide = 3,
            LMenu = 4,
            RButton1 = 5,
            RButton2 = 6,
            RButton3 = 7,
            RSide = 8,
            RMenu = 9,
            Lever = 10,
            Logo = 11,
            Others = 12
        }
    }
}

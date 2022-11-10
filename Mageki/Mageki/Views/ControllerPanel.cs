using Mageki.DependencyServices;
using Mageki.Drawables;
using Mageki.Resources;
using Mageki.TouchTracking;
using Mageki.Utils;

using SkiaSharp;
using SkiaSharp.Views.Forms;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Timers;

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
        Dictionary<long, (TouchArea button, SKPoint startPosition, SKPoint lastPosition)> touchPoints = new Dictionary<long, (TouchArea, SKPoint, SKPoint)>();

        bool inRhythmGame;
        private bool nfcScanning = false;
        private bool logoHoldUnhandled = false;

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
            // 安卓平台可以免提示使用NFC刷卡
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                try
                {
                    DependencyService.Get<INfcService>().StartReadFelicaId(ScanFelica, () => { });
                }
                catch (Exception ex) { }
            }
            InitIO();
            Settings.ValueChanged += Settings_ValueChanged;

        }

        private void Settings_ValueChanged(string name)
        {
            if (name == nameof(Settings.ButtonBottomMargin))
            {
                ForceGenRects();
            }
            if (name == nameof(Settings.Protocol) || name == nameof(Settings.Port))
            {
                InitIO();
            }
        }
        public void InitIO()
        {
            bool protocolChanged = io is null || io.GetType().ToString().ToLower() != $"{Settings.Protocol}IO".ToLower();
            bool portChanged =
                (io is UdpIO udpIO && udpIO.Port != Settings.Port) ||
                (io is TcpIO tcpIO && tcpIO.Port != Settings.Port);
            if (protocolChanged || portChanged)
            {
                if (io != null)
                {
                    io.OnConnected -= OnConnected;
                    io.OnDisconnected -= OnDisconnected;
                    io.OnLedChanged -= OnLedChanged;
                    io.Dispose();
                }
                try
                {
                    io = Settings.Protocol switch
                    {
                        Protocols.UDP => new UdpIO(),
                        Protocols.TCP => new TcpIO(),
                        _ => throw new NotImplementedException($"Unsupported protocols:{Settings.Protocol}"),
                    };
                    io.OnConnected += OnConnected;
                    io.OnDisconnected += OnDisconnected;
                    io.OnLedChanged += OnLedChanged;
                    OnDisconnected(this, EventArgs.Empty);
                    OnLedChanged(this, EventArgs.Empty);
                    io.Init();
                }
                catch (Exception ex)
                {
                    App.Logger.Error(ex);
                    //bool copy = App.Current.MainPage.DisplayAlert(AppResources.Error, ex.ToString(), AppResources.Copy, AppResources.Cancel).Result;
                    //if (copy)
                    //{
                    //    Clipboard.SetTextAsync(ex.ToString());
                    //}
                }
            }
        }

        private void OnLedChanged(object sender, EventArgs e)
        {
            SetLed(io.Colors);
        }

        private void OnConnected(object sender, EventArgs e)
        {
            logo.Color = SKColors.Black;
            canvasView.InvalidateSurface();
        }
        private void OnDisconnected(object sender, EventArgs e)
        {
            logo.Color = SKColors.LightGray;
            canvasView.InvalidateSurface();
        }
        public void ForceGenRects()
        {
            requireGenRects = true;
            MainThread.InvokeOnMainThreadAsync(canvasView.InvalidateSurface);
        }

        public async void ScanFelica(byte[] packet)
        {
            if (nfcScanning || logoHoldUnhandled) return;
            nfcScanning = true;
            io.SetAime(2, packet);
            await Task.Delay(3000);
            io.SetAime(0, new byte[10]);
            nfcScanning = false;
        }
        private void ScanFelicaInvalidated()
        {
            ScanFelica(GetSimulatedAimeId());
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
            buttons.Draw(canvas);
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
            TouchArea currentArea = GetArea(pixelLocation);
            //处理点击事件
            switch (args.Type)
            {
                case TouchActionType.Pressed:
                    {
                        if (touchPoints.ContainsKey(args.Id)) break;
                        TouchArea area = currentArea;
                        // 储存触点id与按下位置
                        touchPoints.Add(args.Id, (area, pixelLocation, pixelLocation));
                        // 按下按键
                        if ((byte)area < 10)
                        {
                            //按在这个按键上的触点数量
                            byte count = (byte)touchPoints.Count(p => p.Value.button == area);
                            // 设置io中按键的状态
                            io.SetGameButton((int)area, count);
                            buttons[(int)area].IsHold = true;
                        }
                        // 按下logo根据放开时间刷卡或显示菜单
                        else if (area == TouchArea.Logo)
                        {
                            if (!(nfcScanning || logoHoldUnhandled))
                            {
                                logoHoldUnhandled = true;
                                scanTime = DateTime.Now;
                                if (DeviceInfo.Platform != DevicePlatform.iOS || !DependencyService.Get<INfcService>().ReadingAvailable)
                                {
                                    io.SetAime(1, GetSimulatedAimeId());
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
                                                if (Settings.HapticFeedback) HapticFeedback.Perform(HapticFeedbackType.Click);
                                                logoHoldUnhandled = false;
                                                var nfcService = DependencyService.Get<INfcService>();
                                                nfcService.StartReadFelicaId(ScanFelica, ScanFelicaInvalidated);
                                                timer.Dispose();
                                            }
                                        });
                                    };
                                    timer.Start();
                                }
                                if (Settings.HapticFeedback) HapticFeedback.Perform(HapticFeedbackType.Click);
                            }
                        }
                        break;
                    }
                case TouchActionType.Moved:
                    {
                        if (!touchPoints.ContainsKey(args.Id)) break;
                        // 原本的按键
                        TouchArea area0 = touchPoints[args.Id].button;
                        // 新触发的按键
                        TouchArea area1 = GetArea(pixelLocation.X, canvasView.CanvasSize.Width);
                        if (Settings.SeparateButtonsAndLever && (int)area0 < 8 && (int)area0 % 5 < 3)
                        {
                            // 修改触点的触发区域
                            touchPoints[args.Id] = (area1, touchPoints[args.Id].startPosition, pixelLocation);
                            // 如果按键区不触发摇杆则允许搓
                            if (area0 != area1 && (int)area1 < 8 && (int)area1 % 5 < 3)
                            {
                                // 搓到的新键上目前有几个触点
                                byte count0 = (byte)touchPoints.Count(p => p.Value.button == area1);
                                // 旧键上有几个触点
                                byte count1 = (byte)touchPoints.Count(p => p.Value.button == area0);
                                io.SetGameButton((int)area1, count0);
                                io.SetGameButton((int)area0, count1);
                                buttons[(int)area1].IsHold = true;
                                if (count1 == 0)
                                {
                                    buttons[(int)area0].IsHold = false;
                                }
                            }
                        }
                        // 拖动触发摇杆，多个手指在同一帧移动时只会取最大值
                        else if (touchPoints.ContainsKey(args.Id))
                        {
                            lock (leverCache)
                            {
                                // 无法判断触点是哪一帧传来，所以在传来重复id时认为到了下一帧
                                bool idDuplicated = leverCache.Any(c => c.touchID == args.Id);
                                leverCache.Add((pixelLocation.X - touchPoints[args.Id].lastPosition.X, args.Id));
                                if (idDuplicated)
                                {
                                    // 计算全部移动的和，并将其限制在最大与最小值之间
                                    var min = leverCache.Select(v => v.value).Min();
                                    var max = leverCache.Select(v => v.value).Max();
                                    var sum = leverCache.Sum(v => v.value);
                                    if (min < 0 && sum < min)
                                    {
                                        sum = min;
                                    }
                                    if (max > 0 && sum > max)
                                    {
                                        sum = max;
                                    }
                                    MoveLever(sum);
                                    leverCache.Clear();
                                }
                            }
                            // 修改触点上次检测的位置
                            touchPoints[args.Id] = (area0, touchPoints[args.Id].startPosition, pixelLocation);
                        }
                        break;
                    }
                case TouchActionType.Released:
                case TouchActionType.Cancelled:
                    {

                        if (args.Type == TouchActionType.Cancelled && DeviceInfo.Platform == DevicePlatform.Android)
                        {
                            while (touchPoints.Count > 0)
                            {
                                ReleaseTouchPoint(touchPoints.First().Key, currentArea);
                            }
                        }
                        else
                        {
                            ReleaseTouchPoint(args.Id, currentArea);
                        }
                        // 释放按键

                        break;
                    }
            }
            //通知重绘画布
            canvasView.InvalidateSurface();
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
        /// <summary>
        /// 释放按键
        /// </summary>
        /// <param name="id"></param>
        /// <param name="currentArea">释放时的点击区域</param>
        void ReleaseTouchPoint(long id, TouchArea currentArea)
        {
            if (!touchPoints.ContainsKey(id)) return;
            TouchArea button = touchPoints[id].button;
            touchPoints.Remove(id);
            byte count = (byte)touchPoints.Count(p => p.Value.button == button);
            if ((int)button < 10)
            {
                if (count == 0)
                {
                    buttons[(int)button].IsHold = false;
                    if (inRhythmGame && (int)button < 10 && (int)button % 5 < 3) buttons[10..13][(int)button % 5].IsHold = false;
                }
                io.SetGameButton((int)button, count);
            }
            else if (button == TouchArea.Logo && count == 0 && logoHoldUnhandled)
            {
                logoHoldUnhandled = false;
                io.SetAime(0, new byte[10]);
                // 按下超过一秒不触发菜单
                if (DateTime.Now - scanTime < TimeSpan.FromSeconds(0.3) && currentArea == button)
                    LogoClickd.Invoke(this, EventArgs.Empty);
                if (Settings.HapticFeedback) HapticFeedback.Perform(HapticFeedbackType.Click);
            }
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

        private TouchArea GetArea(SKPoint pixelLocation)
        {
            float width = canvasView.CanvasSize.Width;
            float height = canvasView.CanvasSize.Height;
            return GetArea(pixelLocation, width, height);
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
            if (x < (buttons.L1.BorderRect.Right + buttons.L2.BorderRect.Left) / 2) area = TouchArea.LButton1;
            else if (x < (buttons.L2.BorderRect.Right + buttons.L3.BorderRect.Left) / 2) area = TouchArea.LButton2;
            else if (x < (buttons.L3.BorderRect.Right + buttons.R1.BorderRect.Left) / 2) area = TouchArea.LButton3;
            else if (x < (buttons.R1.BorderRect.Right + buttons.R2.BorderRect.Left) / 2) area = TouchArea.RButton1;
            else if (x < (buttons.R2.BorderRect.Right + buttons.R3.BorderRect.Left) / 2) area = TouchArea.RButton2;
            else area = TouchArea.RButton3;
            return area;
        }

        public void SetLed(ButtonColors[] colors)
        {

            buttons.L1.Color = colors[0];
            buttons.L2.Color = colors[1];
            buttons.L3.Color = colors[2];
            buttons.R1.Color = colors[3];
            buttons.R2.Color = colors[4];
            buttons.R3.Color = colors[5];

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

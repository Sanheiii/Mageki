using Mageki.Drawables;
using Mageki.TouchTracking;

using Plugin.FelicaReader;
using Plugin.FelicaReader.Abstractions;

using SkiaSharp;
using SkiaSharp.Extended.Svg;
using SkiaSharp.Views.Forms;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Essentials;
using Xamarin.Forms;

using Button = Mageki.Drawables.Button;
using SKSvg = SkiaSharp.Extended.Svg.SKSvg;
using Slider = Mageki.Drawables.Slider;

namespace Mageki
{
    class ControllerPanel : Grid
    {
        private Button[] buttons = Enumerable.Range(0, 10).Select((n) => new Button()).ToArray();
        private Button[] buttonsInRhythmGame = Enumerable.Range(0, 3).Select((n) => new Button()).ToArray();
        private IFelicaReader felicaReader;
        private IDisposable subscription;
        private IDrawable[] decorations = new IDrawable[]
        {
            new Circles(),
            new Circles(),
            new MenuBackground(){ Side=Side.Left},
            new MenuBackground(){ Side=Side.Right},
            new Logo()
        };
        private Slider slider = new Slider();
        #region 常量
        const float PanelMarginCoef = 0.5f;
        const float DownMarginCoef = 0.75f;
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

        UdpClient client;
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Broadcast, Settings.Port);
        bool inRhythmGame;
        byte dkRandomValue;
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

            dkRandomValue = (byte)(new Random().Next() % 255);
            client = new UdpClient();
            ScanServer();
            new Thread(PollThread).Start();
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                this.felicaReader = CrossFelicaReader.Current;
                this.subscription = this.felicaReader.WhenCardFound().Subscribe(new FelicaCardMediaObserver(this));
            }
        }
        private void ScanServer()
        {
            try
            {
                SendMessage(new byte[] { (byte)MessageType.DokiDoki, dkRandomValue });
            }
            catch (Exception ex) { }
        }
        public async void ScanFelica(byte[] felicaId)
        {
            if (nfcScanning || simulateScanning) return;
            nfcScanning = true;
            var id = new BigInteger(felicaId);
            var bcd = ToBcd(id);
            SendMessage(new byte[] { (byte)MessageType.Scan, 1 }.Concat(new byte[10 - bcd.Length]).Concat(bcd).ToArray());
            await Task.Delay(3000);
            SendMessage(new byte[] { (byte)MessageType.Scan, 0 }.Concat(new byte[10]).ToArray());
            nfcScanning = false;
        }
        SKColor backColor = SKColors.Black;
        int oldWidth = -1;
        int oldHeight = -1;
        bool requireGenRects = false;
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
            canvas.Clear(backColor);
            if (oldWidth != info.Width || oldHeight != info.Height || requireGenRects)
            {
                requireGenRects = false;
                GenRects(info.Width, info.Height);
            }
            foreach (IDrawable drawable in decorations)
            {
                drawable.Draw(canvas);
            }
            var drawingButtons = (inRhythmGame && Settings.UseSimplifiedLayout) ? buttonsInRhythmGame : buttons;
            for (int i = 0; i < drawingButtons.Length; i++)
            {
                drawingButtons[i].Draw(canvas);
            }
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
            float bottomMargin = buttonHeight * DownMarginCoef;
            float lrSpacing = buttonHeight * LRSpacingCoef;
            float bmSpacing = buttonHeight * BMSpacingCoef;
            float buttonSpacing = buttonHeight * ButtonSpacingCoef;

            float buttonBottom = height - bottomMargin;
            if (inRhythmGame && Settings.UseSimplifiedLayout)
            {
                // 打歌时以特殊布局绘制
                float specialButtonWidth = (width - panelMargin * 2 - buttonSpacing * 2) / 4;
                // Button 1
                buttonsInRhythmGame[0].Color = buttons[0].Color;
                buttonsInRhythmGame[0].Height = buttonHeight;
                buttonsInRhythmGame[0].Width = specialButtonWidth;
                buttonsInRhythmGame[0].Center = new SKPoint(panelMargin + buttonSpacing * 0 + specialButtonWidth * 0.5f, buttonBottom - buttonHeight / 2);
                // Button 2
                buttonsInRhythmGame[1].Color = buttons[1].Color;
                buttonsInRhythmGame[1].Height = buttonHeight;
                buttonsInRhythmGame[1].Width = specialButtonWidth * 2;
                buttonsInRhythmGame[1].Center = new SKPoint(panelMargin + buttonSpacing * 1 + specialButtonWidth * 2f, buttonBottom - buttonHeight / 2);
                // Button 3
                buttonsInRhythmGame[2].Color = buttons[2].Color;
                buttonsInRhythmGame[2].Height = buttonHeight;
                buttonsInRhythmGame[2].Width = specialButtonWidth;
                buttonsInRhythmGame[2].Center = new SKPoint(panelMargin + buttonSpacing * 2 + specialButtonWidth * 3.5f, buttonBottom - buttonHeight / 2);
            }
            // Left 1
            buttons[0].Width = buttons[0].Height = buttonWidth;
            buttons[0].Center = new SKPoint(panelMargin + buttonSpacing * 0 + buttonWidth * 0.5f, buttonBottom - buttonHeight / 2);
            // Left 2
            buttons[1].Width = buttons[1].Height = buttonWidth;
            buttons[1].Center = new SKPoint(panelMargin + buttonSpacing * 1 + buttonWidth * 1.5f, buttonBottom - buttonHeight / 2);
            // Left 3
            buttons[2].Width = buttons[2].Height = buttonWidth;
            buttons[2].Center = new SKPoint(panelMargin + buttonSpacing * 2 + buttonWidth * 2.5f, buttonBottom - buttonHeight / 2);
            // Left side 不作绘制
            buttons[3].Center = default;
            // Left menu
            buttons[4].Width = buttons[4].Height = menuSideLength;
            buttons[4].BorderColor = new SKColor(0xFF880000);
            buttons[4].Color = ButtonColors.Red;
            buttons[4].Center = new SKPoint(panelMargin + buttonWidth - buttonSpacing, buttonBottom - buttonHeight - bmSpacing - menuSideLength / 2);

            //-------------------
            // Right 1
            buttons[5].Width = buttons[5].Height = buttonWidth;
            buttons[5].Center = new SKPoint(panelMargin + buttonSpacing * 2 + buttonWidth * 3.5f + lrSpacing, buttonBottom - buttonHeight / 2);
            // Right 2
            buttons[6].Width = buttons[6].Height = buttonWidth;
            buttons[6].Center = new SKPoint(panelMargin + buttonSpacing * 3 + buttonWidth * 4.5f + lrSpacing, buttonBottom - buttonHeight / 2);
            // Right 3
            buttons[7].Width = buttons[7].Height = buttonWidth;
            buttons[7].Center = new SKPoint(panelMargin + buttonSpacing * 4 + buttonWidth * 5.5f + lrSpacing, buttonBottom - buttonHeight / 2);
            // Right side 不作绘制
            buttons[8].Center = default;
            // Right menu
            buttons[9].Width = buttons[9].Height = menuSideLength;
            buttons[9].BorderColor = new SKColor(0xFF888800);
            buttons[9].Color = ButtonColors.Yellow;
            buttons[9].Center = new SKPoint(width - (panelMargin + buttonWidth - buttonSpacing), buttonBottom - buttonHeight - bmSpacing - menuSideLength / 2);
            if (!inRhythmGame)
            {
                // menu键的背景，打歌时无需绘制
                if (decorations[2] is MenuBackground background0)
                {
                    background0.Center = buttons[4].Center;
                    background0.Height = buttons[4].Height * 1.6f;
                    background0.Width = buttons[4].Width * 1.6f;
                }
                if (decorations[3] is MenuBackground background1)
                {
                    background1.Center = buttons[9].Center;
                    background1.Height = buttons[9].Height * 1.6f;
                    background1.Width = buttons[9].Width * 1.6f;
                }
            }
            // 绘制装饰用的环
            if (decorations[0] is Circles circles0)
            {
                circles0.Center = buttons[1].Center;
                circles0.Radius = buttonWidth + buttonSpacing;
                circles0.DrawLeftArc = true;
                circles0.DrawRightArc = false;
            }
            if (decorations[1] is Circles circles1)
            {
                circles1.Center = buttons[6].Center;
                circles1.Radius = buttonWidth + buttonSpacing;
                circles1.DrawLeftArc = false;
                circles1.DrawRightArc = true;
            }
            // logo
            if (decorations[4] is Drawables.Logo logo)
            {
                logo.MaxHeight = menuSideLength;
                logo.MaxWidth = width;
                logo.Center = new SKPoint(width / 2, buttonBottom - buttonHeight - bmSpacing - logo.MaxHeight * 1.5f);
            }
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
                        TouchArea area = currentArea;
                        touchPoints.Add(args.Id, (area, pixelLocation));
                        // 按下按键
                        if ((byte)area < 10)
                        {
                            SendMessage(new byte[] { (byte)MessageType.ButtonStatus, (byte)area, 1 });
                            buttons[(int)area].IsHold = true;
                            if (inRhythmGame && (int)area % 5 < 3) buttonsInRhythmGame[(int)area % 5].IsHold = true;
                        }
                        // 按下logo根据放开时间刷卡或显示菜单
                        else if (area == TouchArea.Logo)
                        {
                            if (!(nfcScanning || simulateScanning))
                            {
                                byte[] aimeId = new byte[10];
                                simulateScanning = true;
                                scanTime = DateTime.Now;
                                if (BigInteger.TryParse(Settings.AimeId, out BigInteger integer))
                                {
                                    var bcd = ToBcd(integer);
                                    var bytes = bcd.Concat(new byte[10 - bcd.Length]);
                                    aimeId = bytes.ToArray();
                                }
                                SendMessage(new byte[] { (byte)MessageType.Scan, 1 }.Concat(aimeId).ToArray());
                            }
                        }
                        break;
                    }
                case TouchActionType.Moved:
                    {
                        // 在任意位置拖动触发摇杆，多个手指在同一帧移动时只会取最大值
                        if (touchPoints.ContainsKey(args.Id))
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
                            TouchArea area = touchPoints[args.Id].touchArea;
                            touchPoints.Remove(args.Id);
                            touchPoints.Add(args.Id, (area, pixelLocation));
                        }
                        break;
                    }
                case TouchActionType.Released:
                case TouchActionType.Cancelled:
                    {
                        // 释放按键
                        TouchArea area = touchPoints[args.Id].touchArea;
                        if ((int)area < 10 && touchPoints.Count(p => p.Value.touchArea == area) < 2)
                        {
                            SendMessage(new byte[] { (byte)MessageType.ButtonStatus, (byte)area, 0 });
                            buttons[(int)area].IsHold = false;
                            if (inRhythmGame && (int)area % 5 < 3) buttonsInRhythmGame[(int)area % 5].IsHold = false;
                        }
                        else if (area == TouchArea.Logo && touchPoints.Count(p => p.Value.touchArea == area) < 2)
                        {
                            simulateScanning = false;
                            SendMessage(new byte[] { (byte)MessageType.Scan, 0 }.Concat(new byte[10]).ToArray());
                            // 按下超过一秒不触发菜单
                            if (DateTime.Now - scanTime < TimeSpan.FromSeconds(1) && currentArea == area)
                                LogoClickd.Invoke(this, EventArgs.Empty);
                        }
                        if (touchPoints.ContainsKey(args.Id))
                        {
                            touchPoints.Remove(args.Id);
                        }
                        break;
                    }
            }
            //通知重绘画布
            canvasView.InvalidateSurface();
        }

        private void MoveLever(float x)
        {
            var pixelWidth = Width * Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Density;
            var oldValue = slider.Value;
            slider.Value += (short)(x * (pixelWidth / 30) * Settings.LeverSensitivity);
            // check会导致iOS端崩溃，使用土方法检查溢出
            if (x < 0 && oldValue < slider.Value) slider.Value = short.MinValue;
            else if (x > 0 && oldValue > slider.Value) slider.Value = short.MaxValue;
            SendMessage(
                new byte[] { (byte)MessageType.MoveLever }.
                Concat(BitConverter.GetBytes(slider.Value)).ToArray());
        }

        private TouchArea GetArea(SKPoint pixelLocation, float width, float height)
        {
            TouchArea area = TouchArea.Others;
            // 大概点到中间六键的范围就会触发
            if (pixelLocation.Y > buttons[0].BorderRect.Top - buttons[0].BorderRect.Left)
            {
                if (inRhythmGame && Settings.UseSimplifiedLayout)
                {
                    if (pixelLocation.X < width / 4 * 1) area = buttons[5].IsHold ? TouchArea.LButton1 : TouchArea.RButton1;
                    else if (pixelLocation.X < width / 4 * 3) area = buttons[6].IsHold ? TouchArea.LButton2 : TouchArea.RButton2;
                    else area = buttons[7].IsHold ? TouchArea.LButton3 : TouchArea.RButton3;
                }
                else
                {
                    if (pixelLocation.X < (buttons[0].BorderRect.Right + buttons[1].BorderRect.Left) / 2) area = TouchArea.LButton1;
                    else if (pixelLocation.X < (buttons[1].BorderRect.Right + buttons[2].BorderRect.Left) / 2) area = TouchArea.LButton2;
                    else if (pixelLocation.X < (buttons[2].BorderRect.Right + buttons[5].BorderRect.Left) / 2) area = TouchArea.LButton3;
                    else if (pixelLocation.X < (buttons[5].BorderRect.Right + buttons[6].BorderRect.Left) / 2) area = TouchArea.RButton1;
                    else if (pixelLocation.X < (buttons[6].BorderRect.Right + buttons[7].BorderRect.Left) / 2) area = TouchArea.RButton2;
                    else area = TouchArea.RButton3;
                }
            }
            else if (!inRhythmGame && buttons[4].BorderRect.Contains(pixelLocation)) area = TouchArea.LMenu;
            else if (!inRhythmGame && buttons[9].BorderRect.Contains(pixelLocation)) area = TouchArea.RMenu;
            else if (!inRhythmGame && decorations[4] is Drawables.Logo logo && logo.Rect.Contains(pixelLocation)) area = TouchArea.Logo;
            else if (pixelLocation.X < width / 2) area = TouchArea.LSide;
            else area = TouchArea.RSide;
            Debug.WriteLine(area);
            return area;
        }

        private void SendMessage(byte[] data)
        {
            // 没有连接到就再试一次
            if (remoteEP.Address.Address == IPAddress.Broadcast.Address && data[0] != (byte)MessageType.DokiDoki)
            {
                ScanServer();
                return;
            }
            client.Send(data, data.Length, remoteEP);
        }

        IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);
        /// <summary>
        /// 用于接收数据并设置LED
        /// </summary>
        private void PollThread()
        {
            while (true)
            {
                byte[] buffer = client.Receive(ref ep);
                ParseBuffer(buffer);
            }
        }

        private void ParseBuffer(byte[] buffer)
        {
            if ((buffer?.Length ?? 0) == 0) return;
            if (buffer[0] == (byte)MessageType.SetLed && buffer.Length == 5)
            {
                uint ledData = BitConverter.ToUInt32(buffer, 1);
                SetLed(ledData);
            }
            else if (buffer[0] == (byte)MessageType.SetLever && buffer.Length == 3)
            {
                short lever = BitConverter.ToInt16(buffer, 1);
                slider.Value = lever;
            }
            else if (buffer[0] == (byte)MessageType.DokiDoki && buffer.Length == 2 && buffer[1] == dkRandomValue)
            {
                remoteEP.Address = new IPAddress(ep.Address.GetAddressBytes());
                backColor = SKColors.White;
                if (decorations[4] is Logo logo)
                {
                    logo.Color = SKColors.Black;
                }
                RequestValues();
            }
            //// 用于直接打开测试显示按键
            //Mu3IO._test.UpdateData();
        }

        private void RequestValues()
        {
            SendMessage(new byte[] { (byte)MessageType.RequestValues });
        }


        public void SetLed(uint data)
        {

            buttons[0].Color = (ButtonColors)((data >> 23 & 1) << 2 | (data >> 19 & 1) << 1 | (data >> 22 & 1) << 0);
            buttons[1].Color = (ButtonColors)((data >> 20 & 1) << 2 | (data >> 21 & 1) << 1 | (data >> 18 & 1) << 0);
            buttons[2].Color = (ButtonColors)((data >> 17 & 1) << 2 | (data >> 16 & 1) << 1 | (data >> 15 & 1) << 0);
            buttons[5].Color = (ButtonColors)((data >> 14 & 1) << 2 | (data >> 13 & 1) << 1 | (data >> 12 & 1) << 0);
            buttons[6].Color = (ButtonColors)((data >> 11 & 1) << 2 | (data >> 10 & 1) << 1 | (data >> 9 & 1) << 0);
            buttons[7].Color = (ButtonColors)((data >> 8 & 1) << 2 | (data >> 7 & 1) << 1 | (data >> 6 & 1) << 0);

            // 判断是否在游戏中
            var midButtons = buttons[0..3].Concat(buttons[5..8]);
            bool temp =
                midButtons.Count(b => b.Color == ButtonColors.Red) == 2 &&
                midButtons.Count(b => b.Color == ButtonColors.Blue) == 2 &&
                midButtons.Count(b => b.Color == ButtonColors.Green) == 2;
            requireGenRects = temp != inRhythmGame;
            inRhythmGame = temp;

            if (inRhythmGame)
            {
                buttons[4].Visible = buttons[9].Visible = decorations[2].Visible = decorations[3].Visible = false;
            }
            else
            {
                buttons[4].Visible = buttons[9].Visible = decorations[2].Visible = decorations[3].Visible = true;
            }
            Xamarin.Essentials.MainThread.InvokeOnMainThreadAsync(canvasView.InvalidateSurface);
        }

        public static byte[] ToBcd(BigInteger value)
        {
            var length = value.ToString().Length / 2 + value.ToString().Length % 2;
            byte[] ret = new byte[length];
            for (int i = length - 1; i >= 0; i--)
            {
                ret[i] = (byte)(value % 10);
                value /= 10;
                ret[i] |= (byte)((value % 10) << 4);
                value /= 10;
            }
            return ret;
        }


        enum MessageType : byte
        {
            // 控制器向IO发送的
            ButtonStatus = 1,
            MoveLever = 2,
            Scan = 3,
            Test = 4,
            RequestValues = 5,
            // IO向控制器发送的
            SetLed = 6,
            SetLever = 7,
            // 寻找在线设备
            DokiDoki = 255
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

using Mageki.Drawables;
using Mageki.TouchTracking;

using SkiaSharp;
using SkiaSharp.Extended.Svg;
using SkiaSharp.Views.Forms;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

using Xamarin.Forms;

using Button = Mageki.Drawables.Button;
using SKSvg = SkiaSharp.Extended.Svg.SKSvg;
using Slider = Mageki.Drawables.Slider;

namespace Mageki
{
    class ControllerPanel : Grid
    {
        private Button[] buttons = Enumerable.Range(0, 10).Select((n) => new Button()).ToArray();
        private IDrawable[] decorations = new IDrawable[]
        {
            new Circles(),
            new Circles(),
            new MenuBackground(){ Side=Side.Left},
            new MenuBackground(){ Side=Side.Right},
            Drawables.Svg.FromResource("Mageki.Resources.オンゲキ.svg")
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
        TcpClient client;
        NetworkStream networkStream;


        public bool IsConnected => client?.Connected ?? false;
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
            Connect();
            new Thread(PollThread).Start();
        }
        public void Connect()
        {
            client = new TcpClient("192.168.50.104", 4354);
            networkStream = client.GetStream();
        }
        private void Disconnect()
        {
            if (IsConnected)
            {
                networkStream?.Dispose();
                client?.Dispose();
                client = null;
                networkStream = null;
            }
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
            if (oldWidth != info.Width || oldHeight != info.Height)
            {
                GenRects(info.Width, info.Height);
            }
            foreach (IDrawable drawable in decorations)
            {
                drawable.Draw(canvas);
            }
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].Draw(canvas);
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
            // Left menu 不作绘制
            buttons[4].Width = buttons[4].Height = menuSideLength;
            buttons[4].Color = ButtonColors.Red;
            buttons[4].Center = new SKPoint(panelMargin + buttonWidth - buttonSpacing, buttonBottom - buttonHeight - bmSpacing - menuSideLength / 2);
            ;
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
            // Right menu 不作绘制
            buttons[9].Width = buttons[9].Height = menuSideLength;
            buttons[9].Color = ButtonColors.Yellow;
            buttons[9].Center = new SKPoint(width - (panelMargin + buttonWidth - buttonSpacing), buttonBottom - buttonHeight - bmSpacing - menuSideLength / 2);
            // Lever
            // Lever 不作绘制
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
            // menu键的背景
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
            // logo
            if (decorations[4] is Drawables.Svg svg)
            {
                svg.MaxHeight = menuSideLength;
                svg.MaxWidth = width;
                svg.Center = new SKPoint(width / 2, buttonBottom - buttonHeight - bmSpacing - svg.MaxHeight * 1.5f);
            }
        }
        private bool InRhythmGame()
        {
            var midButtons = buttons[0..3].Concat(buttons[5..8]);
            return
                midButtons.Count(b => b.Color == ButtonColors.Red) == 2 &&
                midButtons.Count(b => b.Color == ButtonColors.Blue) == 2 &&
                midButtons.Count(b => b.Color == ButtonColors.Green) == 2;
        }
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
                        if ((int)area < 10)
                        {
                            buttons[(int)area].IsHold = true;
                        }
                        break;
                    }
                case TouchActionType.Moved:
                    {
                        if (touchPoints.ContainsKey(args.Id))
                        {
                            TouchArea area = touchPoints[args.Id].touchArea;
                            // 在任意位置拖动触发摇杆
                            MoveLever(pixelLocation.X - touchPoints[args.Id].position.X);
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
                            buttons[(int)area].IsHold = false;
                        }
                        if (currentArea == area && area == TouchArea.Logo)
                        {
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
            SendMessage();
        }

        private void MoveLever(float x)
        {
            var oldValue = slider.Value;
            slider.Value += (short)(x * 100);
            // check会导致iOS端崩溃，使用土方法检查溢出
            if (x < 0 && oldValue < slider.Value) slider.Value = short.MinValue;
            else if (x > 0 && oldValue > slider.Value) slider.Value = short.MaxValue;
        }

        private TouchArea GetArea(SKPoint pixelLocation, float width, float height)
        {
            bool inRgythmGame = InRhythmGame();
            TouchArea area = TouchArea.Others;
            // 大概点到中间六键的范围就会触发
            if (pixelLocation.Y > buttons[0].BorderRect.Top - buttons[0].BorderRect.Left / 2)
            {
                if (pixelLocation.X < (buttons[0].BorderRect.Right + buttons[1].BorderRect.Left) / 2) area = TouchArea.LButton1;
                else if (pixelLocation.X < (buttons[1].BorderRect.Right + buttons[2].BorderRect.Left) / 2) area = TouchArea.LButton2;
                else if (pixelLocation.X < (buttons[2].BorderRect.Right + buttons[5].BorderRect.Left) / 2) area = TouchArea.LButton3;
                else if (pixelLocation.X < (buttons[5].BorderRect.Right + buttons[6].BorderRect.Left) / 2) area = TouchArea.RButton1;
                else if (pixelLocation.X < (buttons[6].BorderRect.Right + buttons[7].BorderRect.Left) / 2) area = TouchArea.RButton2;
                else area = TouchArea.RButton3;
            }
            //else if (pixelLocation.X <= slider.BackRect.Right && pixelLocation.X >= slider.BackRect.Left) area = TouchArea.Lever;
            else if (!inRgythmGame && buttons[4].BorderRect.Contains(pixelLocation)) area = TouchArea.LMenu;
            else if (!inRgythmGame && buttons[9].BorderRect.Contains(pixelLocation)) area = TouchArea.RMenu;
            else if (!inRgythmGame && decorations[4] is Drawables.Svg logo && logo.Rect.Contains(pixelLocation)) area = TouchArea.Logo;
            else if (pixelLocation.X < width / 2) area = TouchArea.LSide;
            else area = TouchArea.RSide;
            Debug.WriteLine(area);
            return area;
        }

        object locker = new object();
        private void SendMessage()
        {
            lock (locker)
            {
                byte[] buffer = new byte[10];
                //检测被按下的按键将对应的值设为1
                foreach (var point in touchPoints)
                {
                    if ((int)point.Value.touchArea < 10)
                        buffer[(int)point.Value.touchArea] = 1;
                }
                bool isScan = touchPoints.Any(v => v.Value.touchArea == TouchArea.Others);
                byte[] aimeId = new byte[10];
                buffer = buffer//buffer已有对应十个按键的值
                    .Concat(BitConverter.GetBytes(slider.Value))//摇杆的值
                    .Concat(BitConverter.GetBytes(false))//是否在刷卡
                    .Concat(aimeId)//aime id
                    .Concat(BitConverter.GetBytes(isScan))//是否按下test键
                    .ToArray();
                networkStream.Write(buffer, 0, buffer.Length);
            }
        }

        private byte[] _inBuffer = new byte[4];
        /// <summary>
        /// 用于接收数据并设置LED
        /// </summary>
        private void PollThread()
        {
            while (true)
            {
                if (!IsConnected)
                {
                    //Connect();
                    continue;
                }
                IAsyncResult result = networkStream.BeginRead(_inBuffer, 0, 4, new AsyncCallback((res) => { }), null);
                int len = networkStream.EndRead(result);
                if (len <= 0)
                {
                    Disconnect();
                    continue;
                }
                uint ledData = BitConverter.ToUInt32(_inBuffer, 0);
                SetLed(ledData);
                //// 用于直接打开测试显示按键
                //Mu3IO._test.UpdateData();
            }
        }
        public void SetLed(uint data)
        {

            buttons[0].Color = (ButtonColors)((data >> 23 & 1) << 2 | (data >> 19 & 1) << 1 | (data >> 22 & 1) << 0);
            buttons[1].Color = (ButtonColors)((data >> 20 & 1) << 2 | (data >> 21 & 1) << 1 | (data >> 18 & 1) << 0);
            buttons[2].Color = (ButtonColors)((data >> 17 & 1) << 2 | (data >> 16 & 1) << 1 | (data >> 15 & 1) << 0);
            buttons[5].Color = (ButtonColors)((data >> 14 & 1) << 2 | (data >> 13 & 1) << 1 | (data >> 12 & 1) << 0);
            buttons[6].Color = (ButtonColors)((data >> 11 & 1) << 2 | (data >> 10 & 1) << 1 | (data >> 9 & 1) << 0);
            buttons[7].Color = (ButtonColors)((data >> 8 & 1) << 2 | (data >> 7 & 1) << 1 | (data >> 6 & 1) << 0);
            if (InRhythmGame())
            {
                buttons[4].Visible = buttons[9].Visible = decorations[2].Visible = decorations[3].Visible = false;
            }
            else
            {
                buttons[4].Visible = buttons[9].Visible = decorations[2].Visible = decorations[3].Visible = true;
            }
            Xamarin.Essentials.MainThread.InvokeOnMainThreadAsync(canvasView.InvalidateSurface);
        }



        enum TouchArea
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

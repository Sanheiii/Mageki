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

using SquareButton = Mageki.Drawables.SquareButton;
using DeviceInfo = Xamarin.Essentials.DeviceInfo;
using Keyboard = Mageki.Drawables.Keyboard;

namespace Mageki
{
    public class ControllerPanel : Grid
    {
        private IO io;
        private bool requireUpdate = false;
        private Keyboard keyboard = new Keyboard();
        private SideButton lSide = new SideButton() { Side = Side.Left, Color = SKColors.Pink };
        private SideButton rSide = new SideButton() { Side = Side.Right, Color = SKColors.Purple };
        private SquareButton lMenu = new SquareButton() { Color = ButtonColors.Red, BorderColor = new SKColor(0xFF880000) };
        private SquareButton rMenu = new SquareButton() { Color = ButtonColors.Yellow, BorderColor = new SKColor(0xFF888800) };
        private Lever lever = new Lever();
        private MenuFrame lMenuFrame;
        private MenuFrame rMenuFrame;
        private Circles circles;
        private SettingButton settingButton;
        private IList<TouchableObject> touchableObject;
        int oldWidth = -1;
        int oldHeight = -1;

        #region 常量
        const float PanelPaddingRatio = 0.5f;
        const float LRSpacingCoef = 0.5f;
        const float KeyboardMarginTopCoef = 0.25f;
        const float ButtonSpacingCoef = 0.25f;
        const float MenuSizeCoef = 0.5f;
        const float SettingSizeCoef = 0.4f;
        const float MenuPaddingCoef = 1.125f;
        #endregion

        /// <summary>
        /// 绘图面板
        /// </summary>
        SKCanvasView canvasView = new SKCanvasView();
        /// <summary>
        /// 捕获点击
        /// </summary>
        TouchEffect touchEffect = new TouchEffect { Capture = true };

        bool inRhythmGame;
        private bool nfcScanning = false;


        public ControllerPanel()
        {
            settingButton = new SettingButton(this);
            circles = new Circles(keyboard);
            touchableObject = new List<TouchableObject>() { settingButton, keyboard, lMenu, rMenu, lever, lSide, rSide };

            lMenuFrame = new MenuFrame(lMenu, Side.Left);
            rMenuFrame = new MenuFrame(rMenu, Side.Right);
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
                    DependencyService.Get<INfcService>().StartReadAime(ScanFelica, ScanMifare, () => { });
                }
                catch (Exception ex)
                {
                    App.Logger.Error(ex);
                }
            }

            InitIO();
            Settings.ValueChanged += Settings_ValueChanged;
        }

        private void Settings_ValueChanged(string name)
        {
            if (name == nameof(Settings.ButtonBottomMargin))
            {
                ForceUpdate();
            }
            if (name == nameof(Settings.HideButtons))
            {
                InvalidateSurface();
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
                }
            }
        }

        private void OnLedChanged(object sender, EventArgs e)
        {
            SetLed(io.Colors);
        }

        private void OnConnected(object sender, EventArgs e)
        {
            //logo.Color = SKColors.Black;
            InvalidateSurface();
        }
        private void OnDisconnected(object sender, EventArgs e)
        {
            //logo.Color = SKColors.LightGray;
            InvalidateSurface();
        }
        public void InvalidateSurface()
        {
            canvasView.InvalidateSurface();
        }
        public void ForceUpdate()
        {
            requireUpdate = true;
            MainThread.InvokeOnMainThreadAsync(canvasView.InvalidateSurface);
        }

        public async void ScanFelica(byte[] packet)
        {
            if (nfcScanning) return;

            string idmString = "0x" + BitConverter.ToUInt64(packet[0..8].Reverse().ToArray(), 0).ToString("X16");
            string pmMString = "0x" + BitConverter.ToUInt64(packet[8..16].Reverse().ToArray(), 0).ToString("X16");
            string systemCodeString = BitConverter.ToUInt16(packet[16..18].Reverse().ToArray(), 0).ToString("X4");
            App.Logger.Debug($"FeliCa card is present\nIDm: {idmString}\nPMm: {pmMString}\nSystemCode: {systemCodeString}");

            nfcScanning = true;
            io.SetAime(2, packet);
            await Task.Delay(3000);
            io.SetAime(0, new byte[0]);
            nfcScanning = false;
        }
        public async void ScanMifare(byte[] packet)
        {
            if (nfcScanning) return;

            App.Logger.Debug($"Mifare card is present\nAccessCode: {1}");

            nfcScanning = true;
            io.SetAime(1, packet);
            await Task.Delay(3000);
            io.SetAime(0, new byte[0]);
            nfcScanning = false;
        }

        private void ScanFelicaInvalidated()
        {
            ScanMifare(GetSimulatedAimeId());
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

#if DEBUG
        Stopwatch sw = new Stopwatch();
#endif
        /// <summary>
        /// 绘图
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanvasView_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
#if DEBUG
            sw.Restart();
#endif
            //获取绘图面板的信息
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;
            //清空画布
            canvas.Clear(SKColors.White);
            if (oldWidth != info.Width || oldHeight != info.Height || requireUpdate)
            {
                requireUpdate = false;
                Update(info.Width, info.Height);
            }

            if (!Settings.HideButtons)
            {
                circles.Draw(canvas);
            }
            lMenuFrame.Draw(canvas);
            rMenuFrame.Draw(canvas);

            lSide.Draw(canvas);
            rSide.Draw(canvas);
            lever.Draw(canvas);

            lMenu.Draw(canvas);
            rMenu.Draw(canvas);

            if (!Settings.HideButtons)
            {
                keyboard.Draw(canvas);
            }

            settingButton.Draw(canvas);

            oldWidth = info.Width;
            oldHeight = info.Height;
#if DEBUG
            Debug.WriteLine($"[Draw Frame]: {sw.Elapsed.TotalMilliseconds}ms");
            sw.Stop();
#endif
        }
        /// <summary>
        /// 计算各个元素的位置和大小
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        private void Update(int width, int height)
        {
            var nSide = BitConverter.GetBytes(keyboard.ShowLeft)[0] + BitConverter.GetBytes(keyboard.ShowRight)[0];
            float baseCoef = 1 / (PanelPaddingRatio * 2 + LRSpacingCoef * (nSide / 2) + ButtonSpacingCoef * nSide * 2 + nSide * 3);
            // 以一个按钮的边长作为基数计算其他部分尺寸
            float baseLength = (width * baseCoef);

            float menuSideLength = baseLength * MenuSizeCoef;
            float menuPadding = baseLength * MenuPaddingCoef;
            float keyboardMarginTop = baseLength * KeyboardMarginTopCoef;

            float bottomMargin = (height - baseLength) * Settings.ButtonBottomMargin;

            keyboard.Padding = new SKPoint(baseLength * PanelPaddingRatio, 0);
            keyboard.Position = new SKPoint(0, height - bottomMargin - baseLength);
            keyboard.Spacing = baseLength * LRSpacingCoef;
            keyboard.Size = new SKSize(width, height - keyboard.Position.Y);
            keyboard.Left.Spacing = keyboard.Right.Spacing = baseLength * ButtonSpacingCoef;

            lMenu.Size = rMenu.Size = new SKSize(menuSideLength, menuSideLength);
            lMenu.Position = new SKPoint(menuPadding, keyboard.BoundingBox.Top - keyboardMarginTop - menuSideLength * 2);
            rMenu.Position = new SKPoint(width - menuPadding - menuSideLength, lMenu.Position.Y);

            float sideButtonWidth = baseLength * (PanelPaddingRatio + 1.5f) + baseLength * ButtonSpacingCoef;
            lSide.Size = rSide.Size = new SKSize(sideButtonWidth, height - keyboard.BoundingBox.Height - keyboardMarginTop);
            lSide.Position = new SKPoint(0, 0);
            rSide.Position = new SKPoint(width - sideButtonWidth, 0);
            lSide.Padding = rSide.Padding = new SKPoint(0, keyboardMarginTop);

            lever.Size = new SKSize(width - lSide.Size.Width - rSide.Size.Width, lSide.Size.Height);
            lever.Position = new SKPoint(lSide.Size.Width, 0);
            lever.Padding = new SKPoint(0, keyboard.BoundingBox.Top - lMenu.BoundingBox.Bottom);

            float settingSideLength = baseLength * SettingSizeCoef;
            settingButton.Size = new SKSize(settingSideLength, settingSideLength);
            settingButton.Position = new SKPoint(settingSideLength * 0.5f, settingSideLength * 0.5f);
            //settingButton.Padding = new SKPoint(settingSideLength * 0.1f, settingSideLength * 0.1f);
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
            switch (args.Type)
            {
                case TouchActionType.Pressed:
                    foreach (TouchableObject obj in touchableObject)
                    {
                        if (obj.Visible && obj.HitTest(pixelLocation) && obj.HandleTouchPressed(args.Id, pixelLocation))
                        {
                            break;
                        }
                    }
                    break;
                case TouchActionType.Moved:
                    foreach (TouchableObject obj in touchableObject)
                    {
                        if (obj.HandleTouchMoved(args.Id, pixelLocation))
                        {
                            break;
                        }
                    }
                    break;
                case TouchActionType.Released:
                    foreach (TouchableObject obj in touchableObject)
                    {
                        if (obj.HandleTouchReleased(args.Id))
                        {
                            break;
                        }
                    }
                    break;
                case TouchActionType.Cancelled:
                    foreach (TouchableObject obj in touchableObject)
                    {
                        obj.HandleTouchCancelled(args.Id);
                    }
                    break;
            }
            //更新IO状态
            UpdateIO();
            //通知重绘画布
            InvalidateSurface();
            return;
        }

        private void UpdateIO()
        {
            ButtonBase[] buttons = new ButtonBase[]
            {
                    keyboard[0],
                    keyboard[1],
                    keyboard[2],
                    lSide,
                    lMenu,
                    keyboard[3],
                    keyboard[4],
                    keyboard[5],
                    rSide,
                    rMenu
            };
            // buttons
            for (int i = 0; i < buttons.Length; i++)
            {
                if (io.Data.GameButtons[i] != buttons[i].TouchCount)
                {
                    io.SetGameButton(i, buttons[i].TouchCount);
                }
            }
            // lever
            MoveLever(lever.Value);
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

        private void MoveLever(float x)
        {
            short newValue = (short)(short.MaxValue * x);
            short oldValue = io.Data.Lever;

            var threshold = short.MaxValue / (Settings.LeverLinearity / 2f);

            // 仅在经过分界的时候发包
            if ((int)(newValue / threshold) != (int)(oldValue / threshold))
            {
                io.SetLever(newValue);
                return;
            }
        }

        public void SetLed(ButtonColors[] colors)
        {
            for (int i = 0; i < colors.Length; i++)
            {
                keyboard[i].Color = colors[i];
            }

            bool temp =
                keyboard.Left[0].Color == keyboard.Right[0].Color &&
                keyboard.Left[1].Color == keyboard.Right[1].Color &&
                keyboard.Left[2].Color == keyboard.Right[2].Color &&
                keyboard.Left[0].Color == ButtonColors.Red &&
                keyboard.Left[1].Color == ButtonColors.Blue &&
                keyboard.Left[2].Color == ButtonColors.Green;

            if (temp != inRhythmGame) ForceUpdate();
            inRhythmGame = temp;
            lMenu.Visible = rMenu.Visible = settingButton.Visible = !inRhythmGame;
            MainThread.InvokeOnMainThreadAsync(canvasView.InvalidateSurface);
        }
    }
}

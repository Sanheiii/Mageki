using Mageki.Drawables;
using Mageki.Resources;
using Mageki.Services;
using Mageki.Utils;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Timers;

using Microsoft.Maui.Controls;
using SquareButton = Mageki.Drawables.SquareButton;
using Keyboard = Mageki.Drawables.Keyboard;

namespace Mageki
{
    public class ControllerPanel : Grid
    {
        private bool requireUpdate = false;
        private Keyboard keyboard = new Keyboard();
        private SideButton lSide = new SideButton() { Side = Side.Left, Color = SKColors.Pink };
        private SideButton rSide = new SideButton() { Side = Side.Right, Color = SKColors.Purple };

        private SquareButton lMenu = new SquareButton()
        { Color = ButtonColors.Red, BorderColor = new SKColor(0xFF880000) };

        private SquareButton rMenu = new SquareButton()
        { Color = ButtonColors.Yellow, BorderColor = new SKColor(0xFF888800) };

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
        const float KeyboardMarginTopCoef = 0.5f;
        const float ButtonSpacingCoef = 0.25f;
        const float MenuSizeCoef = 0.5f;
        const float SettingSizeCoef = 0.4f;
        const float MenuPaddingCoef = 1.125f;
        const float LeverWidth = 0.5f;

        #endregion

        /// <summary>
        /// 绘图面板
        /// </summary>
        SKCanvasView canvasView = new SKCanvasView
        {
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };
        bool inRhythmGame;
        private readonly HashSet<long> activeTouches = new();


        public ControllerPanel(INfcService nfcService, ISettingsPopupService settingsPopupService)
        {
            settingButton = new SettingButton(nfcService, settingsPopupService);
            circles = new Circles(keyboard);
            touchableObject = new List<TouchableObject>()
                { settingButton, keyboard, lMenu, rMenu, lever, lSide, rSide };

            lMenuFrame = new MenuFrame(lMenu, Side.Left);
            rMenuFrame = new MenuFrame(rMenu, Side.Right);
            //添加绘图面板
            Children.Add(canvasView);
            //注册绘图方法
            canvasView.PaintSurface += CanvasView_PaintSurface;
            //捕获点击
            canvasView.EnableTouchEvents = true;
            canvasView.Touch += CanvasView_Touch;

            Settings.ValueChanged += Settings_ValueChanged;
            StaticIO.OnLedChanged += (_, _) => MainThread.BeginInvokeOnMainThread(() => SetLed(StaticIO.Colors));
            StaticIO.OnLeverChanged += (_, _) => MainThread.BeginInvokeOnMainThread(() =>
            {
                lever.Value = System.Math.Clamp(StaticIO.Data.Lever / (float)short.MaxValue, -1, 1);
                lever.valueOverflow = 0;
                canvasView.InvalidateSurface();
            });
            settingsPopupService.Showing += ReleaseInputs;
            SetLed(StaticIO.Colors);
        }


        private void Settings_ValueChanged(string name)
        {
            if (name == nameof(Settings.ButtonBottomMargin) ||
                name == nameof(Settings.HideGameButtons) ||
                name == nameof(Settings.EnableCompositeMode) ||
                name == nameof(Settings.HideWallActionDevices) ||
                name == nameof(Settings.HideMenuButtons) ||
                name == nameof(Settings.AntiMisTouch) ||
                name == nameof(Settings.LeverMoveMode))
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ReleaseInputs();
                    keyboard.AntiMisTouch = Settings.AntiMisTouch && inRhythmGame;
                    ForceUpdate();
                });
            }
        }

        public void ForceUpdate()
        {
            requireUpdate = true;
            MainThread.InvokeOnMainThreadAsync(canvasView.InvalidateSurface);
        }

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
            if (oldWidth != info.Width || oldHeight != info.Height || requireUpdate)
            {
                if (oldWidth >= 0 && (oldWidth != info.Width || oldHeight != info.Height))
                    ReleaseInputs();
                requireUpdate = false;
                Update(info.Width, info.Height);
            }

            lever.FlushMovement();
            UpdateIO();
            circles.Draw(canvas);

            lMenuFrame.Draw(canvas);
            rMenuFrame.Draw(canvas);

            lSide.Draw(canvas);
            rSide.Draw(canvas);

            lMenu.Draw(canvas);
            rMenu.Draw(canvas);

            lever.Draw(canvas);

            keyboard.Draw(canvas);

            settingButton.Draw(canvas);

            oldWidth = info.Width;
            oldHeight = info.Height;
        }

        /// <summary>
        /// 计算各个元素的位置和大小
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        private void Update(int width, int height)
        {
            var nSide = BitConverter.GetBytes(keyboard.ShowLeft)[0] + BitConverter.GetBytes(keyboard.ShowRight)[0];
            float baseCoef = 1 / (PanelPaddingRatio * 2 + LRSpacingCoef * (nSide / 2) + ButtonSpacingCoef * nSide * 2 +
                                  nSide * 3);
            // 以一个按钮的边长作为基数计算其他部分尺寸
            float baseLength = MathF.Min(width * baseCoef, height / 3.5f);
            float keyboardWidth = baseLength / baseCoef;
            float keyboardLeft = (width - keyboardWidth) / 2;
            float buttonSideLength = baseLength;

            float menuSideLength = baseLength * MenuSizeCoef;
            float menuPadding = baseLength * MenuPaddingCoef;
            float keyboardMarginTop = baseLength * KeyboardMarginTopCoef;

            float bottomMargin = MathF.Min((height - baseLength) * Settings.ButtonBottomMargin,
                height - baseLength * 3.5f);

            if (Settings.HideGameButtons)
            {
                bottomMargin = 0;
                buttonSideLength = 0;
            }

            keyboard.Padding = new SKPoint(baseLength * PanelPaddingRatio, 0);
            keyboard.Position = new SKPoint(keyboardLeft, height - bottomMargin - buttonSideLength);
            keyboard.Spacing = baseLength * LRSpacingCoef;
            keyboard.Size = new SKSize(keyboardWidth, height - keyboard.Position.Y);
            keyboard.Left.Spacing = keyboard.Right.Spacing = baseLength * ButtonSpacingCoef;
            keyboard.Visible = !Settings.HideGameButtons;

            lMenu.Size = rMenu.Size = new SKSize(menuSideLength, menuSideLength);
            lMenu.Position =
                new SKPoint(keyboardLeft + menuPadding, keyboard.BoundingBox.Top - keyboardMarginTop - menuSideLength * 2);
            rMenu.Position = new SKPoint(keyboard.BoundingBox.Right - menuPadding - menuSideLength, lMenu.Position.Y);
            lMenu.Visible = rMenu.Visible = !Settings.HideMenuButtons && !inRhythmGame;

            lSide.ButtonHeight = rSide.ButtonHeight = baseLength;
            lSide.Size = rSide.Size =
                new SKSize(width / 2f,
                    height - keyboard.BoundingBox.Height);
            lSide.ButtonHeight = rSide.ButtonHeight = baseLength;
            lSide.Position = new SKPoint(0, 0);
            rSide.Position = new SKPoint(width / 2f, 0);
            lSide.Padding = rSide.Padding = new SKPoint(0, keyboardMarginTop);
            lSide.Visible = rSide.Visible = !Settings.HideWallActionDevices;

            if (Settings.EnableCompositeMode || Settings.HideWallActionDevices)
            {
                lever.Size = new SKSize(width, lSide.Size.Height);
                lever.Position = new SKPoint(0, 0);
                lever.Padding = new SKPoint((width - keyboardWidth * LeverWidth) / 2,
                    keyboard.BoundingBox.Top - lMenu.BoundingBox.Bottom);
            }
            else
            {
                lever.Size = new SKSize(keyboardWidth * LeverWidth, lSide.Size.Height);
                lever.Position = new SKPoint((width - lever.Size.Width) / 2, 0);
                lever.Padding = new SKPoint(0, keyboard.BoundingBox.Top - lMenu.BoundingBox.Bottom);
            }

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
        private void CanvasView_Touch(object sender, SKTouchEventArgs args)
        {
            // SKTouchEventArgs.Location is already in the canvas coordinate system.
            SKPoint pixelLocation = args.Location;
            switch (args.ActionType)
            {
                case SKTouchAction.Pressed:
                    if (!activeTouches.Add(args.Id)) break;
                    foreach (TouchableObject obj in touchableObject)
                    {
                        if (obj.Visible && obj.HitTest(pixelLocation) && obj.HandleTouchPressed(args.Id, pixelLocation))
                        {
                            break;
                        }
                    }

                    break;
                case SKTouchAction.Moved:
                    foreach (TouchableObject obj in touchableObject)
                    {
                        if (obj.HandleTouchMoved(args.Id, pixelLocation))
                        {
                            break;
                        }
                    }

                    break;
                case SKTouchAction.Released:
                    activeTouches.Remove(args.Id);
                    foreach (TouchableObject obj in touchableObject)
                    {
                        obj.HandleTouchReleased(args.Id);
                    }

                    break;
                case SKTouchAction.Cancelled:
                    activeTouches.Remove(args.Id);
                    foreach (TouchableObject obj in touchableObject)
                    {
                        obj.HandleTouchCancelled(args.Id);
                    }

                    break;
            }

            //更新IO状态
            UpdateIO();
            //通知重绘画布
            canvasView.InvalidateSurface();
            args.Handled = true;
        }

        public void ReleaseInputs()
        {
            foreach (long id in activeTouches.ToArray())
                foreach (var obj in touchableObject)
                    obj.HandleTouchCancelled(id);
            activeTouches.Clear();
            UpdateIO();
            StaticIO.SetOptionButton(OptionButtons.Test, false);
            StaticIO.SetOptionButton(OptionButtons.Service, false);
            canvasView.InvalidateSurface();
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
                if (StaticIO.Data.GameButtons[i] != buttons[i].TouchCount)
                {
                    StaticIO.SetGameButton(i, buttons[i].TouchCount);
                }
            }

            // lever
            MoveLever(lever.Value);
        }

        private void MoveLever(float x)
        {
            short newValue = (short)(short.MaxValue * x);
            short oldValue = StaticIO.Data.Lever;

            var threshold = short.MaxValue / (Settings.LeverLinearity / 2f);

            // 仅在经过分界的时候发包
            if ((int)(newValue / threshold) != (int)(oldValue / threshold))
            {
                StaticIO.SetLever(newValue);
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
                keyboard.Left[1].Color == ButtonColors.Green &&
                keyboard.Left[2].Color == ButtonColors.Blue;

            if (temp != inRhythmGame) ReleaseInputs();
            inRhythmGame = temp;
            lMenu.Visible = rMenu.Visible = !inRhythmGame && !Settings.HideMenuButtons;
            settingButton.Visible = !inRhythmGame;
            keyboard.AntiMisTouch = Settings.AntiMisTouch && inRhythmGame;
            MainThread.InvokeOnMainThreadAsync(canvasView.InvalidateSurface);
        }
    }
}

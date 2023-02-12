using Mageki.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Mageki.Drawables
{
    public class SideButton : ButtonBase
    {
        public const float colorSaturation = 0.6f;

        public SideButtonMode Mode
        {
            get => GetValue(SideButtonMode.Touch);
            set => SetValueWithNotify(value);
        }

        public SKColor Color
        {
            get => GetValue(SKColors.Black);
            set => SetValueWithNotify(value);
        }

        public float CornerRatio
        {
            get => GetValue(0.025f);
            set => SetValueWithNotify(value);
        }

        public float BorderWidthRatio
        {
            get => GetValue(0.4f);
            set => SetValueWithNotify(value);
        }

        public float ButtonHeight
        {
            get => GetValue(0f);
            set => SetValueWithNotify(value);
        }

        public float Aspect
        {
            get => GetValue(0.08f);
            set => SetValueWithNotify(value);
        }

        public Side Side
        {
            get => GetValue(default(Side));
            set => SetValueWithNotify(value);
        }

        public float Pressure
        {
            get => GetValue(default(float));
            set
            {
                if (value < MinPressure) value = MinPressure;
                else if (value > MaxPressure) value = MaxPressure;
                SetValueWithNotify(value);
            }
        }

        public float MaxPressure
        {
            get => GetValue(0.95f);
            set => SetValueWithNotify(value);
        }

        public float MinPressure
        {
            get => GetValue(0.05f);
            set => SetValueWithNotify(value);
        }

        public float Trigger
        {
            get => GetValue(0.5f);
            set => SetValueWithNotify(value);
        }


        private SKPaint buttonFramePaint = new SKPaint
        {
            Style = SKPaintStyle.StrokeAndFill,
            Color = new SKColor(0xFF222222)
        };

        private SKPaint buttonPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            //MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Solid, 30)
        };

        private SKPaint buttonBlankPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = Colors[ButtonColors.Blank]
        };

        private SKPaint lightPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 100)
        };

        private SKPaint buttonBorderPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = new SKColor(0xFF222222)
        };

        private SKPaint separatorPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = new SKColor(0x44666666)
        };

        private SKPaint holdMaskPaint = new SKPaint()
        {
            Style = SKPaintStyle.Fill,
            Color = new SKColor(0x66000000),
            //MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Solid, 10)
        };

        SKPath buttonPath = new SKPath();
        SKRect buttonFrame = new SKRect();
        SKPath buttonBorderPath = new SKPath();
        SKPath separatorPath = new SKPath();
        SKRect lightBox;

        public SideButton()
        {
        }

        public override void Update()
        {
            if (Mode == SideButtonMode.Touch)
            {
                if (TouchCount > 0)
                {
                    Pressure = 0.6f;
                }
                else
                {
                    Pressure = 0f;
                }
            }
            else
            {
                if (Pressure >= Trigger)
                {
                    TouchCount = 1;
                }
                else
                {
                    TouchCount = 0;
                }
            }

            int n = Side == Side.Left ? 1 : -1;
            SKRect boundingBox = BoundingBox;
            float buttonHeight = ButtonHeight;
            float buttonWidth = buttonHeight * Aspect;

            var buttonTopFaceLeft = boundingBox.MidX - boundingBox.Width / 2 * n;
            var buttonTopFaceRight = buttonTopFaceLeft + (buttonWidth * (1 - Pressure)) * n;
            var buttonTopFaceBottom = boundingBox.Bottom - Padding.Y;
            var buttonTopFaceTop = buttonTopFaceBottom - buttonHeight;

            var buttonSideFaceRight = buttonTopFaceRight + (buttonWidth * (1 + Pressure * 0.6f)) * n;
            var buttonSideFaceTop = buttonTopFaceTop + (buttonTopFaceBottom - buttonTopFaceTop) * (1f / 16f);
            var buttonSideFaceBottom = buttonTopFaceBottom - (buttonSideFaceTop - buttonTopFaceTop);

            buttonPath.Reset();
            buttonPath.MoveTo(buttonTopFaceLeft, buttonTopFaceTop);
            buttonPath.LineTo(buttonTopFaceRight, buttonTopFaceTop);
            buttonPath.LineTo(buttonSideFaceRight, buttonSideFaceTop);
            buttonPath.LineTo(buttonSideFaceRight, buttonSideFaceBottom);
            buttonPath.LineTo(buttonTopFaceRight, buttonTopFaceBottom);
            buttonPath.LineTo(buttonTopFaceLeft, buttonTopFaceBottom);
            buttonPath.Close();

            buttonBorderPath.Reset();
            buttonBorderPath.MoveTo(buttonTopFaceLeft, buttonTopFaceTop);
            buttonBorderPath.LineTo(buttonTopFaceRight, buttonTopFaceTop);
            buttonBorderPath.LineTo(buttonSideFaceRight, buttonSideFaceTop);
            buttonBorderPath.LineTo(buttonSideFaceRight, buttonSideFaceBottom);
            buttonBorderPath.LineTo(buttonTopFaceRight, buttonTopFaceBottom);
            buttonBorderPath.LineTo(buttonTopFaceLeft, buttonTopFaceBottom);

            separatorPath.Reset();
            separatorPath.MoveTo(buttonTopFaceRight, buttonTopFaceTop);
            separatorPath.LineTo(buttonTopFaceRight, buttonTopFaceBottom);

            buttonFrame = SKRect.Create(buttonSideFaceRight, buttonSideFaceTop, buttonWidth * (Pressure * 0.4f),
                buttonSideFaceBottom - buttonSideFaceTop);

            lightBox = new SKRect(
                buttonTopFaceRight + (buttonWidth * 2) * n,
                buttonTopFaceTop,
                boundingBox.MidX + (boundingBox.Width / 2) * n,
                buttonTopFaceBottom);

            buttonPaint.Color = Color.Standardization();
            lightPaint.Color = buttonPaint.Color.WithAlpha((byte)(buttonPaint.Color.Alpha));
            lightPaint.Shader = SKShader.CreateLinearGradient(
                new SKPoint(lightBox.Left, lightBox.MidY),
                new SKPoint(lightBox.Right, lightBox.MidY),
                new SKColor[]
                {
                    new SKColor(lightPaint.Color.Red, lightPaint.Color.Green, lightPaint.Color.Blue, 0xAA),
                    new SKColor(lightPaint.Color.Red, lightPaint.Color.Green, lightPaint.Color.Blue, 0x44)
                },
                SKShaderTileMode.Clamp);

            buttonBorderPaint.StrokeWidth = buttonFramePaint.StrokeWidth = buttonWidth * 0.5f;
            separatorPaint.StrokeWidth = buttonWidth * 0.2f;

            base.Update();
        }


        public override void Draw(SKCanvas canvas)
        {
            if (!Visible) return;
            base.Draw(canvas);
            canvas.DrawPath(buttonPath, buttonBlankPaint);
            canvas.DrawPath(buttonPath, buttonPaint);
            if (TouchCount > 0)
            {
                canvas.DrawPath(buttonPath, holdMaskPaint);
            }

            canvas.DrawPath(separatorPath, separatorPaint);
            canvas.DrawPath(buttonBorderPath, buttonBorderPaint);
            canvas.DrawRect(buttonFrame, buttonFramePaint);

            //canvas.DrawRect(lightBox, lightPaint);
        }

        public override bool HandleTouchPressed(long id, SKPoint point)
        {
            if (Mode == SideButtonMode.Touch)
            {
                return base.HandleTouchPressed(id, point);
            }
            else
            {
                touchPoints.Add(id, point);
                return true;
            }
        }

        private List<(float value, long touchID)> moveCache = new List<(float value, long touchID)>();

        public override bool HandleTouchMoved(long id, SKPoint point)
        {
            if (Mode == SideButtonMode.Slide)
            {
                if (touchPoints.ContainsKey(id))
                {
                    lock (moveCache)
                    {
                        // 无法判断触点是哪一帧传来，所以在传来重复id时认为到了下一帧
                        bool idDuplicated = moveCache.Any(c => c.touchID == id);
                        moveCache.Add((point.X - touchPoints[id].X, id));
                        if (idDuplicated)
                        {
                            // 计算全部移动的和，并将其限制在最大与最小值之间
                            var min = moveCache.Select(v => v.value).Min();
                            var max = moveCache.Select(v => v.value).Max();
                            var sum = moveCache.Sum(v => v.value);
                            if (min < 0 && sum < min)
                            {
                                sum = min;
                            }

                            if (max > 0 && sum > max)
                            {
                                sum = max;
                            }

                            int n = Side == Side.Left ? 1 : -1;
                            SKRect boundingBox = BoundingBox;

                            float buttonHeight = ButtonHeight;
                            float buttonWidth = buttonHeight * Aspect;

                            Pressure += sum / buttonWidth * -n;

                            moveCache.Clear();
                        }
                    }
                }
            }

            return base.HandleTouchMoved(id, point);
        }

        public override void HandleTouchReleased(long id)
        {
            if (Mode == SideButtonMode.Touch)
            {
                base.HandleTouchReleased(id);
            }
            else if (touchPoints.ContainsKey(id))
            {
                touchPoints.Remove(id);
                if (touchPoints.Count == 0)
                {
                    Pressure = 0;
                    Update();
                }
            }
        }
    }

    public enum SideButtonMode
    {
        Touch,
        Slide,
    }
}
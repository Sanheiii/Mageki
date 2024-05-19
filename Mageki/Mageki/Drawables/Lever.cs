using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Mageki.Drawables
{
    public class Lever : Box
    {
        /// <summary>
        /// 眼睛距离面板的距离
        /// </summary>
        const float eyePanelDistanceMM = 500f;

        const float holeWidthMM = 86f;
        const float holeHeightMM = 14f;

        /// <summary>
        /// 摇杆直径
        /// </summary>
        const float stickDiameterMM = 8f;

        /// <summary>
        /// 摇杆长度
        /// </summary>
        const float stickLengthMM = 160f;

        /// <summary>
        /// 摇杆帽直径
        /// </summary>
        const float capDiameterMM = 34f;

        /// <summary>
        /// 摇杆帽高度
        /// </summary>
        const float capHeightMM = 60f;

        /// <summary>
        /// 转轴和面板的距离
        /// </summary>
        const float panelShaftDistanceMM = 63f;

        /// <summary>
        /// 限位器的直径
        /// </summary>
        const float limiterDiameterMM = 5f;

        /// <summary>
        /// 两个限位器中心的距离
        /// </summary>
        const float limitersDistanceMM = 49f;

        /// <summary>
        /// -1 -> 1
        /// </summary>
        public float Value
        {
            get => GetValue(default(float));
            set
            {
                if (value < -1)
                {
                    valueOverflow += value + 1;
                    value = -1;
                }
                else if (value > 1)
                {
                    valueOverflow += value - 1;
                    value = 1;
                }
                SetValueWithNotify(value);
            }
        }

        public float valueOverflow = 0;

        private SKPath capTopPath = new SKPath();
        private SKPath capSidePath = new SKPath();
        private SKPath holePath = new SKPath();
        private SKPath stickSide = new SKPath();
        private SKPath stickHighLightPath = new SKPath();
        private SKPath capHighLightPath = new SKPath();

        private SKPaint stickSidePaint = new SKPaint()
        {
            Style = SKPaintStyle.Fill,
            Color = new SKColor(0xFFD0D0D0)
        };

        private SKPaint stickHighLightPaint = new SKPaint()
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.White,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 5)
        };

        private SKPaint capHighLightPaint = new SKPaint()
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.White,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 20)
        };

        private SKPaint stickTopBorderPaint = new SKPaint()
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 10,
            Color = new SKColor(0xFF222222)
        };

        private SKPaint capPaint = new SKPaint()
        {
            Style = SKPaintStyle.Fill,
            Color = new SKColor(0xFF222222)
        };

        private SKPaint capStrokePaint = new SKPaint()
        {
            Style = SKPaintStyle.Stroke,
            Color = new SKColor(0xFF888888)
        };

        private SKPaint stickSideBorderPaint = new SKPaint()
        {
            Style = SKPaintStyle.Stroke,
            Color = new SKColor(0xFF222222)
        };

        private SKPaint holePaint = new SKPaint
        {
            Style = SKPaintStyle.StrokeAndFill,
            Color = new SKColor(0xFF666666)
        };

        private SKPaint backPaint = new SKPaint
        {
            Style = SKPaintStyle.StrokeAndFill,
            Color = SKColors.White
        };

        public override void Update()
        {
            var boundingBox = BoundingBox;
            // 摇杆末端的最大运动范围
            var endPointRangeMM = stickLengthMM;
            var endPointRangePx = (Size.Width - Padding.X * 2) * stickLengthMM /
                                  (stickLengthMM + capDiameterMM * 0.5f /*cos 60°*/);
            // 摇杆末端在最边上的高度
            var endPointMinHeightMM = stickLengthMM * MathF.Cos(MathF.PI / 6);
            // 计算面板平面上像素密度
            var panelPlanePixelPerMM = endPointRangePx / endPointRangeMM / eyePanelDistanceMM *
                                       (eyePanelDistanceMM - (endPointMinHeightMM - panelShaftDistanceMM));
            var holeWidthPx = holeWidthMM * panelPlanePixelPerMM;
            var holeHeightPx = holeHeightMM * panelPlanePixelPerMM;

            // 洞不含两侧半圆的四角，半圆直径等于洞的高度
            var holeLeft = boundingBox.MidX - holeWidthPx / 2;
            var holeBottom = boundingBox.Bottom - Padding.Y;
            var holeTop = holeBottom - holeHeightPx;
            var holeRight = holeLeft + holeWidthPx;

            holePath.Reset();
            holePath.MoveTo(holeLeft, holeBottom);
            holePath.LineTo(holeRight, holeBottom);
            holePath.ArcTo(new SKRect(holeRight, holeTop, holeRight + holeHeightPx, holeBottom), 90, -180, false);
            holePath.LineTo(holeLeft, holeTop);
            holePath.ArcTo(new SKRect(holeLeft - holeHeightPx, holeTop, holeLeft, holeBottom), -90, -180, false);
            holePath.Close();

            holePaint.StrokeWidth = holeHeightPx / 8f;
            backPaint.StrokeWidth = holePaint.StrokeWidth * 2;

            // 摇杆末端高度
            var endPointHeightMM = stickLengthMM * MathF.Cos(MathF.PI / 6f * Value);
            // 摇杆末端水平面的像素密度
            var endPointPlanePixelPerMM = panelPlanePixelPerMM /
                (eyePanelDistanceMM - (endPointHeightMM - panelShaftDistanceMM)) * eyePanelDistanceMM;

            var endPointStickDiameterPx = stickDiameterMM * endPointPlanePixelPerMM;

            // 计算限位器水平面的像素密度与摇杆直径
            var limiterRangeMM = limitersDistanceMM - limiterDiameterMM - stickDiameterMM;
            var limiterPlaneHeightMM = -panelShaftDistanceMM + limiterRangeMM / 2 / (MathF.Tan(MathF.PI / 6f));
            var limiterPlanePixelPerMM = panelPlanePixelPerMM /
                (eyePanelDistanceMM - (limiterPlaneHeightMM - panelShaftDistanceMM)) * eyePanelDistanceMM;
            var limiterPlaneStickDiameterPx = stickDiameterMM * limiterPlanePixelPerMM;
            var limiterRangePx = limiterRangeMM * limiterPlanePixelPerMM;

            var holeMidX = (holeLeft + holeRight) / 2;
            var holeMidY = (holeTop + holeBottom) / 2;
            // 摇杆当前角度下切面经过透视的宽高比
            var ratio = MathF.Cos(MathF.PI / 3 * Value);
            // 摇杆末端，会被摇杆头遮住，无需绘制
            //stickTop.Reset();
            //stickTop.AddOval(new SKRect(
            //    holeMidX + endPointRangePx / 2f * Value - endPointStickDiameterPx / 2 * ratio,
            //    holeMidY - endPointStickDiameterPx / 2f,
            //    holeMidX + endPointRangePx / 2f * Value + endPointStickDiameterPx / 2 * ratio,
            //    holeMidY + endPointStickDiameterPx / 2f));
            // 摇杆侧面
            stickSide.Reset();
            stickSide.MoveTo(holeMidX + endPointRangePx / 2f * Value, holeMidY + endPointStickDiameterPx / 2f);
            stickSide.LineTo(holeMidX + limiterRangePx / 2f * Value, holeMidY + limiterPlaneStickDiameterPx / 2f);
            stickSide.ArcTo(new SKRect(
                    holeMidX + limiterRangePx / 2f * Value - limiterPlaneStickDiameterPx / 2 * ratio,
                    holeMidY - limiterPlaneStickDiameterPx / 2f,
                    holeMidX + limiterRangePx / 2f * Value + limiterPlaneStickDiameterPx / 2 * ratio,
                    holeMidY + limiterPlaneStickDiameterPx / 2f),
                90, Value > 0 ? 180 : -180, false);
            stickSide.LineTo(holeMidX + endPointRangePx / 2f * Value, holeMidY - endPointStickDiameterPx / 2f);

            // 面板下至限位器部分摇杆渐隐效果，额外加上(limiterPlaneStickDiameterPx / 2f * ratio * (Value < 0 ? 1 : -1))使渐隐末端看起来是圆头
            stickSideBorderPaint.Shader = SKShader.CreateLinearGradient(
                new SKPoint(holeMidX + holeWidthPx / 2f * Value, holeMidY),
                new SKPoint(
                    holeMidX + limiterRangePx / 2f * Value +
                    (limiterPlaneStickDiameterPx / 2f * ratio * (Value < 0 ? 1 : -1)), holeMidY),
                new SKColor[] { stickSideBorderPaint.Color, stickSideBorderPaint.Color.WithAlpha(0) },
                SKShaderTileMode.Clamp);
            stickSidePaint.Shader = SKShader.CreateLinearGradient(
                new SKPoint(holeMidX + holeWidthPx / 2f * Value, holeMidY),
                new SKPoint(
                    holeMidX + limiterRangePx / 2f * Value +
                    (limiterPlaneStickDiameterPx / 2f * ratio * (Value < 0 ? 1 : -1)), holeMidY),
                new SKColor[] { stickSidePaint.Color, stickSidePaint.Color.WithAlpha(0) },
                SKShaderTileMode.Clamp);

            // 摇杆高光使摇杆更立体
            stickHighLightPath.Reset();
            stickHighLightPath.MoveTo(holeMidX + limiterRangePx / 2f * Value,
                holeMidY + limiterPlaneStickDiameterPx * -0.1f / 2f);
            stickHighLightPath.LineTo(holeMidX + endPointRangePx / 2f * Value,
                holeMidY + endPointStickDiameterPx * -0.1f / 2f);
            stickHighLightPath.LineTo(holeMidX + endPointRangePx / 2f * Value,
                holeMidY + endPointStickDiameterPx * -0.7f / 2f);
            stickHighLightPath.LineTo(holeMidX + limiterRangePx / 2f * Value,
                holeMidY + limiterPlaneStickDiameterPx * -0.7f / 2f);
            stickHighLightPaint.Shader = SKShader.CreateLinearGradient(
                new SKPoint(holeMidX + holeWidthPx / 2f * Value, holeMidY),
                new SKPoint(holeMidX + limiterRangePx / 2f * Value, holeMidY),
                new SKColor[] { stickHighLightPaint.Color, stickHighLightPaint.Color.WithAlpha(0) },
                SKShaderTileMode.Clamp);

            // 摇杆帽
            capTopPath.Reset();
            var capDiameterPx = capDiameterMM * endPointPlanePixelPerMM;
            capTopPath.AddOval(new SKRect(
                holeMidX + endPointRangePx / 2f * Value - capDiameterPx / 2 * ratio,
                holeMidY - capDiameterPx / 2f,
                holeMidX + endPointRangePx / 2f * Value + capDiameterPx / 2 * ratio,
                holeMidY + capDiameterPx / 2f));
            // 摇杆帽侧面
            var capBottomRangeMM = endPointRangeMM / stickLengthMM * (stickLengthMM - capHeightMM);
            // 摇杆帽底面相对于转轴的高度
            var capBottomHeightMM = (stickLengthMM - capHeightMM) * MathF.Cos(MathF.PI / 6f * Value);
            var capBottomPlanePixelPerMM = panelPlanePixelPerMM /
                (eyePanelDistanceMM - (capBottomHeightMM - panelShaftDistanceMM)) * eyePanelDistanceMM;
            var capBottomRangePx = capBottomRangeMM * capBottomPlanePixelPerMM;
            var capBottomDiameterPx = capDiameterMM * capBottomPlanePixelPerMM;
            capSidePath.Reset();
            capSidePath.MoveTo(holeMidX + endPointRangePx / 2f * Value, holeMidY + capDiameterPx / 2f);
            capSidePath.LineTo(holeMidX + capBottomRangePx / 2f * Value, holeMidY + capBottomDiameterPx / 2f);
            capSidePath.ArcTo(new SKRect(
                    holeMidX + capBottomRangePx / 2f * Value - capBottomDiameterPx / 2 * ratio,
                    holeMidY - capBottomDiameterPx / 2f,
                    holeMidX + capBottomRangePx / 2f * Value + capBottomDiameterPx / 2 * ratio,
                    holeMidY + capBottomDiameterPx / 2f),
                90, Value > 0 ? 180 : -180, false);
            capSidePath.LineTo(holeMidX + endPointRangePx / 2f * Value, holeMidY - capDiameterPx / 2f);
            capStrokePaint.StrokeWidth = stickSideBorderPaint.StrokeWidth = capDiameterPx / 20;

            // 摇杆头高光
            //capHighLightPath.Reset();
            //capHighLightPath.MoveTo(holeMidX + capBottomRangePx / 2f * Value + capBottomDiameterPx / 2 * ratio * (Value < 0 ? 1 : -1), holeMidY + capBottomDiameterPx * -0.1f / 2f);
            //capHighLightPath.LineTo(holeMidX + endPointRangePx / 2f * Value + capDiameterPx / 2 * ratio * (Value < 0 ? 1 : -1), holeMidY + capDiameterPx * -0.1f / 2f);
            //capHighLightPath.LineTo(holeMidX + endPointRangePx / 2f * Value + capDiameterPx / 2 * ratio * (Value < 0 ? 1 : -1), holeMidY + capDiameterPx * -0.5f / 2f);
            //capHighLightPath.LineTo(holeMidX + capBottomRangePx / 2f * Value + capBottomDiameterPx / 2 * ratio * (Value < 0 ? 1 : -1), holeMidY + capBottomDiameterPx * -0.5f / 2f);
            //stickHighLightPaint.Shader = SKShader.CreateLinearGradient(
            //    new SKPoint(holeMidX + holeWidthPx / 2f * Value, holeMidY),
            //    new SKPoint(holeMidX + capBottomRangePx / 2f * Value, holeMidY),
            //    new SKColor[] { stickHighLightPaint.Color, stickHighLightPaint.Color.WithAlpha(0) },
            //    SKShaderTileMode.Clamp);

            base.Update();
        }

        public override void Draw(SKCanvas canvas)
        {
            if (!Visible) return;
            base.Draw(canvas);

            canvas.DrawPath(holePath, backPaint);
            canvas.DrawPath(holePath, holePaint);
            canvas.DrawPath(stickSide, stickSidePaint);
            canvas.DrawPath(stickSide, stickSideBorderPaint);
            canvas.DrawPath(stickHighLightPath, stickHighLightPaint);
            //canvas.DrawPath(stickTop, stickTopPaint);
            //canvas.DrawPath(stickTop, stickTopBorderPaint);
            canvas.DrawPath(capSidePath, capPaint);
            canvas.DrawPath(capSidePath, capStrokePaint);
            //canvas.DrawPath(capHighLightPath, capHighLightPaint);
            canvas.DrawPath(capTopPath, capPaint);
            canvas.DrawPath(capTopPath, capStrokePaint);
            //canvas.DrawPath(ballPath, stickTopPaint);
        }

        public override bool HandleTouchPressed(long id, SKPoint point)
        {
            if ((Settings.LeverMoveMode == LeverMoveMode.Absolute && touchPoints.Count == 0) ||
                Settings.LeverMoveMode == LeverMoveMode.Relative)
            {
                touchPoints.Add(id, point);
            }

            bool handled = base.HandleTouchPressed(id, point);
            if (Settings.EnableCompositeMode) return false;
            else return handled;
        }

        private List<(float value, long touchID)> moveCache = new List<(float value, long touchID)>();

        public override bool HandleTouchMoved(long id, SKPoint point)
        {
            if (touchPoints.ContainsKey(id))
            {
                var boundingBox = BoundingBox;
                if (Settings.LeverMoveMode == LeverMoveMode.Absolute)
                {
                    var x = point.X - boundingBox.MidX;
                    Value = x / (boundingBox.Width / 2);
                }
                else if (Settings.LeverMoveMode == LeverMoveMode.Relative)
                {
                    lock (moveCache)
                    {
                        // 无法判断触点是哪一帧传来，所以在传来重复id时认为到了下一帧
                        bool idDuplicated = moveCache.Any(c => c.touchID == id);
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

                            var diffPx = sum / (boundingBox.Width / 2 - Padding.X);
                            var diff = diffPx * Settings.LeverSensitivity;
                            // 如果溢出值有剩余优先抵消溢出值
                            if(Settings.EnableLeverOverflowHandling && valueOverflow * diff < 0)
                            {
                                if(Math.Abs(valueOverflow) < Math.Abs(diff))
                                {
                                    diff += valueOverflow;
                                    valueOverflow = 0;
                                }
                                else
                                {
                                    valueOverflow += diff;
                                    diff = 0;
                                }
                            }
                            if(diff != 0)
                            {
                                Value += diff;
                            }

                            moveCache.Clear();
                        }

                        moveCache.Add((point.X - touchPoints[id].X, id));
                    }
                }
            }

            return base.HandleTouchMoved(id, point);
        }

        public override void HandleTouchReleased(long id)
        {
            base.HandleTouchReleased(id);
            if (Settings.EnableLeverOverflowHandling && touchPoints.Count == 0) valueOverflow = 0;
        }
    }
}
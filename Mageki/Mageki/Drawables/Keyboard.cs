using SkiaSharp;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;

using Xamarin.Forms;

namespace Mageki.Drawables
{
    public class Keyboard : Container
    {
        public float Spacing { get => GetValue(default(float)); set => SetValueWithNotify(value); }
        public HalfKeyBoard Left => Children[0] as HalfKeyBoard;
        public HalfKeyBoard Right => Children[1] as HalfKeyBoard;
        public bool ShowLeft { get => GetValue(true); set => SetValueWithNotify(value); }
        public bool ShowRight { get => GetValue(true); set => SetValueWithNotify(value); }
        public bool AntiMisTouch
        {
            get => GetValue(false);
            set
            {
                if (!value) buttonBasePoints = new float[2][];
                SetValueWithNotify(value);
            }
        }

        private SKPaint basePointPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 4,
            Color = new SKColor(0xFF000000)
        };

        private SKPaint basePointStrokePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 8,
            Color = new SKColor(0xFFFFFFFF)
        };

        public Keyboard() : base()
        {
            Children = new List<IDrawable>() { new HalfKeyBoard(), new HalfKeyBoard() };
            this[0].Color = ButtonColors.Red;
            this[1].Color = ButtonColors.Green;
            this[2].Color = ButtonColors.Blue;
            this[3].Color = ButtonColors.Red;
            this[4].Color = ButtonColors.Green;
            this[5].Color = ButtonColors.Blue;
        }
        public SquareButton this[int index]
        {
            get
            {
                if (index > 5 || index < 0) throw new IndexOutOfRangeException();

                if (index / 3 == 0)
                {
                    return Left[index];
                }
                else
                {
                    return Right[index % 3];
                }
            }
        }
        public override void Update()
        {
            var position = new SKPoint(Position.X + Padding.X, Position.Y + Padding.Y);
            var n = BitConverter.GetBytes(ShowLeft)[0] + BitConverter.GetBytes(ShowRight)[0];
            var size = new SKSize((Size.Width - Padding.X * 2 - Spacing * (n / 2)) / n, Size.Height - Padding.Y * n);
            Left.Visible = ShowLeft;
            Right.Visible = ShowRight;
            if (ShowLeft)
            {
                Left.Position = position;
                Left.Size = size;
                position = new SKPoint(position.X + size.Width + Spacing, Left.Position.Y);
            }
            if (ShowRight)
            {
                Right.Position = position;
                Right.Size = size;
            }
            base.Update();
        }

        public override void Draw(SKCanvas canvas)
        {
            base.Draw(canvas);
            if (AntiMisTouch)
            {
                if (ShowLeft && buttonBasePoints[0] != null)
                {
                    var leftBasePoints = buttonBasePoints[0].Select(x => new SKPoint(x, Left[0].BoundingBox.MidY)).ToArray();
                    canvas.DrawPoints(SKPointMode.Points, leftBasePoints, basePointStrokePaint);
                    canvas.DrawPoints(SKPointMode.Points, leftBasePoints, basePointPaint);
                }
                if (ShowRight && buttonBasePoints[1] != null)
                {
                    var rightBasePoints = buttonBasePoints[1].Select(x => new SKPoint(x, Right[0].BoundingBox.MidY)).ToArray();
                    canvas.DrawPoints(SKPointMode.Points, rightBasePoints, basePointStrokePaint);
                    canvas.DrawPoints(SKPointMode.Points, rightBasePoints, basePointPaint);
                }
            }
        }

        public override bool HandleTouchPressed(long id, SKPoint point)
        {
            touchPoints.Add(id, point);
            if (AntiMisTouch)
            {
                SetButtonsWithAntiMisTouch();
            }
            else
            {
                int index = GetKeyIndexFromX(point.X);
                this[index].TouchCount++;
            }
            return base.HandleTouchPressed(id, point);
        }

        public override bool HandleTouchMoved(long id, SKPoint point)
        {
            if (touchPoints.ContainsKey(id))
            {
                if (AntiMisTouch)
                {
                    SetButtonsWithAntiMisTouch();
                }
                else
                {
                    var oldIndex = GetKeyIndexFromX(touchPoints[id].X);
                    var newIndex = GetKeyIndexFromX(point.X);
                    if (oldIndex != newIndex)
                    {
                        this[oldIndex].TouchCount--;
                        this[newIndex].TouchCount++;
                    }
                }
            }

            return base.HandleTouchMoved(id, point);
        }

        private void SetButtonsWithAntiMisTouch()
        {
            var points = touchPoints
                .Select(p => p.Value);
            var leftPoints = points.Where(p => GetKeyIndexFromX(p.X) < 3).ToArray();
            var rightPoints = points.Except(leftPoints).ToArray();
            if (ShowLeft)
            {
                SetHalfKeyboardWithAntiMisTouch(leftPoints, Side.Left);
            }
            else
            {
                Left[0].TouchCount = Left[1].TouchCount = Left[2].TouchCount = 0;
            }

            if (ShowRight)
            {
                SetHalfKeyboardWithAntiMisTouch(rightPoints, Side.Right);
            }
            else
            {
                Right[0].TouchCount = Right[1].TouchCount = Right[2].TouchCount = 0;
            }
        }
        float[][] buttonBasePoints = new float[2][];
        private void SetHalfKeyboardWithAntiMisTouch(SKPoint[] points, Side side)
        {
            points = points.OrderBy(p => p.X).ToArray();
            HalfKeyBoard half = (HalfKeyBoard)Children[(int)side];
            var buttonWidth = half[1].BoundingBox.Right - half[0].BoundingBox.Right;
            if (buttonBasePoints[(int)side] == null)
            {
                buttonBasePoints[(int)side] = new float[3]
                {
                    half[0].BoundingBox.MidX,
                    half[1].BoundingBox.MidX,
                    half[2].BoundingBox.MidX
                };
            }
            float[] xSeparations = new float[2];
            xSeparations[0] = buttonBasePoints[(int)side][0] / 2 + buttonBasePoints[(int)side][1] / 2;
            xSeparations[1] = buttonBasePoints[(int)side][1] / 2 + buttonBasePoints[(int)side][2] / 2;
            if (points.Length == 0)
            {
                half[0].TouchCount = 0;
                half[1].TouchCount = 0;
                half[2].TouchCount = 0;
            }
            else if (points.Length == 1)
            {
                int index = 2;
                for (int i = 0; i < 2; i++)
                {
                    if (points[0].X < xSeparations[i])
                    {
                        index = i;
                        break;
                    }
                }
                var offset = points[0].X - buttonBasePoints[(int)side][index];
                for (int i = 0; i < 3; i++)
                {
                    half[i].TouchCount = i == index ? (byte)1 : (byte)0;
                    buttonBasePoints[(int)side][i] += offset;
                }
            }
            else if (points.Length == 2)
            {
                if (MathF.Abs(points[0].X - points[1].X) < buttonWidth * 1.5f)
                {
                    if ((points[0].X + points[1].X) / 2 < buttonBasePoints[(int)side][1])
                    {
                        half[0].TouchCount = 1;
                        half[1].TouchCount = 1;
                        half[2].TouchCount = 0;
                        buttonBasePoints[(int)side][1] = points[1].X;
                    }
                    else
                    {
                        half[0].TouchCount = 0;
                        half[1].TouchCount = 1;
                        half[2].TouchCount = 1;
                        buttonBasePoints[(int)side][1] = points[0].X;
                    }
                }
                else
                {
                    half[0].TouchCount = 1;
                    half[1].TouchCount = 0;
                    half[2].TouchCount = 1;
                    buttonBasePoints[(int)side][1] = points[0].X / 2 + points[1].X / 2;
                }
                buttonBasePoints[(int)side][0] = buttonBasePoints[(int)side][1] - buttonWidth;
                buttonBasePoints[(int)side][2] = buttonBasePoints[(int)side][1] + buttonWidth;
            }
            else if (points.Length >= 3)
            {
                if (side == Side.Left)
                {
                    buttonBasePoints[(int)side][1] = points[^2].X;
                }
                if (side == Side.Right)
                {
                    buttonBasePoints[(int)side][1] = points[1].X;
                }
                buttonBasePoints[(int)side][0] = buttonBasePoints[(int)side][1] - buttonWidth;
                buttonBasePoints[(int)side][2] = buttonBasePoints[(int)side][1] + buttonWidth;
                half[0].TouchCount = 1;
                half[1].TouchCount = 1;
                half[2].TouchCount = 1;
            }

            if (buttonBasePoints[(int)side][0] < half.BoundingBox.Left)
            {
                float offset = half.BoundingBox.Left - buttonBasePoints[(int)side][0];
                for (int i = 0; i < 3; i++)
                {
                    buttonBasePoints[(int)side][i] += offset;
                }
            }
            if (buttonBasePoints[(int)side][2] > half.BoundingBox.Right)
            {
                float offset = half.BoundingBox.Right - buttonBasePoints[(int)side][2];
                for (int i = 0; i < 3; i++)
                {
                    buttonBasePoints[(int)side][i] += offset;
                }
            }
        }

        public override void HandleTouchReleased(long id)
        {
            if (AntiMisTouch)
            {
                base.HandleTouchReleased(id);
                SetButtonsWithAntiMisTouch();
            }
            else
            {
                if (touchPoints.ContainsKey(id))
                {
                    var index = GetKeyIndexFromX(touchPoints[id].X);
                    this[index].TouchCount--;
                }
                base.HandleTouchReleased(id);
            }
        }
        private List<float> GetXSeparations()
        {
            List<float> xSeparations = new List<float>();
            if (ShowLeft)
            {
                xSeparations.Add(Left[0].BoundingBox.Right + Left.Spacing / 2);
                xSeparations.Add(Left[1].BoundingBox.Right + Left.Spacing / 2);
            }
            if (ShowLeft && ShowRight)
            {
                xSeparations.Add(Left[2].BoundingBox.Right + Spacing / 2);
            }
            if (ShowRight)
            {
                xSeparations.Add(Right[0].BoundingBox.Right + Right.Spacing / 2);
                xSeparations.Add(Right[1].BoundingBox.Right + Right.Spacing / 2);
            }
            return xSeparations;
        }
        public int GetKeyIndexFromX(float x)
        {
            List<float> xSeparations = GetXSeparations();
            int result = -1;
            for (int i = 0; i < xSeparations.Count; i++)
            {
                if (x < xSeparations[i])
                {
                    result = i;
                    break;
                }
            }
            if (result == -1) result = xSeparations.Count;
            if (!ShowLeft) result += 3;
            return result;
        }
    }
}

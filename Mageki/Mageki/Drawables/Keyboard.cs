using SkiaSharp;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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

        public override bool HandleTouchPressed(long id, SKPoint point)
        {
            touchPoints.Add(id, point);
            int index = GetKeyIndexFromX(point.X);
            this[index].TouchCount++;
            return base.HandleTouchPressed(id, point);
        }

        public override bool HandleTouchMoved(long id, SKPoint point)
        {
            if (touchPoints.ContainsKey(id))
            {
                var oldIndex = GetKeyIndexFromX(touchPoints[id].X);
                var newIndex = GetKeyIndexFromX(point.X);
                if (oldIndex != newIndex)
                {
                    this[oldIndex].TouchCount--;
                    this[newIndex].TouchCount++;
                }
            }
            return base.HandleTouchMoved(id, point);
        }

        public override bool HandleTouchReleased(long id)
        {
            if (touchPoints.ContainsKey(id))
            {
                var index = GetKeyIndexFromX(touchPoints[id].X);
                this[index].TouchCount--;
            }
            return base.HandleTouchReleased(id);
        }

        public int GetKeyIndexFromX(float x)
        {
            var n = BitConverter.GetBytes(ShowLeft)[0] + BitConverter.GetBytes(ShowRight)[0];
            List<float> separationXs = new List<float>();
            if (ShowLeft)
            {
                separationXs.Add(Left[0].BoundingBox.Right + Left.Spacing / 2);
                separationXs.Add(Left[1].BoundingBox.Right + Left.Spacing / 2);
            }
            if (n == 2)
            {
                separationXs.Add(Left[2].BoundingBox.Right + Spacing / 2);
            }
            if (ShowRight)
            {
                separationXs.Add(Right[0].BoundingBox.Right + Right.Spacing / 2);
                separationXs.Add(Right[1].BoundingBox.Right + Right.Spacing / 2);
            }
            int result = -1;
            for (int i = 0; i < separationXs.Count; i++)
            {
                if (x < separationXs[i])
                {
                    result = i;
                    break;
                }
            }
            if (result == -1) result = separationXs.Count;
            if (!ShowLeft) result += 3;
            return result;
        }
    }
}

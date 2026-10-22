using SkiaSharp;

using System;
using System.Collections.Generic;

namespace Mageki.Drawables
{
    public class HalfKeyBoard : Container
    {
        public float Spacing { get => GetValue(default(float)); set => SetValueWithNotify(value); }

        public HalfKeyBoard()
        {
            Children = new List<IDrawable>() { new SquareButton(), new SquareButton(), new SquareButton() };
        }

        public SquareButton this[int index]
        {
            get
            {
                if (index > 2 || index < 0) throw new IndexOutOfRangeException();
                return Children[index] as SquareButton;
            }
        }
        public override void Update()
        {
            this[0].Position = new SKPoint(Position.X + Padding.X, Position.Y + Padding.Y);
            this[0].Size = new SKSize((Size.Width - Spacing * 2 - Padding.X * 2) / 3, Size.Height - Padding.Y * 2);
            this[1].Position = new SKPoint(this[0].BoundingBox.Right + Spacing, this[0].Position.Y);
            this[1].Size = this[0].Size;
            this[2].Position = new SKPoint(this[1].BoundingBox.Right + Spacing, this[0].Position.Y);
            this[2].Size = this[0].Size;
            base.Update();
        }
    }
}

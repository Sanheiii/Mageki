using SkiaSharp;

using System;
using System.Collections.Generic;
using System.Text;

namespace Mageki.Drawables
{
    internal class Slider
    {
        public const short MaxValue = short.MaxValue;
        int leverHalfWidth = 25;
        public short Value { get; set; }
        public SKPaint BackPaint { get; set; }
        public SKRect BackRect { get; set; }
        public SKPaint LeverPaint { get; set; }
        public SKRect LeverRect
        {
            get
            {
                var leverCenter = ((BackRect.Left + BackRect.Right) / 2) + (BackRect.Right - BackRect.Left) / 2 * (Value / (float)MaxValue);
                return new SKRect(leverCenter - leverHalfWidth, BackRect.Top, leverCenter + leverHalfWidth, BackRect.Bottom);
            }
        }
        public Slider()
        {
            BackPaint = new SKPaint()
            {
                Style = SKPaintStyle.Fill,
                Color = SKColor.Parse("FFFF0000")
            };
            LeverPaint = new SKPaint()
            {
                Style = SKPaintStyle.Fill,
                Color = SKColor.Parse("FFFFFFFF")
            };
        }
    }
}

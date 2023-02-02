using SkiaSharp;

using System;
using System.ComponentModel;

namespace Mageki.Drawables
{
    internal class Circles : Drawable
    {
        public Keyboard keyboard;

        public Circles(Keyboard keyboard)
        {
            this.keyboard = keyboard;
            keyboard.PropertyChanged += Keyboard_PropertyChanged;
        }
        public override void Update()
        {
            var n = BitConverter.GetBytes(keyboard.ShowLeft)[0] + BitConverter.GetBytes(keyboard.ShowRight)[0];
            if (n == 0) return;
            keyboard.Update();
            var size = new SKSize((keyboard.Size.Width - keyboard.Padding.X * 2 - keyboard.Spacing * (n / 2)) / n, keyboard.Size.Height - keyboard.Padding.Y * n);
            float baseRadius = size.Width / 2.8f;
            radius0 = baseRadius * 1.2f;
            radius1 = baseRadius * 1.0f;
            radius2 = baseRadius * 0.85f;
            radius3 = size.Width / 2 + keyboard.Padding.X;
            paint0.StrokeWidth = baseRadius / 10;
            paint1.StrokeWidth = baseRadius / 50;
            paint2.StrokeWidth = baseRadius / 15;
            paint3.StrokeWidth = baseRadius / 10;
            path1.Reset();
            path2.Reset();

            if (keyboard.ShowLeft)
            {
                lCenter = new SKPoint(keyboard.Left[1].BoundingBox.MidX, keyboard.Left[1].BoundingBox.Top + keyboard.Left[1].BoundingBox.Width / 2);
                var oval1 = new SKRect(lCenter.X - radius2, lCenter.Y - radius2, lCenter.X + radius2, lCenter.Y + radius2);
                path1.AddArc(oval1, 40, 100);
                path1.AddArc(oval1, -40, -100);
                var oval3 = new SKRect(lCenter.X - radius3, lCenter.Y - radius3, lCenter.X + radius3, lCenter.Y + radius3);
                path2.AddArc(oval3, 168, 24);
            }
            if (keyboard.ShowRight)
            {
                rCenter = new SKPoint(keyboard.Right[1].BoundingBox.MidX, keyboard.Right[1].BoundingBox.Top + keyboard.Right[1].BoundingBox.Width / 2);
                var oval2 = new SKRect(rCenter.X - radius2, rCenter.Y - radius2, rCenter.X + radius2, rCenter.Y + radius2);
                path1.AddArc(oval2, 40, 100);
                path1.AddArc(oval2, -40, -100);
                var oval4 = new SKRect(rCenter.X - radius3, rCenter.Y - radius3, rCenter.X + radius3, rCenter.Y + radius3);
                path2.AddArc(oval4, -12, 24);
            }
            base.Update();
        }

        private void Keyboard_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NeedUpdate = true;
        }

        private SKPaint paint0 = new SKPaint()
        {
            Style = SKPaintStyle.Stroke,
            Color = new SKColor(0xFFAAAAAA)
        };
        private SKPaint paint1 = new SKPaint()
        {
            Style = SKPaintStyle.Stroke,
            Color = new SKColor(0xFFAAAAAA)
        };
        private SKPaint paint2 = new SKPaint()
        {
            Style = SKPaintStyle.Stroke,
            StrokeCap = SKStrokeCap.Round,
            Color = new SKColor(0xFF888888)
        };
        private SKPaint paint3 = new SKPaint()
        {
            Style = SKPaintStyle.Stroke,
            StrokeCap = SKStrokeCap.Square,
            Color = new SKColor(0xFF888888)
        };
        private SKPath path1 = new SKPath();
        private SKPath path2 = new SKPath();
        private float radius0;
        private float radius1;
        private float radius2;
        private float radius3;
        SKPoint lCenter;
        SKPoint rCenter;

        public override void Draw(SKCanvas canvas)
        {
            if (!Visible || !keyboard.Visible) return;
            base.Draw(canvas);
            if (keyboard.ShowLeft)
            {
                canvas.DrawCircle(lCenter, radius0, paint0);
                canvas.DrawCircle(lCenter, radius1, paint1);
            }
            if (keyboard.ShowRight)
            {
                canvas.DrawCircle(rCenter, radius0, paint0);
                canvas.DrawCircle(rCenter, radius1, paint1);
            }
            canvas.DrawPath(path1, paint2);
            canvas.DrawPath(path2, paint3);
        }
    }
}

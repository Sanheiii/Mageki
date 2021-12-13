using SkiaSharp;

using System;
using System.Collections.Generic;
using System.Text;

namespace Mageki.Drawables
{
    internal class Circles : IDrawable
    {
        public SKPoint Center { get => center; set { center = value; propertyChanged = true; } }
        public float Radius { get => radius; set { radius = value; propertyChanged = true; } }

        public bool Visible { get;set; } = true;

        public bool DrawLeftArc = false;
        public bool DrawRightArc = false;

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
        private SKPoint center;
        private float radius;
        private float radius0;
        private float radius1;
        private float radius2;
        private float radius3;
        private bool propertyChanged;

        public void Draw(SKCanvas canvas)
        {
            if (!Visible) return;
            if (propertyChanged)
            {
                OnPropertyChanged();
            }
            canvas.DrawCircle(Center, radius0, paint0);
            canvas.DrawCircle(Center, radius1, paint1);
            canvas.DrawPath(path1, paint2);
            canvas.DrawPath(path2, paint3);
        }

        private void OnPropertyChanged()
        {
            radius0 = radius * 1.2f;
            paint0.StrokeWidth = Radius / 10;
            radius1 = radius * 1.0f;
            paint1.StrokeWidth = Radius / 50;
            radius2 = radius * 0.85f;
            paint2.StrokeWidth = Radius / 15;
            var oval1 = new SKRect(center.X - radius2, center.Y - radius2, center.X + radius2, center.Y + radius2);
            path1.Reset();
            path1.AddArc(oval1, 40, 100);
            path1.AddArc(oval1, -40, -100);
            radius3 = radius * 1.8f;
            paint3.StrokeWidth = Radius / 10;
            var oval2 = new SKRect(center.X - radius3, center.Y - radius3, center.X + radius3, center.Y + radius3);
            path2.Reset();
            if (DrawLeftArc) path2.AddArc(oval2, 168, 24);
            if (DrawRightArc) path2.AddArc(oval2, -12, 24);
        }
    }
}

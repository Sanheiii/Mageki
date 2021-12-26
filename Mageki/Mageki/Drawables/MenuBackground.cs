using SkiaSharp;

using System;
using System.Collections.Generic;
using System.Text;

namespace Mageki.Drawables
{
    internal class MenuBackground : IDrawable
    {
        public const float TextSizeCoef = 0.16f;
        private SKPoint center;
        public SKPoint Center { get => center; set { center = value; propertyChanged = true; } }

        private Side side;
        public Side Side { get => side; set { side = value; propertyChanged = true; } }

        private float width;
        public float Width { get => width; set { width = value; propertyChanged = true; } }

        private float height;
        public float Height { get => height; set { height = value; propertyChanged = true; } }

        public bool Visible { get;set; } = true;

        private bool propertyChanged = false;
        SKRect rect = new SKRect();
        public SKRect Rect => rect;


        SKPaint thickBorderPaint = new SKPaint()
        {
            Color = SKColors.White,
            Style = SKPaintStyle.StrokeAndFill,
        };
        SKPaint thinBorderPaint = new SKPaint()
        {
            Color = new SKColor(0xFF888888),
            Style = SKPaintStyle.Stroke,
        };
        SKPaint textPaint = new SKPaint()
        {
            Color = new SKColor(0xFF888888),
            TextAlign = SKTextAlign.Center,
            FakeBoldText = true,
            Style = SKPaintStyle.StrokeAndFill,
        };
        SKPaint textBoundsPaint = new SKPaint()
        {
            Color = SKColors.White,
            Style = SKPaintStyle.StrokeAndFill,
        };
        public void Draw(SKCanvas canvas)
        {
            if (!Visible) return;
            if (propertyChanged) OnPropertyChanged();
            canvas.DrawRect(rect, thickBorderPaint);
            canvas.DrawRoundRect(rect, width * Button.CornerCoef, height * Button.CornerCoef, thinBorderPaint);
            string text = string.Empty;
            if (side == Side.Left) text = "L-MENU";
            else if (side == Side.Right) text = "R-MENU";
            if (text != string.Empty)
            {
                SKRect bounds = default;
                textPaint.MeasureText(text, ref bounds);
                float textHeight=bounds.Height;
                bounds.Top = 0;
                bounds.Bottom = thinBorderPaint.StrokeWidth;
                bounds.Inflate(bounds.Height, 0);
                bounds.Location = new SKPoint(center.X - bounds.Width / 2, rect.Bottom - bounds.Height / 2);
                canvas.DrawRect(bounds, textBoundsPaint);
                canvas.DrawText(text, center.X, rect.Bottom + textHeight / 2, textPaint);
            }
        }

        private void OnPropertyChanged()
        {
            thickBorderPaint.StrokeWidth = width * Button.StrokeWidthCoef * 5;
            thinBorderPaint.StrokeWidth = width * Button.StrokeWidthCoef;
            rect = new SKRect(0, 0, width, height) { Location = new SKPoint(center.X - width / 2, center.Y - height / 2) };
            textPaint.TextSize = height * TextSizeCoef;
            textPaint.StrokeWidth = textPaint.TextSize / 15;
            //path.Reset();
            //path.AddRoundRect)
        }
    }
    public enum Side { Left, Right }
}

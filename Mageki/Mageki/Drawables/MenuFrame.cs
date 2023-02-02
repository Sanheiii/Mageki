using SkiaSharp;

using System.ComponentModel;

namespace Mageki.Drawables
{
    internal class MenuFrame : Drawable
    {
        public const float textSizeCoef = 0.16f;
        public const float menuFrameSizeCoef = 1.6f;

        public Side Side { get => GetValue(default(Side)); set => SetValueWithNotify(value); }
        public SKRect BoundingBox => boundingBox;

        SquareButton menu;
        SKRect boundingBox;


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

        public MenuFrame(SquareButton menu, Side side)
        {
            this.menu = menu;
            this.Side = side;
            menu.PropertyChanged += Menu_PropertyChanged;
        }

        private void Menu_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(menu.Size) ||
                e.PropertyName == nameof(menu.Padding) ||
                e.PropertyName == nameof(menu.Position))
            {
                NeedUpdate = true;
            }
        }

        public override void Update()
        {
            SKRect buttonBoundingBox = menu.BoundingBox;
            float frameSideLength = menu.BoundingBox.Width * menuFrameSizeCoef;
            boundingBox = new SKRect(buttonBoundingBox.MidX - frameSideLength / 2,
                buttonBoundingBox.MidY - frameSideLength / 2,
                buttonBoundingBox.MidX + frameSideLength / 2,
                buttonBoundingBox.MidY + frameSideLength / 2);

            thickBorderPaint.StrokeWidth = frameSideLength * menu.BorderWidthRatio * 5;
            thinBorderPaint.StrokeWidth = frameSideLength * menu.BorderWidthRatio;
            textPaint.TextSize = frameSideLength * textSizeCoef;
            textPaint.StrokeWidth = textPaint.TextSize / 15;
            base.Update();
        }

        public override void Draw(SKCanvas canvas)
        {
            if (!Visible || !menu.Visible) return;
            base.Draw(canvas);
            canvas.DrawRoundRect(boundingBox, boundingBox.Width * menu.CornerRatio, boundingBox.Height * menu.CornerRatio, thickBorderPaint);
            canvas.DrawRoundRect(boundingBox, boundingBox.Width * menu.CornerRatio, boundingBox.Height * menu.CornerRatio, thinBorderPaint);
            string text = string.Empty;
            if (Side == Side.Left) text = "L-MENU";
            else if (Side == Side.Right) text = "R-MENU";
            if (text != string.Empty)
            {
                SKRect bounds = default;
                textPaint.MeasureText(text, ref bounds);
                float textHeight = bounds.Height;
                bounds.Top = 0;
                bounds.Bottom = thinBorderPaint.StrokeWidth;
                bounds.Inflate(bounds.Height, 0);
                bounds.Location = new SKPoint(boundingBox.MidX - bounds.Width / 2, boundingBox.Bottom - bounds.Height / 2);
                canvas.DrawRect(bounds, textBoundsPaint);
                canvas.DrawText(text, boundingBox.MidX, boundingBox.Bottom + textHeight / 2, textPaint);
            }
        }
    }
}

using SkiaSharp;

using System;
using System.Collections.Generic;
using System.Text;

namespace Mageki.Drawables
{
    class Button:IDrawable
    {
        public static Dictionary<ButtonColors, SKColor> Colors { get; } = new Dictionary<ButtonColors, SKColor>()
        {
            { ButtonColors.Red, new SKColor(0xFFFF455B) },
            { ButtonColors.Green, new SKColor(0xFF45FF75) },
            { ButtonColors.Blue, new SKColor(0xFF4589FF) },
            { ButtonColors.Yellow, new SKColor(0xFFFFD545) },
            { ButtonColors.Cyan, new SKColor(0xFF45F8FF) },
            { ButtonColors.Purple, new SKColor(0xFF8B45FF) },
            { ButtonColors.Blank, new SKColor(0xFFDDDDDD) },
            { ButtonColors.White, new SKColor(0xFFFFFFFF) },
        };

        private SKPoint center;
        public SKPoint Center { get => center; set { center = value; propertyChanged = true; } }

        private float width;
        public float Width { get => width; set { width = value; propertyChanged = true; } }

        private float height;
        public float Height { get => height; set { height = value; propertyChanged = true; } }

        private SKRect borderRect;
        private SKRect buttonRect;
        public SKRect BorderRect
        {
            get => borderRect;
            private set
            {
                var strokeWidth = value.Width * StrokeWidthCoef;
                borderRect = value;
                buttonRect = SKRect.Inflate(value, -strokeWidth / 2, -strokeWidth / 2);
                backPaint.StrokeWidth = strokeWidth * 2;
                Color = baseColor;
            }
        }

        private ButtonColors baseColor;
        public ButtonColors Color
        {
            get => baseColor;
            set
            {
                baseColor = value;
                var color1 = Colors[value];
                SKColor color;
                color = new SKColor((byte)(color1.Red * colorSaturation + 255 * (1 - colorSaturation)), (byte)(color1.Green * colorSaturation + 255 * (1 - colorSaturation)), (byte)(color1.Blue * colorSaturation + 255 * (1 - colorSaturation)));
                paint.Shader = SKShader.CreateRadialGradient(new SKPoint(BorderRect.MidX, BorderRect.MidY), BorderRect.Width, new SKColor[] { color, color1 }, SKShaderTileMode.Mirror);
            }
        }

        public SKColor BorderColor { set => backPaint.Color = value; }

        public bool IsHold { get; set; }
        public bool Visible { get; set; } = true;

        const double colorSaturation = 0.6;
        public const float StrokeWidthCoef = 0.05f;
        public const float CornerCoef = 0.05f;
        private bool propertyChanged = false;

        private SKPaint paint = new SKPaint()
        {
            Style = SKPaintStyle.Fill,
        };
        private SKPaint holdMaskPaint = new SKPaint()
        {
            Style = SKPaintStyle.Fill,
            Color = new SKColor(0x22000000)
        };
        private SKPaint backPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = new SKColor(0xFF222222)
        };



        public Button()
        {
            Color = ButtonColors.Blank;
        }


        public void Draw(SKCanvas canvas)
        {
            if (!Visible) return;
            if (propertyChanged) OnPropertyChanged();
            canvas.DrawRoundRect(borderRect, new SKSize(BorderRect.Width * CornerCoef, BorderRect.Height * CornerCoef), backPaint);
            canvas.DrawRoundRect(buttonRect, new SKSize(buttonRect.Width * CornerCoef, buttonRect.Height * CornerCoef), paint);
            if (IsHold)
            {
                canvas.DrawRoundRect(buttonRect, new SKSize(BorderRect.Width * CornerCoef, BorderRect.Height * CornerCoef), holdMaskPaint);
            }
        }

        private void OnPropertyChanged()
        {
            BorderRect = new SKRect(center.X - width / 2, center.Y - height / 2, center.X + width / 2, center.Y + height / 2);
        }
    }
    enum ButtonColors
    {
        Red = 0b100,
        Green = 0b010,
        Blue = 0b001,
        Yellow = 0b110,
        Cyan = 0b011,
        Purple = 0b101,
        White = 0b111,
        Blank = 0b000,
    }
}

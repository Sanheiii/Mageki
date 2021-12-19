using SkiaSharp;

using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Essentials;

namespace Mageki.Drawables
{
    public class Button : IButton
    {


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
                var strokeWidth = value.Height * StrokeWidthCoef;
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
                var color1 = IButton.Colors[value];
                SKColor color;
                color = new SKColor((byte)(color1.Red * colorSaturation + 255 * (1 - colorSaturation)), (byte)(color1.Green * colorSaturation + 255 * (1 - colorSaturation)), (byte)(color1.Blue * colorSaturation + 255 * (1 - colorSaturation)));
                paint.Shader = SKShader.CreateRadialGradient(new SKPoint(BorderRect.MidX, BorderRect.MidY), MathF.Max(BorderRect.Height, BorderRect.Width), new SKColor[] { color, color1 }, SKShaderTileMode.Mirror);
            }
        }

        public SKColor BorderColor { set => backPaint.Color = value; }

        public bool IsHold
        {
            get => isHold;
            set
            {
                isHold = value;
                if (Settings.HapticFeedback) HapticFeedback.Perform(HapticFeedbackType.Click);
            }
        }
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
            Color = new SKColor(0x66000000)
        };
        private SKPaint backPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = new SKColor(0xFF222222)
        };
        private bool isHold;

        public Button()
        {
            Color = ButtonColors.Blank;
        }


        public void Draw(SKCanvas canvas)
        {
            if (!Visible) return;
            if (propertyChanged) OnPropertyChanged();
            canvas.DrawRoundRect(borderRect, new SKSize(BorderRect.Height * CornerCoef, BorderRect.Height * CornerCoef), backPaint);
            canvas.DrawRoundRect(buttonRect, new SKSize(buttonRect.Height * CornerCoef, buttonRect.Height * CornerCoef), paint);
            if (IsHold)
            {
                canvas.DrawRoundRect(buttonRect, new SKSize(BorderRect.Height * CornerCoef, BorderRect.Height * CornerCoef), holdMaskPaint);
            }
        }

        private void OnPropertyChanged()
        {
            BorderRect = new SKRect(center.X - width / 2, center.Y - height / 2, center.X + width / 2, center.Y + height / 2);
        }
    }
    public enum ButtonColors
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

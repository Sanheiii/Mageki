using SkiaSharp;

using System;

namespace Mageki.Drawables
{
    public class SquareButton : ButtonBase
    {
        public const float colorSaturation = 0.6f;
        public ButtonColors Color { get => GetValue(ButtonColors.Blank); set => SetValueWithNotify(value); }
        public float CornerRatio { get => GetValue(0.10f); set => SetValueWithNotify(value); }
        public float BorderWidthRatio { get => GetValue(0.07f); set => SetValueWithNotify(value); }

        SKPath borderPath = new SKPath();
        SKPath buttonPath = new SKPath();

        public SKColor BorderColor
        {
            get => borderPaint.Color;
            set
            {
                base.NotifyChanging(nameof(BorderColor));
                borderPaint.Color = value;
                base.NotifyChanged(nameof(BorderColor));
            }
        }
        private SKPaint buttonPaint = new SKPaint()
        {
            Style = SKPaintStyle.Fill,
        };
        private SKPaint buttonLightPaint = new SKPaint()
        {
            Style = SKPaintStyle.Fill,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Solid, 20)
        };
        private SKPaint borderPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = new SKColor(0xFF222222)
        };
        private SKPaint holdMaskPaint = new SKPaint()
        {
            Style = SKPaintStyle.Fill,
            Color = new SKColor(0x66000000),
            //MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Solid, 10)
        };

        public SquareButton()
        {
            Color = ButtonColors.Blank;
        }
        public override void Update()
        {
            float sideLength = Size.Width - Padding.X * 2;
            var strokeWidth = BorderWidthRatio * sideLength;
            SKRect borderRect = SKRect.Create(Position.X + Size.Width / 2 - sideLength / 2,
                Position.Y + Padding.Y,
                sideLength,
                sideLength);
            SKRect buttonRect = SKRect.Inflate(borderRect, -strokeWidth, -strokeWidth);
            float borderCorner = borderRect.Height * CornerRatio;
            float buttonCorner = buttonRect.Height * CornerRatio;

            borderPath.Reset();
            buttonPath.Reset();

            borderPath.FillType = SKPathFillType.EvenOdd;
            borderPath.AddRoundRect(borderRect, borderCorner, borderCorner);
            borderPath.AddRoundRect(buttonRect, buttonCorner, buttonCorner);
            buttonPath.AddRoundRect(buttonRect, buttonCorner, buttonCorner);

            SKColor color1 = Colors[Color];
            SKColor color2 = new SKColor((byte)(color1.Red * colorSaturation + 255 * (1 - colorSaturation)),
                (byte)(color1.Green * colorSaturation + 255 * (1 - colorSaturation)),
                (byte)(color1.Blue * colorSaturation + 255 * (1 - colorSaturation)));
            buttonPaint.Color= color1;
            buttonPaint.Shader = SKShader.CreateRadialGradient(new SKPoint(borderRect.MidX, borderRect.MidY),
                MathF.Max(borderRect.Height, borderRect.Width),
                new SKColor[] { color2, color1 },
                SKShaderTileMode.Mirror);
            buttonLightPaint.Shader = buttonPaint.Shader;

            base.Update();
        }

        public override void Draw(SKCanvas canvas)
        {
            if (!Visible) return;
            base.Draw(canvas);
            canvas.DrawPath(borderPath, borderPaint);
            canvas.DrawPath(buttonPath, buttonPaint);
            if (TouchCount > 0)
            {
                canvas.DrawPath(buttonPath, holdMaskPaint);
            }
        }
    }
}

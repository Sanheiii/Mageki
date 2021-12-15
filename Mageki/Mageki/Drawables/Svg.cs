using SkiaSharp;

using System.IO;

using SKSvg = SkiaSharp.Extended.Svg.SKSvg;

namespace Mageki.Drawables
{
    internal class Svg : IDrawable
    {
        public SKPoint Center { get; set; }

        public float MaxWidth { get; set; }

        public float MaxHeight { get; set; }

        public SKRect Rect => GetTransform().MapRect(svg.ViewBox);

        public bool Visible { get; set; } = true;

        SKSvg svg;

        public Svg(Stream stream)
        {
            svg = new SKSvg();
            svg.Load(stream);
        }

        public void Draw(SKCanvas canvas)
        {
            if (!Visible) return;
            SKMatrix matrix = GetTransform();
            canvas.DrawPicture(svg.Picture, ref matrix, null);
        }
        public SKMatrix GetTransform()
        {
            float xRatio = MaxWidth / svg.ViewBox.Width;
            float yRatio = MaxHeight / svg.ViewBox.Height;
            float ratio = xRatio < yRatio ? xRatio : yRatio;
            return SKMatrix.CreateScaleTranslation(ratio, ratio, Center.X - svg.ViewBox.Width * ratio / 2, Center.Y);
        }

        public static Svg FromResource(string resourceName)
        {
            using (Stream stream = typeof(Svg).Assembly.GetManifestResourceStream(resourceName))
            {
                return new Svg(stream);
            }
        }
    }
}

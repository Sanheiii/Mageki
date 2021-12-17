﻿using SkiaSharp;

using System;
using System.Collections.Generic;
using System.Text;

namespace Mageki.Drawables
{
    public class Logo : IDrawable
    {
        public SKPoint Center { get => center; set { center = value; propertyChanged = true; } }

        public float MaxWidth { get => maxWidth; set { maxWidth = value; propertyChanged = true; } }

        public float MaxHeight { get => maxHeight; set { maxHeight = value; propertyChanged = true; } }

        public SKRect Rect => GetTransform().MapRect(new SKRect(0, 0, viewBoxWidth, viewBoxHeight));

        public SKColor Color { get => paint.Color; set => paint.Color = value; }

        public SKPath[] Paths { get; private set; }

        public bool Visible { get; set; } = true;

        private bool propertyChanged = false;

        private SKPaint paint = new SKPaint
        {
            Color = SKColors.LightGray,
            Style = SKPaintStyle.Fill
        };

        public Logo()
        {
            OnPropertyChanged();
        }

        public void Draw(SKCanvas canvas)
        {
            if (!Visible) return;
            if (propertyChanged) OnPropertyChanged();
            foreach (var path in Paths)
            {
                canvas.DrawPath(path, paint);
            }
        }
        const float viewBoxWidth = 1257f;
        const float viewBoxHeight = 522f;
        private SKPoint center;
        private float maxWidth;
        private float maxHeight;

        public SKMatrix GetTransform()
        {
            float xRatio = MaxWidth / viewBoxWidth;
            float yRatio = MaxHeight / viewBoxHeight;
            float ratio = xRatio < yRatio ? xRatio : yRatio;
            return SKMatrix.CreateScaleTranslation(ratio, ratio, Center.X - viewBoxWidth * ratio / 2, Center.Y);
        }
        private void OnPropertyChanged()
        {
            CreatePaths();
            propertyChanged = false;
        }
        private void CreatePaths()
        {
            SKPath[] paths = new SKPath[4];
            paths[0] = SKPath.ParseSvgPathData("M358,30l-57,99-180-1,3-6H296l4-6H104L73,167H271L0,346H61L251,221,129,431h43L326,167H467l23-38H346l59-99H358Zm6,85-5,8H494l5-8H364Z");
            paths[1] = SKPath.ParseSvgPathData("M451,272H343l30-49H480l-3,5H394l-4,7,82,1Zm105-62L482,338l-129,1,4-8H474l4-6H337l-29,50H504l96-165H556ZM673,413m1-1H222l-8,12H665ZM211,429l-4,6H663l-2-6H211Z");
            paths[2] = SKPath.ParseSvgPathData("M815,19H770L574,359h44l81-139H810L636,522h41L854,220H957l24-40H735l3-6H984l4-6H728ZM955,62l-51,87H875s-14.87-2.343-4-15c0.1-.115,32-56,32-56s4.673-11.075,11-15c0.14-.087,11-1,11-1s33.871-58.876,91-62c9.48-.518,64.3-5.315,46,53-0.33,1.048-3.53,6.391-3,9,0.24,1.207,2.36,1,4,1,1.74,0.005,7.54,4.4,4,10-6.59,10.411-44,76-44,76H980l50-87h17l6-11s12.86-29.143-6-37a67.819,67.819,0,0,0-24-5c-8.6-.13-18.18,2.029-27,5-22,7.407-40,25-40,25a139.741,139.741,0,0,0-21,23C934.821,62.244,955,62,955,62ZM898,76l-35,59s-7.959-1.449-5-8c0.08-.177,29-49,29-49s3.539-6.24,9-6C898.916,72.128,898.167,75,898,76Zm137.31,61.5,35-59s7.96,1.449,5,8c-0.08.177-29,49-29,49s-3.54,6.24-9,6C1034.4,141.372,1035.15,138.5,1035.31,137.5Z");
            paths[3] = SKPath.ParseSvgPathData("M1195,119h-45l-36,62h-63l3-6h52l4-6h-78l-29,51h89l-32,55H997l4-6h51l4-6H978l-30,51h89l-56,98H764l-8,12h351l10-12h-92l57-98h90l24-39h-92l32-55h91l23-39h-92Zm-73,144h81l-4,6h-80ZM749,435l3-5h350l2,5H749Zm505-260,3-6h-82l-4,6h83Z");
            SKMatrix matrix = GetTransform();
            foreach (var path in paths)
            {
                path.Transform(matrix);
            }
            Paths = paths;
        }
    }
}

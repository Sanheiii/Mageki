using SkiaSharp;

using System;
using System.Collections.Generic;
using System.Text;

namespace Mageki.Drawables
{
    public interface IDrawable
    {
        bool Visible { get; set; }
        void Draw(SKCanvas canvas);
    }
}

using SkiaSharp;

using System;
using System.Text;

using Microsoft.Maui.Controls;

namespace Mageki.Drawables
{
    public interface IDrawable
    {
        bool Visible { get; set; }
        void Update();
        void Draw(SKCanvas canvas);
    }
}

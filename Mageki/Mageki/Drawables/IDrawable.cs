using SkiaSharp;

using System;
using System.Text;

using Xamarin.Forms;

namespace Mageki.Drawables
{
    public interface IDrawable
    {
        bool Visible { get; set; }
        void Update();
        void Draw(SKCanvas canvas);
    }
}

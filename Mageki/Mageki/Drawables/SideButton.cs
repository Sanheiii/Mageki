using SkiaSharp;

using System;
using System.Collections.Generic;
using System.Text;

namespace Mageki.Drawables
{
    public class SideButton : IButton
    {
        public bool IsHold { get; set; }
        public bool Visible { get; set; } = true;

        public void Draw(SKCanvas canvas)
        {
            if (!Visible) return;
        }
    }
}

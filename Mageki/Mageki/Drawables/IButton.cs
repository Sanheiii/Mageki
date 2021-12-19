using SkiaSharp;

using System;
using System.Collections.Generic;
using System.Text;

namespace Mageki.Drawables
{
    public interface IButton:IDrawable
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
        bool IsHold { get; set; }
    }
}

using SkiaSharp;

using System.Collections.Generic;

namespace Mageki.Drawables
{
    public abstract class ButtonBase : Box
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

        public byte TouchCount { get => GetValue((byte)0); set => SetValueWithNotify(value); }

        public ButtonBase() : base() { }

        public override bool HandleTouchPressed(long id, SKPoint point)
        {
            touchPoints.Add(id, point);
            TouchCount++;
            return base.HandleTouchPressed(id, point);
        }

        public override bool HandleTouchMoved(long id, SKPoint point)
        {
            return base.HandleTouchMoved(id, point);
        }

        public override bool HandleTouchReleased(long id)
        {
            if (touchPoints.ContainsKey(id))
            {
                TouchCount--;
            }
            return base.HandleTouchReleased(id);
        }
    }
}

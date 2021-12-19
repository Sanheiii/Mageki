using SkiaSharp;

using System;
using System.Collections.Generic;
using System.Text;

namespace Mageki.Drawables
{
    public class MenuButton : IButton
    {
        const float backgroundSizeRatio = 1.6f;
        private Button button = new Button();
        MenuBackground background = new MenuBackground();
        public bool Visible { get; set; } = true;
        public SKPoint Center
        {
            get => button.Center;
            set
            {
                button.Center = value;
                background.Center = value;
            }
        }

        public float Width
        {
            get => button.Width;
            set
            {
                button.Width = value;
                background.Width = value * backgroundSizeRatio;
            }
        }

        public float Height
        {
            get => button.Height;
            set
            {
                button.Height = value;
                background.Height = value * backgroundSizeRatio;
            }
        }

        public Side Side
        {
            get => background.Side;
            set
            {
                background.Side = value;
                if (Side == Side.Left)
                    button.Color = ButtonColors.Red;
                else if (Side == Side.Right)
                    button.Color = ButtonColors.Yellow;
            }
        }

        public bool IsHold { get => button.IsHold; set => button.IsHold = value; }
        public ButtonColors Color { get => button.Color; set => button.Color = value; }
        public SKColor BorderColor { set => button.BorderColor = value; }
        public SKRect BorderRect => button.BorderRect;

        public void Draw(SKCanvas canvas)
        {
            if (!Visible) return;
            background.Draw(canvas);
            button.Draw(canvas);
        }
    }
}

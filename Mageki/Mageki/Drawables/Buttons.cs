using SkiaSharp;

using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms.Internals;

namespace Mageki.Drawables
{
    public class ButtonCollection
    {
        public IButton[] Buttons { get; } = new IButton[13]
        {
            new Button(),
            new Button(),
            new Button(),
            new SideButton(),
            new MenuButton(){ Side=Side.Left},
            new Button(),
            new Button(),
            new Button(),
            new SideButton(),
            new MenuButton(){ Side=Side.Right},
            new Button(),
            new Button(),
            new Button(),
        };
        public Button L1 => Buttons[0] as Button;
        public Button L2 => Buttons[1] as Button;
        public Button L3 => Buttons[2] as Button;
        public SideButton LSide => Buttons[3] as SideButton;
        public MenuButton LMenu => Buttons[4] as MenuButton;
        public Button R1 => Buttons[5] as Button;
        public Button R2 => Buttons[6] as Button;
        public Button R3 => Buttons[7] as Button;
        public SideButton RSide => Buttons[8] as SideButton;
        public MenuButton RMenu => Buttons[9] as MenuButton;

        public IButton this[int i]
        {
            get { return Buttons[i]; }
        }
        public IButton[] this[Range r]
        {
            get { return Buttons[r]; }
        }

        public void Draw(SKCanvas canvas)
        {
            LSide.Draw(canvas);
            RSide.Draw(canvas);
            LMenu.Draw(canvas);
            RMenu.Draw(canvas);

            L1.Draw(canvas);
            L2.Draw(canvas);
            L3.Draw(canvas);
            R1.Draw(canvas);
            R2.Draw(canvas);
            R3.Draw(canvas);
        }
    }
}

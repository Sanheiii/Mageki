using SkiaSharp;

using System.Collections.Generic;

namespace Mageki.Drawables
{
    public abstract class Container : Box
    {
        public IList<IDrawable> Children { get => GetValue(default(IList<IDrawable>)); set => SetValueWithNotify(value); }

        public Container() : base() { }

        public override void Update()
        {
            base.Update();
            foreach (var child in Children)
            {
                child.Update();
            }
        }

        public override void Draw(SKCanvas canvas)
        {
            if (!Visible) return;
            base.Draw(canvas);
            foreach (var child in Children)
            {
                child.Draw(canvas);
            }
        }
    }
}

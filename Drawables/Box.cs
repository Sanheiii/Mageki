using Mageki.TouchTracking;

using SkiaSharp;

namespace Mageki.Drawables
{
    public abstract class Box : TouchableObject
    {
        public SKPoint Position { get => GetValue(default(SKPoint)); set => SetValueWithNotify(value); }
        public SKSize Size { get => GetValue(default(SKSize)); set => SetValueWithNotify(value); }
        public SKPoint Padding { get => GetValue(default(SKPoint)); set => SetValueWithNotify(value); }
        public SKRect BoundingBox => new SKRect(Position.X, Position.Y, Position.X + Size.Width, Position.Y + Size.Height);

        public Box() : base() { }

        public override bool HitTest(SKPoint point)
        {
            return BoundingBox.Contains(point);
        }
    }
}

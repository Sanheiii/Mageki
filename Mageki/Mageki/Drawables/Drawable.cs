using SkiaSharp;

using System.ComponentModel;

namespace Mageki.Drawables
{
    public abstract class Drawable : NotifyingEntity, IDrawable
    {
        public bool Visible { get; set; } = true;
        public bool NeedUpdate { get; set; }

        public Drawable()
        {
            PropertyChanged += OnPropertyChanged;
        }

        protected virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            NeedUpdate = true;
        }

        public virtual void Update()
        {
            NeedUpdate = false;
        }
        public virtual void Draw(SKCanvas canvas)
        {
            if (NeedUpdate) Update();
        }
    }
}

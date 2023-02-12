using SkiaSharp;

using System.Collections.Generic;

using Xamarin.Essentials;

namespace Mageki.Drawables
{
    public abstract class TouchableObject : Drawable
    {

        protected Dictionary<long, SKPoint> touchPoints = new Dictionary<long, SKPoint>();
        public abstract bool HitTest(SKPoint point);

        public virtual bool HandleTouchPressed(long id, SKPoint point)
        {
            return touchPoints.ContainsKey(id);
        }

        public virtual bool HandleTouchMoved(long id, SKPoint point)
        {
            if (touchPoints.ContainsKey(id))
            {
                touchPoints[id] = point;
                return true;
            }
            else
            {
                return false;
            }
        }

        public virtual void HandleTouchReleased(long id)
        {
            if (touchPoints.ContainsKey(id))
            {
                touchPoints.Remove(id);
            }
        }

        public virtual void HandleTouchCancelled(long id)
        {
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                foreach (var key in touchPoints.Keys)
                {
                    HandleTouchReleased(key);
                }
            }
            else
            {
                HandleTouchReleased(id);
            }
        }
    }
}

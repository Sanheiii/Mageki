using SkiaSharp;

using System;
using System.Collections.Generic;
using System.Text;

namespace Mageki.Utils
{
    public class Math
    {
        public static SKPoint GetFootPoint(SKPoint linePoint1, SKPoint linePoint2, SKPoint point)
        {
            float a = linePoint2.Y - linePoint1.Y;
            float b = linePoint1.X - linePoint2.X;
            float c = linePoint2.X * linePoint1.Y - linePoint1.X * linePoint2.Y;
            float x = (b * b * point.X - a * b * point.Y - a * c) / (a * a + b * b);
            float y = (-a * b * point.X + a * a * point.Y - b * c) / (a * a + b * b);
            return new SKPoint(x, y);
        }
    }
}

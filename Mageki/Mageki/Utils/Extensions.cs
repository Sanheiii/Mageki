using SkiaSharp;

using System.Linq;
using System.Numerics;

namespace Mageki.Utils
{
    public static class Extensions
    {
        public static byte[] ToBcd(this BigInteger value)
        {
            var length = value.ToString().Length / 2 + value.ToString().Length % 2;
            byte[] ret = new byte[length];
            for (int i = length - 1; i >= 0; i--)
            {
                ret[i] = (byte)(value % 10);
                value /= 10;
                ret[i] |= (byte)((value % 10) << 4);
                value /= 10;
            }
            return ret;
        }
        public static SKColor Standardization(this SKColor value)
        {
            var y = ((value.Red * 299) + (value.Green * 587) + (value.Blue * 114)) / 1000f / 255f;
            float ratio = 1;
            if (y != 0)
            {
                ratio = 255f / new byte[] { value.Red, value.Green, value.Blue }.Max();
            }
            return new SKColor((byte)(value.Red * ratio), (byte)(value.Green * ratio), (byte)(value.Blue * ratio), (byte)(value.Alpha * y));
        }
    }
}

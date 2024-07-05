using Microsoft.Xna.Framework;
using xTile.Dimensions;

namespace PredictorPatchFramework.Extentions
{
    public static class Vector2Extentions
    {
        public static Location ToLocation(this Vector2 vector)
        {
            return new Location((int)vector.X, (int)vector.Y);
        }
        public static Point ToPoint(this Vector2 vector)
        {
            return new Point((int)vector.X, (int)vector.Y);
        }

        public static double Angle(this Vector2 a, Vector2 b)
        {
            return Math.Atan2(b.Y - a.Y, b.X - a.X);
        }
    }
}

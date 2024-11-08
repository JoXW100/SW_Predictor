using Microsoft.Xna.Framework;
using xTile.Dimensions;

namespace PredictorPatchFramework.Extensions
{
    public static class LocationExtensions
    {
        public static Vector2 ToVector2(this Location location)
        {
            return new Vector2(location.X, location.Y);
        }
    }
}

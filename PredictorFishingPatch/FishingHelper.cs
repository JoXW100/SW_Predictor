using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Tools;

namespace PredictorFishingPatch
{
    internal static class FishingHelper
    {
        public static double GetBaitPotensy(StardewValley.Object? bait)
        {
            return bait is not null ? bait.Price / 10d : 0d;
        }

        public static bool TryFindFishingLocation(Farmer who, FishingRod? rod, out Vector2 fishingLocation)
        {
            fishingLocation = -Vector2.One;
            if (rod != null && rod.isFishing)
            {
                var bobberPosition = rod.bobber.Value / 64f;
                int x = (int)bobberPosition.X;
                int y = (int)bobberPosition.Y;

                if (who.currentLocation.isTileFishable(x, y) && !who.currentLocation.isTileBuildingFishable(x, y))
                {
                    fishingLocation = bobberPosition;
                    return true;
                }
            }

            if (fishingLocation.X < 0)
            {
                const int MaxRange = 8;
                int searchStartX = (int)who.Position.X - MaxRange;
                int searchStartY = (int)who.position.Y - MaxRange;
                int searchEndX = (int)who.Position.X + MaxRange;
                int searchEndY = (int)who.Position.Y + MaxRange;

                // Change search order to start in facing direction
                switch (who.FacingDirection)
                {
                    case 0: // Up
                        for (int y = searchStartY; y < searchEndY; y++)
                        {
                            for (int x = searchStartX; x < searchEndX; x++)
                            {
                                if (who.currentLocation.isTileFishable(x, y) && !who.currentLocation.isTileBuildingFishable(x, y))
                                {
                                    fishingLocation = new(x, y);
                                    return true;
                                }
                            }
                        }
                        break;
                    case 1: // Right
                        for (int x = searchEndX; x < searchStartX; x--)
                        {
                            for (int y = searchStartY; y < searchEndY; y++)
                            {
                                if (who.currentLocation.isTileFishable(x, y) && !who.currentLocation.isTileBuildingFishable(x, y))
                                {
                                    fishingLocation = new(x, y);
                                    return true;
                                }
                            }
                        }
                        break;
                    case 2: // Down
                        for (int y = searchEndY; y > searchStartY; y--)
                        {
                            for (int x = searchStartX; x < searchEndX; x++)
                            {
                                if (who.currentLocation.isTileFishable(x, y) && !who.currentLocation.isTileBuildingFishable(x, y))
                                {
                                    fishingLocation = new(x, y);
                                    return true;
                                }
                            }
                        }
                        break;
                    case 3: // Left
                        for (int x = searchStartX; x < searchEndX; x++)
                        {
                            for (int y = searchStartY; y < searchEndY; y++)
                            {
                                if (who.currentLocation.isTileFishable(x, y) && !who.currentLocation.isTileBuildingFishable(x, y))
                                {
                                    fishingLocation = new(x, y);
                                    return true;
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            return false;
        }
    }
}

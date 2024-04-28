using StardewValley;
using StardewValley.Extensions;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace Predictor.Framework.Extentions
{
    internal static class ObjectExtentions
    {
        public static bool IsBreakableContainer(this Object obj)
        {
            return obj is BreakableContainer || (!obj.isTemporarilyInvisible && obj.name.Contains("SupplyCrate"));
        }

        public static bool Predict_performToolAction(this Object obj, PredictionContext _ctx, Tool? t = null)
        {
            GameLocation location = obj.Location;
            if (obj.isTemporarilyInvisible)
            {
                return false;
            }
            // ...
            if (obj.name.Contains("SupplyCrate") && (t is null || t.isHeavyHitter()))
            {
                Random random = Utility.CreateRandom(Game1.uniqueIDForThisGame, obj.TileLocation.X * 777.0, obj.TileLocation.Y * 7.0);
                int houseUpgradeLevel = (t?.getLastFarmerToUse() ?? Game1.player).HouseUpgradeLevel;
                int xTile = (int)obj.TileLocation.X;
                int yTile = (int)obj.TileLocation.Y;

                switch (houseUpgradeLevel)
                {
                    case 0:
                        switch (random.Next(7))
                        {
                            case 0:
                                Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)770", xTile, yTile, random.Next(3, 6), location);
                                break;
                            case 1:
                                Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)371", xTile, yTile, random.Next(5, 8), location);
                                break;
                            case 2:
                                Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)535", xTile, yTile, random.Next(2, 5), location);
                                break;
                            case 3:
                                Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)241", xTile, yTile, random.Next(1, 3), location);
                                break;
                            case 4:
                                Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)395", xTile, yTile, random.Next(1, 3), location);
                                break;
                            case 5:
                                Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)286", xTile, yTile, random.Next(3, 6), location);
                                break;
                            default:
                                Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)286", xTile, yTile, random.Next(3, 6), location);
                                break;
                        }

                        break;
                    case 1:
                        switch (random.Next(10))
                        {
                            case 0:
                                Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)770", xTile, yTile, random.Next(3, 6), location);
                                break;
                            case 1:
                                Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)371", xTile, yTile, random.Next(5, 8), location);
                                break;
                            case 2:
                                Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)749", xTile, yTile, random.Next(2, 5), location);
                                break;
                            case 3:
                                Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)253", xTile, yTile, random.Next(1, 3), location);
                                break;
                            case 4:
                                Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)237", xTile, yTile, random.Next(1, 3), location);
                                break;
                            case 5:
                                Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)246", xTile, yTile, random.Next(4, 8), location);
                                break;
                            case 6:
                                Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)247", xTile, yTile, random.Next(2, 5), location);
                                break;
                            case 7:
                                Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)245", xTile, yTile, random.Next(4, 8), location);
                                break;
                            case 8:
                                Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)287", xTile, yTile, random.Next(3, 6), location);
                                break;
                            default:
                                Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "MixedFlowerSeeds", xTile, yTile, random.Next(4, 6), location);
                                break;
                        }

                        break;
                    default:
                        switch (random.Next(9))
                        {
                            case 0:
                                Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)770", xTile, yTile, random.Next(3, 6), location);
                                break;
                            case 1:
                                Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)920", xTile, yTile, random.Next(5, 8), location);
                                break;
                            case 2:
                                Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)749", xTile, yTile, random.Next(2, 5), location);
                                break;
                            case 3:
                                Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)253", xTile, yTile, random.Next(2, 4), location);
                                break;
                            case 4:
                                Game1Extentions.Predict_createMultipleObjectDebris(_ctx, random.Choose("(O)904", "(O)905"), xTile, yTile, random.Next(1, 3), location);
                                break;
                            case 5:
                                Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)246", xTile, yTile, random.Next(4, 8), location);
                                Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)247", xTile, yTile, random.Next(2, 5), location);
                                Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)245", xTile, yTile, random.Next(4, 8), location);
                                break;
                            case 6:
                                Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)275", xTile, yTile, 2, location);
                                break;
                            case 7:
                                Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)288", xTile, yTile, random.Next(3, 6), location);
                                break;
                            default:
                                Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "MixedFlowerSeeds", xTile, yTile, random.Next(5, 6), location);
                                break;
                        }

                        break;
                }

                return true;
            }

            return false;
        }
    }
}

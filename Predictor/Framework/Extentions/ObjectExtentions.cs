using Microsoft.Xna.Framework;
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

        // Unused
        public static void Predict_cutWeed(this Object obj, PredictionContext _ctx, Farmer who)
        {
            GameLocation location = obj.Location;
            string? text2 = null;
            if (_ctx.Random.NextBool())
            {
                text2 = "771";
            }
            else if (_ctx.Random.NextDouble() < 0.05 + ((who.stats.Get("Book_WildSeeds") != 0) ? 0.04 : 0.0))
            {
                text2 = "770";
            }
            else if (Game1.currentSeason == "summer" && _ctx.Random.NextDouble() < 0.05 + ((who.stats.Get("Book_WildSeeds") != 0) ? 0.04 : 0.0))
            {
                text2 = "MixedFlowerSeeds";
            }

            if (obj.name.Contains("GreenRainWeeds") && _ctx.Random.NextDouble() < 0.1)
            {
                text2 = "Moss";
            }

            bool isGlass = obj.QualifiedItemId switch
            {
                "(O)319" => true,
                "(O)320" => true,
                "(O)321" => true,
                _ => false
            };

            if (isGlass && _ctx.Random.NextDouble() < 0.0025)
            {
                text2 = "338";
            }
            else if (isGlass)
            {
                text2 = null;
            }

            if (!isGlass)
            {
                if (_ctx.Random.NextDouble() < 1E-05)
                {
                    _ctx.AddItemIfNotNull("(H)40");
                }

                if (_ctx.Random.NextDouble() <= 0.01 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
                {
                    _ctx.AddItemIfNotNull("(O)890");
                }
            }

            if (text2 != null)
            {
                _ctx.AddItemIfNotNull(text2, 1);
            }

            if (_ctx.Random.NextDouble() < 0.02)
            {
                // location.addJumperFrog(obj.TileLocation);
            }

            if (location.HasUnlockedAreaSecretNotes(who) && _ctx.Random.NextDouble() < 0.009)
            {
                PredictionItem? note = location.Predict_tryToCreateUnseenSecretNote(_ctx, who);
                if (note != null)
                {
                    Game1Extentions.Predict_createItemDebris(_ctx, note, new Vector2(obj.TileLocation.X + 0.5f, obj.TileLocation.Y + 0.75f) * 64f, Game1.player.FacingDirection, location);
                }
            }
        }
    }
}

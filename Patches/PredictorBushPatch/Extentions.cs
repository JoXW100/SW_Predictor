using Microsoft.Xna.Framework;
using PredictorPatchFramework;
using PredictorPatchFramework.Extentions;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace PredictorBushPatch
{
    internal static class Extentions
    {
        public static bool IsHarvestable(this Bush bush)
        {
            return !bush.townBush.Value && (int)bush.tileSheetOffset.Value == 1 && bush.inBloom();
        }

        public static void Predict_shake(this Bush bush, PredictionContext _ctx, Vector2 tileLocation)
        {
            if (bush.IsHarvestable())
            {
                string? shakeOff = bush.GetShakeOffItem();
                if (shakeOff == null)
                {
                    return;
                }

                switch (bush.size.Value)
                {
                    case 4:
                        CreateItemExtentions.Predict_createItemDebris(_ctx, PredictionItem.Create(shakeOff), new Vector2(bush.getBoundingBox().Center.X, bush.getBoundingBox().Bottom - 2), 0, bush. Location, bush.getBoundingBox().Bottom);
                        break;
                    case 3:
                        CreateItemExtentions.Predict_createObjectDebris(_ctx, shakeOff, (int)tileLocation.X, (int)tileLocation.Y);
                        break;
                    default:
                        {
                            int num = Utility.CreateRandom(tileLocation.X, (double)tileLocation.Y * 5000.0, Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed).Next(1, 2) + Game1.player.ForagingLevel / 4;
                            for (int i = 0; i < num; i++)
                            {
                                var item = PredictionItem.Create(shakeOff);
                                if (item != null && Game1.player.professions.Contains(16))
                                {
                                    item.Quality = 4;
                                }

                                CreateItemExtentions.Predict_createItemDebris(_ctx, item, Utility.PointToVector2(bush.getBoundingBox().Center), Game1.random.Next(1, 4));
                            }
                            break;
                        }
                }
            }
            else if (tileLocation.X == 20f && tileLocation.Y == 8f && Game1.dayOfMonth == 28 && Game1.timeOfDay == 1200 && !Game1.player.mailReceived.Contains("junimoPlush"))
            {
                _ctx.AddItemIfNotNull("(F)1733");
            }
        }
    }
}

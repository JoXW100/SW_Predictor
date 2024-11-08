using Microsoft.Xna.Framework;
using StardewValley.GameData.Crops;
using StardewValley;
using StardewValley.TerrainFeatures;
using PredictorPatchFramework;
using PredictorPatchFramework.Extensions;

namespace PredictorCropPatch.Extensions
{
    public static class Extensions
    {
        public static bool Predict_harvest(this CropData? data, PredictionContext _ctx, int xTile, int yTile, HoeDirt soil)
        {
            if (data == null)
            {
                return false;
            }

            int daysUntilFullyGrown = (int)Math.Ceiling(data.DaysInPhase.Sum() * (1.0 - soil.GetFertilizerSpeedBoost()));
            Random random2 = Utility.CreateRandom((double)xTile * 7.0, (double)yTile * 11.0, Game1.stats.DaysPlayed + daysUntilFullyGrown, Game1.uniqueIDForThisGame);
            
            int fertilizerQualityBoostLevel = soil.GetFertilizerQualityBoostLevel();
            double num = 0.2 * (Game1.player.FarmingLevel / 10.0) + 0.2 * fertilizerQualityBoostLevel * ((Game1.player.FarmingLevel + 2.0) / 12.0) + 0.01;
            double num2 = Math.Min(0.75, num * 2.0);
            int num3 = 0;
            bool flag = false;
            if (fertilizerQualityBoostLevel >= 3 && random2.NextDouble() < num / 2.0)
            {
                num3 = 4;
            }
            else if (random2.NextDouble() < num)
            {
                num3 = 2;
            }
            else if (random2.NextDouble() < num2 || fertilizerQualityBoostLevel >= 3)
            {
                num3 = 1;
            }

            num3 = MathHelper.Clamp(num3, data.HarvestMinQuality, data.HarvestMaxQuality ?? num3);
            int num4 = 1;
            int harvestMinStack = data.HarvestMinStack;
            int num5 = Math.Max(harvestMinStack, data.HarvestMaxStack);
            if (data.HarvestMaxIncreasePerFarmingLevel > 0f)
            {
                num5 += (int)(Game1.player.FarmingLevel * data.HarvestMaxIncreasePerFarmingLevel);
            }

            if (harvestMinStack > 1 || num5 > 1)
            {
                num4 = random2.Next(harvestMinStack, num5 + 1);
            }

            if (data.ExtraHarvestChance > 0.0)
            {
                while (random2.NextDouble() < Math.Min(0.9, data.ExtraHarvestChance))
                {
                    num4++;
                }
            }


            PredictionItem? item = PredictionItem.Create(data.HarvestItemId, 1, num3);
            if (data.HarvestMethod == HarvestMethod.Scythe)
            {
                CreateItemExtensions.Predict_createItemDebris(_ctx, item?.GetOne(), new Vector2(xTile * 64 + 32, yTile * 64 + 32), -1);

                flag = true;
            }
            else if (item != null)
            {
                // Game1.player.addItemToInventoryBool(item?.GetOne()))
                _ctx.AddItemIfNotNull(item?.GetOne());

                flag = true;
            }

            if (flag)
            {
                var harvestItemId = data.HarvestItemId;
                if (harvestItemId == "421")
                {
                    harvestItemId = "431";
                    num4 = random2.Next(1, 4);
                }

                item = PredictionItem.Create(harvestItemId);

                for (int i = 0; i < num4 - 1; i++)
                {
                    CreateItemExtensions.Predict_createItemDebris(_ctx, item?.GetOne(), new Vector2(xTile * 64 + 32, yTile * 64 + 32), -1);
                }

                if (harvestItemId == "262" && random2.NextDouble() < 0.4)
                {
                    PredictionItem? item2 = PredictionItem.Create("(O)178");
                    CreateItemExtensions.Predict_createItemDebris(_ctx, item2?.GetOne(), new Vector2(xTile * 64 + 32, yTile * 64 + 32), -1);
                }
                else if (harvestItemId == "771" && random2.NextDouble() < 0.1)
                {
                    PredictionItem? item3 = PredictionItem.Create("(O)770");
                    CreateItemExtensions.Predict_createItemDebris(_ctx, item3?.GetOne(), new Vector2(xTile * 64 + 32, yTile * 64 + 32), -1);
                }
            }

            return false;
        }
    }
}

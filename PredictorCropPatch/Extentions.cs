using Microsoft.Xna.Framework;
using StardewValley.GameData.Crops;
using StardewValley.Locations;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using StardewValley.Enchantments;
using StardewValley.Characters;
using StardewValley.Objects;
using PredictorPatchFramework;
using PredictorPatchFramework.Extensions;
using Object = StardewValley.Object;

namespace PredictorCropPatch.Extensions
{
    public static class HarvestExtensions
    {
        public static bool Predict_harvest(this HoeDirt dirt, PredictionContext _ctx, Tool? tool, Vector2 tileLocation)
        {
            if (tool != null && tool.isScythe())
            {
                return Predict_performToolAction(dirt, _ctx, tool, 0, tileLocation);
            }
            else
            {
                return Predict_performUseAction(dirt, _ctx, tileLocation);
            }
        }

        public static bool Predict_performUseAction(this HoeDirt dirt, PredictionContext _ctx, Vector2 tileLocation)
        {
            if (dirt.crop != null)
            {
                bool result = dirt.crop.currentPhase.Value >= dirt.crop.phaseDays.Count - 1 && (!dirt.crop.fullyGrown.Value || dirt.crop.dayOfCurrentPhase.Value <= 0);
                HarvestMethod harvestMethod = dirt.crop.GetHarvestMethod();
                if (Game1.player.CurrentTool != null && Game1.player.CurrentTool.isScythe() && Game1.player.CurrentTool.ItemId == "66")
                {
                    harvestMethod = HarvestMethod.Scythe;
                }

                if (harvestMethod == HarvestMethod.Grab && dirt.crop.Predict_harvest(_ctx, (int)tileLocation.X, (int)tileLocation.Y, dirt))
                {
                    GameLocation location = dirt.Location;
                    if (location is IslandLocation && _ctx.Random.NextDouble() < 0.05)
                    {
                        Game1.player.team.Predict_RequestLimitedNutDrops(_ctx, "IslandFarming", location, (int)tileLocation.X * 64, (int)tileLocation.Y * 64, 5);
                    }
                    return true;
                }

                return result;
            }

            return false;
        }

        public static bool Predict_performToolAction(this HoeDirt dirt, PredictionContext _ctx, Tool? t, int damage, Vector2 tileLocation)
        {
            GameLocation location = dirt.Location;
            if (t != null && t is not Hoe)
            {
                if (t is Pickaxe && dirt.crop == null)
                {
                    return true;
                }

                if (t is not WateringCan && t.isScythe())
                {
                    if ((dirt.crop != null && dirt.crop.GetHarvestMethod() == HarvestMethod.Scythe) || (dirt.crop != null && t.ItemId == "66"))
                    {
                        if (dirt.crop.indexOfHarvest.Value == "771" && t.hasEnchantmentOfType<HaymakerEnchantment>())
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                CreateItemExtensions.Predict_createItemDebris(_ctx, PredictionItem.Create("(O)771"), new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f + 32f), -1);
                            }
                        }

                        if (dirt.crop.Predict_harvest(_ctx, (int)tileLocation.X, (int)tileLocation.Y, dirt, null, isForcedScytheHarvest: true))
                        {
                            if (location is IslandLocation && _ctx.Random.NextDouble() < 0.05)
                            {
                                Game1.player.team.RequestLimitedNutDrops("IslandFarming", location, (int)tileLocation.X * 64, (int)tileLocation.Y * 64, 5);
                            }
                        }
                    }

                    if (dirt.crop == null && t.ItemId == "66" && location.objects.ContainsKey(tileLocation) && location.objects[tileLocation].isForage())
                    {
                        Object @object = location.objects[tileLocation];
                        if (t.getLastFarmerToUse() != null && t.getLastFarmerToUse().professions.Contains(16))
                        {
                            @object.Quality = 4;
                        }

                        CreateItemExtensions.Predict_createItemDebris(_ctx, new PredictionItem(@object), new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f + 32f), -1);
                    }
                }
            }

            return false;
        }

        public static bool Predict_harvest(this Crop crop, PredictionContext _ctx, int xTile, int yTile, HoeDirt soil, JunimoHarvester? junimoHarvester = null, bool isForcedScytheHarvest = false)
        {
            if (crop.dead.Value)
            {
                if (junimoHarvester != null)
                {
                    return true;
                }

                return false;
            }

            bool flag = false;
            if (crop.forageCrop.Value)
            {
                PredictionItem? @object = null;
                Random random = Utility.CreateDaySaveRandom(xTile * 1000, yTile * 2000);
                if (crop.whichForageCrop.Value == "1")
                {
                    @object = PredictionItem.Create("(O)399");
                }
                else if (crop.whichForageCrop.Value == "2")
                {
                    return false;
                }
                
                if (@object is null)
                {
                    return false;
                }

                if (Game1.player.professions.Contains(16))
                {
                    @object.Quality = 4;
                }
                else if (random.NextDouble() < (double)((float)Game1.player.ForagingLevel / 30f))
                {
                    @object.Quality = 2;
                }
                else if (random.NextDouble() < (double)((float)Game1.player.ForagingLevel / 15f))
                {
                    @object.Quality = 1;
                }

                if (junimoHarvester != null)
                {
                    // junimoHarvester.tryToAddItemToHut(@object);
                    _ctx.AddItemIfNotNull(@object);
                    return true;
                }

                if (isForcedScytheHarvest)
                {
                    Vector2 vector = new(xTile, yTile);
                    CreateItemExtensions.Predict_createItemDebris(_ctx, @object, new Vector2(vector.X * 64f + 32f, vector.Y * 64f + 32f), -1);
                    return true;
                }

                _ctx.AddItemIfNotNull(@object);
            }
            else if (crop.currentPhase.Value >= crop.phaseDays.Count - 1 && (!crop.fullyGrown.Value || crop.dayOfCurrentPhase.Value <= 0))
            {
                if (crop.indexOfHarvest.Value == null)
                {
                    return true;
                }

                CropData data = crop.GetData();
                Random random2 = Utility.CreateRandom((double)xTile * 7.0, (double)yTile * 11.0, Game1.stats.DaysPlayed, Game1.uniqueIDForThisGame);
                int fertilizerQualityBoostLevel = soil.GetFertilizerQualityBoostLevel();
                double num = 0.2 * (Game1.player.FarmingLevel / 10.0) + 0.2 * fertilizerQualityBoostLevel * ((Game1.player.FarmingLevel + 2.0) / 12.0) + 0.01;
                double num2 = Math.Min(0.75, num * 2.0);
                int num3 = 0;
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

                num3 = MathHelper.Clamp(num3, data?.HarvestMinQuality ?? 0, data?.HarvestMaxQuality ?? num3);
                int num4 = 1;
                if (data != null)
                {
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
                }

                if (data != null && data.ExtraHarvestChance > 0.0)
                {
                    while (random2.NextDouble() < Math.Min(0.9, data.ExtraHarvestChance))
                    {
                        num4++;
                    }
                }

                PredictionItem? item = crop.programColored.Value
                    ? new(new ColoredObject(crop.indexOfHarvest.Value, 1, crop.tintColor.Value) { Quality = num3 })
                    : PredictionItem.Create(crop.indexOfHarvest.Value, 1, num3);
                HarvestMethod harvestMethod = data?.HarvestMethod ?? HarvestMethod.Grab;
                if (harvestMethod == HarvestMethod.Scythe || isForcedScytheHarvest)
                {
                    if (junimoHarvester != null)
                    {
                        // junimoHarvester.tryToAddItemToHut(item.GetOne());
                        _ctx.AddItemIfNotNull(item?.GetOne());
                    }
                    else
                    {
                        CreateItemExtensions.Predict_createItemDebris(_ctx, item?.GetOne(), new Vector2(xTile * 64 + 32, yTile * 64 + 32), -1);
                    }

                    flag = true;
                }
                else if (junimoHarvester != null || (item != null))
                {
                    // Game1.player.addItemToInventoryBool(item?.GetOne()))
                    _ctx.AddItemIfNotNull(item?.GetOne());
                    if (junimoHarvester != null)
                    {
                        // junimoHarvester.tryToAddItemToHut(item.getOne());
                        _ctx.AddItemIfNotNull(item?.GetOne());
                    }

                    if (random2.NextDouble() < Game1.player.team.AverageLuckLevel() / 1500.0 + Game1.player.team.AverageDailyLuck() / 1200.0 + 9.9999997473787516E-05)
                    {
                        num4 *= 2;
                    }

                    flag = true;
                }

                if (flag)
                {
                    var indexOfHarvest = crop.indexOfHarvest.Value;
                    if (indexOfHarvest == "421")
                    {
                        indexOfHarvest = "431";
                        num4 = random2.Next(1, 4);
                    }

                    item = crop.programColored.Value
                        ? new PredictionItem(new ColoredObject(indexOfHarvest, 1, crop.tintColor.Value))
                        : PredictionItem.Create(indexOfHarvest);

                    for (int i = 0; i < num4 - 1; i++)
                    {
                        if (junimoHarvester == null)
                        {
                            CreateItemExtensions.Predict_createItemDebris(_ctx, item?.GetOne(), new Vector2(xTile * 64 + 32, yTile * 64 + 32), -1);
                        }
                        else
                        {
                            // junimoHarvester.tryToAddItemToHut(item.getOne())
                            _ctx.AddItemIfNotNull(item?.GetOne());
                        }
                    }

                    if (indexOfHarvest == "262" && random2.NextDouble() < 0.4)
                    {
                        PredictionItem? item2 = PredictionItem.Create("(O)178");
                        if (junimoHarvester == null)
                        {
                            CreateItemExtensions.Predict_createItemDebris(_ctx, item2?.GetOne(), new Vector2(xTile * 64 + 32, yTile * 64 + 32), -1);
                        }
                        else
                        {
                            // junimoHarvester.tryToAddItemToHut(item2.getOne());
                            _ctx.AddItemIfNotNull(item?.GetOne());
                        }
                    }
                    else if (indexOfHarvest == "771")
                    {
                        if (random2.NextDouble() < 0.1)
                        {
                            PredictionItem? item3 = PredictionItem.Create("(O)770");
                            if (junimoHarvester == null)
                            {
                                CreateItemExtensions.Predict_createItemDebris(_ctx, item3?.GetOne(), new Vector2(xTile * 64 + 32, yTile * 64 + 32), -1);
                            }
                            else
                            {
                                // junimoHarvester.tryToAddItemToHut(item3.getOne());
                                _ctx.AddItemIfNotNull(item?.GetOne());
                            }
                        }
                    }
                }
            }

            return false;
        }
    }
}

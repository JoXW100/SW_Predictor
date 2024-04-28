using StardewValley.Extensions;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Enchantments;
using StardewValley.Tools;
using Microsoft.Xna.Framework;

namespace Predictor.Framework.Extentions
{
    internal static class MineShaftExtentions
    {
        public static void Predict_checkStoneForItems(this MineShaft location, PredictionContext _ctx, string stoneId, int x, int y, Farmer? who)
        {
            long whichPlayer = who?.UniqueMultiplayerID ?? 0;
            int num = who?.LuckLevel ?? 0;
            double num2 = who?.DailyLuck ?? 0.0;
            int num3 = who?.MiningLevel ?? 0;
            double num4 = num2 / 2.0 + num3 * 0.005 + num * 0.001;
            Random random = Utility.CreateDaySaveRandom(x * 1000, y, location.mineLevel);
            random.NextDouble();
            double num5 = stoneId == 40.ToString() || stoneId == 42.ToString() ? 1.2 : 0.8;
            var stonesLeftOnThisLevel = location.stonesLeftOnThisLevel - 1;
            double num6 = 0.02 + 1.0 / Math.Max(1, stonesLeftOnThisLevel) + num / 100.0 + Game1.player.DailyLuck / 5.0;
            if (location.EnemyCount == 0)
            {
                num6 += 0.04;
            }

            if (who != null && who.hasBuff("dwarfStatue_1"))
            {
                num6 *= 1.25;
            }

            if (!location.ladderHasSpawned && !location.mustKillAllMonstersToAdvance() && (stonesLeftOnThisLevel == 0 || random.NextDouble() < num6) && location.shouldCreateLadderOnThisLevel())
            {
                // Indicate ladder spawned
                _ctx.Properties.Add(PredictionProperty.SpawnLadder, true);
                // createLadderDown(x, y);
            }

            if (location.Predict_breakStone(_ctx, stoneId, x, y, who, random))
            {
                return;
            }

            if (stoneId == "44")
            {
                int num7 = random.Next(59, 70);
                num7 += num7 % 2;
                bool flag = false;
                foreach (Farmer allFarmer in Game1.getAllFarmers())
                {
                    if (allFarmer.timesReachedMineBottom > 0)
                    {
                        flag = true;
                        break;
                    }
                }

                if (!flag)
                {
                    if (location.mineLevel < 40 && num7 != 66 && num7 != 68)
                    {
                        num7 = random.Choose(66, 68);
                    }
                    else if (location.mineLevel < 80 && (num7 == 64 || num7 == 60))
                    {
                        num7 = random.Choose(66, 70, 68, 62);
                    }
                }

                Game1Extentions.Predict_createObjectDebris(_ctx, "(O)" + num7, x, y, whichPlayer, location);
                // Game1.stats.OtherPreciousGemsFound++;
                return;
            }

            int num8 = who == null || !who.professions.Contains(22) ? 1 : 2;
            double num9 = who != null && who.hasBuff("dwarfStatue_4") ? 1.25 : 1.0;
            if (random.NextDouble() < 0.022 * (1.0 + num4) * num8 * num9)
            {
                string id = "(O)" + (535 + (location.getMineArea() == 40 ? 1 : location.getMineArea() == 80 ? 2 : 0));
                if (location.getMineArea() == 121)
                {
                    id = "(O)749";
                }

                if (who != null && who.professions.Contains(19) && random.NextBool())
                {
                    Game1Extentions.Predict_createObjectDebris(_ctx, id, x, y, whichPlayer, location);
                }

                Game1Extentions.Predict_createObjectDebris(_ctx, id, x, y, whichPlayer, location);
                // who?.gainExperience(5, 20 * location.getMineArea());
            }

            if (location.mineLevel > 20 && random.NextDouble() < 0.005 * (1.0 + num4) * num8 * num9)
            {
                if (who != null && who.professions.Contains(19) && random.NextBool())
                {
                    Game1Extentions.Predict_createObjectDebris(_ctx, "(O)749", x, y, whichPlayer, location);
                }

                Game1Extentions.Predict_createObjectDebris(_ctx, "(O)749", x, y, whichPlayer, location);
                // who?.gainExperience(5, 40 * location.getMineArea());
            }

            if (random.NextDouble() < 0.05 * (1.0 + num4) * num5)
            {
                int num10 = who == null || !who.professions.Contains(21) ? 1 : 2;
                double num11 = who != null && who.hasBuff("dwarfStatue_2") ? 0.1 : 0.0;
                if (random.NextDouble() < 0.25 * num10 + num11)
                {
                    Game1Extentions.Predict_createObjectDebris(_ctx, "(O)382", x, y, whichPlayer, location);
                    _ctx.Random.NextBool();
                    // Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(25, new Vector2(64 * x, 64 * y), Color.White, 8, _ctx.Random.NextBool(), 80f, 0, -1, -1f, 128));
                }

                Game1Extentions.Predict_createObjectDebris(_ctx, location.getOreIdForLevel(location.mineLevel, random), x, y, whichPlayer, location);
                // who?.gainExperience(3, 5);
            }
            else if (random.NextBool())
            {
                Game1Extentions.Predict_createDebris(_ctx, 14, x, y, 1, location);
            }
        }

        public static string Predict_checkForBuriedItem(this MineShaft location, PredictionContext _ctx, int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who)
        {
            if (location.isQuarryArea)
            {
                return "";
            }

            if (_ctx.Random.NextDouble() < 0.15)
            {
                string id = "(O)330";
                if (_ctx.Random.NextDouble() < 0.07)
                {
                    if (_ctx.Random.NextDouble() < 0.75)
                    {
                        switch (_ctx.Random.Next(5))
                        {
                            case 0:
                                id = "(O)96";
                                break;
                            case 1:
                                id = ((!who.hasOrWillReceiveMail("lostBookFound")) ? "(O)770" : ((Game1.netWorldState.Value.LostBooksFound < 21) ? "(O)102" : "(O)770"));
                                break;
                            case 2:
                                id = "(O)110";
                                break;
                            case 3:
                                id = "(O)112";
                                break;
                            case 4:
                                id = "(O)585";
                                break;
                        }
                    }
                    else if (_ctx.Random.NextDouble() < 0.75)
                    {
                        switch (location.getMineArea())
                        {
                            case 0:
                            case 10:
                                id = _ctx.Random.Choose("(O)121", "(O)97");
                                break;
                            case 40:
                                id = _ctx.Random.Choose("(O)122", "(O)336");
                                break;
                            case 80:
                                id = "(O)99";
                                break;
                        }
                    }
                    else
                    {
                        id = _ctx.Random.Choose("(O)126", "(O)127");
                    }
                }
                else if (_ctx.Random.NextDouble() < 0.19)
                {
                    id = (_ctx.Random.NextBool() ? "(O)390" : location.getOreIdForLevel(location.mineLevel, _ctx.Random));
                }
                else if (_ctx.Random.NextDouble() < 0.45)
                {
                    id = "(O)330";
                }
                else if (_ctx.Random.NextDouble() < 0.12)
                {
                    if (_ctx.Random.NextDouble() < 0.25)
                    {
                        id = "(O)749";
                    }
                    else
                    {
                        switch (location.getMineArea())
                        {
                            case 0:
                            case 10:
                                id = "(O)535";
                                break;
                            case 40:
                                id = "(O)536";
                                break;
                            case 80:
                                id = "(O)537";
                                break;
                        }
                    }
                }
                else
                {
                    id = "(O)78";
                }

                Game1Extentions.Predict_createObjectDebris(_ctx, id, xLocation, yLocation, who.UniqueMultiplayerID, location);
                bool num = who.CurrentTool is Hoe && who.CurrentTool.hasEnchantmentOfType<GenerousEnchantment>();
                if (num && _ctx.Random.NextDouble() < 0.25)
                {
                    Game1Extentions.Predict_createObjectDebris(_ctx, id, xLocation, yLocation, who.UniqueMultiplayerID, location);
                }

                return "";
            }

            return "";
        }
    
        public static void Predict_getAllMineShaftFish(this MineShaft location, PredictionContext _ctx, float millisecondsAfterNibble, string? bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string? locationName = null)
        {
            double num = 1.0;
            num += 0.4 * (double)who.FishingLevel;
            num += (double)waterDepth * 0.1;
            string text = "";
            if (who.CurrentTool is FishingRod rod)
            {
                if (rod.HasCuriosityLure())
                {
                    num += 5.0;
                }

                text = rod.GetBait()?.Name ?? "";
            }

            PredictionItem? item = null;
            var area = location.getMineArea();
            float baseChance = 1f;
            if (_ctx.Properties.TryGetValue(PredictionProperty.ChanceBase, out var c))
            {
                baseChance = (float)c;
            }
            double chance = 1.0;
            switch (area)
            {
                case 0:
                case 10:
                    num += (double)(text.Contains("Stonefish") ? 10 : 0);
                    chance = 0.02 + 0.01 * num;
                    item = _ctx.AddItemIfNotNull("(O)158");
                    break;
                case 40:
                    num += (double)(text.Contains("Ice Pip") ? 10 : 0);
                    chance = 0.015 + 0.009 * num;
                    item = _ctx.AddItemIfNotNull("(O)161");
                    break;
                case 80:
                    num += (double)(text.Contains("Lava Eel") ? 10 : 0);
                    chance = 0.01 + 0.008 * num;
                    item = _ctx.AddItemIfNotNull("(O)162");
                    break;
            }

            if (item != null)
            {
                _ctx.GetPropertyValue<Dictionary<string, FishChanceData>>(PredictionProperty.FishChanceData)
                    .TryAdd(item.ItemId, new FishChanceData((float)(chance * baseChance), true, false));
            }

            if (area == 80)
            {
                var chance1 = 0.05 + who.LuckLevel * 0.05;
                item = PredictionItem.Create("(O)CaveJelly");
                if (item != null)
                {
                    if (_ctx.GetPropertyValue<Dictionary<string, FishChanceData>>(PredictionProperty.FishChanceData)
                    .TryAdd(item.ItemId, new FishChanceData((float)((1 - chance) * chance1 * baseChance), true, false)))
                    {
                        _ctx.Items.Add(item);
                    }
                }

                var trashChance = (float)((1 - chance) * (1 - chance1) * baseChance);
                if (_ctx.Properties.TryGetValue(PredictionProperty.TrashChance, out var res))
                {
                    trashChance += (float)res;
                }
                _ctx.Properties[PredictionProperty.TrashChance] = (float)trashChance;

                var count = 6;
                for (int i = 167; i < 173; i++)
                {
                    item = PredictionItem.Create("(O)" + i);
                    if (item != null)
                    {
                        if(_ctx.GetPropertyValue<Dictionary<string, FishChanceData>>(PredictionProperty.FishChanceData)
                            .TryAdd(item.ItemId, new FishChanceData(1f / count, true, true)))
                        {
                            _ctx.Items.Add(item);
                        }
                    }
                }

                _ctx.Properties[PredictionProperty.ChanceBase] = 0f;
            }
            else
            {
                _ctx.Properties[PredictionProperty.ChanceBase] = (float)(baseChance * (1 - chance));
            }
        }
    }
}

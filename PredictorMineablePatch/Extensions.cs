using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Constants;
using StardewValley;
using StardewModdingAPI;
using Microsoft.Xna.Framework;
using PredictorPatchFramework;
using PredictorPatchFramework.Extensions;
using Force.DeepCloner;

namespace PredictorMineablePatch
{
    internal static class Extensions
    {
        public static IMonitor ModLog => ModEntry.Instance.Monitor;

        public static void Predict_OnStoneDestroyed(this GameLocation location, PredictionContext _ctx, string stoneId, int x, int y, Farmer who)
        {
            long whichPlayer = who?.UniqueMultiplayerID ?? 0;
            if (who?.currentLocation is MineShaft mineShaft && mineShaft.mineLevel > 120 && !mineShaft.isSideBranch())
            {
                int num = mineShaft.mineLevel - 121;
                if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0)
                {
                    float num2 = 0.01f;
                    num2 += num * 0.0005f;
                    if (num2 > 0.5f)
                    {
                        num2 = 0.5f;
                    }

                    if (_ctx.Random.NextBool(num2))
                    {
                        CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "CalicoEgg", x, y, _ctx.Random.Next(1, 4), who.UniqueMultiplayerID, location);
                    }
                }
            }

            if (who != null && _ctx.Random.NextDouble() <= 0.02 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
            {
                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)890", x, y, 1, who.UniqueMultiplayerID, location);
            }

            if (!MineShaft.IsGeneratedLevel(location, out var _))
            {
                if (stoneId == "343" || stoneId == "450")
                {
                    Random random = Utility.CreateDaySaveRandom(x * 2000, y);
                    double num3 = who != null && who.hasBuff("dwarfStatue_4") ? 1.25 : 0.0;
                    if (random.NextDouble() < 0.035 * num3 && Game1.stats.DaysPlayed > 1)
                    {
                        CreateItemExtensions.Predict_createObjectDebris(_ctx, "(O)" + (535 + (Game1.stats.DaysPlayed > 60 && random.NextDouble() < 0.2 ? 1 : Game1.stats.DaysPlayed > 120 && random.NextDouble() < 0.2 ? 2 : 0)), x, y, whichPlayer, location);
                    }

                    int num4 = who == null || !who.professions.Contains(21) ? 1 : 2;
                    double num5 = who != null && who.hasBuff("dwarfStatue_2") ? 0.03 : 0.0;
                    if (random.NextDouble() < 0.035 * num4 + num5 && Game1.stats.DaysPlayed > 1)
                    {
                        CreateItemExtensions.Predict_createObjectDebris(_ctx, "(O)382", x, y, whichPlayer, location);
                    }

                    if (random.NextDouble() < 0.01 && Game1.stats.DaysPlayed > 1)
                    {
                        CreateItemExtensions.Predict_createObjectDebris(_ctx, "(O)390", x, y, whichPlayer, location);
                    }
                }

                if (location != null)
                {
                    location.Predict_breakStone(_ctx, stoneId, x, y, who, Utility.CreateDaySaveRandom(x * 4000, y));
                }
            }
            else
            {
                if (location is MineShaft ms)
                {
                    ms.Predict_checkStoneForItems(_ctx, stoneId, x, y, who);
                }
            }
        }

        internal static bool Predict_breakStone(this GameLocation location, PredictionContext _ctx, string stoneId, int x, int y, Farmer? who, Random r)
        {
            int num = 0;
            int num2 = who != null && who.professions.Contains(18) ? 1 : 0;
            if (who != null && who.hasBuff("dwarfStatue_0"))
            {
                num2++;
            }

            if (stoneId == "44")
            {
                stoneId = (r.Next(1, 8) * 2).ToString();
            }

            long num3 = who?.UniqueMultiplayerID ?? 0;
            int num4 = who?.LuckLevel ?? 0;
            double num5 = who?.DailyLuck ?? 0.0;
            int num6 = who?.MiningLevel ?? 0;
            switch (stoneId)
            {
                case "95":
                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)909", x, y, num2 + r.Next(1, 3) + (r.NextDouble() < (double)(num4 / 100f) ? 1 : 0) + (r.NextDouble() < (double)(num6 / 200f) ? 1 : 0), num3, location);
                    num = 18;
                    break;
                case "843":
                case "844":
                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)848", x, y, num2 + r.Next(1, 3) + (r.NextDouble() < (double)(num4 / 100f) ? 1 : 0) + (r.NextDouble() < (double)(num6 / 200f) ? 1 : 0), num3, location);
                    break;
                case "25":
                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)719", x, y, r.Next(2, 5), num3, location);
                    num = 5;
                    if (location is IslandLocation && r.NextDouble() < 0.1)
                    {
                        Game1.player.team.Predict_RequestLimitedNutDrops(_ctx, "MusselStone", location, x * 64, y * 64, 5);
                    }

                    break;
                case "75":
                    CreateItemExtensions.Predict_createObjectDebris(_ctx, "(O)535", x, y, num3, location);
                    num = 8;
                    break;
                case "76":
                    CreateItemExtensions.Predict_createObjectDebris(_ctx, "(O)536", x, y, num3, location);
                    num = 16;
                    break;
                case "77":
                    CreateItemExtensions.Predict_createObjectDebris(_ctx, "(O)537", x, y, num3, location);
                    num = 32;
                    break;
                case "816":
                case "817":
                    if (r.NextDouble() < 0.1)
                    {
                        CreateItemExtensions.Predict_createObjectDebris(_ctx, "(O)823", x, y, num3, location);
                    }
                    else if (r.NextDouble() < 0.015)
                    {
                        CreateItemExtensions.Predict_createObjectDebris(_ctx, "(O)824", x, y, num3, location);
                    }
                    else if (r.NextDouble() < 0.1)
                    {
                        CreateItemExtensions.Predict_createObjectDebris(_ctx, "(O)" + (579 + r.Next(11)), x, y, num3, location);
                    }
                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)881", x, y, num2 + r.Next(1, 3) + (r.NextDouble() < (double)(num4 / 100f) ? 1 : 0) + (r.NextDouble() < (double)(num6 / 100f) ? 1 : 0), num3, location);
                    num = 6;
                    break;
                case "818":
                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)330", x, y, num2 + r.Next(1, 3) + (r.NextDouble() < (double)(num4 / 100f) ? 1 : 0) + (r.NextDouble() < (double)(num6 / 100f) ? 1 : 0), num3, location);
                    num = 6;
                    break;
                case "819":
                    CreateItemExtensions.Predict_createObjectDebris(_ctx, "(O)749", x, y, num3, location);
                    num = 64;
                    break;
                case "8":
                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)66", x, y, who == null || who.stats.Get(StatKeys.Mastery(3)) == 0 ? 1 : 2, num3, location);
                    num = 16;
                    break;
                case "10":
                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)68", x, y, who == null || who.stats.Get(StatKeys.Mastery(3)) == 0 ? 1 : 2, num3, location);
                    num = 16;
                    break;
                case "12":
                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)60", x, y, who == null || who.stats.Get(StatKeys.Mastery(3)) == 0 ? 1 : 2, num3, location);
                    num = 80;
                    break;
                case "14":
                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)62", x, y, who == null || who.stats.Get(StatKeys.Mastery(3)) == 0 ? 1 : 2, num3, location);
                    num = 40;
                    break;
                case "6":
                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)70", x, y, who == null || who.stats.Get(StatKeys.Mastery(3)) == 0 ? 1 : 2, num3, location);
                    num = 40;
                    break;
                case "4":
                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)64", x, y, who == null || who.stats.Get(StatKeys.Mastery(3)) == 0 ? 1 : 2, num3, location);
                    num = 80;
                    break;
                case "2":
                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)72", x, y, who == null || who.stats.Get(StatKeys.Mastery(3)) == 0 ? 1 : 2, num3, location);
                    num = 150;
                    break;
                case "845":
                case "846":
                case "847":
                case "670":
                case "668":
                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)390", x, y, num2 + r.Next(1, 3) + (r.NextDouble() < (double)(num4 / 100f) ? 1 : 0) + (r.NextDouble() < (double)(num6 / 100f) ? 1 : 0), num3, location);
                    num = 3;
                    if (r.NextDouble() < 0.08)
                    {
                        CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)382", x, y, 1 + num2, num3, location);
                        num = 4;
                    }

                    break;
                case "849":
                case "751":
                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)378", x, y, num2 + r.Next(1, 4) + (r.NextDouble() < (double)(num4 / 100f) ? 1 : 0) + (r.NextDouble() < (double)(num6 / 100f) ? 1 : 0), num3, location);
                    num = 5;
                    // Game1.multiplayer.broadcastSprites(this, Utility.sparkleWithinArea(new Microsoft.Xna.Framework.Rectangle(x * 64, (y - 1) * 64, 32, 96), 3, Color.Orange * 0.5f, 175, 100));
                    break;
                case "850":
                case "290":
                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)380", x, y, num2 + r.Next(1, 4) + (r.NextDouble() < (double)(num4 / 100f) ? 1 : 0) + (r.NextDouble() < (double)(num6 / 100f) ? 1 : 0), num3, location);
                    num = 12;
                    // Game1.multiplayer.broadcastSprites(this, Utility.sparkleWithinArea(new Microsoft.Xna.Framework.Rectangle(x * 64, (y - 1) * 64, 32, 96), 3, Color.White * 0.5f, 175, 100));
                    break;
                case "BasicCoalNode0":
                case "BasicCoalNode1":
                case "VolcanoCoalNode0":
                case "VolcanoCoalNode1":
                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)382", x, y, num2 + r.Next(1, 4) + (r.NextDouble() < (double)(num4 / 100f) ? 1 : 0) + (r.NextDouble() < (double)(num6 / 100f) ? 1 : 0), num3, location);
                    num = 10;
                    // Game1.multiplayer.broadcastSprites(this, Utility.sparkleWithinArea(new Microsoft.Xna.Framework.Rectangle(x * 64, (y - 1) * 64, 32, 96), 3, Color.Black * 0.5f, 175, 100));
                    break;
                case "VolcanoGoldNode":
                case "764":
                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)384", x, y, num2 + r.Next(1, 4) + (r.NextDouble() < (double)(num4 / 100f) ? 1 : 0) + (r.NextDouble() < (double)(num6 / 100f) ? 1 : 0), num3, location);
                    num = 18;
                    // Game1.multiplayer.broadcastSprites(this, Utility.sparkleWithinArea(new Microsoft.Xna.Framework.Rectangle(x * 64, (y - 1) * 64, 32, 96), 3, Color.Yellow * 0.5f, 175, 100));
                    break;
                case "765":
                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)386", x, y, num2 + r.Next(1, 4) + (r.NextDouble() < (double)(num4 / 100f) ? 1 : 0) + (r.NextDouble() < (double)(num6 / 100f) ? 1 : 0), num3, location);

                    // Game1.multiplayer.broadcastSprites(this, Utility.sparkleWithinArea(new Microsoft.Xna.Framework.Rectangle(x * 64, (y - 1) * 64, 32, 96), 6, Color.BlueViolet * 0.5f, 175, 100));
                    if (r.NextDouble() < 0.035)
                    {
                        CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)74", x, y, 1, num3, location);
                    }

                    num = 50;
                    break;
                case "CalicoEggStone_0":
                case "CalicoEggStone_1":
                case "CalicoEggStone_2":
                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "CalicoEgg", x, y, r.Next(1, 4) + (r.NextBool(num4 / 100f) ? 1 : 0) + (r.NextBool(num6 / 100f) ? 1 : 0), num3, location);
                    num = 50;
                    // Game1.multiplayer.broadcastSprites(this, Utility.sparkleWithinArea(new Microsoft.Xna.Framework.Rectangle(x * 64, (y - 1) * 64, 32, 96), 6, new Color(255, 120, 0) * 0.5f, 175, 100));
                    break;
            }

            if (who != null && who.professions.Contains(19) && r.NextBool())
            {
                int number = who.stats.Get(StatKeys.Mastery(3)) == 0 ? 1 : 2;
                switch (stoneId)
                {
                    case "8":
                        CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)66", x, y, number, who.UniqueMultiplayerID, location);
                        num = 8;
                        break;
                    case "10":
                        CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)68", x, y, number, who.UniqueMultiplayerID, location);
                        num = 8;
                        break;
                    case "12":
                        CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)60", x, y, number, who.UniqueMultiplayerID, location);
                        num = 50;
                        break;
                    case "14":
                        CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)62", x, y, number, who.UniqueMultiplayerID, location);
                        num = 20;
                        break;
                    case "6":
                        CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)70", x, y, number, who.UniqueMultiplayerID, location);
                        num = 20;
                        break;
                    case "4":
                        CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)64", x, y, number, who.UniqueMultiplayerID, location);
                        num = 50;
                        break;
                    case "2":
                        CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)72", x, y, number, who.UniqueMultiplayerID, location);
                        num = 100;
                        break;
                }
            }

            if (stoneId == "46")
            {
                CreateItemExtensions.Predict_createDebris(_ctx, 10, x, y, r.Next(1, 4), location);
                CreateItemExtensions.Predict_createDebris(_ctx, 6, x, y, r.Next(1, 5), location);

                if (r.NextDouble() < 0.25)
                {
                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)74", x, y, 1, num3, location);
                }

                num = 150;
                // Game1.stats.MysticStonesCrushed++;
            }

            if ((location.IsOutdoors || location.treatAsOutdoors.Value) && num == 0)
            {
                double num7 = num5 / 2.0 + num6 * 0.005 + num4 * 0.001;
                Random random = Utility.CreateDaySaveRandom(x * 1000, y);
                CreateItemExtensions.Predict_createDebris(_ctx, 14, x, y, 1, location);

                if (who != null)
                {
                    // who.gainExperience(3, 1);
                    double num8 = 0.0;
                    if (who.professions.Contains(21))
                    {
                        num8 += 0.05 * (1.0 + num7);
                    }

                    if (who.hasBuff("dwarfStatue_2"))
                    {
                        num8 += 0.025;
                    }

                    if (random.NextDouble() < num8)
                    {
                        CreateItemExtensions.Predict_createObjectDebris(_ctx, "(O)382", x, y, who.UniqueMultiplayerID, location);
                    }
                }

                if (random.NextDouble() < 0.05 * (1.0 + num7))
                {
                    CreateItemExtensions.Predict_createObjectDebris(_ctx, "(O)382", x, y, num3, location);
                    // Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(25, new Vector2(64 * x, 64 * y), Color.White, 8, Game1.random.NextBool(), 80f, 0, -1, -1f, 128));
                    // who?.gainExperience(3, 5);
                }
            }

            if (who != null && location.HasUnlockedAreaSecretNotes(who) && r.NextDouble() < 0.0075)
            {
                PredictionItem? @object = location.Predict_tryToCreateUnseenSecretNote(_ctx, who);
                if (@object != null)
                {
                    CreateItemExtensions.Predict_createItemDebris(_ctx, @object, new Vector2(x + 0.5f, y + 0.75f) * 64f, Game1.player.FacingDirection, location);
                }
            }

            // who?.gainExperience(3, num);
            return num > 0;
        }

        public static PredictionItem? Predict_tryToCreateUnseenSecretNote(this GameLocation location, PredictionContext _ctx, Farmer who)
        {
            if (location.currentEvent != null && location.currentEvent.isFestival)
            {
                return null;
            }

            bool flag = location.InIslandContext();
            if (!flag && (who == null || !who.hasMagnifyingGlass))
            {
                return null;
            }

            string itemId = (flag ? "(O)842" : "(O)79");
            int num = Utility.GetUnseenSecretNotes(who, flag, out int totalNotes).Length - who.Items.CountId(itemId);
            if (num <= 0)
            {
                return null;
            }

            float num2 = (float)(num - 1) / (float)Math.Max(1, totalNotes - 1);
            float chance = GameLocation.LAST_SECRET_NOTE_CHANCE + (GameLocation.FIRST_SECRET_NOTE_CHANCE - GameLocation.LAST_SECRET_NOTE_CHANCE) * num2;
            if (!_ctx.Random.NextBool(chance))
            {
                return null;
            }

            return PredictionItem.Create(itemId);
        }

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
                // createLadderDown(x, y);
                location.Predict_createLadderDown(_ctx);
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

                CreateItemExtensions.Predict_createObjectDebris(_ctx, "(O)" + num7, x, y, whichPlayer, location);
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
                    CreateItemExtensions.Predict_createObjectDebris(_ctx, id, x, y, whichPlayer, location);
                }

                CreateItemExtensions.Predict_createObjectDebris(_ctx, id, x, y, whichPlayer, location);
                // who?.gainExperience(5, 20 * location.getMineArea());
            }

            if (location.mineLevel > 20 && random.NextDouble() < 0.005 * (1.0 + num4) * num8 * num9)
            {
                if (who != null && who.professions.Contains(19) && random.NextBool())
                {
                    CreateItemExtensions.Predict_createObjectDebris(_ctx, "(O)749", x, y, whichPlayer, location);
                }

                CreateItemExtensions.Predict_createObjectDebris(_ctx, "(O)749", x, y, whichPlayer, location);
                // who?.gainExperience(5, 40 * location.getMineArea());
            }

            if (random.NextDouble() < 0.05 * (1.0 + num4) * num5)
            {
                int num10 = who == null || !who.professions.Contains(21) ? 1 : 2;
                double num11 = who != null && who.hasBuff("dwarfStatue_2") ? 0.1 : 0.0;
                if (random.NextDouble() < 0.25 * num10 + num11)
                {
                    CreateItemExtensions.Predict_createObjectDebris(_ctx, "(O)382", x, y, whichPlayer, location);
                    _ctx.Random.NextBool();
                    // Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(25, new Vector2(64 * x, 64 * y), Color.White, 8, _ctx.Random.NextBool(), 80f, 0, -1, -1f, 128));
                }

                CreateItemExtensions.Predict_createObjectDebris(_ctx, location.getOreIdForLevel(location.mineLevel, random), x, y, whichPlayer, location);
                // who?.gainExperience(3, 5);
            }
            else if (random.NextBool())
            {
                CreateItemExtensions.Predict_createDebris(_ctx, 14, x, y, 1, location);
            }
        }

        private static void Predict_createLadderDown(this MineShaft location, PredictionContext _ctx, bool forceShaft = false)
        {
            var mineRandom = location.mineRandom.DeepClone();
            _ctx.Properties.Add("spawnLadder", forceShaft || (location.getMineArea() == 121 && !location.mustKillAllMonstersToAdvance() && mineRandom.NextDouble() < 0.2));
        }
    }
}

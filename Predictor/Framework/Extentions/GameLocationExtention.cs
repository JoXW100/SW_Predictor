using StardewValley.Enchantments;
using StardewValley.Extensions;
using StardewValley.Internal;
using StardewValley.Tools;
using StardewValley.Locations;
using StardewValley.Constants;
using StardewValley.Buildings;
using StardewValley.GameData;
using StardewValley.GameData.Characters;
using StardewValley.GameData.Locations;
using StardewValley.Characters;
using StardewValley;
using StardewModdingAPI;
using Microsoft.Xna.Framework;
using Object = StardewValley.Object;

namespace Predictor.Framework.Extentions
{
    internal static class GameLocationExtention
    {
        public static IMonitor ModLog => ModEntry.Instance.Monitor;

        // StardewValley.Object.performToolAction(Tool t) => if (base.QualifiedItemId == "(O)590" || base.QualifiedItemId == "(O)SeedSpot")
        public static void Predict_digUpSpot(this GameLocation location, PredictionContext _ctx, Object spot, Vector2 tileLocation, Farmer who)
        {
            Random random = Utility.CreateDaySaveRandom((0f - tileLocation.X) * 7f, tileLocation.Y * 777f, Game1.netWorldState.Value.TreasureTotemsUsed * 777);
            var artifactSpotsDug = who.stats.Get("ArtifactSpotsDug") + 1;
            var pixelOrigin = tileLocation * Utils.TileSize;
            if (artifactSpotsDug > 2 && random.NextDouble() < 0.008 + ((!who.mailReceived.Contains("DefenseBookDropped")) ? (artifactSpotsDug * 0.002) : 0.005))
            {
                Game1Extentions.Predict_createMultipleItemDebris(_ctx, PredictionItem.Create("(O)Book_Defense"), pixelOrigin, Utility.GetOppositeFacingDirection(who.FacingDirection), location);
            }

            if (spot.QualifiedItemId == "(O)SeedSpot")
            {
                var seedItem = new PredictionItem(Utility.getRaccoonSeedForCurrentTimeOfYear(who, random));
                Game1Extentions.Predict_createMultipleItemDebris(_ctx, seedItem, pixelOrigin, Utility.GetOppositeFacingDirection(who.FacingDirection), location);
            }
            else
            {
                location.Predict_digUpArtifactSpot(_ctx, (int)tileLocation.X, (int)tileLocation.Y, who);
            }
        }

        public static void Predict_digUpArtifactSpot(this GameLocation location, PredictionContext _ctx, int xLocation, int yLocation, Farmer? who)
        {
            Random random = Utility.CreateDaySaveRandom(xLocation * 2000, yLocation, Game1.netWorldState.Value.TreasureTotemsUsed * 777);
            Vector2 vector = new(xLocation * 64, yLocation * 64);
            bool flag = (who?.CurrentTool as Hoe)?.hasEnchantmentOfType<GenerousEnchantment>() ?? false;
            Dictionary<string, LocationData> dictionary = DataLoader.Locations(Game1.content);
            LocationData data = location.GetData();
            ItemQueryContext context = new(location, who, random);
            IEnumerable<ArtifactSpotDropData> enumerable = dictionary["Default"].ArtifactSpots;
            if (data != null && data.ArtifactSpots?.Count > 0)
            {
                enumerable = enumerable.Concat(data.ArtifactSpots);
            }

            enumerable = enumerable.OrderBy((p) => p.Precedence);
            if (Game1.player.mailReceived.Contains("sawQiPlane") && random.NextDouble() < 0.05 + Game1.player.team.AverageDailyLuck() / 2.0)
            {
                var stack = random.Next(1, 3);
                Game1Extentions.Predict_createMultipleItemDebris(_ctx, PredictionItem.Create("(O)MysteryBox", stack), vector, -1, location);
            }

            UtilityExtentions.Predict_trySpawnRareObject(_ctx, who, vector, location, 9.0);
            foreach (ArtifactSpotDropData drop in enumerable)
            {
                if (!random.NextBool(drop.Chance) || drop.Condition != null && !GameStateQuery.CheckConditions(drop.Condition, location, who, null, null, random))
                {
                    continue;
                }

                Item item = ItemQueryResolver.TryResolveRandomItem(drop, context, avoidRepeat: false, null, null, null, delegate (string query, string error)
                {
                    ModLog.Log($"Location '{location.NameOrUniqueName}' failed parsing item query '{query}' for artifact spot '{drop.Id}': {error}", LogLevel.Warn);
                });

                if (item == null)
                {
                    continue;
                }

                if (drop.OneDebrisPerDrop && item.Stack > 1)
                {
                    Game1Extentions.Predict_createMultipleItemDebris(_ctx, new PredictionItem(item), vector, -1, location);
                }
                else
                {
                    Game1Extentions.Predict_createItemDebris(_ctx, new PredictionItem(item), vector, _ctx.Random.Next(4), location);
                }

                if (flag && drop.ApplyGenerousEnchantment && random.NextBool())
                {
                    var item1 = new PredictionItem(ItemQueryResolver.ApplyItemFields(item.getOne(), drop, context));
                    if (drop.OneDebrisPerDrop && item1.Stack > 1)
                    {
                        Game1Extentions.Predict_createMultipleItemDebris(_ctx, item1, vector, -1, location);
                    }
                    else
                    {
                        Game1Extentions.Predict_createItemDebris(_ctx, item1, vector, -1, location);
                    }
                }

                if (!drop.ContinueOnDrop)
                {
                    break;
                }
            }
        }

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
                        Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "CalicoEgg", x, y, _ctx.Random.Next(1, 4), who.UniqueMultiplayerID, location);
                    }
                }
            }

            if (who != null && _ctx.Random.NextDouble() <= 0.02 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
            {
                Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)890", x, y, 1, who.UniqueMultiplayerID, location);
            }

            if (!MineShaft.IsGeneratedLevel(location, out var _))
            {
                if (stoneId == "343" || stoneId == "450")
                {
                    Random random = Utility.CreateDaySaveRandom(x * 2000, y);
                    double num3 = who != null && who.hasBuff("dwarfStatue_4") ? 1.25 : 0.0;
                    if (random.NextDouble() < 0.035 * num3 && Game1.stats.DaysPlayed > 1)
                    {
                        Game1Extentions.Predict_createObjectDebris(_ctx, "(O)" + (535 + (Game1.stats.DaysPlayed > 60 && random.NextDouble() < 0.2 ? 1 : Game1.stats.DaysPlayed > 120 && random.NextDouble() < 0.2 ? 2 : 0)), x, y, whichPlayer, location);
                    }

                    int num4 = who == null || !who.professions.Contains(21) ? 1 : 2;
                    double num5 = who != null && who.hasBuff("dwarfStatue_2") ? 0.03 : 0.0;
                    if (random.NextDouble() < 0.035 * num4 + num5 && Game1.stats.DaysPlayed > 1)
                    {
                        Game1Extentions.Predict_createObjectDebris(_ctx, "(O)382", x, y, whichPlayer, location);
                    }

                    if (random.NextDouble() < 0.01 && Game1.stats.DaysPlayed > 1)
                    {
                        Game1Extentions.Predict_createObjectDebris(_ctx, "(O)390", x, y, whichPlayer, location);
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
                    Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)909", x, y, num2 + r.Next(1, 3) + (r.NextDouble() < (double)(num4 / 100f) ? 1 : 0) + (r.NextDouble() < (double)(num6 / 200f) ? 1 : 0), num3, location);
                    num = 18;
                    break;
                case "843":
                case "844":
                    Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)848", x, y, num2 + r.Next(1, 3) + (r.NextDouble() < (double)(num4 / 100f) ? 1 : 0) + (r.NextDouble() < (double)(num6 / 200f) ? 1 : 0), num3, location);
                    break;
                case "25":
                    Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)719", x, y, r.Next(2, 5), num3, location);
                    num = 5;
                    if (location is IslandLocation && r.NextDouble() < 0.1)
                    {
                        Game1.player.team.Predict_RequestLimitedNutDrops(_ctx, "MusselStone", location, x * 64, y * 64, 5);
                    }

                    break;
                case "75":
                    Game1Extentions.Predict_createObjectDebris(_ctx, "(O)535", x, y, num3, location);
                    num = 8;
                    break;
                case "76":
                    Game1Extentions.Predict_createObjectDebris(_ctx, "(O)536", x, y, num3, location);
                    num = 16;
                    break;
                case "77":
                    Game1Extentions.Predict_createObjectDebris(_ctx, "(O)537", x, y, num3, location);
                    num = 32;
                    break;
                case "816":
                case "817":
                    if (r.NextDouble() < 0.1)
                    {
                        Game1Extentions.Predict_createObjectDebris(_ctx, "(O)823", x, y, num3, location);
                    }
                    else if (r.NextDouble() < 0.015)
                    {
                        Game1Extentions.Predict_createObjectDebris(_ctx, "(O)824", x, y, num3, location);
                    }
                    else if (r.NextDouble() < 0.1)
                    {
                        Game1Extentions.Predict_createObjectDebris(_ctx, "(O)" + (579 + r.Next(11)), x, y, num3, location);
                    }
                    Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)881", x, y, num2 + r.Next(1, 3) + (r.NextDouble() < (double)(num4 / 100f) ? 1 : 0) + (r.NextDouble() < (double)(num6 / 100f) ? 1 : 0), num3, location);
                    num = 6;
                    break;
                case "818":
                    Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)330", x, y, num2 + r.Next(1, 3) + (r.NextDouble() < (double)(num4 / 100f) ? 1 : 0) + (r.NextDouble() < (double)(num6 / 100f) ? 1 : 0), num3, location);
                    num = 6;
                    break;
                case "819":
                    Game1Extentions.Predict_createObjectDebris(_ctx, "(O)749", x, y, num3, location);
                    num = 64;
                    break;
                case "8":
                    Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)66", x, y, who == null || who.stats.Get(StatKeys.Mastery(3)) == 0 ? 1 : 2, num3, location);
                    num = 16;
                    break;
                case "10":
                    Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)68", x, y, who == null || who.stats.Get(StatKeys.Mastery(3)) == 0 ? 1 : 2, num3, location);
                    num = 16;
                    break;
                case "12":
                    Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)60", x, y, who == null || who.stats.Get(StatKeys.Mastery(3)) == 0 ? 1 : 2, num3, location);
                    num = 80;
                    break;
                case "14":
                    Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)62", x, y, who == null || who.stats.Get(StatKeys.Mastery(3)) == 0 ? 1 : 2, num3, location);
                    num = 40;
                    break;
                case "6":
                    Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)70", x, y, who == null || who.stats.Get(StatKeys.Mastery(3)) == 0 ? 1 : 2, num3, location);
                    num = 40;
                    break;
                case "4":
                    Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)64", x, y, who == null || who.stats.Get(StatKeys.Mastery(3)) == 0 ? 1 : 2, num3, location);
                    num = 80;
                    break;
                case "2":
                    Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)72", x, y, who == null || who.stats.Get(StatKeys.Mastery(3)) == 0 ? 1 : 2, num3, location);
                    num = 150;
                    break;
                case "845":
                case "846":
                case "847":
                case "670":
                case "668":
                    Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)390", x, y, num2 + r.Next(1, 3) + (r.NextDouble() < (double)(num4 / 100f) ? 1 : 0) + (r.NextDouble() < (double)(num6 / 100f) ? 1 : 0), num3, location);
                    num = 3;
                    if (r.NextDouble() < 0.08)
                    {
                        Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)382", x, y, 1 + num2, num3, location);
                        num = 4;
                    }

                    break;
                case "849":
                case "751":
                    Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)378", x, y, num2 + r.Next(1, 4) + (r.NextDouble() < (double)(num4 / 100f) ? 1 : 0) + (r.NextDouble() < (double)(num6 / 100f) ? 1 : 0), num3, location);
                    num = 5;
                    // Game1.multiplayer.broadcastSprites(this, Utility.sparkleWithinArea(new Microsoft.Xna.Framework.Rectangle(x * 64, (y - 1) * 64, 32, 96), 3, Color.Orange * 0.5f, 175, 100));
                    break;
                case "850":
                case "290":
                    Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)380", x, y, num2 + r.Next(1, 4) + (r.NextDouble() < (double)(num4 / 100f) ? 1 : 0) + (r.NextDouble() < (double)(num6 / 100f) ? 1 : 0), num3, location);
                    num = 12;
                    // Game1.multiplayer.broadcastSprites(this, Utility.sparkleWithinArea(new Microsoft.Xna.Framework.Rectangle(x * 64, (y - 1) * 64, 32, 96), 3, Color.White * 0.5f, 175, 100));
                    break;
                case "BasicCoalNode0":
                case "BasicCoalNode1":
                case "VolcanoCoalNode0":
                case "VolcanoCoalNode1":
                    Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)382", x, y, num2 + r.Next(1, 4) + (r.NextDouble() < (double)(num4 / 100f) ? 1 : 0) + (r.NextDouble() < (double)(num6 / 100f) ? 1 : 0), num3, location);
                    num = 10;
                    // Game1.multiplayer.broadcastSprites(this, Utility.sparkleWithinArea(new Microsoft.Xna.Framework.Rectangle(x * 64, (y - 1) * 64, 32, 96), 3, Color.Black * 0.5f, 175, 100));
                    break;
                case "VolcanoGoldNode":
                case "764":
                    Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)384", x, y, num2 + r.Next(1, 4) + (r.NextDouble() < (double)(num4 / 100f) ? 1 : 0) + (r.NextDouble() < (double)(num6 / 100f) ? 1 : 0), num3, location);
                    num = 18;
                    // Game1.multiplayer.broadcastSprites(this, Utility.sparkleWithinArea(new Microsoft.Xna.Framework.Rectangle(x * 64, (y - 1) * 64, 32, 96), 3, Color.Yellow * 0.5f, 175, 100));
                    break;
                case "765":
                    Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)386", x, y, num2 + r.Next(1, 4) + (r.NextDouble() < (double)(num4 / 100f) ? 1 : 0) + (r.NextDouble() < (double)(num6 / 100f) ? 1 : 0), num3, location);

                    // Game1.multiplayer.broadcastSprites(this, Utility.sparkleWithinArea(new Microsoft.Xna.Framework.Rectangle(x * 64, (y - 1) * 64, 32, 96), 6, Color.BlueViolet * 0.5f, 175, 100));
                    if (r.NextDouble() < 0.035)
                    {
                        Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)74", x, y, 1, num3, location);
                    }

                    num = 50;
                    break;
                case "CalicoEggStone_0":
                case "CalicoEggStone_1":
                case "CalicoEggStone_2":
                    Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "CalicoEgg", x, y, r.Next(1, 4) + (r.NextBool(num4 / 100f) ? 1 : 0) + (r.NextBool(num6 / 100f) ? 1 : 0), num3, location);
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
                        Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)66", x, y, number, who.UniqueMultiplayerID, location);
                        num = 8;
                        break;
                    case "10":
                        Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)68", x, y, number, who.UniqueMultiplayerID, location);
                        num = 8;
                        break;
                    case "12":
                        Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)60", x, y, number, who.UniqueMultiplayerID, location);
                        num = 50;
                        break;
                    case "14":
                        Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)62", x, y, number, who.UniqueMultiplayerID, location);
                        num = 20;
                        break;
                    case "6":
                        Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)70", x, y, number, who.UniqueMultiplayerID, location);
                        num = 20;
                        break;
                    case "4":
                        Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)64", x, y, number, who.UniqueMultiplayerID, location);
                        num = 50;
                        break;
                    case "2":
                        Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)72", x, y, number, who.UniqueMultiplayerID, location);
                        num = 100;
                        break;
                }
            }

            if (stoneId == "46")
            {
                Game1Extentions.Predict_createDebris(_ctx, 10, x, y, r.Next(1, 4), location);
                Game1Extentions.Predict_createDebris(_ctx, 6, x, y, r.Next(1, 5), location);

                if (r.NextDouble() < 0.25)
                {
                    Game1Extentions.Predict_createMultipleObjectDebris(_ctx, "(O)74", x, y, 1, num3, location);
                }

                num = 150;
                // Game1.stats.MysticStonesCrushed++;
            }

            if ((location.IsOutdoors || location.treatAsOutdoors.Value) && num == 0)
            {
                double num7 = num5 / 2.0 + num6 * 0.005 + num4 * 0.001;
                Random random = Utility.CreateDaySaveRandom(x * 1000, y);
                Game1Extentions.Predict_createDebris(_ctx, 14, x, y, 1, location);

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
                        Game1Extentions.Predict_createObjectDebris(_ctx, "(O)382", x, y, who.UniqueMultiplayerID, location);
                    }
                }

                if (random.NextDouble() < 0.05 * (1.0 + num7))
                {
                    Game1Extentions.Predict_createObjectDebris(_ctx, "(O)382", x, y, num3, location);
                    // Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(25, new Vector2(64 * x, 64 * y), Color.White, 8, Game1.random.NextBool(), 80f, 0, -1, -1f, 128));
                    // who?.gainExperience(3, 5);
                }
            }

            if (who != null && location.HasUnlockedAreaSecretNotes(who) && r.NextDouble() < 0.0075)
            {
                PredictionItem? @object = location.Predict_tryToCreateUnseenSecretNote(_ctx, who);
                if (@object != null)
                {
                    Game1Extentions.Predict_createItemDebris(_ctx, @object, new Vector2(x + 0.5f, y + 0.75f) * 64f, Game1.player.FacingDirection, location);
                }
            }

            // who?.gainExperience(3, num);
            return num > 0;
        }

        public static void Predict_getAllFish(this GameLocation location, PredictionContext _ctx, float millisecondsAfterNibble, string? bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string? locationName = null)
        {
            _ctx.Properties.TryAdd(PredictionProperty.FishChanceData, new Dictionary<string, FishChanceData>());
            _ctx.Properties.TryAdd(PredictionProperty.ChanceBase, 1f);
            _ctx.Properties.TryAdd(PredictionProperty.TrashChance, 0f);

            // var fish = location.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, locationName);

            if (location is MineShaft mine)
            {
                mine.Predict_getAllMineShaftFish(_ctx, millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, locationName);
            }
            else if (location is Farm farm)
            {
                farm.Predict_getAllFarmFish(_ctx, millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, locationName);
            }

            float baseChance = 1f;
            if (_ctx.Properties.TryGetValue(PredictionProperty.ChanceBase, out var c))
            {
                baseChance = (float)c;
            }

            if (baseChance <= 0)
            {
                return;
            }

            if (bobberTile != Vector2.Zero)
            {
                foreach (Building building in location.buildings)
                {
                    if (building is FishPond fishPond && fishPond.isTileFishable(bobberTile))
                    {
                        if (fishPond.FishCount > 0)
                        {
                            var item = _ctx.AddItemIfNotNull(fishPond.fishType.Value);
                            if (item != null)
                            {
                                _ctx.GetPropertyValue<Dictionary<string, FishChanceData>>(PredictionProperty.FishChanceData)
                                    .TryAdd(item.ItemId, new FishChanceData(baseChance, true, false));
                            }
                        }
                        return;
                    }
                }
            }

            bool isTutorialCatch = who.fishCaught.Length == 0;
            location.Predict_GetAllFishFromLocationData(_ctx, bobberTile, waterDepth, who, isTutorialCatch, isInherited: false);
        }

        public static void Predict_GetAllFishFromLocationData(this GameLocation location, PredictionContext _ctx, Vector2 bobberTile, int waterDepth, Farmer player, bool isTutorialCatch, bool isInherited, ItemQueryContext? itemQueryContext = null)
        {
            Dictionary<string, LocationData> dictionary = DataLoader.Locations(Game1.content);
            LocationData? locationData = location?.GetData();
            Dictionary<string, string> allFishData = DataLoader.Fish(Game1.content);
            if (location == null || !location.TryGetFishAreaForTile(bobberTile, out var id, out var _))
            {
                id = null;
            }

            bool flag = false;
            bool hasCuriosityLure = false;
            string? text = null;
            if (player.CurrentTool is FishingRod fishingRod)
            {
                flag = fishingRod.HasMagicBait();
                hasCuriosityLure = fishingRod.HasCuriosityLure();
                Object bait = fishingRod.GetBait();
                if (bait?.QualifiedItemId == "(O)SpecificBait" && bait.preservedParentSheetIndex.Value != null)
                {
                    text = "(O)" + bait.preservedParentSheetIndex.Value;
                }
            }

            Point tilePoint = player.TilePoint;
            itemQueryContext ??= new ItemQueryContext(location, null, _ctx.Random);

            IEnumerable<SpawnFishData> enumerable = dictionary["Default"].Fish;
            if (locationData != null && locationData.Fish != null && locationData.Fish.Count > 0)
            {
                enumerable = enumerable.Concat(locationData.Fish);
            }

            enumerable = from p in enumerable orderby p.Precedence, _ctx.Random.Next() select p;

            float baseChance = _ctx.Properties.ContainsKey(PredictionProperty.ChanceBase) ? (float)_ctx.Properties[PredictionProperty.ChanceBase] : 1f;
            float randomModifier = (float)Utility.CreateRandom(Game1.uniqueIDForThisGame, player.stats.Get("PreciseFishCaught") * 859).NextDouble();
            var spawns = EnumerateValidSpawns(enumerable, location, player, id, isInherited, flag, bobberTile, waterDepth, hasCuriosityLure, text, isTutorialCatch).ToArray();
            var chanceWeights = new Dictionary<string, float>();
            foreach (var spawn in spawns)
            {
                if (spawn.RandomItemId != null && spawn.RandomItemId.Any())
                {
                    foreach (var rid in spawn.RandomItemId)
                    {
                        var results = ItemQueryResolver.TryResolve(rid, itemQueryContext, ItemQuerySearchMode.All, spawn.PerItemCondition, spawn.MaxItems);
                        if (results != null && results.Length == 1 && ItemQueryResolver.ApplyItemFields(results.First().Item, spawn, itemQueryContext) is Item item)
                        {
                            var chance = Predict_CheckGenericFishRequirements(item, allFishData, location, player, spawn, waterDepth, flag, hasCuriosityLure, spawn.ItemId == text, isTutorialCatch);
                            chanceWeights.TryAdd(rid, chance);
                        }
                    }
                }
                else
                {
                    var results = ItemQueryResolver.TryResolve(spawn.ItemId, itemQueryContext, ItemQuerySearchMode.All, spawn.PerItemCondition, spawn.MaxItems);
                    if (results != null && results.Length == 1 && ItemQueryResolver.ApplyItemFields(results.First().Item, spawn, itemQueryContext) is Item item)
                    {
                        var chance = Predict_CheckGenericFishRequirements(item, allFishData, location, player, spawn, waterDepth, flag, hasCuriosityLure, spawn.ItemId == text, isTutorialCatch);
                        chanceWeights.TryAdd(spawn.ItemId, chance);
                    }
                }
            }

            var groupWeightSums = new Dictionary<int, float>();
            foreach (var spawn in spawns)
            {
                var weight = spawn.ItemId != null ? chanceWeights.GetValueOrDefault(spawn.ItemId, 1f) : 1f;
                if (groupWeightSums.TryGetValue(spawn.Precedence, out var sum))
                {
                    groupWeightSums[spawn.Precedence] = sum + weight;
                }
                else
                {
                    groupWeightSums[spawn.Precedence] = weight;
                }
            }

            var continueChance = 1f;
            var groupChanceSum = 1f;
            var currentPrecedence = spawns.FirstOrDefault()?.Precedence ?? 0;
            var chances = spawns.Select(spawn =>
            {
                var r = 1f;
                if (spawn.UseFishCaughtSeededRandom)
                {
                    r = (1 - randomModifier);
                }
                else
                {
                    r = spawn.GetChance(hasCuriosityLure, player.DailyLuck, player.LuckLevel, (float value, IList<QuantityModifier> modifiers, QuantityModifier.QuantityModifierMode mode) => Utility.ApplyQuantityModifiers(value, modifiers, mode, location), spawn.ItemId == text);
                }

                if (r > 1f)
                {
                    r = 1f;
                }

                if (currentPrecedence != spawn.Precedence)
                {
                    currentPrecedence = spawn.Precedence;
                    continueChance *= (1 - groupChanceSum);
                    groupChanceSum = 1f;
                }

                var weight = spawn.ItemId != null ? chanceWeights.GetValueOrDefault(spawn.ItemId, 0f) : 0f;
                r *= weight;

                groupChanceSum *= (1 - r);
                return r * continueChance;
            }).ToArray();

            continueChance = 1f;
            foreach (var chance in chances)
            {
                continueChance *= (1 - chance);
            }

            float chanceSum = Enumerable.Sum(chances) + continueChance;
            for (int i = 0; i < spawns.Length; i++)
            {
                SpawnFishData spawn = spawns[i];
                float chance = chances[i];

                var isTrash = spawn.RandomItemId != null && spawn.RandomItemId.Any();
                var ids = isTrash && spawn.RandomItemId != null
                    ? spawn.RandomItemId
                    : new List<string>() { spawn.ItemId };

                chance /= chanceSum;

                foreach (var selected in ids)
                {
                    var prediction = PredictionItem.Create(selected);
                    if (prediction != null && chance > 0)
                    {
                        var selectChance = (chance / ids.Count) * baseChance;
                        if (_ctx.GetPropertyValue<Dictionary<string, FishChanceData>>(PredictionProperty.FishChanceData).TryGetValue(prediction.ItemId, out var chanceData))
                        {
                            chanceData.Chance += selectChance;
                        }
                        else
                        {
                            _ctx.Items.Add(prediction);
                        }
                        _ctx.GetPropertyValue<Dictionary<string, FishChanceData>>(PredictionProperty.FishChanceData)[prediction.ItemId] = chanceData ?? new FishChanceData(selectChance, true, isTrash);
                    }
                }
            }

            if (isTutorialCatch)
            {
                var item = _ctx.AddItemIfNotNull("(O)145");
                if (item != null)
                {
                    _ctx.GetPropertyValue<Dictionary<string, FishChanceData>>(PredictionProperty.FishChanceData)
                        .TryAdd(item.ItemId, new FishChanceData(continueChance, true, false));
                }
                _ctx.Properties[PredictionProperty.TrashChance] = 0f;
            }
            else
            {
                if (_ctx.Properties.TryGetValue(PredictionProperty.TrashChance, out var res))
                {
                    _ctx.Properties[PredictionProperty.TrashChance] = (float)res + continueChance;
                }
                else
                {
                    _ctx.Properties[PredictionProperty.TrashChance] = continueChance;
                }
            }
        }

        public static IEnumerable<SpawnFishData> EnumerateValidSpawns(IEnumerable<SpawnFishData> enumerable, GameLocation? location, Farmer player, string? id, bool isInherited, bool flag, Vector2 bobberTile, int waterDepth, bool hasCuriosityLure, string? baitTarget, bool isTutorialCatch)
        {
            Point tilePoint = player.TilePoint;
            Season seasonForLocation = Game1.GetSeasonForLocation(location);
            HashSet<string>? ignoreQueryKeys = (flag ? GameStateQuery.MagicBaitIgnoreQueryKeys : null);
            foreach (var spawn in enumerable)
            {
                if ((isInherited && !spawn.CanBeInherited) || (spawn.FishAreaId != null && id != spawn.FishAreaId) || (spawn.Season.HasValue && !flag && spawn.Season != seasonForLocation))
                {
                    continue;
                }

                Rectangle? playerPosition = spawn.PlayerPosition;
                if (playerPosition.HasValue && !playerPosition.GetValueOrDefault().Contains(tilePoint.X, tilePoint.Y))
                {
                    continue;
                }

                playerPosition = spawn.BobberPosition;
                if ((playerPosition.HasValue && !playerPosition.GetValueOrDefault().Contains((int)bobberTile.X, (int)bobberTile.Y))
                    || player.FishingLevel < spawn.MinFishingLevel
                    || waterDepth < spawn.MinDistanceFromShore
                    || (spawn.MaxDistanceFromShore > -1 && waterDepth > spawn.MaxDistanceFromShore) || (spawn.RequireMagicBait && !flag))
                {
                    continue;
                }

                if (spawn.Condition != null && !GameStateQuery.CheckConditions(spawn.Condition, location, ignoreQueryKeys: ignoreQueryKeys))
                {
                    continue;
                }

                if (spawn.ItemId != null)
                {
                    var data = ItemRegistry.Create(spawn.ItemId);
                    if (data == null || data.Name.Contains("Error") || (!ModEntry.Instance.Config.ShowSecretNoteChances && data.Name.Contains("Secret Note")))
                    {
                        continue;
                    }

                    if (spawn.CatchLimit > -1 && player.fishCaught.TryGetValue(data.QualifiedItemId, out var value2) && (value2?.FirstOrDefault() ?? 0) >= spawn.CatchLimit)
                    {
                        continue;
                    }
                }

                yield return spawn;
            }
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

        internal static float Predict_CheckGenericFishRequirements(Item fish, Dictionary<string, string> allFishData, GameLocation? location, Farmer player, SpawnFishData spawn, int waterDepth, bool usingMagicBait, bool hasCuriosityLure, bool usingTargetBait, bool isTutorialCatch)
        {
            if (!fish.HasTypeObject() || !allFishData.TryGetValue(fish.ItemId, out var value))
            {
                return !isTutorialCatch ? 1f : 0f;
            }

            string[] array = value.Split('/');
            if (ArgUtility.Get(array, 1) == "trap")
            {
                return !isTutorialCatch ? 1f : 0f;
            }

            bool flag = player.CurrentTool.QualifiedItemId == "(T)TrainingRod";
            if (flag)
            {
                if (!ArgUtility.TryGetInt(array, 1, out var value2, out var error2))
                {
                    return 0f;
                }

                if (value2 >= 50)
                {
                    return 0f;
                }
            }

            if (isTutorialCatch)
            {
                if (!ArgUtility.TryGetOptionalBool(array, 13, out var value3, out var error3))
                {
                    return 0f;
                }

                if (!value3)
                {
                    return 0f;
                }
            }

            if (!spawn.IgnoreFishDataRequirements)
            {
                if (!usingMagicBait)
                {
                    if (!ArgUtility.TryGet(array, 5, out var value4, out var error4))
                    {
                        return 0f;
                    }

                    string[] array2 = ArgUtility.SplitBySpace(value4);
                    bool flag2 = false;
                    for (int i = 0; i < array2.Length; i += 2)
                    {
                        if (!ArgUtility.TryGetInt(array2, i, out var value5, out error4) || !ArgUtility.TryGetInt(array2, i + 1, out var value6, out error4))
                        {
                            return 0f;
                        }

                        if (Game1.timeOfDay >= value5 && Game1.timeOfDay < value6)
                        {
                            flag2 = true;
                            break;
                        }
                    }

                    if (!flag2)
                    {
                        return 0f;
                    }
                }

                if (!usingMagicBait && location != null)
                {
                    if (!ArgUtility.TryGet(array, 7, out var value7, out var error5))
                    {
                        return 0f;
                    }

                    if (!(value7 == "rainy"))
                    {
                        if (value7 == "sunny" && location.IsRainingHere())
                        {
                            return 0f;
                        }
                    }
                    else if (!location.IsRainingHere())
                    {
                        return 0f;
                    }
                }

                if (!ArgUtility.TryGetInt(array, 12, out var value8, out var error6))
                {
                    return 0f;
                }

                if (player.FishingLevel < value8)
                {
                    return 0f;
                }

                if (!ArgUtility.TryGetInt(array, 9, out var value9, out var error7) || !ArgUtility.TryGetFloat(array, 10, out var value10, out error7) || !ArgUtility.TryGetFloat(array, 11, out var value11, out error7))
                {
                    return 0f;
                }

                float num = value11 * value10;
                value10 -= (float)Math.Max(0, value9 - waterDepth) * num;
                value10 += (float)player.FishingLevel / 50f;
                if (flag)
                {
                    value10 *= 1.1f;
                }

                value10 = Math.Min(value10, 0.9f);
                if ((double)value10 < 0.25 && hasCuriosityLure)
                {
                    if (spawn.CuriosityLureBuff > -1f)
                    {
                        value10 += spawn.CuriosityLureBuff;
                    }
                    else
                    {
                        float num2 = 0.25f;
                        float num3 = 0.08f;
                        value10 = (num2 - num3) / num2 * value10 + (num2 - num3) / 2f;
                    }
                }

                if (usingTargetBait)
                {
                    value10 *= 1.66f;
                }

                if (spawn.ApplyDailyLuck)
                {
                    value10 += (float)player.DailyLuck;
                }

                List<QuantityModifier> chanceModifiers = spawn.ChanceModifiers;
                if (chanceModifiers != null && chanceModifiers.Count > 0)
                {
                    value10 = Utility.ApplyQuantityModifiers(value10, spawn.ChanceModifiers, spawn.ChanceModifierMode, location);
                }

                return value10;
            }

            return 0f;
        }
    
        public static bool Predict_CheckGarbage(this GameLocation location, PredictionContext _ctx, string id, Vector2 tile, Farmer who, bool reactNpcs = true)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            id = id switch
            {
                "0" => "JodiAndKent",
                "1" => "EmilyAndHaley",
                "2" => "Mayor",
                "3" => "Museum",
                "4" => "Blacksmith",
                "5" => "Saloon",
                "6" => "Evelyn",
                "7" => "JojaMart",
                _ => id
            };

            if (Game1.netWorldState.Value.CheckedGarbage.Overlaps(new[] { id }))
            {
                _ctx.Properties.Add(PredictionProperty.Exhausted, true);
                return true;
            }

            location.TryGetGarbageItem(id, who.DailyLuck, out var itemResult, out var selected, out var garbageRandom);
            var item = itemResult == null ? null : new PredictionItem(itemResult);

            if (reactNpcs)
            {
                List<NPC> reactions = new List<NPC>();
                foreach (NPC npc in Utility.GetNpcsWithinDistance(tile, 7, location))
                {
                    if (npc is not Horse)
                    {
                        CharacterData data = npc.GetData();
                        if (data == null || data.DumpsterDiveFriendshipEffect < 0)
                        {
                            reactions.Add(npc);
                        }
                    }
                }

                _ctx.Properties.Add(PredictionProperty.Affected, reactions);
            }

            if (selected != null)
            {
                if (selected.AddToInventoryDirectly)
                {
                    _ctx.AddItemIfNotNull(item);
                }
                else
                {
                    var pixelOrigin = new Vector2(tile.X + 0.5f, tile.Y - 1f) * 64f;
                    if (selected.CreateMultipleDebris)
                    {
                        Game1Extentions.Predict_createMultipleItemDebris(_ctx, item, pixelOrigin, 2, location, (int)pixelOrigin.Y + 64);
                    }
                    else
                    {
                        Game1Extentions.Predict_createItemDebris(_ctx, item, pixelOrigin, 2, location, (int)pixelOrigin.Y + 64);
                    }
                }
            }

            return true;
        }

        public static string Predict_checkForBuriedItem(this GameLocation location, PredictionContext _ctx, int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who)
        {
            if (location is MineShaft ms)
            {
                return MineShaftExtentions.Predict_checkForBuriedItem(ms, _ctx, xLocation, yLocation, explosion, detectOnly, who);
            }

            var random = Utility.CreateDaySaveRandom(xLocation * 2000, yLocation * 77, Game1.stats.DirtHoed);
            var text = location.Predict_HandleTreasureTileProperty(_ctx, xLocation, yLocation, detectOnly);
            if (text != null)
            {
                return text;
            }

            var flag = who?.CurrentTool is Hoe hoe && hoe.hasEnchantmentOfType<GenerousEnchantment>();
            var num = 0.5f;
            if (!location.IsFarm && location.IsOutdoors && location.GetSeason() is Season.Winter && random.NextDouble() < 0.08 && !explosion && !detectOnly && !(location is Desert))
            {
                Game1Extentions.Predict_createObjectDebris(_ctx, random.Choose("(O)412", "(O)416"), xLocation, yLocation);
                if (flag && random.NextDouble() < (double)num)
                {
                    Game1Extentions.Predict_createObjectDebris(_ctx, random.Choose("(O)412", "(O)416"), xLocation, yLocation);
                }

                return "";
            }

            var data = location.GetData();
            if (location.IsOutdoors && random.NextBool(data?.ChanceForClay ?? 0.03) && !explosion)
            {
                if (detectOnly)
                {
                    return "Item";
                }

                Game1Extentions.Predict_createObjectDebris(_ctx, "(O)330", xLocation, yLocation);
                if (flag && random.NextDouble() < (double)num)
                {
                    Game1Extentions.Predict_createObjectDebris(_ctx, "(O)330", xLocation, yLocation);
                }

                return "";
            }

            return "";
        }

        private static string? Predict_HandleTreasureTileProperty(this GameLocation location, PredictionContext _ctx, int xLocation, int yLocation, bool detectOnly)
        {
            // TODO: Optimize & remove
            var text = location.doesTileHaveProperty(xLocation, yLocation, "Treasure", "Back");
            if (text == null)
            {
                return null;
            }

            var array = ArgUtility.SplitBySpace(text);
            if (!ArgUtility.TryGet(array, 0, out var value, out var error))
            {
                return null;
            }

            if (detectOnly)
            {
                return value;
            }

            switch (value)
            {
                case "Arch":
                {
                    if (ArgUtility.TryGet(array, 1, out var id, out error))
                    {
                        Game1Extentions.Predict_createObjectDebris(_ctx, id, xLocation, yLocation, location);
                    }

                    break;
                }
                case "CaveCarrot":
                    Game1Extentions.Predict_createObjectDebris(_ctx, "(O)78", xLocation, yLocation, location: location);
                    break;
                case "Coins":
                    Game1Extentions.Predict_createObjectDebris(_ctx, "(O)330", xLocation, yLocation, location: location);
                    break;
                case "Coal":
                case "Copper":
                case "Gold":
                case "Iridium":
                case "Iron":
                {
                    int debrisType = value switch
                    {
                        "Coal" => 4,
                        "Copper" => 0,
                        "Gold" => 6,
                        "Iridium" => 10,
                        _ => 2,
                    };
                    if (ArgUtility.TryGetInt(array, 1, out var id, out error))
                    {
                        Game1Extentions.Predict_createDebris(_ctx, debrisType, xLocation, yLocation, id, location);
                    }

                    break;
                }
                case "Object":
                {
                    if (ArgUtility.TryGet(array, 1, out var id, out error))
                    {
                        Game1Extentions.Predict_createObjectDebris(_ctx, id, xLocation, yLocation, location: location);
                    }

                    break;
                }
                case "Item":
                {
                    if (ArgUtility.TryGet(array, 1, out var id, out error))
                    {
                        var item = PredictionItem.Create(id);
                        Game1Extentions.Predict_createItemDebris(_ctx, item, new Vector2(xLocation, yLocation), -1, location);
                    }
                    break;
                }
                default:
                    value = null;
                    break;
            }

            return value;
        }
    }
}

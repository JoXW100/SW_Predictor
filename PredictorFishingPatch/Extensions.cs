using StardewValley.Extensions;
using StardewValley.Internal;
using StardewValley.Tools;
using StardewValley.Locations;
using StardewValley.Buildings;
using StardewValley.GameData;
using StardewValley.GameData.Locations;
using StardewValley;
using StardewModdingAPI;
using Microsoft.Xna.Framework;
using PredictorPatchFramework;
using System.Reflection;
using Object = StardewValley.Object;
using xTile;
using System.Collections.Generic;

namespace PredictorFishingPatch
{
    internal static class Extensions
    {
        public static IMonitor ModLog => ModEntry.Instance.Monitor;

        public static void Predict_getAllFish(this GameLocation location, PredictionContext _ctx, string? bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string? locationName = null)
        {
            _ctx.Properties.TryAdd("fishChances", new Dictionary<string, FishChanceData>());
            _ctx.Properties.TryAdd("baseChance", 1f);
            _ctx.Properties.TryAdd("trashChance", 0f);

            // var fish = location.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, locationName);

            if (location is MineShaft mine)
            {
                mine.Predict_getAllMineShaftFish(_ctx, bait, waterDepth, who, baitPotency, bobberTile, locationName);
            }
            else if (location is Farm farm)
            {
                farm.Predict_getAllFarmFish(_ctx, bait, waterDepth, who, baitPotency, bobberTile, locationName);
            }
            else if (location is IslandSouthEast islandSouthEast)
            {
                islandSouthEast.Predict_getAllIslandSouthEastFish(_ctx, bait, waterDepth, who, baitPotency, bobberTile, locationName);
            }
            else if (location is IslandLocation island)
            {
                island.Predict_getAllIslandFish(_ctx, bait, waterDepth, who, baitPotency, bobberTile, locationName);
            }

            float baseChance = 1f;
            if (_ctx.Properties.TryGetValue("baseChance", out var c))
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
                                _ctx.GetPropertyValue<Dictionary<string, FishChanceData>>("fishChances")
                                    .TryAdd(item.ItemId, new FishChanceData(baseChance, true, false));
                            }
                        }
                        return;
                    }
                }
            }

            if (location.fishFrenzyFish.Value != null && !location.fishFrenzyFish.Value.Equals("") && Vector2.Distance(bobberTile, Utility.PointToVector2(location.fishSplashPoint.Value)) <= 2f)
            {
                _ctx.AddItemIfNotNull(location.fishFrenzyFish.Value);
                return;
            }

            bool isTutorialCatch = who.fishCaught.Length == 0;
            location.Predict_GetAllFishFromLocationData(_ctx, location.Name, bobberTile, waterDepth, who, isTutorialCatch, isInherited: false);
        }

        public static void Predict_getAllFarmFish(this Farm farm, PredictionContext _ctx, string? bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string? locationName = null)
        {
            var _fishLocationOverrideField = farm.GetType().GetField("_fishLocationOverride", BindingFlags.NonPublic | BindingFlags.Instance);
            var _fishChanceOverrideField = farm.GetType().GetField("_fishChanceOverride", BindingFlags.NonPublic | BindingFlags.Instance);
            string? _fishLocationOverride = _fishLocationOverrideField?.GetValue(farm) as string;
            float _fishChanceOverride = _fishChanceOverrideField?.GetValue(farm) as float? ?? 0;
            if (_fishLocationOverride == null)
            {
                _fishLocationOverride = "";
                string[] mapPropertySplitBySpaces = farm.GetMapPropertySplitBySpaces("FarmFishLocationOverride");
                if (mapPropertySplitBySpaces.Length != 0)
                {
                    if (!ArgUtility.TryGet(mapPropertySplitBySpaces, 0, out var value, out var error) || !ArgUtility.TryGetFloat(mapPropertySplitBySpaces, 1, out var value2, out error))
                    {
                        // LogMapPropertyError("FarmFishLocationOverride", mapPropertySplitBySpaces, error);
                    }
                    else
                    {
                        _fishLocationOverride = value;
                        _fishChanceOverride = value2;
                    }
                }
            }

            float baseChance = 1f;
            if (_ctx.Properties.TryGetValue("baseChance", out var c))
            {
                baseChance = (float)c;
            }

            if (_fishChanceOverride > 0f && _ctx.Random.NextDouble() < _fishChanceOverride)
            {
                _ctx.Properties["baseChance"] = _fishChanceOverride * baseChance;
                GameLocation? location = Game1.getLocationFromName(_fishLocationOverride);
                if (location != null)
                {
                    Predict_getAllFish(location, _ctx, bait, waterDepth, who, baitPotency, bobberTile);
                    return;
                }
                // return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, _fishLocationOverride);
            }
            else
            {
                _ctx.Properties["baseChance"] = (1 - _fishChanceOverride) * baseChance;
            }

            // return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile);
        }

        public static void Predict_GetAllFishFromLocationData(this GameLocation location, PredictionContext _ctx, string locationName, Vector2 bobberTile, int waterDepth, Farmer player, bool isTutorialCatch, bool isInherited, ItemQueryContext? itemQueryContext = null)
        {
            if (location == null)
            {
                location = Game1.getLocationFromName(locationName);
            }

            LocationData? locationData = location != null ? location.GetData() : GameLocation.GetData(locationName);
            Dictionary<string, string> allFishData = DataLoader.Fish(Game1.content);
            if (location == null || !location.TryGetFishAreaForTile(bobberTile, out var fishAreaId, out var _))
            {
                fishAreaId = null;
            }

            bool usingMagicBait = false;
            bool hasCuriosityLure = false;
            string? baitTargetFish = null;
            if (player.CurrentTool is FishingRod fishingRod)
            {
                usingMagicBait = fishingRod.HasMagicBait();
                hasCuriosityLure = fishingRod.HasCuriosityLure();
                Object bait = fishingRod.GetBait();
                if (bait != null)
                {
                    if (bait.QualifiedItemId == "(O)SpecificBait" && bait.preservedParentSheetIndex.Value != null)
                    {
                        baitTargetFish = "(O)" + bait.preservedParentSheetIndex.Value;
                    }
                }
            }

            Point tilePoint = player.TilePoint;
            itemQueryContext ??= new ItemQueryContext(location, null, _ctx.Random, "location '" + locationName + "' > fish data");

            IEnumerable<SpawnFishData> enumerable = Game1.locationData["Default"].Fish;
            if (locationData != null && locationData.Fish != null && locationData.Fish.Count > 0)
            {
                enumerable = enumerable.Concat(locationData.Fish);
            }

            enumerable = from p in enumerable orderby p.Precedence, _ctx.Random.Next() select p;

            float baseChance = (float)_ctx.Properties.GetValueOrDefault("baseChance", 1f);
            float randomModifier = (float)Utility.CreateRandom(Game1.uniqueIDForThisGame, player.stats.Get("PreciseFishCaught") * 859).NextDouble();
            var spawns = EnumerateValidSpawns(enumerable, location, player, fishAreaId, isInherited, usingMagicBait, bobberTile, waterDepth, hasCuriosityLure, baitTargetFish, isTutorialCatch).ToArray();
            var chanceWeights = new Dictionary<string, float>();
            foreach (var spawn in spawns)
            {
                var ids = spawn.RandomItemId != null && spawn.RandomItemId.Any() ? spawn.RandomItemId : new List<string>() { spawn.ItemId };
                foreach (var id in ids)
                {
                    var query = id;
                    if (!query.StartsWith('('))
                    {
                        query = query
                            .Replace("BOBBER_X", ((int)bobberTile.X).ToString()).Replace("BOBBER_Y", ((int)bobberTile.Y).ToString())
                            .Replace("WATER_DEPTH", waterDepth.ToString());
                    }

                    var results = ItemQueryResolver.TryResolve(query, itemQueryContext, ItemQuerySearchMode.AllOfTypeItem, spawn.PerItemCondition, spawn.MaxItems);
                    if (results == null)
                    {
                        continue;
                    }
                    foreach (var result in results)
                    {
                        if (result != null && ItemQueryResolver.ApplyItemFields(result.Item, spawn, itemQueryContext) is Item item)
                        {
                            var chance = Predict_CheckGenericFishRequirements(item, allFishData, location, player, spawn, waterDepth, usingMagicBait, hasCuriosityLure, spawn.ItemId == baitTargetFish, isTutorialCatch);
                            chanceWeights.TryAdd(item.QualifiedItemId, chance);
                        }
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
                    r = spawn.GetChance(hasCuriosityLure, player.DailyLuck, player.LuckLevel, (float value, IList<QuantityModifier> modifiers, QuantityModifier.QuantityModifierMode mode) => Utility.ApplyQuantityModifiers(value, modifiers, mode, location), spawn.ItemId == baitTargetFish);
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
                        if (_ctx.GetPropertyValue<Dictionary<string, FishChanceData>>("fishChances").TryGetValue(prediction.ItemId, out var chanceData))
                        {
                            chanceData.Chance += selectChance;
                        }
                        else
                        {
                            _ctx.Items.Add(prediction);
                        }
                        _ctx.GetPropertyValue<Dictionary<string, FishChanceData>>("fishChances")[prediction.ItemId] = chanceData ?? new FishChanceData(selectChance, true, isTrash);
                    }
                }
            }

            if (isTutorialCatch)
            {
                var item = _ctx.AddItemIfNotNull("(O)145");
                if (item != null)
                {
                    _ctx.GetPropertyValue<Dictionary<string, FishChanceData>>("fishChances")
                        .TryAdd(item.ItemId, new FishChanceData(continueChance, true, false));
                }
                _ctx.Properties["trashChance"] = 0f;
            }
            else
            {
                if (_ctx.Properties.TryGetValue("trashChance", out var res))
                {
                    _ctx.Properties["trashChance"] = (float)res + continueChance;
                }
                else
                {
                    _ctx.Properties["trashChance"] = continueChance;
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

        public static void Predict_getAllIslandFish(this IslandLocation location, PredictionContext _ctx, string? bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string? locationName = null)
        {
            float baseChance = _ctx.Properties.TryGetValue("baseChance", out var c) ? (float)c : 1f;
            float chance = 0.15f * baseChance;
            var item = _ctx.AddItemIfNotNull("(O)73");
            if (item != null)
            {
                _ctx.GetPropertyValue<Dictionary<string, FishChanceData>>("fishChances")
                    .TryAdd(item.ItemId, new FishChanceData(chance, true, false));
                baseChance = baseChance - chance;
            }
            _ctx.Properties["baseChance"] = baseChance;
        }

        public static void Predict_getAllIslandSouthEastFish(this IslandSouthEast location, PredictionContext _ctx, string? bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string? locationName = null)
        {
            if ((int)bobberTile.X >= 18 && (int)bobberTile.X <= 20 && (int)bobberTile.Y >= 20 && (int)bobberTile.Y <= 22 && !location.fishedWalnut.Value)
            {
                float baseChance = _ctx.Properties.TryGetValue("baseChance", out var c) ? (float)c : 1f;
                float chance = 0.15f * baseChance;
                var item = _ctx.AddItemIfNotNull("(O)73");
                if (item != null)
                {
                    _ctx.GetPropertyValue<Dictionary<string, FishChanceData>>("fishChances")
                        .TryAdd(item.ItemId, new FishChanceData(chance, true, false));
                    baseChance = baseChance - chance;
                }
                _ctx.Properties["baseChance"] = baseChance;
            }
            else
            {
                location.Predict_getAllIslandFish(_ctx, bait, waterDepth, who, baitPotency, bobberTile, locationName);
            }
        }

        public static void Predict_getAllMineShaftFish(this MineShaft location, PredictionContext _ctx, string? bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string? locationName = null)
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
            float baseChance = _ctx.Properties.TryGetValue("baseChance", out var c) ? (float)c : 1f;
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
                _ctx.GetPropertyValue<Dictionary<string, FishChanceData>>("fishChances")
                    .TryAdd(item.ItemId, new FishChanceData((float)(chance * baseChance), true, false));
            }

            if (area == 80)
            {
                var chance1 = 0.05 + who.LuckLevel * 0.05;
                item = PredictionItem.Create("(O)CaveJelly");
                if (item != null)
                {
                    if (_ctx.GetPropertyValue<Dictionary<string, FishChanceData>>("fishChances")
                    .TryAdd(item.ItemId, new FishChanceData((float)((1 - chance) * chance1 * baseChance), true, false)))
                    {
                        _ctx.Items.Add(item);
                    }
                }

                var trashChance = (float)((1 - chance) * (1 - chance1) * baseChance);
                if (_ctx.Properties.TryGetValue("trashChance", out var res))
                {
                    trashChance += (float)res;
                }
                _ctx.Properties["trashChance"] = (float)trashChance;

                var count = 6;
                for (int i = 167; i < 173; i++)
                {
                    item = PredictionItem.Create("(O)" + i);
                    if (item != null)
                    {
                        if (_ctx.GetPropertyValue<Dictionary<string, FishChanceData>>("fishChances")
                            .TryAdd(item.ItemId, new FishChanceData(1f / count, true, true)))
                        {
                            _ctx.Items.Add(item);
                        }
                    }
                }

                _ctx.Properties["baseChance"] = 0f;
            }
            else
            {
                _ctx.Properties["baseChance"] = (float)(baseChance * (1 - chance));
            }
        }
    }
}

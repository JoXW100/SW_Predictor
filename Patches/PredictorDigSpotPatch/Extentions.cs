using StardewValley.Enchantments;
using StardewValley.Extensions;
using StardewValley.Internal;
using StardewValley.Tools;
using StardewValley.GameData.Locations;
using StardewValley;
using StardewModdingAPI;
using Microsoft.Xna.Framework;
using Object = StardewValley.Object;
using PredictorPatchFramework;
using PredictorPatchFramework.Extentions;

namespace PredictorDigSpotPatch
{
    internal static class Extentions
    {
        public static IMonitor ModLog => ModEntry.Instance.Monitor;

        // StardewValley.Object.performToolAction(Tool t) => if (base.QualifiedItemId == "(O)590" || base.QualifiedItemId == "(O)SeedSpot")
        public static void Predict_digUpSpot(this GameLocation location, PredictionContext _ctx, Object spot, Vector2 tileLocation, Farmer who)
        {
            Random random = Utility.CreateDaySaveRandom((0f - tileLocation.X) * 7f, tileLocation.Y * 777f, Game1.netWorldState.Value.TreasureTotemsUsed * 777);
            var artifactSpotsDug = who.stats.Get("ArtifactSpotsDug") + 1;
            var pixelOrigin = tileLocation * FrameworkUtils.TileSize;
            if (artifactSpotsDug > 2 && random.NextDouble() < 0.008 + ((!who.mailReceived.Contains("DefenseBookDropped")) ? (artifactSpotsDug * 0.002) : 0.005))
            {
                CreateItemExtentions.Predict_createMultipleItemDebris(_ctx, PredictionItem.Create("(O)Book_Defense"), pixelOrigin, Utility.GetOppositeFacingDirection(who.FacingDirection), location);
            }

            if (spot.QualifiedItemId == "(O)SeedSpot")
            {
                var seedItem = new PredictionItem(Utility.getRaccoonSeedForCurrentTimeOfYear(who, random));
                CreateItemExtentions.Predict_createMultipleItemDebris(_ctx, seedItem, pixelOrigin, Utility.GetOppositeFacingDirection(who.FacingDirection), location);
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
                CreateItemExtentions.Predict_createMultipleItemDebris(_ctx, PredictionItem.Create("(O)MysteryBox", stack), vector, -1, location);
            }

            CreateItemExtentions.Predict_trySpawnRareObject(_ctx, who, vector, location, 9.0);
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
                    CreateItemExtentions.Predict_createMultipleItemDebris(_ctx, new PredictionItem(item), vector, -1, location);
                }
                else
                {
                    CreateItemExtentions.Predict_createItemDebris(_ctx, new PredictionItem(item), vector, _ctx.Random.Next(4), location);
                }

                if (flag && drop.ApplyGenerousEnchantment && random.NextBool())
                {
                    var item1 = new PredictionItem(ItemQueryResolver.ApplyItemFields(item.getOne(), drop, context));
                    if (drop.OneDebrisPerDrop && item1.Stack > 1)
                    {
                        CreateItemExtentions.Predict_createMultipleItemDebris(_ctx, item1, vector, -1, location);
                    }
                    else
                    {
                        CreateItemExtentions.Predict_createItemDebris(_ctx, item1, vector, -1, location);
                    }
                }

                if (!drop.ContinueOnDrop)
                {
                    break;
                }
            }
        }
    }
}

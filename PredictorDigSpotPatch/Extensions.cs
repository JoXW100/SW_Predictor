using StardewValley.Enchantments;
using StardewValley.Extensions;
using StardewValley.Internal;
using StardewValley.Tools;
using StardewValley.GameData.Locations;
using StardewValley;
using StardewModdingAPI;
using Microsoft.Xna.Framework;
using PredictorPatchFramework;
using PredictorPatchFramework.Extensions;
using Object = StardewValley.Object;
using StardewValley.Locations;

namespace PredictorDigSpotPatch
{
    internal static class Extensions
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
                CreateItemExtensions.Predict_createMultipleItemDebris(_ctx, PredictionItem.Create("(O)Book_Defense"), pixelOrigin, Utility.GetOppositeFacingDirection(who.FacingDirection), location);
            }

            if (spot.QualifiedItemId == "(O)SeedSpot")
            {
                var seedItem = new PredictionItem(Utility.getRaccoonSeedForCurrentTimeOfYear(who, random));
                CreateItemExtensions.Predict_createMultipleItemDebris(_ctx, seedItem, pixelOrigin, Utility.GetOppositeFacingDirection(who.FacingDirection), location);
            }
            else
            {
                location.Predict_digUpArtifactSpot(_ctx, (int)tileLocation.X, (int)tileLocation.Y, who);
            }
        }

        public static void Predict_digUpArtifactSpot(this GameLocation location, PredictionContext _ctx, int xLocation, int yLocation, Farmer? who)
        {
            if (location is DesertFestival desert)
            {
                var r = Utility.CreateDaySaveRandom((double)(xLocation * 2000), (double)yLocation, 0.0);
                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "CalicoEgg", xLocation, yLocation, r.Next(3, 7), who!.UniqueMultiplayerID, location);
            }
            else if (location is IslandLocation island)
            {
                Random r = Utility.CreateDaySaveRandom((double)(xLocation * 2000), (double)yLocation, 0.0);
                string? toDigUp = null;
                int stack = 1;
                if (Game1.netWorldState.Value.GoldenCoconutCracked && r.NextDouble() < 0.1)
                {
                    toDigUp = "(O)791";
                }
                else if (r.NextDouble() < 0.33)
                {
                    toDigUp = "(O)831";
                    stack = r.Next(2, 5);
                }
                else if (r.NextDouble() < 0.15)
                {
                    toDigUp = "(O)275";
                    stack = r.Next(1, 3);
                }
                if (toDigUp != null)
                {
                    for (int i = 0; i < stack; i++)
                    {
                        CreateItemExtensions.Predict_createItemDebris(_ctx, PredictionItem.Create(toDigUp, 1, 0), new Vector2((float)xLocation, (float)yLocation) * 64f, -1, location, -1, false);
                    }
                }
            }

            var random = Utility.CreateDaySaveRandom(xLocation * 2000, yLocation, Game1.netWorldState.Value.TreasureTotemsUsed * 777);
            var tilePixelPos = new Vector2(xLocation * 64, yLocation * 64);
            var hasGenerousEnchantment = who?.CurrentTool is Hoe hoe && hoe.hasEnchantmentOfType<GenerousEnchantment>();
            var locationData = location.GetData();
            var context = new ItemQueryContext(location, who, random, "location '" + location.NameOrUniqueName + "' > artifact spots");
            var possibleDrops = Game1.locationData["Default"].ArtifactSpots.AsEnumerable();
            if (locationData?.ArtifactSpots != null && locationData.ArtifactSpots.Count > 0)
            {
                possibleDrops = possibleDrops.Concat(locationData.ArtifactSpots);
            }

            possibleDrops = possibleDrops.OrderBy(p => p.Precedence);
            if (Game1.player.mailReceived.Contains("sawQiPlane") && random.NextDouble() < 0.05 + Game1.player.team.AverageDailyLuck() / 2.0)
            {
                CreateItemExtensions.Predict_createMultipleItemDebris(_ctx, PredictionItem.Create("(O)MysteryBox", random.Next(1, 3)), tilePixelPos, -1, location);
            }

            CreateItemExtensions.Predict_trySpawnRareObject(_ctx, who, tilePixelPos, location, 9.0, 1.0, -1, random);
            foreach (ArtifactSpotDropData drop in possibleDrops)
            {
                if (!random.NextBool(drop.Chance) || (drop.Condition != null && !GameStateQuery.CheckConditions(drop.Condition, location, who, null, null, random)))
                {
                    continue;
                }

                Item item = ItemQueryResolver.TryResolveRandomItem(drop, context, avoidRepeat: false, logError: (string query, string error) =>
                {
                    ModLog.Log($"Location '{location.NameOrUniqueName}' failed parsing item query '{query}' for artifact spot '{drop.Id}': {error}", LogLevel.Warn);
                });

                if (item == null)
                {
                    continue;
                }

                if (drop.OneDebrisPerDrop && item.Stack > 1)
                {
                    CreateItemExtensions.Predict_createMultipleItemDebris(_ctx, new PredictionItem(item), tilePixelPos, -1, location);
                }
                else
                {
                    CreateItemExtensions.Predict_createItemDebris(_ctx, new PredictionItem(item), tilePixelPos, _ctx.Random.Next(4), location);
                }

                if (hasGenerousEnchantment && drop.ApplyGenerousEnchantment && random.NextBool())
                {
                    item = item.getOne();
                    item = (Item)ItemQueryResolver.ApplyItemFields(item, drop, context);
                    var item1 = new PredictionItem(item);
                    if (drop.OneDebrisPerDrop && item1.Stack > 1)
                    {
                        CreateItemExtensions.Predict_createMultipleItemDebris(_ctx, item1, tilePixelPos, -1, location);
                    }
                    else
                    {
                        CreateItemExtensions.Predict_createItemDebris(_ctx, item1, tilePixelPos, -1, location);
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

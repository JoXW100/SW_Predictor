using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Constants;
using StardewValley.GameData.WildTrees;
using StardewValley.TerrainFeatures;
using StardewValley.Extensions;
using Microsoft.Xna.Framework;
using System.Reflection;
using PredictorPatchFramework;
using PredictorPatchFramework.Extensions;

namespace PredictorTreePatch
{
    internal static class Extensions
    {
        public static Item? TryGetDrop(this Tree tree, WildTreeItemData drop, Random r, Farmer targetFarmer, string fieldName, Func<string, string>? formatItemId = null, bool? isStump = null)
        {
            var method = tree.GetType().GetMethod("TryGetDrop", BindingFlags.NonPublic | BindingFlags.Instance);
            return method?.Invoke(tree, new object?[] { drop, r, targetFarmer, fieldName, formatItemId, isStump }) as Item;
        }

        public static void Predict_shake(this Tree tree, PredictionContext _ctx, Vector2 tileLocation, bool doEvenIfStillShaking)
        {
            GameLocation location = tree.Location;
            WildTreeData data = tree.GetData();
            var localSeason = tree.Location.GetSeason();
            if ((tree.maxShake == 0f || doEvenIfStillShaking) && tree.growthStage.Value >= 3 && !tree.stump.Value)
            {
                var shakeLeft = Game1.player.StandingPixel.X > (tileLocation.X + 0.5f) * 64f || (Game1.player.Tile.X == tileLocation.X && _ctx.Random.NextBool());
                var maxShake = (float)((tree.growthStage.Value >= 5) ? (Math.PI / 128.0) : (Math.PI / 64.0));
                if (tree.growthStage.Value >= 5)
                {
                    if (tree.IsLeafy())
                    {
                        if (_ctx.Random.NextDouble() < 0.66)
                        {
                            int num = _ctx.Random.Next(1, 6);
                            for (int i = 0; i < num; i++)
                            {
                                var leaf = new Leaf(new Vector2(_ctx.Random.Next((int)(tileLocation.X * 64f - 64f), (int)(tileLocation.X * 64f + 128f)), _ctx.Random.Next((int)(tileLocation.Y * 64f - 256f), (int)(tileLocation.Y * 64f - 192f))), (float)_ctx.Random.Next(-10, 10) / 100f, _ctx.Random.Next(4), (float)_ctx.Random.Next(5) / 10f);
                                // tree.leaves.Add(leaf)
                            }
                        }
                        if (_ctx.Random.NextDouble() < 0.01 && (localSeason == Season.Spring || localSeason == Season.Summer))
                        {
                            bool islandButterfly = tree.Location.InIslandContext();
                            while (_ctx.Random.NextDouble() < 0.8)
                            {
                                var creature = new Butterfly(location, new Vector2(tileLocation.X + _ctx.Random.Next(1, 3), tileLocation.Y - 2f + _ctx.Random.Next(-1, 2)), islandButterfly);
                                // location.addCritter(creature)
                            }
                        }
                    }

                    if (tree.hasSeed.Value && (Game1.IsMultiplayer || Game1.player.ForagingLevel >= 1))
                    {
                        bool flag = true;
                        if (data != null && data.SeedDropItems?.Count > 0)
                        {
                            foreach (WildTreeSeedDropItemData seedDropItem in data.SeedDropItems)
                            {
                                Item? item = tree.TryGetDrop(seedDropItem, _ctx.Random, Game1.player, "SeedDropItems");
                                if (item != null)
                                {
                                    if (Game1.player.professions.Contains(16) && item.HasContextTag("forage_item"))
                                    {
                                        item.Quality = 4;
                                    }

                                    CreateItemExtensions.Predict_createItemDebris(_ctx, new PredictionItem(item), new Vector2(tileLocation.X * 64f, (tileLocation.Y - 3f) * 64f), -1, location, Game1.player.StandingPixel.Y);
                                    if (!seedDropItem.ContinueOnDrop)
                                    {
                                        flag = false;
                                        break;
                                    }
                                }
                            }
                        }

                        if (flag && data != null)
                        {
                            Item item2 = ItemRegistry.Create(data.SeedItemId);
                            if (Game1.player.professions.Contains(16) && item2.HasContextTag("forage_item"))
                            {
                                item2.Quality = 4;
                            }

                            CreateItemExtensions.Predict_createItemDebris(_ctx, new PredictionItem(item2), new Vector2(tileLocation.X * 64f, (tileLocation.Y - 3f) * 64f), -1, location, Game1.player.StandingPixel.Y);
                        }

                        if (Utility.tryRollMysteryBox(0.03))
                        {
                            CreateItemExtensions.Predict_createItemDebris(_ctx, PredictionItem.Create((Game1.player.stats.Get(StatKeys.Mastery(2)) != 0) ? "(O)GoldenMysteryBox" : "(O)MysteryBox"), new Vector2(tileLocation.X, tileLocation.Y - 3f) * 64f, -1, location, Game1.player.StandingPixel.Y - 32);
                        }

                        CreateItemExtensions.Predict_trySpawnRareObject(_ctx, Game1.player, new Vector2(tileLocation.X, tileLocation.Y - 3f) * 64f, tree.Location, 2.0, 1.0, Game1.player.StandingPixel.Y - 32);
                        if (_ctx.Random.NextBool() && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
                        {
                            CreateItemExtensions.Predict_createObjectDebris(_ctx, "(O)890", (int)tileLocation.X, (int)tileLocation.Y - 3, ((int)tileLocation.Y + 1) * 64, 0, 1f, location);
                        }

                        // tree.hasSeed.Value = false;
                    }

                    if (tree.wasShakenToday.Value)
                    {
                        return;
                    }

                    // tree.wasShakenToday.Value = true;
                    if (data?.ShakeItems == null)
                    {
                        return;
                    }

                    {
                        foreach (WildTreeItemData shakeItem in data.ShakeItems)
                        {
                            Item? item3 = tree.TryGetDrop(shakeItem, _ctx.Random, Game1.player, "ShakeItems");
                            if (item3 != null)
                            {
                                CreateItemExtensions.Predict_createItemDebris(_ctx, new PredictionItem(item3), tileLocation * 64f, -2, tree.Location);
                            }
                        }

                        return;
                    }
                }
                /*
                if (_ctx.Random.NextDouble() < 0.66)
                {
                    int num2 = _ctx.Random.Next(1, 3);
                    for (int j = 0; j < num2; j++)
                    {
                        var leaf = new Leaf(new Vector2(_ctx.Random.Next((int)(tileLocation.X * 64f), (int)(tileLocation.X * 64f + 48f)), tileLocation.Y * 64f - 32f), (float)_ctx.Random.Next(-10, 10) / 100f, _ctx.Random.Next(4), (float)_ctx.Random.Next(30) / 10f);
                        // leaves.Add(leaf);
                    }
                }
                */
            }
            /*
            else if (tree.stump.Value)
            {
                // tree.shakeTimer = 100f;
            }
            */
        }
    }
}

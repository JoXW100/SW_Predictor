using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Constants;
using StardewValley.GameData;
using StardewValley.Monsters;
using StardewValley.Objects.Trinkets;

namespace PredictorPatchFramework.Extensions
{
    public static class CreateItemExtensions
    {
        public static void Predict_createMultipleItemDebris(PredictionContext _ctx, PredictionItem? item, Vector2 pixelOrigin, int direction, GameLocation? location = null, int groundLevel = -1, bool flopFish = false)
        {
            int stack = 0;
            if (item != null)
            {
                stack = item.Stack;
                item.Stack = 1;
            }

            Predict_createItemDebris(_ctx, item, pixelOrigin, direction == -1 ? _ctx.Random.Next(4) : direction, location, groundLevel, flopFish);
            for (int i = 1; i < stack; i++)
            {
                Predict_createItemDebris(_ctx, item?.GetOne(), pixelOrigin, direction == -1 ? _ctx.Random.Next(4) : direction, location, groundLevel, flopFish);
            }
        }

        public static Debris? Predict_createItemDebris(PredictionContext _ctx, PredictionItem? item, Vector2 pixelOrigin, int direction, GameLocation? location = null, int groundLevel = -1, bool flopFish = false)
        {
            _ctx.AddItemIfNotNull(item);
            return null;
        }

        public static void Predict_createMultipleObjectDebris(PredictionContext _ctx, string id, int xTile, int yTile, int number)
        {
            for (int i = 0; i < number; i++)
            {
                Predict_createObjectDebris(_ctx, id, xTile, yTile);
            }
        }

        public static void Predict_createMultipleObjectDebris(PredictionContext _ctx, string id, int xTile, int yTile, int number, GameLocation location)
        {
            for (int i = 0; i < number; i++)
            {
                Predict_createObjectDebris(_ctx, id, xTile, yTile, -1, 0, 1f, location);
            }
        }

        public static void Predict_createMultipleObjectDebris(PredictionContext _ctx, string id, int xTile, int yTile, int number, long who, GameLocation location)
        {
            for (int i = 0; i < number; i++)
            {
                Predict_createObjectDebris(_ctx, id, xTile, yTile, who, location);
            }
        }

        public static void Predict_createObjectDebris(PredictionContext _ctx, string id, int xTile, int yTile, GameLocation location)
        {
            Predict_createObjectDebris(_ctx, id, xTile, yTile, -1, 0, 1f, location);
        }

        public static void Predict_createObjectDebris(PredictionContext _ctx, string id, int xTile, int yTile, long whichPlayer, GameLocation location)
        {
            _ctx.AddItemIfNotNull(id);
        }

        public static void Predict_createObjectDebris(PredictionContext _ctx, string id, int xTile, int yTile, int groundLevel = -1, int itemQuality = 0, float velocityMultiplyer = 1f, GameLocation? location = null)
        {
            _ctx.AddItemIfNotNull(id, quality: itemQuality);
        }

        public static void Predict_createDebris(PredictionContext _ctx, int debrisType, int xTile, int yTile, int numberOfChunks, GameLocation location)
        {
            var itemId = debrisType switch
            {
                0 or 378 => "(O)378",
                2 or 380 => "(O)380",
                4 or 382 => "(O)382",
                6 or 384 => "(O)384",
                10 or 386 => "(O)386",
                12 or 388 => "(O)388",
                14 or 390 => "(O)390",
                _ => "(O)" + debrisType
            };

            _ctx.AddItemIfNotNull(itemId, stack: numberOfChunks);
        }

        public static void Predict_TrySpawnTrinket(PredictionContext _ctx, GameLocation? location, Monster? monster, Vector2 spawnPosition, double chanceModifier = 1.0)
        {
            if (!Trinket.CanSpawnTrinket(Game1.player))
            {
                return;
            }

            double num = 0.004;
            if (monster != null)
            {
                num += (double)monster.MaxHealth * 1E-05;
                if (monster.isGlider.Value && monster.MaxHealth >= 150)
                {
                    num += 0.002;
                }

                if (monster is Leaper)
                {
                    num -= 0.005;
                }
            }

            num = Math.Min(0.025, num);
            num += Game1.player.DailyLuck / 25.0;
            num += (double)((float)Game1.player.LuckLevel * 0.00133f);
            num *= chanceModifier;
            if (_ctx.Random.NextDouble() < num)
            {
                Predict_SpawnTrinket(_ctx, location, spawnPosition);
            }
        }

        public static void Predict_SpawnTrinket(PredictionContext _ctx, GameLocation? location, Vector2 spawnPoint)
        {
            var randomTrinket = Predict_GetRandomTrinket(_ctx);
            if (randomTrinket != null)
            {
                Predict_createItemDebris(_ctx, randomTrinket, spawnPoint, _ctx.Random.Next(4), location);
            }
        }

        public static PredictionItem Predict_GetRandomTrinket(PredictionContext _ctx)
        {
            Dictionary<string, TrinketData> dictionary = DataLoader.Trinkets(Game1.content);
            PredictionItem? trinket = null;
            while (trinket == null)
            {
                int num = _ctx.Random.Next(dictionary.Count);
                int num2 = 0;
                foreach (TrinketData value in dictionary.Values)
                {
                    if (num == num2 && value.DropsNaturally)
                    {
                        trinket = PredictionItem.Create("(TR)" + value.Id);
                        break;
                    }

                    num2++;
                }
            }

            return trinket;
        }

        public static bool Predict_trySpawnRareObject(PredictionContext _ctx, Farmer? who, Vector2 position, GameLocation location, double chanceModifier = 1.0, double dailyLuckWeight = 1.0, int groundLevel = -1, Random? random = null)
        {
            if (random is null)
            {
                random = _ctx.Random;
            }

            double luckMod = 1.0;
            if (who != null)
            {
                luckMod = 1.0 + who.team.AverageDailyLuck() * dailyLuckWeight;
            }

            if (who != null && who.stats.Get(StatKeys.Mastery(0)) != 0 && random.NextDouble() < 0.001 * chanceModifier * luckMod)
            {
                Predict_createItemDebris(_ctx, PredictionItem.Create("(O)GoldenAnimalCracker"), position, -1, location, groundLevel);
            }

            if (Game1.stats.DaysPlayed > 2 && random.NextDouble() < 0.002 * chanceModifier)
            {
                Predict_createItemDebris(_ctx, new PredictionItem(Utility.getRandomCosmeticItem(_ctx.Random)), position, -1, location, groundLevel);
            }

            if (Game1.stats.DaysPlayed > 2 && random.NextDouble() < 0.0006 * chanceModifier)
            {
                Predict_createItemDebris(_ctx, PredictionItem.Create("(O)SkillBook_" + _ctx.Random.Next(5)), position, -1, location, groundLevel);
            }

            return false;
        }

        public static void Predict_RequestLimitedNutDrops(this FarmerTeam team, PredictionContext _ctx, string key, GameLocation? location, int x, int y, int limit, int rewardAmount = 1)
        {
            if (!team.limitedNutDrops.TryGetValue(key, out var value) || value < limit)
            {
                if (location != null)
                {
                    for (int i = 0; i < rewardAmount; i++)
                    {
                        Predict_createItemDebris(_ctx, PredictionItem.Create("(O)73"), new Vector2(x, y), -1, location);
                    }
                }
            }
        }
    }
}

using StardewValley.Monsters;
using StardewValley;
using Microsoft.Xna.Framework;
using StardewValley.Objects;
using StardewValley.GameData;

namespace Predictor.Framework.Extentions
{
    internal static class TrinketExtentions
    {
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
                Game1Extentions.Predict_createItemDebris(_ctx, randomTrinket, spawnPoint, _ctx.Random.Next(4), location);
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
                        trinket = PredictionItem.Create("(TR)" + value.ID);
                        break;
                    }

                    num2++;
                }
            }

            return trinket;
        }
    }
}

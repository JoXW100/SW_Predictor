using StardewValley.Constants;
using StardewValley.Enchantments;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley;
using StardewValley.Tools;

namespace Predictor.Framework.Extentions
{
    internal static class PanExtentions
    {
        public static void Predict_getPanItems(this Pan pan, PredictionContext _ctx, GameLocation location, Farmer who)
        {
            string text = "378";
            // who.stats.Increment("TimesPanned", 1);
            Random random = Utility.CreateRandom(location.orePanPoint.X, location.orePanPoint.Y * 1000.0, Game1.stats.DaysPlayed, (who.stats.Get("TimesPanned") + 1) * 77);
            double num = random.NextDouble() - who.LuckLevel * 0.001 - who.DailyLuck;
            num -= (double)((int)pan.UpgradeLevel - 1) * 0.05;
            if (num < 0.01)
            {
                text = "386";
            }
            else if (num < 0.241)
            {
                text = "384";
            }
            else if (num < 0.6)
            {
                text = "380";
            }

            if (text != "386" && random.NextDouble() < 0.1 + (pan.hasEnchantmentOfType<ArchaeologistEnchantment>() ? 0.1 : 0.0))
            {
                text = "881";
            }

            int num2 = random.Next(2, 7) + 1 + (int)((random.NextDouble() + 0.1 + (double)(who.LuckLevel / 10f) + who.DailyLuck) * 2.0);
            int num3 = random.Next(5) + 1 + (int)((random.NextDouble() + 0.1 + (double)(who.LuckLevel / 10f)) * 2.0);
            num2 += (int)pan.UpgradeLevel - 1;
            num = random.NextDouble() - who.DailyLuck;
            int num4 = pan.UpgradeLevel;
            bool flag = false;
            double num5 = (double)((int)pan.UpgradeLevel - 1) * 0.04;
            if (pan.enchantments.Count > 0)
            {
                num5 *= 1.25;
            }

            if (pan.hasEnchantmentOfType<GenerousEnchantment>())
            {
                num4 += 2;
            }

            while (random.NextDouble() - who.DailyLuck < 0.4 + who.LuckLevel * 0.04 + num5 && num4 > 0)
            {
                num = random.NextDouble() - who.DailyLuck;
                num -= (pan.UpgradeLevel - 1) * 0.005;
                string text2 = "382";
                if (num < 0.02 + who.LuckLevel * 0.002 && random.NextDouble() < 0.75)
                {
                    text2 = "72";
                    num3 = 1;
                }
                else if (num < 0.1 && random.NextDouble() < 0.75)
                {
                    text2 = (60 + random.Next(5) * 2).ToString();
                    num3 = 1;
                }
                else if (num < 0.36)
                {
                    text2 = "749";
                    num3 = Math.Max(1, num3 / 2);
                }
                else if (num < 0.5)
                {
                    text2 = random.Choose("82", "84", "86");
                    num3 = 1;
                }

                if (num < (double)who.LuckLevel * 0.002 && !flag && random.NextDouble() < 0.33)
                {
                    _ctx.AddItemIfNotNull(new Ring("859"));
                    flag = true;
                }

                if (num < 0.01 && random.NextDouble() < 0.5)
                {
                    _ctx.AddItemIfNotNull(Utility.getRandomCosmeticItem(random));
                }

                if (random.NextDouble() < 0.1 && pan.hasEnchantmentOfType<FisherEnchantment>())
                {
                    Item fish = location.getFish(1f, null, random.Next(1, 6), who, 0.0, who.Tile);
                    if (fish != null && fish.Category == -4)
                    {
                        _ctx.AddItemIfNotNull(fish);
                    }
                }

                if (random.NextDouble() < 0.02 + (pan.hasEnchantmentOfType<ArchaeologistEnchantment>() ? 0.05 : 0.0))
                {
                    Item item = location.tryGetRandomArtifactFromThisLocation(who, random);
                    if (item != null)
                    {
                        _ctx.AddItemIfNotNull(item);
                    }
                }

                if (Utility.tryRollMysteryBox(0.05, random))
                {
                    _ctx.AddItemIfNotNull(ItemRegistry.Create((Game1.player.stats.Get(StatKeys.Mastery(2)) != 0) ? "(O)GoldenMysteryBox" : "(O)MysteryBox"));
                }

                if (text2 != null)
                {
                    _ctx.AddItemIfNotNull(text2, num3);
                }

                num4--;
            }

            int num6 = 0;
            while (random.NextDouble() < 0.05 + (pan.hasEnchantmentOfType<ArchaeologistEnchantment>() ? 0.15 : 0.0))
            {
                num6++;
            }

            if (num6 > 0)
            {
                _ctx.AddItemIfNotNull("(O)275", num6);
            }

            _ctx.AddItemIfNotNull(text, num2);
            if (location is IslandNorth islandNorth && islandNorth.bridgeFixed.Value && random.NextDouble() < 0.2)
            {
                _ctx.AddItemIfNotNull("(O)822");
            }
            else if (location is IslandLocation && random.NextDouble() < 0.2)
            {
                _ctx.AddItemIfNotNull("(O)831", random.Next(2, 6));
            }
        }
    }
}

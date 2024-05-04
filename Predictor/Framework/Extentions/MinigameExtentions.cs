using Force.DeepCloner;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Minigames;
using System.Reflection;

namespace Predictor.Framework.Extentions
{
    public static class MinigameExtentions
    {
        public static float Predict_slotResults(this Slots slots, PredictionContext _ctx)
        {
            double num = _ctx.Random.NextDouble();
            double num2 = 1.0 + Game1.player.DailyLuck * 2.0 + (double)Game1.player.LuckLevel * 0.08;
            if (num < 0.001 * num2)
            {
                return 2500f;
            }

            if (num < 0.0016 * num2)
            {
                return 1000f;
            }

            if (num < 0.0025 * num2)
            {
                return 500f;
            }

            if (num < 0.005 * num2)
            {
                return 200f;
            }

            if (num < 0.007 * num2)
            {
                return 120f;
            }

            if (num < 0.01 * num2)
            {
                return 80f;
            }

            if (num < 0.02 * num2)
            {
                return 30f;
            }

            if (num < 0.12 * num2)
            {
                int num3 = _ctx.Random.Next(3);
                for (int i = 0; i < 3; i++)
                {
                    var _ = ((i == num3) ? _ctx.Random.Next(7) : 7);
                }

                return 3f;
            }

            if (num < 0.2 * num2)
            {
                return 5f;
            }

            if (num < 0.4 * num2)
            {
                int num4 = _ctx.Random.Next(3);
                for (int j = 0; j < 3; j++)
                {
                    var _ = ((j == num4) ? 7 : _ctx.Random.Next(7));
                }

                return 2f;
            }

            int[] array = new int[8];
            for (int k = 0; k < 3; k++)
            {
                int num5 = _ctx.Random.Next(6);
                while (array[num5] > 1)
                {
                    num5 = _ctx.Random.Next(6);
                }

                array[num5]++;
            }

            return 0f;
        }
    
        public static Random? CloneCurrentRandom(this CalicoJack game)
        {
            // Copy random field
            var r_0 = game.GetType().GetField("r", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(game) as Random;
            return r_0?.DeepClone();
        }

        public static int PredictCalicoJackHitCard(this CalicoJack game, PredictionContext _ctx)
        {
            var r = _ctx.Random;
            int currentCardTotal = 0;
            foreach (int[] playerCard in game.playerCards)
            {
                currentCardTotal += playerCard[0];
            }

            int num2 = r.Next(1, 10);
            int num3 = 21 - currentCardTotal;
            if (num3 > 1 && num3 < 6 && r.NextDouble() < (double)(1f / (float)num3))
            {
                num2 = r.Choose(num3, num3 - 1);
            }

            return num2;
        }

        public static int PredictCalicoJackNextDealerCard(this CalicoJack game, PredictionContext _ctx)
        {
            var r = _ctx.Random;
            int currentDealerCardTotal = 0;
            foreach (int[] dealerCard in game.dealerCards)
            {
                currentDealerCardTotal += dealerCard[0];
            }

            int currentPlayerCardTotal = 0;
            foreach (int[] playerCard in game.playerCards)
            {
                currentPlayerCardTotal += playerCard[0];
            }

            int num4 = r.Next(1, 10);
            int num5 = 21 - currentDealerCardTotal;
            if (currentPlayerCardTotal == 20 && r.NextBool())
            {
                num4 = num5 + r.Next(1, 4);
            }
            else if (currentPlayerCardTotal == 19 && r.NextDouble() < 0.25)
            {
                num4 = num5 + r.Next(1, 4);
            }
            else if (currentPlayerCardTotal == 18 && r.NextDouble() < 0.1)
            {
                num4 = num5 + r.Next(1, 4);
            }

            // ???
            if (r.NextDouble() < Math.Max(0.0005, 0.001 + Game1.player.DailyLuck / 20.0 + (double)((float)Game1.player.LuckLevel * 0.002f)))
            {
                num4 = 999;
                // currentBet *= 3;
            }

            return num4;
        }
    
        public static bool PredictCalicoJackStandResult(this CalicoJack game, PredictionContext _ctx)
        {
            int currentDealerCardTotal = 0;
            foreach (int[] dealerCard in game.dealerCards)
            {
                currentDealerCardTotal += dealerCard[0];
            }

            int currentPlayerCardTotal = 0;
            foreach (int[] playerCard in game.playerCards)
            {
                currentPlayerCardTotal += playerCard[0];
            }

            while (currentDealerCardTotal < currentPlayerCardTotal)
            {
                currentDealerCardTotal += game.PredictCalicoJackNextDealerCard(_ctx);
            }

            return currentDealerCardTotal > 21;
        }
    }
}

using StardewValley;
using Microsoft.Xna.Framework;

namespace Predictor.Framework.Extentions
{
    internal static class FarmerTeamExtentions
    {
        public static void Predict_RequestLimitedNutDrops(this FarmerTeam team, PredictionContext _ctx, string key, GameLocation? location, int x, int y, int limit, int rewardAmount = 1)
        {
            if (!team.limitedNutDrops.TryGetValue(key, out var value) || value < limit)
            {
                if (location != null)
                {
                    for (int i = 0; i < rewardAmount; i++)
                    {
                        Game1Extentions.Predict_createItemDebris(_ctx, PredictionItem.Create("(O)73"), new Vector2(x, y), -1, location);
                    }
                }
            }
        }
    }
}

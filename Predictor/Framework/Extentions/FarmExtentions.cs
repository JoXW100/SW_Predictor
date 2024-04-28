using Microsoft.Xna.Framework;
using StardewValley;
using System.Reflection;

namespace Predictor.Framework.Extentions
{
    internal static class FarmExtentions
    {
        public static void Predict_getAllFarmFish(this Farm farm, PredictionContext _ctx, float millisecondsAfterNibble, string? bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string? locationName = null)
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
            if (_ctx.Properties.TryGetValue(PredictionProperty.ChanceBase, out var c))
            {
                baseChance = (float)c;
            }

            if (_fishChanceOverride > 0f && _ctx.Random.NextDouble() < _fishChanceOverride)
            {
                _ctx.Properties[PredictionProperty.ChanceBase] = _fishChanceOverride * baseChance;
                GameLocation? location = Game1.getLocationFromName(_fishLocationOverride);
                if (location != null)
                {
                    GameLocationExtention.Predict_getAllFish(location, _ctx, millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile);
                    return;
                }
                // return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, _fishLocationOverride);
            }
            else
            {
                _ctx.Properties[PredictionProperty.ChanceBase] = (1 - _fishChanceOverride) * baseChance;
            }
            
            // return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile);
        }
    }
}

using StardewValley.GameData.Characters;
using StardewValley.Characters;
using StardewValley;
using Microsoft.Xna.Framework;
using PredictorPatchFramework;
using PredictorPatchFramework.Extensions;

namespace PredictorGarbageCanPatch
{
    internal static class Extensions
    {
        public static bool Predict_CheckGarbage(this GameLocation location, PredictionContext _ctx, string id, Vector2 tile, Farmer who, bool reactNpcs = true)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            id = id switch
            {
                "0" => "JodiAndKent",
                "1" => "EmilyAndHaley",
                "2" => "Mayor",
                "3" => "Museum",
                "4" => "Blacksmith",
                "5" => "Saloon",
                "6" => "Evelyn",
                "7" => "JojaMart",
                _ => id
            };

            if (Game1.netWorldState.Value.CheckedGarbage.Overlaps(new[] { id }))
            {
                _ctx.Properties.Add("exhausted", true);
                return true;
            }

            location.TryGetGarbageItem(id, who.DailyLuck, out var itemResult, out var selected, out var garbageRandom);
            var item = itemResult == null ? null : new PredictionItem(itemResult);

            if (reactNpcs)
            {
                List<NPC> reactions = new List<NPC>();
                foreach (NPC npc in Utility.GetNpcsWithinDistance(tile, 7, location))
                {
                    if (npc is not Horse)
                    {
                        CharacterData data = npc.GetData();
                        if (data == null || data.DumpsterDiveFriendshipEffect < 0)
                        {
                            reactions.Add(npc);
                        }
                    }
                }

                _ctx.Properties.Add("affected", reactions);
            }

            if (selected != null)
            {
                if (selected.AddToInventoryDirectly)
                {
                    _ctx.AddItemIfNotNull(item);
                }
                else
                {
                    var pixelOrigin = new Vector2(tile.X + 0.5f, tile.Y - 1f) * 64f;
                    if (selected.CreateMultipleDebris)
                    {
                        CreateItemExtensions.Predict_createMultipleItemDebris(_ctx, item, pixelOrigin, 2, location, (int)pixelOrigin.Y + 64);
                    }
                    else
                    {
                        CreateItemExtensions.Predict_createItemDebris(_ctx, item, pixelOrigin, 2, location, (int)pixelOrigin.Y + 64);
                    }
                }
            }

            return true;
        }
    }
}

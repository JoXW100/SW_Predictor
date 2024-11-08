using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley;
using PredictorPatchFramework;
using PredictorPatchFramework.Extensions;
using StardewValley.Enchantments;
using StardewValley.Tools;
using Microsoft.Xna.Framework;

namespace PredictorTillablePatch
{
    internal static class Extensions
    {
        public static string Predict_checkForBuriedItem(this GameLocation location, PredictionContext _ctx, int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who)
        {
            if (location is MineShaft ms)
            {
                return Predict_checkForBuriedItem(ms, _ctx, xLocation, yLocation, explosion, detectOnly, who);
            }

            var random = Utility.CreateDaySaveRandom(xLocation * 2000, yLocation * 77, Game1.stats.DirtHoed);
            var text = location.Predict_HandleTreasureTileProperty(_ctx, xLocation, yLocation, detectOnly);
            if (text != null)
            {
                return text;
            }

            var flag = who?.CurrentTool is Hoe hoe && hoe.hasEnchantmentOfType<GenerousEnchantment>();
            var num = 0.5f;
            if (!location.IsFarm && location.IsOutdoors && location.GetSeason() is Season.Winter && random.NextDouble() < 0.08 && !explosion && !detectOnly && !(location is Desert))
            {
                CreateItemExtensions.Predict_createObjectDebris(_ctx, random.Choose("(O)412", "(O)416"), xLocation, yLocation);
                if (flag && random.NextDouble() < (double)num)
                {
                    CreateItemExtensions.Predict_createObjectDebris(_ctx, random.Choose("(O)412", "(O)416"), xLocation, yLocation);
                }

                return "";
            }

            var data = location.GetData();
            if (location.IsOutdoors && random.NextBool(data?.ChanceForClay ?? 0.03) && !explosion)
            {
                if (detectOnly)
                {
                    return "Item";
                }

                CreateItemExtensions.Predict_createObjectDebris(_ctx, "(O)330", xLocation, yLocation);
                if (flag && random.NextDouble() < (double)num)
                {
                    CreateItemExtensions.Predict_createObjectDebris(_ctx, "(O)330", xLocation, yLocation);
                }

                return "";
            }

            return "";
        }

        public static string Predict_checkForBuriedItem(this MineShaft location, PredictionContext _ctx, int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who)
        {
            if (location.isQuarryArea)
            {
                return "";
            }

            if (_ctx.Random.NextDouble() < 0.15)
            {
                string id = "(O)330";
                if (_ctx.Random.NextDouble() < 0.07)
                {
                    if (_ctx.Random.NextDouble() < 0.75)
                    {
                        switch (_ctx.Random.Next(5))
                        {
                            case 0:
                                id = "(O)96";
                                break;
                            case 1:
                                id = ((!who.hasOrWillReceiveMail("lostBookFound")) ? "(O)770" : ((Game1.netWorldState.Value.LostBooksFound < 21) ? "(O)102" : "(O)770"));
                                break;
                            case 2:
                                id = "(O)110";
                                break;
                            case 3:
                                id = "(O)112";
                                break;
                            case 4:
                                id = "(O)585";
                                break;
                        }
                    }
                    else if (_ctx.Random.NextDouble() < 0.75)
                    {
                        switch (location.getMineArea())
                        {
                            case 0:
                            case 10:
                                id = _ctx.Random.Choose("(O)121", "(O)97");
                                break;
                            case 40:
                                id = _ctx.Random.Choose("(O)122", "(O)336");
                                break;
                            case 80:
                                id = "(O)99";
                                break;
                        }
                    }
                    else
                    {
                        id = _ctx.Random.Choose("(O)126", "(O)127");
                    }
                }
                else if (_ctx.Random.NextDouble() < 0.19)
                {
                    id = (_ctx.Random.NextBool() ? "(O)390" : location.getOreIdForLevel(location.mineLevel, _ctx.Random));
                }
                else if (_ctx.Random.NextDouble() < 0.45)
                {
                    id = "(O)330";
                }
                else if (_ctx.Random.NextDouble() < 0.12)
                {
                    if (_ctx.Random.NextDouble() < 0.25)
                    {
                        id = "(O)749";
                    }
                    else
                    {
                        switch (location.getMineArea())
                        {
                            case 0:
                            case 10:
                                id = "(O)535";
                                break;
                            case 40:
                                id = "(O)536";
                                break;
                            case 80:
                                id = "(O)537";
                                break;
                        }
                    }
                }
                else
                {
                    id = "(O)78";
                }

                CreateItemExtensions.Predict_createObjectDebris(_ctx, id, xLocation, yLocation, who.UniqueMultiplayerID, location);
                bool num = who.CurrentTool is Hoe && who.CurrentTool.hasEnchantmentOfType<GenerousEnchantment>();
                if (num && _ctx.Random.NextDouble() < 0.25)
                {
                    CreateItemExtensions.Predict_createObjectDebris(_ctx, id, xLocation, yLocation, who.UniqueMultiplayerID, location);
                }

                return "";
            }

            return "";
        }

        private static string? Predict_HandleTreasureTileProperty(this GameLocation location, PredictionContext _ctx, int xLocation, int yLocation, bool detectOnly)
        {
            var tile = location.map.GetLayer("Back")?.Tiles[xLocation, yLocation];
            if (tile is null || !(tile.Properties.TryGetValue("Treasure", out var text) || tile.TileIndexProperties.TryGetValue("Treasure", out text)) || text is null)
            {
                return null;
            }

            var array = ArgUtility.SplitBySpace(text);
            if (!ArgUtility.TryGet(array, 0, out var value, out var error))
            {
                return null;
            }

            if (detectOnly)
            {
                return value;
            }

            switch (value)
            {
                case "Arch":
                    {
                        if (ArgUtility.TryGet(array, 1, out var id, out error))
                        {
                            CreateItemExtensions.Predict_createObjectDebris(_ctx, id, xLocation, yLocation, location);
                        }

                        break;
                    }
                case "CaveCarrot":
                    CreateItemExtensions.Predict_createObjectDebris(_ctx, "(O)78", xLocation, yLocation, location: location);
                    break;
                case "Coins":
                    CreateItemExtensions.Predict_createObjectDebris(_ctx, "(O)330", xLocation, yLocation, location: location);
                    break;
                case "Coal":
                case "Copper":
                case "Gold":
                case "Iridium":
                case "Iron":
                    {
                        int debrisType = value switch
                        {
                            "Coal" => 4,
                            "Copper" => 0,
                            "Gold" => 6,
                            "Iridium" => 10,
                            _ => 2,
                        };
                        if (ArgUtility.TryGetInt(array, 1, out var id, out error))
                        {
                            CreateItemExtensions.Predict_createDebris(_ctx, debrisType, xLocation, yLocation, id, location);
                        }

                        break;
                    }
                case "Object":
                    {
                        if (ArgUtility.TryGet(array, 1, out var id, out error))
                        {
                            CreateItemExtensions.Predict_createObjectDebris(_ctx, id, xLocation, yLocation, location: location);
                        }

                        break;
                    }
                case "Item":
                    {
                        if (ArgUtility.TryGet(array, 1, out var id, out error))
                        {
                            var item = PredictionItem.Create(id);
                            CreateItemExtensions.Predict_createItemDebris(_ctx, item, new Vector2(xLocation, yLocation), -1, location);
                        }
                        break;
                    }
                default:
                    value = null;
                    break;
            }

            return value;
        }
    }
}

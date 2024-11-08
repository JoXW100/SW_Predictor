using PredictorPatchFramework;
using StardewValley.Extensions;
using StardewValley;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace PredictorCrabPotPatch
{
    public static class Extensions
    {
        public static Random CreateNextDaySaveRandom(double seedA = 0.0, double seedB = 0.0, double seedC = 0.0)
        {
            return Utility.CreateRandom(Game1.stats.DaysPlayed + 1, Game1.uniqueIDForThisGame / 2, seedA, seedB, seedC);
        }

        public static bool IsBait(this Object obj)
        {
            return obj.Name.Contains("Bait");
        }

        public static Object? AsObject(this Item? item)
        {
            if (item is Object obj)
            {
                return obj;
            }
            return null;
        }

        public static PredictionItem? Predict_NextDayOutput(this CrabPot pot, Object? bait = null)
        {
            GameLocation location = pot.Location;
            bool flag = Game1.getFarmer(pot.owner.Value) != null && Game1.getFarmer(pot.owner.Value).professions.Contains(11);
            bool flag2 = Game1.getFarmer(pot.owner.Value) != null && Game1.getFarmer(pot.owner.Value).professions.Contains(10);
            if (pot.owner.Value == 0L && Game1.player.professions.Contains(11))
            {
                flag2 = true;
            }

            if (bait is null || !IsBait(bait))
            {
                bait = pot.bait.Value;
            }

            if (!(bait != null || flag) || pot.heldObject.Value != null)
            {
                return null;
            }

            Random random = CreateNextDaySaveRandom(pot.TileLocation.X * 1000f, pot.TileLocation.Y * 255f, pot.directionOffset.X * 1000f + pot.directionOffset.Y);
            Dictionary<string, string> dictionary = DataLoader.Fish(Game1.content);
            List<string> list = new();
            if (!location.TryGetFishAreaForTile(pot.TileLocation, out var _, out var data))
            {
                data = null;
            }

            double num = flag2 ? 0.0 : (((double?)data?.CrabPotJunkChance) ?? 0.2);
            int initialStack = 1;
            int num2 = 0;
            string? text = null;
            if (bait != null && bait.QualifiedItemId == "(O)DeluxeBait")
            {
                num2 = 1;
                num /= 2.0;
            }
            else if (bait != null && bait.QualifiedItemId == "(O)774")
            {
                num /= 2.0;
                if (random.NextBool(0.25))
                {
                    initialStack = 2;
                }
            }
            else if (bait != null && bait.Name.Contains("Bait") && bait.preservedParentSheetIndex.Value != null && bait.preserve.Value.HasValue)
            {
                text = bait.preservedParentSheetIndex.Value;
                num /= 2.0;
            }

            if (!random.NextBool(num))
            {
                IList<string> crabPotFishForTile = location.GetCrabPotFishForTile(pot.TileLocation);
                foreach (KeyValuePair<string, string> item in dictionary)
                {
                    if (!item.Value.Contains("trap"))
                    {
                        continue;
                    }

                    string[] array = item.Value.Split('/');
                    string[] array2 = ArgUtility.SplitBySpace(array[4]);
                    bool flag3 = false;
                    string[] array3 = array2;
                    foreach (string text2 in array3)
                    {
                        foreach (string item2 in crabPotFishForTile)
                        {
                            if (text2 == item2)
                            {
                                flag3 = true;
                                break;
                            }
                        }
                    }

                    if (!flag3)
                    {
                        continue;
                    }

                    if (flag2)
                    {
                        list.Add(item.Key);
                        continue;
                    }

                    double num3 = Convert.ToDouble(array[2]);
                    if (text != null && text == item.Key)
                    {
                        num3 *= (double)((num3 < 0.1) ? 4 : ((num3 < 0.2) ? 3 : 2));
                    }

                    if (!(random.NextDouble() < num3))
                    {
                        continue;
                    }

                    return PredictionItem.Create(item.Key, initialStack, num2);
                }
            }

            if (flag2 && list.Count > 0)
            {
                return PredictionItem.Create("(O)" + random.ChooseFrom(list));
            }
            else
            {
                return PredictionItem.Create("(O)" + random.Next(168, 173));
            }
        }
    }
}

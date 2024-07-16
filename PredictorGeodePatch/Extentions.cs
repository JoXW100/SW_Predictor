using StardewValley.Constants;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.Objects;
using StardewValley.Internal;
using PredictorPatchFramework;

namespace PredictorGeodePatch
{
    internal static class Extentions
    {
        public static PredictionItem? Predict_getTreasureFromGeode(Item geode)
        {
            if (!Utility.IsGeode(geode))
            {
                return null;
            }

            try
            {
                string qualifiedItemId = geode.QualifiedItemId;
                Random random = Utility.CreateRandom(qualifiedItemId.Contains("MysteryBox") ? Game1.stats.Get("MysteryBoxesOpened") + 1 : Game1.stats.GeodesCracked + 1, Game1.uniqueIDForThisGame / 2, (int)Game1.player.UniqueMultiplayerID / 2);
                int num = random.Next(1, 10);
                for (int i = 0; i < num; i++)
                {
                    random.NextDouble();
                }

                num = random.Next(1, 10);
                for (int j = 0; j < num; j++)
                {
                    random.NextDouble();
                }

                if (qualifiedItemId.Contains("MysteryBox"))
                {
                    if (Game1.stats.Get("MysteryBoxesOpened") > 10 || qualifiedItemId == "(O)GoldenMysteryBox")
                    {
                        double num2 = ((!(qualifiedItemId == "(O)GoldenMysteryBox")) ? 1 : 2);
                        if (qualifiedItemId == "(O)GoldenMysteryBox" && Game1.player.stats.Get(StatKeys.Mastery(0)) != 0 && random.NextBool(0.005))
                        {
                            return PredictionItem.Create("(O)GoldenAnimalCracker");
                        }

                        if (qualifiedItemId == "(O)GoldenMysteryBox" && random.NextBool(0.005))
                        {
                            return PredictionItem.Create("(BC)272");
                        }

                        if (random.NextBool(0.002 * num2))
                        {
                            return PredictionItem.Create("(O)279");
                        }

                        if (random.NextBool(0.004 * num2))
                        {
                            return PredictionItem.Create("(O)74");
                        }

                        if (random.NextBool(0.008 * num2))
                        {
                            return PredictionItem.Create("(O)166");
                        }

                        if (random.NextBool(0.01 * num2 + (Game1.player.mailReceived.Contains("GotMysteryBook") ? 0.0 : (0.0004 * (double)Game1.stats.Get("MysteryBoxesOpened")))))
                        {
                            if (!Game1.player.mailReceived.Contains("GotMysteryBook"))
                            {
                                // Game1.player.mailReceived.Add("GotMysteryBook");
                                return PredictionItem.Create("(O)Book_Mystery");
                            }

                            return PredictionItem.Create(random.Choose("(O)PurpleBook", "(O)Book_Mystery"));
                        }

                        if (random.NextBool(0.01 * num2))
                        {
                            return PredictionItem.Create(random.Choose("(O)797", "(O)373"));
                        }

                        if (random.NextBool(0.01 * num2))
                        {
                            return PredictionItem.Create("(H)MysteryHat");
                        }

                        if (random.NextBool(0.01 * num2))
                        {
                            return PredictionItem.Create("(S)MysteryShirt");
                        }

                        if (random.NextBool(0.01 * num2))
                        {
                            return PredictionItem.Create("(WP)MoreWalls:11");
                        }

                        if (random.NextBool(0.1) || qualifiedItemId == "(O)GoldenMysteryBox")
                        {
                            switch (random.Next(15))
                            {
                                case 0:
                                    return PredictionItem.Create("(O)288", 5);
                                case 1:
                                    return PredictionItem.Create("(O)253", 3);
                                case 2:
                                    if (Game1.player.GetUnmodifiedSkillLevel(1) >= 6 && random.NextBool())
                                    {
                                        return PredictionItem.Create(random.Choose("(O)687", "(O)695"));
                                    }

                                    return PredictionItem.Create("(O)242", 2);
                                case 3:
                                    return PredictionItem.Create("(O)204", 2);
                                case 4:
                                    return PredictionItem.Create("(O)369", 20);
                                case 5:
                                    return PredictionItem.Create("(O)466", 20);
                                case 6:
                                    return PredictionItem.Create("(O)773", 2);
                                case 7:
                                    return PredictionItem.Create("(O)688", 3);
                                case 8:
                                    return PredictionItem.Create("(O)" + random.Next(628, 634));
                                case 9:
                                    return PredictionItem.Create("(O)" + Crop.getRandomLowGradeCropForThisSeason(Game1.season), 20);
                                case 10:
                                    if (random.NextBool())
                                    {
                                        return PredictionItem.Create("(W)60");
                                    }

                                    return PredictionItem.Create(random.Choose("(O)533", "(O)534"));
                                case 11:
                                    return PredictionItem.Create("(O)621");
                                case 12:
                                    return PredictionItem.Create("(O)MysteryBox", random.Next(3, 5));
                                case 13:
                                    return PredictionItem.Create("(O)SkillBook_" + random.Next(5));
                                case 14:
                                    return new PredictionItem(Utility.getRaccoonSeedForCurrentTimeOfYear(Game1.player, random, 8));
                            }
                        }
                    }

                    switch (random.Next(14))
                    {
                        case 0:
                            return PredictionItem.Create("(O)395", 3);
                        case 1:
                            return PredictionItem.Create("(O)287", 5);
                        case 2:
                            return PredictionItem.Create("(O)" + Crop.getRandomLowGradeCropForThisSeason(Game1.season), 8);
                        case 3:
                            return PredictionItem.Create("(O)" + random.Next(727, 734));
                        case 4:
                            return PredictionItem.Create("(O)" + Utility.getRandomIntWithExceptions(random, 194, 240, new List<int> { 217 }));
                        case 5:
                            return PredictionItem.Create("(O)709", 10);
                        case 6:
                            return PredictionItem.Create("(O)369", 10);
                        case 7:
                            return PredictionItem.Create("(O)466", 10);
                        case 8:
                            return PredictionItem.Create("(O)688");
                        case 9:
                            return PredictionItem.Create("(O)689");
                        case 10:
                            return PredictionItem.Create("(O)770", 10);
                        case 11:
                            return PredictionItem.Create("(O)MixedFlowerSeeds", 10);
                        case 12:
                            if (random.NextBool(0.4))
                            {
                                return random.Next(4) switch
                                {
                                    0 => PredictionItem.Create("(O)525"),
                                    1 => PredictionItem.Create("(O)529"),
                                    2 => PredictionItem.Create("(O)888"),
                                    _ => PredictionItem.Create("(O)" + random.Next(531, 533)),
                                };
                            }

                            return PredictionItem.Create("(O)MysteryBox", 2);
                        case 13:
                            return PredictionItem.Create("(O)690");
                        default:
                            return PredictionItem.Create("(O)382");
                    }
                }

                if (random.NextBool(0.1) && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
                {
                    return PredictionItem.Create("(O)890", (!random.NextBool(0.25)) ? 1 : 5);
                }

                if (Game1.objectData.TryGetValue(geode.ItemId, out var value))
                {
                    List<ObjectGeodeDropData> geodeDrops = value.GeodeDrops;
                    if (geodeDrops != null && geodeDrops.Count > 0 && (!value.GeodeDropsDefaultItems || random.NextBool()))
                    {
                        foreach (ObjectGeodeDropData drop in value.GeodeDrops.OrderBy((ObjectGeodeDropData p) => p.Precedence))
                        {
                            if (!random.NextBool(drop.Chance) || (drop.Condition != null && !GameStateQuery.CheckConditions(drop.Condition, random: random)))
                            {
                                continue;
                            }

                            Item? item = ItemQueryResolver.TryResolveRandomItem(drop, new ItemQueryContext(null, null, random), avoidRepeat: false);
                            if (item != null)
                            {
                                if (drop.SetFlagOnPickup != null)
                                {
                                    item.SetFlagOnPickup = drop.SetFlagOnPickup;
                                }

                                return new PredictionItem(item);
                            }
                        }
                    }
                }

                int num3 = random.Next(3) * 2 + 1;
                if (random.NextBool(0.1))
                {
                    num3 = 10;
                }

                if (random.NextBool(0.01))
                {
                    num3 = 20;
                }

                if (random.NextBool())
                {
                    switch (random.Next(4))
                    {
                        case 0:
                        case 1:
                            return PredictionItem.Create("(O)390", num3);
                        case 2:
                            return PredictionItem.Create("(O)330");
                        default:
                            return qualifiedItemId switch
                            {
                                "(O)749" => PredictionItem.Create("(O)" + (82 + random.Next(3) * 2)),
                                "(O)535" => PredictionItem.Create("(O)86"),
                                "(O)536" => PredictionItem.Create("(O)84"),
                                _ => PredictionItem.Create("(O)82"),
                            };
                    }
                }

                if (!(qualifiedItemId == "(O)535"))
                {
                    if (qualifiedItemId == "(O)536")
                    {
                        return random.Next(4) switch
                        {
                            0 => PredictionItem.Create("(O)378", num3),
                            1 => PredictionItem.Create("(O)380", num3),
                            2 => PredictionItem.Create("(O)382", num3),
                            _ => PredictionItem.Create((Game1.player.deepestMineLevel > 75) ? "(O)384" : "(O)380", num3),
                        };
                    }

                    return random.Next(5) switch
                    {
                        0 => PredictionItem.Create("(O)378", num3),
                        1 => PredictionItem.Create("(O)380", num3),
                        2 => PredictionItem.Create("(O)382", num3),
                        3 => PredictionItem.Create("(O)384", num3),
                        _ => PredictionItem.Create("(O)386", num3 / 2 + 1),
                    };
                }

                return random.Next(3) switch
                {
                    0 => PredictionItem.Create("(O)378", num3),
                    1 => PredictionItem.Create((Game1.player.deepestMineLevel > 25) ? "(O)380" : "(O)378", num3),
                    _ => PredictionItem.Create("(O)382", num3),
                };
            }
            catch
            {
                // Game1.log.Error("Geode '" + geode?.QualifiedItemId + "' failed creating treasure.", exception);
            }

            return PredictionItem.Create("(O)390");
        }
    }
}

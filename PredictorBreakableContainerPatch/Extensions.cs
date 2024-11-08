using PredictorPatchFramework.Extensions;
using PredictorPatchFramework;
using StardewValley.Constants;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley;
using StardewValley.Objects;
using Microsoft.Xna.Framework;
using Object = StardewValley.Object;

namespace PredictorBreakableContainerPatch
{
    internal static class Extensions
    {
        public static bool IsBreakableContainer(this Object obj)
        {
            return obj is BreakableContainer || (!obj.isTemporarilyInvisible && obj.name.Contains("SupplyCrate"));
        }

        public static void Predict_releaseContents(this BreakableContainer container, PredictionContext _ctx, Farmer who)
        {
            GameLocation location = container.Location;
            if (location == null)
            {
                return;
            }

            Random random = Utility.CreateRandom(container.TileLocation.X, container.TileLocation.Y * 10000.0, Game1.stats.DaysPlayed, (location as MineShaft)?.mineLevel ?? 0);
            int num = (int)container.TileLocation.X;
            int num2 = (int)container.TileLocation.Y;
            int level = -1;
            int num3 = 0;
            if (location is MineShaft mineShaft)
            {
                level = mineShaft.mineLevel;
                if (mineShaft.isContainerPlatform(num, num2))
                {
                    mineShaft.updateMineLevelData(0, -1);
                }

                num3 = mineShaft.GetAdditionalDifficulty();
            }

            if (random.NextDouble() < 0.2)
            {
                if (random.NextDouble() < 0.1)
                {
                    var item = Utility.getRaccoonSeedForCurrentTimeOfYear(who, random);
                    CreateItemExtensions.Predict_createMultipleItemDebris(_ctx, new PredictionItem(item), new Vector2(num, num2) * 64f + new Vector2(32f), -1, location);
                }

                return;
            }

            if (location is MineShaft mineShaft2)
            {
                if (mineShaft2.mineLevel > 120 && !mineShaft2.isSideBranch())
                {
                    int num4 = mineShaft2.mineLevel - 121;
                    if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0)
                    {
                        float num5 = (float)(num4 + (int)Game1.player.team.calicoEggSkullCavernRating.Value * 2) * 0.003f;
                        if (num5 > 0.33f)
                        {
                            num5 = 0.33f;
                        }

                        if (random.NextBool(num5))
                        {
                            CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "CalicoEgg", num, num2, random.Next(1, 4), who.UniqueMultiplayerID, location);
                        }
                    }
                }

                int num6 = mineShaft2.mineLevel;
                if (mineShaft2.mineLevel == 77377)
                {
                    num6 = 5000;
                }

                CreateItemExtensions.Predict_TrySpawnTrinket(_ctx, location, null, new Vector2(num, num2) * 64f + new Vector2(32f), 1.0 + (double)num6 * 0.001);
            }

            if (random.NextDouble() <= 0.05 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
            {
                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)890", num, num2, random.Next(1, 3), who.UniqueMultiplayerID, location);
            }

            if (Utility.tryRollMysteryBox(0.0081 + Game1.player.team.AverageDailyLuck() / 15.0, random))
            {
                string id = (Game1.player.stats.Get(StatKeys.Mastery(2)) != 0) ? "(O)GoldenMysteryBox" : "(O)MysteryBox";
                CreateItemExtensions.Predict_createItemDebris(_ctx, PredictionItem.Create(id), new Vector2(num, num2) * 64f + new Vector2(32f), -1, location);
            }

            CreateItemExtensions.Predict_trySpawnRareObject(_ctx, who, new Vector2(num, num2) * 64f, location, 1.5, 1.0, -1, random);
            if (num3 > 0)
            {
                if (!(random.NextDouble() < 0.15))
                {
                    if (random.NextDouble() < 0.008)
                    {
                        CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)858", num, num2, 1, location);
                    }

                    if (random.NextDouble() < 0.01)
                    {
                        CreateItemExtensions.Predict_createItemDebris(_ctx, PredictionItem.Create("(BC)71"), new Vector2(num, num2) * 64f + new Vector2(32f), 0);
                    }

                    if (random.NextDouble() < 0.01)
                    {
                        CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, random.Choose("(O)918", "(O)919", "(O)920"), num, num2, 1, location);
                    }

                    if (random.NextDouble() < 0.01)
                    {
                        CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)386", num, num2, random.Next(1, 4), location);
                    }

                    switch (random.Next(17))
                    {
                        case 0:
                            CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)382", num, num2, random.Next(1, 3), location);
                            break;
                        case 1:
                            CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)380", num, num2, random.Next(1, 4), location);
                            break;
                        case 2:
                            CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)62", num, num2, 1, location);
                            break;
                        case 3:
                            CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)390", num, num2, random.Next(2, 6), location);
                            break;
                        case 4:
                            CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)80", num, num2, random.Next(2, 3), location);
                            break;
                        case 5:
                            CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, (who.timesReachedMineBottom > 0) ? "(O)84" : random.Choose("(O)92", "(O)370"), num, num2, random.Choose(2, 3), location);
                            break;
                        case 6:
                            CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)70", num, num2, 1, location);
                            break;
                        case 7:
                            CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)390", num, num2, random.Next(2, 6), location);
                            break;
                        case 8:
                            CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)" + random.Next(218, 245), num, num2, 1, location);
                            break;
                        case 9:
                            CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, (Game1.whichFarm == 6) ? "(O)920" : "(O)749", num, num2, 1, location);
                            break;
                        case 10:
                            CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)286", num, num2, 1, location);
                            break;
                        case 11:
                            CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)378", num, num2, random.Next(1, 4), location);
                            break;
                        case 12:
                            CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)384", num, num2, random.Next(1, 4), location);
                            break;
                        case 13:
                            CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)287", num, num2, 1, location);
                            break;
                    }
                }

                return;
            }

            switch (container.ItemId)
            {
                case "118":
                    if (random.NextDouble() < 0.65)
                    {
                        if (random.NextDouble() < 0.8)
                        {
                            switch (random.Next(9))
                            {
                                case 0:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)382", num, num2, random.Next(1, 3), location);
                                    break;
                                case 1:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)378", num, num2, random.Next(1, 4), location);
                                    break;
                                case 3:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)390", num, num2, random.Next(2, 6), location);
                                    break;
                                case 4:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)388", num, num2, random.Next(2, 3), location);
                                    break;
                                case 5:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, (who.timesReachedMineBottom > 0) ? "(O)80" : random.Choose("(O)92", "(O)370"), num, num2, random.Choose(2, 3), location);
                                    break;
                                case 6:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)388", num, num2, random.Next(2, 6), location);
                                    break;
                                case 7:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)390", num, num2, random.Next(2, 6), location);
                                    break;
                                case 8:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)770", num, num2, 1, location);
                                    break;
                                case 2:
                                    break;
                            }
                        }
                        else
                        {
                            switch (random.Next(4))
                            {
                                case 0:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)78", num, num2, random.Next(1, 3), location);
                                    break;
                                case 1:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)78", num, num2, random.Next(1, 3), location);
                                    break;
                                case 2:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)78", num, num2, random.Next(1, 3), location);
                                    break;
                                case 3:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)535", num, num2, random.Next(1, 3), location);
                                    break;
                            }
                        }
                    }
                    else if (random.NextDouble() < 0.4)
                    {
                        switch (random.Next(5))
                        {
                            case 0:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)66", num, num2, 1, location);
                                break;
                            case 1:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)68", num, num2, 1, location);
                                break;
                            case 2:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)709", num, num2, 1, location);
                                break;
                            case 3:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)535", num, num2, 1, location);
                                break;
                            case 4:
                                var item = MineShaft.getSpecialItemForThisMineLevel(level, num, num2);
                                CreateItemExtensions.Predict_createItemDebris(_ctx, new PredictionItem(item), new Vector2(num, num2) * 64f + new Vector2(32f, 32f), random.Next(4), location);
                                break;
                        }
                    }

                    break;
                case "120":
                    if (random.NextDouble() < 0.65)
                    {
                        if (random.NextDouble() < 0.8)
                        {
                            switch (random.Next(9))
                            {
                                case 0:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)382", num, num2, random.Next(1, 3), location);
                                    break;
                                case 1:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)380", num, num2, random.Next(1, 4), location);
                                    break;
                                case 3:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)378", num, num2, random.Next(2, 6), location);
                                    break;
                                case 4:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)388", num, num2, random.Next(2, 6), location);
                                    break;
                                case 5:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, (who.timesReachedMineBottom > 0) ? "(O)84" : random.Choose("(O)92", "(O)371"), num, num2, random.Choose(2, 3), location);
                                    break;
                                case 6:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)390", num, num2, random.Next(2, 4), location);
                                    break;
                                case 7:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)390", num, num2, random.Next(2, 6), location);
                                    break;
                                case 8:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)770", num, num2, 1, location);
                                    break;
                                case 2:
                                    break;
                            }
                        }
                        else
                        {
                            switch (random.Next(4))
                            {
                                case 0:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)78", num, num2, random.Next(1, 3), location);
                                    break;
                                case 1:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)536", num, num2, random.Next(1, 3), location);
                                    break;
                                case 2:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)78", num, num2, random.Next(1, 3), location);
                                    break;
                                case 3:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)78", num, num2, random.Next(1, 3), location);
                                    break;
                            }
                        }
                    }
                    else if (random.NextDouble() < 0.4)
                    {
                        switch (random.Next(5))
                        {
                            case 0:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)62", num, num2, 1, location);
                                break;
                            case 1:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)70", num, num2, 1, location);
                                break;
                            case 2:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)709", num, num2, random.Next(1, 4), location);
                                break;
                            case 3:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)536", num, num2, 1, location);
                                break;
                            case 4:
                                var item = MineShaft.getSpecialItemForThisMineLevel(level, num, num2);
                                CreateItemExtensions.Predict_createItemDebris(_ctx, new PredictionItem(item), new Vector2(num, num2) * 64f + new Vector2(32f, 32f), random.Next(4), location);
                                break;
                        }
                    }

                    break;
                case "124":
                case "122":
                    if (random.NextDouble() < 0.65)
                    {
                        if (random.NextDouble() < 0.8)
                        {
                            switch (random.Next(8))
                            {
                                case 0:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)382", num, num2, random.Next(1, 3), location);
                                    break;
                                case 1:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)384", num, num2, random.Next(1, 4), location);
                                    break;
                                case 3:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)380", num, num2, random.Next(2, 6), location);
                                    break;
                                case 4:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)378", num, num2, random.Next(2, 6), location);
                                    break;
                                case 5:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)390", num, num2, random.Next(2, 6), location);
                                    break;
                                case 6:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)388", num, num2, random.Next(2, 6), location);
                                    break;
                                case 7:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)881", num, num2, random.Next(2, 6), location);
                                    break;
                                case 2:
                                    break;
                            }
                        }
                        else
                        {
                            switch (random.Next(4))
                            {
                                case 0:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)78", num, num2, random.Next(1, 3), location);
                                    break;
                                case 1:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)537", num, num2, random.Next(1, 3), location);
                                    break;
                                case 2:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, (who.timesReachedMineBottom > 0) ? "(O)82" : "(O)78", num, num2, random.Next(1, 3), location);
                                    break;
                                case 3:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)78", num, num2, random.Next(1, 3), location);
                                    break;
                            }
                        }
                    }
                    else if (random.NextDouble() < 0.4)
                    {
                        switch (random.Next(6))
                        {
                            case 0:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)60", num, num2, 1, location);
                                break;
                            case 1:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)64", num, num2, 1, location);
                                break;
                            case 2:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)709", num, num2, random.Next(1, 4), location);
                                break;
                            case 3:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)749", num, num2, 1, location);
                                break;
                            case 4:
                                var item = MineShaft.getSpecialItemForThisMineLevel(level, num, num2);
                                CreateItemExtensions.Predict_createItemDebris(_ctx, new PredictionItem(item), new Vector2(num, num2) * 64f + new Vector2(32f, 32f), random.Next(4), location);
                                break;
                            case 5:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)688", num, num2, 1, location);
                                break;
                        }
                    }

                    break;
                case "174":
                    if (random.NextDouble() < 0.1)
                    {
                        Game1.player.team.Predict_RequestLimitedNutDrops(_ctx, "VolcanoBarrel", location, num * 64, num2 * 64, 5);
                    }

                    if (location is VolcanoDungeon volcanoDungeon && volcanoDungeon.level.Value == 5 && num == 34)
                    {
                        PredictionItem? item = PredictionItem.Create("(O)851", quality: 2);
                        CreateItemExtensions.Predict_createItemDebris(_ctx, item, new Vector2(num, num2) * 64f, 1);
                    }
                    else if (random.NextDouble() < 0.75)
                    {
                        if (random.NextDouble() < 0.8)
                        {
                            switch (random.Next(7))
                            {
                                case 0:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)382", num, num2, random.Next(1, 3), location);
                                    break;
                                case 1:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)384", num, num2, random.Next(1, 4), location);
                                    break;
                                case 2:
                                    // location.characters.Add(new DwarvishSentry(new Vector2(num, num2) * 64f));
                                    break;
                                case 3:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)380", num, num2, random.Next(2, 6), location);
                                    break;
                                case 4:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)378", num, num2, random.Next(2, 6), location);
                                    break;
                                case 5:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "66", num, num2, 1, location);
                                    break;
                                case 6:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)709", num, num2, random.Next(2, 6), location);
                                    break;
                            }
                        }
                        else
                        {
                            switch (random.Next(5))
                            {
                                case 0:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)78", num, num2, random.Next(1, 3), location);
                                    break;
                                case 1:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)749", num, num2, random.Next(1, 3), location);
                                    break;
                                case 2:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)60", num, num2, 1, location);
                                    break;
                                case 3:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)64", num, num2, 1, location);
                                    break;
                                case 4:
                                    CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)68", num, num2, 1, location);
                                    break;
                            }
                        }
                    }
                    else if (random.NextDouble() < 0.4)
                    {
                        switch (random.Next(9))
                        {
                            case 0:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)72", num, num2, 1, location);
                                break;
                            case 1:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)831", num, num2, random.Next(1, 4), location);
                                break;
                            case 2:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)833", num, num2, random.Next(1, 3), location);
                                break;
                            case 3:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)749", num, num2, 1, location);
                                break;
                            case 4:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)386", num, num2, 1, location);
                                break;
                            case 5:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)848", num, num2, 1, location);
                                break;
                            case 6:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)856", num, num2, 1, location);
                                break;
                            case 7:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)886", num, num2, 1, location);
                                break;
                            case 8:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)688", num, num2, 1, location);
                                break;
                        }
                    }
                    else
                    {
                        // location.characters.Add(new DwarvishSentry(new Vector2(num, num2) * 64f));
                    }

                    break;
            }
        }

        public static bool Predict_performToolAction(this Object obj, PredictionContext _ctx, Tool? t = null)
        {
            GameLocation location = obj.Location;
            if (obj.isTemporarilyInvisible)
            {
                return false;
            }
            // ...
            if (obj.name.Contains("SupplyCrate") && (t is null || t.isHeavyHitter()))
            {
                Random random = Utility.CreateRandom(Game1.uniqueIDForThisGame, obj.TileLocation.X * 777.0, obj.TileLocation.Y * 7.0);
                int houseUpgradeLevel = (t?.getLastFarmerToUse() ?? Game1.player).HouseUpgradeLevel;
                int xTile = (int)obj.TileLocation.X;
                int yTile = (int)obj.TileLocation.Y;

                switch (houseUpgradeLevel)
                {
                    case 0:
                        switch (random.Next(7))
                        {
                            case 0:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)770", xTile, yTile, random.Next(3, 6), location);
                                break;
                            case 1:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)371", xTile, yTile, random.Next(5, 8), location);
                                break;
                            case 2:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)535", xTile, yTile, random.Next(2, 5), location);
                                break;
                            case 3:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)241", xTile, yTile, random.Next(1, 3), location);
                                break;
                            case 4:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)395", xTile, yTile, random.Next(1, 3), location);
                                break;
                            case 5:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)286", xTile, yTile, random.Next(3, 6), location);
                                break;
                            default:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)286", xTile, yTile, random.Next(3, 6), location);
                                break;
                        }

                        break;
                    case 1:
                        switch (random.Next(10))
                        {
                            case 0:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)770", xTile, yTile, random.Next(3, 6), location);
                                break;
                            case 1:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)371", xTile, yTile, random.Next(5, 8), location);
                                break;
                            case 2:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)749", xTile, yTile, random.Next(2, 5), location);
                                break;
                            case 3:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)253", xTile, yTile, random.Next(1, 3), location);
                                break;
                            case 4:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)237", xTile, yTile, random.Next(1, 3), location);
                                break;
                            case 5:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)246", xTile, yTile, random.Next(4, 8), location);
                                break;
                            case 6:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)247", xTile, yTile, random.Next(2, 5), location);
                                break;
                            case 7:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)245", xTile, yTile, random.Next(4, 8), location);
                                break;
                            case 8:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)287", xTile, yTile, random.Next(3, 6), location);
                                break;
                            default:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "MixedFlowerSeeds", xTile, yTile, random.Next(4, 6), location);
                                break;
                        }

                        break;
                    default:
                        switch (random.Next(9))
                        {
                            case 0:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)770", xTile, yTile, random.Next(3, 6), location);
                                break;
                            case 1:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)920", xTile, yTile, random.Next(5, 8), location);
                                break;
                            case 2:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)749", xTile, yTile, random.Next(2, 5), location);
                                break;
                            case 3:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)253", xTile, yTile, random.Next(2, 4), location);
                                break;
                            case 4:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, random.Choose("(O)904", "(O)905"), xTile, yTile, random.Next(1, 3), location);
                                break;
                            case 5:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)246", xTile, yTile, random.Next(4, 8), location);
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)247", xTile, yTile, random.Next(2, 5), location);
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)245", xTile, yTile, random.Next(4, 8), location);
                                break;
                            case 6:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)275", xTile, yTile, 2, location);
                                break;
                            case 7:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "(O)288", xTile, yTile, random.Next(3, 6), location);
                                break;
                            default:
                                CreateItemExtensions.Predict_createMultipleObjectDebris(_ctx, "MixedFlowerSeeds", xTile, yTile, random.Next(5, 6), location);
                                break;
                        }

                        break;
                }

                return true;
            }

            return false;
        }
    }
}

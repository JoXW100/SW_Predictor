using Microsoft.Xna.Framework;
using Predictor.Framework;
using Predictor.Framework.Extentions;
using Predictor.Framework.UI;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Tools;
using StardewValley.TokenizableStrings;

namespace Predictor.Patches
{
    internal class FishingPatch : PatchBase
    {
        public override string Name => nameof(FishingPatch);

        private PredictionContext Context;
        private long PredictionIdentifier;
        private readonly PushStack<string> Catches;

        public FishingPatch(IModHelper helper, IMonitor monitor) : base(helper, monitor)
        {
            Context = new();
            PredictionIdentifier = 0;
            Catches = new PushStack<string>(ModEntry.Instance.Config.NumFishCatches);
        }

        public override void OnAttach()
        {
            Helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            Helper.Events.GameLoop.UpdateTicked += OnTick;
        }

        public override void OnLazyAttach()
        {
            Helper.Events.GameLoop.OneSecondUpdateTicked += OnUpdateTicked;
            Helper.Events.GameLoop.UpdateTicked += OnTick;
        }

        public override void OnDetatch()
        {
            Helper.Events.GameLoop.UpdateTicked -= OnUpdateTicked;
            Helper.Events.GameLoop.OneSecondUpdateTicked -= OnUpdateTicked;
            Helper.Events.GameLoop.UpdateTicked -= OnTick;
        }

        public override bool CheckRequirements()
        {
            return base.CheckRequirements()
                && (ModEntry.Instance.Config.ShowFishChances || ModEntry.Instance.Config.ShowTrashChances);
        }

        private IUIElement? CreateMenu()
        {
            var spacing = Vector2.One * 2f;

            IUIElement? fishInfoDisplay = null;
            if (Context.Items.Any())
            {
                Context.Items.Sort((x, y) => CompareItemChances(x, y, Context.GetPropertyValue<Dictionary<string, FishChanceData>>(PredictionProperty.FishChanceData)));
                var chanceData = Context.GetPropertyValue<Dictionary<string, FishChanceData>>(PredictionProperty.FishChanceData);
                var numItems = Context.Items.Count(x => chanceData.TryGetValue(x.ItemId, out var chance) && ((ModEntry.Instance.Config.ShowTrashChances && chance.IsTrash) || (ModEntry.Instance.Config.ShowFishChances && !chance.IsTrash)));
                if (numItems <= 0)
                {
                    return null;
                }

                List<IUIElement?> children = new();
                for (int i = 0; i < Context.Items.Count; i++)
                {
                    PredictionItem fish = Context.Items[i];
                    var chance = chanceData[fish.ItemId];

                    if (!ModEntry.Instance.Config.ShowTrashChances && chance.IsTrash)
                    {
                        continue;
                    }

                    if (!ModEntry.Instance.Config.ShowFishChances && !chance.IsTrash)
                    {
                        continue;
                    }

                    if (!ModEntry.Instance.Config.ShowUncaughtFish && !chance.IsTrash && !Game1.player.fishCaught.ContainsKey(fish.QualifiedItemId))
                    {
                        children.AddRange(new IUIElement?[]
                        {
                            null,
                            new Label("???", Game1.smallFont),
                            null,
                        });
                    }
                    else
                    {
                        var color = chance.BobberInArea ? chance.IsTrash ? Color.Brown : Utils.TextColor : Color.Gray;
                        var chanceString = String.Format("{0:0.0%}", chance.Chance);
                        children.AddRange(new IUIElement?[]
                        {
                            new ItemSprite(fish, Utils.UIScale),
                            ModEntry.Instance.Config.ShowLessFishInfo
                                ? null
                                : new Label(TokenParser.ParseText(fish.DisplayName), Game1.smallFont, color),
                            new Label(chanceString, Game1.smallFont, color),
                        });
                    }
                }

                var headerText = ModEntry.Instance.Helper.Translation.Get("menu.FishingHeader");
                var trashChance = Context.GetPropertyValue<float>(PredictionProperty.TrashChance);
                var trashText = string.Format("{1} {0:0.0%}", trashChance, ModEntry.Instance.Helper.Translation.Get("menu.FishingTrashChanceLabel"));

                fishInfoDisplay = new Grid(
                    children: new IUIElement?[]
                    {
                        new Label(headerText),
                        new Label(trashText),
                        new Grid(children: children, spacing: spacing, layout: "auto auto auto")
                    },
                    spacing: spacing,
                    padding: Vector4.One * 6 * Utils.UIScale,
                    backgroundColor: Utils.MenuBackground
                );
            }

            IUIElement? baitDisplay = null;
            IUIElement? tackleDisplay = null;
            if (Game1.player.ActiveItem is FishingRod rod)
            {
                if (ModEntry.Instance.Config.ShowBait)
                {
                    var bait = rod.GetBait();
                    var baitItem = bait is null ? null : PredictionItem.Create(bait.ItemId);
                    if (baitItem is not null)
                    {
                        var headerText = TokenParser.ParseText(ModEntry.Instance.Helper.Translation.Get("menu.FishingBaitLabel"));
                        baitDisplay = new Grid(
                            children: new IUIElement?[]
                            {
                                new Label(headerText, Game1.smallFont),
                                new ItemSprite(baitItem, Utils.UIScale),
                                new Label(String.Format("x{0} ", bait!.Stack)),
                                ModEntry.Instance.Config.ShowLessFishInfo
                                    ? null
                                    : new Label(TokenParser.ParseText(baitItem.DisplayName), Game1.smallFont),
                            },
                            spacing: spacing,
                            padding: Vector4.One * 6 * Utils.UIScale,
                            backgroundColor: Utils.MenuBackground,
                            layout: "auto auto auto auto"
                        );
                    }
                }

                if (ModEntry.Instance.Config.ShowTackle)
                {
                    var tackle = rod.GetTackle();
                    if (tackle.Any())
                    {
                        var children = new List<IUIElement?>();
                        for (int i = 0; i < tackle.Count; i++)
                        {
                            var tackleItem = tackle[i] is null ? null : PredictionItem.Create(tackle[i].ItemId, durability: PredictionItem.GetTackleDurability(tackle[i].uses.Value));
                            if (tackleItem is not null)
                            {
                                children.AddRange(new IUIElement?[]
                                {
                                new ItemSprite(tackleItem, Utils.UIScale),
                                new Label(String.Format("{0:0%} ", tackleItem.Durability)),
                                ModEntry.Instance.Config.ShowLessFishInfo
                                    ? null
                                    : new Label(TokenParser.ParseText(tackleItem.DisplayName), Game1.smallFont)
                                });
                            }
                        }

                        if (children.Any())
                        {
                            var headerText = TokenParser.ParseText(ModEntry.Instance.Helper.Translation.Get("menu.FishingTackleLabel"));
                            if (children.Count == 3)
                            {
                                tackleDisplay = new Grid(
                                    children: new IUIElement?[]
                                    {
                                        new Label(headerText, Game1.smallFont),
                                        children[0],
                                        children[1],
                                        children[2]
                                    },
                                    spacing: spacing,
                                    padding: Vector4.One * 6 * Utils.UIScale,
                                    backgroundColor: Utils.MenuBackground,
                                    layout: "auto auto auto auto"
                                );
                            }
                            else
                            {
                                tackleDisplay = new Grid(
                                    children: new IUIElement?[]
                                    {
                                        new Label(headerText, Game1.smallFont),
                                        new Grid(children: children, spacing: spacing, layout: "auto auto auto")
                                    },
                                    spacing: spacing,
                                    padding: Vector4.One * 6 * Utils.UIScale,
                                    backgroundColor: Utils.MenuBackground
                                );
                            }
                        }
                    }
                }
            }

            IUIElement? catchDisplay = null;
            if (Game1.activeClickableMenu is BobberBar bar && ModEntry.Instance.Config.ShowFishInMinigame)
            {
                var item = PredictionItem.Create(bar.whichFish);
                var headerText = TokenParser.ParseText(ModEntry.Instance.Helper.Translation.Get("menu.FishingMinigameLabel"));
                if (item is not null)
                {
                    catchDisplay = new Grid(
                        children: new IUIElement?[]
                        {
                            new Label(headerText, Game1.smallFont),
                            new ItemSprite(item, Utils.UIScale),
                            ModEntry.Instance.Config.ShowLessFishInfo
                                ? null
                                : new Label(TokenParser.ParseText(item.DisplayName), Game1.smallFont),
                        },
                        spacing: spacing,
                        padding: Vector4.One * 6 * Utils.UIScale,
                        backgroundColor: Utils.MenuBackground,
                        layout: "auto auto auto"
                    );
                }
            }

            if (fishInfoDisplay is null && baitDisplay is null && tackleDisplay is null && catchDisplay is null)
            {
                return null;
            }
            else
            {
                return new Grid(
                    children: new IUIElement?[]
                    {
                        fishInfoDisplay,
                        baitDisplay,
                        tackleDisplay,
                        catchDisplay
                    },
                    spacing: spacing
                );
            }
        }

        private static int CompareItemChances(PredictionItem a, PredictionItem b, Dictionary<string, FishChanceData> c)
        {
            var chanceA = c[a.ItemId];
            var chanceB = c[b.ItemId];
            if (chanceA.IsTrash == chanceB.IsTrash)
            {
                if (a.Category == b.Category)
                {
                    var x = chanceB.Chance.CompareTo(chanceA.Chance);
                    if (x == 0)
                    {
                        return b.SpriteIndex.CompareTo(a.SpriteIndex);
                    }
                    return x;
                }
                return a.Category - b.Category;
            }
            return chanceA.IsTrash ? 1 : -1;
        }

        private void OnUpdateTicked(object? sender, EventArgs e)
        {
            if (!ModEntry.Instance.Config.EstimateFishChances)
            {
                return;
            }

            Menu = null;
            Context = new();

            if (!CheckRequirements())
            {
                Monitor.LogOnce($"{nameof(FishingPatch)} attached when requirements are false", LogLevel.Debug);
                return;
            }

            if (Game1.player.CurrentItem is FishingRod rod)
            {
                var bait = rod.GetBait();
                double potensy = (bait != null) ? bait.Price / 10f : 0f;
                Game1.player.currentLocation.Predict_getAllFish(Context, 0, bait?.QualifiedItemId, rod.clearWaterDistance, Game1.player, potensy, rod.bobber.Value);
            }

            Menu = CreateMenu();
        }

        private void OnTick(object? sender, EventArgs e)
        {
            if (ModEntry.Instance.Config.EstimateFishChances)
            {
                Catches.Clear();
                return;
            }

            Menu = null;
            Context = new();

            if (!CheckRequirements())
            {
                Monitor.LogOnce($"{nameof(FishingPatch)} attached when requirements are false", LogLevel.Debug);
                return;
            }

            if (Game1.player.CurrentItem is FishingRod rod)
            {
                var bobberTile = rod.bobber.Value;
                var identifier = Game1.currentLocation.TryGetFishAreaForTile(bobberTile, out var areaId, out var area) 
                    ? areaId.GetHashCode() 
                    : Game1.currentLocation.Name.GetHashCode();

                if (identifier != PredictionIdentifier)
                {
                    Catches.Clear();
                    PredictionIdentifier = identifier;
                }

                var bait = rod.GetBait();
                double potensy = (bait != null) ? bait.Price / 10f : 0f;
                var fish = Game1.player.currentLocation.getFish(0, bait?.QualifiedItemId, rod.clearWaterDistance, Game1.player, potensy, bobberTile);
                Catches.Push(fish.ItemId);

                var chanceData = new Dictionary<string, FishChanceData>();
                var collection = new Dictionary<string, int>();
                foreach (var item in Catches)
                {
                    if (collection.TryGetValue(item, out var c))
                    {
                        collection[item] = c + 1;
                    }
                    else
                    {
                        collection[item] = 1;
                    }
                }

                var total = Catches.Count();
                var trash = new List<PredictionItem>();
                foreach (var (id, count) in collection)
                {
                    var item = PredictionItem.Create(id);
                    if (item != null)
                    {
                        if (item.Category == -20)
                        {
                            trash.Add(item);
                        }
                        else
                        {
                            Context.Items.Add(item);
                            chanceData[item.ItemId] = new FishChanceData((float)count / total, true, false);
                        }
                    }
                }

                foreach (var item in trash)
                {
                    Context.Items.Add(item);
                    chanceData[item.ItemId] = new FishChanceData(1f / trash.Count, true, true);
                }

                Context.Properties[PredictionProperty.TrashChance] = (float)trash.Count / total;
                Context.Properties[PredictionProperty.FishChanceData] = chanceData;
            }
            else
            {
                Catches.Clear();
            }

            Menu = CreateMenu();
        }
    }
}

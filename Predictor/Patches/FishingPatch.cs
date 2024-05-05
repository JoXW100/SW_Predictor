using Microsoft.Xna.Framework;
using Predictor.Framework;
using Predictor.Framework.Extentions;
using Predictor.Framework.UI;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Tools;
using StardewValley.TokenizableStrings;
using Object = StardewValley.Object;

namespace Predictor.Patches
{
    public sealed class FishinPatchContext : IPatchContextWithMenu
    {
        public IUIElement? Menu { get; set; }
        public long PredictionIdentifier { get; set; } = 0;
        public PushStack<string> Catches { get; set; } = new(ModEntry.Instance.Config.NumFishCatches);
    }

    public sealed class FishingPatch : PatchWithMenuBase<FishinPatchContext>
    {
        public override string Name => nameof(FishingPatch);
        private long PredictionIdentifier
        {
            get => MultiplayerManager.GetValue().PredictionIdentifier;
            set => MultiplayerManager.GetValue().PredictionIdentifier = value;
        }
        private PushStack<string> Catches
        {
            get => MultiplayerManager.GetValue().Catches;
            set => MultiplayerManager.GetValue().Catches = value;
        }
        private static Vector2 Spacing => Vector2.One * 2f;
        private static readonly PredictionItem TrashCan = PredictionItem.Create("2427")!;

        public FishingPatch(IModHelper helper, IMonitor monitor) : base(helper, monitor)
        {

        }

        public override void OnAttach()
        {
            if (ModEntry.Instance.Config.EstimateFishChances)
            {
                Helper.Events.GameLoop.UpdateTicked += OnEstimateInfoUpdate;
            }
            else
            {
                Helper.Events.GameLoop.UpdateTicked += OnPredictInfoUpdate;
            }
        }

        public override void OnLazyAttach()
        {
            if (ModEntry.Instance.Config.EstimateFishChances)
            {
                Helper.Events.GameLoop.OneSecondUpdateTicked += OnEstimateInfoUpdate;
            }
            else
            {
                Helper.Events.GameLoop.UpdateTicked += OnPredictInfoUpdate;
            }
        }

        public override void OnDetatch()
        {
            Helper.Events.GameLoop.UpdateTicked -= OnEstimateInfoUpdate;
            Helper.Events.GameLoop.OneSecondUpdateTicked -= OnEstimateInfoUpdate;
            Helper.Events.GameLoop.UpdateTicked -= OnPredictInfoUpdate;
            Menu = null;
            Catches.Clear();
        }

        public override bool CheckRequirements()
        {
            return base.CheckRequirements()
                && (ModEntry.Instance.Config.ShowFishChances || ModEntry.Instance.Config.ShowTrashChances);
        }

        private void OnEstimateInfoUpdate(object? sender, EventArgs e)
        {
            if (!CheckRequirements())
            {
                return;
            }

            var context = new PredictionContext();
            if (Game1.player.CurrentItem is FishingRod rod)
            {
                var bait = rod.GetBait();
                double potensy = GetBaitPotensy(bait);
                Game1.player.currentLocation.Predict_getAllFish(context, 0, bait?.QualifiedItemId, rod.clearWaterDistance, Game1.player, potensy, rod.bobber.Value);
                Menu = CreateMenu(rod, context);
            }
            else
            {
                Menu = null;
            }
        }

        private void OnPredictInfoUpdate(object? sender, EventArgs e)
        {
            if (!CheckRequirements())
            {
                return;
            }

            if (Game1.player.CurrentItem is FishingRod rod)
            {
                var bobberTile = rod.bobber.Value;
                var identifier = Game1.currentLocation.TryGetFishAreaForTile(bobberTile, out var areaId, out var _) 
                    ? areaId.GetHashCode() 
                    : Game1.currentLocation.Name.GetHashCode();

                if (identifier != PredictionIdentifier)
                {
                    Catches.Clear();
                    PredictionIdentifier = identifier;
                }

                var bait = rod.GetBait();
                double potensy = GetBaitPotensy(bait);
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

                var context = new PredictionContext();
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
                            context.Items.Add(item);
                            chanceData[item.ItemId] = new FishChanceData((float)count / total, true, false);
                        }
                    }
                }

                foreach (var item in trash)
                {
                    context.Items.Add(item);
                    chanceData[item.ItemId] = new FishChanceData(1f / trash.Count, true, true);
                }

                context.Properties[PredictionProperty.TrashChance] = (float)trash.Count / total;
                context.Properties[PredictionProperty.FishChanceData] = chanceData;
                Menu = CreateMenu(rod, context);
            }
            else
            {
                Catches.Clear();
                Menu = null;
            }
        }

        private IUIElement? CreateMenu(FishingRod rod, PredictionContext context)
        {
            IUIElement? fishInfoDisplay = CreateFishInfoDisplay(context);
            IUIElement? baitDisplay = CreateBaitDisplay(rod);
            IUIElement? tackleDisplay = CreateTackleDisplay(rod);
            IUIElement? catchDisplay = CreateCatchDisplay();
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
                    spacing: Spacing
                );
            }
        }

        private IUIElement? CreateFishInfoDisplay(PredictionContext context)
        {
            if (!context.Items.Any())
            {
                return null;
            }

            context.Items.Sort((x, y) => CompareItemChances(x, y, context.GetPropertyValue<Dictionary<string, FishChanceData>>(PredictionProperty.FishChanceData)));
            var chanceData = context.GetPropertyValue<Dictionary<string, FishChanceData>>(PredictionProperty.FishChanceData);
            var numItems = context.Items.Count(x => chanceData.TryGetValue(x.ItemId, out var chance) && ((ModEntry.Instance.Config.ShowTrashChances && chance.IsTrash) || (ModEntry.Instance.Config.ShowFishChances && !chance.IsTrash)));
            if (numItems <= 0)
            {
                return null;
            }

            List<IUIElement?> children = new();
            var trashChance = context.GetPropertyValue<float>(PredictionProperty.TrashChance);
            var trashLabel = Helper.Translation.Get("menu.FishingTrashChanceLabel");
            var trashChanceString = string.Format("{0:0.0%}", trashChance);
            children.AddRange(new IUIElement?[]
            {
                 new ItemSprite(TrashCan, Utils.UIScale / 2f, tooltips: trashLabel),
                 ModEntry.Instance.Config.ShowLessFishInfo
                    ? null
                    : new Label(trashLabel, Game1.smallFont, Color.Brown),
                 new Label(trashChanceString, Game1.smallFont, Color.Brown),
            });

            for (int i = 0; i < context.Items.Count; i++)
            {
                PredictionItem fish = context.Items[i];
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
                    var chanceString = string.Format("{0:0.0%}", chance.Chance);
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

            var headerText = string.Format("{0}:", Helper.Translation.Get("menu.FishingHeader"));
            return new Grid(
                children: new IUIElement?[]
                {
                    new Label(headerText),
                    new Grid(children: children, spacing: Spacing, layout: "auto auto auto", childAlignment: "center")
                },
                spacing: Spacing,
                padding: Vector4.One * 6 * Utils.UIScale,
                backgroundColor: Utils.MenuBackground
            );
        }

        private IUIElement? CreateBaitDisplay(FishingRod rod)
        {
            if (!ModEntry.Instance.Config.ShowBait)
            {
                return null;
            }

            var bait = rod.GetBait();
            var baitItem = bait is null ? null : PredictionItem.Create(bait.ItemId);
            if (baitItem is null)
            {
                return null;
            }

            var headerText = string.Format("{0}:", Helper.Translation.Get("menu.FishingBaitLabel"));
            return new Group(
                children: new IUIElement?[]
                {
                    new Label(headerText, Game1.smallFont),
                    new ItemSprite(baitItem, Utils.UIScale),
                    new Label(string.Format("x{0} ", bait!.Stack)),
                    ModEntry.Instance.Config.ShowLessFishInfo
                        ? null
                        : new Label(TokenParser.ParseText(baitItem.DisplayName), Game1.smallFont),
                },
                spacing: 2f,
                padding: Vector4.One * 6 * Utils.UIScale,
                backgroundColor: Utils.MenuBackground
            );
        }

        private IUIElement? CreateTackleDisplay(FishingRod rod)
        {
            if (!ModEntry.Instance.Config.ShowTackle)
            {
                return null;
            }

            var tackle = rod.GetTackle();
            if (!tackle.Any())
            {
                return null;
            }

            var children = new List<IUIElement?>();
            for (int i = 0; i < tackle.Count; i++)
            {
                var tackleItem = tackle[i] is null ? null : PredictionItem.Create(tackle[i].ItemId, durability: PredictionItem.GetTackleDurability(tackle[i].uses.Value));
                if (tackleItem is not null)
                {
                    children.AddRange(new IUIElement?[]
                    {
                        new ItemSprite(tackleItem, Utils.UIScale),
                        new Label(string.Format("{0:0%} ", tackleItem.Durability)),
                        ModEntry.Instance.Config.ShowLessFishInfo
                            ? null
                            : new Label(TokenParser.ParseText(tackleItem.DisplayName), Game1.smallFont)
                    });
                }
            }

            if (!children.Any())
            {
                return null;
            }

            var headerText = string.Format("{0}:", Helper.Translation.Get("menu.FishingTackleLabel"));
            if (children.Count == 3)
            {
                return new Group(
                    children: new IUIElement?[]
                    {
                        new Label(headerText, Game1.smallFont),
                        children[0],
                        children[1],
                        children[2]
                    },
                    spacing: 2f,
                    padding: Vector4.One * 6 * Utils.UIScale,
                    backgroundColor: Utils.MenuBackground
                );
            }
            else
            {
                return new Grid(
                    children: new IUIElement?[]
                    {
                        new Label(headerText, Game1.smallFont),
                        new Grid(children: children, spacing: Spacing, layout: "auto auto auto")
                    },
                    spacing: Spacing,
                    padding: Vector4.One * 6 * Utils.UIScale,
                    backgroundColor: Utils.MenuBackground
                );
            }
        }

        private IUIElement? CreateCatchDisplay()
        {
            if (!ModEntry.Instance.Config.ShowFishInMinigame)
            {
                return null;
            }

            if (Game1.activeClickableMenu is not BobberBar bar)
            {
                return null;
            }

            var item = PredictionItem.Create(bar.whichFish);
            if (item is null)
            {
                return null;
            }

            var headerText = string.Format("{0}:", Helper.Translation.Get("menu.FishingMinigameLabel"));
            return new Grid(
                children: new IUIElement?[]
                {
                    new Label(headerText, Game1.smallFont),
                    new ItemSprite(item, Utils.UIScale),
                    ModEntry.Instance.Config.ShowLessFishInfo
                        ? null
                        : new Label(TokenParser.ParseText(item.DisplayName), Game1.smallFont),
                },
                spacing: Spacing,
                padding: Vector4.One * 6 * Utils.UIScale,
                backgroundColor: Utils.MenuBackground,
                layout: "auto auto auto"
            );
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

        private static double GetBaitPotensy(Object? bait)
        {
            return bait is not null ? bait.Price / 10d : 0d;
        }
    }
}

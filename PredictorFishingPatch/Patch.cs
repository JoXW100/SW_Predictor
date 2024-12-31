using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;
using StardewValley.TokenizableStrings;
using DynamicUIFramework.Elements;
using DynamicUIFramework;
using PredictorPatchFramework;
using PredictorPatchFramework.UI;
using PredictorFishingPatch.UI;
using Object = StardewValley.Object;

namespace PredictorFishingPatch
{
    internal sealed class FishinPatchContext : IPatchContextWithMenu
    {
        public IUIElement Menu { get; set; }
        public long PredictionIdentifier { get; set; } = 0;
        public FixedSizePushStack<string> Catches { get; set; } = new(ModEntry.Instance.Config.NumFishCatches);

        public FishinPatchContext(IUIElement menu)
        {
            Menu = menu;
        }
    }

    internal sealed class Patch : PatchWithMenuBase<FishinPatchContext>
    {
        public override string Name => ModEntry.Instance.ModManifest.Name;
        private long PredictionIdentifier
        {
            get => MultiplayerManager.GetValue().PredictionIdentifier;
            set => MultiplayerManager.GetValue().PredictionIdentifier = value;
        }
        private FixedSizePushStack<string> Catches
        {
            get => MultiplayerManager.GetValue().Catches;
            set => MultiplayerManager.GetValue().Catches = value;
        }
        private IUIElement? FishInfoDisplay => Menu.Children.ElementAtOrDefault(0);
        private IUIElement? FishInfoDisplayContent => FishInfoDisplay?.Children.ElementAtOrDefault(1);

        public Patch(IModHelper helper, IMonitor monitor) : base(helper, monitor, () => new FishinPatchContext(CreateMenu(helper)))
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
            Menu.IsVisible = false;
            Catches.Clear();
        }

        public override bool CheckRequirements()
        {
            return base.CheckRequirements()
                && (ModEntry.Instance.Config.ShowFishChances || ModEntry.Instance.Config.ShowTrashChances || ModEntry.Instance.Config.ShowTackle || ModEntry.Instance.Config.ShowBait || ModEntry.Instance.Config.ShowFish);
        }

        private void OnEstimateInfoUpdate(object? sender, EventArgs e)
        {
            if (!CheckRequirements())
            {
                Menu.IsVisible = false;
                return;
            }

            var context = new PredictionContext();
            if (Game1.player.currentLocation.canFishHere() 
                && Game1.player.CurrentItem is FishingRod rod 
                && FishingHelper.TryFindFishingLocation(Game1.player, rod, out var fishingLocation))
            {
                var bait = rod.GetBait();
                var potensy = FishingHelper.GetBaitPotensy(bait);
                Game1.player.currentLocation.Predict_getAllFish(context, bait?.QualifiedItemId, rod.clearWaterDistance, Game1.player, potensy, fishingLocation);
                var holder = FishInfoDisplayContent;
                if (holder is not null)
                {
                    holder.Children = GetFishInfoDisplayContent(context);
                    holder.IsVisible = true;
                    FishInfoDisplay!.IsVisible = holder.Children.Any();
                }

                Menu.IsVisible = true;
            }
            else
            {
                FishInfoDisplay!.IsVisible = false;
            }
        }

        private void OnPredictInfoUpdate(object? sender, EventArgs e)
        {
            if (!CheckRequirements())
            {
                Catches.Clear();
                Menu.IsVisible = false;
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
                double potensy = FishingHelper.GetBaitPotensy(bait);
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

                context.Properties["trashChance"] = (float)trash.Count / total;
                context.Properties["fishChances"] = chanceData;

                var holder = FishInfoDisplayContent;
                if (holder is not null)
                {
                    holder.Children = GetFishInfoDisplayContent(context);
                    FishInfoDisplay!.IsVisible = holder.Children.Any();
                }

                Menu.IsVisible = true;
            }
            else
            {
                FishInfoDisplay!.IsVisible = false;
            }
        }

        private static IUIElement CreateMenu(IModHelper helper)
        {
            var headerText = string.Format("{0}:", helper.Translation.Get("menu.FishingHeader"));
            return new Grid(
                children: new IUIElement[]
                {
                    new Grid(children: new IUIElement?[]
                    {
                        new Label(headerText, Game1.smallFont, FrameworkUtils.API.TextColor),
                        new Grid(
                            spacing: FrameworkUtils.API.MenuInnerSpacing,
                            layout: "auto auto auto",
                            childAlignment: Alignment.CenterRight
                        )
                    }, FrameworkUtils.API.MenuPadding, FrameworkUtils.API.MenuInnerSpacing, FrameworkUtils.API.MenuBackground),
                    new FishBaitDisplay(helper, FrameworkUtils.API.MenuPadding, FrameworkUtils.API.MenuInnerSpacing, FrameworkUtils.API.MenuBackground),
                    new FishTackleDisplay(helper, FrameworkUtils.API.MenuPadding, FrameworkUtils.API.MenuInnerSpacing, FrameworkUtils.API.MenuBackground),
                    new FishCatchDisplay(helper, FrameworkUtils.API.MenuPadding, FrameworkUtils.API.MenuInnerSpacing, FrameworkUtils.API.MenuBackground),
                },
                spacing: FrameworkUtils.API.MenuInnerSpacing
            );
        }

        private IEnumerable<IUIElement?> GetFishInfoDisplayContent(PredictionContext context)
        {
            if (!context.Items.Any())
            {
                yield break;
            }

            var chanceData = context.GetPropertyValue<Dictionary<string, FishChanceData>>("fishChances");
            var hasValidItem = context.Items.Any(x => chanceData.TryGetValue(x.ItemId, out var chance) && ((ModEntry.Instance.Config.ShowTrashChances && chance.IsTrash) || (ModEntry.Instance.Config.ShowFishChances && !chance.IsTrash)));
            if (!hasValidItem)
            {
                yield break;
            }

            context.Items.Sort((x, y) => CompareItemChances(x, y, context.GetPropertyValue<Dictionary<string, FishChanceData>>("fishChances")));
            var trashChance = context.GetPropertyValue<float>("trashChance");
            var trashLabel = Helper.Translation.Get("menu.FishingTrashChanceLabel");
            var trashChanceString = string.Format("{0:0.0%}", trashChance);
            var trashSprite = new Sprite(ElementFactory.TrashCanTexture, (Vector2.One * FrameworkUtils.API.UIScale * PredictionItem.TextureSize).ToPoint());
            yield return ElementFactory.AddTooltips(trashSprite, trashLabel);
            yield return ModEntry.Instance.Config.ShowLessFishInfo
                ? null
                : new Label(trashLabel, Game1.smallFont, Color.Brown);
            yield return new Label(trashChanceString, Game1.smallFont, Color.Brown);

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
                    yield return null;
                    yield return new Label("???", Game1.smallFont, FrameworkUtils.API.TextColor);
                    yield return null;
                }
                else
                {
                    var color = chance.BobberInArea ? chance.IsTrash ? Color.Brown : FrameworkUtils.API.TextColor : Color.Gray;
                    var chanceString = string.Format("{0:0.0%}", chance.Chance);
                    yield return ElementFactory.CreateItemSprite(fish);
                    yield return ModEntry.Instance.Config.ShowLessFishInfo
                        ? null
                        : new Label(TokenParser.ParseText(fish.DisplayName), Game1.smallFont, color);
                    yield return new Label(chanceString, Game1.smallFont, color);
                }
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
    }
}

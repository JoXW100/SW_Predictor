using PredictorPatchFramework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace PredictorSpawnedPatch
{
    internal sealed class Patch : PatchWithContextAndMenuBase<PredictionItem>
    {
        public override string Name => ModEntry.Instance.ModManifest.Name;

        public Patch(IModHelper helper, IMonitor monitor) : base(helper, monitor)
        {
            
        }

        public override void OnAttach()
        {
            Helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            Helper.Events.Display.RenderedWorld += OnRendered;
        }

        public override void OnLazyAttach()
        {
            Helper.Events.GameLoop.OneSecondUpdateTicked += OnUpdateTicked;
            Helper.Events.World.ObjectListChanged += OnUpdateTicked;
            Helper.Events.World.TerrainFeatureListChanged += OnUpdateTicked;
            Helper.Events.Display.RenderedWorld += OnRendered;
        }

        public override void OnDetatch()
        {
            Helper.Events.GameLoop.UpdateTicked -= OnUpdateTicked;
            Helper.Events.GameLoop.OneSecondUpdateTicked -= OnUpdateTicked;
            Helper.Events.World.ObjectListChanged -= OnUpdateTicked;
            Helper.Events.World.TerrainFeatureListChanged -= OnUpdateTicked;
            Helper.Events.Display.RenderedWorld -= OnRendered;
            Menu.IsVisible = false;
        }

        public override bool CheckRequirements()
        {
            return base.CheckRequirements()
                && (ModEntry.Instance.Config.ShowTrackers || ModEntry.Instance.Config.ShowOutlines);
        }

        private void OnRendered(object? sender, RenderedWorldEventArgs e)
        {
            if (CheckRequirements() && ModEntry.Instance.Config.ShowOutlines)
            {
                FrameworkUtils.DrawOutlines(e.SpriteBatch, Context.Keys);
            }
        }

        private void OnUpdateTicked(object? sender, EventArgs e)
        {
            if (!CheckRequirements())
            {
                return;
            }

            Context.Clear();
            foreach (var (pos, obj) in Game1.player.currentLocation.Objects.Pairs)
            {
                if (obj.IsSpawnedObject)
                {
                    Context.TryAdd(pos, new PredictionItem(obj));
                }
            }

            // Wild plants
            foreach (var features in Game1.player.currentLocation.terrainFeatures)
            {
                foreach (var (pos, feature) in features)
                {
                    if (feature is HoeDirt dirt && dirt.crop != null && dirt.crop.forageCrop.Value && dirt.crop.whichForageCrop.Value == "1")
                    {
                        var item = PredictionItem.Create("(O)399");
                        if (item != null)
                        {
                            Context.TryAdd(pos, item);
                        }
                    }
                }
            }

            if (ModEntry.Instance.Config.ShowTrackers)
            {
                FrameworkUtils.UpdateNearbyContextItemsMenu(Menu, Context, Helper.Translation.Get("menu.SpawedHeader"), ModEntry.Instance.Config.TrackerMaxItemCount, ModEntry.Instance.Config.TrackerShowLess);
            }
            else
            {
                Menu.IsVisible = false;
            }
        }
    }
}

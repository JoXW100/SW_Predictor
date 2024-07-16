using PredictorCropPatch.Extentions;
using PredictorPatchFramework;
using PredictorPatchFramework.Extentions;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace PredictorSpawnedPatch
{
    internal sealed class Patch : PatchWithContextAndMenuBase<PredictionContext>
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
                && (ModEntry.Instance.Config.ShowItems || ModEntry.Instance.Config.ShowTrackers || ModEntry.Instance.Config.ShowOutlines);
        }

        private void OnRendered(object? sender, RenderedWorldEventArgs e)
        {
            if (CheckRequirements())
            {
                FrameworkUtils.DrawContextItems(e.SpriteBatch, Context, ModEntry.Instance.Config.ShowItems, ModEntry.Instance.Config.ShowOutlines);
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
                if (obj.IsSpawnedObject && ModEntry.Instance.Config.ShowTrackers || Game1.viewport.Contains(pos.ToLocation() * FrameworkUtils.TileSize))
                {
                    var ctx = new PredictionContext();
                    obj.Predict_harvestSpawned(ctx, Game1.player, pos);

                    if (ctx.Items.Any())
                    {
                        Context.TryAdd(pos, ctx);
                    }
                }
            }

            // Wild plants
            var location = Game1.player.currentLocation;
            foreach (var features in location.terrainFeatures)
            {
                foreach (var (pos, feature) in features)
                {
                    if ((ModEntry.Instance.Config.ShowTrackers || Game1.viewport.Contains(pos.ToLocation() * FrameworkUtils.TileSize)) &&
                        feature is HoeDirt dirt && dirt.crop is not null && dirt.crop.forageCrop.Value && dirt.crop.whichForageCrop.Value == "1")
                    {
                        var ctx = new PredictionContext();
                        dirt.crop.Predict_harvest(ctx, (int)pos.X, (int)pos.Y, dirt);

                        if (ctx.Items.Any())
                        {
                            Context.TryAdd(pos, ctx);
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

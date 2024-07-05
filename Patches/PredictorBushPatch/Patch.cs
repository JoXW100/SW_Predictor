using StardewValley;
using StardewValley.TerrainFeatures;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using PredictorPatchFramework;
using PredictorPatchFramework.Extentions;

namespace PredictorBushPatch
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
            Helper.Events.World.LargeTerrainFeatureListChanged += OnUpdateTicked;
            Helper.Events.Display.RenderedWorld += OnRendered;
        }

        public override void OnDetatch()
        {
            Helper.Events.GameLoop.UpdateTicked -= OnUpdateTicked;
            Helper.Events.GameLoop.OneSecondUpdateTicked -= OnUpdateTicked;
            Helper.Events.World.LargeTerrainFeatureListChanged -= OnUpdateTicked;
            Helper.Events.Display.RenderedWorld -= OnRendered;
            Menu.IsVisible = false;
        }

        public override bool CheckRequirements()
        {
            return base.CheckRequirements()
                && (ModEntry.Instance.Config.ShowItems || ModEntry.Instance.Config.ShowOutlines || ModEntry.Instance.Config.ShowTrackers);
        }

        private void OnRendered(object? sender, RenderedWorldEventArgs e)
        {
            if (CheckRequirements())
            {
                FrameworkUtils.DrawContextItems(e.SpriteBatch, Context, ModEntry.Instance.Config.ShowItems, ModEntry.Instance.Config.ShowOutlines, 2);
            }
        }

        private void OnUpdateTicked(object? sender, EventArgs e)
        {
            if (!CheckRequirements())
            {
                return;
            }

            Context.Clear();
            foreach (var feature in Game1.player.currentLocation.largeTerrainFeatures)
            {
                if (feature is Bush bush)
                {
                    if (!ModEntry.Instance.Config.ShowTrackers)
                    {
                        var location = bush.Tile.ToLocation() * FrameworkUtils.TileSize;
                        if (!Game1.viewport.Contains(location))
                        {
                            continue;
                        }
                    }

                    PredictionContext ctx = new();
                    if (ModEntry.Instance.Config.ShowTrackers)
                    {
                        bush.Predict_shake(ctx, bush.Tile);
                    }

                    if (ctx.Items.Any() || (ModEntry.Instance.Config.ShowOutlines && bush.IsHarvestable()))
                    {
                        if (Context.TryGetValue(feature.Tile, out var current))
                        {
                            current.Join(ctx);
                        }
                        else
                        {
                            Context.Add(feature.Tile, ctx);
                        }
                    }
                }
            }

            if (ModEntry.Instance.Config.ShowTrackers)
            {
                FrameworkUtils.UpdateNearbyContextItemsMenu(Menu, Context, Helper.Translation.Get("menu.HarvestableBushHeader"), ModEntry.Instance.Config.TrackerMaxItemCount, ModEntry.Instance.Config.TrackerShowLess);
            }
            else
            {
                Menu.IsVisible = false;
            }
        }
    }
}

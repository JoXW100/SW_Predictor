using Microsoft.Xna.Framework;
using PredictorPatchFramework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Tools;

namespace PredictorPanningPatch
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
            if (Game1.player.CurrentItem is Pan pan)
            {
                var pos = Game1.currentLocation.orePanPoint.Value;
                if (pos != Point.Zero)
                {
                    PredictionContext ctx = new();
                    if (ModEntry.Instance.Config.ShowItems)
                    {
                        pan.Predict_getPanItems(ctx, Game1.currentLocation, Game1.player);
                    }
                    Context.TryAdd(pos.ToVector2(), ctx);
                }
            }

            if (ModEntry.Instance.Config.ShowTrackers)
            {
                FrameworkUtils.UpdateNearbyContextItemsMenu(Menu, Context, Helper.Translation.Get("menu.PanningSpotHeader"), ModEntry.Instance.Config.TrackerMaxItemCount, ModEntry.Instance.Config.TrackerShowLess);
            }
            else
            {
                Menu.IsVisible = false;
            }
        }
    }
}

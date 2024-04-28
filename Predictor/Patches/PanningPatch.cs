using Microsoft.Xna.Framework;
using Predictor.Framework;
using Predictor.Framework.Extentions;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Tools;

namespace Predictor.Patches
{
    internal class PanningPatch : PatchBase
    {
        public override string Name => nameof(PanningPatch);

        private readonly Dictionary<Vector2, PredictionContext> Context;

        public PanningPatch(IModHelper helper, IMonitor monitor) : base(helper, monitor)
        {
            Context = new();
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
        }
        
        public override bool CheckRequirements()
        {
            return base.CheckRequirements()
                && (ModEntry.Instance.Config.EnablePaningSpotItems || ModEntry.Instance.Config.EnablePaningSpotOutlines || ModEntry.Instance.Config.TrackPanningSpots);
        }

        private void OnRendered(object? sender, RenderedWorldEventArgs e)
        {
            Utils.DrawContextItems(e.SpriteBatch, Context, ModEntry.Instance.Config.EnablePaningSpotItems, ModEntry.Instance.Config.EnablePaningSpotOutlines, 2);
        }

        private void OnUpdateTicked(object? sender, EventArgs e)
        {
            Menu = null;
            Context.Clear();

            if (!CheckRequirements())
            {
                Monitor.LogOnce($"{nameof(PanningPatch)} attached when requirements are false", LogLevel.Debug);
                return;
            }

            if (Game1.player.CurrentItem is Pan pan)
            {
                var pos = Game1.currentLocation.orePanPoint.Value;
                if (pos != Point.Zero)
                {
                    PredictionContext ctx = new();
                    if (ModEntry.Instance.Config.EnablePaningSpotItems)
                    {
                        pan.Predict_getPanItems(ctx, Game1.currentLocation, Game1.player);
                    }
                    Context.TryAdd(pos.ToVector2(), ctx);
                }
            }

            if (ModEntry.Instance.Config.TrackPanningSpots)
            {
                Menu = Utils.CreateNearbyContextItemsMenu(Context, "menu.PanningSpotHeader");
            }
        }
    }
}

using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using PredictorPatchFramework;
using PredictorPatchFramework.Extentions;
using StardewValley.Objects;
using PredictorCrabPotPatch;

namespace PredictorSeedMakerPatch
{
    internal sealed class Patch : PatchWithContexBase<PredictionContext>
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
            Helper.Events.Display.RenderedWorld += OnRendered;
        }

        public override void OnDetatch()
        {
            Helper.Events.GameLoop.UpdateTicked -= OnUpdateTicked;
            Helper.Events.GameLoop.OneSecondUpdateTicked -= OnUpdateTicked;
            Helper.Events.Display.RenderedWorld -= OnRendered;
        }

        private void OnRendered(object? sender, RenderedWorldEventArgs e)
        {
            if (CheckRequirements())
            {
                FrameworkUtils.DrawContextItems(e.SpriteBatch, Context);
            }
        }

        private void OnUpdateTicked(object? sender, EventArgs e)
        {
            if (!CheckRequirements())
            {
                return;
            }

            Context.Clear();

            foreach (var (pos, obj) in Game1.currentLocation.Objects.Pairs)
            {
                if (Game1.viewport.Contains(pos.ToLocation() * FrameworkUtils.TileSize) && obj is CrabPot pot)
                {
                    var ctx = new PredictionContext();
                    ctx.AddItemIfNotNull(pot.Predict_NextDayOutput(Game1.player.CurrentItem.AsObject()));
                    if (ctx.Items.Any())
                    {
                        Context.Add(pos, ctx);
                    }
                }
            }
        }
    }
}

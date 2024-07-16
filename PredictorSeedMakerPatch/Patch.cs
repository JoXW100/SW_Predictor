using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using PredictorPatchFramework;
using PredictorPatchFramework.Extentions;
using Object = StardewValley.Object;

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

            if (Game1.player.CurrentItem is null)
            {
                return;
            }

            foreach (var (pos, obj) in Game1.currentLocation.Objects.Pairs)
            {
                if (obj.QualifiedItemId != "(BC)25" || !Game1.viewport.Contains(pos.ToLocation() * FrameworkUtils.TileSize))
                {
                    continue;
                }

                var context = new PredictionContext();
                var item = Object.OutputSeedMaker(obj, Game1.player.CurrentItem, true, null, out var min);
                context.AddItemIfNotNull(item);
                Context.Add(pos, context);
            }
        }
    }
}

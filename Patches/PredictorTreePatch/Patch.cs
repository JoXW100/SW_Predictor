using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;
using PredictorPatchFramework;
using PredictorPatchFramework.Extentions;
using DynamicUIFramework;

namespace PredictorTreePatch
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
            Helper.Events.World.TerrainFeatureListChanged += OnUpdateTicked;
            Helper.Events.Display.RenderedWorld += OnRendered;
        }

        public override void OnDetatch()
        {
            Helper.Events.GameLoop.UpdateTicked -= OnUpdateTicked;
            Helper.Events.GameLoop.OneSecondUpdateTicked -= OnUpdateTicked;
            Helper.Events.World.TerrainFeatureListChanged -= OnUpdateTicked;
            Helper.Events.Display.RenderedWorld -= OnRendered;
        }

        public override IUIElement? GetMenu()
        {
            return null;
        }

        public override bool CheckRequirements()
        {
            return base.CheckRequirements()
                && (ModEntry.Instance.Config.ShowItems || ModEntry.Instance.Config.ShowOutlines);
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
            foreach (var posx in Game1.currentLocation.terrainFeatures.Keys)
            {
                if (!Game1.viewport.Contains(posx.ToLocation() * FrameworkUtils.TileSize))
                {
                    continue;
                }

                PredictionContext ctx = new();
                TerrainFeature feature = Game1.currentLocation.terrainFeatures[posx];
                if (feature is Tree tree)
                {
                    tree.Predict_shake(ctx, posx, false);

                    if (tree.hasMoss.Value)
                    {
                        ctx.AddItemIfNotNull(Tree.CreateMossItem());
                    }

                    if (ctx.Items.Any() || ModEntry.Instance.Config.ShowOutlines)
                    {
                        Context.TryAdd(posx, ctx);
                    }
                }
            }
        }
    }
}

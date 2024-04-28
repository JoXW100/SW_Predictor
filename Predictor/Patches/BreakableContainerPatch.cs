using Microsoft.Xna.Framework;
using Predictor.Framework;
using Predictor.Framework.Extentions;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;

namespace Predictor.Patches
{
    public class BreakableContainerPatch : PatchBase
    {
        public override string Name => nameof(BreakableContainerPatch);

        private readonly Dictionary<Vector2, PredictionContext> Context;

        public BreakableContainerPatch(IModHelper helper, IMonitor monitor) : base(helper, monitor)
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
            Helper.Events.World.ObjectListChanged += OnUpdateTicked;
            Helper.Events.Display.RenderedWorld += OnRendered;
        }

        public override void OnDetatch()
        {
            Helper.Events.GameLoop.UpdateTicked -= OnUpdateTicked;
            Helper.Events.GameLoop.OneSecondUpdateTicked -= OnUpdateTicked;
            Helper.Events.World.ObjectListChanged -= OnUpdateTicked;
            Helper.Events.Display.RenderedWorld -= OnRendered;
        }

        public override bool CheckRequirements()
        {
            return base.CheckRequirements()
                && (ModEntry.Instance.Config.EnableBreakableContainerItems || ModEntry.Instance.Config.EnableBreakableContainerOutlines);
        }

        private void OnRendered(object? sender, RenderedWorldEventArgs e)
        {
            Utils.DrawContextItems(e.SpriteBatch, Context, ModEntry.Instance.Config.EnableBreakableContainerItems, ModEntry.Instance.Config.EnableBreakableContainerOutlines);
        }

        private void OnUpdateTicked(object? sender, EventArgs e)
        {
            Context.Clear();

            if (!CheckRequirements())
            {
                Monitor.LogOnce($"{nameof(BreakableContainer)} attached when requirements are false", LogLevel.Debug);
                return;
            }

            foreach (var (pos, obj) in Game1.player.currentLocation.Objects.Pairs)
            {
                // skip non-stone objects too far away.
                var location = pos.ToLocation();
                if (!Game1.viewport.Contains(location * Utils.TileSize))
                {
                    continue;
                }

                PredictionContext ctx = new();
                if (ModEntry.Instance.Config.EnableBreakableContainerItems)
                {
                    // Exclude item categories
                    if (obj is BreakableContainer container)
                    {
                        container.Predict_releaseContents(ctx, Game1.player);
                    }
                    else
                    {
                        obj.Predict_performToolAction(ctx);
                    }
                }

                if (ctx.Items.Any() || ModEntry.Instance.Config.EnableBreakableContainerOutlines && obj.IsBreakableContainer())
                {
                    if (Context.TryGetValue(pos, out var current))
                    {
                        current.Join(ctx);
                    }
                    else
                    {
                        Context.Add(pos, ctx);
                    }
                }
            }
        }
    }
}

using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;
using PredictorPatchFramework;
using PredictorPatchFramework.Extentions;

namespace PredictorBreakableContainerPatch
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
            Helper.Events.Display.RenderedWorld += OnRendered;
        }

        public override void OnDetatch()
        {
            Helper.Events.GameLoop.UpdateTicked -= OnUpdateTicked;
            Helper.Events.GameLoop.OneSecondUpdateTicked -= OnUpdateTicked;
            Helper.Events.World.ObjectListChanged -= OnUpdateTicked;
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
                // skip non-stone objects too far away.
                var location = pos.ToLocation();
                if (!ModEntry.Instance.Config.ShowTrackers && !Game1.viewport.Contains(location * FrameworkUtils.TileSize))
                {
                    continue;
                }

                if (!obj.IsBreakableContainer())
                {
                    continue;
                }

                PredictionContext ctx = new();
                if (ModEntry.Instance.Config.ShowItems)
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

                if (ctx.Items.Any() || ModEntry.Instance.Config.ShowOutlines)
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

            if (ModEntry.Instance.Config.ShowTrackers)
            {
                FrameworkUtils.UpdateNearbyContextItemsMenu(Menu, Context, Helper.Translation.Get("menu.BreakableContainerHeader"), ModEntry.Instance.Config.TrackerMaxItemCount, ModEntry.Instance.Config.TrackerShowLess);
            }
            else
            {
                Menu.IsVisible = false;
            }
        }
    }
}

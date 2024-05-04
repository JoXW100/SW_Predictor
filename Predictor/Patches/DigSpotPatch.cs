using Predictor.Framework;
using Predictor.Framework.Extentions;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace Predictor.Patches
{
    public sealed class DigSpotPatch : PatchWithContextBase<PredictionContext>
    {
        public override string Name => nameof(DigSpotPatch);

        public DigSpotPatch(IModHelper helper, IMonitor monitor) : base(helper, monitor)
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
            Menu = null;
        }

        public override bool CheckRequirements()
        {
            return base.CheckRequirements()
                && (ModEntry.Instance.Config.EnableDigSpotItems || ModEntry.Instance.Config.EnableDigSpotOutlines || ModEntry.Instance.Config.TrackDigSpots);
        }

        private void OnRendered(object? sender, RenderedWorldEventArgs e)
        {
            if (CheckRequirements())
            {
                Utils.DrawContextItems(e.SpriteBatch, Context, ModEntry.Instance.Config.EnableDigSpotItems, ModEntry.Instance.Config.EnableDigSpotOutlines);
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
                var location = pos.ToLocation();
                if ((obj.QualifiedItemId != "(O)590" && obj.QualifiedItemId != "(O)SeedSpot") 
                || !(ModEntry.Instance.Config.TrackDigSpots || Game1.viewport.Contains(location * Utils.TileSize)))
                {
                    continue;
                }

                // Dig spots
                PredictionContext ctx = new();
                if (ModEntry.Instance.Config.EnableDigSpotItems)
                {
                    Game1.player.currentLocation.Predict_digUpSpot(ctx, obj, pos, Game1.player);
                }

                if (ctx.Items.Any() || ModEntry.Instance.Config.EnableDigSpotItems)
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

            if (ModEntry.Instance.Config.TrackDigSpots)
            {
                Menu = Utils.CreateNearbyContextItemsMenu(Context, "menu.DigSpotHeader");
            }
        }
    }
}

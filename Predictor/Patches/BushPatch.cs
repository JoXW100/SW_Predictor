using Microsoft.Xna.Framework;
using Predictor.Framework;
using Predictor.Framework.Extentions;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace Predictor.Patches
{
    public sealed class BushPatch : PatchBase
    {
        public override string Name =>  nameof(BushPatch);

        private readonly Dictionary<Vector2, PredictionContext> Context;

        public BushPatch(IModHelper helper, IMonitor monitor) : base(helper, monitor)
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
                && (ModEntry.Instance.Config.EnableHarvestableBushItems || ModEntry.Instance.Config.EnableHarvestableBushOutlines || ModEntry.Instance.Config.TrackHarvestableBushes);
        }

        private void OnRendered(object? sender, RenderedWorldEventArgs e)
        {
            Utils.DrawContextItems(e.SpriteBatch, Context, ModEntry.Instance.Config.EnableHarvestableBushItems, ModEntry.Instance.Config.EnableHarvestableBushOutlines, 2);
        }

        private void OnUpdateTicked(object? sender, EventArgs e)
        {
            Context.Clear();
            Menu = null;

            if (!CheckRequirements())
            {
                Monitor.LogOnce($"{nameof(TreePatch)} attached when requirements are false", LogLevel.Debug);
                return;
            }

            foreach (var feature in Game1.player.currentLocation.largeTerrainFeatures)
            {
                if (feature is Bush bush)
                {
                    if (!ModEntry.Instance.Config.TrackHarvestableBushes)
                    {
                        var location = bush.Tile.ToLocation() * Utils.TileSize;
                        if (!Game1.viewport.Contains(location))
                        {
                            continue;
                        }
                    }

                    PredictionContext ctx = new();
                    if (ModEntry.Instance.Config.TrackHarvestableBushes)
                    {
                        bush.Predict_shake(ctx, bush.Tile);
                    }

                    if (ctx.Items.Any() || (ModEntry.Instance.Config.EnableHarvestableBushOutlines && bush.IsHarvestable()))
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

            if (ModEntry.Instance.Config.TrackHarvestableBushes)
            {
                Menu = Utils.CreateNearbyContextItemsMenu(Context, "menu.HarvestableBushHeader");
            }
        }
    }
}

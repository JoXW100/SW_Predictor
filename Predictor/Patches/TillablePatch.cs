using Microsoft.Xna.Framework;
using xTile.Dimensions;
using Predictor.Framework;
using Predictor.Framework.UI;
using Predictor.Framework.Extentions;
using StardewModdingAPI.Events;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;
using StardewValley.TerrainFeatures;

namespace Predictor.Patches
{
    internal class TillablePatch : PatchBase
    {

        public override string Name => nameof(TillablePatch);

        private readonly Dictionary<Vector2, PredictionContext> Context;

        public TillablePatch(IModHelper helper, IMonitor monitor) : base(helper, monitor)
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

        public override bool CheckRequirements()
        {
            return base.CheckRequirements()
                && (ModEntry.Instance.Config.EnableTillableItems || ModEntry.Instance.Config.EnableTillableOutlines);
        }

        private void OnRendered(object? sender, RenderedWorldEventArgs e)
        {
            Utils.DrawContextItems(e.SpriteBatch, Context, ModEntry.Instance.Config.EnableTillableItems, ModEntry.Instance.Config.EnableTillableOutlines);
        }

        private static IEnumerable<Location> GetDiggableTileLocations(GameLocation location)
        {
            var layer = location.Map.GetLayer("Back");
            if (layer != null)
            {
                // skip garbage cans too far away.
                var xlim = Math.Min((Game1.viewport.X + Game1.viewport.Width) / Utils.TileSize, layer.LayerWidth);
                var ylim = Math.Min((Game1.viewport.Y + Game1.viewport.Height) / Utils.TileSize, layer.LayerHeight);
                for (var x = Math.Max((Game1.viewport.X + Utils.TileSize - 1) / Utils.TileSize, 0); x < xlim; x++)
                {
                    for (var y = Math.Max((Game1.viewport.Y + Utils.TileSize - 1) / Utils.TileSize, 0); y < ylim; y++)
                    {
                        var tile = layer.Tiles[x, y];
                        if (tile != null 
                        && (tile.TileIndexProperties.ContainsKey("Diggable") || tile.Properties.ContainsKey("Diggable"))
                        && (!location.terrainFeatures.TryGetValue(new Vector2(x, y), out var value) || value is not HoeDirt))
                        {
                            yield return new Location(x, y);
                        }
                    }
                }
            }
        }

        private void OnUpdateTicked(object? sender, EventArgs e)
        {
            Context.Clear();

            if (!CheckRequirements())
            {
                Monitor.LogOnce($"{nameof(TillablePatch)} attached when requirements are false", LogLevel.Debug);
                return;
            }

            if (Game1.player.CurrentItem is Hoe)
            {
                foreach (var pos in GetDiggableTileLocations(Game1.currentLocation))
                {
                    PredictionContext ctx = new();
                    Game1.currentLocation.Predict_checkForBuriedItem(ctx, pos.X, pos.Y, false, false, Game1.player);
                    if (ctx.Items.Any())
                    {
                        Context.TryAdd(pos.ToVector2(), ctx);
                    }
                }
            }
        }
    }
}

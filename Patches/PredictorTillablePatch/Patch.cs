using Microsoft.Xna.Framework;
using xTile.Dimensions;
using StardewModdingAPI.Events;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;
using StardewValley.TerrainFeatures;
using PredictorPatchFramework;
using PredictorPatchFramework.Extentions;

namespace PredictorTillablePatch
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

        private static IEnumerable<Location> GetDiggableTileLocations(GameLocation location)
        {
            var layer = location.Map.GetLayer("Back");
            if (layer != null)
            {
                // skip garbage cans too far away.
                var xlim = Math.Min((Game1.viewport.X + Game1.viewport.Width) / FrameworkUtils.TileSize, layer.LayerWidth);
                var ylim = Math.Min((Game1.viewport.Y + Game1.viewport.Height) / FrameworkUtils.TileSize, layer.LayerHeight);
                for (var x = Math.Max((Game1.viewport.X + FrameworkUtils.TileSize - 1) / FrameworkUtils.TileSize, 0); x < xlim; x++)
                {
                    for (var y = Math.Max((Game1.viewport.Y + FrameworkUtils.TileSize - 1) / FrameworkUtils.TileSize, 0); y < ylim; y++)
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
            if (!CheckRequirements())
            {
                return;
            }

            Context.Clear();
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

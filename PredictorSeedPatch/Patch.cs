using Microsoft.Xna.Framework;
using PredictorCropPatch.Extensions;
using PredictorPatchFramework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace PredictorCropPatch
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
            Helper.Events.World.ObjectListChanged += OnUpdateTicked;
            Helper.Events.World.TerrainFeatureListChanged += OnUpdateTicked;
            Helper.Events.Display.RenderedWorld += OnRendered;
        }

        public override void OnDetatch()
        {
            Helper.Events.GameLoop.UpdateTicked -= OnUpdateTicked;
            Helper.Events.GameLoop.OneSecondUpdateTicked -= OnUpdateTicked;
            Helper.Events.World.ObjectListChanged -= OnUpdateTicked;
            Helper.Events.World.TerrainFeatureListChanged -= OnUpdateTicked;
            Helper.Events.Display.RenderedWorld -= OnRendered;
        }

        private void OnRendered(object? sender, RenderedWorldEventArgs e)
        {
            if (CheckRequirements() && ModEntry.Instance.Config.ShowOutlines)
            {
                FrameworkUtils.DrawContextItems(e.SpriteBatch, Context, drawOutline: ModEntry.Instance.Config.ShowOutlines);
            }
        }

        private static IEnumerable<HoeDirt> GetDiggableTiles(GameLocation location)
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
                        && location.terrainFeatures.TryGetValue(new Vector2(x, y), out var value)
                        && value is HoeDirt dirt
                        && dirt.crop is null)
                        {
                            yield return dirt;
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

            var seedId = Game1.player.CurrentItem?.ItemId;
            if (seedId != null && Crop.TryGetData(seedId, out var data))
            {
                foreach (var dirt in GetDiggableTiles(Game1.currentLocation))
                {
                    PredictionContext ctx = new();
                    data.Predict_harvest(ctx, (int)dirt.Tile.X, (int)dirt.Tile.Y, dirt);
                    if (ctx.Items.Count > 0)
                    {
                        Context.TryAdd(dirt.Tile, ctx);
                    }
                }
            }
        }
    }
}

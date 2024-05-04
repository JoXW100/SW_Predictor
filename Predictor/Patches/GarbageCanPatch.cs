using Microsoft.Xna.Framework;
using Predictor.Framework;
using Predictor.Framework.Extentions;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace Predictor.Patches
{
    public sealed class GarbageCanPatch : PatchWithContextBase<PredictionContext>
    {
        public override string Name => nameof(GarbageCanPatch);

        public GarbageCanPatch(IModHelper helper, IMonitor monitor) : base(helper, monitor)
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

        public override bool CheckRequirements()
        {
            return base.CheckRequirements()
                && (ModEntry.Instance.Config.EnableGarbageCanItems || ModEntry.Instance.Config.EnableGarbageCanOutlines);
        }

        private void OnRendered(object? sender, RenderedWorldEventArgs e)
        {
            if (!CheckRequirements())
            {
                return;
            }

            var spriteBatch = e.SpriteBatch;
            float ratio = Game1.options.zoomLevel != 1f ? 1f : 1f / Game1.options.uiScale;
            float size = Utils.TileSize * ratio;
            double angle = Game1.currentGameTime.TotalGameTime.Milliseconds * Math.PI / 512.0;
            var scale = 2.5f * (1f + (float)(Math.Cos(angle) + 1f) * 0.03f) * ratio;
            var scaleOffset = Vector2.One * PredictionItem.TextureSize * 0.5f * scale;
            var centerOffset = Vector2.One * size * 0.5f;

            foreach (var (posx, ctx) in Context)
            {
                var pos = Game1.GlobalToLocal(Game1.viewport, posx * Utils.TileSize) * ratio;
                if (ModEntry.Instance.Config.EnableGarbageCanItems)
                {
                    var positions = Utils.GetCirclePositions(ctx.Items.Count, ctx.Items.Count % 2 == 0 ? 0 : Math.PI / 2).ToArray();
                    for (int i = 0; i < ctx.Items.Count; i++)
                    {
                        var spawn = ctx.Items[i];
                        var circleOffset = positions[i] * size * 0.25f;
                        spawn.Draw(spriteBatch, pos + centerOffset + circleOffset - scaleOffset, scale);
                    }
                }

                if (ModEntry.Instance.Config.EnableGarbageCanOutlines)
                {
                    var color = Utils.OutlineColor;
                    
                    if (ModEntry.Instance.Config.EnableGarbageCanWarningOutlines)
                    {
                        color = (ctx.Properties.TryGetValue(PredictionProperty.Affected, out var npcs) && npcs is List<NPC> list && list.Any())
                            ? Utils.GarbageCanWarnColor
                            : Utils.GarbageCanOkColor;
                    }

                    var area = new Rectangle((int)pos.X, (int)pos.Y, (int)size, (int)size);
                    spriteBatch.DrawBorder(area, 4, color);
                }
            }
        }

        private static IEnumerable<KeyValuePair<Vector2, string[]>> GarbageActions(GameLocation? location)
        {
            if (location is null)
            {
                yield break;
            }

            var layer = location.Map.GetLayer("Buildings");
            if (layer is not null)
            {
                // skip garbage cans too far away.
                var xlim = Math.Min((Game1.viewport.X + Game1.viewport.Width) / Utils.TileSize, layer.LayerWidth);
                var ylim = Math.Min((Game1.viewport.Y + Game1.viewport.Height) / Utils.TileSize, layer.LayerHeight);
                for (var x = Math.Max((Game1.viewport.X + Utils.TileSize - 1) / Utils.TileSize, 0); x < xlim; x++)
                {
                    for (var y = Math.Max((Game1.viewport.Y + Utils.TileSize - 1) / Utils.TileSize, 0); y < ylim; y++)
                    {
                        var tile = layer.Tiles[x, y];
                        if (tile is not null && tile.Properties.TryGetValue("Action", out var action))
                        {
                            string[]? actions = action?.ToString().Split(' ');
                            if (actions is not null && actions.Length > 1 && actions[0] == "Garbage")
                            {
                                yield return new KeyValuePair<Vector2, string[]>(new Vector2(x, y), actions);
                            }
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
            foreach (var (pos, actions) in GarbageActions(Game1.currentLocation))
            {
                PredictionContext ctx = new();
                if (Game1.currentLocation.Predict_CheckGarbage(ctx, actions[1], pos, Game1.player)
                && !(ctx.Properties.TryGetValue(PredictionProperty.Exhausted, out var exhausted) && (bool)exhausted))
                {
                    Context.TryAdd(pos, ctx);
                }
            }
        }
    }
}

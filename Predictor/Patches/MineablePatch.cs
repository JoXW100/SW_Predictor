using Microsoft.Xna.Framework;
using Predictor.Framework;
using Predictor.Framework.Extentions;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Monsters;

namespace Predictor.Patches
{
    public sealed class MineablePatch : PatchWithContextBase<PredictionContext>
    {
        public override string Name => nameof(MineablePatch);

        public MineablePatch(IModHelper helper, IMonitor monitor) : base(helper, monitor)
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
            Helper.Events.World.NpcListChanged += OnUpdateTicked;
            Helper.Events.Display.RenderedWorld += OnRendered;
        }

        public override void OnDetatch()
        {
            Helper.Events.GameLoop.UpdateTicked -= OnUpdateTicked;
            Helper.Events.GameLoop.OneSecondUpdateTicked -= OnUpdateTicked;
            Helper.Events.World.ObjectListChanged -= OnUpdateTicked;
            Helper.Events.World.NpcListChanged -= OnUpdateTicked;
            Helper.Events.Display.RenderedWorld -= OnRendered;
        }

        public override bool CheckRequirements()
        {
            return base.CheckRequirements()
                && (ModEntry.Instance.Config.EnableMineableObjectItems || ModEntry.Instance.Config.EnableMineableObjectOutlines || ModEntry.Instance.Config.EnableLadderOutlines);
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
                var drawLadder = ModEntry.Instance.Config.EnableLadderOutlines
                    && ctx.Properties.TryGetValue(PredictionProperty.SpawnLadder, out var ladder) && (bool)ladder
                    && (!ModEntry.Instance.Config.EnableMonstersHideLadders || !Game1.currentLocation.characters.Any(c => c is Monster));
                
                if (ModEntry.Instance.Config.EnableMineableObjectItems)
                {
                    var positions = Utils.GetCirclePositions(ctx.Items.Count, ctx.Items.Count % 2 == 0 ? 0 : Math.PI / 2d).ToArray();
                    for (int i = 0; i < ctx.Items.Count; i++)
                    {
                        var spawn = ctx.Items[i];
                        var circleOffset = positions[i] * size * 0.25f;
                        spawn.Draw(spriteBatch, pos + centerOffset + circleOffset - scaleOffset, scale);
                    }
                }

                if (drawLadder)
                {
                    var area = new Rectangle((int)pos.X, (int)pos.Y, (int)size, (int)size);
                    spriteBatch.DrawBorder(area, 4, Utils.LadderColor);
                }

                if (ModEntry.Instance.Config.EnableMineableObjectOutlines && !drawLadder)
                {
                    var area = new Rectangle((int)pos.X, (int)pos.Y, (int)size, (int)size);
                    spriteBatch.DrawBorder(area, 1, Utils.OutlineColor);
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
            foreach (var (pos, obj) in Game1.player.currentLocation.Objects.Pairs)
            {
                // Exclude item categories
                if (obj.Category > -100 || !obj.Name.Contains("Stone"))
                {
                    continue;
                }

                // skip non-stone objects too far away.
                var location = pos.ToLocation();
                if (!Game1.viewport.Contains(location * Utils.TileSize))
                {
                    continue;
                }

                PredictionContext ctx = new();
                if (ModEntry.Instance.Config.EnableMineableObjectItems || ModEntry.Instance.Config.EnableLadderOutlines)
                {
                    Game1.currentLocation.Predict_OnStoneDestroyed(ctx, obj.ItemId, location.X, location.Y, Game1.player);
                }

                if ((ModEntry.Instance.Config.EnableMineableObjectItems && ctx.Items.Any())
                 || ModEntry.Instance.Config.EnableMineableObjectOutlines
                 || (ModEntry.Instance.Config.EnableLadderOutlines && ctx.Properties.TryGetValue(PredictionProperty.SpawnLadder, out var value) && (bool)value))
                {
                    if (Context.TryGetValue(pos, out var current))
                    {
                        current.Join(ctx);
                    } else
                    {
                        Context.Add(pos, ctx);
                    }
                }
            }
        }
    }
}

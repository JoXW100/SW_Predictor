using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Tools;
using StardewValley.Monsters;
using PredictorPatchFramework;
using PredictorPatchFramework.Extentions;
using DynamicUIFramework;

namespace PredictorMineablePatch
{
    internal sealed class Patch : PatchWithContexBase<PredictionContext>
    {
        public override string Name => ModEntry.Instance.ModManifest.Name;
        public static Color LadderColor => Color.Green;
        public static Color ShaftColor => Color.GreenYellow;

        private readonly Texture2D m_mineTexture;
        private readonly Rectangle m_shaftSourceRect;
        private readonly Rectangle m_ladderSourceRect;

        public Patch(IModHelper helper, IMonitor monitor) : base(helper, monitor)
        {
            m_mineTexture = Game1.content.Load<Texture2D>("Maps/Mines/mine_desert");
            // X: shaft ? 16 * 14 : 16 * 13, Y: 16 * 10
            m_shaftSourceRect = new Rectangle(224, 160, 16, 16);
            m_ladderSourceRect = new Rectangle(208, 160, 16, 16);
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
                && (ModEntry.Instance.Config.ShowItems || ModEntry.Instance.Config.ShowOutlines || ModEntry.Instance.Config.ShowLadders);
        }

        private void OnRendered(object? sender, RenderedWorldEventArgs e)
        {
            if (!CheckRequirements())
            {
                return;
            }

            var spriteBatch = e.SpriteBatch;
            float ratio = FrameworkUtils.Ratio;
            float size = FrameworkUtils.TileSize * ratio;
            double angle = Game1.currentGameTime.TotalGameTime.Milliseconds * Math.PI / 512.0;
            var scale = Vector2.One * 2.5f * (1f + (float)(Math.Cos(angle) + 1f) * 0.03f) * ratio;
            var scaleOffset = Vector2.One * PredictionItem.TextureSize * 0.5f * scale;
            var centerOffset = Vector2.One * size * 0.5f;
            var halfPi = Math.PI / 2d;

            foreach (var (posx, ctx) in Context)
            {
                var pos = Game1.GlobalToLocal(Game1.viewport, posx * FrameworkUtils.TileSize) * ratio;
                var drawOutline = true;
                if (ModEntry.Instance.Config.ShowLadders
                && (!ModEntry.Instance.Config.MonstersHideLadders || !Game1.currentLocation.characters.Any(c => c is Monster))
                && ctx.Properties.TryGetValue("spawnLadder", out var shaft))
                {
                    drawOutline = false;
                    var area = new Rectangle((int)pos.X, (int)pos.Y, (int)size, (int)size);
                    var color = (bool)shaft! ? ShaftColor : LadderColor;
                    spriteBatch.Draw(m_mineTexture, area, (bool)shaft ? m_shaftSourceRect : m_ladderSourceRect, new Color(1f, 1f, 1f, 0.5f));
                    spriteBatch.DrawBorder(area, 4, color);
                }

                if (ModEntry.Instance.Config.ShowItems)
                {
                    var positions = FrameworkUtils.GetCirclePositions(ctx.Items.Count, (ctx.Items.Count % 2) * halfPi).ToArray();
                    for (int i = 0; i < ctx.Items.Count; i++)
                    {
                        var spawn = ctx.Items[i];
                        var circleOffset = positions[i] * size * 0.25f;
                        spawn.Draw(spriteBatch, pos + centerOffset + circleOffset - scaleOffset, scale);
                    }
                }

                if (ModEntry.Instance.Config.ShowOutlines && drawOutline)
                {
                    var area = new Rectangle((int)pos.X, (int)pos.Y, (int)size, (int)size);
                    spriteBatch.DrawBorder(area, 1, FrameworkUtils.API.OutlineColor);
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
            if (ModEntry.Instance.Config.RequireTool && Game1.player.CurrentTool is not Pickaxe)
            {
                return;
            }

            foreach (var (pos, obj) in Game1.player.currentLocation.Objects.Pairs)
            {
                // skip non-stone objects too far away.
                var location = pos.ToLocation();
                if (!obj.IsBreakableStone() || !Game1.viewport.Contains(location * FrameworkUtils.TileSize))
                {
                    continue;
                }

                PredictionContext ctx = new();
                Game1.currentLocation.Predict_OnStoneDestroyed(ctx, obj.ItemId, location.X, location.Y, Game1.player);

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

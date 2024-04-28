using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace Predictor.Framework.UI
{
    public class ItemGroupSprite : IUIElement
    {
        public readonly PredictionItem[] Items;
        private float scale;
        private Rectangle? bounds = null;

        public ItemGroupSprite(IEnumerable<PredictionItem> items, float scale = 1)
        {
            this.Items = items.ToArray();
            this.scale = scale * ModEntry.Instance.Config.MenuScale;
        }

        public virtual void Draw(SpriteBatch sb)
        {
            var bounds = GetBounds();
            var pos = bounds.Location.ToVector2();
            var scaleOffset = Vector2.One * PredictionItem.TextureSize * 0.5f * scale;
            var centerOffset = Vector2.One * bounds.Width * 0.5f;
            var s = Items.Length < 2 ? 0.85f : (float)(0.3 + Math.Max(Math.Pow(1f / Items.Length, 1.2f), 0.3));

            var positions = Utils.GetCirclePositions(Items.Length, Items.Length % 2 == 0 ? 0 : Math.PI / 2).ToArray();
            for (int i = 0; i < Items.Length; i++)
            {
                var spawn = Items[i];
                var circleOffset = positions[i] * PredictionItem.TextureSize * 0.25f;
                spawn.Draw(sb, pos + centerOffset + (circleOffset - scaleOffset) * s, scale * s);
            }
        }

        public void Update(Vector2? offset = null, int maxWidth = -1)
        {
            var pos = offset ?? Vector2.Zero;
            var size = PredictionItem.TextureSize * scale;
            this.bounds = new Rectangle((int)pos.X, (int)pos.Y, (int)size, (int)size);
        }

        public Rectangle GetBounds()
        {
            if (this.bounds == null)
            {
                this.Update();
            }
            return this.bounds ?? Rectangle.Empty;
        }

        public IUIElement? GetChildAt(Vector2 position)
        {
            return null;
        }

        public void DrawTooltips(SpriteBatch sb)
        {
            var bounds = GetBounds();
            var pos = bounds.Location.ToVector2() + new Vector2(0, bounds.Height + 6 * scale);
            var textOffset = Vector2.One * 10 * scale;
            var text = String.Join("\n", Items.Select(x => x.DisplayName));
            var size = Game1.smallFont.MeasureString(text);

            Utils.DrawMenuTextureBox(sb, new Rectangle((int)pos.X, (int)pos.Y, (int)(size.X + 2 * textOffset.X), (int)(size.Y + 2 * textOffset.Y)));
            sb.DrawString(Game1.smallFont, text, pos + textOffset, Utils.TextColor, 0, Vector2.Zero, ModEntry.Instance.Config.MenuScale, SpriteEffects.None, 0);
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Predictor.Framework.Extentions;
using StardewValley;

namespace Predictor.Framework.UI
{
    public class ItemSprite : IUIElement
    {
        public readonly PredictionItem Item;
        private float scale;
        private Rectangle? bounds = null;
        private string? tooltips = null;

        public ItemSprite(PredictionItem item, float scale = 1, string? tooltips = null)
        {
            this.Item = item;
            this.scale = scale * ModEntry.Instance.Config.MenuScale;
            this.tooltips = tooltips;
        }

        public virtual void Draw(SpriteBatch sb)
        {
            var bounds = GetBounds();
            var pos = bounds.Location.ToVector2();
            Item.Draw(sb, pos, scale);
#if DEBUG
            sb.DrawBorder(GetBounds(), 1f, color: Color.Blue);
#endif
        }

        public void Update(Vector2? offset = null)
        {
            var pos = offset ?? Vector2.Zero;
            var width = Item.SourceRect.Width;
            var height = Item.SourceRect.Height;
            this.bounds = new Rectangle((int)pos.X, (int)pos.Y, (int)(width * scale), (int)(height * scale));
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
            var text = tooltips ?? Item.DisplayName;
            if (text.EmptyOrNull())
            {
                return;
            }

            var size = Game1.smallFont.MeasureString(text);
            Utils.DrawMenuTextureBox(sb, new Rectangle((int)pos.X, (int)pos.Y, (int)(size.X + 2 * textOffset.X), (int)(size.Y + 2 * textOffset.Y)));
            sb.DrawString(Game1.smallFont, text, pos + textOffset, Utils.TextColor, 0, Vector2.Zero, ModEntry.Instance.Config.MenuScale, SpriteEffects.None, 0);
        }
    }
}

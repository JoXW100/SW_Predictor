using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Predictor.Framework.Extentions;
using StardewValley;

namespace Predictor.Framework.UI
{
    public class Label : IUIElement
    {
        protected SpriteFont font;
        protected Color color;
        protected Rectangle? bounds = null;

        protected string mText;
        public virtual string Text 
        {
            get => mText;
            protected set => mText = value;
        }

        public Label(string? text, SpriteFont? font = null, Color? color = null)
        {
            this.mText = text ?? "";
            this.font = font ?? Game1.smallFont;
            this.color = color ?? Utils.TextColor;
        }

        public virtual void Draw(SpriteBatch sb)
        {
            var bounds = GetBounds();
            var pos = bounds.Location.ToVector2();
            sb.DrawString(this.font, Text, pos, color);
            // sb.DrawBorder(bounds, 1f, color: Color.Green);
        }

        public Rectangle GetBounds()
        {
            if (this.bounds == null)
            {
                this.Update();
            }
            return this.bounds ?? Rectangle.Empty;
        }

        public void Update(Vector2? offset = null)
        {
            var pos = offset ?? Vector2.Zero;
            var size = this.font.MeasureString(this.Text) * ModEntry.Instance.Config.MenuScale;
            this.bounds = new Rectangle((int)pos.X, (int)pos.Y, (int)size.X, (int)size.Y);
        }

        public IUIElement? GetChildAt(Vector2 position)
        {
            return null;
        }

        public void DrawTooltips(SpriteBatch sb) { }
    }
}

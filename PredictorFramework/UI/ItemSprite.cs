using DynamicUIFramework.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PredictorPatchFramework.UI
{
    public class ItemSprite : UIElement
    {
        public PredictionItem Item { get; }

        protected Vector2 m_scale;

        /// <summary>
        /// Draws the sprite of <paramref name="item"/> in the UI.
        /// </summary>
        /// <param name="item">The item to draw.</param>
        /// <param name="scale">The scale of the icon.</param>
        public ItemSprite(PredictionItem item, Vector2? scale = null)
        {
            Item = item;
            m_scale = scale ?? Vector2.One;
            UpdateBounds();
        }

        private void UpdateBounds(Point? offset = null)
        {
            var pos = offset ?? Point.Zero;
            var width = Item.SourceRect.Width;
            var height = Item.SourceRect.Height;
            Bounds = new Rectangle(pos.X, pos.Y, (int)(width * m_scale.X), (int)(height * m_scale.Y));
        }

        public override void Draw(SpriteBatch sb, Point? offset = null, Point? size = null)
        {
            if (!IsVisible)
            {
                HasChanged = false;
                return;
            }

            DrawStart(sb, offset, size);
            HasChanged = false;
            UpdateBounds(offset);
            CheckCursorEvents();
            Item.Draw(sb, Bounds.Location.ToVector2(), m_scale);
            DrawDebug(sb);
            DrawEnd(sb, offset, size);
        }

        public void DrawProgressBar(SpriteBatch sb, float progress)
        {
            if (!IsVisible)
            {
                return;
            }

            Item.DrawProgressBar(sb, Bounds.Location.ToVector2(), progress, m_scale);
        }
    }
}

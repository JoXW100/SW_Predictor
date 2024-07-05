using DynamicUIFramework.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PredictorPatchFramework.UI
{
    public class ItemGroupSprite : UIElement
    {
        public IReadOnlyCollection<PredictionItem> Items => m_items;

        protected PredictionItem[] m_items;
        protected Vector2 m_scale;

        /// <summary>
        /// Draws the sprites of <paramref name="items"/> in the UI.
        /// </summary>
        /// <param name="items">The items to draw.</param>
        /// <param name="scale">The scale of the icons.</param>
        public ItemGroupSprite(IEnumerable<PredictionItem> items, Vector2? scale = null)
        {
            m_items = items.ToArray();
            m_scale = scale ?? Vector2.One;
            Bounds = new Rectangle(0, 0, (int)(PredictionItem.TextureSize * m_scale.X), (int)(PredictionItem.TextureSize * m_scale.Y));
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
            var point = offset ?? Point.Zero;
            Bounds = new Rectangle(point.X, point.Y, (int)(PredictionItem.TextureSize * m_scale.X), (int)(PredictionItem.TextureSize * m_scale.Y));
            CheckCursorEvents();

            var pos = point.ToVector2();
            var scaleOffset = m_scale * PredictionItem.TextureSize * 0.5f;
            var centerOffset = Vector2.One * Bounds.Width * 0.5f;
            var s = m_items.Length < 2 ? 0.85f : (float)(0.3 + Math.Max(Math.Pow(1f / m_items.Length, 1.2f), 0.3));

            var positions = FrameworkUtils.GetCirclePositions(m_items.Length, m_items.Length % 2 == 0 ? 0 : Math.PI / 2).ToArray();
            for (int i = 0; i < m_items.Length; i++)
            {
                var spawn = m_items[i];
                var circleOffset = positions[i] * PredictionItem.TextureSize * 0.25f;
                spawn.Draw(sb, pos + centerOffset + (circleOffset - scaleOffset) * s, m_scale * s);
            }
            DrawDebug(sb);
            DrawEnd(sb, offset, size);
        }
    }
}

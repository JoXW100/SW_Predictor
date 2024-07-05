using DynamicUIFramework.Drawables;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DynamicUIFramework.Elements
{
    public class Sprite : UIElement
    {
        protected UITexture m_texture;
        protected Point m_size;

        /// <summary>
        /// An UI sprite.
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="size"></param>
        public Sprite(UITexture texture, Point? size = null)
        {
            m_texture = texture;
            m_size = size ?? texture.Size;
            Update(m_size);
        }

        /// <summary>
        /// Updates the child offsets and group size.
        /// </summary>
        /// <param name="offset">The draw offset.</param>
        protected void Update(Point? size = null)
        {
            Bounds = new Rectangle(Bounds.Location, size ?? m_texture.Size);
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
            Bounds = new Rectangle(offset ?? Point.Zero, Bounds.Size);
            CheckCursorEvents();

            m_texture.Draw(sb, Bounds.Location, Bounds.Size);
            DrawDebug(sb);
            DrawEnd(sb, offset, size);
        }
    }
}

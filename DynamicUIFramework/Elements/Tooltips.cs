using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace DynamicUIFramework.Elements
{
    public class Tooltips : UIElement
    {
        protected readonly IUIElement m_parent;
        protected readonly IUIElement m_content;
        protected readonly IUIDrawable? m_background;
        protected readonly Vector4 m_padding; // top - right - bottom - left
        protected readonly float m_spacing;
        protected Point? m_prevSize;

        public Tooltips(IUIElement parent, IUIElement content, IUIDrawable? background, Vector4? padding = null, float spacing = 0)
        {
            m_parent = parent;
            m_content = content;
            m_background = background;
            m_padding = padding ?? Vector4.Zero;
            m_spacing = spacing;
        }

        protected Vector2 CalcOriginPosition(Vector2 size)
        {
            var parentBounds = m_parent.Bounds;
            var pos = new Vector2(parentBounds.X + (parentBounds.Width - size.X) / 2f, parentBounds.Y + parentBounds.Height + m_spacing);

            // Bound by viewport
            if (pos.X < 0)
            {
                pos.X = 0;
            }
            else if (pos.X + size.X > Game1.viewport.Width)
            {
                pos.X = Game1.viewport.Width - size.X;
            }

            if (pos.Y < 0)
            {
                pos.Y = 0;
            }
            else if (pos.Y + size.Y > Game1.viewport.Height)
            {
                pos.Y = Game1.viewport.Height - size.Y;
            }

            return pos;
        }

        /// <summary>
        /// Updates the position.
        /// </summary>
        /// <param name="size">The draw size.</param>
        protected void Update(Point? size = null)
        {
            m_prevSize = size;
            var contentBounds = m_content.Bounds;
            var boxSize = new Vector2(contentBounds.Width + m_padding.Y + m_padding.W, contentBounds.Height + m_padding.X + m_padding.Z);
            if (size is not null)
            {
                boxSize.X = Math.Max(boxSize.X, size.Value.X);
                boxSize.Y = Math.Max(boxSize.Y, size.Value.Y);
            }

            var pos = CalcOriginPosition(boxSize);
            Bounds = new Rectangle(pos.ToPoint(), boxSize.ToPoint());
        }

        public override void Draw(SpriteBatch sb, Point? offset = null, Point? size = null)
        {
            if (!IsVisible)
            {
                HasChanged = false;
                return;
            }

            DrawStart(sb, offset, size);
            HasChanged = m_parent.HasChanged || m_content.HasChanged || m_prevSize != size;
            if (HasChanged)
            {
                Update(size);
            }
            CheckCursorEvents();

            m_background?.Draw(sb, Bounds.Location, Bounds.Size);
            m_content.Draw(sb, new Point((int)(Bounds.X + m_padding.W), (int)(Bounds.Y + m_padding.X)));

            DrawDebug(sb);
            DrawEnd(sb, offset, size);
        }
    }
}

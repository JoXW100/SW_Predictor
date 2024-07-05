using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DynamicUIFramework.Elements
{
    public class Group : UIElement
    {
        public override IEnumerable<IUIElement?> Children
        {
            get => m_children;
            set
            {
                if (m_children != value)
                {
                    m_children = value.ToArray();
                    m_childOffsets = new Point[m_children.Length];
                    HasChanged = true;
                    Update(m_prevSize);
                }
            }
        }

        protected readonly IUIDrawable? m_background;
        protected readonly Vector4 m_padding; // top - right - bottom - left
        protected readonly float m_spacing; // horizontal / vertival
        protected readonly bool m_horizontal;

        protected Point[] m_childOffsets;
        protected Point? m_prevSize;

        /// <summary>
        /// Positions child elements in a specified <paramref name="direction"/>. 
        /// </summary>
        /// <param name="children">Elements to align to the specified <paramref name="direction"/>, <see langword="null"/> values are ignored.</param>
        /// <param name="padding">Space added between the grid bounds, and the contained <paramref name="children"/>, (top, right, bottom, left).</param>
        /// <param name="spacing">Space added between <paramref name="children"/>.</param>
        /// <param name="background">The background, defaults to transparent.</param>
        /// <param name="direction">The direction to position <paramref name="children"/>.</param>
        public Group(IEnumerable<IUIElement?>? children = null, Vector4? padding = null, float spacing = 0, IUIDrawable? background = null, Direction direction = Direction.Horizontal) : base(children)
        {
            m_padding = padding ?? Vector4.Zero;
            m_spacing = spacing;
            m_background = background;
            m_horizontal = direction == Direction.Horizontal;

            m_childOffsets = new Point[m_children.Length];
            m_prevSize = null;
            Update(m_prevSize);
        }

        /// <summary>
        /// Updates the child offsets and group size.
        /// </summary>
        /// <param name="size">The draw size.</param>
        protected void Update(Point? size = null)
        {
            m_prevSize = size;
            var gridWidth = 0f;
            var gridHeight = 0f;
            var innerPos = new Vector2(m_padding.Y, m_padding.X);
            for (int i = 0; i < m_children.Length; i++)
            {
                var child = m_children[i];

                if (child is not null && child.IsVisible)
                {
                    m_childOffsets[i] = innerPos.ToPoint();

                    var bounds = child.Bounds;
                    if (m_horizontal)
                    {
                        gridWidth = Math.Max(gridWidth, innerPos.X + bounds.Width);
                        gridHeight = Math.Max(gridHeight, bounds.Height);
                        innerPos.X += bounds.Width + m_spacing;
                    }
                    else
                    {
                        gridWidth = Math.Max(gridWidth, bounds.Width);
                        gridHeight = Math.Max(gridHeight, innerPos.Y + bounds.Height);
                        innerPos.Y += bounds.Height + m_spacing;
                    }
                }
            }

            var newSize = new Point((int)(gridWidth + m_padding.W), (int)(gridHeight + m_padding.Z));
            if (size is not null)
            {
                newSize.X = Math.Max(newSize.X, size.Value.X);
                newSize.Y = Math.Max(newSize.Y, size.Value.Y);
            }
            Bounds = new Rectangle(Bounds.Location, newSize);
        }

        public override void Draw(SpriteBatch sb, Point? offset = null, Point? size = null)
        {
            if (!IsVisible)
            {
                HasChanged = false;
                return;
            }

            DrawStart(sb, offset, size);
            HasChanged = size != m_prevSize || m_children.Any(child => child is not null && child.HasChanged);
            if (HasChanged)
            {
                Update(size);
            }

            Bounds = new Rectangle(offset ?? Point.Zero, Bounds.Size);
            CheckCursorEvents();

            m_background?.Draw(sb, Bounds.Location, Bounds.Size);
            for (int i = 0; i < m_children.Length; i++)
            {
                var child = m_children[i];
                child?.Draw(sb, m_childOffsets[i] + offset, null);
            }

            DrawDebug(sb);
            DrawEnd(sb, offset, size);
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DynamicUIFramework.Elements
{
    public class Grid : UIElement
    {
        private const int AutoValue = -1;

        public override IEnumerable<IUIElement?> Children
        {
            get => m_children;
            set
            {
                if (m_children != value)
                {
                    m_children = value.ToArray();
                    m_childCells = new Rectangle[m_children.Length];
                    Update(m_prevSize);
                    HasChanged = true;
                }
            }
        }

        protected readonly IUIDrawable? m_background;
        protected readonly Vector4 m_padding; // top - right - bottom - left
        protected readonly Vector2 m_spacing; // horizontal - vertical
        protected readonly Vector2 m_childAlignment;
        protected readonly int[] m_layout; // space separated column measurements ("auto", [number])

        protected int[] m_layoutWidths;
        protected int[] m_layoutHeights;
        protected Rectangle[] m_childCells;
        protected Point? m_prevSize;

        /// <summary>
        /// Positions and aligns child elements to a specified <paramref name="layout"/>, ensuring same width of columns, and heigh of rows. 
        /// </summary>
        /// <param name="children">Elements to align to the specified <paramref name="layout"/>, <see langword="null"/> values can be used to skip columns, a whole row of null values is ignored.</param>
        /// <param name="padding">Space added between the grid bounds, and the contained <paramref name="children"/>, (top, right, bottom, left).</param>
        /// <param name="spacing">Space added between <paramref name="children"/>.</param>
        /// <param name="background">The background, defaults to transparent.</param>
        /// <param name="layout">The column layout of the grid, each column is specified by a number, or "auto" separated by a space. Example: <code>"30 30 auto"</code></param>
        /// <param name="childAlignment">The direction to align <paramref name="children"/> inside their grid cells.</param>
        public Grid(IEnumerable<IUIElement?>? children = null, Vector4? padding = null, Vector2? spacing = null, IUIDrawable? background = null, string layout = "auto", Alignment childAlignment = Alignment.TopLeft) : base(children)
        {
            m_padding = padding ?? Vector4.Zero;
            m_spacing = spacing ?? Vector2.Zero;
            m_background = background;
            m_childAlignment = ConvertAlignment(childAlignment);

            m_layoutWidths = Array.Empty<int>();
            m_layoutHeights = Array.Empty<int>();
            m_prevSize = null;

            // Update layout
            m_layout = ParseLayout(layout);
            m_childCells = new Rectangle[m_children.Length];
            Update(m_prevSize);
        }

        /// <summary>
        /// Updates the grid cells and grid size.
        /// </summary>
        /// <param name="size">The specified grid size.</param>
        protected void Update(Point? size = null)
        {
            m_prevSize = size;
            CalcLayout(m_children, m_layout, out m_layoutWidths, out m_layoutHeights);

            var gridWidth = 0f;
            var gridHeight = 0f;
            var column = 0;
            var row = 0;
            var flag = false;
            var innerOffset = new Vector2(m_padding.Y, m_padding.X);
            var innerPos = Vector2.Zero;

            for (int i = 0; i < m_children.Length; i++)
            {
                var child = m_children[i];
                var columnWidth = m_layoutWidths[column];
                var rowHeight = m_layoutHeights[row];

                if (column > 0)
                {
                    innerPos.X += m_layoutWidths[column - 1] + m_spacing.X;
                }

                if (child is not null && child.IsVisible)
                {
                    flag = true;
                    var alignment = new Vector2(Math.Max(columnWidth - child.Bounds.Width, 0), Math.Max(rowHeight - child.Bounds.Height, 0)) * m_childAlignment;
                    var pos = innerPos + innerOffset + alignment;
                    m_childCells[i] = new Rectangle((int)pos.X, (int)pos.Y, columnWidth, rowHeight);

                    gridWidth = Math.Max(gridWidth, innerPos.X + columnWidth);
                    gridHeight = Math.Max(gridHeight, innerPos.Y + rowHeight);
                }

                // New row
                if (++column >= m_layoutWidths.Length)
                {
                    if (flag)
                    {
                        innerPos.Y += rowHeight + m_spacing.Y;
                    }

                    ++row;
                    column = 0;
                    innerPos.X = 0;
                    flag = false;
                }
            }

            var newSize = new Point((int)(gridWidth + m_padding.Y + m_padding.W), (int)(gridHeight + m_padding.X + m_padding.Z));
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
                child?.Draw(sb, m_childCells[i].Location + offset); // , m_childCells[i].Size
            }

            DrawDebug(sb);
            DrawEnd(sb, offset, size);
        }

        protected override void DrawDebug(SpriteBatch sb)
        {
            if (DebugEnabled && IsVisible)
            {
                base.DrawDebug(sb);
                foreach (var cell in m_childCells)
                {
                    sb.DrawBorder(new Rectangle(cell.Location + Bounds.Location, cell.Size), 1f, Color.Magenta);
                }
            }
        }

        /// <summary>
        /// Converts an alignment to a 2d vector.
        /// </summary>
        /// <param name="alignment">The alignment to convert.</param>
        /// <returns>The vector representation of the alignment.</returns>
        protected static Vector2 ConvertAlignment(Alignment alignment)
        {
            return alignment switch
            {
                Alignment.TopLeft => Vector2.Zero,
                Alignment.TopCenter or Alignment.Top => new Vector2(0.5f, 0f),
                Alignment.TopRight => new Vector2(1f, 0f),
                Alignment.CenterLeft or Alignment.Left => new Vector2(0.0f, 0.5f),
                Alignment.Center => new Vector2(0.5f, 0.5f),
                Alignment.CenterRight or Alignment.Right => new Vector2(1.0f, 0.5f),
                Alignment.BottomLeft => new Vector2(0f, 1f),
                Alignment.BottomCenter or Alignment.Bottom => new Vector2(0.5f, 1f),
                Alignment.BottomRight => new Vector2(1f, 1f),
                _ => Vector2.Zero,
            };
        }

        /// <summary>
        /// Converts layout string to <see langword="int"/> <see cref="Array"/> representation.
        /// </summary>
        /// <param name="layout">The layout string.</param>
        /// <returns></returns>
        protected static int[] ParseLayout(string layout)
        {
            return layout.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(x => x switch
            {
                "auto" => AutoValue,
                _ => int.TryParse(x, out var res) && res > 0 ? res : 0
            }).ToArray();
        }

        /// <summary>
        /// Calculates the grid child cell layout.
        /// </summary>
        /// <param name="children">The children in the grid.</param>
        /// <param name="layout">The <see langword="int"/> <see cref="Array"/> representation of the grid layout.</param>
        /// <param name="widths">The calculated column widths.</param>
        /// <param name="heights">The calcuated row heights.</param>
        protected static void CalcLayout(IUIElement?[] children, int[] layout, out int[] widths, out int[] heights)
        {
            widths = new int[layout.Length];
            heights = new int[(children.Length + layout.Length - 1) / layout.Length];
            for (int i = 0; i < layout.Length; i++)
            {
                var width = layout[i];
                if (width >= 0)
                {
                    widths[i] = width;
                }
            }

            int columnIndex = 0;
            int rowIndex = 0;
            for (int i = 0; i < children.Length; i++)
            {
                var child = children[i];
                var width = layout[columnIndex];
                var bounds = child is not null && child.IsVisible ? child.Bounds : Rectangle.Empty;

                if (width == AutoValue)
                {
                    widths[columnIndex] = Math.Max(widths[columnIndex], bounds.Width);
                }

                heights[rowIndex] = Math.Max(heights[rowIndex], bounds.Height);

                if (++columnIndex >= layout.Length)
                {
                    columnIndex = 0;
                    ++rowIndex;
                }
            }
        }
    }
}

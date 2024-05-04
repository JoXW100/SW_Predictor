using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Predictor.Framework.UI
{
    public class Grid : IUIElement
    {
        protected IUIElement?[] children;
        protected Vector4 padding; // top - right - bottom - left
        protected Vector2 spacing; // horizontal - vertical
        protected Vector2 offset;  // horizontal - vertical
        protected Color backgroundColor; // TODO: backgorund object for better customization
        protected int[] layout; // space separated column measurements ("auto", "fill", [number])
        protected Vector2 childAlignment;
        protected Rectangle? bounds = null;

        public bool IsEmpty => children.All(x => x == null);

        public Grid(IEnumerable<IUIElement?>? children = null, Vector4? padding = null, Vector2? spacing = null, Vector2? offset = null, Color? backgroundColor = null, string layout = "auto", string childAlignment = "left")
        {
            this.children = children?.ToArray() ?? Array.Empty<IUIElement?>();
            this.padding = (padding ?? Vector4.Zero) * ModEntry.Instance.Config.MenuScale;
            this.spacing = (spacing ?? Vector2.Zero) * ModEntry.Instance.Config.MenuScale;
            this.offset = offset ?? Vector2.Zero;
            this.backgroundColor = backgroundColor ?? Color.Transparent;
            this.layout = ParseLayout(layout);

            this.childAlignment = childAlignment switch
            {
                "center" => new Vector2(0.5f, 0.5f),
                "top center" => new Vector2(0.5f, 0f),
                "right" => new Vector2(1f, 0f),
                "bottom" => new Vector2(0f, 1f),
                "bottom right" => new Vector2(1f, 1f),
                "bottom center" => new Vector2(0.5f, 1f),
                _ => Vector2.Zero,
            };
        }

        private static int[] ParseLayout(string layout)
        {
            return layout.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(x => x switch
            {
                "auto" => -1,
                "fill" => -2,
                _ => int.TryParse(x, out var res) && res > 0 ? res : 0
            }).ToArray();
        }

        private static Tuple<int[], int[]> CalcLayout(IUIElement?[] children, int[] layout)
        {
            var widths = new int[layout.Length];
            var heights = new int[(children.Length + layout.Length - 1) / layout.Length];
            for (int i = 0; i < layout.Length; i++)
            {
                var width = layout[i];
                if (width >= 0)
                {
                    widths[i] = width;
                }
                // else if (width == -2)
                // {
                //     fillIndices.Add(i);
                // }
            }

            int columnIndex = 0;
            int rowIndex = 0;
            for (int i = 0; i < children.Length; i++)
            {
                var child = children[i];
                var width = layout[columnIndex];
                var bounds = child?.GetBounds() ?? Rectangle.Empty;
                if (width == -1)
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

            return new Tuple<int[], int[]>(widths, heights);
        }

        public void Update(Vector2? offset = null)
        {
            var gridWidth = 0f;
            var gridHeight = 0f;
            var column = 0;
            var row = 0;
            var flag = false;
            var innerOffset = new Vector2(padding.Y, padding.X) + Utils.MenuPadding + offset;
            var innerPos = Vector2.Zero;
            var gridLayout = CalcLayout(children, layout);
            var layoutWidths = gridLayout.Item1;
            var layoutHeights = gridLayout.Item2;

            for (int i = 0; i < children.Length; i++)
            {
                var child = children[i];
                var rowWidth = layoutWidths[column];
                var columnHeight = layoutHeights[row];

                if (child is not null)
                {
                    var bounds = child.GetBounds();
                    var alignment = new Vector2(Math.Max(rowWidth - bounds.Width, 0), Math.Max(columnHeight - bounds.Height, 0)) * childAlignment;
                    child.Update(innerPos + innerOffset + alignment);
                    flag = true;
                }

                gridWidth = Math.Max(gridWidth, innerPos.X + rowWidth);
                gridHeight = Math.Max(gridHeight, innerPos.Y + columnHeight);

                // New row
                if (++column >= layoutWidths.Length)
                {
                    if (flag)
                    {
                        innerPos.Y += columnHeight + spacing.Y;
                    }

                    ++row;
                    column = 0;
                    innerPos.X = 0;
                    flag = false;
                }
                else
                {
                    innerPos.X += rowWidth + spacing.X;
                }
            }

            gridWidth += padding.Y + padding.W + Utils.MenuPadding.X * 2;
            gridHeight += padding.X + padding.Z + Utils.MenuPadding.Y * 2;

            var pos = offset ?? Vector2.Zero;
            this.bounds = new Rectangle((int)pos.X, (int)pos.Y, (int)gridWidth, (int)gridHeight);
        }

        public void Draw(SpriteBatch sb)
        {
            if (backgroundColor.A > 0)
            {
                Utils.DrawMenuTextureBox(sb, GetBounds(), backgroundColor);
            }

            for (int i = 0; i < children.Length; i++)
            {
                children[i]?.Draw(sb);
            }

            // sb.DrawBorder(GetBounds(), 1f, color: Color.Red);
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
            foreach (var child in children) 
            { 
                if (child is not null && child.GetBounds().Contains(position))
                {
                    return child.GetChildAt(position) ?? child;
                }
            }
            return null;
        }

        public void DrawTooltips(SpriteBatch sb) { }
    }
}

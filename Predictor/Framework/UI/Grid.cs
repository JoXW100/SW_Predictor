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
        protected Rectangle? bounds = null;

        public Grid(IEnumerable<IUIElement?>? children = null, Vector4? padding = null, Vector2? spacing = null, Vector2? offset = null, Color? backgroundColor = null, string layout = "auto")
        {
            this.children = children?.ToArray() ?? Array.Empty<IUIElement?>();
            this.padding = (padding ?? Vector4.Zero) * ModEntry.Instance.Config.MenuScale;
            this.spacing = (spacing ?? Vector2.Zero) * ModEntry.Instance.Config.MenuScale;
            this.offset = offset ?? Vector2.Zero;
            this.backgroundColor = backgroundColor ?? Color.Transparent;
            this.layout = ParseLayout(layout);
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

        private static int[] CalcLayoutWidths(IUIElement?[] children, int[] layout, int maxWidth)
        {
            var fillIndices = new HashSet<int>();
            var result = new int[layout.Length];
            for (int i = 0; i < layout.Length; i++)
            {
                var width = layout[i];
                if (width >= 0)
                {
                    result[i] = width;
                }
                else if (width == -2)
                {
                    fillIndices.Add(i);
                }
            }

            int columnIndex = 0;
            var fillWidth = fillIndices.Count > 0 ? maxWidth / fillIndices.Count : 0;
            for (int i = 0; i < children.Length; i++)
            {
                if (columnIndex >= layout.Length)
                {
                    columnIndex = 0;
                }

                var width = layout[columnIndex];
                if (width == -1 || (width == -2 && fillWidth < 0))
                {
                    var child = children[i];
                    result[columnIndex] = Math.Max(result[columnIndex], child?.GetBounds().Width ?? 0);
                }

                columnIndex++;
            }

            if (fillWidth >= 0)
            {
                foreach (var i in fillIndices)
                {
                    var width = layout[i];
                    if (width == -2)
                    {
                        result[i] = fillWidth;
                    }
                }
            }

            return result;
        }

        public void Update(Vector2? offset = null, int maxWidth = -1)
        {
            var width = 0f;
            var height = 0f;
            var rowWidth = 0f;
            var rowHeight = 0f;
            var column = 0;
            var flag = false;
            var innerOffset = new Vector2(padding.Y, padding.X) + Utils.MenuPadding + offset;
            var layoutWidths = CalcLayoutWidths(children, layout, maxWidth);

            for (int i = 0; i < children.Length; i++)
            {
                var child = children[i];
                var childWidth = layoutWidths[column];
                if (child is not null)
                {
                    child.Update(new Vector2(rowWidth, height) + innerOffset, layoutWidths[column]);
                    rowHeight = Math.Max(child.GetBounds().Height, rowHeight);
                    flag = true;
                }

                rowWidth += childWidth;

                if (++column >= layoutWidths.Length)
                {
                    if (flag)
                    {
                        width = Math.Max(width, rowWidth);
                        height += rowHeight;

                        if (i + 1 != children.Length)
                        {
                            height += spacing.Y;
                        }
                    }

                    rowWidth = 0;
                    rowHeight = 0;
                    column = 0;
                    flag = false;
                }
                else if (childWidth > 0)
                {
                    rowWidth += spacing.X;
                }
            }

            width = Math.Max(width, rowWidth) + padding.Y + padding.W + Utils.MenuPadding.X * 2;
            height += rowHeight + padding.X + padding.Z + Utils.MenuPadding.Y * 2;

            var pos = offset ?? Vector2.Zero;
            this.bounds = new Rectangle((int)pos.X, (int)pos.Y, (int)width, (int)height);
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

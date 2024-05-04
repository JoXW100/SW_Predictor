using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Predictor.Framework.UI
{
    internal class Group : IUIElement
    {
        protected IUIElement?[] children;
        protected Vector4 padding; // top - right - bottom - left
        protected float spacing; // horizontal
        protected Vector2 offset;  // horizontal - vertical
        protected Color backgroundColor; // TODO: backgorund object for better customization
        public bool IsEmpty => children.All(x => x == null);
        protected Rectangle? bounds = null;

        public Group(IEnumerable<IUIElement?>? children = null, Vector4? padding = null, float spacing = 0, Vector2? offset = null, Color? backgroundColor = null)
        {
            this.children = children?.ToArray() ?? Array.Empty<IUIElement?>();
            this.padding = (padding ?? Vector4.Zero) * ModEntry.Instance.Config.MenuScale;
            this.spacing = spacing * ModEntry.Instance.Config.MenuScale;
            this.offset = offset ?? Vector2.Zero;
            this.backgroundColor = backgroundColor ?? Color.Transparent;
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

        public void DrawTooltips(SpriteBatch sb) { }

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

        public void Update(Vector2? offset = null)
        {
            var gridWidth = 0f;
            var gridHeight = 0f;
            var innerOffset = new Vector2(padding.Y, padding.X) + Utils.MenuPadding + offset;
            var innerPos = Vector2.Zero;
            for (int i = 0; i < children.Length; i++)
            {
                var child = children[i];

                if (child is not null)
                {
                    child.Update(innerPos + innerOffset);
                    var bounds = child.GetBounds();

                    gridWidth = Math.Max(gridWidth, innerPos.X + bounds.Width);
                    gridHeight = Math.Max(gridHeight, bounds.Y);

                    innerPos.X += bounds.Width + spacing;
                }
            }

            var pos = offset ?? Vector2.Zero;
            this.bounds = new Rectangle((int)pos.X, (int)pos.Y, (int)gridWidth, (int)gridHeight);
        }
    }
}

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewValley.Menus;
using StardewValley;

namespace DynamicUIFramework.Backgrounds
{
    public class DefaultMenuBackground : IUIDrawable
    {
        public static Rectangle MenuTextureSourceRect => new(0, 256, 60, 60);

        private readonly Color m_color;

        public DefaultMenuBackground(Color? color = null)
        {
            m_color = color ?? Color.White;
        }

        public void Draw(SpriteBatch sb, Point? offset = null, Point? size = null)
        {
            var area = new Rectangle(offset ?? Point.Zero, size ?? Point.Zero);
            if (area.Width > 0 && area.Height > 0)
            {
                IClickableMenu.drawTextureBox(sb, Game1.menuTexture, MenuTextureSourceRect, area.X, area.Y, area.Width, area.Height, m_color);
            }
        }
    }
}

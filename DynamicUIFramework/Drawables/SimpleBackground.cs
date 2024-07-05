using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DynamicUIFramework.Backgrounds
{
    public class SimpleBackground : IUIDrawable
    {
        private readonly Color m_color;
        private readonly Color m_borderColor;
        private readonly float m_boderWidth;

        public SimpleBackground(Color color, Color? borderColor = null, float boderWidth = 0f)
        {
            m_color = color;
            m_borderColor = borderColor ?? color;
            m_boderWidth = boderWidth;
        }

        public void Draw(SpriteBatch sb, Point? offset = null, Point? size = null)
        {
            var area = new Rectangle(offset ?? Point.Zero, size ?? Point.Zero);
            if (m_color.A > 0 && area.Size.X > 0 && area.Size.Y > 0)
            {
                sb.DrawArea(area, m_color);
                if (m_borderColor.A > 0 && m_boderWidth > 0)
                {
                    sb.DrawBorder(area, m_boderWidth, m_borderColor);
                }
            }
        }
    }
}

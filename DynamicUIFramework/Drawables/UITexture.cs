using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DynamicUIFramework.Drawables
{
    public class UITexture : IUIDrawable
    {
        protected Texture2D m_texture;
        protected Rectangle m_sourceRect;
        protected Color m_color;
        protected float m_rotation;
        protected Vector2 m_origin;
        protected Vector2 m_scale;

        public Point Size => m_sourceRect.Size;

        public UITexture(Texture2D texture, Rectangle? sourceRect = null, Color? color = null, float rotation = 0, Vector2? origin = null, Vector2? scale = null)
        {
            m_texture = texture;
            m_sourceRect = sourceRect ?? texture.Bounds;
            m_color = color ?? Color.White;
            m_rotation = rotation;
            m_origin = origin ?? Vector2.Zero;
            m_scale = scale ?? Vector2.One;
        }

        public void Draw(SpriteBatch sb, Point? offset = null, Point? size = null)
        {
            var pos = offset ?? Point.Zero;

            var scale = m_scale;
            if (size is not null)
            {
                scale.X = size.Value.X / (float)m_sourceRect.Width;
                scale.Y = size.Value.Y / (float)m_sourceRect.Height;
            }

            sb.Draw(m_texture, pos.ToVector2(), m_sourceRect, m_color, m_rotation, m_origin, scale, SpriteEffects.None, 0f);
        }
    }
}

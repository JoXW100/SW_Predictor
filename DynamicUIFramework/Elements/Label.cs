using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace DynamicUIFramework.Elements
{
    public class Label : UIElement
    {
        public virtual string Text
        {
            get => m_text;
            set
            {
                if (m_text != value)
                {
                    m_text = value;
                    HasChanged = true;
                    Update();
                }
            }
        }

        protected string m_text;
        protected readonly SpriteFont m_font;
        protected readonly Color m_color;

        /// <summary>
        /// An UI text label.
        /// </summary>
        /// <param name="text">The text to draw.</param>
        /// <param name="font">The font of the text, defaults to <see cref="Game1.smallFont"/>.</param>
        /// <param name="color">The color of the text, defaults to <see cref="Color.Black"/>.</param>
        public Label(string text, SpriteFont? font = null, Color? color = null) : base()
        {
            m_text = text;
            m_font = font ?? Game1.smallFont;
            m_color = color ?? Color.Black;
            Update();
        }

        protected void Update()
        {
            Bounds = new Rectangle(Point.Zero, m_font.MeasureString(m_text).ToPoint());
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
            sb.DrawString(m_font, Text, Bounds.Location.ToVector2(), m_color);
            DrawDebug(sb);
            DrawEnd(sb, offset, size);
        }
    }
}

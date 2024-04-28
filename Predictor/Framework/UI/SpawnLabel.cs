using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System.Diagnostics;

namespace Predictor.Framework.UI
{
    public class SpawnLabel : Label
    {
        public readonly Vector2 Location;
        public override string Text 
        {
            get => mText;
            protected set
            {
                var length = mText.Length;

                if (value.Length > length)
                {
                    mText = new string('X', length);
                }
                else
                {
                    mText = string.Format("{0," + length + "}", value);
                }

                Debug.Assert(mText.Length == length);
            }
        }

        public SpawnLabel(Vector2 location, int length, SpriteFont? font = null, Color? color = null) : base(new string('X', length), font, color)
        {
            this.Location = location;
        }

        public override void Draw(SpriteBatch sb)
        {
            this.Text = ((int)(Game1.player.Tile - Location).Length()).ToString();
            base.Draw(sb);
        }
    }
}

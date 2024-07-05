using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DynamicUIFramework.Events
{
    public class DrawUIEventArgs : EventArgs
    {
        public SpriteBatch SpriteBatch { get; }
        public Point? Offset { get; init; }
        public Point? Size { get; init; }

        public DrawUIEventArgs(SpriteBatch spriteBatch)
        {
            SpriteBatch = spriteBatch;
        }
    }
}

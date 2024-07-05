using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DynamicUIFramework
{
    public interface IUIDrawable
    {
        /// <summary>
        /// Draws the drawable to the given sprite batch.
        /// </summary>
        /// <param name="sb">The <see cref="SpriteBatch"/> to draw the drawable to.</param>
        /// <param name="offset">The positional offset when drawing.</param>
        /// <param name="size">The size of the drawable.</param>
        public void Draw(SpriteBatch sb, Point? offset = null, Point? size = null);
    }
}

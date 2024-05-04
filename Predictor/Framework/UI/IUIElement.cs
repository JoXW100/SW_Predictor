using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Predictor.Framework.UI
{
    public interface IUIElement
    {
        /// <summary>
        /// Gets the bounds containing the element
        /// </summary>
        public abstract Rectangle GetBounds();

        /// <summary>
        /// Gets the child element at the given position, or null if no child exists at the given position.
        /// </summary>
        /// <param name="position">The window postion to search</param>
        /// <returns>The child at the given position, or null</returns>
        public abstract IUIElement? GetChildAt(Vector2 position);

        /// <summary>
        /// Updates the UI element
        /// </summary>
        /// <param name="offset">The positional offset to update the location of the element</param>
        public abstract void Update(Vector2? offset = null);

        /// <summary>
        /// Draws the UI element to the given sprite batch
        /// </summary>
        /// <param name="sb">The sprite batch to draw the UI element to</param>
        public abstract void Draw(SpriteBatch sb);

        /// <summary>
        /// Draws the UI element tooltip to the given sprite batch if it has any
        /// </summary>
        /// <param name="sb">The sprite batch to draw the UI element to</param>
        public abstract void DrawTooltips(SpriteBatch sb);
    }
}

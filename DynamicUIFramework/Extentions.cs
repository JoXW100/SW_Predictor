using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace DynamicUIFramework
{
    public static class Extentions
    {
        private static Texture2D Pixel => LazyPixel.Value;

        private static readonly Lazy<Texture2D> LazyPixel = new(delegate
        {
            Texture2D pixel = new(Game1.graphics.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
            return pixel;
        });

        public static void DrawArea(this SpriteBatch spriteBatch, Rectangle area, Color? color = null)
        {
            spriteBatch.Draw(Pixel, area, color ?? Color.White);
        }

        public static void DrawBorder(this SpriteBatch spriteBatch, Rectangle area, float width = 1, Color? color = null)
        {
            var c = color ?? Color.White;
            spriteBatch.Draw(Pixel, new Rectangle(area.X, area.Y, area.Width, (int)width), c);
            spriteBatch.Draw(Pixel, new Rectangle(area.X, area.Y, (int)width, area.Height), c);
            spriteBatch.Draw(Pixel, new Rectangle(area.X + area.Width - (int)width, area.Y, (int)width, area.Height), c);
            spriteBatch.Draw(Pixel, new Rectangle(area.X, area.Y + area.Height - (int)width, area.Width, (int)width), c);
        }
    }
}

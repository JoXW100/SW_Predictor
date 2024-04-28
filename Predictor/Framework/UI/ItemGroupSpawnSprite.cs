using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Predictor.Framework.Extentions;
using StardewValley;

namespace Predictor.Framework.UI
{
    public class ItemGroupSpawnSprite : ItemGroupSprite
    {
        public readonly Vector2 Location;

        public ItemGroupSpawnSprite(IEnumerable<PredictionItem> items, Vector2 location, float scale = 1) : base(items, scale)
        {
            this.Location = location;
        }

        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);

            var direction = Location - Game1.player.Tile;
            var distance = direction.Length();

            var bounds = GetBounds();
            var arrowCenter = bounds.Location.ToVector2() + Vector2.One * bounds.Height * 0.5f;
            var arrowPos = arrowCenter + (direction / distance) * 12f;
            var arrowRect = new Rectangle(412, 495, 5, 4);
            sb.Draw(Game1.mouseCursors, arrowPos, arrowRect, Color.White, (float)(Game1.player.Tile.Angle(Location) + Math.PI / 2), new Vector2(2.25f, 3f), 2.4f, 0, 1f);
        }
    }
}

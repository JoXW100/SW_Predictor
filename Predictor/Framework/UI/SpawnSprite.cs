using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Predictor.Framework.Extentions;
using StardewValley;

namespace Predictor.Framework.UI
{
    internal class SpawnSprite : ItemSprite
    {
        public readonly Vector2 Location;
        private readonly Rectangle ArrowRect = new (412, 495, 5, 4);
        private readonly Vector2 ArrowOrigin = new (2.25f, 3f);

        public SpawnSprite(PredictionItem item, Vector2 location, float scale = 1) : base(item, scale)
        {
            this.Location = location;
        }

        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);

            var direction = (Location - Game1.player.Tile);
            var distance = direction.Length();

            var bounds = GetBounds();
            var arrowCenter = bounds.Location.ToVector2() + Vector2.One * bounds.Height * 0.5f;
            var arrowPos = arrowCenter + (direction / distance) * 12f;
            sb.Draw(Game1.mouseCursors, arrowPos, ArrowRect, Color.White, (float)(Game1.player.Tile.Angle(Location) + Math.PI / 2), ArrowOrigin, 2.4f, 0, 1f);
        }
    }
}

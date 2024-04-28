using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Predictor.Framework.Extentions;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.TokenizableStrings;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace Predictor.Framework
{
    public class PredictionItem
    {
        public const int TextureSize = 16;

        public Texture2D Texture => Data.GetTexture();
        public int SpriteIndex => Data.SpriteIndex;
        public string ItemId => Data.ItemId;
        public string QualifiedItemId => Data.QualifiedItemId;
        public string InternalName => Data.InternalName;
        public string DisplayName => TokenParser.ParseText(Data.DisplayName);
        public int Category => Data.Category;
        public Rectangle SourceRect => Data.GetSourceRect();
        public int Stack { get; set; }
        public int Quality { get; set; }
        public float Durability { get; set; }

        protected readonly ParsedItemData Data;

        public PredictionItem(ParsedItemData data, int stack, int quality, float durability = 1f)
        {
            this.Data = data;
            this.Stack = stack;
            this.Quality = quality;
            this.Durability = durability;
        }

        public PredictionItem(ISalable item)
        {
            this.Data = ItemRegistry.GetData(item.QualifiedItemId) ?? throw new ArgumentException($"{item.Name} has no data.");
            this.Stack = item.Stack;
            this.Quality = item.Quality;
        }

        public PredictionItem(Object item) : this((ISalable)item)
        {
            if (item.Category == -22)
            {
                this.Durability = GetTackleDurability(item.uses.Value);
            }
        }

        public static PredictionItem? Create(string itemId, int stack = 1, int quality = 0, float durability = 1f)
        {
            var data = ItemRegistry.GetData(itemId);
            if (data == null)
            {
                return null;
            }

            return new PredictionItem(data, stack, quality, durability);
        }

        public PredictionItem GetOne()
        {
            return new PredictionItem(Data, 1, Quality);
        }

        public static float GetTackleDurability(int uses)
        {
            return (FishingRod.maxTackleUses - uses) / (float)FishingRod.maxTackleUses;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 pos, float scale, float layer = 0f, bool outline = false)
        {
            Draw(spriteBatch, Texture, SourceRect, pos, scale, Stack, layer);
            if (Durability < 1f)
            {
                DrawProgressBar(spriteBatch, Durability, pos, scale);
            }
        }

        public static void Draw(SpriteBatch spriteBatch, ISalable item, Vector2 pos, float scale, int stack = 1, float layer = 0f)
        {
            var data = ItemRegistry.GetData(item.QualifiedItemId) ?? throw new ArgumentException($"{item.Name} has no data.");
            Draw(spriteBatch, data.GetTexture(), data.GetSourceRect(), pos, scale, stack, layer);
        }

        public static void Draw(SpriteBatch spriteBatch, Texture2D texture, Rectangle sourceRect, Vector2 pos, float scale, int stack = 1, float layer = 0f)
        {
            spriteBatch.Draw(texture, pos, sourceRect, Utils.ItemColor, 0f, Vector2.One * 0.5f, scale, SpriteEffects.None, layer);
            if (stack > 1)
            {
                Utility.drawTinyDigits(stack, spriteBatch, pos, scale, 0f, Utils.TextColor);
            }
        }

        public static void DrawOutline(SpriteBatch spriteBatch, Vector2 pos, float scale, float width, Color color)
        {
            spriteBatch.DrawBorder(new Rectangle((int)pos.X, (int)pos.Y, (int)(TextureSize * scale), (int)(TextureSize * scale)), width, color);
        }

        public static void DrawProgressBar(SpriteBatch spriteBatch, float progress, Vector2 location, float scale = 1)
        {
            var height = 3f;
            spriteBatch.Draw(Game1.staminaRect, new Rectangle((int)location.X, (int)(location.Y + (TextureSize - height) * scale), (int)(TextureSize * scale * progress), (int)(height * scale)), Utility.getRedToGreenLerpColor(progress));
        }
    }
}

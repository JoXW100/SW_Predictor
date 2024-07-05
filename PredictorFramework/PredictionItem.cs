using DynamicUIFramework;
using DynamicUIFramework.Drawables;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.TokenizableStrings;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace PredictorPatchFramework
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
        public int Uses { get; set; }

        protected readonly ParsedItemData Data;

        public static IUIDrawable? QualityIcon(int quality, Vector2? scale = null)
        {
            if (quality <= 0)
            {
                return null;
            }

            Rectangle value = quality < 4 
                ? new Rectangle(338 + (quality - 1) * 8, 400, 8, 8) 
                : new Rectangle(346, 392, 8, 8);

            return new UITexture(Game1.mouseCursors, value, scale: scale);
        }

        public PredictionItem(ParsedItemData data, int stack, int quality, int uses = -1)
        {
            this.Data = data;
            this.Stack = stack;
            this.Quality = quality;
            this.Uses = uses;
        }

        public PredictionItem(ISalable item)
        {
            this.Data = ItemRegistry.GetData(item.QualifiedItemId) ?? throw new ArgumentException($"{item.Name} has no data.");
            this.Stack = item.Stack;
            this.Quality = item.Quality;
        }

        public PredictionItem(Object item) : this((ISalable)item)
        {
            this.Uses = item.uses.Value;
        }

        public static PredictionItem? Create(string itemId, int stack = 1, int quality = 0, int uses = -1)
        {
            var data = ItemRegistry.GetData(itemId);
            if (data == null)
            {
                return null;
            }

            return new PredictionItem(data, stack, quality, uses);
        }

        public PredictionItem GetOne()
        {
            return new PredictionItem(Data, 1, Quality);
        }

        public static float GetTackleDurability(int uses)
        {
            return (FishingRod.maxTackleUses - uses) / (float)FishingRod.maxTackleUses;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 pos, Vector2 scale, float layer = 0f, bool outline = false)
        {
            Draw(spriteBatch, Texture, SourceRect, pos, scale, Stack, layer);
        }

        public void DrawProgressBar(SpriteBatch spriteBatch, Vector2 pos, float progress, Vector2 scale)
        {
            if (Uses != -1)
            {
                DrawProgressBar(spriteBatch, progress, pos, scale);
            }
        }

        public static void Draw(SpriteBatch spriteBatch, ISalable item, Vector2 pos, Vector2 scale, int stack = 1, float layer = 0f)
        {
            var data = ItemRegistry.GetData(item.QualifiedItemId) ?? throw new ArgumentException($"{item.Name} has no data.");
            Draw(spriteBatch, data.GetTexture(), data.GetSourceRect(), pos, scale, stack, layer);
        }

        public static void Draw(SpriteBatch spriteBatch, Texture2D texture, Rectangle sourceRect, Vector2 pos, Vector2 scale, int stack = 1, float layer = 0f)
        {
            var ratio = TextureSize / (float)Math.Max(sourceRect.Width, sourceRect.Height);
            spriteBatch.Draw(texture, pos, sourceRect, FrameworkUtils.API.ItemColor, 0f, Vector2.One * 0.5f, scale, SpriteEffects.None, layer);
            if (stack > 1)
            {
                Utility.drawTinyDigits(stack, spriteBatch, pos, Math.Max(scale.X, scale.Y) * ratio, 0f, FrameworkUtils.API.TextColor);
            }
        }

        public static void DrawOutline(SpriteBatch spriteBatch, Vector2 pos, float scale, float width, Color color)
        {
            spriteBatch.DrawBorder(new Rectangle((int)pos.X, (int)pos.Y, (int)(TextureSize * scale), (int)(TextureSize * scale)), width, color);
        }

        public static void DrawProgressBar(SpriteBatch spriteBatch, float progress, Vector2 location, Vector2? scale = null)
        {
            var scale_v = scale ?? Vector2.One;
            var height = 3f;
            spriteBatch.Draw(Game1.staminaRect, new Rectangle((int)location.X, (int)(location.Y + (TextureSize - height) * scale_v.Y), (int)(TextureSize * scale_v.X * progress), (int)(height * scale_v.Y)), Utility.getRedToGreenLerpColor(progress));
        }
    }
}

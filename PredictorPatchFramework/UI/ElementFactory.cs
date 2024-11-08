using DynamicUIFramework;
using DynamicUIFramework.Drawables;
using DynamicUIFramework.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PredictorPatchFramework.Extensions;
using StardewValley;

namespace PredictorPatchFramework.UI
{
    public static class ElementFactory
    {
        private static readonly Rectangle ArrowRect = new(412, 495, 5, 4);
        private static readonly Vector2 ArrowOrigin = new(2.25f, 3f);

        private static UITexture? m_trashCanTexture = null;
        public static UITexture TrashCanTexture
        {
            get
            {
                if (m_trashCanTexture is not null)
                {
                    return m_trashCanTexture;
                }
                else
                {
                    var data = ItemRegistry.GetData("2427");
                    var rect = data.GetSourceRect();
                    return m_trashCanTexture = new UITexture(data.GetTexture(), new Rectangle(rect.X, rect.Y + 8, rect.Width, rect.Height - 8), scale: new Vector2(1f, 0.5f) * FrameworkUtils.API.UIScale);
                }
            }
        }

        public static T AddTooltips<T>(T element, string tooltips) where T : IUIElement
        {
            element.OnCursorEnter += (s, e) =>
            {
                FrameworkUtils.API.Tooltips = new Tooltips(
                    parent: element,
                    content: new Label(tooltips, Game1.smallFont, FrameworkUtils.API.TextColor),
                    background: FrameworkUtils.API.MenuBackground,
                    padding: FrameworkUtils.API.MenuPadding,
                    spacing: FrameworkUtils.API.MenuSpacing.Y
                );
            };
            element.OnCursorLeave += (s, e) =>
            {
                FrameworkUtils.API.Tooltips = null;
            };
            return element;
        }

        public static T AddTracker<T>(T element, Vector2 location) where T : IUIElement
        {
            element.OnDrawEnd += (s, e) =>
            {
                if (s is IUIElement sprite)
                {
                    var direction = location - Game1.player.Tile;
                    var distance = direction.Length();
                    var bounds = sprite.Bounds;
                    var arrowCenter = bounds.Location.ToVector2() + Vector2.One * bounds.Height * 0.5f;
                    var arrowPos = arrowCenter + (direction / distance) * 12f;
                    var rotation = (float)(Game1.player.Tile.Angle(location) + Math.PI / 2d);
                    e.SpriteBatch.Draw(Game1.mouseCursors, arrowPos, ArrowRect, Color.White, rotation, ArrowOrigin, 2.4f, 0, 1f);
                }
            };
            return element;
        }

        public static Label CreateSpawnLabel(Vector2 location, int length, SpriteFont? font = null, Color? color = null)
        {
            var label = new Label(new string('X', length), font, color);
            label.OnDrawStart += (s, _) =>
            {
                if (s is Label label)
                {
                    var text = ((int)(Game1.player.Tile - location).Length()).ToString();
                    if (text.Length > length)
                    {
                        label.Text = new string('X', length);
                    }
                    else
                    {
                        label.Text = string.Format("{0," + length + "}", text);
                    }
                }
            };
            return label;
        }

        public static Label CreateUpdatingLabel(Func<string> textFactory, SpriteFont? font = null, Color? color = null)
        {
            var label = new Label(textFactory.Invoke(), font, color);
            label.OnDrawStart += (s, _) =>
            {
                if (s is Label item)
                {
                    ((Label) s).Text = textFactory.Invoke();
                }
            };
            return label;
        }

        public static ItemSprite CreateItemSprite(PredictionItem item, Vector2? scale = null, string? tooltips = null)
        {
            var sprite = new ItemSprite(item, (scale ?? Vector2.One) * FrameworkUtils.API.UIScale);
            return AddTooltips(sprite, tooltips ?? item.DisplayName);
        }

        public static ItemSprite CreateItemWithProgressBar(PredictionItem item, Func<PredictionItem, float> progress, Vector2? scale = null, string? tooltips = null)
        {
            var sprite = new ItemSprite(item, (scale ?? Vector2.One) * FrameworkUtils.API.UIScale);
            sprite.OnDrawEnd += (s, e) =>
            {
                if (s is ItemSprite item)
                {
                    item.DrawProgressBar(e.SpriteBatch, progress.Invoke(item.Item));
                }
            };
            return AddTooltips(sprite, tooltips ?? item.DisplayName);
        }

        public static ItemSprite CreateSpawnSprite(PredictionItem item, Vector2 location, Vector2? scale = null, string? tooltips = null)
        {
            var sprite = CreateItemSprite(item, scale, tooltips);
            return AddTracker(sprite, location);
        }

        public static ItemGroupSprite CreateItemGroupSprite(IEnumerable<PredictionItem> items, Vector2? scale = null, string? tooltips = null)
        {
            var sprite = new ItemGroupSprite(items, (scale ?? Vector2.One) * FrameworkUtils.API.UIScale);
            return AddTooltips(sprite, tooltips ?? string.Join("\n", items.Select(item => item.DisplayName)));
        }

        public static ItemGroupSprite CreateItemGroupSpawnSprite(IEnumerable<PredictionItem> items, Vector2 location, Vector2? scale = null, string? tooltips = null)
        {
            var sprite = CreateItemGroupSprite(items, scale, tooltips);
            return AddTracker(sprite, location);
        }
    }
}

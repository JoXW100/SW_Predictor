using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Predictor.Framework.Extentions;
using Predictor.Framework.UI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.TokenizableStrings;

namespace Predictor.Framework
{
    public static class Utils
    {
        public const int TileSize = 64;
        public const float UIScale = 1.7f;
        public static Rectangle MenuTextureSourceRect => new(0, 256, 60, 60);
        public static Color MenuBackground => ModEntry.Instance.Config.MenuType == 1 
            ? new Color(0.1f, 0.1f, 0.1f, ModEntry.Instance.Config.MenuAlpha)
            : new Color(1f, 1f, 1f, ModEntry.Instance.Config.MenuAlpha);
        public static Vector2 MenuPadding => ModEntry.Instance.Config.MenuType == 1
            ? Vector2.Zero
            : Vector2.One * ModEntry.Instance.Config.MenuScale * 4f;
        public static Color ItemColor => new(1f, 1f, 1f, 0.8f);
        public static Color TextColor => ModEntry.Instance.Config.MenuType == 1
            ? Color.White
            : Color.Black;
        public static Color OutlineColor => Color.Red;
        public static Color LadderColor => Color.Green;
        public static Color GarbageCanOkColor => Color.Green;
        public static Color GarbageCanWarnColor => Color.Red;

        /// <summary>
        /// Creates <paramref name="num"/> eveny spaced points in a radius of 1 from a zenter point at zero.
        /// </summary>
        /// <param name="num">The number of points</param>
        /// <returns>Points in a circle around zero</returns>
        public static IEnumerable<Vector2> GetCirclePositions(int num, double angleOffset = 0)
        {
            if (num < 0)
            {
                yield break;
            }
            else if (num < 2)
            {
                yield return Vector2.Zero;
                yield break;
            }

            for (int i = 0; i < num; i++)
            {
                var angle = angleOffset + (i * Math.PI * 2f / num);
                yield return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
            }
        }

        public static void DrawContextItems(SpriteBatch spriteBatch, Dictionary<Vector2, PredictionContext> context, bool drawItems = true, bool drawOutline = false, int width = 1)
        {
            if ((!drawItems && !drawOutline) || context.Count < 1)
            {
                return;
            }

            float ratio = Game1.options.zoomLevel != 1f ? 1f : 1f / Game1.options.uiScale;
            float size = width * Utils.TileSize * ratio;
            double angle = Game1.currentGameTime.TotalGameTime.Milliseconds * Math.PI / 512.0;
            var scale = 2.5f * (1f + (float)(Math.Cos(angle) + 1f) * 0.03f) * ratio;
            var scaleOffset = Vector2.One * PredictionItem.TextureSize * 0.5f * scale;
            var centerOffset = Vector2.One * size * 0.5f;

            foreach (var (posx, ctx) in context)
            {
                var pos = Game1.GlobalToLocal(Game1.viewport, posx * Utils.TileSize) * ratio;
                if (drawItems)
                {
                    var positions = Utils.GetCirclePositions(ctx.Items.Count, ctx.Items.Count % 2 == 0 ? 0 : Math.PI / 2).ToArray();
                    var s = ctx.Items.Count < 2 ? 0.85f : (float)(0.3 + Math.Max(Math.Pow(1f / ctx.Items.Count, 1.2f), 0.3));
                    for (int i = 0; i < ctx.Items.Count; i++)
                    {
                        var spawn = ctx.Items[i];
                        var circleOffset = positions[i] * size * 0.25f;
                        spawn.Draw(spriteBatch, pos + centerOffset + (circleOffset - scaleOffset) * s, scale * s);
                    }
                }
                if (drawOutline)
                {
                    spriteBatch.DrawBorder(new Rectangle((int)pos.X, (int)pos.Y, (int)size, (int)size), 1, Utils.OutlineColor);
                }
            }
        }

        public static void DrawOutlines(SpriteBatch spriteBatch, IEnumerable<Vector2> positions, int width = 1)
        {
            float ratio = Game1.options.zoomLevel != 1f ? 1f : 1f / Game1.options.uiScale;
            float size = width * Utils.TileSize * ratio;
            foreach (var posx in positions)
            {
                var pos = Game1.GlobalToLocal(Game1.viewport, posx * Utils.TileSize) * ratio;
                spriteBatch.DrawBorder(new Rectangle((int)pos.X, (int)pos.Y, (int)size, (int)size), 1, Utils.OutlineColor);
            }
        } 
    
        public static void DrawMenuTextureBox(SpriteBatch spriteBatch, Rectangle area, Color? color = null)
        {
            var c = color ?? Utils.MenuBackground;

            if (ModEntry.Instance.Config.MenuType == 1)
            {
                spriteBatch.DrawArea(area, c);
                spriteBatch.DrawBorder(area, 5f, MenuBackground);
            }
            else
            {
                IClickableMenu.drawTextureBox(spriteBatch, Game1.menuTexture, MenuTextureSourceRect, area.X, area.Y, area.Width, area.Height, c);
            }
        }
    
        public static IUIElement? CreateNearbyContextItemsMenu(Dictionary<Vector2, PredictionItem> context, string headerTranslationId)
        {
            if (context.Count <= 0)
            {
                return null;
            }

            var maxItems = ModEntry.Instance.Config.TrackerMenuMaxItemCount != 0
                ? ModEntry.Instance.Config.TrackerMenuMaxItemCount
                : int.MaxValue;
            Vector2[] positions = context.Keys.OrderBy(p => (p - Game1.player.Tile).Length()).ToArray();
            PredictionItem[] items = positions.Select(k => context[k]).ToArray();

            IUIElement? excessLabel = null;
            List<IUIElement?> children = new();
            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];
                var position = positions[i];
                var direction = (position - Game1.player.Tile);
                var distance = direction.Length();

                if (i >= maxItems)
                {
                    excessLabel = new Label($"+ {items.Length - i} ...", Game1.smallFont);
                    break;
                }

                children.AddRange(new IUIElement?[]
                {
                    new SpawnSprite(item, position, Utils.UIScale),
                    ModEntry.Instance.Config.TrackerMenuShowLess
                        ? null
                        : new Label(TokenParser.ParseText(item.DisplayName), Game1.smallFont),
                    new SpawnLabel(position, 3, Game1.smallFont),
                    new Label(ModEntry.Instance.Helper.Translation.Get("menu.DistanceUnit"), Game1.smallFont)
                });
            }

            return CreateNearbyContextItemsMenu(children, headerTranslationId, excessLabel);
        }

        public static IUIElement? CreateNearbyContextItemsMenu(Dictionary<Vector2, PredictionContext> context, string headerTranslationId)
        {
            if (context.Count <= 0)
            {
                return null;
            }

            var maxItems = ModEntry.Instance.Config.TrackerMenuMaxItemCount != 0
                ? ModEntry.Instance.Config.TrackerMenuMaxItemCount
                : int.MaxValue;
            Vector2[] positions = context.Keys.OrderBy(p => (p - Game1.player.Tile).Length()).ToArray();
            PredictionContext[] items = positions.Select(k => context[k]).Where(k => !k.Items.EmptyOrNull()).ToArray();

            IUIElement? excessLabel = null;
            List<IUIElement?> children = new();
            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];
                var group = item.Items;
                var position = positions[i];

                if (i >= maxItems)
                {
                    excessLabel = new Label($"+ {items.Length - i} ...", Game1.smallFont);
                    break;
                }

                var name = "";
                var found = new HashSet<string>();
                foreach (var value in group)
                {
                    var res = TokenParser.ParseText(value.DisplayName);
                    if (!found.Contains(res))
                    {
                        if (found.Any())
                        {
                            name += ", ";
                        }
                        found.Add(res);
                        name += res;
                    }
                }

                children.AddRange(new IUIElement?[]
                {
                    new ItemGroupSpawnSprite(group, position, Utils.UIScale),
                    ModEntry.Instance.Config.TrackerMenuShowLess
                        ? null
                        : new Label(name, Game1.smallFont),
                    new SpawnLabel(position, 3, Game1.smallFont),
                    new Label(ModEntry.Instance.Helper.Translation.Get("menu.DistanceUnit"), Game1.smallFont)
                });
            }

            return CreateNearbyContextItemsMenu(children, headerTranslationId, excessLabel);
        }

        public static IUIElement? CreateNearbyContextItemsMenu(IEnumerable<IUIElement?> children, string headerTranslationId, IUIElement? excessLabel = null)
        {
            if (children.EmptyOrNull())
            {
                return null;
            }

            var headerText = string.Format("{0}:", ModEntry.Instance.Helper.Translation.Get(headerTranslationId));
            return new Grid(
                children: new[]
                {
                    new Label(headerText, Game1.smallFont),
                    new Grid(children: children, spacing: Vector2.One * 2, layout: "auto auto auto auto"),
                    excessLabel
                },
                padding: Vector4.One * 6 * Utils.UIScale, 
                spacing: Vector2.One * 2,
                backgroundColor: Utils.MenuBackground
            );
        }
    }

    public static class ClassExtentions
    {
        public static bool EmptyOrNull<T>(this IEnumerable<T>? collection)
        {
            return collection is null || !collection.Any();
        }
    }
}

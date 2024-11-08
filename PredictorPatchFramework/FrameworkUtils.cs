
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using DynamicUIFramework;
using DynamicUIFramework.Elements;
using PredictorPatchFramework.Extensions;
using StardewValley.TokenizableStrings;
using PredictorPatchFramework.UI;

namespace PredictorPatchFramework
{
    public static class FrameworkUtils
    {
        public const int TileSize = 64;
        public const int ItemSize = 16;
        public static float Ratio => Game1.options.zoomLevel != 1f ? 1f : 1f / Game1.options.uiScale;

        private static IModHelper? _helper;
        public static IModHelper Helper => _helper ?? throw new ArgumentNullException(nameof(Helper));

        private static IModAPI? _api;
        public static IModAPI API => _api ?? throw new ArgumentNullException(nameof(API));

        /// <summary>
        /// Should be called in the <see cref="IModHelper.Events.GameLoop.GameLaunched"/> event.
        /// </summary>
        /// <param name="helper"></param>
        public static void Initialize(IModHelper helper)
        {
            _helper = helper;
            _api = helper.ModRegistry.GetApi<IModAPI>("JoXW.Predictor");
        }

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

        public static void DrawContextItems(SpriteBatch spriteBatch, Dictionary<Vector2, PredictionContext> context, bool drawItems = true, bool drawOutline = false, int numTilesWidth = 1)
        {
            if ((!drawItems && !drawOutline) || context.Count < 1)
            {
                return;
            }

            float ratio = Ratio;
            float size = numTilesWidth * TileSize * ratio;
            double angle = Game1.currentGameTime.TotalGameTime.Milliseconds * Math.PI / 512.0;
            var scale = 2.5f * (1f + (float)(Math.Cos(angle) + 1f) * 0.03f) * ratio;
            var scaleOffset = Vector2.One * PredictionItem.TextureSize * 0.5f * scale;
            var centerOffset = Vector2.One * size * 0.5f;

            foreach (var (posx, ctx) in context)
            {
                var pos = Game1.GlobalToLocal(Game1.viewport, posx * TileSize) * ratio;
                if (drawItems)
                {
                    var positions = GetCirclePositions(ctx.Items.Count, ctx.Items.Count % 2 == 0 ? 0 : Math.PI / 2).ToArray();
                    var s = ctx.Items.Count < 2 ? 0.85f : (float)(0.3 + Math.Max(Math.Pow(1f / ctx.Items.Count, 1.2f), 0.3));
                    for (int i = 0; i < ctx.Items.Count; i++)
                    {
                        var spawn = ctx.Items[i];
                        var circleOffset = positions[i] * size * 0.25f;
                        spawn.Draw(spriteBatch, pos + centerOffset + (circleOffset - scaleOffset) * s, Vector2.One * scale * s);
                    }
                }
                if (drawOutline)
                {
                    spriteBatch.DrawBorder(new Rectangle((int)pos.X, (int)pos.Y, (int)size, (int)size), API.OutlineWidth, API.OutlineColor);
                }
            }
        }

        public static void DrawOutlines(SpriteBatch spriteBatch, IEnumerable<Vector2> positions, int width = 1)
        {
            float ratio = Ratio;
            float size = width * TileSize * ratio;
            foreach (var posx in positions)
            {
                var pos = Game1.GlobalToLocal(Game1.viewport, posx * TileSize) * ratio;
                spriteBatch.DrawBorder(new Rectangle((int)pos.X, (int)pos.Y, (int)size, (int)size), API.OutlineWidth, API.OutlineColor);
            }
        }

        public static void DrawMenuTextureBox(SpriteBatch spriteBatch, Rectangle area, Color? color = null)
        {
            API.MenuBackground.Draw(spriteBatch, area.Location, area.Size);
        }

        public static void UpdateNearbyContextItemsMenu(IUIElement menu, Dictionary<Vector2, PredictionItem> context, string header, int maxItemCount, bool showLess)
        {
            if (context.Count <= 0)
            {
                menu.IsVisible = false;
                return;
            }

            var maxItems = maxItemCount > 0 ? maxItemCount : int.MaxValue;
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
                    excessLabel = new Label($"+ {items.Length - i} ...", Game1.smallFont, API.TextColor);
                    break;
                }

                children.AddRange(new IUIElement?[]
                {
                    ElementFactory.CreateSpawnSprite(item, position),
                    showLess ? null : new Label(TokenParser.ParseText(item.DisplayName), Game1.smallFont, API.TextColor),
                    ElementFactory.CreateSpawnLabel(position, 3, Game1.smallFont, API.TextColor),
                    new Label(API.GetDistanceUnit(), Game1.smallFont, API.TextColor)
                });
            }

            if (children.EmptyOrNull())
            {
                menu.IsVisible = false;
                return;
            }

            menu.IsVisible = true;
            menu.Children = new[]
            {
                new Label(header, Game1.smallFont, API.TextColor),
                new Grid(children, null, API.MenuInnerSpacing, null, "auto auto auto auto", DynamicUIFramework.Alignment.CenterLeft),
                excessLabel
            };
        }

        public static void UpdateNearbyContextItemsMenu(IUIElement menu, Dictionary<Vector2, PredictionContext> context, string header, int maxItemCount, bool showLess)
        {
            if (context.Count <= 0)
            {
                menu.IsVisible = false;
                return;
            }

            var maxItems = maxItemCount > 0 ? maxItemCount : int.MaxValue;
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
                    excessLabel = new Label($"+ {items.Length - i} ...", Game1.smallFont, API.TextColor);
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
                    ElementFactory.CreateItemGroupSpawnSprite(group, position),
                    showLess ? null : new Label(name, Game1.smallFont, API.TextColor),
                    ElementFactory.CreateSpawnLabel(position, 3, Game1.smallFont, API.TextColor),
                    new Label(API.GetDistanceUnit(), Game1.smallFont, API.TextColor)
                });
            }

            if (children.EmptyOrNull())
            {
                menu.IsVisible = false;
                return;
            }

            menu.IsVisible = true;
            menu.Children = new[]
            {
                new Label(header, Game1.smallFont, API.TextColor),
                new Grid(children, null, API.MenuInnerSpacing, null, "auto auto auto auto", DynamicUIFramework.Alignment.CenterLeft),
                excessLabel
            };
        }
    }
}

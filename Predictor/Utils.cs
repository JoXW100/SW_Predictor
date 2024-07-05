using DynamicUIFramework.Backgrounds;
using Microsoft.Xna.Framework;
using StardewValley;
using DynamicUIFramework;

namespace Predictor
{
    public static class Utils
    {
        public const float UIScale = 1.7f;
        public static float Ratio => Game1.options.zoomLevel != 1f ? 1f : 1f / Game1.options.uiScale;
        public static IUIDrawable MenuBackground => ModEntry.Instance.Config.MenuType == 1
            ? new SimpleBackground(MenuBackgroundColor, boderWidth: 5f)
            : new DefaultMenuBackground();
        public static Color MenuBackgroundColor => new(0.1f, 0.1f, 0.1f, ModEntry.Instance.Config.MenuAlpha);
        public static Vector2 MenuBackgroundPadding => ModEntry.Instance.Config.MenuType == 1
            ? Vector2.Zero
            : Vector2.One * 4f * Ratio * ModEntry.Instance.Config.MenuScale;
        public static Vector4 MenuPadding => (Vector4.One * 4f * UIScale + new Vector4(MenuBackgroundPadding.Y, MenuBackgroundPadding.X, MenuBackgroundPadding.Y, MenuBackgroundPadding.X)) * ModEntry.Instance.Config.MenuScale;
        public static Vector2 MenuSpacing => Vector2.One * 4f * UIScale * ModEntry.Instance.Config.MenuScale;
        public static Vector2 MenuInnerSpacing => Vector2.One * UIScale * ModEntry.Instance.Config.MenuScale;
        public static Color ItemColor => new(1f, 1f, 1f, 0.8f);
        public static Color TextColor => ModEntry.Instance.Config.MenuType == 1
            ? Color.White
            : Color.Black;
        public static Color OutlineColor => Color.Red;
    }
}

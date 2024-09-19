using Microsoft.Xna.Framework;
using StardewValley;
using DynamicUIFramework;
using DynamicUIFramework.Backgrounds;

namespace Predictor
{
    public static class Utils
    {
        public const float UIScale = 1.7f;

        public static float Ratio => Game1.options.zoomLevel != 1f ? 1f : 1f / Game1.options.uiScale;

        public static float MenuBorderWidth => ModEntry.Instance.Config.MenuBorderWidth;

        public static Color MenuBackgroundColor => ModEntry.Instance.Config.MenuBackgroundColor;

        public static Color ItemColor => new(byte.MaxValue, byte.MaxValue, byte.MaxValue, (byte)200);

        public static Color OutlineColor => ModEntry.Instance.Config.OutlineColor;

        public static IUIDrawable MenuBackground => ModEntry.Instance.Config.MenuType == 1
            ? new SimpleBackground(MenuBackgroundColor, boderWidth: MenuBorderWidth)
            : new DefaultMenuBackground();

        public static Color TextColor => ModEntry.Instance.Config.MenuType == 1
            ? ModEntry.Instance.Config.MenuTextColor
            : Color.Black;

        public static Vector2 MenuBackgroundPadding => ModEntry.Instance.Config.MenuType == 1
            ? Vector2.Zero
            : Vector2.One * 6f * Ratio * ModEntry.Instance.Config.MenuScale;

        public static Vector4 MenuPadding => (Vector4.One * 4f * UIScale + new Vector4(MenuBackgroundPadding.Y, MenuBackgroundPadding.X, MenuBackgroundPadding.Y, MenuBackgroundPadding.X)) * ModEntry.Instance.Config.MenuScale;

        public static Vector2 MenuSpacing => Vector2.One * 4f * UIScale * ModEntry.Instance.Config.MenuScale;

        public static Vector2 MenuInnerSpacing => Vector2.One * UIScale * ModEntry.Instance.Config.MenuScale;
    }
}

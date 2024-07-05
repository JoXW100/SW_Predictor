using Microsoft.Xna.Framework;
using StardewValley;

namespace DynamicUIFramework.Events
{
    public class CursorEventArgs : EventArgs
    {
        public Point Location { get; init; }

        public static bool LeftClickPressed => Game1.didPlayerJustLeftClick(true);
        public static bool LeftClickDown => Game1.didPlayerJustLeftClick();
        public static bool RightClickPressed => Game1.didPlayerJustRightClick(true);
        public static bool RightClickDown => Game1.didPlayerJustRightClick();
    }
}

using Microsoft.Xna.Framework;
using Predictor.Framework.UI;
using StardewModdingAPI;

namespace Predictor.Framework
{
    public sealed class PatchContext : IPatchContextWithMenu
    {
        public IUIElement? Menu { get; set; }
    }

    public sealed class PatchContext<T> : IPatchContextWithMenu
    {
        public IUIElement? Menu { get; set; }
        public Dictionary<Vector2, T> Context { get; set; } = new();
    }

    public abstract class PatchWithContextBase<T> : PatchWithMenuBase<PatchContext<T>>
    {
        protected Dictionary<Vector2, T> Context => MultiplayerManager.GetValue().Context;

        protected PatchWithContextBase(IModHelper helper, IMonitor monitor) : base(helper, monitor)
        {

        }
    }
}

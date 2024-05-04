using Microsoft.Xna.Framework;
using Predictor.Framework.UI;
using StardewModdingAPI;
using System.Diagnostics;

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
        protected Dictionary<Vector2, T> Context
        {
            get
            {
                if (MultiplayerManager.TryGetValue(out var context))
                {
                    return context.Context;
                }
                else
                {
                    Debug.Assert(false);
                    return new();
                }
            }
        }

        protected PatchWithContextBase(IModHelper helper, IMonitor monitor) : base(helper, monitor)
        {

        }
    }
}

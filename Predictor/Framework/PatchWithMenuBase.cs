using Predictor.Framework.UI;
using StardewModdingAPI;
using System.Diagnostics;

namespace Predictor.Framework
{
    public abstract class PatchWithMenuBase<T> : PatchMultiplayerBase<T> where T : class, IPatchContextWithMenu
    {
        protected virtual IUIElement? Menu
        {
            get
            {
                if (MultiplayerManager.TryGetValue(out var context))
                {
                    return context.Menu;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (MultiplayerManager.TryGetValue(out var context))
                {
                    context.Menu = value;
                }
                else
                {
                    Debug.Assert(false);
                }
            }
        }

        protected PatchWithMenuBase(IModHelper helper, IMonitor monitor) : base(helper, monitor)
        {

        }

        public override IUIElement? GetMenu()
        {
            return Attached && CheckRequirements() ? Menu : null;
        }
    }
}

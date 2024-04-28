using Predictor.Framework.UI;
using StardewModdingAPI;
using StardewValley;

namespace Predictor.Framework
{
    public abstract class PatchBase : IPatch
    {
        protected readonly IModHelper Helper;
        protected readonly IMonitor Monitor;
        protected IUIElement? Menu;

        public bool Attached { get; private set; }

        public abstract string Name { get; }

        public PatchBase(IModHelper helper, IMonitor monitor)
        {
            Menu = null;
            Helper = helper;
            Monitor = monitor;
        }

        public void Attach()
        {
            if (!Attached && CheckRequirements())
            {
                Attached = true;
                if (ModEntry.Instance.Config.LazyUpdates)
                {
                    OnLazyAttach();
                }
                else
                {
                    OnAttach();
                }
                Monitor.Log($"{Name} Attached.", LogLevel.Debug);
            }
        }


        public void Detatch()
        {
            if (Attached)
            {
                Attached = false;
                Menu = null;
                OnDetatch();
                Monitor.Log($"{Name} Detatched.", LogLevel.Debug);
            }
        }

        public IUIElement? GetMenu()
        {
            return CheckRequirements() ? Menu : null;
        }

        public virtual bool CheckRequirements() 
        { 
            return Game1.player.currentLocation is not null; 
        }

        public abstract void OnAttach();
        public abstract void OnLazyAttach();
        public abstract void OnDetatch();
    }
}

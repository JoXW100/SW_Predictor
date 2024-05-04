using Predictor.Framework.UI;
using StardewModdingAPI;
using StardewValley;

namespace Predictor.Framework
{
    public abstract class PatchBase : IPatch
    {
        protected readonly IModHelper Helper;
        protected readonly IMonitor Monitor;

        public bool Attached { get; private set; }

        public abstract string Name { get; }

        public PatchBase(IModHelper helper, IMonitor monitor)
        {
            Helper = helper;
            Monitor = monitor;
        }

        public virtual void Attach()
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

        public virtual void Detatch()
        {
            if (Attached)
            {
                Attached = false;
                OnDetatch();
                Monitor.Log($"{Name} Detatched.", LogLevel.Debug);
            }
        }

        public virtual IUIElement? GetMenu()
        {
            return null;
        }

        public virtual bool CheckRequirements()
        {
            return Context.IsWorldReady && Game1.currentLocation is not null;
        }

        public abstract void OnAttach();
        public abstract void OnLazyAttach();
        public abstract void OnDetatch();
    }
}

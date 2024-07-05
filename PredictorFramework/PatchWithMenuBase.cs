using DynamicUIFramework;
using StardewModdingAPI;

namespace PredictorPatchFramework
{
    public abstract class PatchWithMenuBase<T> : PatchMultiplayerBase<T> where T : class, IPatchContextWithMenu
    {
        protected virtual IUIElement Menu
        {
            get => GetValue().Menu;
            set => GetValue().Menu = value;
        }

        protected PatchWithMenuBase(IModHelper helper, IMonitor monitor, Func<T>? factory = null) : base(helper, monitor, factory)
        {

        }

        public override IUIElement? GetMenu()
        {
            return Attached && Menu is not null && Menu.IsVisible && CheckRequirements() ? Menu : null;
        }
    }
}

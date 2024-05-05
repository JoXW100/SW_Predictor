using Predictor.Framework.UI;
using StardewModdingAPI;

namespace Predictor.Framework
{
    public abstract class PatchWithMenuBase<T> : PatchMultiplayerBase<T> where T : class, IPatchContextWithMenu
    {
        protected virtual IUIElement? Menu
        {
            get => MultiplayerManager.GetValue().Menu;
            set => MultiplayerManager.GetValue().Menu = value;
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

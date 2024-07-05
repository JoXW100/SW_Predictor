using DynamicUIFramework;
using DynamicUIFramework.Elements;
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace PredictorPatchFramework
{
    public class PatchContextWithMenut<T> : PatchContext<T>, IPatchContextWithMenu
    {
        public IUIElement Menu { get; set; }

        public PatchContextWithMenut()
        {
            Menu = new Grid(
                padding: FrameworkUtils.API.MenuPadding,
                spacing: FrameworkUtils.API.MenuSpacing,
                background: FrameworkUtils.API.MenuBackground
            );
        }

        public PatchContextWithMenut(IUIElement menu)
        {
            Menu = menu;
        }
    }

    public abstract class PatchWithContextAndMenuBase<T> : PatchWithMenuBase<PatchContextWithMenut<T>>
    {
        protected Dictionary<Vector2, T> Context => GetValue().Context;

        protected PatchWithContextAndMenuBase(IModHelper helper, IMonitor monitor, Func<PatchContextWithMenut<T>>? factory = null) : base(helper, monitor, factory)
        {

        }
    }
}

using DynamicUIFramework;
using Microsoft.Xna.Framework;
using PredictorPatchFramework;
using StardewModdingAPI;

namespace Predictor
{
    public class ModAPI : IModAPI
    {
        public bool IsEnabled => ModEntry.Instance.Config.Enabled;
        public bool IsLazy => ModEntry.Instance.Config.LazyUpdates;
        // Styling
        public float UIScale => Utils.UIScale;
        public float MenuScale => ModEntry.Instance.Config.MenuScale;
        public float OutlineWidth => ModEntry.Instance.Config.OutlineWidth;
        public float ThickOutlineWidth => ModEntry.Instance.Config.ThickOutlineWidth;

        public IUIDrawable MenuBackground => Utils.MenuBackground;
        public Vector4 MenuPadding => Utils.MenuPadding;
        public Vector2 MenuSpacing => Utils.MenuSpacing;
        public Vector2 MenuInnerSpacing => Utils.MenuInnerSpacing;
        public Color ItemColor => Utils.ItemColor;
        public Color TextColor => Utils.TextColor;
        public Color OutlineColor => Utils.OutlineColor;
        // Global elements
        public IUIElement RootUIElement => ModEntry.Instance.RootUIElement;
        public IUIDrawable? Tooltips
        {
            get => ModEntry.Instance.Tooltips;
            set => ModEntry.Instance.Tooltips = value;
        }

        public Point GetMenuOffset()
        {
            return ModEntry.Instance.Config.GetMenuOffset();
        }

        public string GetDistanceUnit()
        {
            return ModEntry.Instance.Helper.Translation.Get("menu.DistanceUnit");
        }

        public bool RegisterPatch(IPatch patch)
        {
            ModEntry.Instance.Monitor.Log("Registering Patch: " + patch.Name, LogLevel.Debug);
            return ModEntry.Instance.RegisterPatch(patch);
        }

        public bool DeRegisterPatch(IPatch patch)
        {
            ModEntry.Instance.Monitor.Log("DeRegistering Patch: " + patch.Name, LogLevel.Debug);
            return ModEntry.Instance.DeRegisterPatch(patch);
        }

        public void RetatchPatches()
        {
            ModEntry.Instance.Monitor.Log("Retatching Patches");
            ModEntry.Instance.RetatchPatches();
        }
    }
}

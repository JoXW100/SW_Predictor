using DynamicUIFramework;
using Microsoft.Xna.Framework;
using PredictorPatchFramework;
using StardewModdingAPI;

namespace Predictor
{
    public class ModAPI : IModAPI
    {
        private static Dictionary<string, PatchConfigApi> RegisteredModConfigs { get; } = new Dictionary<string, PatchConfigApi>();

        private IManifest Manifest => ModEntry.Instance.ModManifest;
        private IGenericModConfigMenuApi? GenericModConfigAPI => ModEntry.Instance.GenericModConfigAPI;

        internal ModAPI() { }

        public bool IsEnabled => ModEntry.Instance.Config.Enabled;
        public bool IsLazy => ModEntry.Instance.Config.LazyUpdates;
        // Styling
        public float UIScale => Utils.UIScale;
        public float MenuScale => ModEntry.Instance.Config.MenuScale;
        public float OutlineWidth => ModEntry.Instance.Config.OutlineWidth;
        public float ThickOutlineWidth => ModEntry.Instance.Config.ThickOutlineWidth;

        public IUIDrawable MenuBackground => Utils.MenuBackground;
        public Point MenuOffset => ModEntry.Instance.Config.GetMenuOffset();
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

        public void SavePatchConfigs()
        {
            foreach (var config in RegisteredModConfigs.Values)
            {
                config.Save.Invoke();
            }
        }

        public void ResetPatchConfigs()
        {
            foreach (var config in RegisteredModConfigs.Values)
            {
                config.Reset.Invoke();
            }
        }

        public void RegisterPatchConfig(IManifest manifest, Action<IPatchConfigApi> registerOptions, Action reset, Action save)
        {
            if (GenericModConfigAPI is null)
            {
                return;
            }

            if (RegisteredModConfigs.ContainsKey(manifest.UniqueID))
            {
                throw new InvalidOperationException($"Mod configuration for ID={manifest.UniqueID}, already registered.");
            }

            var api = PatchConfigApi.Register(GenericModConfigAPI, Manifest, manifest.UniqueID, manifest.Name, reset, save);
            RegisteredModConfigs[manifest.UniqueID] = api;
            registerOptions.Invoke(api);
        }
    }
}

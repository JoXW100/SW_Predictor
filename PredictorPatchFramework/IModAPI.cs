using DynamicUIFramework;
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace PredictorPatchFramework
{
    public interface IModAPI
    {
        // Styling
        bool IsEnabled { get; }
        bool IsLazy { get; }
        float UIScale { get; }
        float MenuScale { get; }
        float OutlineWidth { get; }
        float ThickOutlineWidth { get; }

        IUIDrawable MenuBackground { get; }
        Point MenuOffset { get; }
        Vector4 MenuPadding { get; }
        Vector2 MenuSpacing { get; }
        Vector2 MenuInnerSpacing { get; }
        Color ItemColor { get; }
        Color TextColor { get; }
        Color OutlineColor { get; }
        // Global elements
        IUIElement RootUIElement { get; }
        IUIDrawable? Tooltips { get; set; }

        /// <summary>
        /// Registers the patch to be handled by the Predictor mod.
        /// </summary>
        /// <param name="patch">The patch to register.</param>
        /// <returns><see langword="true"/> if the patch was registered, <see langword="false"/> otherwise.</returns>
        bool RegisterPatch(IPatch patch);

        /// <summary>
        /// De-registers the patch from the Predictor mod.
        /// </summary>
        /// <param name="patch">The patch to de-register.</param>
        /// <returns><see langword="true"/> if the patch was de-registered, <see langword="false"/> otherwise.</returns>
        bool DeRegisterPatch(IPatch patch);

        /// <summary>
        /// Detatches and then attaches all registered patches. Should be used to apply changes in the config.
        /// </summary>
        void RetatchPatches();

        /// <summary>
        /// Gets the localized unit of distance, commonly "Tiles"
        /// </summary>
        /// <returns>The localized unit of distance.</returns>
        string GetDistanceUnit();

        /// <summary>
        /// Registers a config menu as a sub-page in the generic config menu of the main Predicor mod for a patch.
        /// </summary>
        /// <param name="manifest">The manifest of the patch to register.</param>
        /// <param name="registerOptions">Callback method to register individual options inside the config page.</param>
        /// <param name="reset">The reset action, called when settings are cleared.</param>
        /// <param name="save">The save option action, called when settings are saved.</param>
        /// <exception cref="InvalidOperationException">Thrown when registering a config for a patch that already is registered.</exception>
        void RegisterPatchConfig(IManifest manifest, Action<IPatchConfigApi> registerOptions, Action reset, Action save);
    }
}

using DynamicUIFramework;
using Microsoft.Xna.Framework;

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
        /// Get the offset anchor point to start drawing the menu from.
        /// </summary>
        /// <returns>The offset point.</returns>
        Point GetMenuOffset();

        /// <summary>
        /// Gets the localized unit of distance, commonly "Tiles"
        /// </summary>
        /// <returns>The localized unit of distance.</returns>
        string GetDistanceUnit();
    }
}

using DynamicUIFramework;

namespace PredictorPatchFramework
{
    public interface IPatch
    {
        public string Name { get; }
        /// <summary>
        /// Attaches the patch to event handlers.
        /// </summary>
        public void Attach();

        /// <summary>
        /// Detaches the patch to event handlers.
        /// </summary>
        public void Detatch();

        /// <summary>
        /// Checks the patch requirements. Should be checked before updating or rendering content from the patch.
        /// </summary>
        /// <returns>True if requirements are met, False otherwise</returns>
        public bool CheckRequirements();

        /// <summary>
        /// Gets a UI element for the patch if any.
        /// </summary>
        /// <returns>The active UI element of the patch, or null</returns>
        public IUIElement GetMenu();
    }
}

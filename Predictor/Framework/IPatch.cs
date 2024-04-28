using Predictor.Framework.UI;

namespace Predictor.Framework
{
    internal interface IPatch
    {
        public void Attach();

        public void Detatch();

        public bool CheckRequirements();

        public IUIElement? GetMenu();
    }
}

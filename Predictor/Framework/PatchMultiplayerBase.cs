using StardewModdingAPI;

namespace Predictor.Framework
{
    public abstract class PatchMultiplayerBase<T> : PatchBase where T : class
    {
        protected readonly MultiplayerManager<T> MultiplayerManager;

        public PatchMultiplayerBase(IModHelper helper, IMonitor monitor) : base(helper, monitor)
        {
            MultiplayerManager = new(helper);
        }

        public override void Detatch()
        {
            if (Attached)
            {
                MultiplayerManager.ClearConnections();
            }
            base.Detatch();
        }

        public override bool CheckRequirements()
        {
            return base.CheckRequirements() && MultiplayerManager.IsConnected();
        }
    }
}

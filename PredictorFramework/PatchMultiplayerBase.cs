using StardewModdingAPI;

namespace PredictorPatchFramework
{
    public abstract class PatchMultiplayerBase<T> : PatchBase where T : class
    {
        protected readonly MultiplayerManager<T> MultiplayerManager;
        private readonly Func<T>? m_factory;

        public PatchMultiplayerBase(IModHelper helper, IMonitor monitor, Func<T>? factory = null) : base(helper, monitor)
        {
            MultiplayerManager = new(helper);
            m_factory = factory;
        }

        protected T GetValue()
        {
            return MultiplayerManager.GetValue(m_factory);
        }

        public override void Detatch()
        {
            if (Attached)
            {
                MultiplayerManager.ClearConnections();
            }
            base.Detatch();
        }
    }
}

using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace PredictorPatchFramework
{
    public class PatchContext<T>
    {
        public Dictionary<Vector2, T> Context { get; set; } = new();
    }

    public abstract class PatchWithContexBase<T> : PatchMultiplayerBase<PatchContext<T>>
    {
        protected Dictionary<Vector2, T> Context => GetValue().Context;

        protected PatchWithContexBase(IModHelper helper, IMonitor monitor, Func<PatchContext<T>>? factory = null) : base(helper, monitor, factory)
        {

        }
    }
}

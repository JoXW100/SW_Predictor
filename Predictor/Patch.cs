using Microsoft.Xna.Framework;
using PredictorPatchFramework;
using PredictorPatchFramework.Extensions;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace Predictor
{
    internal sealed class Patch : PatchBase
    {
        public override string Name => nameof(Patch);

        public Patch(IModHelper helper, IMonitor monitor) : base(helper, monitor)
        {

        }

        public override void OnAttach()
        {
            Helper.Events.Display.RenderedWorld += OnRendered;
        }

        public override void OnLazyAttach()
        {
            Helper.Events.Display.RenderedWorld += OnRendered;
        }

        public override void OnDetatch()
        {
            Helper.Events.Display.RenderedWorld -= OnRendered;
        }

        public override bool CheckRequirements()
        {
            return base.CheckRequirements()
                && ModEntry.Instance.Config.EnableObjectOutlines;
        }

        private static IEnumerable<Vector2> GetObjectsLocationsOnScreen()
        {
            foreach (var pos in Game1.player.currentLocation.Objects.Keys)
            {
                var location = pos.ToLocation();
                if (Game1.viewport.Contains(location * FrameworkUtils.TileSize))
                {
                    yield return pos;
                }
            }
        }

        private void OnRendered(object? sender, RenderedWorldEventArgs e)
        {
            if (CheckRequirements())
            {
                FrameworkUtils.DrawOutlines(e.SpriteBatch, GetObjectsLocationsOnScreen());
            }
        }
    }
}

using Microsoft.Xna.Framework;
using Predictor.Framework;
using Predictor.Framework.Extentions;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace Predictor.Patches
{
    public sealed class ObjectPatch : PatchBase
    {
        public override string Name => nameof(ObjectPatch);

        public ObjectPatch(IModHelper helper, IMonitor monitor) : base(helper, monitor) 
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
                if (Game1.viewport.Contains(location * Utils.TileSize))
                {
                    yield return pos;
                }
            }
        }

        private void OnRendered(object? sender, RenderedWorldEventArgs e)
        {
            if (CheckRequirements())
            {
                Utils.DrawOutlines(e.SpriteBatch, GetObjectsLocationsOnScreen());
            }
        }
    }
}

using Microsoft.Xna.Framework;
using Predictor.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace Predictor.Patches
{
    internal sealed class SpawnedPatch : PatchBase
    {
        public override string Name => nameof(SpawnedPatch);

        private readonly Dictionary<Vector2, PredictionItem> Context;

        public SpawnedPatch(IModHelper helper, IMonitor monitor) : base(helper, monitor)
        {
            Context = new();
        }

        public override void OnAttach()
        {
            Helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            Helper.Events.Display.RenderedWorld += OnRendered;
        }

        public override void OnLazyAttach()
        {
            Helper.Events.GameLoop.OneSecondUpdateTicked += OnUpdateTicked;
            Helper.Events.World.ObjectListChanged += OnUpdateTicked;
            Helper.Events.World.TerrainFeatureListChanged += OnUpdateTicked;
            Helper.Events.Display.RenderedWorld += OnRendered;
        }

        public override void OnDetatch()
        {
            Helper.Events.GameLoop.UpdateTicked -= OnUpdateTicked;
            Helper.Events.GameLoop.OneSecondUpdateTicked -= OnUpdateTicked;
            Helper.Events.World.ObjectListChanged -= OnUpdateTicked;
            Helper.Events.World.TerrainFeatureListChanged -= OnUpdateTicked;
            Helper.Events.Display.RenderedWorld -= OnRendered;
        }

        public override bool CheckRequirements()
        {
            return base.CheckRequirements()
                && (ModEntry.Instance.Config.TrackSpawned || ModEntry.Instance.Config.EnableSpawnedOutlines);
        }

        private void OnRendered(object? sender, RenderedWorldEventArgs e)
        {
            if (ModEntry.Instance.Config.EnableSpawnedOutlines)
            {
                Utils.DrawOutlines(e.SpriteBatch, Context.Keys);
            }
        }

        private void OnUpdateTicked(object? sender, EventArgs e)
        {
            Context.Clear();
            Menu = null;

            if (!CheckRequirements())
            {
                Monitor.LogOnce($"{nameof(SpawnedPatch)} attached when requirements are false", LogLevel.Debug);
                return;
            }

            foreach (var (pos, obj) in Game1.player.currentLocation.Objects.Pairs)
            {
                if (obj.IsSpawnedObject)
                {
                    Context.TryAdd(pos, new PredictionItem(obj));
                }
            }

            // Wild plants
            foreach (var features in Game1.player.currentLocation.terrainFeatures)
            {
                foreach (var (pos, feature) in features)
                {
                    if (feature is HoeDirt dirt && dirt.crop != null && dirt.crop.forageCrop.Value && dirt.crop.whichForageCrop.Value == "1")
                    {
                        var item = PredictionItem.Create("(O)399");
                        if (item != null)
                        {
                            Context.TryAdd(pos, item);
                        }
                    }
                }
            }

            if (ModEntry.Instance.Config.TrackSpawned)
            {
                Menu = Utils.CreateNearbyContextItemsMenu(Context, "menu.SpawedHeader");
            }
        }
    }
}

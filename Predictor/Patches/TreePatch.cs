﻿using Predictor.Framework;
using Predictor.Framework.Extentions;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace Predictor.Patches
{
    public sealed class TreePatch : PatchWithContextBase<PredictionContext>
    {
        public override string Name => nameof(TreePatch);

        public TreePatch(IModHelper helper, IMonitor monitor) : base(helper, monitor)
        {
            
        }

        public override void OnAttach()
        {
            Helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            Helper.Events.Display.RenderedWorld += OnRendered;
        }

        public override void OnLazyAttach()
        {
            Helper.Events.GameLoop.OneSecondUpdateTicked += OnUpdateTicked;
            Helper.Events.World.TerrainFeatureListChanged += OnUpdateTicked;
            Helper.Events.Display.RenderedWorld += OnRendered;
        }

        public override void OnDetatch()
        {
            Helper.Events.GameLoop.UpdateTicked -= OnUpdateTicked;
            Helper.Events.GameLoop.OneSecondUpdateTicked -= OnUpdateTicked;
            Helper.Events.World.TerrainFeatureListChanged -= OnUpdateTicked;
            Helper.Events.Display.RenderedWorld -= OnRendered;
        }

        public override bool CheckRequirements()
        {
            return base.CheckRequirements()
                && (ModEntry.Instance.Config.EnableHarvestableTreeItems || ModEntry.Instance.Config.EnableHarvestableTreeOutlines);
        }

        private void OnRendered(object? sender, RenderedWorldEventArgs e)
        {
            if (CheckRequirements())
            {
                Utils.DrawContextItems(e.SpriteBatch, Context, ModEntry.Instance.Config.EnableHarvestableTreeItems, ModEntry.Instance.Config.EnableHarvestableTreeOutlines);
            }
        }

        private void OnUpdateTicked(object? sender, EventArgs e)
        {
            if (!CheckRequirements())
            {
                Monitor.LogOnce($"{nameof(TreePatch)} attached when requirements are false", LogLevel.Debug);
                return;
            }

            Context.Clear();
            foreach (var posx in Game1.currentLocation.terrainFeatures.Keys)
            {
                if (!Game1.viewport.Contains(posx.ToLocation() * Utils.TileSize))
                {
                    continue;
                }

                PredictionContext ctx = new();
                TerrainFeature feature = Game1.currentLocation.terrainFeatures[posx];
                if (feature is Tree tree)
                {
                    tree.Predict_shake(ctx, posx, false);

                    if (tree.hasMoss.Value)
                    {
                        ctx.AddItemIfNotNull(Tree.CreateMossItem());
                    }

                    if (ctx.Items.Any() || ModEntry.Instance.Config.EnableHarvestableTreeOutlines)
                    {
                        Context.TryAdd(posx, ctx);
                    }
                }
            }
        }
    }
}

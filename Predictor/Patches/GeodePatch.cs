using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Predictor.Framework;
using Predictor.Framework.Extentions;
using StardewModdingAPI;
using StardewValley;
using System.Reflection;
using Object = StardewValley.Object;

namespace Predictor.Patches
{
    internal class GeodePatch : PatchBase
    {
        public override string Name => nameof(Name);

        private readonly Harmony Harmony;

        private MethodBase Target => AccessTools.Method(typeof(Object), nameof(Object.drawInMenu), new[]
        {
            typeof(SpriteBatch),
            typeof(Vector2),
            typeof(float),
            typeof(float),
            typeof(float),
            typeof(StackDrawType),
            typeof(Color),
            typeof(bool)
        });

        public GeodePatch(IModHelper helper, IMonitor monitor) : base(helper, monitor)
        {
            Harmony = new (ModEntry.Instance.ModManifest.UniqueID);
        }

        public override void OnAttach()
        {
            Harmony.Patch(Target, postfix: new HarmonyMethod(typeof(GeodePatch), nameof(Postfix_drawInMenu)));
        }

        public override void OnLazyAttach()
        {
            OnAttach();
        }

        public override void OnDetatch()
        {
            Harmony.Unpatch(Target, HarmonyPatchType.Postfix, Harmony.Id);
        }

        public override bool CheckRequirements()
        {
            return base.CheckRequirements()
                && ModEntry.Instance.Config.EnableGeodeItems;
        }

        private static void Postfix_drawInMenu(Object __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            PredictionItem? treasure;
            if (__instance.QualifiedItemId == "(O)791" && !Game1.netWorldState.Value.GoldenCoconutCracked)
            {
                treasure = PredictionItem.Create("(O)73"); // Golden Coconut
            }
            else
            {
                treasure = UtilityExtentions.Predict_getTreasureFromGeode(__instance);
            }

            if (treasure is not null)
            {
                var scale = Utils.UIScale * scaleSize;
                treasure.Draw(spriteBatch, location + new Vector2(64f * scaleSize - PredictionItem.TextureSize * scale, 0f), scale, layerDepth);
            }
        }
    }
}

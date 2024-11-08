using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PredictorPatchFramework;
using StardewModdingAPI;
using StardewValley;
using System.Reflection;
using Object = StardewValley.Object;

namespace PredictorGeodePatch
{
    internal sealed class Patch : PatchBase
    {
        public override string Name => ModEntry.Instance.ModManifest.Name;

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

        public Patch(IModHelper helper, IMonitor monitor) : base(helper, monitor)
        {
            Harmony = new (ModEntry.Instance.ModManifest.UniqueID);
        }

        public override void OnAttach()
        {
            Harmony.Patch(Target, postfix: new HarmonyMethod(typeof(Patch), nameof(Postfix_drawInMenu)));
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
            return base.CheckRequirements() && ModEntry.Instance.Config.Enabled;
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
                treasure = Extensions.Predict_getTreasureFromGeode(__instance);
            }

            if (treasure is not null)
            {
                var scale = FrameworkUtils.API.UIScale * scaleSize;
                treasure.Draw(spriteBatch, location + new Vector2(64f * scaleSize - PredictionItem.TextureSize * scale, 0f), Vector2.One * scale, layerDepth);
            }
        }
    }
}

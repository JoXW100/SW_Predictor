using Predictor.Framework;
using Predictor.Framework.UI;
using Predictor.Framework.Extentions;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Minigames;
using StardewValley;
using Microsoft.Xna.Framework;

namespace Predictor.Patches
{
    internal class MinigamesPatch : PatchBase
    {
        public override string Name => nameof(MinigamesPatch);

        public MinigamesPatch(IModHelper helper, IMonitor monitor) : base(helper, monitor)
        {

        }

        public override void OnAttach()
        {
            Helper.Events.Display.RenderedStep += OnRenderedStep;
        }

        public override void OnLazyAttach()
        {
            OnAttach();
        }

        public override void OnDetatch()
        {
            Helper.Events.Display.RenderedStep -= OnRenderedStep;
        }

        public override bool CheckRequirements()
        {
            return base.CheckRequirements() 
                && (ModEntry.Instance.Config.ShowSlotsOutcome || ModEntry.Instance.Config.ShowCalicoJackOutcome);
        }

        private void OnRenderedStep(object? sender, RenderedStepEventArgs e)
        {
            if (e.Step != StardewValley.Mods.RenderSteps.Minigame || !CheckRequirements())
            {
                return;
            }

            IEnumerable<IUIElement?> children = Array.Empty<IUIElement?>();
            if (Game1.currentMinigame is Slots slots && ModEntry.Instance.Config.ShowSlotsOutcome)
            {
                var result = slots.Predict_slotResults(new PredictionContext());

                children = new[] {
                    new Label(string.Format(Helper.Translation.Get("menu.SlotsWinMultiplierLabel"), result), Game1.smallFont, Utils.TextColor)
                };
            }
            else if (Game1.currentMinigame is CalicoJack calicoJack && ModEntry.Instance.Config.ShowCalicoJackOutcome)
            {
                var ctx = new PredictionContext(calicoJack.CloneCurrentRandom());
                var hitPlayerCard = calicoJack.PredictCalicoJackHitCard(ctx);
                var hitDelaerCard = calicoJack.PredictCalicoJackNextDealerCard(ctx);

                var ctx1 = new PredictionContext(calicoJack.CloneCurrentRandom());
                var result = calicoJack.PredictCalicoJackStandResult(ctx1);

                children = new[] {
                    new Label(string.Format(Helper.Translation.Get("menu.CalicoNextHitPlayerCardLabel"), hitPlayerCard), Game1.smallFont, Utils.TextColor),
                    new Label(string.Format(Helper.Translation.Get("menu.CalicoNextHitDealerCardLabel"), hitDelaerCard), Game1.smallFont, Utils.TextColor),
                    new Label(result 
                        ? Helper.Translation.Get("menu.CalicoNextStandOutcomeLabel.Win") 
                        : Helper.Translation.Get("menu.CalicoNextStandOutcomeLabel.Loss")
                    , Game1.smallFont, Utils.TextColor)
                };
            }

            if (children.Any())
            {
                var root = new Grid(
                    children,
                    backgroundColor: Utils.MenuBackground,
                    padding: Vector4.One * 6 * Utils.UIScale,
                    spacing: Vector2.One * 2
                );

                var offset = ModEntry.Instance.Config.GetMenuOffset();
                root.Update(offset);
                root.Draw(e.SpriteBatch);
            }
        }
    }
}

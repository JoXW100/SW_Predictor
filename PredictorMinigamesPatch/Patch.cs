using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Minigames;
using StardewValley;
using PredictorPatchFramework;
using PredictorPatchFramework.UI;
using DynamicUIFramework;
using DynamicUIFramework.Elements;

namespace PredictorMinigamesPatch
{
    internal enum MinigameType
    {
        None, Slots, CalicoJack
    }

    internal class MinigamesContext : IPatchContextWithMenu
    {
        public MinigameType Minigame { get; set; }
        public IUIElement Menu { get; set; }

        public MinigamesContext()
        {
            Minigame = MinigameType.None;
            Menu = new Grid(
                background: FrameworkUtils.API.MenuBackground,
                padding: FrameworkUtils.API.MenuPadding,
                spacing: FrameworkUtils.API.MenuInnerSpacing
            );
        }
    }

    internal sealed class Patch : PatchWithMenuBase<MinigamesContext>
    {
        public override string Name => ModEntry.Instance.ModManifest.Name;

        public Patch(IModHelper helper, IMonitor monitor) : base(helper, monitor)
        {

        }

        public MinigameType Minigame
        {
            get => MultiplayerManager.GetValue().Minigame;
            set => MultiplayerManager.GetValue().Minigame = value;
        }

        public override void OnAttach()
        {
            Helper.Events.GameLoop.UpdateTicked += OnUpdate;
            Helper.Events.Display.Rendered += OnRendered;
        }

        public override void OnLazyAttach()
        {
            Helper.Events.GameLoop.OneSecondUpdateTicked += OnUpdate;
            Helper.Events.Display.Rendered += OnRendered;
        }

        public override void OnDetatch()
        {
            Helper.Events.GameLoop.UpdateTicked -= OnUpdate;
            Helper.Events.GameLoop.OneSecondUpdateTicked -= OnUpdate;
            Helper.Events.Display.Rendered -= OnRendered;
            Minigame = MinigameType.None;
        }

        public override bool CheckRequirements()
        {
            return base.CheckRequirements() 
                && (ModEntry.Instance.Config.ShowSlotsOutcomes || ModEntry.Instance.Config.ShowCalicoJackOutcomes);
        }

        public override IUIElement? GetMenu()
        {
            return null;
        }

        private void OnUpdate(object? sender, EventArgs e)
        {
            if (!CheckRequirements())
            {
                Minigame = MinigameType.None;
                return;
            }

            switch (Game1.currentMinigame)
            {
                case Slots:
                    ShowSlotsOutcomes();
                    break;
                case CalicoJack:
                    ShowCalicoJackOutcomes();
                    break;
                default:
                    Minigame = MinigameType.None;
                    break;
            }
        }

        private void OnRendered(object? sender, RenderedEventArgs e)
        {
            if (!CheckRequirements() || Minigame == MinigameType.None)
            {
                return;
            }

            Menu.Draw(e.SpriteBatch, FrameworkUtils.API.MenuOffset);
        }

        private void ShowSlotsOutcomes()
        {
            if (!ModEntry.Instance.Config.ShowSlotsOutcomes)
            {
                Minigame = MinigameType.None;
                return;
            }

            if (Minigame is MinigameType.Slots)
            {
                return;
            }

            Minigame = MinigameType.Slots;
            Menu.Children = new[] {
                ElementFactory.CreateUpdatingLabel(() =>
                {
                    if (Game1.currentMinigame is not Slots slots)
                    {
                        return "N/A";
                    }

                    return string.Format(Helper.Translation.Get("menu.SlotsWinMultiplierLabel"), slots.Predict_slotResults(new PredictionContext()));
                }, Game1.smallFont, FrameworkUtils.API.TextColor)
            };
        }

        private void ShowCalicoJackOutcomes()
        {
            if (!ModEntry.Instance.Config.ShowCalicoJackOutcomes)
            {
                Minigame = MinigameType.None;
                return;
            }

            if (Minigame is MinigameType.CalicoJack)
            {
                return;
            }

            Minigame = MinigameType.CalicoJack;
            Menu.Children = new[] {
                ElementFactory.CreateUpdatingLabel(() =>
                {
                    if (Game1.currentMinigame is not CalicoJack calicoJack)
                    {
                        return "N/A";
                    }

                    var ctx = new PredictionContext(calicoJack.CloneCurrentRandom());
                    var hitPlayerCard = calicoJack.PredictCalicoJackHitCard(ctx);
                    return string.Format(Helper.Translation.Get("menu.CalicoNextHitPlayerCardLabel"), hitPlayerCard);
                }, Game1.smallFont, FrameworkUtils.API.TextColor),
                ElementFactory.CreateUpdatingLabel(() =>
                {
                    if (Game1.currentMinigame is not CalicoJack calicoJack)
                    {
                        return "N/A";
                    }

                    var ctx = new PredictionContext(calicoJack.CloneCurrentRandom());
                    calicoJack.PredictCalicoJackHitCard(ctx); // skip one prediction
                    var hitDelaerCard = calicoJack.PredictCalicoJackNextDealerCard(ctx);
                    return string.Format(Helper.Translation.Get("menu.CalicoNextHitDealerCardLabel"), hitDelaerCard);
                }, Game1.smallFont, FrameworkUtils.API.TextColor),
                ElementFactory.CreateUpdatingLabel(() =>
                {
                    if (Game1.currentMinigame is not CalicoJack calicoJack)
                    {
                        return "N/A";
                    }

                    var ctx = new PredictionContext(calicoJack.CloneCurrentRandom());
                    var result = calicoJack.PredictCalicoJackStandResult(ctx);
                    return result
                        ? Helper.Translation.Get("menu.CalicoNextStandOutcomeLabel.win")
                        : Helper.Translation.Get("menu.CalicoNextStandOutcomeLabel.loss");
                }, Game1.smallFont, FrameworkUtils.API.TextColor)
            };
        }
    }
}

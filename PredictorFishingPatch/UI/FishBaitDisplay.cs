using DynamicUIFramework.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley.TokenizableStrings;
using StardewValley.Tools;
using StardewValley;
using PredictorPatchFramework;
using DynamicUIFramework;
using PredictorPatchFramework.UI;

namespace PredictorFishingPatch.UI
{
    internal sealed class FishBaitDisplay : UIElement
    {
        private readonly Label m_header;
        private readonly Grid m_innerGrid;
        private readonly IModHelper Helper;
        private long m_updateHash;

        public FishBaitDisplay(IModHelper helper, Vector4? padding = null, Vector2? spacing = null, IUIDrawable? background = null)
        {
            Helper = helper;
            var headerText = string.Format("{0}:", Helper.Translation.Get("menu.FishingBaitLabel"));
            m_header = new Label(headerText, Game1.smallFont, FrameworkUtils.API.TextColor);
            m_innerGrid = new Grid(null, padding, spacing, background, "auto auto auto auto", DynamicUIFramework.Alignment.CenterLeft);
            m_updateHash = 0;
            Update();
        }

        private void Update(FishingRod? rod = null)
        {
            var bait = rod?.GetBait();
            var baitItem = bait is null ? null : PredictionItem.Create(bait.ItemId);
            if (baitItem is null)
            {
                IsVisible = false;
                return;
            }

            m_innerGrid.Children = new IUIElement?[]
            {
                m_header,
                ElementFactory.CreateItemSprite(baitItem),
                ElementFactory.CreateUpdatingLabel(() =>
                {
                    var bait = rod?.GetBait();
                    var stack = bait is null ? 0f : bait.Stack;
                    return string.Format("x{0} ", stack);
                }, color: FrameworkUtils.API.TextColor),
                ModEntry.Instance.Config.ShowLessFishInfo
                    ? null
                    : new Label(TokenParser.ParseText(baitItem.DisplayName), Game1.smallFont, FrameworkUtils.API.TextColor),
            };

            IsVisible = true;
            Bounds = m_innerGrid.Bounds;
        }

        private static long GetBaitHash(FishingRod rod)
        {
            var bait = rod.GetBait();
            if (bait is null)
            {
                return 0;
            }

            return string.GetHashCode(bait.ItemId);
        }

        public override void Draw(SpriteBatch sb, Point? offset = null, Point? size = null)
        {
            if (!ModEntry.Instance.Config.ShowBait)
            {
                IsVisible = false;
                return;
            }

            if (Game1.player.CurrentTool is FishingRod rod)
            {
                var hash = GetBaitHash(rod);
                HasChanged = hash != m_updateHash;
                if (HasChanged || !IsVisible)
                {
                    m_updateHash = hash;
                    Update(rod);
                }

                if (IsVisible)
                {
                    m_innerGrid.Draw(sb, offset, size); ;
                }
                
                Bounds = m_innerGrid.Bounds;
            }
            else
            {
                IsVisible = false;
            }
        }
    }
}

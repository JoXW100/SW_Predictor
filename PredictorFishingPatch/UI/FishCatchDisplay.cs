using DynamicUIFramework.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley.Menus;
using StardewValley.TokenizableStrings;
using StardewValley;
using PredictorPatchFramework;
using DynamicUIFramework;
using PredictorPatchFramework.UI;

namespace PredictorFishingPatch.UI
{
    internal sealed class FishCatchDisplay : UIElement
    {
        private readonly Label m_header;
        private readonly Grid m_innerGrid;
        private readonly IModHelper Helper;
        private long m_updateHash;

        public FishCatchDisplay(IModHelper helper, Vector4? padding = null, Vector2? spacing = null, IUIDrawable? background = null)
        {
            Helper = helper;
            var headerText = string.Format("{0}:", Helper.Translation.Get("menu.FishingMinigameLabel"));
            m_header = new Label(headerText, Game1.smallFont, FrameworkUtils.API.TextColor);
            m_innerGrid = new Grid(null, padding, spacing, background, "auto auto auto", Alignment.CenterLeft);
            m_updateHash = 0;
            Update();
        }

        private void Update(BobberBar? bar = null)
        {
            var item = PredictionItem.Create(bar?.whichFish ?? "");
            if (item is null)
            {
                IsVisible = false;
                return;
            }

            m_innerGrid.Children = new IUIElement?[]
            {
                m_header,
                ElementFactory.CreateItemSprite(item),
                ModEntry.Instance.Config.ShowLessFishInfo
                    ? null
                    : new Label(TokenParser.ParseText(item.DisplayName), Game1.smallFont, FrameworkUtils.API.TextColor),
            };

            IsVisible = true;
            Bounds = m_innerGrid.Bounds;
        }

        public override void Draw(SpriteBatch sb, Point? offset = null, Point? size = null)
        {
            if (!ModEntry.Instance.Config.ShowFish)
            {
                IsVisible = false;
                return;
            }

            if (Game1.activeClickableMenu is BobberBar bar)
            {
                var hash = bar.whichFish.GetHashCode();
                HasChanged = hash != m_updateHash;
                if (HasChanged || !IsVisible)
                {
                    m_updateHash = hash;
                    Update(bar);
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

using DynamicUIFramework;
using DynamicUIFramework.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PredictorPatchFramework;
using PredictorPatchFramework.UI;
using PredictorPatchFramework.Extentions;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TokenizableStrings;
using StardewValley.Tools;

namespace PredictorFishingPatch.UI
{
    internal sealed class FishTackleDisplay : UIElement
    {
        private readonly Label m_header;
        private readonly Grid m_innerGrid;
        private readonly IModHelper Helper;
        private long m_updateHash;

        public FishTackleDisplay(IModHelper helper, Vector4? padding = null, Vector2? spacing = null, IUIDrawable? background = null)
        {
            Helper = helper;
            var headerText = string.Format("{0}:", Helper.Translation.Get("menu.FishingTackleLabel"));
            m_header = new Label(headerText, Game1.smallFont, FrameworkUtils.API.TextColor);
            m_innerGrid = new Grid(null, padding, spacing, background);
            m_updateHash = 0;
            Update();
        }

        private void Update(FishingRod? rod = null)
        {
            var children = new List<IUIElement?>();
            var tackle = rod?.GetTackle();

            if (tackle is null || tackle.All(x => x is null))
            {
                IsVisible = false;
                return;
            }

            foreach (var tackleObject in tackle)
            {
                var tackleItem = tackleObject is null ? null : new PredictionItem(tackleObject);
                if (tackleItem is not null)
                {
                    var itemSprite = ElementFactory.CreateItemWithProgressBar(tackleItem, (item) =>
                    {
                        return PredictionItem.GetTackleDurability(item.Uses);
                    });

                    if (ModEntry.Instance.Config.ShowLessFishInfo)
                    {
                        children.Add(itemSprite);
                    }
                    else
                    {
                        children.AddRange(new IUIElement?[]
                        {
                            itemSprite,
                            ElementFactory.CreateUpdatingLabel(() =>
                            {
                                var durability = tackleObject is null ? 0f : PredictionItem.GetTackleDurability(tackleObject.uses.Value);
                                return string.Format("{0:0%}", durability);
                            }, color: FrameworkUtils.API.TextColor),
                            new Label(TokenParser.ParseText(tackleItem.DisplayName), Game1.smallFont, FrameworkUtils.API.TextColor)
                        });
                    }
                }
            }

            if (ModEntry.Instance.Config.ShowLessFishInfo)
            {
                children.Insert(0, m_header);
                m_innerGrid.Children = new[]
                {
                    new Grid(
                        children: children,
                        spacing: FrameworkUtils.API.MenuInnerSpacing,
                        layout: "auto auto auto",
                        childAlignment: DynamicUIFramework.Alignment.CenterLeft
                    )
                };
            }
            else
            {
                m_innerGrid.Children = new IUIElement[]
                {
                    m_header,
                    new Grid(
                        children: children,
                        spacing: FrameworkUtils.API.MenuInnerSpacing,
                        layout: "auto auto auto",
                        childAlignment: DynamicUIFramework.Alignment.CenterLeft
                    )
                };
            }

            IsVisible = true;
            Bounds = m_innerGrid.Bounds;
        }

        private static long GetTackleHash(FishingRod rod)
        {
            var tackle = rod.GetTackle();
            if (tackle.EmptyOrNull())
            {
                return 0;
            }

            return string.GetHashCode(string.Join('\n', tackle.Select(x => x?.ItemId)));
        }

        public override void Draw(SpriteBatch sb, Point? offset = null, Point? size = null)
        {
            if (!ModEntry.Instance.Config.ShowTackle)
            {
                IsVisible = false;
                return;
            }

            if (Game1.player.CurrentTool is FishingRod rod)
            {
                var hash = GetTackleHash(rod);
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

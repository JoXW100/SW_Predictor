using Predictor.Framework;
using Predictor.Patches;
using Predictor.Framework.UI;
using StardewValley;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework;
using Predictor.Framework.Config;

namespace Predictor
{
    public class ModEntry : Mod
    {
        private static ModEntry? _instance;
        public static ModEntry Instance 
        { 
            get => _instance ?? throw new ArgumentNullException(nameof(Instance));
            private set => _instance = value; 
        }

        public ModConfig Config { get; private set; } = new ModConfig();

        private List<IPatch> Patches = new();

        private Grid? rootUIElement = null;
        private Vector2 cursorPosition = Vector2.Zero;

        public override void Entry(IModHelper helper)
        {
            Instance = this;
            Config = this.Helper.ReadConfig<ModConfig>();
            Patches = new List<IPatch>()
            {
                new ObjectPatch(helper, Monitor),
                new FishingPatch(helper, Monitor),
                new TillablePatch(helper, Monitor),
                new MineablePatch(helper, Monitor),
                new BreakableContainerPatch(helper, Monitor),
                new GarbageCanPatch(helper, Monitor),
                new GeodePatch(helper, Monitor),
                new TreePatch(helper, Monitor),
                new DigSpotPatch(helper, Monitor),
                new SpawnedPatch(helper, Monitor),
                new BushPatch(helper, Monitor),
                new PanningPatch(helper, Monitor),
            };

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.Display.RenderedHud += OnRendered;
            helper.Events.Input.CursorMoved += OnCursorMoved;

            Config.PropertyChanged += OnConfigPropertyChanged;

            if (Config.LazyUpdates)
            {
                helper.Events.GameLoop.OneSecondUpdateTicked += OnUpdate;
            }
            else
            {
                helper.Events.GameLoop.UpdateTicked += OnUpdate;
            }
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
            {
                return;
            }

            RegisterModConfig(configMenu);
        }

        private void OnReturnedToTitle(object? sender, ReturnedToTitleEventArgs e)
        {
            foreach (var patch in Patches)
            {
                patch.Detatch();
            }
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (e.Button == Config.ToggleKey)
            {
                Config.SetProperty(ref Config.Enabled, !Config.Enabled, nameof(Config.Enabled));
            }
        }

        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            if (Config.Enabled)
            {
                foreach (var patch in Patches)
                {
                    patch.Attach();
                }
            }
        }

        private void OnConfigPropertyChanged(object? sender, ProperyChangedEventArgs e)
        {
            if (e.Name == nameof(ModConfig.Enabled))
            {
                if ((bool)e.Value)
                {
                    foreach (var patch in Patches)
                    {
                        patch.Attach();
                    }
                }
                else
                {
                    foreach (var patch in Patches)
                    {
                        patch.Detatch();
                    }
                }
            }
            else if (Config.Enabled)
            {
                foreach (var patch in Patches)
                {
                    patch.Detatch();
                    patch.Attach();
                }
            }
        }

        private void OnRendered(object? sender, RenderedHudEventArgs e)
        {
            rootUIElement?.Draw(e.SpriteBatch);
            rootUIElement?.GetChildAt(cursorPosition)?.DrawTooltips(e.SpriteBatch);
        }

        private void OnCursorMoved(object? sender, CursorMovedEventArgs e)
        {
            cursorPosition = e.NewPosition.ScreenPixels * Game1.options.zoomLevel / Game1.options.uiScale;
        }

        private void OnUpdate(object? sender, EventArgs e)
        {
            var offset = Vector2.One * 4 + new Vector2(Config.MenuOffsetX, Config.MenuOffsetY);
            rootUIElement = new Grid(
                children: Patches.Select(x => x.GetMenu()),
                spacing: Vector2.One * 4,
                layout: string.Join(" ", Enumerable.Repeat("auto", Patches.Count))
            );
            rootUIElement.Update(offset);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var patch in Patches)
                {
                    patch.Detatch();
                }
            }
        }
    
        private void RegisterModConfig(IGenericModConfigMenuApi menu)
        {
            #region Setup
            menu.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );
            // menu.OnFieldChanged(this.ModManifest, OnModconfigFieldChanges);
            menu.AddSectionTitle(mod: ModManifest, text: () => Helper.Translation.Get("config.default.header"));
            menu.AddPageLink(ModManifest, "general", () => Helper.Translation.Get("config.general.title"));
            menu.AddPageLink(ModManifest, "items", () => Helper.Translation.Get("config.items.title"));
            menu.AddPageLink(ModManifest, "outlines", () => Helper.Translation.Get("config.outlines.title"));
            menu.AddPageLink(ModManifest, "trackers", () => Helper.Translation.Get("config.trackers.title"));
            menu.AddPageLink(ModManifest, "fishing", () => Helper.Translation.Get("config.fishing.title"));
            menu.AddPageLink(ModManifest, "menu", () => Helper.Translation.Get("config.menu.title"));
            #endregion

            #region General
            menu.AddPage(
                mod: ModManifest,
                pageId: "general",
                pageTitle: () => Helper.Translation.Get("config.general.title")
            );
            menu.AddBoolOption(
                mod: ModManifest,
                fieldId: nameof(Config.Enabled),
                name: () => Helper.Translation.Get($"options.{nameof(Config.Enabled)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.Enabled)}.desc"),
                getValue: () => Config.Enabled,
                setValue: value => Config.SetProperty(ref Config.Enabled, value, nameof(Config.Enabled))
            );
            menu.AddBoolOption(
                mod: ModManifest,
                fieldId: nameof(Config.LazyUpdates),
                name: () => Helper.Translation.Get("options.LazyUpdates"),
                tooltip: () => Helper.Translation.Get("options.LazyUpdates.desc"),
                getValue: () => Config.LazyUpdates,
                setValue: value => Config.SetProperty(ref Config.LazyUpdates, value, nameof(Config.LazyUpdates))
            );
            menu.AddKeybind(
                mod: ModManifest,
                fieldId: nameof(Config.ToggleKey),
                name: () => Helper.Translation.Get("options.ToggleKey"),
                tooltip: () => Helper.Translation.Get("options.ToggleKey.desc"),
                getValue: () => Config.ToggleKey,
                setValue: value => Config.SetProperty(ref Config.ToggleKey, value, nameof(Config.ToggleKey))
            );
            #endregion

            #region Items
            menu.AddPage(
                mod: ModManifest,
                pageId: "items",
                pageTitle: () => Helper.Translation.Get("config.items.title")
            );
            menu.AddSectionTitle(
                mod: ModManifest,
                text: () => Helper.Translation.Get("config.items.text")
            );
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.EnableHarvestableBushItems)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.EnableHarvestableBushItems)}.desc"),
                getValue: () => Config.EnableHarvestableBushItems,
                setValue: value => Config.SetProperty(ref Config.EnableHarvestableBushItems, value, nameof(Config.EnableHarvestableBushItems))
            );
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.EnableBreakableContainerItems)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.EnableBreakableContainerItems)}.desc"),
                getValue: () => Config.EnableBreakableContainerItems,
                setValue: value => Config.SetProperty(ref Config.EnableBreakableContainerItems, value, nameof(Config.EnableBreakableContainerItems))
            );
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.EnableDigSpotItems)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.EnableDigSpotItems)}.desc"),
                getValue: () => Config.EnableDigSpotItems,
                setValue: value => Config.SetProperty(ref Config.EnableDigSpotItems, value, nameof(Config.EnableDigSpotItems))
            );
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.EnableGeodeItems)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.EnableGeodeItems)}.desc"),
                getValue: () => Config.EnableGeodeItems,
                setValue: value => Config.SetProperty(ref Config.EnableGeodeItems, value, nameof(Config.EnableGeodeItems))
            );
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.EnableGarbageCanItems)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.EnableGarbageCanItems)}.desc"),
                getValue: () => Config.EnableGarbageCanItems,
                setValue: value => Config.SetProperty(ref Config.EnableGarbageCanItems, value, nameof(Config.EnableGarbageCanItems))
            );
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.EnablePaningSpotItems)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.EnablePaningSpotItems)}.desc"),
                getValue: () => Config.EnablePaningSpotItems,
                setValue: value => Config.SetProperty(ref Config.EnablePaningSpotItems, value, nameof(Config.EnablePaningSpotItems))
            );
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.EnableMineableObjectItems)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.EnableMineableObjectItems)}.desc"),
                getValue: () => Config.EnableMineableObjectItems,
                setValue: value => Config.SetProperty(ref Config.EnableMineableObjectItems, value, nameof(Config.EnableMineableObjectItems))
            );
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.EnableHarvestableTreeItems)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.EnableHarvestableTreeItems)}.desc"),
                getValue: () => Config.EnableHarvestableTreeItems,
                setValue: value => Config.SetProperty(ref Config.EnableHarvestableTreeItems, value, nameof(Config.EnableHarvestableTreeItems))
            );
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.EnableTillableItems)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.EnableTillableItems)}.desc"),
                getValue: () => Config.EnableTillableItems,
                setValue: value => Config.SetProperty(ref Config.EnableTillableItems, value, nameof(Config.EnableTillableItems))
            );
            #endregion

            #region Outlines
            menu.AddPage(
                mod: ModManifest,
                pageId: "outlines",
                pageTitle: () => Helper.Translation.Get("config.outlines.title")
            );
            menu.AddSectionTitle(
                mod: ModManifest,
                text: () => Helper.Translation.Get("config.outlines.text")
            );
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.EnableDigSpotOutlines)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.EnableDigSpotOutlines)}.desc"),
                getValue: () => Config.EnableDigSpotOutlines,
                setValue: value => Config.SetProperty(ref Config.EnableDigSpotOutlines, value, nameof(Config.EnableDigSpotOutlines))
            );
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.EnableHarvestableBushOutlines)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.EnableHarvestableBushOutlines)}.desc"),
                getValue: () => Config.EnableHarvestableBushOutlines,
                setValue: value => Config.SetProperty(ref Config.EnableHarvestableBushOutlines, value, nameof(Config.EnableHarvestableBushOutlines))
            );
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.EnableBreakableContainerOutlines)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.EnableBreakableContainerOutlines)}.desc"),
                getValue: () => Config.EnableBreakableContainerOutlines,
                setValue: value => Config.SetProperty(ref Config.EnableBreakableContainerOutlines, value, nameof(Config.EnableBreakableContainerOutlines))
            );
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.EnableSpawnedOutlines)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.EnableSpawnedOutlines)}.desc"),
                getValue: () => Config.EnableSpawnedOutlines,
                setValue: value => Config.SetProperty(ref Config.EnableSpawnedOutlines, value, nameof(Config.EnableSpawnedOutlines))
            );
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.EnableGarbageCanOutlines)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.EnableGarbageCanOutlines)}.desc"),
                getValue: () => Config.EnableGarbageCanOutlines,
                setValue: value => Config.SetProperty(ref Config.EnableGarbageCanOutlines, value, nameof(Config.EnableGarbageCanOutlines))
            );
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.EnableGarbageCanWarningOutlines)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.EnableGarbageCanWarningOutlines)}.desc"),
                getValue: () => Config.EnableGarbageCanWarningOutlines,
                setValue: value => Config.SetProperty(ref Config.EnableGarbageCanWarningOutlines, value, nameof(Config.EnableGarbageCanWarningOutlines))
            );
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.EnablePaningSpotOutlines)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.EnablePaningSpotOutlines)}.desc"),
                getValue: () => Config.EnablePaningSpotOutlines,
                setValue: value => Config.SetProperty(ref Config.EnablePaningSpotOutlines, value, nameof(Config.EnablePaningSpotOutlines))
            );
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.EnableMineableObjectOutlines)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.EnableMineableObjectOutlines)}.desc"),
                getValue: () => Config.EnableMineableObjectOutlines,
                setValue: value => Config.SetProperty(ref Config.EnableMineableObjectOutlines, value, nameof(Config.EnableMineableObjectOutlines))
            );
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.EnableHarvestableTreeOutlines)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.EnableHarvestableTreeOutlines)}.desc"),
                getValue: () => Config.EnableHarvestableTreeOutlines,
                setValue: value => Config.SetProperty(ref Config.EnableHarvestableTreeOutlines, value, nameof(Config.EnableHarvestableTreeOutlines))
            );
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.EnableTillableOutlines)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.EnableTillableOutlines)}.desc"),
                getValue: () => Config.EnableTillableOutlines,
                setValue: value => Config.SetProperty(ref Config.EnableTillableOutlines, value, nameof(Config.EnableTillableOutlines))
            );
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.EnableLadderOutlines)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.EnableLadderOutlines)}.desc"),
                getValue: () => Config.EnableLadderOutlines,
                setValue: value => Config.SetProperty(ref Config.EnableLadderOutlines, value, nameof(Config.EnableLadderOutlines))
            );
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.EnableMonstersHideLadders)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.EnableMonstersHideLadders)}.desc"),
                getValue: () => Config.EnableMonstersHideLadders,
                setValue: value => Config.SetProperty(ref Config.EnableMonstersHideLadders, value, nameof(Config.EnableMonstersHideLadders))
            );
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.EnableObjectOutlines)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.EnableObjectOutlines)}.desc"),
                getValue: () => Config.EnableObjectOutlines,
                setValue: value => Config.SetProperty(ref Config.EnableObjectOutlines, value, nameof(Config.EnableObjectOutlines))
            );
            #endregion

            #region Trackers
            menu.AddPage(
                mod: ModManifest,
                pageId: "trackers",
                pageTitle: () => Helper.Translation.Get("config.trackers.title")
            );
            menu.AddSectionTitle(
                mod: ModManifest,
                text: () => Helper.Translation.Get("config.trackers.text")
            );
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.TrackHarvestableBushes)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.TrackHarvestableBushes)}.desc"),
                getValue: () => Config.TrackHarvestableBushes,
                setValue: value => Config.SetProperty(ref Config.TrackHarvestableBushes, value, nameof(Config.TrackHarvestableBushes))
            );
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.TrackDigSpots)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.TrackDigSpots)}.desc"),
                getValue: () => Config.TrackDigSpots,
                setValue: value => Config.SetProperty(ref Config.TrackDigSpots, value, nameof(Config.TrackDigSpots))
            );
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.TrackSpawned)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.TrackSpawned)}.desc"),
                getValue: () => Config.TrackSpawned,
                setValue: value => Config.SetProperty(ref Config.TrackSpawned, value, nameof(Config.TrackSpawned))
            );
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.TrackPanningSpots)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.TrackPanningSpots)}.desc"),
                getValue: () => Config.TrackPanningSpots,
                setValue: value => Config.SetProperty(ref Config.TrackPanningSpots, value, nameof(Config.TrackPanningSpots))
            );
            menu.AddSectionTitle(ModManifest, () => Helper.Translation.Get("config.trackers.subsection1"));
            menu.AddNumberOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.TrackerMenuMaxItemCount)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.TrackerMenuMaxItemCount)}.desc"),
                getValue: () => Config.TrackerMenuMaxItemCount,
                setValue: value => Config.SetProperty(ref Config.TrackerMenuMaxItemCount, value, nameof(Config.TrackerMenuMaxItemCount))
            );
            menu.AddNumberOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.TrackerMenuMaxItemDistance)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.TrackerMenuMaxItemDistance)}.desc"),
                getValue: () => Config.TrackerMenuMaxItemDistance,
                setValue: value => Config.SetProperty(ref Config.TrackerMenuMaxItemDistance, value, nameof(Config.TrackerMenuMaxItemDistance))
            );
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.TrackerMenuShowLess)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.TrackerMenuShowLess)}.desc"),
                getValue: () => Config.TrackerMenuShowLess,
                setValue: value => Config.SetProperty(ref Config.TrackerMenuShowLess, value, nameof(Config.TrackerMenuShowLess))
            );
            #endregion

            #region Fishing
            menu.AddPage(
                mod: ModManifest,
                pageId: "fishing",
                pageTitle: () => Helper.Translation.Get("config.fishing.title")
            );
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.ShowFishInMinigame)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.ShowFishInMinigame)}.desc"),
                getValue: () => Config.ShowFishInMinigame,
                setValue: value => Config.SetProperty(ref Config.ShowFishInMinigame, value, nameof(Config.ShowFishInMinigame))
            );
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.ShowBait)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.ShowBait)}.desc"),
                getValue: () => Config.ShowBait,
                setValue: value => Config.SetProperty(ref Config.ShowBait, value, nameof(Config.ShowBait))
            );
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.ShowTackle)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.ShowTackle)}.desc"),
                getValue: () => Config.ShowTackle,
                setValue: value => Config.SetProperty(ref Config.ShowTackle, value, nameof(Config.ShowTackle))
            );
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.ShowFishChances)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.ShowFishChances)}.desc"),
                getValue: () => Config.ShowFishChances,
                setValue: value => Config.SetProperty(ref Config.ShowFishChances, value, nameof(Config.ShowFishChances))
            );
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.EstimateFishChances)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.EstimateFishChances)}.desc"),
                getValue: () => Config.EstimateFishChances,
                setValue: value => Config.SetProperty(ref Config.EstimateFishChances, value, nameof(Config.EstimateFishChances))
            );
            menu.AddNumberOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.NumFishCatches)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.NumFishCatches)}.desc"),
                getValue: () => Config.NumFishCatches,
                setValue: value => Config.SetProperty(ref Config.NumFishCatches, value, nameof(Config.NumFishCatches))
            );
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.ShowTrashChances)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.ShowTrashChances)}.desc"),
                getValue: () => Config.ShowTrashChances,
                setValue: value => Config.SetProperty(ref Config.ShowTrashChances, value, nameof(Config.ShowTrashChances))
            );
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.ShowSecretNoteChances)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.ShowSecretNoteChances)}.desc"),
                getValue: () => Config.ShowSecretNoteChances,
                setValue: value => Config.SetProperty(ref Config.ShowSecretNoteChances, value, nameof(Config.ShowSecretNoteChances))
            );
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.ShowUncaughtFish)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.ShowUncaughtFish)}.desc"),
                getValue: () => Config.ShowUncaughtFish,
                setValue: value => Config.SetProperty(ref Config.ShowUncaughtFish, value, nameof(Config.ShowUncaughtFish))
            );
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.ShowLessFishInfo)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.ShowLessFishInfo)}.desc"),
                getValue: () => Config.ShowLessFishInfo,
                setValue: value => Config.SetProperty(ref Config.ShowLessFishInfo, value, nameof(Config.ShowLessFishInfo))
            );
            #endregion

            #region Menu
            menu.AddPage(
                mod: ModManifest,
                pageId: "menu",
                pageTitle: () => Helper.Translation.Get("config.fishing.title")
            );
            menu.AddNumberOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.MenuOffsetX)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.MenuOffsetX)}.desc"),
                getValue: () => Config.MenuOffsetX,
                setValue: value => Config.SetProperty(ref Config.MenuOffsetX, value, nameof(Config.MenuOffsetX))
            );
            menu.AddNumberOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.MenuOffsetY)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.MenuOffsetY)}.desc"),
                getValue: () => Config.MenuOffsetY,
                setValue: value => Config.SetProperty(ref Config.MenuOffsetY, value, nameof(Config.MenuOffsetY))
            );
            var menyTypeOptions = ModConfig.MenyTypeOptions.Select(i =>
                Helper.Translation.Get($"options.{nameof(Config.MenuType)}.items.{i}").ToString()
            ).ToList();
            menu.AddTextOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.MenuType)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.MenuType)}.desc"),
                getValue: () => menyTypeOptions[Config.MenuType],
                setValue: value => Config.MenuType = menyTypeOptions.IndexOf(value),
                allowedValues: menyTypeOptions.ToArray()
            );
            menu.AddNumberOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.MenuScale)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.MenuScale)}.desc"),
                getValue: () => Config.MenuScale,
                setValue: value => Config.SetProperty(ref Config.MenuScale, value, nameof(Config.MenuScale)),
                min: 0.1f,
                max: 10f
            );
            menu.AddNumberOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.MenuAlpha)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.MenuAlpha)}.desc"),
                getValue: () => Config.MenuAlpha,
                setValue: value => Config.SetProperty(ref Config.MenuAlpha, value, nameof(Config.MenuAlpha)),
                min: 0f,
                max: 1f
            );
            #endregion
        }
    }
}

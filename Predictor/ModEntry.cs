using Predictor.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using DynamicUIFramework.Elements;
using PredictorPatchFramework;
using PredictorPatchFramework.Extentions;
using DynamicUIFramework;

namespace Predictor
{
    internal class MultiplayerRootContext
    {
        public Group RootUIElement;
        public IUIDrawable? Tooltips;

        public MultiplayerRootContext()
        {
            RootUIElement = new Group(spacing: Utils.MenuSpacing.X);
        }

        public MultiplayerRootContext(Group root)
        {
            RootUIElement = root;
        }
    }

    internal class ModEntry : Mod
    {
        private static ModEntry? _instance;
        public static ModEntry Instance 
        { 
            get => _instance ?? throw new NullReferenceException(nameof(Instance));
            private set => _instance = value; 
        }

        public ModConfig Config { get; private set; } = new ModConfig();

        internal MultiplayerManager<MultiplayerRootContext> MultiplayerManager = new();
        private List<IPatch> Patches = new();
        public Group RootUIElement
        {
            get => MultiplayerManager.GetValue().RootUIElement;
            set => MultiplayerManager.GetValue().RootUIElement = value;
        }
        public IUIDrawable? Tooltips
        {
            get => MultiplayerManager.GetValue().Tooltips;
            set => MultiplayerManager.GetValue().Tooltips = value;
        }
        private bool EventsRegistered = false;

        public void UnsubscribeEventHandlers()
        {
            MultiplayerManager.ClearConnections();
            if (EventsRegistered)
            {
                EventsRegistered = false;
                Helper.Events.Display.RenderedHud -= OnRendered;
                Helper.Events.GameLoop.OneSecondUpdateTicked -= OnUpdate;
                Helper.Events.GameLoop.UpdateTicked -= OnUpdate;
            }

            foreach (var patch in Patches)
            {
                patch.Detatch();
            }
        }

        public void SubscribeEventHandlers()
        {
            if (!EventsRegistered)
            {
                EventsRegistered = true;
                Helper.Events.Display.RenderedHud += OnRendered;

                if (Config.LazyUpdates)
                {
                    Helper.Events.GameLoop.OneSecondUpdateTicked += OnUpdate;
                }
                else
                {
                    Helper.Events.GameLoop.UpdateTicked += OnUpdate;
                }
            }

            foreach (var patch in Patches)
            {
                patch.Attach();
            }
        }

        public void RetatchPatches()
        {
            foreach (var patch in Patches)
            {
                patch.Detatch();
                if (Config.Enabled)
                {
                    patch.Attach();
                }
            }
        }

        public bool RegisterPatch(IPatch patch)
        {
            if (Patches.Contains(patch))
            {
                return false;
            }

            Patches.Add(patch);
            if (Config.Enabled && EventsRegistered)
            {
                patch.Attach();
            }

            return true;
        }

        public bool DeRegisterPatch(IPatch patch)
        {
            return this.Patches.Remove(patch);
        }

        public override void Entry(IModHelper helper)
        {
            Instance = this;
            Config = helper.ReadConfig<ModConfig>();
            MultiplayerManager.Init(helper, Monitor);
            Patches = new List<IPatch>()
            {
                new Patch(helper, Monitor)
            };

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
            helper.Events.Input.ButtonPressed += OnButtonPressed;

            Config.PropertyChanged += OnConfigPropertyChanged;
        }

        public override object GetApi()
        {
            return new ModAPI();
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            FrameworkUtils.Initialize(Helper);
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
            {
                return;
            }

            RegisterModConfig(configMenu);
        }

        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            if (!Config.Enabled)
            {
                return;
            }

            SubscribeEventHandlers();
        }

        private void OnReturnedToTitle(object? sender, ReturnedToTitleEventArgs e)
        {
            UnsubscribeEventHandlers();
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (e.Button == Config.ToggleKey && Context.IsMainPlayer)
            {
                Config.SetProperty(ref Config.Enabled, !Config.Enabled, nameof(Config.Enabled));
            }
        }

        private void OnConfigPropertyChanged(object? sender, ProperyChangedEventArgs e)
        {
            if (e.Name == nameof(ModConfig.Enabled))
            {
                if ((bool)e.Value)
                {
                    SubscribeEventHandlers();
                }
                else
                {
                    UnsubscribeEventHandlers();
                }
            }
            else if (Config.Enabled && EventsRegistered)
            {
                RetatchPatches();
            }
        }

        private void OnRendered(object? sender, RenderedHudEventArgs e)
        {
            RootUIElement.Draw(e.SpriteBatch, Config.GetMenuOffset());
            Tooltips?.Draw(e.SpriteBatch);
        }

        private void OnUpdate(object? sender, EventArgs e)
        {
            RootUIElement.Children = Patches.Select(x => x.GetMenu());
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
            menu.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );

            #region General
            menu.AddSectionTitle(
                mod: ModManifest,
                text: () => Helper.Translation.Get("config.general.title")
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
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.EnableObjectOutlines)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.EnableObjectOutlines)}.desc"),
                getValue: () => Config.EnableObjectOutlines,
                setValue: value => Config.SetProperty(ref Config.EnableObjectOutlines, value, nameof(Config.EnableObjectOutlines))
            );
            #endregion

            #region Menus
            menu.AddSectionTitle(
                mod: ModManifest,
                text: () => Helper.Translation.Get("config.menus.title")
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
            menu.AddTextOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.MenuType)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.MenuType)}.desc"),
                getValue: () => Config.MenuType.ToString(),
                setValue: value => Config.SetProperty(ref Config.MenuType, int.Parse(value), nameof(Config.MenuType)),
                formatAllowedValue: value => Helper.Translation.Get($"options.{nameof(Config.MenuType)}.items.{value}"),
                allowedValues: ModConfig.MenyTypeOptions.Select(value => value.ToString()).ToArray()
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
                name: () => Helper.Translation.Get($"options.{nameof(Config.MenuBorderWidth)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.MenuBorderWidth)}.desc"),
                getValue: () => Config.MenuBorderWidth,
                setValue: value => Config.SetProperty(ref Config.MenuBorderWidth, value, nameof(Config.MenuBorderWidth)),
                min: 0f,
                max: 32f
            );
            menu.AddColorOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.MenuBackgroundColor)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.MenuBackgroundColor)}.desc"),
                getValue: () => Config.MenuBackgroundColor,
                setValue: value => Config.SetProperty(ref Config.MenuBackgroundColor, value, nameof(Config.MenuBackgroundColor))
            );
            menu.AddColorOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.MenuTextColor)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.MenuTextColor)}.desc"),
                getValue: () => Config.MenuTextColor,
                setValue: value => Config.SetProperty(ref Config.MenuTextColor, value, nameof(Config.MenuTextColor))
            );
            #endregion
            #region Outlines
            menu.AddSectionTitle(
                mod: ModManifest,
                text: () => Helper.Translation.Get("config.outlines.title")
            );
            menu.AddNumberOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.OutlineWidth)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.OutlineWidth)}.desc"),
                getValue: () => Config.OutlineWidth,
                setValue: value => Config.SetProperty(ref Config.OutlineWidth, value, nameof(Config.OutlineWidth)),
                min: 0f,
                max: 32f
            );
            menu.AddNumberOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.ThickOutlineWidth)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.ThickOutlineWidth)}.desc"),
                getValue: () => Config.ThickOutlineWidth,
                setValue: value => Config.SetProperty(ref Config.ThickOutlineWidth, value, nameof(Config.ThickOutlineWidth)),
                min: 0f,
                max: 32f
            );
            menu.AddColorOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.OutlineColor)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.OutlineColor)}.desc"),
                getValue: () => Config.OutlineColor,
                setValue: value => Config.SetProperty(ref Config.OutlineColor, value, nameof(Config.OutlineColor))
            );
            #endregion
        }
    }
}

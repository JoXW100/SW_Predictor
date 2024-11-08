using StardewModdingAPI;
using StardewModdingAPI.Events;
using DynamicUIFramework.Elements;
using PredictorPatchFramework;
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
        private static ModAPI ModAPI { get; } = new ModAPI();

        private static ModEntry? _instance;
        public static ModEntry Instance 
        { 
            get => _instance ?? throw new NullReferenceException(nameof(Instance));
            private set => _instance = value; 
        }

        public ModConfig Config { get; private set; } = new ModConfig();


        public IGenericModConfigMenuApi? GenericModConfigAPI { get; private set; } 

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

        public override object GetApi() => ModAPI;

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            FrameworkUtils.Initialize(Helper);
            GenericModConfigAPI = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            RegisterModConfig();
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

        private void RegisterModConfig()
        {
            if (GenericModConfigAPI is null)
            {
                return;
            }

            GenericModConfigAPI.Register(
                mod: ModManifest,
                reset: ModAPI.ResetPatchConfigs,
                save: ModAPI.SavePatchConfigs
            );

            GenericModConfigAPI.AddSectionTitle(ModManifest, () => Helper.Translation.Get("config.category.core"));

            ModAPI.RegisterPatchConfig(
                manifest: ModManifest,
                registerOptions: api =>
                {
                    #region General
                    api.AddSectionTitle(() => Helper.Translation.Get("config.general.title"));
                    api.AddBoolOption(
                        getName: () => Helper.Translation.Get($"options.{nameof(Config.Enabled)}"),
                        getTooltip: () => Helper.Translation.Get($"options.{nameof(Config.Enabled)}.desc"),
                        getValue: () => Config.Enabled,
                        setValue: value => Config.SetProperty(ref Config.Enabled, value, nameof(Config.Enabled))
                    );
                    api.AddBoolOption(
                        getName: () => Helper.Translation.Get("options.LazyUpdates"),
                        getTooltip: () => Helper.Translation.Get("options.LazyUpdates.desc"),
                        getValue: () => Config.LazyUpdates,
                        setValue: value => Config.SetProperty(ref Config.LazyUpdates, value, nameof(Config.LazyUpdates))
                    );
                    api.AddKeybind(
                        getName: () => Helper.Translation.Get("options.ToggleKey"),
                        getTooltip: () => Helper.Translation.Get("options.ToggleKey.desc"),
                        getValue: () => Config.ToggleKey,
                        setValue: value => Config.SetProperty(ref Config.ToggleKey, value, nameof(Config.ToggleKey))
                    );
                    api.AddBoolOption(
                        getName: () => Helper.Translation.Get($"options.{nameof(Config.EnableObjectOutlines)}"),
                        getTooltip: () => Helper.Translation.Get($"options.{nameof(Config.EnableObjectOutlines)}.desc"),
                        getValue: () => Config.EnableObjectOutlines,
                        setValue: value => Config.SetProperty(ref Config.EnableObjectOutlines, value, nameof(Config.EnableObjectOutlines))
                    );
                    #endregion
                    #region Menus
                    api.AddSectionTitle(() => Helper.Translation.Get("config.menus.title"));
                    api.AddNumberOption(
                        getName: () => Helper.Translation.Get($"options.{nameof(Config.MenuOffsetX)}"),
                        getTooltip: () => Helper.Translation.Get($"options.{nameof(Config.MenuOffsetX)}.desc"),
                        getValue: () => Config.MenuOffsetX,
                        setValue: value => Config.SetProperty(ref Config.MenuOffsetX, value, nameof(Config.MenuOffsetX))
                    );
                    api.AddNumberOption(
                        getName: () => Helper.Translation.Get($"options.{nameof(Config.MenuOffsetY)}"),
                        getTooltip: () => Helper.Translation.Get($"options.{nameof(Config.MenuOffsetY)}.desc"),
                        getValue: () => Config.MenuOffsetY,
                        setValue: value => Config.SetProperty(ref Config.MenuOffsetY, value, nameof(Config.MenuOffsetY))
                    );
                    api.AddTextOption(
                        getName: () => Helper.Translation.Get($"options.{nameof(Config.MenuType)}"),
                        getTooltip: () => Helper.Translation.Get($"options.{nameof(Config.MenuType)}.desc"),
                        getValue: () => Config.MenuType.ToString(),
                        setValue: value => Config.SetProperty(ref Config.MenuType, int.Parse(value), nameof(Config.MenuType)),
                        formatAllowedValue: value => Helper.Translation.Get($"options.{nameof(Config.MenuType)}.items.{value}"),
                        allowedValues: ModConfig.MenyTypeOptions.Select(value => value.ToString()).ToArray()
                    );
                    api.AddNumberOption(
                        getName: () => Helper.Translation.Get($"options.{nameof(Config.MenuScale)}"),
                        getTooltip: () => Helper.Translation.Get($"options.{nameof(Config.MenuScale)}.desc"),
                        getValue: () => Config.MenuScale,
                        setValue: value => Config.SetProperty(ref Config.MenuScale, value, nameof(Config.MenuScale)),
                        min: 0.1f,
                        max: 10f
                    );
                    api.AddNumberOption(
                        getName: () => Helper.Translation.Get($"options.{nameof(Config.MenuBorderWidth)}"),
                        getTooltip: () => Helper.Translation.Get($"options.{nameof(Config.MenuBorderWidth)}.desc"),
                        getValue: () => Config.MenuBorderWidth,
                        setValue: value => Config.SetProperty(ref Config.MenuBorderWidth, value, nameof(Config.MenuBorderWidth)),
                        min: 0f,
                        max: 32f
                    );
                    api.AddColorOption(
                        getName: () => Helper.Translation.Get($"options.{nameof(Config.MenuBackgroundColor)}"),
                        getTooltip: () => Helper.Translation.Get($"options.{nameof(Config.MenuBackgroundColor)}.desc"),
                        getValue: () => Config.MenuBackgroundColor,
                        setValue: value => Config.SetProperty(ref Config.MenuBackgroundColor, value, nameof(Config.MenuBackgroundColor))
                    );
                    api.AddColorOption(
                        getName: () => Helper.Translation.Get($"options.{nameof(Config.MenuTextColor)}"),
                        getTooltip: () => Helper.Translation.Get($"options.{nameof(Config.MenuTextColor)}.desc"),
                        getValue: () => Config.MenuTextColor,
                        setValue: value => Config.SetProperty(ref Config.MenuTextColor, value, nameof(Config.MenuTextColor))
                    );
                    #endregion
                    #region Outlines
                    api.AddSectionTitle(() => Helper.Translation.Get("config.outlines.title")
                    );
                    api.AddNumberOption(
                        getName: () => Helper.Translation.Get($"options.{nameof(Config.OutlineWidth)}"),
                        getTooltip: () => Helper.Translation.Get($"options.{nameof(Config.OutlineWidth)}.desc"),
                        getValue: () => Config.OutlineWidth,
                        setValue: value => Config.SetProperty(ref Config.OutlineWidth, value, nameof(Config.OutlineWidth)),
                        min: 0f,
                        max: 32f
                    );
                    api.AddNumberOption(
                        getName: () => Helper.Translation.Get($"options.{nameof(Config.ThickOutlineWidth)}"),
                        getTooltip: () => Helper.Translation.Get($"options.{nameof(Config.ThickOutlineWidth)}.desc"),
                        getValue: () => Config.ThickOutlineWidth,
                        setValue: value => Config.SetProperty(ref Config.ThickOutlineWidth, value, nameof(Config.ThickOutlineWidth)),
                        min: 0f,
                        max: 32f
                    );
                    api.AddColorOption(
                        getName: () => Helper.Translation.Get($"options.{nameof(Config.OutlineColor)}"),
                        getTooltip: () => Helper.Translation.Get($"options.{nameof(Config.OutlineColor)}.desc"),
                        getValue: () => Config.OutlineColor,
                        setValue: value => Config.SetProperty(ref Config.OutlineColor, value, nameof(Config.OutlineColor))
                    );
                    #endregion
                },
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config));


            GenericModConfigAPI.AddPage(ModManifest, string.Empty);
            GenericModConfigAPI.AddSectionTitle(ModManifest, () => Helper.Translation.Get("config.category.patches"));
        }
    }
}

using PredictorPatchFramework;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace PredictorGarbageCanPatch
{
    internal class ModEntry : Mod
    {
        private static ModEntry? _instance;
        public static ModEntry Instance
        {
            get => _instance ?? throw new NullReferenceException(nameof(Instance));
            private set => _instance = value;
        }

        public ModConfig Config { get; private set; } = new ModConfig();

        public Patch? Patch;

        public override void Entry(IModHelper helper)
        {
            _instance = this;
            Config = helper.ReadConfig<ModConfig>();
            Patch = new Patch(Helper, Monitor);
            Config.PropertyChanged += OnConfigPropertyChanged;
            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;

        }

        private void OnConfigPropertyChanged(object? sender, ProperyChangedEventArgs e)
        {
            if (Patch is null)
            {
                return;
            }

            if (e.Name == nameof(ModConfig.Enabled))
            {
                if ((bool)e.Value)
                {
                    FrameworkUtils.API.RegisterPatch(Patch);
                }
                else
                {
                    FrameworkUtils.API.DeRegisterPatch(Patch);
                }
            }
            else if (Config.Enabled)
            {
                FrameworkUtils.API.RetatchPatches();
            }
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            FrameworkUtils.Initialize(Helper);
            if (Patch is not null && Config.Enabled)
            {
                FrameworkUtils.API.RegisterPatch(Patch);
            }

            FrameworkUtils.API.RegisterPatchConfig(
            manifest: ModManifest,
            reset: () => Config = new ModConfig(),
            save: () => Helper.WriteConfig(Config),
            registerOptions: api =>
            {
                api.AddSectionTitle(() => Helper.Translation.Get("config.general.title"));
                api.AddBoolOption(
                    getName: () => Helper.Translation.Get($"options.{nameof(Config.Enabled)}"),
                    getTooltip: () => Helper.Translation.Get($"options.{nameof(Config.Enabled)}.desc"),
                    getValue: () => Config.Enabled,
                    setValue: value => Config.SetProperty(ref Config.Enabled, value, nameof(Config.Enabled))
                );
                api.AddSectionTitle(() => Helper.Translation.Get("config.features.title"));
                api.AddBoolOption(
                    getName: () => Helper.Translation.Get($"options.{nameof(Config.ShowOutlines)}"),
                    getTooltip: () => Helper.Translation.Get($"options.{nameof(Config.ShowOutlines)}.desc"),
                    getValue: () => Config.ShowOutlines,
                    setValue: value => Config.SetProperty(ref Config.ShowOutlines, value, nameof(Config.ShowOutlines))
                );
                api.AddBoolOption(
                    getName: () => Helper.Translation.Get($"options.{nameof(Config.ShowNearbyNPCWarning)}"),
                    getTooltip: () => Helper.Translation.Get($"options.{nameof(Config.ShowNearbyNPCWarning)}.desc"),
                    getValue: () => Config.ShowNearbyNPCWarning,
                    setValue: value => Config.SetProperty(ref Config.ShowNearbyNPCWarning, value, nameof(Config.ShowNearbyNPCWarning))
                );
                api.AddColorOption(
                    getName: () => Helper.Translation.Get($"options.{nameof(Config.GarbageCanOkColor)}"),
                    getTooltip: () => Helper.Translation.Get($"options.{nameof(Config.GarbageCanOkColor)}.desc"),
                    getValue: () => Config.GarbageCanOkColor,
                    setValue: value => Config.SetProperty(ref Config.GarbageCanOkColor, value, nameof(Config.GarbageCanOkColor))
                );
                api.AddColorOption(
                    getName: () => Helper.Translation.Get($"options.{nameof(Config.GarbageCanWarnColor)}"),
                    getTooltip: () => Helper.Translation.Get($"options.{nameof(Config.GarbageCanWarnColor)}.desc"),
                    getValue: () => Config.GarbageCanWarnColor,
                    setValue: value => Config.SetProperty(ref Config.GarbageCanWarnColor, value, nameof(Config.GarbageCanWarnColor))
                );
            });
        }
    }
}

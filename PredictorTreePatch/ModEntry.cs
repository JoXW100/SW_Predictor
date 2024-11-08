using PredictorPatchFramework;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace PredictorTreePatch
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
                    getName: () => Helper.Translation.Get($"options.{nameof(Config.ShowItems)}"),
                    getTooltip: () => Helper.Translation.Get($"options.{nameof(Config.ShowItems)}.desc"),
                    getValue: () => Config.ShowItems,
                    setValue: value => Config.SetProperty(ref Config.ShowItems, value, nameof(Config.ShowItems))
                );
                api.AddBoolOption(
                    getName: () => Helper.Translation.Get($"options.{nameof(Config.ShowOutlines)}"),
                    getTooltip: () => Helper.Translation.Get($"options.{nameof(Config.ShowOutlines)}.desc"),
                    getValue: () => Config.ShowOutlines,
                    setValue: value => Config.SetProperty(ref Config.ShowOutlines, value, nameof(Config.ShowOutlines))
                );
                api.AddBoolOption(
                    getName: () => Helper.Translation.Get($"options.{nameof(Config.ShowTrackers)}"),
                    getTooltip: () => Helper.Translation.Get($"options.{nameof(Config.ShowTrackers)}.desc"),
                    getValue: () => Config.ShowTrackers,
                    setValue: value => Config.SetProperty(ref Config.ShowTrackers, value, nameof(Config.ShowTrackers))
                );
                api.AddBoolOption(
                    getName: () => Helper.Translation.Get($"options.{nameof(Config.TrackerShowLess)}"),
                    getTooltip: () => Helper.Translation.Get($"options.{nameof(Config.TrackerShowLess)}.desc"),
                    getValue: () => Config.TrackerShowLess,
                    setValue: value => Config.SetProperty(ref Config.TrackerShowLess, value, nameof(Config.TrackerShowLess))
                );
                api.AddNumberOption(
                    getName: () => Helper.Translation.Get($"options.{nameof(Config.TrackerMaxItemCount)}"),
                    getTooltip: () => Helper.Translation.Get($"options.{nameof(Config.TrackerMaxItemCount)}.desc"),
                    getValue: () => Config.TrackerMaxItemCount,
                    setValue: value => Config.SetProperty(ref Config.TrackerMaxItemCount, value, nameof(Config.TrackerMaxItemCount)),
                    min: 0,
                    max: 100
                );
            });
        }
    }
}

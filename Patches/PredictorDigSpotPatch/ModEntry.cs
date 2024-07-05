using PredictorPatchFramework;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace PredictorDigSpotPatch
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
            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

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

        private void GameLoop_GameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            FrameworkUtils.Initialize(Helper);
            if (Patch is not null)
            {
                FrameworkUtils.API.RegisterPatch(Patch);
            }

            var menu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (menu is null)
            {
                return;
            }

            menu.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );
            menu.AddSectionTitle(
                mod: ModManifest, 
                text: () => Helper.Translation.Get("config.general.title")
            );
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.Enabled)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.Enabled)}.desc"),
                getValue: () => Config.Enabled,
                setValue: value => Config.SetProperty(ref Config.Enabled, value, nameof(Config.Enabled))
            );
            menu.AddSectionTitle(
                mod: ModManifest,
                text: () => Helper.Translation.Get("config.features.title")
            );
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.ShowItems)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.ShowItems)}.desc"),
                getValue: () => Config.ShowItems,
                setValue: value => Config.SetProperty(ref Config.ShowItems, value, nameof(Config.ShowItems))
            );
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.ShowOutlines)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.ShowOutlines)}.desc"),
                getValue: () => Config.ShowOutlines,
                setValue: value => Config.SetProperty(ref Config.ShowOutlines, value, nameof(Config.ShowOutlines))
            );
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.ShowTrackers)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.ShowTrackers)}.desc"),
                getValue: () => Config.ShowTrackers,
                setValue: value => Config.SetProperty(ref Config.ShowTrackers, value, nameof(Config.ShowTrackers))
            );
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.TrackerShowLess)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.TrackerShowLess)}.desc"),
                getValue: () => Config.TrackerShowLess,
                setValue: value => Config.SetProperty(ref Config.TrackerShowLess, value, nameof(Config.TrackerShowLess))
            );
            menu.AddNumberOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.TrackerMaxItemCount)}"),
                min: 0,
                max: 100,
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.TrackerMaxItemCount)}.desc"),
                getValue: () => Config.TrackerMaxItemCount,
                setValue: value => Config.SetProperty(ref Config.TrackerMaxItemCount, value, nameof(Config.TrackerMaxItemCount))
            );
        }
    }
}

using PredictorPatchFramework;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace PredictorFishingPatch
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
                name: () => Helper.Translation.Get($"options.{nameof(Config.ShowFish)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.ShowFish)}.desc"),
                getValue: () => Config.ShowFish,
                setValue: value => Config.SetProperty(ref Config.ShowFish, value, nameof(Config.ShowFish))
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
                min: 1,
                max: 9999,
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.NumFishCatches)}.desc"),
                getValue: () => Config.NumFishCatches,
                setValue: value => Config.SetProperty(ref Config.NumFishCatches, value, nameof(Config.NumFishCatches))
            );
        }
    }
}

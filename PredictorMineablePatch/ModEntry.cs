using PredictorPatchFramework;
using PredictorPatchFramework.Extentions;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace PredictorMineablePatch
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
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.RequireTool)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.RequireTool)}.desc"),
                getValue: () => Config.RequireTool,
                setValue: value => Config.SetProperty(ref Config.RequireTool, value, nameof(Config.RequireTool))
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
                name: () => Helper.Translation.Get($"options.{nameof(Config.ShowLadders)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.ShowLadders)}.desc"),
                getValue: () => Config.ShowLadders,
                setValue: value => Config.SetProperty(ref Config.ShowLadders, value, nameof(Config.ShowLadders))
            );
            menu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.MonstersHideLadders)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.MonstersHideLadders)}.desc"),
                getValue: () => Config.MonstersHideLadders,
                setValue: value => Config.SetProperty(ref Config.MonstersHideLadders, value, nameof(Config.MonstersHideLadders))
            );
            menu.AddColorOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.LadderColor)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.LadderColor)}.desc"),
                getValue: () => Config.LadderColor,
                setValue: value => Config.SetProperty(ref Config.LadderColor, value, nameof(Config.LadderColor))
            );
            menu.AddColorOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get($"options.{nameof(Config.ShaftColor)}"),
                tooltip: () => Helper.Translation.Get($"options.{nameof(Config.ShaftColor)}.desc"),
                getValue: () => Config.ShaftColor,
                setValue: value => Config.SetProperty(ref Config.ShaftColor, value, nameof(Config.ShaftColor))
            );
        }
    }
}

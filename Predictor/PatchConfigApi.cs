using Microsoft.Xna.Framework;
using PredictorPatchFramework;
using StardewModdingAPI;

namespace Predictor
{
    internal class PatchConfigApi : IPatchConfigApi
    {
        private readonly IGenericModConfigMenuApi api;
        private readonly IManifest manifest;

        private PatchConfigApi(IGenericModConfigMenuApi api, IManifest manifest, Action reset, Action save)
        {
            this.api = api;
            this.manifest = manifest;
            Reset = reset;
            Save = save;
        }

        public Action Reset { get; }
        public Action Save { get; }

        public static PatchConfigApi Register(IGenericModConfigMenuApi api, IManifest manifest, string uniqueId, string name, Action reset, Action save)
        {
            api.AddPage(manifest, string.Empty); // Go to default
            api.AddPageLink(manifest, uniqueId, () => name);
            api.AddPage(manifest, uniqueId, () => name);
            return new PatchConfigApi(api, manifest, reset, save);
        }

        public void AddSectionTitle(Func<string> getTitle)
        {
            api.AddSectionTitle(manifest, getTitle);
        }

        public void AddBoolOption(Func<bool> getValue, Action<bool> setValue, Func<string> getName, Func<string> getTooltip)
        {
            api.AddBoolOption(manifest, getValue, setValue, getName, getTooltip);
        }

        public void AddNumberOption(Func<int> getValue, Action<int> setValue, Func<string> getName, Func<string> getTooltip, int? min = null, int? max = null, int? interval = null, Func<int, string>? formatValue = null)
        {
            api.AddNumberOption(manifest, getValue, setValue, getName, getTooltip, min, max, interval, formatValue);
        }

        public void AddNumberOption(Func<float> getValue, Action<float> setValue, Func<string> getName, Func<string> getTooltip, float? min = null, float? max = null, float? interval = null, Func<float, string>? formatValue = null)
        {
            api.AddNumberOption(manifest, getValue, setValue, getName, getTooltip, min, max, interval, formatValue);
        }

        public void AddTextOption(Func<string> getValue, Action<string> setValue, Func<string> getName, Func<string> getTooltip, string[]? allowedValues = null, Func<string, string>? formatAllowedValue = null)
        {
            api.AddTextOption(manifest, getValue, setValue, getName, getTooltip, allowedValues, formatAllowedValue);
        }

        public void AddColorOption(Func<Color> getValue, Action<Color> setValue, Func<string> getName, Func<string> getTooltip)
        {
            api.AddColorOption(manifest, getValue, setValue, getName, getTooltip);
        }

        public void AddKeybind(Func<SButton> getValue, Action<SButton> setValue, Func<string> getName, Func<string> getTooltip)
        {
            api.AddKeybind(manifest, getValue, setValue, getName, getTooltip);
        }
    }
}

using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace PredictorPatchFramework
{
    public interface IPatchConfigApi
    {
        void AddSectionTitle(Func<string> getTitle);

        void AddBoolOption(Func<bool> getValue, Action<bool> setValue, Func<string> getName, Func<string> getTooltip);

        void AddNumberOption(Func<int> getValue, Action<int> setValue, Func<string> getName, Func<string> getTooltip, int? min = null, int? max = null, int? interval = null, Func<int, string>? formatValue = null);

        void AddNumberOption(Func<float> getValue, Action<float> setValue, Func<string> getName, Func<string> getTooltip, float? min = null, float? max = null, float? interval = null, Func<float, string>? formatValue = null);

        void AddTextOption(Func<string> getValue, Action<string> setValue, Func<string> getName, Func<string> getTooltip, string[]? allowedValues = null, Func<string, string>? formatAllowedValue = null);

        void AddColorOption(Func<Color> getValue, Action<Color> setValue, Func<string> getName, Func<string> getTooltip);

        void AddKeybind(Func<SButton> getValue, Action<SButton> setValue, Func<string> getName, Func<string> getTooltip);
    }
}
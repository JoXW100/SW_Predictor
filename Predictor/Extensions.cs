using DynamicUIFramework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace Predictor
{
    public static class Extensions
    {
        private const int ColorPreviewSize = 32;

        public static void AddColorOption(
            this IGenericModConfigMenuApi api,
            IManifest mod,
            Func<Color> getValue,
            Action<Color> setValue,
            Func<string> name,
            Func<string> tooltip,
            string? fieldId = null)
        {
            api.AddComplexOption(
                mod: mod,
                height: () => ColorPreviewSize,
                name: name,
                tooltip: tooltip,
                draw: (SpriteBatch sb, Vector2 offset) =>
                {
                    var value = getValue.Invoke();
                    sb.DrawArea(new Rectangle((int)offset.X, (int)offset.Y, ColorPreviewSize, ColorPreviewSize), value);
                }
            );
            api.AddNumberOption(
                mod: mod,
                name: () => $"{name.Invoke()}: R",
                tooltip: tooltip,
                getValue: () => getValue.Invoke().R,
                setValue: (value) =>
                {
                    var current = getValue.Invoke();
                    setValue.Invoke(new Color(value, current.G, current.B, current.A));
                },
                min: 0,
                max: byte.MaxValue,
                fieldId: fieldId is null ? null : $"{fieldId}.A"
            );
            api.AddNumberOption(
                mod: mod,
                name: () => $"{name.Invoke()}: G",
                tooltip: tooltip,
                getValue: () => getValue.Invoke().G,
                setValue: (value) =>
                {
                    var current = getValue.Invoke();
                    setValue.Invoke(new Color(current.R, value, current.B, current.A));
                },
                min: 0,
                max: byte.MaxValue,
                fieldId: fieldId is null ? null : $"{fieldId}.G"
            );
            api.AddNumberOption(
                mod: mod,
                name: () => $"{name.Invoke()}: B",
                tooltip: tooltip,
                getValue: () => getValue.Invoke().B,
                setValue: (value) =>
                {
                    var current = getValue.Invoke();
                    setValue.Invoke(new Color(current.R, current.G, value, current.A));
                },
                min: 0,
                max: byte.MaxValue,
                fieldId: fieldId is null ? null : $"{fieldId}.B"
            );
            api.AddNumberOption(
                mod: mod,
                name: () => $"{name.Invoke()}: A",
                tooltip: tooltip,
                getValue: () => getValue.Invoke().A,
                setValue: (value) =>
                {
                    var current = getValue.Invoke();
                    setValue.Invoke(new Color(current.R, current.G, current.B, value));
                },
                min: 0,
                max: byte.MaxValue,
                fieldId: fieldId is null ? null : $"{fieldId}.A"
            );
        }
    }
}

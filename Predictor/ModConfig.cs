using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace Predictor.Framework
{
    internal class ProperyChangedEventArgs : EventArgs
    {
        public string Name { get; private set; }
        public object Value { get; private set; }

        public ProperyChangedEventArgs(string name, object value) 
        { 
            Name = name;
            Value = value;
        }
    }

    internal class ModConfig
    {
        public static readonly int[] MenyTypeOptions = { 0, 1 };
        public event EventHandler<ProperyChangedEventArgs>? PropertyChanged;

        // General
        public bool Enabled = true;
        public bool LazyUpdates = true;
        public SButton ToggleKey = SButton.Q;
        public bool EnableObjectOutlines = false;

        // Menu
        public int MenuOffsetX = 4;
        public int MenuOffsetY = 4;
        public int MenuType = 0;
        public float MenuScale = 1f;
        public float MenuAlpha = 0.8f;

        public Point GetMenuOffset()
        {
            return new Point(MenuOffsetX, MenuOffsetY);
        }

        public void SetProperty<T>(ref T property, T value, string name) where T : notnull
        {
            if (!property.Equals(value))
            {
                property = value;
                PropertyChanged?.Invoke(this, new ProperyChangedEventArgs(name, value));
            }
        }
    }
}

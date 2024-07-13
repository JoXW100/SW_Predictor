namespace PredictorMineablePatch
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
        public event EventHandler<ProperyChangedEventArgs>? PropertyChanged;

        public bool Enabled = true;
        public bool RequireTool = false;
        public bool ShowItems = true;
        public bool ShowOutlines = true;
        public bool ShowLadders = true;
        public bool MonstersHideLadders = false;

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

namespace PredictorFishingPatch
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
        public bool ShowFish = true;
        public bool ShowBait = true;
        public bool ShowTackle = true;
        public bool ShowFishChances = true;
        public bool ShowTrashChances = false;
        public bool ShowSecretNoteChances = false;
        public bool ShowUncaughtFish = true;
        public bool ShowLessFishInfo = false;
        public bool EstimateFishChances = true;
        public int  NumFishCatches = 1000;

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

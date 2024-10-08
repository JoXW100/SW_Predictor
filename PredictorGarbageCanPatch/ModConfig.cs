﻿using Microsoft.Xna.Framework;
using xTile.Tiles;

namespace PredictorGarbageCanPatch
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
        public bool ShowItems = true;
        public bool ShowOutlines = true;
        public bool ShowNearbyNPCWarning = true;

        public Color GarbageCanOkColor = Color.Green;
        public Color GarbageCanWarnColor = Color.Red;

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

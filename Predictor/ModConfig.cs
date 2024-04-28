using StardewModdingAPI;

namespace Predictor.Framework
{
    public class ProperyChangedEventArgs : EventArgs
    {
        public string Name { get; private set; }
        public object Value { get; private set; }

        public ProperyChangedEventArgs(string name, object value) 
        { 
            Name = name;
            Value = value;
        }
    }

    public class ModConfig
    {
        public static readonly int[] MenyTypeOptions = { 0, 1 };
        public event EventHandler<ProperyChangedEventArgs>? PropertyChanged;

        // General
        public bool Enabled = true;
        public bool LazyUpdates = true;
        public SButton ToggleKey = SButton.Q;

        // Items
        public bool EnableHarvestableBushItems = true;
        public bool EnableBreakableContainerItems = true;
        public bool EnableDigSpotItems = true;
        public bool EnableGeodeItems = true;
        public bool EnableGarbageCanItems = true;
        public bool EnablePaningSpotItems = true;
        public bool EnableMineableObjectItems = true;
        public bool EnableHarvestableTreeItems = true;
        public bool EnableTillableItems = true;

        // Outlines
        public bool EnableHarvestableBushOutlines = false;
        public bool EnableBreakableContainerOutlines = false;
        public bool EnableDigSpotOutlines = false;
        public bool EnableSpawnedOutlines = false;
        public bool EnableGarbageCanOutlines = true;
        public bool EnableGarbageCanWarningOutlines = true;
        public bool EnablePaningSpotOutlines = false;
        public bool EnableMineableObjectOutlines = false;
        public bool EnableHarvestableTreeOutlines = false;
        public bool EnableTillableOutlines = true;
        public bool EnableLadderOutlines = true;
        public bool EnableMonstersHideLadders = false;
        public bool EnableObjectOutlines = false;

        // Trackers
        public bool TrackHarvestableBushes = true;
        public bool TrackDigSpots = true;
        public bool TrackSpawned = true;
        public bool TrackPanningSpots = true;
        public int  TrackerMenuMaxItemCount = 5;
        public int  TrackerMenuMaxItemDistance = 0;
        public bool TrackerMenuShowLess = false;

        // Fishing
        public bool ShowFishInMinigame = true;
        public bool ShowBait = true;
        public bool ShowTackle = true;
        public bool ShowFishChances = true;
        public bool ShowTrashChances = false;
        public bool ShowSecretNoteChances = false;
        public bool ShowUncaughtFish = true;
        public bool EstimateFishChances = true;
        public int  NumFishCatches = 1000;
        public bool ShowLessFishInfo = false;

        // Menu
        public int MenuOffsetX = 0;
        public int MenuOffsetY = 0;
        public int MenuType = 0;
        public float MenuScale = 1f;
        public float MenuAlpha = 0.8f;

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

using Force.DeepCloner;
using StardewValley;

namespace Predictor.Framework
{
    public enum PredictionProperty
    {
        SpawnLadder,
        FishChanceData,
        TrashChance,
        Affected,
        Exhausted,
        ChanceBase
    }

    public class PredictionContext
    {
        public readonly List<PredictionItem> Items;
        public readonly Dictionary<PredictionProperty, object> Properties;
        public readonly Random Random;

        public PredictionContext(Random? gameRandom = null) 
        { 
            Items = new List<PredictionItem>();
            Properties = new Dictionary<PredictionProperty, object>();
            Random = gameRandom ?? Game1.random.DeepClone();
        }

        public PredictionItem? AddItemIfNotNull(string itemId, int stack = 1, int quality = 0)
        {
            return AddItemIfNotNull(PredictionItem.Create(itemId, stack, quality));
        }

        public PredictionItem? AddItemIfNotNull(PredictionItem? item)
        {
            if (item != null)
            {
                Items.Add(item);
            }
            return item;
        }

        public PredictionItem? AddItemIfNotNull(Item? item)
        {
            PredictionItem? predition = null;
            if (item != null)
            {
                predition = new PredictionItem(item);
                Items.Add(predition);
            }
            return predition;
        }

        public void Join(PredictionContext other)
        {
            this.Items.AddRange(other.Items);
            foreach (var pair in other.Properties)
            {
                this.Properties.TryAdd(pair.Key, pair.Value);
            }
        }

        public T GetPropertyValue<T>(PredictionProperty key)
        {
            if (this.Properties.TryGetValue(key, out var value) && value is T result)
            {
                return result;
            }
            else
            {
                throw new ArgumentNullException(nameof(key));
            }
        }
    }
}

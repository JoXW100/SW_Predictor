namespace Predictor.Framework
{
    public class FishChanceData
    {
        public float Chance { get; set; }
        public bool BobberInArea { get; set; }
        public bool IsTrash { get; set; }

        public FishChanceData(float chance, bool bobberInArea, bool isTrash)
        {
            Chance = chance;
            BobberInArea = bobberInArea;
            IsTrash = isTrash;
        }
    }
}

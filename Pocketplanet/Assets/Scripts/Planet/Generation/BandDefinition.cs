using System;

namespace PocketPlanet.Planet
{
    [Serializable]
    public class BandDefinition
    {
        public string Name;
        public float Min;
        public float Max;

        public BandDefinition(string name, float min, float max)
        {
            Name = name;
            Min = min;
            Max = max;
        }

        public bool Contains(float value) => value >= Min && value < Max;
    }
}

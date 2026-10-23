using System.Collections.Generic;

namespace PocketPlanet.Planet.Tests
{
    public class RecordingEffect : IWorldMoodEffect
    {
        public readonly List<float> Applied = new List<float>();
        public void Apply(float mood) => Applied.Add(mood);
    }
}

using UnityEngine;

namespace PocketPlanet.Planet
{
    public class ManualMoodSource : MonoBehaviour, IWorldMoodSource
    {
        [SerializeField, Range(-1f, 1f)] float mood;

        public float TargetMood => mood;

        public void SetMood(float value) => mood = Mathf.Clamp(value, -1f, 1f);
    }
}

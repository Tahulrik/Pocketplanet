using UnityEngine;

namespace PocketPlanet.Planet
{
    public class PlanetPaletteMoodEffect : MonoBehaviour, IWorldMoodEffect
    {
        [SerializeField] PlanetView view;

        public void Apply(float mood)
        {
            if (view != null)
                view.SetMood(mood);
        }
    }
}

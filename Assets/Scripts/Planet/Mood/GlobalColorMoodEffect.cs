using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PocketPlanet.Planet
{
    // Owns a runtime-only volume profile so previewing mood in the editor never modifies an asset.
    [ExecuteAlways]
    public class GlobalColorMoodEffect : MonoBehaviour, IWorldMoodEffect
    {
        [SerializeField] Volume volume;

        VolumeProfile profile;
        ColorAdjustments colorAdjustments;

        public void Apply(float mood)
        {
            if (!EnsureProfile())
                return;
            var grading = MoodGrading.Evaluate(mood);
            colorAdjustments.saturation.Override(grading.Saturation);
            colorAdjustments.colorFilter.Override(grading.Filter);
        }

        bool EnsureProfile()
        {
            if (volume == null)
                return false;
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                profile.hideFlags = HideFlags.DontSave;
                colorAdjustments = profile.Add<ColorAdjustments>(true);
            }
            volume.sharedProfile = profile;
            return true;
        }

        void OnDestroy()
        {
            if (profile != null)
                DestroyImmediate(profile);
        }
    }
}

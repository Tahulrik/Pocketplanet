using System;
using System.Collections.Generic;
using UnityEngine;

namespace PocketPlanet.Planet
{
    [ExecuteAlways]
    public class WorldMoodController : MonoBehaviour
    {
        const float EditModeStepSeconds = 10f;

        [SerializeField] MonoBehaviour source;
        [SerializeField] MonoBehaviour[] effects = new MonoBehaviour[0];
        [SerializeField, Min(0.01f)] float unitsPerSecond = 0.25f;

        readonly List<IWorldMoodEffect> registered = new List<IWorldMoodEffect>();

        public event Action<float> MoodChanged;

        public float CurrentMood { get; private set; }

        public void Configure(MonoBehaviour moodSource, float rate)
        {
            source = moodSource;
            unitsPerSecond = rate;
            OnValidate();
        }

        public void Register(IWorldMoodEffect effect)
        {
            registered.Add(effect);
            effect.Apply(CurrentMood);
        }

        public void Unregister(IWorldMoodEffect effect) => registered.Remove(effect);

        public void Tick(float deltaTime)
        {
            if (!(source is IWorldMoodSource moodSource))
                return;
            var next = MoodSmoother.Step(CurrentMood, moodSource.TargetMood, deltaTime, unitsPerSecond);
            if (Mathf.Approximately(next, CurrentMood))
                return;
            CurrentMood = next;
            ApplyEffects();
            MoodChanged?.Invoke(CurrentMood);
        }

        void OnEnable() => ApplyEffects();

        void Update() => Tick(Application.isPlaying ? Time.deltaTime : EditModeStepSeconds);

        void ApplyEffects()
        {
            foreach (var effect in effects)
                if (effect is IWorldMoodEffect moodEffect)
                    moodEffect.Apply(CurrentMood);
            foreach (var effect in registered)
                effect.Apply(CurrentMood);
        }

        void OnValidate()
        {
            if (source != null && !(source is IWorldMoodSource))
                Debug.LogError($"{source.GetType().Name} does not implement {nameof(IWorldMoodSource)}.", this);
            foreach (var effect in effects)
                if (effect != null && !(effect is IWorldMoodEffect))
                    Debug.LogError($"{effect.GetType().Name} does not implement {nameof(IWorldMoodEffect)}.", this);
        }
    }
}

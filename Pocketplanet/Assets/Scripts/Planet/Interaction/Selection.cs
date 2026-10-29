using System;
using UnityEngine;

namespace PocketPlanet.Planet
{
    // The world object the player has picked. It outlives any menu shown for it.
    public sealed class Selection
    {
        readonly Func<GameObject, bool> isValid;

        public Selection(Func<GameObject, bool> isValid = null) => this.isValid = isValid ?? (_ => true);

        public event Action Changed;
        public event Action Reselected;

        public GameObject Current { get; private set; }
        public bool HasTarget => Current != null;

        public void Select(GameObject target)
        {
            if (target == null)
            {
                Clear();
                return;
            }
            if (target == Current)
            {
                Reselected?.Invoke();
                return;
            }
            Current = target;
            Changed?.Invoke();
        }

        public void Clear()
        {
            // ReferenceEquals, because a destroyed target compares equal to null but still has to be released.
            if (ReferenceEquals(Current, null))
                return;
            Current = null;
            Changed?.Invoke();
        }

        public void Refresh()
        {
            if (!ReferenceEquals(Current, null) && !IsStillValid(Current))
                Clear();
        }

        bool IsStillValid(GameObject target) => target != null && target.activeInHierarchy && isValid(target);
    }
}

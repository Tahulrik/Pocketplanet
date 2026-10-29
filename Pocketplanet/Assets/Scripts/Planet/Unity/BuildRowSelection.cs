using System;
using UnityEngine;

namespace PocketPlanet.Planet
{
    public class BuildRowSelection : MonoBehaviour
    {
        [SerializeField] SurfaceRow current = SurfaceRow.Front;

        public static BuildRowSelection Active { get; private set; }

        public static event Action<SurfaceRow> ActiveRowChanged;

        public SurfaceRow Current => current;

        public static SurfaceRow? ActiveRow => Active != null ? Active.Current : (SurfaceRow?)null;

        public static bool IsInteractive(SurfaceRow row) => !ActiveRow.HasValue || row == ActiveRow.Value;

        public void Select(SurfaceRow row)
        {
            if (row == current)
                return;
            current = row;
            ActiveRowChanged?.Invoke(row);
        }

        // Pushing the belt back sends every row one place deeper; the row at the very back wraps round to the front.
        public void PushBack() => Advance(-1);

        public void PushForward() => Advance(1);

        void Advance(int direction)
        {
            var rows = SurfaceRows.FrontToBack;
            Select(rows[(DepthIndex() + direction + rows.Count) % rows.Count]);
        }

        int DepthIndex() => SurfaceRows.DepthIndex(current);

        void OnEnable()
        {
            Active = this;
            ActiveRowChanged?.Invoke(current);
        }

        void OnDisable()
        {
            if (Active != this)
                return;
            Active = null;
            ActiveRowChanged?.Invoke(SurfaceRow.Front);
        }
    }
}

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

        public static bool IsInteractive(SurfaceRow row) => !ActiveRow.HasValue || !SurfaceRows.IsInFrontOf(row, ActiveRow.Value);

        public void Select(SurfaceRow row)
        {
            if (row == current)
                return;
            current = row;
            ActiveRowChanged?.Invoke(row);
        }

        public void StepBack() => Step(1);

        public void StepForward() => Step(-1);

        public void CycleBackward()
        {
            var rows = SurfaceRows.FrontToBack;
            Select(rows[(DepthIndex() + 1) % rows.Count]);
        }

        void Step(int direction)
        {
            var rows = SurfaceRows.FrontToBack;
            Select(rows[Mathf.Clamp(DepthIndex() + direction, 0, rows.Count - 1)]);
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
            ActiveRowChanged?.Invoke(SurfaceRow.Back);
        }
    }
}

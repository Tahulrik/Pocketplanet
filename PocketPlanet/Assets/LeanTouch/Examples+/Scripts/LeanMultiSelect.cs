using UnityEngine;
using System.Collections.Generic;

namespace Lean.Touch
{
	// This script allows you to select multiple LeanSelectable components
	public class LeanMultiSelect : MonoBehaviour
	{
		public enum SelectType
		{
			Raycast3D,
			Overlap2D
		}

		public enum SearchType
		{
			GetComponent,
			GetComponentInParent,
			GetComponentInChildren
		}
		
		public SelectType SelectUsing;

		[Tooltip("This stores the layers we want the raycast/overlap to hit (make sure this GameObject's layer is included!)")]
		public LayerMask LayerMask = Physics.DefaultRaycastLayers;

		[Tooltip("How should the selected GameObject be searched for the LeanSelectable component?")]
		public SearchType Search;

		[Tooltip("The currently selected LeanSelectables")]
		public List<LeanSelectable> CurrentSelectables;
		
		[Tooltip("If you select an already selected selectable, should it get deselected?")]
		public bool ToggleSelection;

		[Tooltip("Automatically deselect the CurrentSelectables if Select gets called with null?")]
		public bool AutoDeselect;
		
		// NOTE: This must be called from somewhere
		public void Select(LeanFinger finger)
		{
			// Stores the component we hit (Collider or Collider2D)
			var component = default(Component);

			switch (SelectUsing)
			{
				case SelectType.Raycast3D:
				{
					// Get ray for finger
					var ray = finger.GetRay();

					// Stores the raycast hit info
					var hit = default(RaycastHit);
					
					// Was this finger pressed down on a collider?
					if (Physics.Raycast(ray, out hit, float.PositiveInfinity, LayerMask) == true)
					{
						component = hit.collider;
					}
				}
				break;
				
				case SelectType.Overlap2D:
				{
					// Find the position under the current finger
					var point = finger.GetWorldPosition(1.0f);

					// Find the collider at this position
					component = Physics2D.OverlapPoint(point, LayerMask);
				}
				break;
			}

			// Select the component
			Select(finger, component);
		}

		public void Select(LeanFinger finger, Component component)
		{
			// Stores the selectable we will search for
			var selectable = default(LeanSelectable);

			// Was a collider found?
			if (component != null)
			{
				switch (Search)
				{
					case SearchType.GetComponent:           selectable = component.GetComponent          <LeanSelectable>(); break;
					case SearchType.GetComponentInParent:   selectable = component.GetComponentInParent  <LeanSelectable>(); break;
					case SearchType.GetComponentInChildren: selectable = component.GetComponentInChildren<LeanSelectable>(); break;
				}
			}

			// Select the selectable
			Select(finger, selectable);
		}

		public void Select(LeanFinger finger, LeanSelectable selectable)
		{
			// Something was selected?
			if (selectable != null)
			{
				if (CurrentSelectables == null)
				{
					CurrentSelectables = new List<LeanSelectable>();
				}

				// Loop through all current selectables
				for (var i = CurrentSelectables.Count - 1; i >= 0; i--)
				{
					var currentSelectable = CurrentSelectables[i];

					if (currentSelectable != null)
					{
						// Already selected?
						if (currentSelectable == selectable)
						{
							if (ToggleSelection == true)
							{
								DeselectAndRemove(currentSelectable);
							}

							return;
						}
					}
					else
					{
						CurrentSelectables.RemoveAt(i);
					}
				}

				// Not selected yet, so select it
				CurrentSelectables.Add(selectable);

				selectable.Select(finger);
			}
			// Nothing was selected?
			else
			{
				// Deselect all?
				if (AutoDeselect == true)
				{
					DeselectAll();
				}
			}
		}
		
		[ContextMenu("Deselect All")]
		public void DeselectAll()
		{
			// Loop through all current selectables and deselect if not null
			if (CurrentSelectables != null)
			{
				for (var i = CurrentSelectables.Count - 1; i >= 0; i--)
				{
					var currentSelectable = CurrentSelectables[i];

					if (currentSelectable != null)
					{
						currentSelectable.Deselect();
					}
				}

				// Clear
				CurrentSelectables.Clear();
			}
		}
		
		// Deselect the specified selectable, if it exists
		public void Deselect(LeanSelectable selectable)
		{
			// Loop through all current selectables
			if (CurrentSelectables != null)
			{
				for (var i = CurrentSelectables.Count - 1; i >= 0; i--)
				{
					var currentSelectable = CurrentSelectables[i];

					if (currentSelectable != null)
					{
						// Match?
						if (currentSelectable == selectable)
						{
							DeselectAndRemove(currentSelectable); return;
						}
					}
					else
					{
						CurrentSelectables.RemoveAt(i);
					}
				}
			}
		}

		// Deselect and remove without null checks
		private void DeselectAndRemove(LeanSelectable selectable)
		{
			selectable.Deselect();
			
			CurrentSelectables.Remove(selectable);
		}
	}
}
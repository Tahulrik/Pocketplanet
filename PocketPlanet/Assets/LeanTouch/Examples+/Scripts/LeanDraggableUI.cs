using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Lean.Touch
{
	// This component allows you to drag any UI element
	[RequireComponent(typeof(RectTransform))]
	public class LeanDraggableUI : Selectable, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		[Tooltip("By default this GameObject will be moved when dragging, but you can override that here")]
		public RectTransform Target;

		// Is this element currently being dragged?
		protected bool dragging;
		
		// The cached rect transform attached to this GameObject
		private RectTransform rectTransform;

		// The anchored position of the target rect while it's being dragged
		private Vector2 anchoredPosition;
		
		public RectTransform TargetTransform
		{
			get
			{
				if (Target != null)
				{
					return Target;
				}

				if (rectTransform == null)
				{
					rectTransform = GetComponent<RectTransform>();
				}

				return rectTransform;
			}
		}
		
		public void OnBeginDrag(PointerEventData eventData)
		{
			// Only allow dragging if certain conditions are met
			if (MayDrag(eventData) == true)
			{
				var vector = default(Vector2);
				var target = TargetTransform;

				// Is this pointer inside this rect transform?
				if (RectTransformUtility.ScreenPointToLocalPointInRectangle(target, eventData.position, eventData.pressEventCamera, out vector) == true)
				{
					dragging         = true;
					anchoredPosition = TargetTransform.anchoredPosition;
				}
			}
		}

		public virtual void OnDrag(PointerEventData eventData)
		{
			// Only drag if OnBeginDrag was successful
			if (dragging == true)
			{
				// Only allow dragging if certain conditions are met
				if (MayDrag(eventData) == true)
				{
					var oldVector = default(Vector2);
					var target    = TargetTransform;

					// Get the previous pointer position relative to this rect transform
					if (RectTransformUtility.ScreenPointToLocalPointInRectangle(target, eventData.position - eventData.delta, eventData.pressEventCamera, out oldVector) == true)
					{
						var newVector = default(Vector2);

						// Get the current pointer position relative to this rect transform
						if (RectTransformUtility.ScreenPointToLocalPointInRectangle(target, eventData.position, eventData.pressEventCamera, out newVector) == true)
						{
							// Offset the anchored position by the difference
							anchoredPosition += newVector - oldVector;

							// Apply new anchored position
							target.anchoredPosition = anchoredPosition;
						}
					}
				}
			}
		}
		
		public void OnEndDrag(PointerEventData eventData)
		{
			dragging = false;
		}
		
		private bool MayDrag(PointerEventData eventData)
		{
			return this.IsActive() && this.IsInteractable();// && eventData.button == PointerEventData.InputButton.Left;
		}
	}
}
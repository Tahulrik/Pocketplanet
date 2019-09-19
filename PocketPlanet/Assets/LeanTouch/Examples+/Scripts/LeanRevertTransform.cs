using UnityEngine;

namespace Lean.Touch
{
	// This script will revert the transform of the current GameObject when the target selectable isn't selected
	public class LeanRevertTransform : MonoBehaviour
	{
		[Tooltip("Does translation require an object to be selected?")]
		public LeanSelectable RequiredSelectable;

		[Tooltip("How quickly this object moves to its original transform")]
		public float Dampening = 10.0f;

		[SerializeField]
		[HideInInspector]
		private Vector3 originalPosition;

		[SerializeField]
		[HideInInspector]
		private Quaternion originalRotation;

		[SerializeField]
		[HideInInspector]
		private Vector3 originalScale;

#if UNITY_EDITOR
		protected virtual void Reset()
		{
			if (RequiredSelectable == null)
			{
				RequiredSelectable = GetComponent<LeanSelectable>();
			}
		}
#endif

		protected virtual void Awake()
		{
			originalPosition = transform.localPosition;
			originalRotation = transform.localRotation;
			originalScale    = transform.localScale;
		}

		protected virtual void Update()
		{
			if (RequiredSelectable != null && RequiredSelectable.IsSelected == false)
			{
				// The framerate independent damping factor
				var factor = Mathf.Exp(-Dampening * Time.deltaTime);

				transform.localPosition =    Vector3. Lerp(originalPosition, transform.localPosition, factor);
				transform.localRotation = Quaternion.Slerp(originalRotation, transform.localRotation, factor);
				transform.localScale    =    Vector3. Lerp(originalScale   , transform.localScale   , factor);
			}
		}
	}
}
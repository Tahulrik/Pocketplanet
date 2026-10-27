using UnityEngine;
using UnityEngine.EventSystems;

public class EnsureEventSystem : MonoBehaviour
{
	void Awake()
	{
		if (FindAnyObjectByType<EventSystem>() != null)
			return;
		var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
		eventSystem.transform.SetParent(transform, false);
	}
}

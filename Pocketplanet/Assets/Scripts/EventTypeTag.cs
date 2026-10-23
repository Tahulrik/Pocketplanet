using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EventCategory
{
    Mafia,
    Robber,
    Meteor,
	MeteorShower,
    Blockzilla,
    BlockMan,
    Building,
	Missile
}

// Was "EventType", renamed so it no longer clashes with UnityEngine.EventType.
// The .meta GUID is unchanged, so prefabs that use this component keep their data.
public class EventTypeTag : MonoBehaviour {

	public EventCategory _EventType;
}

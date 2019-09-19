using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InteractionTypes
{
	public enum InteractionSetting
	{
		Building,
		Meteor,
		Camera
	}

	public static class InteractionType
	{
		public delegate void SetInteractionListeners();

		public static event SetInteractionListeners MeteorInteraction;
		public static event SetInteractionListeners BuildingInteraction;
		public static event SetInteractionListeners CameraInteraction;


		public static InteractionSetting currentInteraction; 

		//currently not used
		static InteractionSetting previousMode;

		public static void ChangeModeToMeteor()
		{
			previousMode = currentInteraction;

			currentInteraction = InteractionSetting.Meteor;

			if (MeteorInteraction != null) {
				MeteorInteraction ();
			}
		}

		public static void ChangeModeToBuilding()
		{
			previousMode = currentInteraction;

			currentInteraction = InteractionSetting.Building;

			if (BuildingInteraction != null) {
				BuildingInteraction ();
			}
		}

		public static void ChangeModeToCamera()
		{
			previousMode = currentInteraction;

			currentInteraction = InteractionSetting.Camera;

			if (CameraInteraction != null) {
				CameraInteraction ();
			}
		}

		public static InteractionSetting GetCurrentMode()
		{
			return currentInteraction;
		}
	}
}

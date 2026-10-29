// Safe access to the game's camera controller for world interaction, which may run before the game is set up.
public static class CameraRig
{
	public static CameraController Controller => GameHandler.Instance != null ? GameHandler.Instance.CameraController : null;

	public static bool IsInCityView()
	{
		var controller = Controller;
		return controller != null && controller.IsZoomedIn;
	}
}

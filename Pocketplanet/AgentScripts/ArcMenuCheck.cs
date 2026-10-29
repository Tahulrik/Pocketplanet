using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Buildings;
using PocketPlanet.Planet;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// Play Mode checks for the arc menu, selection highlight and off-screen pointer.
// Run each entry with run_script while playing and capture the Game view between them.
public static class ArcMenuCheck
{
    const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

    static WorldTapSelector Selector => Object.FindAnyObjectByType<WorldTapSelector>();
    static CameraController Cam => GameHandler.Instance.CameraController;
    static Transform Root(string canvas, string child) => Selector.transform.Find(canvas)?.Find(child);
    static Transform MenuRoot => Root("ArcMenuCanvas", "ArcMenu");
    static bool MenuShown => MenuRoot != null && MenuRoot.gameObject.activeInHierarchy;
    static bool HighlightShown => Root("SelectionHighlightCanvas", "Highlight")?.gameObject.activeInHierarchy == true;
    static Transform Pointer => Root("OffscreenPointerCanvas", "Pointer");

    public static async Task<string> SelectBuilding()
    {
        if (!Application.isPlaying)
            return "Not playing";
        var building = PickBuilding() ?? BringFirstBuildingRowForward();
        if (building == null)
            return "No standing interactive building found";
        Cam.FocusOn(building.transform);
        await Awaitable.WaitForSecondsAsync(2f);
        Selector.Selection.Select(building);
        await Awaitable.NextFrameAsync();
        await Awaitable.NextFrameAsync();
        return $"Selected {building.name}. zoomedIn={Cam.IsZoomedIn} {MenuReport()}";
    }

    public static string Scene() =>
        $"active={UnityEngine.SceneManagement.SceneManager.GetActiveScene().path} playing={Application.isPlaying}";

    public static string PickName()
    {
        var all = Object.FindObjectsByType<Building>().Select(b => b.gameObject).ToList();
        var both = all.Where(b => WorldPicker.IsPickable(b) && RowDepthStyle.IsInteractiveObject(b)).ToList();
        return $"all={all.Count} both={both.Count} names=[{string.Join(", ", all.Select(b => $"{b.name}:{WorldPicker.IsPickable(b)}/{RowDepthStyle.IsInteractiveObject(b)}"))}] pick={PickBuilding()?.name ?? "none"}";
    }

    public static async Task<string> PickNameAsync()
    {
        await Task.Yield();
        return $"async pick={PickBuilding()?.name ?? "none"} thread={System.Threading.Thread.CurrentThread.ManagedThreadId} playing={Application.isPlaying}";
    }

    public static string Diagnose()
    {
        var buildings = Object.FindObjectsByType<Building>();
        var named = Object.FindObjectsByType<Transform>().Count(t => t.name.StartsWith("Building_"));
        return $"Building components={buildings.Length} pickable={buildings.Count(b => WorldPicker.IsPickable(b.gameObject))} " +
               $"interactive={buildings.Count(b => RowDepthStyle.IsInteractiveObject(b.gameObject))} objectsNamedBuilding_={named} " +
               $"selector={(Selector != null)} zoomedIn={CameraRig.IsInCityView()}";
    }

    public static async Task<string> ZoomLevels()
    {
        var log = new StringBuilder();
        foreach (var level in new[] { 0.6f, 0.85f, 1f, 0.72f })
        {
            SetZoom(level);
            await Awaitable.NextFrameAsync();
            await Awaitable.NextFrameAsync();
            log.Append($"zoom {level}: shown={MenuShown} radius={MeasuredRadius():0.0}; ");
        }
        return log.ToString();
    }

    public static async Task<string> PinchState()
    {
        var hidden = 0;
        typeof(CameraController).GetField("CurrentCameraState").SetValue(Cam, CameraState.Zooming);
        for (int i = 0; i < 30; i++)
        {
            SetZoom(Mathf.Lerp(0.72f, 0.9f, i / 29f));
            await Awaitable.NextFrameAsync();
            if (!MenuShown)
                hidden++;
        }
        SetZoom(0.72f);
        Cam.GetType().GetMethod("SetCameraZoomState", Private).Invoke(Cam, null);
        await Awaitable.NextFrameAsync();
        return $"Frames hidden while pinching in city view: {hidden}/30. shown now={MenuShown}";
    }

    public static async Task<string> ZoomOut()
    {
        SetZoom(0.1f);
        Cam.GetType().GetMethod("SetCameraZoomState", Private).Invoke(Cam, null);
        await Awaitable.NextFrameAsync();
        await Awaitable.NextFrameAsync();
        return $"Planet view: zoomedIn={Cam.IsZoomedIn} menu={MenuShown} highlight={HighlightShown} selected={Selector.Selection.Current?.name}";
    }

    public static async Task<string> ZoomBackIn()
    {
        SetZoom(0.72f);
        Cam.GetType().GetMethod("SetCameraZoomState", Private).Invoke(Cam, null);
        Cam.FocusOn(Selector.Selection.Current.transform);
        await Awaitable.WaitForSecondsAsync(2f);
        return $"City view again: {MenuReport()} highlight={HighlightShown}";
    }

    public static async Task<string> TurnAway()
    {
        Cam.StopFollowing();
        Cam.transform.Rotate(0f, 0f, 90f);
        await Awaitable.NextFrameAsync();
        await Awaitable.NextFrameAsync();
        var pointer = Pointer;
        var shown = pointer != null && pointer.gameObject.activeInHierarchy;
        return $"Turned 90°: pointer={shown} at {(shown ? ((RectTransform)pointer).anchoredPosition.ToString() : "-")} highlight={HighlightShown} menu={MenuShown}";
    }

    public static async Task<string> TapPointer()
    {
        Pointer.GetComponent<Button>().onClick.Invoke();
        await Awaitable.WaitForSecondsAsync(2.2f);
        var target = Selector.Selection.Current;
        Vector2 viewport = Camera.main.WorldToViewportPoint(WorldPicker.VisibleBounds(target).center);
        return $"After tap: pointer={Pointer.gameObject.activeInHierarchy} target viewport={viewport} menu={MenuShown}";
    }

    public static async Task<string> GuiTapGuard()
    {
        var button = MenuRoot.GetComponentsInChildren<Button>().FirstOrDefault();
        if (button == null)
            return "No button to test";
        var hits = new List<RaycastResult>();
        EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = button.transform.position }, hits);
        await Awaitable.NextFrameAsync();
        return $"UI raycast at '{button.name}' hits {hits.Count} ({string.Join(", ", hits.Select(h => h.gameObject.name))}) so Lean reports it as over GUI";
    }

    public static async Task<string> Paging()
    {
        var catalog = Selector.Actions;
        var providers = (List<IContextActionProvider>)typeof(ContextActionCatalog).GetField("providers", Private).GetValue(catalog);
        var extra = new ManyActions(Selector.Selection.Current);
        providers.Insert(0, extra);
        try
        {
            Selector.Selection.Clear();
            await Awaitable.NextFrameAsync();
            Selector.Selection.Select(extra.Target);
            await Awaitable.NextFrameAsync();
            await Awaitable.NextFrameAsync();
            var first = Labels();
            Press(">");
            await Awaitable.NextFrameAsync();
            await Awaitable.NextFrameAsync();
            var second = Labels();
            return $"Page 1: {first}\nPage 2: {second}";
        }
        finally
        {
            providers.Remove(extra);
        }
    }

    public static async Task<string> RestoreMenu()
    {
        var target = Selector.Selection.Current;
        Selector.Selection.Clear();
        await Awaitable.NextFrameAsync();
        Selector.Selection.Select(target);
        await Awaitable.NextFrameAsync();
        await Awaitable.NextFrameAsync();
        return MenuReport();
    }

    public static async Task<string> Demolish()
    {
        var target = Selector.Selection.Current;
        Press("Demolish");
        await Awaitable.NextFrameAsync();
        await Awaitable.NextFrameAsync();
        return $"After Demolish on {target.name}: selected={Selector.Selection.HasTarget} menu={MenuShown} highlight={HighlightShown}";
    }

    public static async Task<string> FollowBlockman()
    {
        if (Object.FindAnyObjectByType<Citizen_Blockman>() == null)
        {
            EventHandler.TriggerBlockman(EventCategory.BlockMan, Random.insideUnitCircle * 2, GameHandler.Instance.Blockman, false);
            await Awaitable.WaitForSecondsAsync(3f);
        }
        var blockman = Object.FindAnyObjectByType<Citizen_Blockman>();
        if (blockman == null)
            return "No Blockman in the scene";
        Cam.FocusOn(blockman.transform);
        await Awaitable.WaitForSecondsAsync(2f);
        Selector.Selection.Select(blockman.gameObject);
        await Awaitable.NextFrameAsync();
        await Awaitable.NextFrameAsync();
        var ring = MenuRoot.Find("Arc").GetComponent<Image>();
        var before = $"Blockman airborne={blockman.GetComponent<AirborneAgent>() != null} ringFill={ring.fillAmount:0.00} {MenuReport()}";
        Press("Follow");
        await Awaitable.WaitForSecondsAsync(0.5f);
        var afterFollow = $"after Follow: menu={MenuShown} highlight={HighlightShown} selected={Selector.Selection.Current?.name}";
        Selector.Selection.Select(blockman.gameObject);
        await Awaitable.NextFrameAsync();
        await Awaitable.NextFrameAsync();
        return $"{before}\n{afterFollow}\nafter reselect: menu={MenuShown}";
    }

    public static string ZoomOutWhileFollowing()
    {
        SetZoom(0.1f);
        Cam.GetType().GetMethod("SetCameraZoomState", Private).Invoke(Cam, null);
        return $"zoomed out: highlight={HighlightShown} selected={Selector.Selection.Current?.name}";
    }

    static GameObject PickBuilding() => Object.FindObjectsByType<Building>()
        .Select(b => b.gameObject)
        .Where(b => WorldPicker.IsPickable(b) && RowDepthStyle.IsInteractiveObject(b) && b.GetComponent<Building_Normal_Residence>() != null)
        .Concat(Object.FindObjectsByType<Building>().Select(b => b.gameObject)
            .Where(b => WorldPicker.IsPickable(b) && RowDepthStyle.IsInteractiveObject(b)))
        .FirstOrDefault();

    static GameObject BringFirstBuildingRowForward()
    {
        var style = Object.FindObjectsByType<Building>()
            .Where(b => WorldPicker.IsPickable(b.gameObject))
            .Select(b => b.GetComponent<RowDepthStyle>())
            .FirstOrDefault(s => s != null);
        if (style == null || BuildRowSelection.Active == null)
            return null;
        BuildRowSelection.Active.Select(style.Row);
        return PickBuilding();
    }

    static void SetZoom(float level) => Cam.GetType().GetMethod("SetZoomLevel", Private).Invoke(Cam, new object[] { level });

    static string MenuReport()
    {
        if (!MenuShown)
            return "menu=hidden";
        return $"menu=shown radius={MeasuredRadius():0.0} buttons=[{Labels()}]";
    }

    static string Labels() => string.Join(", ", MenuRoot.GetComponentsInChildren<Button>()
        .Select(b => $"{b.name}@{AngleOf(b.transform):0}°"));

    static float AngleOf(Transform button)
    {
        var local = (Vector2)MenuRoot.InverseTransformPoint(button.position);
        return Vector2.SignedAngle(local, Vector2.up);
    }

    // Distance from the menu centre to its first button, in canvas units.
    static float MeasuredRadius()
    {
        var button = MenuRoot.GetComponentsInChildren<Button>().FirstOrDefault();
        if (button == null)
            return -1f;
        var scale = MenuRoot.GetComponentInParent<Canvas>().scaleFactor;
        return Vector2.Distance(MenuRoot.position, button.transform.position) / scale;
    }

    static void Press(string label) =>
        MenuRoot.GetComponentsInChildren<Button>().First(b => b.name == label).onClick.Invoke();

    sealed class ManyActions : IContextActionProvider
    {
        public ManyActions(GameObject target) => Target = target;

        public GameObject Target { get; }

        public bool Handles(GameObject target) => target == Target;

        public IEnumerable<ContextAction> ActionsFor(GameObject target) =>
            Enumerable.Range(1, 7).Select(i => new ContextAction($"A{i}", _ => { }));
    }
}

# Arc Menu and Selection Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the scaling half-circle context menu with a fixed-size, paged arc menu tied to a selection that outlives the menu. The selection gets a highlight at every zoom level and an off-screen arrow that jumps the camera to it.

**Architecture:**
- Pure logic lives in the `PocketPlanet.Planet` assembly so edit-mode tests can reach it: `ArcLayout` (slot maths) and `Selection` (state and events).
- Three MonoBehaviour views read `Selection` and the camera, and never refer to each other:
  - `ArcMenuView`, which replaces `RadialContextMenu`
  - `SelectionHighlight`
  - `OffscreenPointer`
- `WorldTapSelector` owns the `Selection` and turns taps into select and clear calls.

**Tech Stack:** Unity 6000.6.5f1, C#, uGUI on a screen-space overlay (`OverlayFactory`), Lean Touch, NUnit edit-mode tests, and the `unity` pipeline CLI.

**Spec:** `docs/superpowers/specs/2026-10-28-arc-menu-selection-design.md`

## Global Constraints

- Drive the open editor with the `unity` CLI (add `C:\Users\Ulrik\AppData\Local\Unity\bin` to `PATH`). Never ask the user to close Unity.
  - Compile: `unity command recompile`, then poll `unity command recompile_status` until it reports `completed`.
  - Tests: `unity command run_tests --mode editor --json --verbose`.
- Work on `master` in the main folder. Do not create worktrees.
- Code style follows the user's CLAUDE.md:
  - SOLID, small classes, short well-named functions.
  - Comments only for non-obvious logic.
  - Tabs for files under `Scripts/WorldInteraction` and `Scripts/UI`; 4 spaces for files under `Scripts/Planet` (match the neighbouring files).
- The working tree has uncommitted work by the user that must never be staged or committed: `GameControlsSetup.cs` (the `BuildRowIndicator` line), `TreeFactory.cs`, `SurfaceLayer.cs`, `MountainView.cs`, `RowDepth.cs`, `RowDepthStyle.cs`, `BuildRowSelectionTests.cs` and `BuildRowIndicator.cs`.
  - Stage files by path.
  - For `GameControlsSetup.cs`, stage only this plan's hunk. Build a patch of just that hunk and run `git apply --cached`. `git add -p` is not available.
- Commit Unity's `.meta` files for every new script, and end each commit message with `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
- Angle convention for the arc: degrees, 0 = the object's local up, positive = clockwise (90 = right, 270 = left).
- Spec values:
  - ground span 160°
  - ring span 360°
  - flying = has an `AirborneAgent` (`IAirborne`) component
  - menu only while zoomed into the city
  - tapping empty space clears the selection

## Review Focus

1. **Every action is reachable.** For any action count from 0 to 20, on both shapes, paging through all pages shows each action index exactly once. Test in Task 1.
2. **No actions or a too-small arc.** Zero actions give no slots. A slot angle that leaves a ground arc with fewer than 3 slots throws `ArgumentException` instead of producing overlapping arrows. Tests in Task 1.
3. **A demolished selection.** After "Demolish", the building lingers through its collapse animation. The selection must clear at once instead of keeping a menu on a dying building. `isValid` test in Task 2; play check in Task 7.
4. **Pinch-zooming inside city view.** During the pinch the camera state is `Zooming`, so the menu must key off the zoom level (`IsZoomedIn`), not `CameraState.CityView`, or it flickers. Play check in Task 7.
5. **Tapping a menu button.** A tap on a menu button must not also count as a tap on empty space and clear the selection. The `IsOverGui` guard covers it. Play check in Task 7.

---

### Task 1: `ArcLayout`

**Files:**
- Create: `Pocketplanet/Assets/Scripts/Planet/Interaction/ArcLayout.cs`, which holds `ArcShape`, `ArcSlotKind`, `ArcSlot` and `ArcLayout`, all in namespace `PocketPlanet.Planet`.
- Test: `Pocketplanet/Assets/Tests/EditMode/Planet/ArcLayoutTests.cs`

**Interfaces:**
- Produces:
  ```csharp
  public enum ArcShape { Ground, Ring }
  public enum ArcSlotKind { Action, PreviousPage, NextPage }
  public readonly struct ArcSlot { public float Angle { get; } public ArcSlotKind Kind { get; } public int ActionIndex { get; } } // ActionIndex is -1 for arrows
  public static class ArcLayout
  {
      public const float GroundSpan = 160f;
      public static float SpanOf(ArcShape shape);                                   // 160 or 360
      public static int Capacity(ArcShape shape, float slotAngle);                  // floor(span / slotAngle)
      public static int PageCount(ArcShape shape, float slotAngle, int actionCount); // >= 1
      public static int NormalizePage(ArcShape shape, float slotAngle, int actionCount, int page); // Ground clamps, Ring wraps
      public static IReadOnlyList<ArcSlot> Layout(ArcShape shape, float slotAngle, int actionCount, int page);
  }
  ```

**Algorithm.** Let `C = Capacity` and `n = actionCount`, with ground angles in [-80, 80] and ring angles in [0, 360).
- **Everything fits (`n <= C`):** one page. Action `i` sits at `(i - (n - 1) / 2f) * slotAngle`, taken modulo 360 for the ring.
- **Ground, too many actions:** there are `C` fixed slot positions at `(j - (C - 1) / 2f) * slotAngle`, for `j = 0..C-1`.
  - Page 0 puts actions in slots `0..C-2` and `NextPage` in slot `C-1`.
  - A middle page puts `PreviousPage` in slot 0, `NextPage` in slot `C-1`, and `C-2` actions in between.
  - The last page puts `PreviousPage` in slot 0 and the remaining actions from slot 1 on. Leftover slots stay empty.
  - Page count: if `m = n - (C - 1) <= C - 1`, there are 2 pages. Otherwise there are `2 + ceil((m - (C - 1)) / (C - 2f))`.
  - Throw `ArgumentException` when `C < 3`.
- **Ring, too many actions:** use the even spacing `s = 360f / C`. Slot `k` is at `(180 + s/2 + k*s) % 360`.
  - `k = 0` is `PreviousPage` (just left of the bottom) and `k = C-1` is `NextPage` (just right of it).
  - Slots `1..C-2` hold up to `C-2` actions.
  - Page count is `ceil(n / (C - 2f))`, and pages wrap.

- [ ] **Step 1: Write the failing tests** in `ArcLayoutTests` (namespace `PocketPlanet.Planet.Tests`, compare angles with `Mathf.DeltaAngle(a, b)` within `1e-3`):
  - `Capacity_GroundAt30Degrees_Is5`: `Capacity(Ground, 30f) == 5`. Also `Capacity(Ring, 30f) == 12`.
  - `Fitting_ThreeActions_CentredLeftToRight`: `Layout(Ground, 30f, 3, 0)` gives action angles -30, 0, 30 for indexes 0, 1, 2, and no arrows.
  - `Fitting_TwoActions_StraddleTop`: the angles are -15 and 15.
  - `NoActions_NoSlots`: `Layout(Ground, 30f, 0, 0)` is empty, and `PageCount == 1`.
  - `GroundOverflow_FirstPage_HasOnlyNextArrowOnRight`: `Layout(Ground, 30f, 7, 0)` has 4 actions (0..3) and one `NextPage` at +60.
  - `GroundOverflow_LastPage_HasOnlyPreviousArrowOnLeft`: page 1 of 7 actions has `PreviousPage` at -60 and actions 4, 5, 6.
  - `GroundOverflow_MiddlePage_HasBothArrows`: with 10 actions, `PageCount == 3` and page 1 holds actions 4, 5, 6 with arrows at ±60.
  - `Ground_PageIsClamped`: `NormalizePage(Ground, 30f, 7, 5) == 1` and `NormalizePage(Ground, 30f, 7, -2) == 0`.
  - `RingOverflow_ArrowsFlankBottom`: `Layout(Ring, 30f, 15, 0)` has `PreviousPage` at 195, `NextPage` at 165, and 10 actions.
  - `Ring_PagesWrap`: `NormalizePage(Ring, 30f, 15, 2) == 0` and `NormalizePage(Ring, 30f, 15, -1) == 1`.
  - `EveryActionAppearsExactlyOnce` (Review Focus 1): for each shape and each `n` in `0..20`, collecting the `ActionIndex` values over all pages gives exactly `0..n-1`, each once.
  - `GroundTooNarrow_Throws` (Review Focus 2): `Assert.Throws<ArgumentException>(() => Layout(Ground, 60f, 9, 0))`, since its capacity is 2.
- [ ] **Step 2:** Recompile and run the edit-mode tests. Expected: a compile error, or the `ArcLayoutTests` tests failing.
- [ ] **Step 3:** Implement `ArcLayout.cs` following the algorithm above.
- [ ] **Step 4:** Recompile and run the edit-mode tests. Expected: all `ArcLayoutTests` pass, and the existing tests still pass.
- [ ] **Step 5:** Commit `ArcLayout.cs`, `ArcLayoutTests.cs`, their `.meta` files and the new `Interaction` folder `.meta` with the message "Add arc menu slot layout with paging".

### Task 2: `Selection`

**Files:**
- Create: `Pocketplanet/Assets/Scripts/Planet/Interaction/Selection.cs` (namespace `PocketPlanet.Planet`)
- Modify: `Pocketplanet/Assets/Scripts/WorldInteraction/WorldPicker.cs` to add a public `IsPickable`.
- Test: `Pocketplanet/Assets/Tests/EditMode/Planet/SelectionTests.cs`

**Interfaces:**
- Produces:
  ```csharp
  public sealed class Selection
  {
      public Selection(Func<GameObject, bool> isValid = null); // null means always valid
      public GameObject Current { get; }                        // null when nothing is selected
      public bool HasTarget { get; }
      public event Action Changed;
      public event Action Reselected;
      public void Select(GameObject target);  // null clears; the same object raises Reselected only
      public void Clear();                    // raises Changed only if something was selected
      public void Refresh();                  // clears when Current is destroyed, inactive or !isValid
  }
  ```
- `WorldPicker.IsPickable(GameObject target) : bool` returns `IsStanding(target)` for buildings and `true` for everything else. It is the `isValid` that Task 4 passes in.

- [ ] **Step 1: Write the failing tests** in `SelectionTests`. Create GameObjects with `new GameObject()` and remove them in `[TearDown]` with `Object.DestroyImmediate`. Count events with local counters.
  - `Select_SetsCurrent_RaisesChanged`
  - `SelectSame_RaisesReselectedNotChanged`
  - `SelectOther_Switches_RaisesChanged`
  - `Clear_RaisesChanged_OnlyWhenSomethingSelected`: clearing an empty selection raises nothing.
  - `SelectNull_Clears`
  - `Refresh_DestroyedTarget_Clears`: after `DestroyImmediate` and `Refresh()`, `HasTarget` is false and `Changed` was raised once. A second `Refresh()` raises nothing.
  - `Refresh_InactiveTarget_Clears`
  - `Refresh_InvalidTarget_Clears` (Review Focus 3): use `new Selection(go => valid)`, flip `valid` to false, call `Refresh()`, and the selection is cleared.
- [ ] **Step 2:** Recompile and run the tests. Expected: fail.
- [ ] **Step 3:** Implement `Selection.cs`, and add `public static bool IsPickable(GameObject target)` to `WorldPicker`.
- [ ] **Step 4:** Recompile and run the tests. Expected: all pass.
- [ ] **Step 5:** Commit with the message "Add selection that outlives the context menu".

### Task 3: Camera support

**Files:**
- Modify: `Pocketplanet/Assets/Scripts/CameraControls/CameraController.cs`

**Interfaces:**
- Produces:
  - `public bool IsZoomedIn` (the existing private property, made public). The views and the tap selector use it as "in city view".
  - `public void FocusOn(Transform target)`
- Replace `MoveCameraToSelectedPosition(Vector2)` with `MoveCameraTo(Vector2 worldPoint, float zoomLevel)`.
  - Double-tap calls it with `comfortableZoom`.
  - `FocusOn` calls it with `IsZoomedIn ? CurrentZoomAmount : comfortableZoom`.
- `FocusOn` first calls `StopFollowing()` and `StopAllCoroutines()`. At the end of the move it sets the camera height the way `FollowTarget` does, by extracting the height part of `FollowTarget` into `void AimHeightAt(Vector2 worldPoint)` and calling it from both.

- [ ] **Step 1:** Make the changes above.
- [ ] **Step 2:** Recompile. Expected: `completed` with no errors, and the edit-mode tests still pass.
- [ ] **Step 3:** In Play Mode, run a throwaway `AgentScripts` entry that calls `GameHandler.Instance.CameraController.FocusOn(<any Building transform>)`, waits 2 s and captures the Game view. Expected: the building is roughly centred, and the double-tap zoom still works when tested with `Gestures.cs`.
- [ ] **Step 4:** Commit `CameraController.cs` with the message "Let the camera focus once on a target".

### Task 4: `ArcMenuView` replaces `RadialContextMenu`

**Files:**
- Create: `Pocketplanet/Assets/Scripts/WorldInteraction/ContextActionCatalog.cs`
- Create: `Pocketplanet/Assets/Scripts/WorldInteraction/ArcMenuView.cs`
- Modify: `Pocketplanet/Assets/Scripts/WorldInteraction/WorldTapSelector.cs`
- Modify: `Pocketplanet/Assets/Editor/GameControlsSetup.cs`
- Delete: `Pocketplanet/Assets/Scripts/WorldInteraction/RadialContextMenu.cs` and its `.meta`
- Regenerate: `Pocketplanet/Assets/Prefabs/WorldInteraction.prefab`

**Interfaces:**
- Consumes: `ArcLayout` and `ArcShape` (Task 1), `Selection` and `WorldPicker.IsPickable` (Task 2), `CameraController.IsZoomedIn` (Task 3).
- Produces:
  - `ContextActionCatalog`, which holds the provider list that is in `WorldTapSelector` today:
    - `List<ContextAction> For(GameObject target)`
    - `bool HasActions(GameObject target)`
  - `WorldTapSelector.Selection : Selection`, created in `Awake` with `WorldPicker.IsPickable`. `Update()` calls `Selection.Refresh()`.
  - `ArcMenuView : MonoBehaviour, IContextMenu`, with `[RequireComponent(typeof(WorldTapSelector))]`.
  - `GameControlsSetup.CreateInteractionPrefab() : GameObject`. Its components are `WorldTapSelector`, `ArcMenuView`, `SelectionHighlight` and `OffscreenPointer`. In this task, add only the first two; Tasks 5 and 6 add the other two.

**`WorldTapSelector` tap rules:**
- Keep the existing guards: over GUI, multi-tap, placing an object.
- Return early unless `CameraController.IsZoomedIn`.
- Pick the target as today. If it is `null` or has no actions, call `Selection.Clear()`. Otherwise call `Selection.Select(target)`.

**`ArcMenuView`.** Reuse the button, label, info-pill and upright-label code from `RadialContextMenu`.
- **Serialized sizes:** `radius = 300f`, `buttonSize = 140f`, `buttonGap = 12f`, all in canvas units.
- **Slot angle:** `2 * asin((buttonSize + buttonGap) / 2 / radius)` in degrees, computed once.
- **Shape:** `Ring` when the target has an `AirborneAgent`, otherwise `Ground`.
- **Background:** a filled `UiShapes.Ring` image (`Radial360`) with `fillAmount = span / 360`, rotated so the fill is centred on up.
- **Arrow buttons:** labelled `<` and `>`. Pressing one changes the page through `ArcLayout.NormalizePage` and rebuilds the buttons.
- **Rebuilding:** on `Selection.Changed`, reset to page 0, clear `dismissed` and rebuild. On `Selection.Reselected`, clear `dismissed`.
- **`Close()`:** sets `dismissed = true` and hides the menu. It does not clear the selection.
- **Shown** only when the selection has a target, `IsZoomedIn` is true, `dismissed` is false, there are actions, and `RowDepthStyle.IsInteractiveObject(target)` is true. Otherwise hide the root GameObject without destroying it.
- **Placement in `LateUpdate`:**
  - Ground: the current `RadialContextMenu.Place` logic (ground point and screen-up rotation) without `RadiusFor`.
  - Ring: positioned at the screen point of `WorldPicker.VisibleBounds(target).center`, with no rotation.
  - Button positions use `PositionOnArc` converted to this angle convention: `x = sin(a)`, `y = cos(a)`.

- [ ] **Step 1:** Create `ContextActionCatalog` and `ArcMenuView`. Rewrite `WorldTapSelector`. Delete `RadialContextMenu`.
- [ ] **Step 2:** Refactor `GameControlsSetup`: move the interaction prefab block into `public static GameObject CreateInteractionPrefab()` and use the new components. `AddToGameScenes` calls it.
- [ ] **Step 3:** Recompile until `completed` with no errors.
- [ ] **Step 4:** Rebuild only the interaction prefab with `run_script` in edit mode (`AgentScripts/RebuildInteractionPrefab.cs`, entry `RebuildInteractionPrefab.Run`, which calls `GameControlsSetup.CreateInteractionPrefab()` and `AssetDatabase.SaveAssets()`). Do not run `AddToGameScenes`: it would also rebuild the build-row prefab with the user's uncommitted `BuildRowIndicator`. Expected: `git diff --stat` shows `WorldInteraction.prefab` changed, and its GUID in the `.meta` is unchanged.
- [ ] **Step 5:** In Play Mode, zoom in with `SwipeCheck`-style pinch helpers from `Gestures.cs`, call `Selection.Select` on a `Building` through a throwaway agent script, and capture the Game view. Expected: a 160° arch sits on the ground point with buttons that don't overlap.
- [ ] **Step 6:** Commit the new and changed scripts, the deleted `RadialContextMenu.cs` and `.meta`, the prefab, and only this task's hunk of `GameControlsSetup.cs`, with the message "Replace radial context menu with fixed-size arc menu".

### Task 5: `SelectionHighlight`

**Files:**
- Create: `Pocketplanet/Assets/Scripts/WorldInteraction/SelectionHighlight.cs`
- Modify: `GameControlsSetup.CreateInteractionPrefab()` to add the component, then rebuild the prefab as in Task 4 Step 4.

**Interfaces:**
- Consumes: `WorldTapSelector.Selection` and `WorldPicker.VisibleBounds`.
- **Overlay:** its own canvas from `OverlayFactory.CreateOverlay(transform, "SelectionHighlightCanvas", 54)`, so it sits below the menu's 55.
- **Ring:** a `UiShapes.Ring` image at the screen point of the bounds' centre.
  - Diameter is `max(minDiameter, onScreenBoundsDiagonal / canvas.scaleFactor + padding)`.
  - Serialized `minDiameter = 90f`, `padding = 30f`, `color = new Color(1f, 0.92f, 0.55f, 0.9f)`.
- **Hidden** when there is no selection or the bounds' centre is outside the viewport. Shown at every zoom level otherwise.

- [ ] **Step 1:** Implement the component, add it to the prefab and rebuild the prefab.
- [ ] **Step 2:** Recompile. Then in Play Mode, select Blockman (`Citizen_Blockman`) through an agent script, zoom fully out, and capture. Expected: a ring of at least 90 units sits on Blockman in planet view.
- [ ] **Step 3:** Commit with the message "Highlight the selection at every zoom level".

### Task 6: `OffscreenPointer`

**Files:**
- Create: `Pocketplanet/Assets/Scripts/WorldInteraction/OffscreenPointer.cs`
- Modify: `Pocketplanet/Assets/Scripts/UI/UiShapes.cs` to add `public static Sprite Arrow`, a filled triangle pointing up.
  - Generalise the private `Create` to take a `Func<Vector2, float>` alpha over the unit square. The existing shapes wrap their radial function.
- Modify: `GameControlsSetup.CreateInteractionPrefab()` to add the component, then rebuild the prefab.

**Interfaces:**
- Consumes: `WorldTapSelector.Selection` and `CameraController.FocusOn` (through `GameHandler.Instance.CameraController`).
- **Overlay:** its own canvas at sorting order 56.
- **Arrow:** an `Image` with a `Button`, size 90 units, inset by `edgeMargin = 80f` canvas units.
- **Shown** when there is a selection and the target's viewport point is outside [0, 1] on either axis.
- **Position:** where the line from the screen centre toward the target meets the screen rectangle shrunk by the margin. Rotated so the triangle's tip points at the target.
- **Click:** calls `FocusOn(Selection.Current.transform)`.

- [ ] **Step 1:** Implement the arrow sprite, the component and the prefab change, then rebuild the prefab.
- [ ] **Step 2:** Recompile, and run the edit-mode tests to confirm none broke.
- [ ] **Step 3:** In Play Mode, select a building, turn the camera away from it in a script (rotate the `CameraController` transform 90°), and capture. Expected: the arrow sits at the screen edge pointing toward the building. Invoke the button's `onClick`, wait 2 s and capture again. Expected: the building is centred and the arrow is gone.
- [ ] **Step 4:** Commit with the message "Point to an off-screen selection and jump to it".

### Task 7: End-to-end check

**Files:**
- Create: `Pocketplanet/AgentScripts/ArcMenuCheck.cs`, which is kept for future checks. Entry `ArcMenuCheck.Run` reports to the log and saves captures under `Temp/arcmenu-*.png`.

Each check below is done in Play Mode and confirmed by its capture or log line:

- [ ] **Step 1: Ground arch.** Select a building in city view. The arch is centred on its ground point and cut off at 160°, and a tall neighbour behind it is drawn underneath the UI.
- [ ] **Step 2: Fixed size.** Change the zoom level between the snap threshold and 1. The on-screen radius is the same, logged in canvas units.
- [ ] **Step 3: Pinch (Review Focus 4).** Pinch with the `Gestures.cs` helpers while the menu is open and the zoom stays above the threshold. The menu stays visible every frame; log any frame where it is hidden.
- [ ] **Step 4: Zoom out and back.** Zoom out to planet view: the menu hides and the highlight stays. Zoom back in: the menu reappears.
- [ ] **Step 5: Demolish (Review Focus 3).** Invoke "Demolish". The selection clears in the same frame, and the menu and highlight are gone.
- [ ] **Step 6: Follow.** Invoke "Follow" on Blockman. The menu closes, the highlight stays, and re-selecting Blockman reopens the menu as a full ring.
- [ ] **Step 7: Button tap (Review Focus 5).** Simulate a tap over a menu button through Lean Touch. The selection is not cleared.
- [ ] **Step 8: Paging.** Temporarily register a test provider with 7 actions (only inside the agent script). The first page shows a `>` arrow, and pressing it shows the rest with `<`.
- [ ] **Step 9:** Run all edit-mode tests. Expected: everything passes.
- [ ] **Step 10:** Commit `ArcMenuCheck.cs` with the message "Add play mode check for the arc menu".

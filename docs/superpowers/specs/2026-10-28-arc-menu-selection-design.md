# Arc Menu and Selection Design

Date: 2026-10-28

## Goal

Tapping a building or character in city view selects it and opens a menu shaped as an arch around it. The selection is visible at every zoom level, and when the selected object is out of view an arrow at the screen edge points to it and jumps the camera there.

## Decisions from the user

- The arch has a fixed on-screen size so every button stays a valid touch target.
- Ground objects get an arch centred on the object's ground point, cut off before it reaches the planet. Flying objects get a full circle.
- Each button takes a fixed minimum angle of the arch. When the actions don't fit, arrow buttons at the ends page left and right.
- The menu can only be open in city view, never in planet view.
- The menu is attached to the object, not the screen. Panning can move it off-screen.
- Other objects may stick up through the arch; the UI draws on top of them.
- A highlight marks the selection at every zoom level, including fully zoomed out (for example, following Blockman as he flies).
- An arrow at the screen edge points to the selected object when it is out of view. Tapping it jumps the camera to the object.
- Tapping empty space deselects.

## Assumptions (approved)

- Taps in planet view do not select. An existing selection keeps its highlight there.
- An object is flying when it has an `IAirborne` component. Everything else gets the ground arch.
- When the selected object is on a layer that is not active, the menu hides; the selection and highlight stay.
- Tapping the off-screen arrow centres the camera on the object once. It does not start following. The existing "Follow" action is still how to follow.
- Tapping the already selected object does nothing, unless an action dismissed its menu. In that case the menu opens again.
- Running an action that closes the menu (for example "Follow") keeps the selection and its highlight.

## Current state

- `RadialContextMenu` draws a half circle whose radius scales with the target (190 to 520 canvas units) and spreads buttons evenly. It has no paging.
- `WorldTapSelector` opens and closes that menu. The selection only exists while the menu is open.
- `CameraController` already has a `CameraState` with `PlanetView` and `CityView`, and a `Follow(Transform)` method.
- `ContextAction`, `IContextActionProvider`, `BuildingActionProvider`, `CitizenActionProvider`, `WorldPicker`, `OverlayFactory` and `UiShapes` stay as they are.

## Components

### `Selection`

Plain C# class with no Unity view code.

- `GameObject Current`, `void Select(GameObject)`, `void Clear()`, `event Action Changed`.
- `Select` with the current object raises `Reselected` instead of `Changed`. The menu uses it to reopen after being dismissed.
- When `Current` is destroyed, the selection reads as empty and raises `Changed` once. This is checked with a `Refresh()` call that the owning component runs each frame, so `Selection` stays testable in edit mode.

### `ArcLayout`

Pure math in the `PocketPlanet.Planet` assembly so edit-mode tests can reach it.

- Input: `ArcShape` (`Ground` = 160° centred on up, `Ring` = 360°), slot angle in degrees, action count, page index.
- Output: a list of `ArcSlot` values, each with a centre angle (degrees, 0 = local up, positive = clockwise) and a kind: `Action(index)`, `PreviousPage` or `NextPage`.
- Capacity is `floor(span / slotAngle)`.
- When the actions fit, they run left to right in order, and the group is centred on 0°. Unused slots stay empty; buttons are never stretched.
- Ground shape, too many actions: the left end slot is `PreviousPage` when an earlier page exists and the right end slot is `NextPage` when a later page exists. The remaining slots hold actions. The page index is clamped to the valid range.
- Ring shape, too many actions: the two slots on either side of 180° (the bottom) hold `PreviousPage` (left) and `NextPage` (right). Both always show, and paging wraps around.
- Exposes the page count so the view can clamp or wrap the page.

### `ArcMenuView`

Replaces `RadialContextMenu`. A MonoBehaviour on the screen-space overlay canvas (`OverlayFactory`).

- Fixed radius and button size in canvas units (serialized fields). The slot angle is derived from them so buttons never overlap.
- Each `LateUpdate` it positions itself:
  - Ground: at the object's ground point (`PlanetFrame` coordinate at height 0), rotated so its up matches the surface's outward direction on screen.
  - Ring: at the centre of the object's visible bounds, with no rotation.
- Button labels stay upright, as today.
- It does not clamp to the screen.
- It is visible only when `Selection.Current` is set, the camera state is `CityView`, the object has actions and the object is on the interactive layer (`RowDepthStyle.IsInteractiveObject`). Otherwise it hides without clearing the selection, and shows again when the conditions hold.
- It rebuilds its buttons when the selection changes or a page arrow is pressed. A new selection starts on page 0.
- The info pill (`IContextMenu.ShowInfo`) keeps working: it sits above the arch's top centre.
- `IContextMenu.Close()` dismisses the menu but keeps the selection. The menu stays dismissed until the selection changes or the player taps the selected object again.

### `SelectionHighlight`

A ring on its own overlay canvas, drawn below the menu.

- Centred on the selected object's visible bounds. Its diameter is the bounds' on-screen size plus padding, with a serialized minimum in canvas units, so it stays visible at full zoom-out.
- Visible at every zoom level while there is a selection and the object is on screen.

### `OffscreenPointer`

An arrow on the overlay canvas.

- Shown when there is a selection and the object's screen position is outside the viewport.
- Placed on the screen edge, inset by a margin, along the line from the screen centre to the object. Rotated to point at the object.
- Tapping it calls the new `CameraController.FocusOn(Transform)`.

### `CameraController.FocusOn(Transform)`

- Moves the camera once to centre on the target, reusing the existing move-to coroutine.
- If the camera is zoomed out past the snap threshold, it zooms in to the comfortable zoom level first, as `Follow` does.
- It does not start following and does not change an existing follow target.

### `WorldTapSelector`

- Owns the `Selection` instance and exposes it to the menu, highlight and pointer.
- In `CityView`: a tap on an object with actions selects it, and a tap on empty space clears.
- In other camera states, taps do nothing.
- Keeps today's guards: taps over GUI, multi-taps and placing an object are ignored.

## Data flow

`WorldTapSelector` → `Selection` → (`ArcMenuView`, `SelectionHighlight`, `OffscreenPointer`). The three views read the selection and the camera state. None of them refer to each other. Pressing an action runs `ContextAction.Execute(menu)` as today.

## Error handling

- The selected object is destroyed, deactivated or demolished (a building that is no longer standing, as `WorldPicker` defines it): the selection clears and all three views hide.
- There is no `PlanetHost.Active` or no main camera: the views hide and do nothing that frame.
- The object has no visible sprites: the highlight uses the object's position with the minimum size.

## Testing

Edit-mode tests in `Tests/EditMode/Planet`:

- `ArcLayoutTests`:
  - the capacity for a given slot angle
  - centring when the actions fit
  - the first action at 0°
  - no arrows when the actions fit
  - arrows only on the sides with more pages
  - clamping the page index
  - ring arrows at the bottom
  - ring pages wrapping around
- `SelectionTests`:
  - select raises `Changed`
  - selecting the same object raises `Reselected`, not `Changed`
  - selecting another object switches
  - clear
  - a destroyed object reads as empty after `Refresh()`

Play-mode checks with an `AgentScripts` script and Game view captures:

- the arch sits on the ground point, cut off at the surface
- the arch keeps the same size at different zoom levels in city view
- the menu hides in planet view and returns on zooming back in
- the highlight stays visible at full zoom-out
- the pointer appears at the screen edge when the object is panned out of view, and tapping it centres the camera

## Out of scope

- Icons for actions (text labels stay).
- Dragging the arch to scroll.
- New actions or changes to the action providers.

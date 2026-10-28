# Pocket Planet

Unity project in `Pocketplanet/`. Unity **6000.6.5f1**, legacy Input Manager, Lean Touch for gestures.

## Driving the open editor from the command line

The editor runs the Unity Pipeline package (`com.unity.pipeline`), so the open editor can be controlled with the `unity` CLI (`C:\Users\Ulrik\AppData\Local\Unity\bin`, add it to `PATH` in bash). Do not use the Unity window itself. Batch mode can't open the project while the editor has it open.

- `unity pipeline list` shows whether the editor's server is reachable. After installing or updating the package, Unity only imports it once its window has been focused.
- `unity command set_autotick --enable true` keeps the editor ticking while unfocused.
- `unity command recompile` then poll `unity command recompile_status` until `completed`.
- `unity command run_tests --mode editor` (add `--json --verbose` for failure details).
- `unity command editor_play` / `editor_stop` / `editor_status`.
- `unity command run_script --file AgentScripts/<File>.cs --entry <Class.Method>` runs C# from `Pocketplanet/AgentScripts/` (outside `Assets`, so no reimport). Only touch scene objects there while `Application.isPlaying`; changes made in edit mode stick to the open scene.
- `unity command capture_game_view --save_path Temp/<name>.png` (the path must be inside the project).
- In `run_script`, `Screen` is not the Game view size; use `PlayModeWindow.GetRenderingResolution`.

`AgentScripts/Gestures.cs` replays a pinch and two-finger swipes through Lean Touch in Play Mode (`--entry SwipeCheck.Run`) and reports the camera zoom, focus height and active layer.

## Input in the editor

Ctrl + drag simulates a pinch and Alt + drag a two-finger drag (layer swipe), in both the Game view and the Device Simulator. Alt also moves the pinch pivot to the cursor.

# Procedural Planet Foundation — Design

Date: 2026-10-23
Status: Draft for review

## 1. Purpose

Pocket Planet shows a player's budget health through a small planet city. Sticking to the budget brings good events; overspending brings bad ones. The 2019 revival stalled at world generation. This spec covers the first step of the new revival: a procedural planet that replaces the hand-drawn planet in the existing game and exposes its contents as data, so events, characters and buildings can react to anything on the planet.

### Goals

- Generate a planet in the style of the existing asset: a round outline with blocky, thresholded continents and sea inside it.
- Add biomes for visual variety and as gameplay data.
- Make the number of continents and the circumference adjustable. Circumference sets how much room the player has.
- Provide layered data: surface (two building rows), underground, and air.
- Allow building only on the rim, on land. Water is not buildable, except through dedicated rules for ports and bridges.
- Add a world-mood axis from doom (−1) to hallowed (+1), with the hooks for its effects.
- Drop the planet into the old game without rewriting existing systems.

### Non-goals (later specs)

- Drawing the underground layer (a zoom/toggle view), building interiors and the interior upgrade path.
- Planes, plane emergencies and Blockman rescues. This spec only supplies the data and queries they need.
- Fire, smoke, rainbow and sunray effects. This spec only provides the interface they plug into.
- Fixing player drag-to-build input (Lean Touch was removed and the input code is commented out).
- Connecting world mood to the banking/budget code.

## 2. Context: what exists today

- `Planet.cs` provides a static API (`Radius`, `PlanetObject`, `FindPointOnSurface`, `PlaceObjectOnSurface`, `RotateObjectToSurface`, `GetBlockzillaSpawnpoint`). Buildings, citizens, Blockman, Blockzilla, meteors, missiles and the camera all use it.
- The old planet prefab is a sprite with a `CircleCollider2D` tagged `Water` (radius 7.62) and a `PolygonCollider2D` "BuildingArea" tagged `Ground`. `ObjectSpawner.ValidBuildPosition` and `ConstructionHandler.InWater/Intersecting` check placement with Physics2D overlaps against those tags.
- Characters move along the rim with `RotateAround` the planet centre.
- `Assets/Scripts/WorldGeneration/` holds the unfinished 2019 generator (`MapGenerator`, `WorldData`, `NoiseGenerator`, `WorldKernel`, `MapDisplay`). It builds a large square grid of land/water/air/space nodes. It is superseded by this design.
- Unity 6000.6.5f1, URP 17.6, Unity Test Framework 1.8.

## 3. Architecture

A pure C# generator turns a seed plus settings into a `PlanetModel` that holds independent layers. Views and adapters read from the model, and the model knows nothing about them.

```
PlanetSettings + seed ──► PlanetGenerator ──► PlanetModel
                                               ├─ BlockMap     (biome per block, drives visuals)
                                               ├─ Surface      (rim profile, Back/Front rows)
                                               ├─ Underground  (depth bands, data only)
                                               └─ Air          (altitude bands, traffic, zones)
                                                      │
                     ┌────────────────────────────────┼─────────────────────────┐
                 PlanetView                    Planet.cs adapter            Game systems
          (shader, palette, mood)           (old static API kept)   (events, buildings, Blockman)
```

- Code lives in `Assets/Scripts/Planet/` with its own assembly definition (`PocketPlanet.Planet`, auto-referenced so `Assembly-CSharp` can use it). Tests go in `Assets/Tests/EditMode/Planet/` (`PocketPlanet.Planet.Tests`).
- Data and generation classes don't depend on MonoBehaviours, so they can be tested in EditMode.
- Everything is reached through an `IPlanet` interface (`Frame`, `BlockMap`, `Surface`, `Underground`, `Air`, `Queries`). One `PlanetHost` MonoBehaviour generates the model and provides `IPlanet`. The legacy `Planet.cs` adapter is the only static access point.
- Persistence: the same seed and settings always give the same planet, so a save only needs the seed, the settings reference and the occupants.

## 4. Coordinates

- **Arc:** distance along the rim in world units, in `[0, Circumference)`, wrapping at the seam. Arc 0 is the planet's local +X axis, increasing counter-clockwise.
- **Height:** distance from the rim along the outward normal. Positive is air, negative is underground, and 0 is the surface where buildings stand.
- `PlanetCoord(arc, height)`: a value type.
- `ArcRange(start, length)`: a wrap-aware range with `Contains(arc)`, `Overlaps(other)`, `Center`, and `Distance(a, b)` (shortest way around).
- `PlanetFrame` (centre transform + radius): `ToCoord(Vector2 world)`, `ToWorld(PlanetCoord)`, `UprightRotation(arc)`. `ToCoord` always returns a valid arc, whatever the distance, which is what makes placement snap to the rim from anywhere.

## 5. Generation

### 5.1 Settings — `PlanetSettings` (ScriptableObject)

| Field | Meaning | Default |
|---|---|---|
| `Seed` | int, all randomness derives from it | 0 |
| `Circumference` | world units; radius = C / 2π | 48 (≈ old radius 7.62) |
| `BlockSize` | world units per block; bigger planets get more blocks | 0.6 |
| `ContinentCount` | 1–8 separate land arcs on the rim | 3 |
| `LandCoverage` | 0.2–0.9, fraction of the rim that is land | 0.6 |
| `CoastRoughness` | 0–1, noise strength on coastlines | 0.5 |
| `BiomeTable` | list of `BiomeDefinition` assets | default set |
| `UndergroundBands` | list of depth band definitions | Topsoil, Bedrock, Deep |
| `AirBands` | list of altitude band definitions | Low, Flight, High, Orbit |
| `SurfaceSampleSpacing` | rim sample spacing as a fraction of `BlockSize` | 0.25 |

### 5.2 Steps

1. **Grid.** A square grid of `N = ceil(2R / BlockSize)` blocks covers the disc. Blocks whose centre lies outside the circle are `Empty`.
2. **Rim continents.** Split the circumference into `ContinentCount` land arcs and the same number of ocean arcs, alternating. Land arcs add up to `LandCoverage × Circumference`. Each arc's length is jittered by up to ±25% of the average and then renormalised. The whole layout is rotated by a random offset. Each ocean arc is at least `2 × BlockSize` long, so neighbouring continents never merge on the rim.
3. **Inland shapes.** Each continent gets a land score that falls off with distance inward from its arc. The score is perturbed by value noise scaled by `CoastRoughness`. A few noise islands are added in the interior for looks. A block becomes land when its score exceeds a threshold. Rim blocks are then forced to agree with the arc layout from step 2, so the rim guarantee holds.
4. **Biomes.** Two low-frequency noise fields, temperature and moisture, are sampled per block. Land blocks get the first `BiomeDefinition` in the table whose temperature and moisture ranges contain the sample. Water blocks are `ShallowWater` if they are within one block of land, otherwise `DeepWater`.
5. **Surface profile.** See 6.1.
6. **Underground.** See 6.2.

All randomness comes from `System.Random(Seed)` and noise functions seeded from it. `UnityEngine.Random` is never used.

### 5.3 Biomes — `BiomeDefinition` (ScriptableObject)

- `Id` (string), `DisplayName`
- `Kind`: Land or Water
- `NormalColor`, `DoomColor`, `HallowedColor`
- `TemperatureRange`, `MoistureRange` (0–1, land only)
- `Buildable` (bool)
- `Tags`: free-form strings such as `Fertile`, `Arid`, `Cold` or `Coastal`, for events and placement rules

Default set: Grassland, Forest, Desert, Snow, ShallowWater, DeepWater. All default land biomes are buildable. You can add biomes without code changes.

## 6. Layers

### 6.1 Surface layer

- **`SurfaceProfile`.** The rim is sampled every `SurfaceSampleSpacing × BlockSize` of arc. Each sample reads the block directly under the rim (radius `R − BlockSize/2`), so what is visible at the edge matches the data exactly. Each sample stores the terrain (Land/Water), the biome and `IsCoast` (adjacent to the other terrain type). Consecutive samples with the same terrain and biome merge into `SurfaceSegment(range, terrain, biome)`.
  - Queries: `TerrainAt(arc)`, `BiomeAt(arc)`, `SamplesIn(range)`, `Segments` (filterable), `IsAll(range, predicate)`.
- **Rows.** `Surface.Rows[Back]` and `Surface.Rows[Front]`, each an `OccupancyTrack`.

### 6.2 Underground layer (data only)

- Depth bands come from `UndergroundBands` (name, depth range).
- For each band and each rim sample there is a `MaterialId` (Soil, Sand, Rock, Water, …), derived from the surface biome above it plus noise.
- Each band has its own `OccupancyTrack` for future basements and tunnels.
- Nothing is rendered in this spec.

### 6.3 Air layer

- **Bands** come from `AirBands` (name, height range). Defaults: Low (Blockman hovering, birds), Flight (planes), High (weather, meteor entry), Orbit.
- **`AirTraffic`.** A registry of `IAirborne` (`Id`, `PlanetCoord Position`, `Vector2 Velocity`, `Tags`). Owners register and unregister their entities, and `Position` is read live. Queries: `InBand(band)`, `InRange(arcRange, band?)`, `Nearest(coord, filter)`. Raises `Registered` and `Unregistered`.
- **`AirZones`.** Area effects placed by events: `AirZone(arcRange, band, kind, tags)`, where kind is e.g. `Storm`, `AshCloud` or `NoFly`. Queries: `ZonesAt(coord)` and `ZonesIn(range, band)`. Raises `ZoneAdded` and `ZoneRemoved`. This is the data hook for planes getting hit or breaking down.

### 6.4 Shared occupancy — `OccupancyTrack`

- A wrap-aware registry of `(IPlanetOccupant, ArcRange)`.
- Methods: `TryAdd(occupant, range)` (fails on overlap), `Remove(occupant)`, `Move(occupant, newRange)`, `IsFree(range, ignore?)`, `OccupantsIn(range)`.
- Raises `OccupantAdded` and `OccupantRemoved`.
- `IPlanetOccupant`: `Id`, `Kind` (Building, ConstructionSite, Prop, …) and `Tags`.

### 6.5 Cross-layer queries — `PlanetQueries`

- `FindLandingSite(nearArc, length, allowWater)` finds the nearest range (searching both ways from `nearArc`) that is all land and free in both surface rows. With `allowWater`, open water also counts. Returns `ArcRange?`.
- `NearestCoast(arc, direction?)`
- `FindNearestDeepWater(arc)`, used for Blockzilla spawning.
- `FindBuildSite(footprint, nearArc, maxNudge)` (see 7).

## 7. Placement

Placement is free (no slots) but checked against data. The building **always snaps to the rim**: only the cursor's angle from the planet centre matters, and its distance is ignored.

- **`PlanetFootprint`** (component on building prefabs):
  - `Width` (arc length). If it isn't set, it's computed from the sprite bounds.
  - `Row`: Back, Front or Either. The default is Front.
  - `Rule`: a reference to a placement rule.
  - `RequiredBiomeTags`, `ForbiddenBiomeTags`.
- **`IPlacementRule`** implementations:
  - `LandOnlyRule` (default): every sample in the range is land and its biome is `Buildable`.
  - `CoastalRule` (ports): the range contains both land and water samples, and the land samples are buildable.
  - `SpanningRule` (bridges): both ends are on buildable land, and the water in between is at most `MaxSpan`.
- **`PlacementValidator.Check(footprint, arc, row)`** returns `PlacementResult { IsValid, Reason }`, where `Reason` is one of `None`, `Water`, `BiomeNotBuildable`, `MissingBiomeTag`, `ForbiddenBiomeTag`, `Occupied` or `RuleViolation`. The order is fixed: rule first, then biome tags, then occupancy.
- **`FindNearestValid(footprint, arc, row, maxNudge)`** searches outward in small steps on both sides and returns the closest valid centre within `maxNudge`, or none.
- **Lifecycle:**
  1. The construction site reserves its range with `TryAdd`.
  2. The finished building takes over the same range.
  3. Destroying or upgrading the building calls `Remove`.
- **Rendering rows:** Back-row objects get a lower sorting order (and an optional tint) than Front-row objects. Both stand at the rim radius.

## 8. Visuals

- **`PlanetView`** creates two textures:
  - an N×N point-filtered R8 texture holding the biome index per block
  - a palette texture of size (biomeCount × 3) with rows for doom, normal and hallowed colours
- **Shader `PocketPlanet/PlanetSurface`:** hand-written URP unlit HLSL, transparent, on a quad sized 2R.
  - Looks up the colour from the biome index and the palette, blending between the rows by `_Mood`.
  - Discards pixels outside the circle, with an anti-aliased edge using `fwidth`.
  - Properties: `_BiomeMap`, `_Palette`, `_Mood` (−1..1).
- Editing biome colours rebuilds only the palette. Regenerating rebuilds the biome map.
- The existing sky and star background shaders stay as they are.
- `PlanetHost` has a custom inspector with a **Regenerate** button and live preview in edit mode. This replaces `MapGeneratorEditor`.

## 9. World mood (doom ↔ hallowed)

- A single axis: `Mood` in [−1, 1]. −1 is doom, 0 is normal, +1 is hallowed.
- `IWorldMoodSource` returns the target mood. For now an inspector value (`ManualMoodSource`) provides it. Later it will be fed by budget health.
- `WorldMoodController` reads the source, smooths it over time, applies it to every registered `IWorldMoodEffect`, and raises `MoodChanged(float)`.
- Effects built in this spec:
  - `PlanetPaletteMoodEffect`: sets `_Mood` on the planet material.
  - `GlobalColorMoodEffect`: drives a URP Volume `ColorAdjustments` component. Saturation rises towards hallowed. Doom gets lower saturation and a red/orange tint, hallowed a light warm tint. The curves can be set in the inspector.
- Future effects (not built, same interface): fires and smoke at land spots from `SurfaceProfile` (doom), and rainbows and sunrays (hallowed).

## 10. Integration with the existing game

- **`Planet.cs`** becomes an adapter over `PlanetHost`. It keeps its public static API and signatures:
  - `Radius`
  - `PlanetObject`
  - `FindPointOnSurface`
  - `PlaceObjectOnSurface`
  - `RotateObjectToSurface`
  - `GetBlockzillaSpawnpoint`, which now uses `FindNearestDeepWater` and returns a transform at that rim point
- **`ProceduralPlanet` prefab:** `PlanetHost` + `PlanetView` quad + a `CircleCollider2D` auto-sized to the radius (meteors and other physics users keep working). It replaces the old planet in `Main.unity` and `Development_Main.unity`.
- **`PlanetOccupant`** component: registers its building or construction site in the matching row track and implements `IPlanetOccupant`.
- **`ObjectSpawner.ValidBuildPosition`** and **`ConstructionHandler.InWater` / `Intersecting` / random placement** switch to `PlacementValidator` and `FindNearestValid`. `AppropriateDistanceToPlanet` is removed for buildings.
- **`Citizen_Blockman`** registers in `AirTraffic` while airborne and unregisters on landing.
- **Camera:** zoom limits that assume radius 7.62 are scaled by the actual radius.
- **Removed:** `Assets/Scripts/WorldGeneration/*`, `Assets/Editor/MapGeneratorEditor.cs` and `Assets/Scenes/WorldGeneration/*`. They remain in git history.

## 11. Debugging aids

`PlanetHost.OnDrawGizmos` draws:
- rim segments coloured by terrain
- occupied ranges for each surface row
- air band circles
- active air zones

Placement can be tested in the editor by picking an arc in the inspector, which shows the `PlacementResult`.

## 12. Testing

EditMode tests (`PocketPlanet.Planet.Tests`):
- The same seed and settings give an identical block map, profile and underground. Different seeds give different results.
- Rim land fraction is within ±3% of `LandCoverage`.
- The number of land arcs on the rim equals `ContinentCount` for counts 1–8 across several seeds.
- Radius equals `Circumference / 2π`, and the block count grows with circumference.
- `ArcRange` wrap maths: contains, overlaps and distance across the seam.
- `OccupancyTrack`: overlap rejection, including across the seam; remove; move; events.
- Each placement rule and each rejection reason; `FindNearestValid` with and without a solution.
- `FindLandingSite` with land only, with water allowed, and with no site.
- `AirTraffic` and `AirZones` queries by band and range.
- Mood palette blending at −1, 0 and +1.
- Performance: generation with `Circumference = 500` completes in under 50 ms in the editor.

Manual check: open `Development_Main`, regenerate with different settings, sweep the mood slider, and use the gizmos and inspector placement test.

## 13. Risks and open points

- The old prefabs' sprite pivots and collider assumptions may need small fixes once buildings are positioned by footprint instead of physics overlaps.
- The camera and the UI may assume the old planet size in more places than `CameraControls`. These are fixed as found.
- Player-driven placement can't be tested end to end until input is repaired (a separate task).

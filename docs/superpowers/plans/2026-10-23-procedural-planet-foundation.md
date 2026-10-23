# Procedural Planet Foundation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the hand-drawn planet with a seeded, procedural, blocky planet. Its surface, underground and air layers are exposed as data, it has data-driven placement and a doom↔hallowed mood, and it plugs into the existing game through the unchanged `Planet.cs` API.

**Architecture:** Pure C# generation and data classes (`PocketPlanet.Planet` assembly) build a `PlanetModel` behind an `IPlanet` interface. MonoBehaviours (`PlanetHost`, `PlanetView`, mood effects, occupant/footprint components) adapt it to Unity. The legacy static `Planet` class forwards to `PlanetHost`.

**Tech Stack:** Unity 6000.6.5f1, URP 17.6 (HLSL shader, Volume `ColorAdjustments`), Unity Test Framework 1.8 (NUnit, EditMode), C#.

**Spec:** `docs/superpowers/specs/2026-10-23-procedural-planet-foundation-design.md`

## Global Constraints

- All new runtime code: namespace `PocketPlanet.Planet`, folder `Assets/Scripts/Planet/`, assembly `PocketPlanet.Planet` (autoReferenced: true; references `Unity.RenderPipelines.Core.Runtime`, `Unity.RenderPipelines.Universal.Runtime`).
- Editor code: `Assets/Scripts/Planet/Editor/`, assembly `PocketPlanet.Planet.Editor` (Editor platform only).
- Tests: `Assets/Tests/EditMode/Planet/`, assembly `PocketPlanet.Planet.Tests` (Editor only, `UNITY_INCLUDE_TESTS`, references `PocketPlanet.Planet`, `UnityEngine.TestRunner`, `UnityEditor.TestRunner`, `nunit.framework.dll`).
- Planet code never calls `UnityEngine.Random`. All randomness derives from `PlanetSettings.Seed` via `System.Random` / `SeededNoise`.
- Coordinates: arc in world units in `[0, Circumference)`, arc 0 = planet local +X, increasing counter-clockwise. Height > 0 is air, < 0 is underground.
- Defaults: `Circumference` 48, `BlockSize` 0.6, `ContinentCount` 3 (range 1–8), `LandCoverage` 0.6 (range 0.2–0.9), `CoastRoughness` 0.5, `SurfaceSampleSpacing` 0.25 (× BlockSize).
- Default biomes: Grassland, Forest, Desert, Snow (land, all buildable), ShallowWater, DeepWater. No rock biome.
- Default air bands: Low 0–2, Flight 2–5, High 5–8, Orbit 8–14. Default underground bands: Topsoil 0–1, Bedrock 1–3, Deep 3–6 (depths positive downward).
- Mood is a single float in [−1, 1]: −1 doom, 0 normal, +1 hallowed.
- Placement always snaps to the rim. Only the angle from the planet centre is used.
- Code style (user CLAUDE.md): SOLID, small single-purpose classes, short well-named methods, comments only for non-obvious logic, no section banners, no commented-out code.

**Running tests** (the Unity Editor must be closed on this project, otherwise use Window → General → Test Runner → EditMode → Run):

```bash
"/c/Program Files/Unity/Hub/Editor/6000.6.5f1/Editor/Unity.exe" -batchmode -nographics -projectPath "W:/Projects/PocketPlanet/Pocketplanet" -runTests -testPlatform EditMode -testFilter "<Filter>" -testResults "W:/Projects/PocketPlanet/Pocketplanet/Logs/test-results.xml" -logFile "W:/Projects/PocketPlanet/Pocketplanet/Logs/test-run.log"
```

Exit code 0 = all passed, 2 = test failures (see `Logs/test-results.xml`), 1/3 = compile or run error (see `Logs/test-run.log`). A "fails first" step is satisfied by exit code 2, or by exit 1 with a compile error naming the missing type.

## Review Focus

1. **Tiny planet, many continents** (e.g. Circumference 10, ContinentCount 8): the generator clamps the continent count so each land and ocean arc is at least 3 × BlockSize, logs a warning, and still produces a valid planet. Test in Task 3.
2. **Footprint across the arc seam** (centre arc 0.1, width 2): validation and occupancy treat it as one contiguous range, so it is neither falsely free nor falsely occupied. Tests in Tasks 1, 5 and 9.
3. **Upgrading a building in place:** releasing the old occupant must happen before the new construction site reserves the same range, otherwise the upgrade is silently rejected as `Occupied`. Test in Task 13.
4. **Cursor exactly at the planet centre** (or on the rim seam): `ToCoord` returns a finite arc (0), never NaN. Test in Task 1.
5. **No room anywhere** (footprint wider than every land segment, or the rim fully occupied): `FindNearestValid` returns null and random auto-construction stops without exceptions or infinite loops. Tests in Task 9 and Task 13.

---

## File Structure

```
Assets/Scripts/Planet/
  PocketPlanet.Planet.asmdef
  Core/        PlanetCoord.cs, ArcRange.cs, PlanetFrame.cs
  Generation/  SeededNoise.cs, PlanetSettings.cs, BiomeDefinition.cs, BandDefinition.cs,
               BiomeTable.cs, RimLayout.cs, BlockMap.cs, BlockMapGenerator.cs, PlanetGenerator.cs
  Layers/      TerrainKind.cs, SurfaceSample.cs, SurfaceSegment.cs, SurfaceProfile.cs, SurfaceRow.cs,
               SurfaceLayer.cs, IPlanetOccupant.cs, OccupancyTrack.cs, UndergroundMaterial.cs,
               UndergroundLayer.cs, IAirborne.cs, AirTraffic.cs, AirZone.cs, AirZones.cs, AirLayer.cs
  Model/       IPlanet.cs, PlanetModel.cs, PlanetQueries.cs
  Placement/   PlacementReason.cs, PlacementResult.cs, IPlacementRule.cs, LandOnlyRule.cs,
               CoastalRule.cs, SpanningRule.cs, Footprint.cs, PlacementValidator.cs
  Rendering/   BiomePalette.cs, PlanetView.cs, PlanetSurface.shader
  Mood/        IWorldMoodSource.cs, IWorldMoodEffect.cs, ManualMoodSource.cs, MoodSmoother.cs,
               WorldMoodController.cs, PlanetPaletteMoodEffect.cs, MoodGrading.cs, GlobalColorMoodEffect.cs
  Unity/       PlanetHost.cs, PlanetFootprint.cs, PlanetOccupant.cs, AirborneAgent.cs
  Editor/      PocketPlanet.Planet.Editor.asmdef, PlanetHostEditor.cs, PlanetSetup.cs
Assets/Planet/  (generated assets: DefaultPlanetSettings.asset, Biomes/*.asset, ProceduralPlanet.prefab, PlanetSurface.mat)
Assets/Tests/EditMode/Planet/  PocketPlanet.Planet.Tests.asmdef, TestPlanets.cs, *Tests.cs
```

Modified: `Assets/Scripts/Other/Planet.cs`, `GameManagers/ConstructionHandler.cs`, `ObjectSpawner.cs`, `Buildings/Building_ConstructionSite.cs`, `Buildings/Building.cs`, `Citizens/Citizens/Citizen_Blockman.cs`, `Controls/CameraControls.cs`, `Scenes/Main.unity`, `Scenes/Development_Main.unity`.
Deleted: `Assets/Scripts/WorldGeneration/`, `Assets/Editor/MapGeneratorEditor.cs`, `Assets/Scenes/WorldGeneration/`.

---

### Task 1: Assemblies and coordinates

**Files:**
- Create: `Assets/Scripts/Planet/PocketPlanet.Planet.asmdef`, `Assets/Tests/EditMode/Planet/PocketPlanet.Planet.Tests.asmdef`
- Create: `Assets/Scripts/Planet/Core/PlanetCoord.cs`, `ArcRange.cs`, `PlanetFrame.cs`
- Test: `Assets/Tests/EditMode/Planet/ArcRangeTests.cs`, `PlanetFrameTests.cs`

**Interfaces:**
- Produces:
  - `readonly struct PlanetCoord(float arc, float height)` with `Arc`, `Height`.
  - `readonly struct ArcRange`:
    - `static ArcRange Centered(float center, float width, float circumference)`
    - `ArcRange(float start, float length, float circumference)`; `Start` is always wrapped into `[0, C)` and `Length` is clamped to `[0, C]`
    - `float Center`, `float Circumference`
    - `bool Contains(float arc)`, `bool Overlaps(ArcRange other)`
    - `static float Wrap(float arc, float c)`, `static float Distance(float a, float b, float c)` (shortest, ≥ 0), `static float SignedDelta(float from, float to, float c)` (in `(-C/2, C/2]`)
  - `class PlanetFrame(Vector2 center, float radius)`:
    - `Vector2 Center { get; set; }`, `float Radius`, `float Circumference`
    - `PlanetCoord ToCoord(Vector2 world)`, `Vector2 ToWorld(PlanetCoord coord)`
    - `Quaternion UprightRotation(float arc)` (local up = outward normal)

- [ ] **Step 1: Create both asmdefs** per Global Constraints. In Unity, a folder with a tests asmdef needs `"defineConstraints": ["UNITY_INCLUDE_TESTS"]`, `"overrideReferences": true`, `"precompiledReferences": ["nunit.framework.dll"]`, `"includePlatforms": ["Editor"]`.

- [ ] **Step 2: Write failing tests**

```csharp
[TestFixture] public class ArcRangeTests {
  const float C = 48f;
  [Test] public void Wrap_NegativeAndOverflow() { Assert.AreEqual(47f, ArcRange.Wrap(-1f, C), 1e-4); Assert.AreEqual(1f, ArcRange.Wrap(49f, C), 1e-4); }
  [Test] public void Distance_IsShortestWay() => Assert.AreEqual(2f, ArcRange.Distance(47f, 1f, C), 1e-4);
  [Test] public void SignedDelta_AcrossSeam() => Assert.AreEqual(2f, ArcRange.SignedDelta(47f, 1f, C), 1e-4);
  [Test] public void Centered_AcrossSeam_ContainsBothSides() {
    var r = ArcRange.Centered(0.1f, 2f, C);
    Assert.IsTrue(r.Contains(47.5f)); Assert.IsTrue(r.Contains(0.9f)); Assert.IsFalse(r.Contains(2f));
    Assert.AreEqual(0.1f, r.Center, 1e-4); }
  [Test] public void Overlaps_AcrossSeam() {
    Assert.IsTrue(ArcRange.Centered(0f, 2f, C).Overlaps(ArcRange.Centered(47.5f, 1f, C)));
    Assert.IsFalse(ArcRange.Centered(0f, 2f, C).Overlaps(ArcRange.Centered(3f, 1f, C))); }
  [Test] public void Overlaps_TouchingEdgesDoNotOverlap() =>
    Assert.IsFalse(new ArcRange(0f, 2f, C).Overlaps(new ArcRange(2f, 2f, C)));
}
[TestFixture] public class PlanetFrameTests {
  [Test] public void RoundTrip() { var f = new PlanetFrame(Vector2.zero, 10f);
    var w = f.ToWorld(new PlanetCoord(5f, 2f)); var c = f.ToCoord(w);
    Assert.AreEqual(5f, c.Arc, 1e-3); Assert.AreEqual(2f, c.Height, 1e-3); }
  [Test] public void ArcZero_IsPlusX() => Assert.AreEqual(new Vector2(10f, 0f), new PlanetFrame(Vector2.zero, 10f).ToWorld(new PlanetCoord(0f, 0f)));
  [Test] public void ToCoord_AtCenter_IsFinite() { var c = new PlanetFrame(Vector2.zero, 10f).ToCoord(Vector2.zero);
    Assert.IsFalse(float.IsNaN(c.Arc)); Assert.AreEqual(0f, c.Arc); Assert.AreEqual(-10f, c.Height, 1e-4); }
  [Test] public void ToCoord_FarAway_StillMapsToArc() { var f = new PlanetFrame(new Vector2(3, 3), 10f);
    Assert.AreEqual(f.Circumference / 4f, f.ToCoord(new Vector2(3, 503)).Arc, 1e-2); }
  [Test] public void UprightRotation_PointsOutward() {
    var f = new PlanetFrame(Vector2.zero, 10f);
    Vector2 up = f.UprightRotation(f.Circumference / 4f) * Vector3.up;
    Assert.AreEqual(0f, up.x, 1e-4); Assert.AreEqual(1f, up.y, 1e-4); }
}
```

- [ ] **Step 3: Run with filter `PocketPlanet.Planet.Tests`.** Expected: compile failure on missing `ArcRange`.

- [ ] **Step 4: Implement.** `Overlaps` treats ranges as half-open `[start, start+length)`. `Contains(a) = Wrap(a − Start) < Length` (a zero-length range contains nothing). `Overlaps(o) = Contains(o.Start) || o.Contains(Start)`. `ToCoord`: angle = `Atan2`; a zero-length offset gives angle 0. Arc = `Wrap(angle·R)`, height = `|offset| − R`.

- [ ] **Step 5: Run tests.** Expected: exit 0.

- [ ] **Step 6: Commit** — `git add Assets/Scripts/Planet Assets/Tests && git commit -m "Add planet assemblies and arc coordinates"` (include `.meta` files Unity generated; if Unity has not run, the batch test run generates them).

---

### Task 2: Settings, biomes and noise

**Files:**
- Create: `Generation/SeededNoise.cs`, `PlanetSettings.cs`, `BiomeDefinition.cs`, `BandDefinition.cs`, `BiomeTable.cs`
- Test: `Assets/Tests/EditMode/Planet/TestPlanets.cs` (helper), `SeededNoiseTests.cs`, `BiomeTableTests.cs`

**Interfaces:**
- Produces:
  - `class SeededNoise(int seed)`: `float Value(float x, float y)` (smooth value noise in [0,1]) and `float Fractal(float x, float y, int octaves)` (persistence 0.5, lacunarity 2, normalised to [0,1]).
  - `BiomeKind { Land, Water }`.
  - `BiomeDefinition : ScriptableObject` with public fields `Id`, `DisplayName`, `Kind`, `NormalColor`, `DoomColor`, `HallowedColor`, `Vector2 TemperatureRange`, `Vector2 MoistureRange`, `bool Buildable`, `string[] Tags`; `bool HasTag(string)`.
  - `[Serializable] class BandDefinition { string Name; float Min; float Max; }`.
  - `PlanetSettings : ScriptableObject` fields exactly as in the spec table, with the Global Constraints defaults. Also `float Radius => Circumference / (2π)`, `int GridSize => CeilToInt(2·Radius / BlockSize)`, `float SampleSpacing => SurfaceSampleSpacing · BlockSize`.
  - `class BiomeTable(IReadOnlyList<BiomeDefinition> biomes)`:
    - `const string ShallowWaterId = "ShallowWater"`, `DeepWaterId = "DeepWater"`
    - `int Count`, `BiomeDefinition this[byte index]`
    - `byte ShallowWater`, `byte DeepWater`
    - `byte ClassifyLand(float temperature, float moisture)`
    - `bool IsLand(byte index)`
    - throws `ArgumentException` if the list is empty, lacks either water id, or has no land biome
  - `static class TestPlanets` (tests only):
    - `PlanetSettings Settings(int seed = 1, float circumference = 48f, int continents = 3, float coverage = 0.6f)`, which creates the in-memory default biome set and bands
    - `PlanetModel Generate(...)` with the same parameters, filled in after Task 8

- [ ] **Step 1: Write failing tests**

```csharp
public class SeededNoiseTests {
  [Test] public void SameSeed_SameValues() => Assert.AreEqual(new SeededNoise(7).Fractal(1.3f, 2.1f, 4), new SeededNoise(7).Fractal(1.3f, 2.1f, 4));
  [Test] public void DifferentSeed_DifferentValues() => Assert.AreNotEqual(new SeededNoise(7).Value(1.3f, 2.1f), new SeededNoise(8).Value(1.3f, 2.1f));
  [Test] public void Range_IsZeroToOne() { var n = new SeededNoise(3);
    for (int i = 0; i < 1000; i++) { var v = n.Fractal(i * 0.37f, i * 0.11f, 4); Assert.That(v, Is.InRange(0f, 1f)); } }
}
public class BiomeTableTests {
  [Test] public void Classify_Cold_IsSnow() { var t = new BiomeTable(TestPlanets.Settings().BiomeTable); Assert.AreEqual("Snow", t[t.ClassifyLand(0.05f, 0.5f)].Id); }
  [Test] public void Classify_HotDry_IsDesert() { var t = new BiomeTable(TestPlanets.Settings().BiomeTable); Assert.AreEqual("Desert", t[t.ClassifyLand(0.9f, 0.1f)].Id); }
  [Test] public void Classify_NoMatch_FallsBackToFirstLand() { var t = new BiomeTable(TestPlanets.Settings().BiomeTable);
    Assert.AreEqual("Grassland", t[t.ClassifyLand(2f, 2f)].Id); }
  [Test] public void MissingWater_Throws() => Assert.Throws<ArgumentException>(() =>
    new BiomeTable(TestPlanets.Settings().BiomeTable.Where(b => b.Id != "DeepWater").ToList()));
}
```

- [ ] **Step 2: Run with filter `PocketPlanet.Planet.Tests`.** Expected: compile failure.

- [ ] **Step 3: Implement.** Default biome ranges (temperature, moisture), used both by `TestPlanets` and by the asset factory in Task 12, so put them in one static `DefaultBiomes.Create()` in `Generation/`:
  - Snow (0–0.25, 0–1)
  - Desert (0.65–1, 0–0.35)
  - Forest (0.25–0.65, 0.55–1)
  - Grassland (0–1, 0–1, listed first among land so it is the fallback)

  Lookup order is Snow, Desert, Forest, then Grassland. Order the list as Grassland, Forest, Desert, Snow, ShallowWater, DeepWater, and have `ClassifyLand` scan the land biomes except the first, falling back to the first. Colours:
  - Grassland: normal `#00AE2F` (from the user's asset)
  - ShallowWater: normal `#63B3F2` (from the user's asset)
  - Other normal colours: designer's choice in the same flat style
  - Doom colours: ash/charcoal for land, lava red-orange for water
  - Hallowed colours: more saturated and brighter versions of the normal colours

  `Tags`: Grassland `Fertile`, Forest `Fertile`, Desert `Arid`, Snow `Cold`, water `Water`. `SeededNoise`: hash the integer lattice with the seed, smoothstep-interpolate.

- [ ] **Step 4: Run tests.** Expected: exit 0.
- [ ] **Step 5: Commit** — "Add planet settings, biome table and seeded noise".

---

### Task 3: Rim layout and block map generation

**Files:**
- Create: `Generation/RimLayout.cs`, `BlockMap.cs`, `BlockMapGenerator.cs`
- Test: `RimLayoutTests.cs`, `BlockMapGeneratorTests.cs`

**Interfaces:**
- Consumes: `ArcRange`, `SeededNoise`, `PlanetSettings`, `BiomeTable` (Tasks 1–2).
- Produces:
  - `static class RimLayout`: `IReadOnlyList<ArcRange> BuildLandArcs(PlanetSettings s, System.Random rng)`, sorted by `Start`.
  - `class BlockMap(int size, float blockSize)`:
    - `const byte Empty = 255`
    - `int Size`, `float BlockSize`
    - `byte this[int x, int y]` (get/set)
    - `Vector2 BlockCenter(int x, int y)` (local, planet centre = origin)
    - `bool TryGetBlock(Vector2 local, out int x, out int y)`
  - `static class BlockMapGenerator`: `BlockMap Generate(PlanetSettings s, BiomeTable biomes, IReadOnlyList<ArcRange> landArcs, int seed)`.

- [ ] **Step 1: Write failing tests**

```csharp
public class RimLayoutTests {
  [TestCase(1)] [TestCase(3)] [TestCase(8)]
  public void ArcCount_EqualsContinentCount(int n) => Assert.AreEqual(n, RimLayout.BuildLandArcs(TestPlanets.Settings(continents: n, circumference: 120f), new System.Random(1)).Count);
  [TestCase(1)] [TestCase(2)] [TestCase(3)]
  public void LandLength_MatchesCoverage(int seed) { var s = TestPlanets.Settings(seed);
    Assert.AreEqual(0.6f * s.Circumference, RimLayout.BuildLandArcs(s, new System.Random(seed)).Sum(a => a.Length), 1e-2); }
  [Test] public void ArcsDoNotOverlap_AndGapsAtLeastThreeBlocks() { var s = TestPlanets.Settings(continents: 5);
    var arcs = RimLayout.BuildLandArcs(s, new System.Random(4));
    for (int i = 0; i < arcs.Count; i++) { var next = arcs[(i + 1) % arcs.Count];
      var gap = ArcRange.Wrap(next.Start - (arcs[i].Start + arcs[i].Length), s.Circumference);
      Assert.GreaterOrEqual(gap, 3f * s.BlockSize - 1e-3); Assert.GreaterOrEqual(arcs[i].Length, 3f * s.BlockSize - 1e-3); } }
  [Test] public void TinyPlanet_ClampsContinentCount() { var s = TestPlanets.Settings(circumference: 10f, continents: 8);
    LogAssert.Expect(LogType.Warning, new Regex("ContinentCount"));
    var arcs = RimLayout.BuildLandArcs(s, new System.Random(1));
    Assert.That(arcs.Count, Is.InRange(1, 2)); }
}
public class BlockMapGeneratorTests {
  [Test] public void SameSeed_IdenticalMap() { /* generate twice with seed 5; assert every cell equal */ }
  [Test] public void DifferentSeed_DifferentMap() { /* seeds 5 and 6; assert at least one cell differs */ }
  [Test] public void OutsideCircle_IsEmpty_InsideIsNot() { /* corner (0,0) == Empty; centre cell != Empty */ }
  [Test] public void GridGrowsWithCircumference() => Assert.Greater(TestPlanets.Settings(circumference: 200f).GridSize, TestPlanets.Settings(circumference: 48f).GridSize);
}
```

(The bodies of the three commented tests are the single assertion each comment states. Write them out in full.)

- [ ] **Step 2: Run with filter `PocketPlanet.Planet.Tests`.** Expected: compile failure.

- [ ] **Step 3: Implement `RimLayout.BuildLandArcs`.** The algorithm:

```
minArc = 3 * BlockSize
maxCount = floor(C / (2 * minArc)); n = clamp(ContinentCount, 1, max(1, maxCount)); warn "ContinentCount clamped" if n < ContinentCount
land = C * LandCoverage; ocean = C - land
landLen[i]  = distribute(land,  n, jitter ±25%, min minArc)   // jitter, renormalise to sum, then raise any < min and take the excess from the largest
oceanLen[i] = distribute(ocean, n, jitter ±25%, min minArc)
cursor = rng.NextDouble() * C
for i: arcs.Add(new ArcRange(cursor, landLen[i], C)); cursor += landLen[i] + oceanLen[i]
return arcs sorted by Start
```

If `land < n·minArc` or `ocean < n·minArc`, reduce `n` further until both fit.

- [ ] **Step 4: Implement `BlockMapGenerator.Generate`.**
  - **Inside the disc:** a block is inside if its centre is within the radius.
  - **Land score** for a block at local `p` with `r = |p|`, `arc = angle·R`:
    - inland part: `inland = 1 − (R − r) / (0.45·R)`
    - `score = max over arcs of (arcMembership(arc) · inland) + (Fractal(p·0.15, 4) − 0.5) · CoastRoughness + islandBonus`
    - `arcMembership = 1` inside the arc, falling off linearly to 0 over `2·BlockSize` outside it
    - `islandBonus = 0.6` where a second noise field (`Fractal(p·0.25 + 100)`) exceeds 0.78, else 0
    - land if `score > 0.5`
  - **Force the rim:** every block hit by sampling the ring at radius `R − BlockSize/2` every `SampleSpacing` is set to land iff its centre's arc lies in a land arc.
  - **Biomes:**
    - land: `ClassifyLand(Fractal(p·0.08 + 300), Fractal(p·0.08 + 600))`
    - water: `ShallowWater` if any of the 8 neighbours is land, else `DeepWater`

- [ ] **Step 5: Run tests.** Expected: exit 0.
- [ ] **Step 6: Commit** — "Generate rim continent layout and blocky biome map".

---

### Task 4: Surface profile

**Files:**
- Create: `Layers/TerrainKind.cs`, `SurfaceSample.cs`, `SurfaceSegment.cs`, `SurfaceProfile.cs`
- Test: `SurfaceProfileTests.cs`

**Interfaces:**
- Consumes: `BlockMap`, `BiomeTable`, `ArcRange`.
- Produces:
  - `enum TerrainKind { Land, Water }`
  - `readonly struct SurfaceSample(TerrainKind terrain, byte biome, bool isCoast)`
  - `readonly struct SurfaceSegment(ArcRange range, TerrainKind terrain, byte biome)`
  - `class SurfaceProfile`:
    - `static SurfaceProfile Build(BlockMap map, BiomeTable biomes, float radius, float spacing)`
    - `float Spacing`, `float Circumference`, `int SampleCount`
    - `SurfaceSample SampleAt(float arc)`, `TerrainKind TerrainAt(float arc)`, `byte BiomeAt(float arc)`
    - `IEnumerable<SurfaceSample> SamplesIn(ArcRange range)`, which includes every sample whose centre lies in the range, plus at least the sample at `range.Center`
    - `bool IsAll(ArcRange range, Func<SurfaceSample, bool> predicate)`
    - `IReadOnlyList<SurfaceSegment> Segments`
    - `internal SurfaceProfile(SurfaceSample[] samples, float spacing)` for hand-built test profiles; add `[assembly: InternalsVisibleTo("PocketPlanet.Planet.Tests")]` in `Assets/Scripts/Planet/AssemblyInfo.cs`

- [ ] **Step 1: Write failing tests** in `SurfaceProfileTests`. Build from a generated planet (`TestPlanets` settings → `RimLayout` + `BlockMapGenerator`, seed 2):
  - `LandFraction_WithinTolerance`: the land samples ÷ `SampleCount` is within `0.03 + ContinentCount·2·BlockSize/Circumference` of 0.6 for seeds 1–5, with Circumference 120. The second term allows for block quantisation at each coastline; see the note at the end of the plan.
  - `LandSegments_EqualContinentCount`: count the land runs, treating a run that crosses the seam as one, for counts 1, 3 and 8 across seeds 1–3, with Circumference 120.
  - `Segments_CoverWholeCircumference`: the sum of segment lengths equals the circumference (±1e-3), and neighbouring segments differ in terrain or biome.
  - `CoastFlag_SetOnlyAtTerrainChange`: for every sample, `IsCoast == (prev.Terrain != Terrain || next.Terrain != Terrain)`.
  - `SamplesIn_TinyRange_ReturnsCenterSample`: a range of width 0.01 yields exactly 1 sample.

- [ ] **Step 2: Run tests.** Expected: compile failure.
- [ ] **Step 3: Implement.**
  - `SampleCount = round(C / spacing)`; sample `i` sits at arc `(i + 0.5)·spacing`.
  - Each sample reads the block at radius `R − BlockSize/2` via `TryGetBlock`. An `Empty` block there counts as `DeepWater`.
  - Segments merge consecutive equal (terrain, biome) samples, rotated so no segment is split by the seam unless the whole rim is one segment.
- [ ] **Step 4: Run tests.** Expected: exit 0.
- [ ] **Step 5: Commit** — "Add rim surface profile and segments".

---

### Task 5: Occupancy tracks and the surface layer

**Files:**
- Create: `Layers/IPlanetOccupant.cs`, `OccupancyTrack.cs`, `SurfaceRow.cs`, `SurfaceLayer.cs`
- Test: `OccupancyTrackTests.cs`

**Interfaces:**
- Produces:
  - `enum OccupantKind { Building, ConstructionSite, Prop }`
  - `interface IPlanetOccupant { string Id { get; } OccupantKind Kind { get; } IReadOnlyCollection<string> Tags { get; } }`
  - `class OccupancyTrack(float circumference)`:
    - `bool TryAdd(IPlanetOccupant o, ArcRange range)`: false on any overlap or if `o` is already present
    - `bool Remove(IPlanetOccupant o)`
    - `bool Move(IPlanetOccupant o, ArcRange newRange)`: ignores its own old range; false and unchanged on overlap
    - `bool IsFree(ArcRange range, IPlanetOccupant ignore = null)`
    - `IEnumerable<(IPlanetOccupant Occupant, ArcRange Range)> OccupantsIn(ArcRange range)`
    - `bool TryGetRange(IPlanetOccupant o, out ArcRange range)`
    - `event Action<IPlanetOccupant, ArcRange> OccupantAdded, OccupantRemoved`
  - `enum SurfaceRow { Back, Front }`
  - `class SurfaceLayer(SurfaceProfile profile)`: `SurfaceProfile Profile`, `OccupancyTrack Row(SurfaceRow row)`

- [ ] **Step 1: Write failing tests**, using a `FakeOccupant(string id)` test class:
  - `TryAdd_Overlap_Rejected`: add A at (10, 2); B at (11, 2) → false; B at (12, 2) → true (touching edges).
  - `TryAdd_AcrossSeam_Rejected`: A = `Centered(0.1, 2)`; B = `Centered(47.5, 1)` → false.
  - `Remove_FreesRange_AndRaisesEvent`: removing A raises `OccupantRemoved` once, and `IsFree` then returns true.
  - `Move_IgnoresOwnRange`: A at (10, 2) moving to (11, 2) → true.
  - `Move_IntoOther_FailsAndKeepsOldRange`.
  - `OccupantsIn_ReturnsOnlyOverlapping`.
  - `DuplicateAdd_Rejected`.
- [ ] **Step 2: Run tests.** Expected: compile failure.
- [ ] **Step 3: Implement** with a `Dictionary<IPlanetOccupant, ArcRange>` plus a linear overlap scan. Occupant counts are in the tens to hundreds, so no tree is needed.
- [ ] **Step 4: Run tests.** Expected: exit 0.
- [ ] **Step 5: Commit** — "Add occupancy tracks and surface rows".

---

### Task 6: Underground layer (data only)

**Files:**
- Create: `Layers/UndergroundMaterial.cs`, `UndergroundLayer.cs`
- Test: `UndergroundLayerTests.cs`

**Interfaces:**
- Consumes: `SurfaceProfile`, `BiomeTable`, `BandDefinition`, `SeededNoise`, `OccupancyTrack`.
- Produces:
  - `enum UndergroundMaterial { Soil, Sand, Rock, Water, Ice }`
  - `class UndergroundLayer`:
    - `static UndergroundLayer Build(SurfaceProfile profile, BiomeTable biomes, IReadOnlyList<BandDefinition> bands, int seed)`
    - `IReadOnlyList<BandDefinition> Bands`
    - `int BandAtDepth(float depth)`: depth is positive downward; returns −1 if outside every band
    - `UndergroundMaterial MaterialAt(int band, float arc)`
    - `OccupancyTrack Track(int band)`

- [ ] **Step 1: Write failing tests:**
  - `BandAtDepth_UsesDefaults`: 0.5 → 0, 2 → 1, 10 → −1.
  - `UnderWater_TopBandIsWater`: an arc whose `TerrainAt` is Water has `MaterialAt(0, arc) == Water`.
  - `UnderSnow_TopBandIsIce`, `UnderDesert_TopBandIsSand`: find a sample with the biome; skip via `Assume.That` if the seed has none.
  - `DeepestBand_IsRockOrWater`.
  - `SameSeed_SameMaterials`.
- [ ] **Step 2: Run tests.** Expected: compile failure.
- [ ] **Step 3: Implement** the rules per sample:
  - **Band 0:** water → Water; Snow → Ice; Desert → Sand; other land → Soil.
  - **Middle bands:** Rock, or Soil where `Fractal(arc·0.3, band) > 0.6`.
  - **Last band:** Rock, or Water where `Fractal(arc·0.2 + 50, band) > 0.75` (aquifer).
  - Biome checks use `BiomeDefinition.Id`.
- [ ] **Step 4: Run tests.** Expected: exit 0.
- [ ] **Step 5: Commit** — "Add underground data layer".

---

### Task 7: Air layer

**Files:**
- Create: `Layers/IAirborne.cs`, `AirTraffic.cs`, `AirZone.cs`, `AirZones.cs`, `AirLayer.cs`
- Test: `AirLayerTests.cs`

**Interfaces:**
- Produces:
  - `interface IAirborne { string Id { get; } PlanetCoord Position { get; } Vector2 Velocity { get; } IReadOnlyCollection<string> Tags { get; } }`
  - `class AirTraffic`:
    - `bool Register(IAirborne a)`, `bool Unregister(IAirborne a)`
    - `IReadOnlyCollection<IAirborne> All`
    - `event Action<IAirborne> Registered, Unregistered`
  - `enum AirZoneKind { Storm, AshCloud, NoFly }`
  - `sealed class AirZone(ArcRange range, int band, AirZoneKind kind, IReadOnlyCollection<string> tags)`
  - `class AirZones`:
    - `void Add(AirZone z)`, `bool Remove(AirZone z)`
    - `IEnumerable<AirZone> ZonesIn(ArcRange range, int band)`
    - `event Action<AirZone> ZoneAdded, ZoneRemoved`
  - `class AirLayer(IReadOnlyList<BandDefinition> bands, float circumference)`:
    - `IReadOnlyList<BandDefinition> Bands`, `int BandAt(float height)` (−1 outside)
    - `AirTraffic Traffic`, `AirZones Zones`
    - `IEnumerable<IAirborne> InBand(int band)`, `IEnumerable<IAirborne> InRange(ArcRange range, int? band = null)`
    - `IAirborne Nearest(PlanetCoord from, Func<IAirborne, bool> filter = null)`: uses arc distance plus height difference (Euclidean in (arc, height) space)
    - `IEnumerable<AirZone> ZonesAt(PlanetCoord coord)`

- [ ] **Step 1: Write failing tests** with a `FakeAirborne(id, arc, height, tags)`:
  - `BandAt_Defaults`: 1 → 0 (Low), 3 → 1 (Flight), 20 → −1.
  - `InBand_FiltersByHeight`.
  - `InRange_AcrossSeam`.
  - `Nearest_UsesWrappedArc`: from arc 47.5, an entity at arc 0.5 beats one at arc 45.
  - `Nearest_RespectsFilter`.
  - `ZonesAt_MatchesBandAndRange`: a storm in the Flight band over (10, 4) covers (11, 3) but not (11, 1) or (20, 3).
  - `Register_Twice_ReturnsFalse`.
  - `Events_Fire`.
- [ ] **Step 2: Run tests.** Expected: compile failure.
- [ ] **Step 3: Implement.** Entity positions are read live from `IAirborne.Position` on each query; no caching.
- [ ] **Step 4: Run tests.** Expected: exit 0.
- [ ] **Step 5: Commit** — "Add air layer with traffic and zones".

---

### Task 8: Planet model, generator and cross-layer queries

**Files:**
- Create: `Model/IPlanet.cs`, `PlanetModel.cs`, `PlanetQueries.cs`, `Generation/PlanetGenerator.cs`
- Modify: `Assets/Tests/EditMode/Planet/TestPlanets.cs` (add `Generate`)
- Test: `PlanetGeneratorTests.cs`, `PlanetQueriesTests.cs`

**Interfaces:**
- Consumes: Tasks 1–7.
- Produces:
  - `interface IPlanet`: `PlanetSettings Settings`, `PlanetFrame Frame`, `BiomeTable Biomes`, `BlockMap BlockMap`, `SurfaceLayer Surface`, `UndergroundLayer Underground`, `AirLayer Air`, `PlanetQueries Queries`, `PlacementValidator Placement` (the last one is added in Task 9; leave it out of the interface until then).
  - `sealed class PlanetModel : IPlanet`: constructor takes all parts.
  - `static class PlanetGenerator`: `PlanetModel Generate(PlanetSettings settings, Vector2 center = default)`.
  - `class PlanetQueries(SurfaceLayer surface, BiomeTable biomes)`:
    - `ArcRange? FindLandingSite(float nearArc, float length, bool allowWater)`
    - `float? NearestCoast(float arc)`
    - `float? FindNearestDeepWater(float arc)`

- [ ] **Step 1: Write failing tests:**
  - `Generate_SameSeed_IdenticalLayers`: equal block maps, profile samples and underground materials.
  - `Radius_FromCircumference`: `Frame.Radius == 48 / (2π)` (±1e-4).
  - `Generate_Circumference500_Under50ms`: warm up once, then time with `Stopwatch` and assert < 50 ms.
  - `LandingSite_IsLandAndFreeInBothRows`: the result satisfies `Profile.IsAll(range, Land)` and both rows are free.
  - `LandingSite_SkipsOccupied`: occupy the first result in the Front row; the next result doesn't overlap it.
  - `LandingSite_None_ReturnsNull`: a length greater than the longest land segment, with `allowWater` false → null.
  - `LandingSite_AllowWater_FindsWaterNearOcean`: a request near the centre of a water segment returns a range inside water.
  - `NearestCoast_ReturnsCoastSampleArc`.
  - `FindNearestDeepWater_ReturnsDeepWaterArc`.
- [ ] **Step 2: Run tests.** Expected: compile failure.
- [ ] **Step 3: Implement `PlanetGenerator.Generate`** in this order:
  1. `rng = new System.Random(Seed)`
  2. `RimLayout`
  3. `BlockMapGenerator` (seed `Seed`)
  4. `SurfaceProfile` (radius `settings.Radius`, `SampleSpacing`)
  5. `UndergroundLayer` (seed `Seed + 1`)
  6. `AirLayer`
  7. `PlanetQueries`
  8. `PlanetFrame(center, Radius)`

- [ ] **Step 4: Implement `FindLandingSite`.** Search outward from `nearArc` in steps of `profile.Spacing`, alternating +d and −d, up to `C/2`. At each candidate centre `c`, take `range = Centered(c, length)` and accept it if `IsAll(range, s => s.Terrain == Land || allowWater)` and both rows are free. With `allowWater`, a fully-water range is acceptable even if the rows are occupied, since rows only hold land structures. Return the first match, else null. `NearestCoast` and `FindNearestDeepWater` use the same outward walk with a sample predicate.
- [ ] **Step 5: Run tests.** Expected: exit 0.
- [ ] **Step 6: Commit** — "Add planet model, generator and cross-layer queries".

---

### Task 9: Placement rules and validator

**Files:**
- Create: `Placement/PlacementReason.cs`, `PlacementResult.cs`, `IPlacementRule.cs`, `PlacementRules.cs`, `LandOnlyRule.cs`, `CoastalRule.cs`, `SpanningRule.cs`, `Footprint.cs`, `PlacementValidator.cs`
- Modify: `Model/IPlanet.cs`, `PlanetModel.cs`, `Generation/PlanetGenerator.cs` (add `Placement`)
- Test: `PlacementTests.cs`

**Interfaces:**
- Consumes: `SurfaceLayer`, `BiomeTable`, `ArcRange`.
- Produces:
  - `enum PlacementReason { None, Water, BiomeNotBuildable, MissingBiomeTag, ForbiddenBiomeTag, Occupied, RuleViolation }`
  - `enum RowPreference { Back, Front, Either }`
  - `readonly struct PlacementResult`: `PlacementReason Reason`, `bool IsValid => Reason == None`, `SurfaceRow Row`, `ArcRange Range`
  - `interface IPlacementRule { PlacementReason Evaluate(ArcRange range, SurfaceProfile profile, BiomeTable biomes); }`
  - `LandOnlyRule`: Water if any sample is water, BiomeNotBuildable if any land biome is not `Buildable`, else None.
  - `CoastalRule`: RuleViolation unless the range has both land and water samples; BiomeNotBuildable if a land sample isn't buildable.
  - `SpanningRule(float maxSpan)`: RuleViolation unless the first and last samples are land and the longest contiguous water run ≤ `maxSpan`; Water if there is no land at both ends.
  - `enum PlacementRuleKind { LandOnly, Coastal, Spanning }` and `static class PlacementRules { IPlacementRule Create(PlacementRuleKind kind, float maxSpan); }` (used by `PlanetFootprint` and the editor placement test)
  - `sealed class Footprint(float width, RowPreference row, IPlacementRule rule, IReadOnlyCollection<string> requiredTags = null, IReadOnlyCollection<string> forbiddenTags = null)`
  - `class PlacementValidator(SurfaceLayer surface, BiomeTable biomes)`:
    - `PlacementResult Check(Footprint f, float arc)`
    - `PlacementResult? FindNearestValid(Footprint f, float arc, float maxNudge)`
    - `ArcRange RangeFor(Footprint f, float arc)`

- [ ] **Step 1: Write failing tests.** Pick arcs from the generated profile's segments: the centre of the longest land segment, the centre of a water segment, and a coast boundary.
  - `LandOnly_OnLand_Valid_FrontRow`
  - `LandOnly_OnWater_ReasonWater`
  - `LandOnly_UnbuildableBiome_ReasonBiomeNotBuildable` (flip `Buildable` false on Grassland in the test settings)
  - `RequiredTag_Missing_ReasonMissingBiomeTag`
  - `ForbiddenTag_Present_ReasonForbiddenBiomeTag`
  - `Coastal_AtCoast_Valid`
  - `Coastal_Inland_RuleViolation`
  - `Spanning_NarrowStrait_Valid` and `Spanning_TooWide_RuleViolation` (use a hand-built profile from Task 4's internal constructor: land, 4 water samples, land)
  - `Either_FrontTaken_FallsBackToBack`
  - `Either_BothTaken_ReasonOccupied`
  - `Order_RuleBeforeTagsBeforeOccupancy`: an occupied water spot reports `Water`, not `Occupied`
  - `AcrossSeam_OccupiedDetected`: footprint centred at 0.1 overlaps an occupant at `Centered(C − 0.3, 1)` → Occupied
  - `FindNearestValid_NudgesOffOccupant`: occupy the land centre; a nudge of 3 finds a valid spot with `Distance ≤ 3`
  - `FindNearestValid_NoRoom_ReturnsNull`: width larger than the longest land segment → null
  - `FindNearestValid_FullyOccupied_ReturnsNull`: fill both rows of every land segment → null
- [ ] **Step 2: Run tests.** Expected: compile failure.
- [ ] **Step 3: Implement.**
  - `Check`: rule → biome tags (any land sample under the footprint must have every required tag and no forbidden tag) → row resolution (Front, Back or Either: Front first, then Back) → Occupied.
  - `FindNearestValid`: walks outward in `Spacing` steps, alternating sides, up to `maxNudge`, and returns the first valid `Check`.
  - Add `PlacementValidator Placement` to `IPlanet` and `PlanetModel`, and build it in `PlanetGenerator`.
- [ ] **Step 4: Run tests.** Expected: exit 0.
- [ ] **Step 5: Commit** — "Add data-driven placement rules and validator".

---

### Task 10: Planet rendering and mood palette

**Files:**
- Create: `Rendering/BiomePalette.cs`, `PlanetView.cs`, `PlanetSurface.shader`
- Test: `BiomePaletteTests.cs`

**Interfaces:**
- Consumes: `IPlanet`, `BiomeTable`, `BlockMap`.
- Produces:
  - `static class BiomePalette`:
    - `Color Evaluate(BiomeDefinition b, float mood)`: mood < 0 lerps Normal→Doom by −mood; mood > 0 lerps Normal→Hallowed by mood; clamped to [−1, 1]
    - `Texture2D BuildTexture(BiomeTable t)`: width = count, height 3; row 0 doom, 1 normal, 2 hallowed; point filter; clamp
    - `Texture2D BuildBiomeMap(BlockMap m)`: R8, `Size×Size`, value = index/255; Empty = 255
  - `class PlanetView : MonoBehaviour`:
    - `[SerializeField] Material material`
    - `void Render(IPlanet planet)`: builds a quad of side `2·Radius` centred on the transform, assigns the textures and sets `_BiomeCount`
    - `void SetMood(float mood)`
    - `void RefreshPalette(BiomeTable t)`
  - Shader `PocketPlanet/PlanetSurface`:
    - properties `_BiomeMap`, `_Palette`, `_BiomeCount`, `_Mood` (Range −1..1)
    - URP unlit, transparent, `Blend SrcAlpha OneMinusSrcAlpha`, `ZWrite Off`

- [ ] **Step 1: Write failing tests:**
  - `Evaluate_MinusOne_IsDoom`, `Evaluate_Zero_IsNormal`, `Evaluate_PlusOne_IsHallowed`, `Evaluate_Half_IsMidpointToHallowed`
  - `Evaluate_ClampsOutOfRange` (mood 3 == mood 1)
  - `BuildTexture_RowsMatchColors` (pixel (i, 0/1/2) equals doom/normal/hallowed for each biome)
  - `BuildBiomeMap_EmptyIs255`
- [ ] **Step 2: Run tests.** Expected: compile failure.
- [ ] **Step 3: Implement `BiomePalette`** to pass. Then write `PlanetView` and the shader. The fragment logic, which must match `BiomePalette.Evaluate`:

```hlsl
float2 p = uv * 2 - 1;                     // quad UV → [-1,1], circle radius 1
float d = length(p);
float edge = fwidth(d);
float alpha = 1 - smoothstep(1 - edge, 1, d);
float idx = round(SAMPLE_TEXTURE2D(_BiomeMap, point_clamp, uv).r * 255);
clip(alpha - 0.001); if (idx >= 255) discard;
float u = (idx + 0.5) / _BiomeCount;
half4 doom = SAMPLE(_Palette, float2(u, 1.0/6)), normal = SAMPLE(_Palette, float2(u, 3.0/6)), hallow = SAMPLE(_Palette, float2(u, 5.0/6));
half4 col = _Mood < 0 ? lerp(normal, doom, -_Mood) : lerp(normal, hallow, _Mood);
return half4(col.rgb, alpha);
```

The quad is built in the XY plane: the game is 2D, camera looking down +Z.

- [ ] **Step 4: Run tests.** Expected: exit 0. The shader compiles when the batch run imports it: `Logs/test-run.log` contains no `Shader error in 'PocketPlanet/PlanetSurface'`.
- [ ] **Step 5: Commit** — "Add planet view, surface shader and mood palette".

---

### Task 11: World mood controller and effects

**Files:**
- Create: `Mood/IWorldMoodSource.cs`, `IWorldMoodEffect.cs`, `ManualMoodSource.cs`, `MoodSmoother.cs`, `WorldMoodController.cs`, `PlanetPaletteMoodEffect.cs`, `MoodGrading.cs`, `GlobalColorMoodEffect.cs`
- Test: `MoodTests.cs`

**Interfaces:**
- Produces:
  - `interface IWorldMoodSource { float TargetMood { get; } }`
  - `interface IWorldMoodEffect { void Apply(float mood); }`
  - `ManualMoodSource : MonoBehaviour, IWorldMoodSource` with `[Range(-1,1)] float mood`
  - `static class MoodSmoother`: `float Step(float current, float target, float deltaTime, float unitsPerSecond)` (`MoveTowards`, result clamped to [−1, 1])
  - `WorldMoodController : MonoBehaviour`:
    - `[SerializeField] MonoBehaviour source` (must implement `IWorldMoodSource`; validated in `OnValidate` with an error log)
    - `[SerializeField] MonoBehaviour[] effects` (each must implement `IWorldMoodEffect`)
    - `[SerializeField] float unitsPerSecond = 0.25f`
    - `float CurrentMood`, `event Action<float> MoodChanged` (raised only when the value changes)
    - in the editor, applies instantly via `[ExecuteAlways]` so the slider previews live
  - `PlanetPaletteMoodEffect : MonoBehaviour, IWorldMoodEffect` (`[SerializeField] PlanetView view`)
  - `readonly struct MoodGrading(float saturation, Color filter)` with `static MoodGrading Evaluate(float mood)`:
    - saturation: −1 → −40, 0 → 0, +1 → +35, linear in each half
    - filter: lerp white → `(1, 0.72, 0.6)` for doom; white → `(1, 0.97, 0.88)` for hallowed
  - `GlobalColorMoodEffect : MonoBehaviour, IWorldMoodEffect` (`[SerializeField] Volume volume`): gets or adds `ColorAdjustments` on `volume.profile` and sets `saturation` and `colorFilter` (overrideState true) from `MoodGrading.Evaluate`

- [ ] **Step 1: Write failing tests:**
  - `Smoother_MovesTowardsTarget_AtRate`: `Step(0, 1, 0.5, 0.25) == 0.125`
  - `Smoother_DoesNotOvershoot`
  - `Smoother_Clamps`
  - `Grading_Endpoints`: saturation −40, 0, 35 and filters as specified
  - `Grading_IsMonotonic`: saturation increases from −1 to +1 in steps of 0.1
  - `Controller_RaisesMoodChanged_AndAppliesEffects`: EditMode test with a `GameObject`, a `ManualMoodSource`, a test `RecordingEffect : MonoBehaviour, IWorldMoodEffect` and a public `Tick(float dt)` method on the controller, called by `Update`, so the test can drive it
- [ ] **Step 2: Run tests.** Expected: compile failure.
- [ ] **Step 3: Implement.**
- [ ] **Step 4: Run tests.** Expected: exit 0.
- [ ] **Step 5: Commit** — "Add world mood axis with palette and colour grading effects".

---

### Task 12: Planet host, editor tooling and assets

**Files:**
- Create: `Unity/PlanetHost.cs`
- Create: `Editor/PocketPlanet.Planet.Editor.asmdef`, `Editor/PlanetHostEditor.cs`, `Editor/PlanetSetup.cs`
- Generated by `PlanetSetup`:
  - `Assets/Planet/DefaultPlanetSettings.asset`
  - `Assets/Planet/Biomes/*.asset`
  - `Assets/Planet/PlanetSurface.mat`
  - `Assets/Planet/ProceduralPlanet.prefab`
  - `Assets/Planet/PlanetMoodVolume.asset`
- Test: `PlanetHostTests.cs`

**Interfaces:**
- Consumes: `PlanetGenerator`, `PlanetView`, `WorldMoodController`.
- Produces:
  - `[ExecuteAlways] class PlanetHost : MonoBehaviour`:
    - `[SerializeField] PlanetSettings settings`, `[SerializeField] PlanetView view`
    - `static PlanetHost Active` (set in `OnEnable`, cleared in `OnDisable` if it is this host)
    - `IPlanet Planet { get; }` (generated lazily on first access, and in `Awake`/`OnValidate`)
    - `event Action<IPlanet> Generated`
    - `void Regenerate()`: generates, renders the view, sizes a `CircleCollider2D` on the same object (tag `Water`, radius = `Radius`), and keeps `Planet.Frame.Center` in sync with `transform.position` in `LateUpdate`
    - `OnDrawGizmos`: rim segments coloured by terrain, occupied ranges per row (Front drawn 0.2 above the rim, Back 0.4 above), air band circles, active air zones
  - `PlanetHostEditor`:
    - **Regenerate** button
    - a "Placement test" foldout: arc slider, width field, `RowPreference`, a rule dropdown (LandOnly, Coastal, Spanning), and a label showing `PlacementResult`; the tested range is drawn in the Scene view
  - `static class PlanetSetup` (callable with `-executeMethod`):
    - `CreateDefaultAssets()`: biomes from `DefaultBiomes.Create()`, the settings asset, the material using `PocketPlanet/PlanetSurface`, a Volume profile, and the prefab (`PlanetHost` + `PlanetView` + `CircleCollider2D` + `WorldMoodController` + `ManualMoodSource` + `PlanetPaletteMoodEffect` + `GlobalColorMoodEffect`, with a child global `Volume` using the profile)
    - `ReplacePlanetInScene(string scenePath, string oldObjectName)` (used in Task 13)

- [ ] **Step 1: Write failing tests**, in EditMode:
  - `Host_Generates_OnFirstAccess`: a new `GameObject` with `PlanetHost` and test settings has non-null `Planet` and a `CircleCollider2D` radius equal to `Radius`.
  - `Host_Active_IsSetAndCleared`.
  - `Host_FrameFollowsTransform`: move the transform and call `LateUpdate` via a public `SyncFrame()` method; `Frame.Center` equals the new position.
  - `Regenerate_RaisesGenerated`.
- [ ] **Step 2: Run tests.** Expected: compile failure.
- [ ] **Step 3: Implement `PlanetHost`, `PlanetHostEditor` and `PlanetSetup`.**
- [ ] **Step 4: Run tests.** Expected: exit 0.
- [ ] **Step 5: Generate the assets.**

```bash
"/c/Program Files/Unity/Hub/Editor/6000.6.5f1/Editor/Unity.exe" -batchmode -nographics -quit -projectPath "W:/Projects/PocketPlanet/Pocketplanet" -executeMethod PocketPlanet.Planet.Editor.PlanetSetup.CreateDefaultAssets -logFile "W:/Projects/PocketPlanet/Pocketplanet/Logs/setup.log"
```

Expected: exit 0, and `ls Assets/Planet` shows the settings, material, prefab, volume profile and a `Biomes` folder with 6 assets.
- [ ] **Step 6: Commit** — "Add planet host, editor tooling and default planet assets".

---

### Task 13: Integrate with the existing game

**Files:**
- Create: `Unity/PlanetFootprint.cs`, `Unity/PlanetOccupant.cs`, `Unity/AirborneAgent.cs`
- Modify:
  - `Assets/Scripts/Other/Planet.cs` (whole file)
  - `GameManagers/ConstructionHandler.cs:101-293`
  - `ObjectSpawner.cs:868-1004`
  - `Buildings/Building_ConstructionSite.cs:37-52`
  - `Buildings/Building.cs:614-717`
  - `Citizens/Citizens/Citizen_Blockman.cs:44-57`
  - `Controls/CameraControls.cs:44,85-87`
  - `ConstructionHandler.RemoveForUpgradeBuilding`
- Modify scenes: `Assets/Scenes/Main.unity`, `Assets/Scenes/Development_Main.unity`
- Delete: `Assets/Scripts/WorldGeneration/` (+ `.meta`), `Assets/Editor/MapGeneratorEditor.cs` (+ `.meta`), `Assets/Scenes/WorldGeneration/` (+ `.meta`)
- Test: `IntegrationTests.cs`

**Interfaces:**
- Consumes: `PlanetHost.Active`, `IPlanet`, `PlacementValidator`, `Footprint`, `OccupancyTrack`, `AirTraffic`.
- Produces:
  - `PlanetFootprint : MonoBehaviour`:
    - fields `float width` (0 = auto), `RowPreference row = Front`, `PlacementRuleKind rule = LandOnly`, `float maxSpan = 3`, `string[] requiredBiomeTags`, `string[] forbiddenBiomeTags`
    - `Footprint ToFootprint()`: auto width = the arc width of the `SpriteRenderer` bounds in children, or 1 if there are none
    - `static Footprint For(GameObject prefab)` (default Footprint when the component is missing)
  - `PlanetOccupant : MonoBehaviour, IPlanetOccupant`:
    - `bool Reserve(PlacementResult result)` (adds to the row track)
    - `bool TransferTo(PlanetOccupant next)` (removes self, then adds `next` with the same row and range)
    - `void Release()`
    - `OnDestroy` → `Release()`
    - `Kind` set from the presence of `Building_ConstructionSite`; `Id` from `GetInstanceID()`
  - `AirborneAgent : MonoBehaviour, IAirborne`: registers on `OnEnable` and unregisters on `OnDisable`; `Position` comes from `PlanetHost.Active.Planet.Frame.ToCoord(transform.position)`; `Velocity` is from the frame delta; `[SerializeField] string[] tags`.
  - Legacy `Planet` static API, same signatures:
    - `Radius => Host.Planet.Frame.Radius`
    - `PlanetObject => Host.gameObject`
    - `FindPointOnSurface(radius, position)` → `Frame.ToWorld(new PlanetCoord(Frame.ToCoord(position).Arc, radius − Frame.Radius))`
    - `PlaceObjectOnSurface`, `RotateObjectToSurface` use `Frame.UprightRotation`
    - `GetBlockzillaSpawnpoint(position)` returns a cached child transform moved to `FindNearestDeepWater`, falling back to the input position's surface point when there is no deep water
    - where `Host` is `PlanetHost.Active`, throwing `InvalidOperationException("No PlanetHost in scene")` if missing
  - `Planet` becomes `public static class Planet`. Its only scene use was via `Planet.prefab`, which Step 7 removes from the scenes.

- [ ] **Step 1: Write failing tests** in `IntegrationTests` (EditMode, building GameObjects in code):
  - `LegacyPlanet_FindPointOnSurface_SnapsFromAnywhere`: from `(0, 0.001)` and from `(0, 500)`, the result has magnitude `Radius` and arc `C/4`.
  - `LegacyPlanet_RotateObjectToSurface_UpIsOutward`.
  - `Occupant_ReserveAndRelease`.
  - `Occupant_TransferKeepsRange`: site → building gives the same range, and the site is no longer present.
  - `UpgradeInPlace_ReleaseThenReserve_Succeeds`: reserve A at a land arc, then `Release` A, then reserve B at the same arc → true. A variant without the release → false (`Occupied`).
  - `Airborne_RegistersWhileEnabled`.
  - `Footprint_AutoWidthFromSprite` (a `SpriteRenderer` with a 2-unit-wide sprite created in code at radius R gives a width ≈ 2).

  `PlanetHost` and `PlanetFootprint` live in `PocketPlanet.Planet`, but `Planet.cs` is in `Assembly-CSharp`, which test assemblies cannot reference. So move the static adapter's logic into `PocketPlanet.Planet` as `public static class LegacyPlanetApi` (same methods), test that, and make `Planet.cs` a one-line forwarder per member.

- [ ] **Step 2: Run tests.** Expected: compile failure.
- [ ] **Step 3: Implement** `PlanetFootprint`, `PlanetOccupant`, `AirborneAgent`, `LegacyPlanetApi`, and the `Planet.cs` forwarder.
- [ ] **Step 4: Rewire building placement.**
  - **`ObjectSpawner`:** `ValidBuildPosition` → `PlanetHost.Active.Planet.Placement.Check(PlanetFootprint.For(prefab), arc).IsValid`, where arc comes from `Frame.ToCoord(transform.position).Arc`. Delete `AppropriateDistanceToPlanet` from the building path (keep it for events).
  - **`ConstructionHandler`:**
    - every `CreateNewBuilding*` overload: resolve via `FindNearestValid(footprint, arc, maxNudge: 1.5f)`; if null → return false, or for void overloads return without placing
    - `PlaceConstructionSite(prefab, site, PlacementResult)` positions at `Frame.ToWorld(new PlanetCoord(result.Range.Center, 0))` and calls `Reserve`
    - delete `InWater`, `Intersecting` and `FindRandomPointOnSurface`; the random arc uses `UnityEngine.Random.value · C`, since gameplay randomness may use Unity's RNG
    - `RemoveForUpgradeBuilding` calls `building.GetComponent<PlanetOccupant>().Release()` before anything else
  - **`Building_ConstructionSite.ConstructBuilding`:** the new building gets a `PlanetOccupant` and `GetComponent<PlanetOccupant>().TransferTo(newOccupant)` is called.
  - **`Building.Kill`:** `GetComponent<PlanetOccupant>()?.Release()`.
  - **Rows:** add `PlanetFootprint` and `PlanetOccupant` to prefabs at runtime when missing (`GetComponent ?? AddComponent`) so prefabs don't need editing. Back-row objects get `sortingOrder −= 10` on all child `SpriteRenderer`s when reserved.
- [ ] **Step 5: Wire Blockman and the camera.**
  - **`Citizen_Blockman.Start`:** add `AirborneAgent` with tag `Blockman`.
  - **`CameraControls.Start`:** scale `MaxOrbitDistance` by `Planet.Radius / 7.62f` before line 85.
- [ ] **Step 6: Run tests.** Expected: exit 0.
- [ ] **Step 7: Swap the planets in the scenes.** In `Main.unity`, replace the old `Planet` prefab instance with `ProceduralPlanet`. In `Development_Main.unity`, replace the object carrying `MapGenerator`/`MapDisplay` with `ProceduralPlanet`. Use `PlanetSetup.ReplacePlanetInScene`, which:
  1. opens the scene
  2. finds the root by name (`"Planet"` in Main; the `MapDisplay` object in Development_Main)
  3. instantiates the prefab at the same position
  4. deletes the old object
  5. saves the scene

```bash
"/c/Program Files/Unity/Hub/Editor/6000.6.5f1/Editor/Unity.exe" -batchmode -nographics -quit -projectPath "W:/Projects/PocketPlanet/Pocketplanet" -executeMethod PocketPlanet.Planet.Editor.PlanetSetup.ReplacePlanetsInGameScenes -logFile "W:/Projects/PocketPlanet/Pocketplanet/Logs/setup.log"
```

(`ReplacePlanetsInGameScenes` calls `ReplacePlanetInScene` for both scenes.) Expected: exit 0. `grep -c <ProceduralPlanet.prefab guid> Assets/Scenes/Main.unity` ≥ 1 and the old `Planet.prefab` guid count is 0.

- [ ] **Step 8: Delete the old generator.** `git rm -r Assets/Scripts/WorldGeneration Assets/Scripts/WorldGeneration.meta Assets/Editor/MapGeneratorEditor.cs Assets/Editor/MapGeneratorEditor.cs.meta Assets/Scenes/WorldGeneration Assets/Scenes/WorldGeneration.meta`. If nothing else references it, also remove `Assets/Prefabs/Planet.prefab` after confirming with `grep -rl <its guid> Assets` (expect no hits).
- [ ] **Step 9: Run the full test suite** with filter `PocketPlanet.Planet.Tests`. Expected: exit 0 and no compile errors in `Logs/test-run.log`.
- [ ] **Step 10: Manual check (the user, in the Editor).**
  1. Open `Development_Main`, select `ProceduralPlanet` and Regenerate with seeds 1–3, continents 1/3/8 and circumference 30/48/150. Expect a round blocky planet with biomes, where the rim gizmo matches the visible edge.
  2. Drag the `ManualMoodSource` slider from −1 to +1. Expect the planet to turn ash/lava, then vivid, with overall saturation following.
  3. Use the Placement test foldout on water, land and coast. Expect the reasons to match what's visible.
  4. Open `Main` and press Play. Expect auto-construction to place buildings only on land, citizens to walk the rim, and Blockman to hover.
- [ ] **Step 11: Commit** — "Integrate procedural planet into the game and remove the 2019 generator".

---

## Notes for the executor

- **Land coverage tolerance.** The spec says ±3%. Rim blocks are quantised, so each coastline can be off by up to one block, and Task 4 adds `ContinentCount·2·BlockSize/Circumference` to the tolerance. The rim arc layout itself (Task 3) meets coverage exactly.
- **Blockzilla spawn points.** The old ones were children of the planet prefab. The adapter replaces them with a generated transform, so `EventHandler.cs:115` keeps working unchanged.
- **Player drag-to-build input stays broken (out of scope).** `ObjectSpawner` changes are still made so it works once input is restored.

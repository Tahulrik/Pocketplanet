using UnityEditor;
using UnityEngine;

namespace PocketPlanet.Planet.Editor
{
    [CustomEditor(typeof(PlanetHost))]
    public class PlanetHostEditor : UnityEditor.Editor
    {
        const float PreviewHeight = 0.6f;

        bool showPlacementTest;
        float testArc;
        float testWidth = 1f;
        RowPreference testRow = RowPreference.Either;
        PlacementRuleKind testRule = PlacementRuleKind.LandOnly;
        float testMaxSpan = 3f;

        PlanetHost Host => (PlanetHost)target;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (GUILayout.Button("Regenerate"))
            {
                Host.Regenerate();
                SceneView.RepaintAll();
            }

            showPlacementTest = EditorGUILayout.Foldout(showPlacementTest, "Placement test", true);
            if (showPlacementTest)
                DrawPlacementTest();
        }

        void DrawPlacementTest()
        {
            var planet = Host.Planet;
            EditorGUI.BeginChangeCheck();
            testArc = EditorGUILayout.Slider("Arc", testArc, 0f, planet.Frame.Circumference);
            testWidth = EditorGUILayout.FloatField("Width", Mathf.Max(0.05f, testWidth));
            testRow = (RowPreference)EditorGUILayout.EnumPopup("Row", testRow);
            testRule = (PlacementRuleKind)EditorGUILayout.EnumPopup("Rule", testRule);
            if (testRule == PlacementRuleKind.Spanning)
                testMaxSpan = EditorGUILayout.FloatField("Max span", testMaxSpan);
            if (EditorGUI.EndChangeCheck())
                SceneView.RepaintAll();

            var result = Check(planet);
            var biome = planet.Biomes[planet.Surface.Profile.BiomeAt(testArc)];
            EditorGUILayout.HelpBox(
                result.IsValid ? $"Valid in {result.Row} row ({biome.DisplayName})" : $"Blocked: {result.Reason} ({biome.DisplayName})",
                result.IsValid ? MessageType.Info : MessageType.Warning);
        }

        PlacementResult Check(IPlanet planet) =>
            planet.Placement.Check(new Footprint(testWidth, testRow, PlacementRules.Create(testRule, testMaxSpan)), testArc);

        void OnSceneGUI()
        {
            if (!showPlacementTest)
                return;
            var planet = Host.Planet;
            var result = Check(planet);
            Handles.color = result.IsValid ? Color.green : Color.red;
            var range = result.Range;
            const int steps = 24;
            var previous = (Vector3)planet.Frame.ToWorld(new PlanetCoord(range.Start, PreviewHeight));
            for (int i = 1; i <= steps; i++)
            {
                var next = (Vector3)planet.Frame.ToWorld(new PlanetCoord(range.Start + range.Length * i / steps, PreviewHeight));
                Handles.DrawLine(previous, next, 3f);
                previous = next;
            }
        }
    }
}

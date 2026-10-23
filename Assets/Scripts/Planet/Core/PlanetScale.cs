namespace PocketPlanet.Planet
{
    public static class PlanetScale
    {
        public static float FromReference(float valueAtReference, float referenceRadius, float planetRadius) =>
            valueAtReference * planetRadius / referenceRadius;
    }
}

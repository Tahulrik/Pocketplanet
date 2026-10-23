namespace PocketPlanet.Planet
{
    public class SeededNoise
    {
        readonly uint seed;

        public SeededNoise(int seed)
        {
            this.seed = (uint)seed;
        }

        public float Value(float x, float y)
        {
            var x0 = (int)x;
            if (x0 > x) x0--;
            var y0 = (int)y;
            if (y0 > y) y0--;
            var sx = Smooth(x - x0);
            var sy = Smooth(y - y0);
            var a = Hash(x0, y0);
            var b = Hash(x0 + 1, y0);
            var c = Hash(x0, y0 + 1);
            var d = Hash(x0 + 1, y0 + 1);
            var bottom = a + (b - a) * sx;
            var top = c + (d - c) * sx;
            return bottom + (top - bottom) * sy;
        }

        public float Fractal(float x, float y, int octaves)
        {
            float sum = 0f, amplitude = 1f, frequency = 1f, total = 0f;
            for (int i = 0; i < octaves; i++)
            {
                sum += Value(x * frequency + i * 31.7f, y * frequency + i * 17.3f) * amplitude;
                total += amplitude;
                amplitude *= 0.5f;
                frequency *= 2f;
            }
            return sum / total;
        }

        static float Smooth(float t) => t * t * (3f - 2f * t);

        float Hash(int x, int y)
        {
            unchecked
            {
                var h = seed * 0x9E3779B1u;
                h = (h ^ (uint)x) * 0x85EBCA6Bu;
                h = (h ^ (uint)y) * 0xC2B2AE35u;
                h ^= h >> 15;
                h *= 0x2C1B3C6Du;
                h ^= h >> 12;
                return (h & 0xFFFFFF) / 16777215f;
            }
        }
    }
}

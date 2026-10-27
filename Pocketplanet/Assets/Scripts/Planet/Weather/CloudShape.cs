using System.Collections.Generic;
using UnityEngine;

namespace PocketPlanet.Planet
{
    public readonly struct CloudPuff
    {
        public CloudPuff(Vector2 offset, float scale, int spriteIndex, bool flipped)
        {
            Offset = offset;
            Scale = scale;
            SpriteIndex = spriteIndex;
            Flipped = flipped;
        }

        public Vector2 Offset { get; }
        public float Scale { get; }
        public int SpriteIndex { get; }
        public bool Flipped { get; }
    }

    public sealed class CloudShapeSettings
    {
        public float PuffWidth { get; set; } = 0.85f;
        public float MinScale { get; set; } = 0.6f;
        public float MaxScale { get; set; } = 1.6f;
        public float GroupChance { get; set; } = 0.35f;
        public int MinGroupSize { get; set; } = 2;
        public int MaxGroupSize { get; set; } = 5;
        public float GroupScaleBoost { get; set; } = 1.25f;
        public float HeightJitter { get; set; } = 0.15f;
    }

    public sealed class CloudShape
    {
        CloudShape(IReadOnlyList<CloudPuff> puffs, float width)
        {
            Puffs = puffs;
            Width = width;
        }

        public IReadOnlyList<CloudPuff> Puffs { get; }
        public float Width { get; }

        public static CloudShape Create(System.Random rng, CloudShapeSettings settings, int spriteCount)
        {
            var count = rng.NextDouble() < settings.GroupChance
                ? rng.Next(settings.MinGroupSize, settings.MaxGroupSize + 1)
                : 1;
            var baseScale = Mathf.Lerp(settings.MinScale, settings.MaxScale, (float)rng.NextDouble())
                * (count > 1 ? settings.GroupScaleBoost : 1f);

            var puffs = new List<CloudPuff>(count);
            float left = 0f, right = 0f;
            for (int i = 0; i < count; i++)
            {
                var scale = baseScale * Mathf.Lerp(0.7f, 1.1f, (float)rng.NextDouble());
                var halfWidth = settings.PuffWidth * scale / 2f;
                var x = i == 0 ? 0f : NextBesideGroup(rng, ref left, ref right, halfWidth);
                if (i == 0)
                {
                    left = -halfWidth;
                    right = halfWidth;
                }
                var y = i == 0 ? 0f : Mathf.Lerp(-settings.HeightJitter, settings.HeightJitter, (float)rng.NextDouble());
                puffs.Add(new CloudPuff(new Vector2(x, y), scale, rng.Next(Mathf.Max(1, spriteCount)), rng.Next(2) == 0));
            }
            return Centered(puffs, left, right);
        }

        // Each new puff overlaps one end of the group by 40-65% of its width.
        static float NextBesideGroup(System.Random rng, ref float left, ref float right, float halfWidth)
        {
            var overlap = halfWidth * 2f * Mathf.Lerp(0.4f, 0.65f, (float)rng.NextDouble());
            if (rng.Next(2) == 0)
            {
                var x = right + halfWidth - overlap;
                right = x + halfWidth;
                return x;
            }
            var leftX = left - halfWidth + overlap;
            left = leftX - halfWidth;
            return leftX;
        }

        static CloudShape Centered(List<CloudPuff> puffs, float left, float right)
        {
            var shift = (left + right) / 2f;
            for (int i = 0; i < puffs.Count; i++)
            {
                var p = puffs[i];
                puffs[i] = new CloudPuff(p.Offset - new Vector2(shift, 0f), p.Scale, p.SpriteIndex, p.Flipped);
            }
            return new CloudShape(puffs, right - left);
        }
    }
}

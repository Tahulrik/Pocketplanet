using UnityEngine;

namespace PocketPlanet.Planet
{
    public class BlockMap
    {
        public const byte Empty = 255;

        readonly byte[,] biomes;

        public BlockMap(int size, float blockSize)
        {
            Size = size;
            BlockSize = blockSize;
            biomes = new byte[size, size];
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    biomes[x, y] = Empty;
        }

        public int Size { get; }
        public float BlockSize { get; }
        float HalfExtent => Size * BlockSize / 2f;

        public byte this[int x, int y]
        {
            get => biomes[x, y];
            set => biomes[x, y] = value;
        }

        public Vector2 BlockCenter(int x, int y) =>
            new Vector2((x + 0.5f) * BlockSize - HalfExtent, (y + 0.5f) * BlockSize - HalfExtent);

        public bool TryGetBlock(Vector2 local, out int x, out int y)
        {
            x = Mathf.FloorToInt((local.x + HalfExtent) / BlockSize);
            y = Mathf.FloorToInt((local.y + HalfExtent) / BlockSize);
            return x >= 0 && y >= 0 && x < Size && y < Size;
        }
    }
}

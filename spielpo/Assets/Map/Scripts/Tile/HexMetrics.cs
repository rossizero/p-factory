using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Map.Tile
{
    public static class HexMetrics
    {
        public const string seed = "Peter Lustig und Käptn Peng";
        const float maxPerlinOffset = 1313131f;
        public static Vector2 perlinSeed = SetSeed();
        public static int randomSeed;
        public const int outerRadius = 10;
        public const float innerRadius = outerRadius * 0.866025404f;
        public const float solidFactor = 0.8f;
        public const float blendFactor = 1f - solidFactor;
        public const int elevationStep = 5;
        public const int terracesPerSlope = 2;
        public const int terraceSteps = terracesPerSlope * 2 + 1;
        public const float verticalTerraceStepSize = 1f / (terracesPerSlope + 1);
        public const float horizontalTerraceStepSize = 1f / terraceSteps;
        public const int chunkSizeX = 5, chunkSizeZ = 5;
        public const float waterElevationOffset = -4.7f;
        public const float noiseScale = 0.008f;
        public const float cellPerturbStrength = 2f;
        public const float elevationPerturbStrength = 1.2f;
        public static Texture2D noiseSource;
        public static float biomsize = 111; //size of the noise grid
        public static float biomeBorderWeight = outerRadius * 3.4f; //bigger number = smoother transitions between biomes

        public static Vector3[] corners = {
            new Vector3(0f, 0f, outerRadius),
            new Vector3(innerRadius, 0f, 0.5f * outerRadius),
            new Vector3(innerRadius, 0f, -0.5f * outerRadius),
            new Vector3(0f, 0f, -outerRadius),
            new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
            new Vector3(-innerRadius, 0f, 0.5f * outerRadius),
            new Vector3(0f, 0f, outerRadius)
        };

        public static Vector3 GetFirstCorner(HexDirection direction)
        {
            return corners[(int)direction];
        }

        public static Vector3 GetSecondCorner(HexDirection direction)
        {
            return corners[(int)direction + 1];
        }
        public static Vector3 GetFirstSolidCorner(HexDirection direction)
        {
            return corners[(int)direction] * solidFactor;
        }

        public static Vector3 GetSecondSolidCorner(HexDirection direction)
        {
            return corners[(int)direction + 1] * solidFactor;
        }
        public static Vector3 GetBridge(HexDirection direction)
        {
            return (corners[(int)direction] + corners[(int)direction + 1]) * blendFactor;
        }

        public static Vector3 TerraceLerp(Vector3 a, Vector3 b, int step)
        {
            float h = step * HexMetrics.horizontalTerraceStepSize;
            a.x += (b.x - a.x) * h;
            a.z += (b.z - a.z) * h;
            float v = ((step + 1) / 2) * HexMetrics.verticalTerraceStepSize;
            a.y += (b.y - a.y) * v;
            return a;
        }
        public static Color TerraceLerp(Color a, Color b, int step)
        {
            float h = step * HexMetrics.horizontalTerraceStepSize;
            return Color.Lerp(a, b, h);
        }
        public static HexEdgeType GetEdgeType(int elevation1, int elevation2)
        {
            if (elevation1 == elevation2)
            {
                return HexEdgeType.Flat;
            }
            int delta = elevation2 - elevation1;
            if (delta == 1 || delta == -1)
            {
                return HexEdgeType.Slope;
            }
            return HexEdgeType.Cliff;
        }

        /// <summary>
        /// Sets UnityEngine.Random seed and the perlin noise seed position for map generation. Could be done way better I guess.
        /// </summary>
        /// <returns>returns Perlin noise seed position</returns>
        public static Vector2 SetSeed()
        {
            MD5 md5Hasher = MD5.Create();
            var hashed = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(seed));
            int ivalue = BitConverter.ToInt32(hashed, 0);
            UnityEngine.Random.InitState(ivalue);
            randomSeed = ivalue;
            float a = (ivalue / (float)Int32.MaxValue) * UnityEngine.Random.value * maxPerlinOffset;
            float b = (ivalue / (float)Int32.MaxValue) * UnityEngine.Random.value * maxPerlinOffset;
            Vector2 ret = new Vector2(a, b);
            // Debug.Log(a + " " + b);
            return ret;
        }

        public static Vector4 SampleNoise(Vector3 position)
        {
            return noiseSource.GetPixelBilinear(
                (position.x + perlinSeed.x) * noiseScale,
                (position.z + perlinSeed.y) * noiseScale
            );
        }

    }
}

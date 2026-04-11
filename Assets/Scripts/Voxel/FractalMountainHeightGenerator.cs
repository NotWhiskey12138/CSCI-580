using UnityEngine;

public static class FractalMountainHeightGenerator
{
    public static float[,] GenerateHeightMatrix(
        int width,
        int length,
        float terrainHeight,
        int maxHeight,
        float baseScale,
        int octaves,
        float persistence,
        float lacunarity,
        int seed,
        Vector2 offset,
        float ridgeStrength,
        float mountainCurve
    )
    {
        width = Mathf.Max(1, width);
        length = Mathf.Max(1, length);
        terrainHeight = Mathf.Max(0f, terrainHeight);
        maxHeight = Mathf.Max(1, maxHeight);
        baseScale = Mathf.Max(0.0001f, baseScale);
        octaves = Mathf.Max(1, octaves);
        persistence = Mathf.Max(0f, persistence);
        lacunarity = Mathf.Max(0.0001f, lacunarity);
        ridgeStrength = Mathf.Max(0.0001f, ridgeStrength);
        mountainCurve = Mathf.Max(0.0001f, mountainCurve);

        float[,] heights = new float[width + 1, length + 1];

        System.Random prng = new System.Random(seed);
        float seedOffsetX = prng.Next(-100000, 100000) + offset.x;
        float seedOffsetY = prng.Next(-100000, 100000) + offset.y;

        for (int x = 0; x <= width; x++)
        {
            for (int y = 0; y <= length; y++)
            {
                float noiseValue = FractalNoise(
                    x + seedOffsetX,
                    y + seedOffsetY,
                    octaves,
                    persistence,
                    lacunarity,
                    baseScale,
                    ridgeStrength);

                if (!Mathf.Approximately(mountainCurve, 1f))
                {
                    noiseValue = Mathf.Pow(noiseValue, mountainCurve);
                }
                heights[x, y] = ApplyExponentialHeightCap(noiseValue * terrainHeight, maxHeight);
                
            }
        }
        return heights;
    }

    private static float FractalNoise(
        float x,
        float y,
        int octaves,
        float persistence,
        float lacunarity,
        float baseScale,
        float ridgeStrength)
    {
        float amplitude = 1f;
        float sum = 0f;
        float sampleX = x;
        float sampleY = y;

        for (int i = 0; i < octaves; i++)
        {
            float perlin = Mathf.PerlinNoise(sampleX / baseScale, sampleY / baseScale);
            perlin = Mathf.Abs(perlin * 2f - 1f);
            perlin = Mathf.Pow(perlin, ridgeStrength);

            sum += perlin * amplitude;

            amplitude *= persistence;
            sampleX *= lacunarity;
            sampleY *= lacunarity;
        }

        return sum;
    }

    private static float ApplyExponentialHeightCap(float height, int maxHeight)
    {
        if (height <= 0f)
        {
            return 0f;
        }

        if (maxHeight <= 0)
        {
            return height;
        }

        return maxHeight * (1f - Mathf.Exp(-height / maxHeight));
    }

}

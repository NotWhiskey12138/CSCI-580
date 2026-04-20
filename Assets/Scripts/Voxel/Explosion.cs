using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Explosion
{
    [Header("Timing")]
    [SerializeField] private float growthDuration = 0.9f;
    [SerializeField] private float upwardDuration = 1.8f;
    [SerializeField] private float dissipationDuration = 0.8f;

    [Header("Shape")]
    [SerializeField] private int sphereCount = 9;
    [SerializeField] private float baseRadius = 5f;
    [SerializeField] private float topRadiusBonus = 3f;
    [SerializeField] private float horizontalSpread = 1.8f;
    [SerializeField] private float verticalSpacing = 1.15f;
    [SerializeField] private float upwardSpeed = 2.1f;
    [SerializeField] private float driftAmount = 0.9f;
    [SerializeField] private float turbulenceStrength = 0.45f;
    [SerializeField] private float fireShellThickness = 0.22f;
    [SerializeField] private float smokeStartDelay = 0.2f;

    public float TotalDuration => growthDuration + upwardDuration + dissipationDuration;

    public int GetBoundsRadius(float t)
    {
        float maxRadius = baseRadius + topRadiusBonus;
        float maxHeight = (sphereCount - 1) * verticalSpacing + upwardSpeed * upwardDuration;
        float maxHorizontal = horizontalSpread + driftAmount + maxRadius;
        return Mathf.CeilToInt(Mathf.Max(maxHorizontal, maxHeight + maxRadius));
    }

    public Dictionary<Vector3Int, VoxelType> BuildVoxelMap(Vector3Int center, float t)
    {
        Dictionary<Vector3Int, VoxelType> voxelMap = new();
        Vector3 centerPoint = center + Vector3.one * 0.5f;
        float activeDuration = growthDuration + upwardDuration;

        if (t >= TotalDuration)
        {
            return voxelMap;
        }

        float safeGrowthDuration = Mathf.Max(0.01f, growthDuration);
        float normalizedGrowth = Mathf.Clamp01(t / safeGrowthDuration);
        float upwardTime = Mathf.Max(0f, t - growthDuration * 0.2f);
        float dissipation = 1f - Mathf.Clamp01((t - activeDuration) / Mathf.Max(0.01f, dissipationDuration));

        for (int i = 0; i < sphereCount; i++)
        {
            float layer = sphereCount == 1 ? 0f : i / (float)(sphereCount - 1);
            float rise = upwardSpeed * upwardTime * Mathf.Lerp(0.5f, 1.2f, layer);
            float driftX = SampleNoise(centerPoint, i, t, 0.17f, 13.4f) * driftAmount * Mathf.Lerp(0.45f, 1f, layer);
            float driftZ = SampleNoise(centerPoint, i, t, 0.19f, 37.9f) * driftAmount * Mathf.Lerp(0.45f, 1f, layer);

            Vector3 sphereCenter = centerPoint + new Vector3(
                driftX,
                layer * verticalSpacing * sphereCount + rise,
                driftZ);

            sphereCenter += RadialOffset(centerPoint, i, layer);

            float radius = (baseRadius + topRadiusBonus * layer) * Mathf.Lerp(0.35f, 1f, normalizedGrowth);
            radius *= Mathf.Lerp(0.6f, 1f, dissipation);
            int sphereBounds = Mathf.CeilToInt(radius + Mathf.Abs(turbulenceStrength) + 1f);

            Vector3Int min = Vector3Int.FloorToInt(sphereCenter - Vector3.one * sphereBounds);
            Vector3Int max = Vector3Int.CeilToInt(sphereCenter + Vector3.one * sphereBounds);

            for (int x = min.x; x <= max.x; x++)
            {
                for (int y = min.y; y <= max.y; y++)
                {
                    for (int z = min.z; z <= max.z; z++)
                    {
                        Vector3Int coord = new Vector3Int(x, y, z);
                        if (!TryGetSphereVoxelType(coord, sphereCenter, i, layer, radius, dissipation, t, out VoxelType voxelType))
                        {
                            continue;
                        }

                        if (!voxelMap.TryGetValue(coord, out VoxelType currentType) || GetPriority(voxelType) > GetPriority(currentType))
                        {
                            voxelMap[coord] = voxelType;
                        }
                    }
                }
            }
        }

        return voxelMap;
    }

    public bool TryGetExplosionType(Vector3Int coord, Vector3 center, float t, out VoxelType voxelType)
    {
        voxelType = VoxelType.Air;
        float activeDuration = growthDuration + upwardDuration;

        if (t >= TotalDuration)
        {
            return false;
        }

        float safeGrowthDuration = Mathf.Max(0.01f, growthDuration);
        float normalizedGrowth = Mathf.Clamp01(t / safeGrowthDuration);
        float upwardTime = Mathf.Max(0f, t - growthDuration * 0.2f);
        float dissipation = 1f - Mathf.Clamp01((t - activeDuration) / Mathf.Max(0.01f, dissipationDuration));
        Vector3 p = coord + new Vector3(0.5f, 0.5f, 0.5f);
        bool hasSmoke = false;
        bool hasFire = false;
        bool hasAir = false;

        for (int i = 0; i < sphereCount; i++)
        {
            float layer = sphereCount == 1 ? 0f : i / (float)(sphereCount - 1);
            float rise = upwardSpeed * upwardTime * Mathf.Lerp(0.5f, 1.2f, layer);
            float driftX = SampleNoise(center, i, t, 0.17f, 13.4f) * driftAmount * Mathf.Lerp(0.45f, 1f, layer);
            float driftZ = SampleNoise(center, i, t, 0.19f, 37.9f) * driftAmount * Mathf.Lerp(0.45f, 1f, layer);

            Vector3 sphereCenter = center + new Vector3(
                driftX,
                layer * verticalSpacing * sphereCount + rise,
                driftZ);

            sphereCenter += RadialOffset(center, i, layer);

            float radius = (baseRadius + topRadiusBonus * layer) * Mathf.Lerp(0.35f, 1f, normalizedGrowth);
            radius *= Mathf.Lerp(0.6f, 1f, dissipation);
            float edgeNoise = SampleNoise(p, i, t, 0.26f, 71.2f) * turbulenceStrength;
            float distance = (p - sphereCenter).magnitude;
            float normalizedDistance = distance / Mathf.Max(0.01f, radius + edgeNoise);

            if (normalizedDistance > 1f)
            {
                continue;
            }

            if (normalizedDistance <= Mathf.Lerp(0.72f, 0.84f, layer))
            {
                hasAir = true;
                continue;
            }

            if (t <= growthDuration * 1.15f * dissipation && layer <= 0.45f && normalizedDistance >= 1f - fireShellThickness)
            {
                hasFire = true;
                continue;
            }

            if (t >= smokeStartDelay && layer >= 0.25f)
            {
                float smokeNoise = SampleNoise(p, i + 17, t, 0.18f, 91.7f) + 0.5f;
                if (smokeNoise < 0.2f + dissipation * 0.35f)
                {
                    hasSmoke = true;
                }
            }
        }

        if (hasAir)
        {
            voxelType = VoxelType.Air;
            return true;
        }

        if (hasFire)
        {
            voxelType = VoxelType.Fire;
            return true;
        }

        if (hasSmoke)
        {
            voxelType = VoxelType.Smoke;
            return true;
        }

        return false;
    }

    private Vector3 RadialOffset(Vector3 center, int index, float layer)
    {
        float angle = Mathf.PI * 2f * Frac(Mathf.Sin(index * 12.9898f + center.x * 0.137f + center.z * 0.173f) * 43758.5453f);
        float radius = horizontalSpread * Mathf.Lerp(0.2f, 1f, layer);
        return new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
    }

    private float SampleNoise(Vector3 p, int seedOffset, float t, float scale, float seed)
    {
        float x = p.x * scale + seedOffset * 0.73f + seed;
        float y = p.y * scale + t * 0.85f + seed * 0.1f;
        float z = p.z * scale - t * 0.55f + seedOffset * 0.37f;

        float xy = Mathf.PerlinNoise(x, y);
        float yz = Mathf.PerlinNoise(y, z);
        float zx = Mathf.PerlinNoise(z, x);
        return (xy + yz + zx) / 3f - 0.5f;
    }

    private float Frac(float value)
    {
        return value - Mathf.Floor(value);
    }

    private bool TryGetSphereVoxelType(
        Vector3Int coord,
        Vector3 sphereCenter,
        int sphereIndex,
        float layer,
        float radius,
        float dissipation,
        float t,
        out VoxelType voxelType)
    {
        voxelType = VoxelType.Air;
        Vector3 p = coord + new Vector3(0.5f, 0.5f, 0.5f);
        float edgeNoise = SampleNoise(p, sphereIndex, t, 0.26f, 71.2f) * turbulenceStrength;
        float distance = (p - sphereCenter).magnitude;
        float normalizedDistance = distance / Mathf.Max(0.01f, radius + edgeNoise);

        if (normalizedDistance > 1f)
        {
            return false;
        }

        if (normalizedDistance <= Mathf.Lerp(0.72f, 0.84f, layer))
        {
            voxelType = VoxelType.Air;
            return true;
        }

        if (t <= growthDuration * 1.15f * dissipation && layer <= 0.45f && normalizedDistance >= 1f - fireShellThickness)
        {
            voxelType = VoxelType.Fire;
            return true;
        }

        if (t >= smokeStartDelay && layer >= 0.25f)
        {
            float smokeNoise = SampleNoise(p, sphereIndex + 17, t, 0.18f, 91.7f) + 0.5f;
            if (smokeNoise < 0.2f + dissipation * 0.35f)
            {
                voxelType = VoxelType.Smoke;
                return true;
            }
        }

        return false;
    }

    private int GetPriority(VoxelType voxelType)
    {
        return voxelType switch
        {
            VoxelType.Smoke => 1,
            VoxelType.Fire => 2,
            VoxelType.Air => 3,
            _ => 0
        };
    }
}

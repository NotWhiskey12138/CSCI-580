using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class VoxelBootstrap : MonoBehaviour
{
    [SerializeField] private VoxelWorld world;
    [SerializeField] private ChunkViewManager viewManager;
    [SerializeField] private Transform player;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float explosionDuration = 1.5f;
    [SerializeField] private Explosion explosion = new();
    [SerializeField] private float explosionRayDistance = 200f;
    [SerializeField] private float explosionRayStep = 0.25f;

    private int playerPosX;
    private int playerPosY;
    private int playerPosZ;
    private Coroutine activeExplosion;

    private void Start()
    {
        BuildMinimalWorld();
    }

    public void Update()
    {
        playerPosX = (int)player.position.x;
        playerPosY = (int)player.position.y;
        playerPosZ = (int)player.position.z;
        
        if (Input.GetKeyDown(KeyCode.R) && activeExplosion == null)
        {
            if (TryGetExplosionCenter(out Vector3Int explosionCenter))
            {
                activeExplosion = StartCoroutine(PlayExplosion(explosionCenter));
            }
            else
            {
                Debug.LogWarning("No non-air voxel found in front of the camera for explosion targeting.");
            }
        }
    }

    private IEnumerator PlayExplosion(Vector3Int center)
    {
        float elapsed = 0f;
        float totalDuration = Mathf.Max(explosionDuration, explosion.TotalDuration);
        bool affectedAnyChunk = false;
        HashSet<Vector3Int> previousTransientVoxels = new();

        while (elapsed < totalDuration)
        {
            Dictionary<Vector3Int, VoxelType> currentVoxels = explosion.BuildVoxelMap(center, elapsed);
            List<Vector3Int> affectedChunks = world.ApplyExplosionFrame(currentVoxels, previousTransientVoxels);
            affectedAnyChunk |= affectedChunks.Count > 0;
            viewManager.RebuildChunks(affectedChunks);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Dictionary<Vector3Int, VoxelType> finalVoxels = explosion.BuildVoxelMap(center, totalDuration);
        List<Vector3Int> finalAffectedChunks = world.ApplyExplosionFrame(finalVoxels, previousTransientVoxels);
        affectedAnyChunk |= finalAffectedChunks.Count > 0;
        viewManager.RebuildChunks(finalAffectedChunks);

        if (!affectedAnyChunk)
        {
            Debug.LogWarning($"Explosion affected no chunks at {center}. Check the chosen voxel against your generated world bounds.");
        }

        activeExplosion = null;
    }

    private bool TryGetExplosionCenter(out Vector3Int center)
    {
        center = default;

        if (world == null)
        {
            return false;
        }

        Transform originTransform = cameraTransform != null ? cameraTransform : player;
        if (originTransform == null)
        {
            return false;
        }

        float step = Mathf.Max(0.05f, explosionRayStep);
        Vector3 origin = originTransform.position;
        Vector3 direction = originTransform.forward.normalized;

        for (float distance = 0f; distance <= explosionRayDistance; distance += step)
        {
            Vector3 samplePoint = origin + direction * distance;
            Vector3Int voxelCoord = Vector3Int.FloorToInt(samplePoint);

            if (world.GetVoxel(voxelCoord.x, voxelCoord.y, voxelCoord.z) == VoxelType.Air)
            {
                continue;
            }

            center = voxelCoord;
            return true;
        }

        return false;
    }

    [ContextMenu("Build Minimal World")]
    public void BuildMinimalWorld()
    {
        if (world == null || viewManager  == null)
        {
            Debug.LogWarning("VoxelBootstrap: Please assign both World and DebugView.");
            return;
        }

        world.GenerateWorld();
        viewManager.Rebuild();
    }
}

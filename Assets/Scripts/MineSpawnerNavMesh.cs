using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;


public class MineSpawnerNavMesh : MonoBehaviour
{
    [Header("Mine Settings")]
    public GameObject minePrefab; 
    public int numberOfMines = 10;

    [Header("Spawn Area")]
    public float spawnYLevel = 0.5f; // Height above ground
    public float spawnRadius = 50f; // How far from this spawner to place mines
    public Vector3 spawnCenter; // Center point (leave 0,0,0 to use spawner position)

    [Header("Spacing")]
    public float minDistanceBetweenMines = 5f;
    public int maxAttempts = 100;

    [Header("Spawn Timing")]
    public bool spawnOnStart = true;

    [Header("Debug")]
    public bool showSpawnArea = true;
    public bool showDebugLogs = true;

    private List<Vector3> spawnedPositions = new List<Vector3>();


    void Start()
    {
        // Use spawner position if no custom center set
        if (spawnCenter == Vector3.zero)
            spawnCenter = transform.position;

        if (spawnOnStart)
        {
            SpawnMines();
        }
    }

    // Main spawn function
    public void SpawnMines()
    {
        if (minePrefab == null)
        {
            Debug.LogError("MineSpawner: No mine prefab assigned!");
            return;
        }

        ClearExistingMines();
        spawnedPositions.Clear();

        int successfulSpawns = 0;

        for (int i = 0; i < numberOfMines; i++)
        {
            Vector3 spawnPos = FindValidSpawnPosition();

            if (spawnPos != Vector3.zero)
            {
                SpawnMineAt(spawnPos);
                spawnedPositions.Add(spawnPos);
                successfulSpawns++;
            }
        }

        if (showDebugLogs)
            Debug.Log($"MineSpawner: Spawned {successfulSpawns}/{numberOfMines} mines on NavMesh");
    }

    // Find valid position on NavMesh (roads/walkable areas)
    private Vector3 FindValidSpawnPosition()
    {
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            // Random position within radius
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 randomPos = spawnCenter + new Vector3(randomCircle.x, 0, randomCircle.y);

            // Try to find nearest point on NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPos, out hit, 10f, NavMesh.AllAreas))
            {
                Vector3 spawnPos = new Vector3(hit.position.x, hit.position.y + spawnYLevel, hit.position.z);

                // Check if far enough from other mines
                if (IsPositionValid(spawnPos))
                {
                    return spawnPos;
                }
            }
        }

        return Vector3.zero;
    }

    // Check if position is far enough from other mines
    private bool IsPositionValid(Vector3 position)
    {
        foreach (Vector3 existingPos in spawnedPositions)
        {
            if (Vector3.Distance(position, existingPos) < minDistanceBetweenMines)
                return false;
        }
        return true;
    }

    // Spawn mine at position
    private void SpawnMineAt(Vector3 position)
    {
        GameObject mine = Instantiate(minePrefab, position, Quaternion.identity);
        mine.transform.parent = this.transform;

        if (showDebugLogs)
            Debug.Log($"Spawned mine at {position}");
    }

    // Clear all mines
    public void ClearExistingMines()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        spawnedPositions.Clear();
    }

    // Visualize spawn area
    private void OnDrawGizmos()
    {
        if (!showSpawnArea) return;

        Vector3 center = spawnCenter == Vector3.zero ? transform.position : spawnCenter;

        // Draw spawn radius
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawSphere(center, spawnRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, spawnRadius);

        // Draw spawned positions
        Gizmos.color = Color.red;
        foreach (Vector3 pos in spawnedPositions)
        {
            Gizmos.DrawWireSphere(pos, 0.5f);
        }
    }
}
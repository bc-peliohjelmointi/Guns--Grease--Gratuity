using System.Collections.Generic;
using UnityEngine;

public class DebreeSpawner : MonoBehaviour
{
    [Header("Debree Spawning")]
    public GameObject debreePrefab;
    public bool spawnOnlyOnce = true;

    [Header("Spawn Chance per Area")]
    [Range(0f, 1f)]
    public float debreeSpawnChance = 0.45f;

    private GameObject[] debreeSpawnAreas;
    private readonly List<GameObject> spawnedDebree = new();

    private bool hasSpawned;

    void Start()
    {
        debreeSpawnAreas = GameObject.FindGameObjectsWithTag("DebreeSpawnArea");
        // SpawnDebree();
    }

    public void SpawnDebree()
    {
        debreeSpawnAreas = GameObject.FindGameObjectsWithTag("DebreeSpawnArea");

        if (debreeSpawnAreas.Length == 0)
        {
            Debug.LogWarning("DebreeSpawner: No DebreeSpawnArea objects found!");
            return;
        }

        if (spawnOnlyOnce && hasSpawned)
            return;

        if (!debreePrefab)
        {
            Debug.LogWarning("DebreeSpawner: No debree prefab assigned!");
            return;
        }

        DestroyDebree();

        foreach (GameObject spawnArea in debreeSpawnAreas)
        {
            if (!spawnArea)
                continue;

            if (Random.value > debreeSpawnChance)
                continue;

            BoxCollider box = spawnArea.GetComponent<BoxCollider>();
            if (!box)
                continue;

            Vector3 spawnPos = GetRandomZInBox(box);

            GameObject debree = Instantiate(
                debreePrefab,
                spawnPos,
                Quaternion.identity
            );

            spawnedDebree.Add(debree);
        }

        hasSpawned = true;
        Debug.Log($"Debree spawned: {spawnedDebree.Count}");
    }

    public void DestroyDebree()
    {
        foreach (GameObject d in spawnedDebree)
        {
            if (d)
                Destroy(d);
        }

        spawnedDebree.Clear();
        hasSpawned = false;
    }

    // --------------------
    // RANDOM Z ONLY
    // --------------------
    Vector3 GetRandomZInBox(BoxCollider box)
    {
        // World center of the box
        Vector3 center = box.transform.TransformPoint(box.center);

        // Half size
        float halfZ = box.size.z * 0.5f;

        // Random Z offset
        float randomZ = Random.Range(-halfZ, halfZ);

        // Apply ONLY Z offset
        Vector3 localOffset = new Vector3(0f, 0f, randomZ);
        Vector3 worldOffset = box.transform.rotation * localOffset;

        return center + worldOffset;
    }
}

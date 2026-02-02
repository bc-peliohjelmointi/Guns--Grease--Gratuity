using System.Collections.Generic;
using UnityEngine;

public class TurretActivator : MonoBehaviour
{
    [Header("Turret Spawning")]
    public GameObject turretPrefab;
    public bool spawnOnlyOnce = true;

    [Header("SpawnChance per Turret Spawnpoint")]
    public float turretSpawnChance = 0.5f;

    private GameObject[] turretSpawnAreas;
    private readonly List<GameObject> spawnedTurrets = new();

    private bool hasSpawned;

    void Start()
    {
        turretSpawnAreas = GameObject.FindGameObjectsWithTag("TurretSpawnArea");
    }

    public void SpawnTurrets()
    {
        if (spawnOnlyOnce && hasSpawned)
            return;

        if (!turretPrefab)
        {
            Debug.LogWarning("TurretActivator: No turret prefab assigned!");
            return;
        }

        DestroyTurrets(); // safety

        foreach (GameObject spawnArea in turretSpawnAreas)
        {
            if (!spawnArea)
                continue;

            // Random spawn chance per spawn area
            if (Random.value > turretSpawnChance)
                continue;

            BoxCollider[] boxes = spawnArea.GetComponents<BoxCollider>();
            if (boxes.Length == 0)
            {
                Debug.LogWarning($"{spawnArea.name} has no BoxColliders!");
                continue;
            }

            // Pick ONE random box
            BoxCollider chosenBox = boxes[Random.Range(0, boxes.Length)];
            Vector3 spawnPos = GetRandomPointInBox(chosenBox);

            // For easier visual placing
            spawnPos.y -= 0.4f;

            GameObject turret = Instantiate(
                turretPrefab,
                spawnPos,
                Quaternion.Euler(0f, Random.Range(0f, 360f), 0f)
            );

            spawnedTurrets.Add(turret);
        }


        hasSpawned = true;
        Debug.Log("Turrets spawned using multiple BoxColliders!");
    }

    public void DestroyTurrets()
    {
        foreach (GameObject turret in spawnedTurrets)
        {
            if (turret)
                Destroy(turret);
        }

        spawnedTurrets.Clear();
        hasSpawned = false;

        Debug.Log("Turrets destroyed >:)");
    }

    // --------------------
    // BOX SPAWN LOGIC
    // --------------------
    Vector3 GetRandomPointInBox(BoxCollider box)
    {
        Vector3 center = box.transform.TransformPoint(box.center);
        Vector3 halfSize = box.size * 0.5f;

        float x = Random.Range(-halfSize.x, halfSize.x);
        float z = Random.Range(-halfSize.z, halfSize.z);

        Vector3 localOffset = new Vector3(x, 0f, z);
        Vector3 worldPos = center + box.transform.rotation * localOffset;

        return worldPos;
    }
}

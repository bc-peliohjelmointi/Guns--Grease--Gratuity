using System;
using UnityEngine;

public class TurretActivator : MonoBehaviour
{
    [Header("Turret Spawning")]
    public GameObject turretPrefab;
    public bool spawnOnlyOnce = true;

    private GameObject[] turretSpawnAreas;
    private GameObject[] cameraTurret;

    private bool hasSpawned;

    void Start()
    {
        turretSpawnAreas = GameObject.FindGameObjectsWithTag("TurretSpawnArea");
        SpawnTurrets();
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

        foreach (GameObject spawnArea in turretSpawnAreas)
        {
            if (!spawnArea)
                continue;

            Instantiate(
                turretPrefab,
                spawnArea.transform.position,
                spawnArea.transform.rotation
            );
        }

        hasSpawned = true;
    }

    public void DestroyTurrets()
    {
        cameraTurret = GameObject.FindGameObjectsWithTag("Turret");
        foreach (var turret in cameraTurret)
            GameObject.Destroy(turret);
        Debug.Log("Turrets have been destroyed >:)");
    }
}

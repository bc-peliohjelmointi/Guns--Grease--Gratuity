using UnityEngine;

public class TurretSpawner : MonoBehaviour
{
    [Header("Turret Settings")]
    public GameObject turretPrefab;
    public float spawnRadius = 10f;

    [Header("Spawn Chance (0–1)")]
    public float spawnChance = 0.25f; // 25%

    void Start()
    {
        TrySpawnTurrets();
    }

    void TrySpawnTurrets()
    {
        // Roll the chance
        float roll = Random.value;
            
        if (roll > spawnChance)
            return; // No turrets spawn this time

        int amountToSpawn = Random.Range(1, 3); // 1–4

        for (int i = 0; i < amountToSpawn; i++)
        {
            Vector3 randomPos = GetRandomPositionInsideRadius();
            Instantiate(turretPrefab, randomPos, Quaternion.identity);
        }
    }

    Vector3 GetRandomPositionInsideRadius()
    {
        Vector2 circle = Random.insideUnitCircle * spawnRadius;
        Vector3 pos3D = new Vector3(circle.x, 0f, circle.y);
        return transform.position + pos3D;
    }
}

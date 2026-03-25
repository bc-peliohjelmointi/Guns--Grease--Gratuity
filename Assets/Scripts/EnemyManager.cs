using UnityEngine;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    [Header("Spawn Rules")]
    [Range(0f, 1f)]
    public float spawnChance = 0.67f;   // % chance per spawn point
    public int minEnemies = 5;
    public int maxEnemies = 25;

    [Header("Enemy Prefabs")]
    public GameObject[] enemyPrefabs; // list of enemy prefabs

    [Header("Spawn Area")]
    public Transform[] spawnPoints;    // where enemies can spawn

    private List<GameObject> activeEnemies = new List<GameObject>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        GenerateEnemies();
    }

    // Generate enemies
    public void GenerateEnemies()
    {
        // Remove old enemies
        foreach (GameObject enemy in activeEnemies)
            Destroy(enemy);
        activeEnemies.Clear();

        List<Transform> selectedSpawnPoints = new List<Transform>();

        // ===== ENEMY ALERT =====
        int adjustedMinEnemies = minEnemies;
        int adjustedMaxEnemies = maxEnemies;

        DeliveryModifierSystem modSystem = FindFirstObjectByType<DeliveryModifierSystem>();
        if (modSystem != null && modSystem.IsEnemyAlertActive())
        {
            adjustedMinEnemies = Mathf.RoundToInt(minEnemies * 1.5f);
            adjustedMaxEnemies = Mathf.RoundToInt(maxEnemies * 1.5f);
            Debug.Log($"Enemy Alert! Min: {minEnemies}->{adjustedMinEnemies}, Max: {maxEnemies}->{adjustedMaxEnemies}");
        }
        

        // Randomly select spawn points
        foreach (Transform point in spawnPoints)
        {
            if (Random.value <= spawnChance)
                selectedSpawnPoints.Add(point);
        }

        // Ensure MIN 
        while (selectedSpawnPoints.Count < adjustedMinEnemies && selectedSpawnPoints.Count < spawnPoints.Length)
        {
            Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            if (!selectedSpawnPoints.Contains(randomPoint))
                selectedSpawnPoints.Add(randomPoint);
        }

        // Ensure MAX 
        while (selectedSpawnPoints.Count > adjustedMaxEnemies)
        {
            selectedSpawnPoints.RemoveAt(Random.Range(0, selectedSpawnPoints.Count));
        }

        // Spawn enemy prefabs
        foreach (Transform point in selectedSpawnPoints)
        {
            if (enemyPrefabs.Length == 0) continue;

            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            GameObject enemy = Instantiate(prefab, point.position, point.rotation);
            activeEnemies.Add(enemy);
        }

        Debug.Log($"Enemies spawned: {activeEnemies.Count} (Min: {adjustedMinEnemies}, Max: {adjustedMaxEnemies})");
    }

    // Public method to get active enemy count
    public int GetActiveEnemyCount()
    {
        return activeEnemies.Count;
    }

    // Public method to clear all enemies 
    public void ClearAllEnemies()
    {
        foreach (GameObject enemy in activeEnemies)
            Destroy(enemy);
        activeEnemies.Clear();
        Debug.Log("All enemies cleared");
    }
}
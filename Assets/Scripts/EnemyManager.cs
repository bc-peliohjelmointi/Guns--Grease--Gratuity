using UnityEngine;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    [Header("Spawn Rules")]
    [Range(0f, 1f)]
    public float spawnChance = 0.67f;   // 67% mahdollisuus per spawn point

    public int minEnemies = 5;
    public int maxEnemies = 25;

    [Header("Turret Prefabs")]
    public GameObject[] enemyPrefabs; // lista turretti prefabeista

    [Header("Spawn Area")]
    public Transform[] spawnPoints;    // minne turretti voi ilmestyä

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

    // -----------------------
    // GENERATE enemies
    // -----------------------
    public void GenerateEnemies()
    {
        // Poista vanhat
        foreach (GameObject turret in activeEnemies)
            Destroy(turret);
        activeEnemies.Clear();

        List<Transform> selectedSpawnPoints = new List<Transform>();

        DeliveryModifierSystem modSystem = FindAnyObjectByType<DeliveryModifierSystem>();
        int enemyCount = maxEnemies;

        // Check if Enemy Alert is active
        //if (modSystem != null && modSystem.HasModifier("Enemy Alert"))
        //{
          //  enemyCount = Mathf.RoundToInt(enemyCount * 1.5f);
        //}
        
        // Satunnaisesti valitaan spawn pointit
        foreach (Transform point in spawnPoints)
        {
            if (Random.value <= spawnChance)
                selectedSpawnPoints.Add(point);
        }

        // Varmista MIN
        while (selectedSpawnPoints.Count < minEnemies && selectedSpawnPoints.Count < spawnPoints.Length)
        {
            Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            if (!selectedSpawnPoints.Contains(randomPoint))
                selectedSpawnPoints.Add(randomPoint);
        }

        // Varmista MAX
        while (selectedSpawnPoints.Count > maxEnemies)
        {
            selectedSpawnPoints.RemoveAt(Random.Range(0, selectedSpawnPoints.Count));
        }

        // Spawn turretti prefabit
        foreach (Transform point in selectedSpawnPoints)
        {
            if (enemyPrefabs.Length == 0) continue;

            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            GameObject enemy = Instantiate(prefab, point.position, point.rotation);
            activeEnemies.Add(enemy);
        }

        Debug.Log("Turrets spawned: " + activeEnemies.Count);
    }
}

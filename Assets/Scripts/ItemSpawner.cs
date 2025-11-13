using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Item Settings")]
    public GameObject itemPrefab;

    private Transform[] spawnPoints;

    void Start()
    {
        var spawnPointObjects = GameObject.FindGameObjectsWithTag("Spawner");
        spawnPoints = new Transform[spawnPointObjects.Length];
        for (int i = 0; i < spawnPoints.Length; i++)
            spawnPoints[i] = spawnPointObjects[i].transform;
    }

    public void SpawnItem()
    {
        if (itemPrefab == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("ItemSpawner: Missing itemPrefab or spawn points!");
            return;
        }

        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Instantiate(itemPrefab, point.position, point.rotation);
    }
}

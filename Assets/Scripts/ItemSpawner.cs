using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Item Settings")]
    public GameObject itemPrefab; // The item to spawn

    [Header("Spawn Points")]
    private string spawnPointTag = "Spawner"; // Tag to search for
    private Transform[] spawnPoints; // Automatically filled at runtime

    void Start()
    {
        // Find all GameObjects with the specified tag
        GameObject[] spawnPointObjects = GameObject.FindGameObjectsWithTag(spawnPointTag);

        // Convert GameObjects to Transforms
        spawnPoints = new Transform[spawnPointObjects.Length];
        for (int i = 0; i < spawnPointObjects.Length; i++)
        {
            spawnPoints[i] = spawnPointObjects[i].transform;
        }

        // Spawn one item
        SpawnItem();
    }

    void SpawnItem()
    {
        if (itemPrefab == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("ItemSpawner: Missing itemPrefab or no spawn points found!");
            return;
        }

        // Pick a random spawn point
        int randomIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[randomIndex];

        // Spawn the item at that location
        Instantiate(itemPrefab, spawnPoint.position, spawnPoint.rotation);
    }
}

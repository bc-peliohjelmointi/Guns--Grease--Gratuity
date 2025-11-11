using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Item Settings")]
    public GameObject itemPrefab;

    private string spawnPointTag = "Spawner";
    private Transform[] spawnPoints;

    void Start()
    {
        GameObject[] spawnPointObjects = GameObject.FindGameObjectsWithTag(spawnPointTag);

        spawnPoints = new Transform[spawnPointObjects.Length];
        for (int i = 0; i < spawnPointObjects.Length; i++)
        {
            spawnPoints[i] = spawnPointObjects[i].transform;
        }
    }

    public void SpawnItem()
    {
        if (itemPrefab == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("ItemSpawner: Missing itemPrefab or spawn points!");
            return;
        }

        int randomIndex = Random.Range(0, spawnPoints.Length);
        Transform point = spawnPoints[randomIndex];

        Instantiate(itemPrefab, point.position, point.rotation);
    }
}

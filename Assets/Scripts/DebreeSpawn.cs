using UnityEngine;

public class DebreeSpawn : MonoBehaviour
{
    public float spawnChance = 1f;

    private void Start()
    {
        SpawnDebree();
    }

    void SpawnDebree()
    {
        if (Random.value > spawnChance)
        {
            gameObject.SetActive(false);
        }
    }
}

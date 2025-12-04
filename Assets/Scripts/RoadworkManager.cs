using UnityEngine;
using System.Collections.Generic;

public class RoadworkManager : MonoBehaviour
{
    public static RoadworkManager Instance;

    [Header("Spawn Rules")]
    [Range(0f, 1f)]
    public float spawnChance = 0.67f;   // 67% chance per roadwork

    public int minClosedPerDay = 2;
    public int maxClosedPerDay = 10;

    private List<GameObject> allRoadworks = new List<GameObject>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        FindAllRoadworks();
        GenerateNewDayRoadworks();
    }

    // -----------------------
    // FIND ALL ROADWORK OBJECTS
    // -----------------------
    void FindAllRoadworks()
    {
        allRoadworks.Clear();

        GameObject[] found = GameObject.FindGameObjectsWithTag("Roadwork");

        foreach (GameObject obj in found)
        {
            allRoadworks.Add(obj);
        }

        Debug.Log("Found Roadworks: " + allRoadworks.Count);
    }

    // -----------------------
    // GENERATE NEW DAY SET
    // -----------------------
    public void GenerateNewDayRoadworks()
    {
        // Disable all first
        foreach (GameObject obj in allRoadworks)
            obj.SetActive(false);

        List<GameObject> selected = new List<GameObject>();

        // Roll chance per roadwork
        foreach (GameObject obj in allRoadworks)
        {
            if (Random.value <= spawnChance)
                selected.Add(obj);
        }

        // Enforce MIN
        while (selected.Count < minClosedPerDay && selected.Count < allRoadworks.Count)
        {
            GameObject random = allRoadworks[Random.Range(0, allRoadworks.Count)];

            if (!selected.Contains(random))
                selected.Add(random);
        }

        // Enforce MAX
        while (selected.Count > maxClosedPerDay)
        {
            selected.RemoveAt(Random.Range(0, selected.Count));
        }

        // Activate selected roadworks
        foreach (GameObject obj in selected)
            obj.SetActive(true);

        Debug.Log("Roads closed today: " + selected.Count);
    }
}

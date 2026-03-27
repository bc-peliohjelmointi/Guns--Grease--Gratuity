using UnityEngine;

public class Resettable : MonoBehaviour
{
    Vector3 startPos;
    Quaternion startRot;

    EnemyAI enemy;

    void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;
        enemy = GetComponent<EnemyAI>();
    }

    public void ResetObject()
    {
        gameObject.SetActive(true);

        transform.position = startPos;
        transform.rotation = startRot;

        gameObject.SetActive(true);

        if (enemy != null)
        {
            enemy.ResetEnemy(); // we add this next
        }
    }
}
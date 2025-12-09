using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;
    public Transform player;
    private DeliverySystem delivery;
    public EnemyGunProjectile gun;

    [Header("Ranges")]
    public float sightRange = 12f;
    public float attackRange = 8f;
    public LayerMask whatIsPlayer;

    private bool playerInSightRange;
    private bool playerInAttackRange;

    [Header("Patrol")]
    public float walkPointRange = 10f;
    private Vector3 walkPoint;
    private bool walkPointSet;
    public LayerMask whatIsGround;

    [Header("Stats")]
    public float health = 50f;

    private void Awake()
    {
        player = GameObject.Find("Player").transform;

        if (player != null)
            delivery = player.GetComponent<DeliverySystem>();

        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange)
            Patrolling();

        if (playerInSightRange && !playerInAttackRange)
            ChasePlayer();

        if (playerInAttackRange)
            AttackPlayer();
    }

    private void Patrolling()
    {
        if (!walkPointSet)
            SearchWalkPoint();

        agent.SetDestination(walkPoint);

        if (Vector3.Distance(transform.position, walkPoint) < 1f)
            walkPointSet = false;
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(
            transform.position.x + randomX,
            transform.position.y,
            transform.position.z + randomZ
        );

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
    }

    private void ChasePlayer()
    {
        if (player != null)
            agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        if (player != null)
            transform.LookAt(player);

        // Enemy liikkuu kohti pelaajaa samalla kun ampuu
        if (player != null)
            agent.SetDestination(player.position);

        // Ammutaan vain jos pelaajalla on paketti
        if (delivery != null && delivery.hasPackage)
            gun.TryShoot(player);
    }

    public void TakeDamage(float dmg)
    {
        health -= dmg;

        if (health <= 0)
            Destroy(gameObject);
    }
}

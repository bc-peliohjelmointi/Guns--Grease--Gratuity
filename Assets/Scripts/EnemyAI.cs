using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;
    public Transform player;
    private DeliverySystem delivery;

    [Header("Attack Settings")]
    public float timeBetweenAttacks = 1f;
    private bool alreadyAttacked;

    [Header("Shooting Settings")]
    public EnemyGunHitscan enemyGun; // oma hitscan ase

    [Header("Patrol Settings")]
    public float walkPointRange = 10f;
    private Vector3 walkPoint;
    private bool walkPointSet;

    [Header("Detection")]
    public LayerMask whatIsGround;
    public LayerMask whatIsPlayer;
    public float sightRange = 8f;
    public float attackRange = 2f;
    private bool playerInSightRange;
    private bool playerInAttackRange;

    [Header("Enemy Stats")]
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
        // RANGE CHECK
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        // STATES
        if (!playerInSightRange && !playerInAttackRange) Patrolling();
        if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        if (playerInSightRange && playerInAttackRange) AttackPlayer();
    }

    private void Patrolling()
    {
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
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
        // Stop movement
        agent.SetDestination(transform.position);

        if (player != null)
            transform.LookAt(player);

        if (!alreadyAttacked)
        {
            // Ampuu vain, jos pelaajalla on paketti
            if (delivery != null && delivery.hasPackage && enemyGun != null)
            {
                enemyGun.TryShoot(delivery);
            }

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    public void TakeDamage(float dmg)
    {
        health -= dmg;
        if (health <= 0f)
        {
            Destroy(gameObject);
        }
    }
}

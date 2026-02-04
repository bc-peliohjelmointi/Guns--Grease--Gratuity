using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;
    public Transform player;
    public LayerMask whatIsGround;
    public LayerMask whatIsPlayer;

    [Header("Health")]
    public float maxHealth = 100f;
    public float health = 100f;

    [Header("Patrolling")]
    public float walkPointRange = 10f;
    private Vector3 walkPoint;
    private bool walkPointSet;

    [Header("Attack Settings")]
    public float timeBetweenAttacks = 1.5f;
    public float damageToPlayer = 10f;
    public float damageToDelivery = 15f;
    public GameObject projectile; // for projectile attacks
    public Transform attackPoint; // where projectiles spawn
    public float aimHeight = 1.2f; // Aims at player body height
    private bool alreadyAttacked;

    [Header("Detection Ranges")]
    public float sightRange = 15f;
    public float attackRange = 5f;

    private bool playerInSightRange;
    private bool playerInAttackRange;
    private StarterAssets.FirstPersonController playerController;
    private DeliverySystem deliverySystem;

    private void Awake()
    {
        // Find player
        GameObject playerObj = GameObject.Find("PlayerArmature");
        if (playerObj == null)
            playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
            playerController = playerObj.GetComponent<StarterAssets.FirstPersonController>();
            deliverySystem = playerObj.GetComponent<DeliverySystem>();
            Debug.Log("Enemy AI: Found player at " + player.position);
        }

        // Get or add NavMeshAgent
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("Enemy AI: No NavMeshAgent found!");
        }

        health = maxHealth;
    }

    private void Update()
    {
        if (player == null)
        {
            Debug.LogWarning("Enemy AI: Player is null!");
            return;
        }

        // Check detection ranges
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        // State machine
        if (!playerInSightRange && !playerInAttackRange)
            Patrolling();
        else if (playerInSightRange && !playerInAttackRange)
            ChasePlayer();
        else if (playerInAttackRange && playerInSightRange)
            AttackPlayer();
    }

    private void Patrolling()
    {
        if (!walkPointSet)
            SearchWalkPoint();

        if (walkPointSet)
        {
            agent.SetDestination(walkPoint);
            
        }

        // Check if reached walk point
        Vector3 distanceToWalkPoint = transform.position - walkPoint;
        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }

    private void SearchWalkPoint()
    {
        // Generate random point around current position
        float randomX = Random.Range(-walkPointRange, walkPointRange);
        float randomZ = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(
            transform.position.x + randomX,
            transform.position.y,
            transform.position.z + randomZ
        );

        // Try to find point on NavMesh instead of using ground raycast
        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(walkPoint, out hit, walkPointRange, UnityEngine.AI.NavMesh.AllAreas))
        {
            walkPoint = hit.position;
            walkPointSet = true;
        }
        
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        agent.SetDestination(transform.position);

        Vector3 targetPos = player.position + Vector3.up * aimHeight;
        float distance = Vector3.Distance(transform.position, player.position);

        // Rotate toward player (Y only)
        Vector3 direction = targetPos - transform.position;
        direction.y = 0f;
        direction.Normalize();

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            lookRotation,
            Time.deltaTime * 5f
        );

        if (alreadyAttacked)
            return;

        // --------------------
        // MELEE
        // --------------------
        if (distance <= 2f)
        {
            if (playerController != null)
                playerController.TakeDamage(damageToPlayer);

            if (deliverySystem != null && deliverySystem.hasPackage)
                deliverySystem.TakeDamage(damageToDelivery);

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
            return;
        }

        // --------------------
        // RANGED
        // --------------------
        if (distance <= 8f && projectile != null)
        {
            Transform spawnPoint = attackPoint != null ? attackPoint : transform;
            GameObject proj = Instantiate(projectile, spawnPoint.position, Quaternion.identity);

            Rigidbody rb = proj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 shootDirection =
                    (targetPos - spawnPoint.position).normalized;

                Projectile projScript = proj.GetComponent<Projectile>();
                float speed = projScript != null ? projScript.speed : 20f;

                rb.AddForce(shootDirection * speed, ForceMode.Impulse);
            }

            Projectile projectileScript = proj.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                projectileScript.damage = damageToPlayer;
                projectileScript.deliveryDamage = damageToDelivery;
            }

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }



    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0)
            DestroyEnemy();
    }

    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // Attack range (red)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Sight range (yellow)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        // Patrol range (blue)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, walkPointRange);
    }
}
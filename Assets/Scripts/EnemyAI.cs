using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;
    public Transform player;
    private DeliverySystem delivery;
    public EnemyGunProjectile gun; // viittaus aseeseen (child)

    [Header("Detection")]
    public float sightRange = 20f;        // kauempaa havainto
    public float attackRange = 12f;       // etäisyys, jolla ampuminen aktivoituu
    public LayerMask whatIsPlayer;        // pelaajan layer
    public LayerMask obstructionMask;     // esteet (seinät yms)
    public Transform eyePoint;            // mistä raycast lähtee (voisi olla ase/firePoint tai enemy head)

    [Header("Patrol")]
    public float walkPointRange = 10f;
    public LayerMask whatIsGround;
    private Vector3 walkPoint;
    private bool walkPointSet;

    [Header("Stats")]
    public float health = 50f;

    bool playerInSight;
    bool playerInAttackRange;

    private void Awake()
    {
        if (player == null)
        {
            var p = GameObject.Find("Player");
            if (p != null) player = p.transform;
        }

        if (player != null)
            delivery = player.GetComponent<DeliverySystem>();

        agent = GetComponent<NavMeshAgent>();
        if (eyePoint == null) eyePoint = transform; // fallback
    }

    private void Update()
    {
        // Tarkista pelaajan etäisyys ensin
        float dist = player == null ? Mathf.Infinity : Vector3.Distance(transform.position, player.position);
        playerInSight = false;
        playerInAttackRange = false;

        if (player != null && dist <= sightRange)
        {
            // Raycast tarkistus: onko näköyhteys pelaajaan?
            Vector3 dir = (player.position - eyePoint.position).normalized;
            if (!Physics.Raycast(eyePoint.position, dir, dist, obstructionMask))
            {
                // ei esteitä -> pelaaja näkyvissä
                playerInSight = true;
            }
        }

        if (player != null && dist <= attackRange)
            playerInAttackRange = true;

        // State-päättely
        if (!playerInSight && !playerInAttackRange)
            Patrolling();
        else if (playerInSight && !playerInAttackRange)
            ChasePlayer();
        else if (playerInSight && playerInAttackRange)
            AttackPlayer();
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
        if (player == null) return;

        // Käänny kohti pelaajaa, mutta älä lukitse paikalleen — liikkuu samalla
        Vector3 lookPos = player.position;
        lookPos.y = transform.position.y;
        transform.LookAt(lookPos);

        agent.SetDestination(player.position);

        // Ampuminen vain, jos pelaajalla on paketti
        if (delivery != null && delivery.hasPackage && gun != null)
        {
            gun.TryShoot(player);
        }
    }

    public void TakeDamage(float dmg)
    {
        health -= dmg;
        if (health <= 0f)
            Destroy(gameObject);
    }

    // Visual debug: näytetään sightRange ray, editorissa näkyy
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        if (eyePoint != null && player != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 dir = (player.position - eyePoint.position).normalized;
            Gizmos.DrawLine(eyePoint.position, eyePoint.position + dir * Mathf.Min(sightRange, Vector3.Distance(eyePoint.position, player.position)));
        }
    }
}

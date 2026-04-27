using UnityEngine;
using UnityEngine.AI;
using System.Collections;

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
    public float walkPointRange = 20f;
    private Vector3 walkPoint;
    private bool walkPointSet;

    [Header("Movement Speeds")]
    public float patrolSpeed = 2.5f;
    public float chaseSpeed = 4.5f;
    public float strafeSpeed = 3.5f;

    [Header("Attack Settings")]
    public float timeBetweenAttacks = 1.5f;
    public float damageToPlayer = 10f;
    public float damageToDelivery = 15f;
    public GameObject projectile; // for projectile attacks
    public Transform attackPoint; // where projectiles spawn
    public float aimHeight = 1.2f; // Aims at player body height
    private bool alreadyAttacked;

    [Header("Melee Indicator")]
    public GameObject meleeIndicator;
    public float meleeIndicatorDuration = 0.4f;
    public float flickerInterval = 0.08f;

    [Header("Detection Ranges")]
    public float sightRange = 15f;
    public float shootRange = 5f;

    private bool playerInSightRange;
    private bool playerInAttackRange;
    private StarterAssets.FirstPersonController playerController;
    private DeliverySystem deliverySystem;

    [Header("Vision")]
    public LayerMask visionMask; // Walls + Player

    [Header("Repositioning")]
    public float repositionRadius = 6f;
    public float repositionCooldown = 2f;
    public float wallOffset = 1f;

    private float lastRepositionTime;

    // [Header("Strafing")]
    // public float strafeDistance = 2f;
    // public float strafeSpeed = 3.5f;
    // public float strafeCooldown = 1.5f;

    [Header("Strafing")]
    public Vector2 strafeIntervalRange = new Vector2(2f, 5f);

    private float nextStrafeTime;
    private int strafeSide = 1; // 1 = right, -1 = left

    [Header("Model Rotation")]
    public Transform upperBody;   // gun / torso
    public Transform lowerBody;   // wheels / base

    public float bodyTurnSpeed = 8f;
    public float baseTurnSpeed = 6f;

    [Header("Sound")]
    public AudioSource audioSource;

    public AudioClip shootSound;
    public AudioClip hitSound;
    public AudioClip deathSound;

    [Header("Effects")]
    public GameObject deathSmokeEffect;

    [Header("UI")]
    public GameObject EnemyBonusTextPrefab;
    public Transform popupAnchor;

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

        // Find popup anchor automatically
        if (popupAnchor == null)
        {
            GameObject anchorObj = GameObject.FindGameObjectWithTag("PopupAnchor");
            if (anchorObj != null)
            {
                popupAnchor = anchorObj.transform;
            }
            else
            {
                Debug.LogWarning("PopupAnchor not found in scene!");
            }
        }
    }

    private void Update()
    {
        if (DaySystem.IsResetting)
            return;

        if (player == null)
        {
            Debug.LogWarning("Enemy AI: Player is null!");
            return;
        }

        if (playerInSightRange)
        {
            RotateUpperBodyTowardPlayer();
        }

        // Check detection ranges
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, shootRange, whatIsPlayer);

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
        agent.speed = patrolSpeed;

        if (!walkPointSet)
            SearchWalkPoint();

        if (walkPointSet)
        {
            agent.SetDestination(walkPoint);
            
        }

        // Check if reached walk point
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            walkPointSet = false;
        }
    }

    private void SearchWalkPoint()
    {
        walkPointRange = Random.Range(5, 20);

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
        if (UnityEngine.AI.NavMesh.SamplePosition(walkPoint, out hit, walkPointRange, UnityEngine.AI.NavMesh.AllAreas) && Vector3.Distance(transform.position, hit.position) > 5f)
        {
            walkPoint = hit.position;
            walkPointSet = true;
        }
        
    }

    private void ChasePlayer()
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        Vector3 targetPos = player.position + Vector3.up * aimHeight;
        float distance = Vector3.Distance(transform.position, player.position);

        bool hasLOS = HasLineOfSight(targetPos);

        // Reposition if line of sight blocked
        if (!hasLOS)
        {
            
            Reposition();
            return;
        }

        if (hasLOS)
        {
            Strafe();
        }

        // Rotate toward player (Y only)
        Vector3 direction = targetPos - transform.position;
        direction.y = 0f;
        direction.Normalize();

        // Rotate LOWER BODY toward movement direction
        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            Vector3 moveDir = agent.velocity.normalized;
            moveDir.y = 0f;

            Quaternion baseRot = Quaternion.LookRotation(moveDir);
            lowerBody.rotation = Quaternion.Slerp(
                lowerBody.rotation,
                baseRot,
                Time.deltaTime * baseTurnSpeed
            );
        }


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

            StartCoroutine(MeleeIndicatorFlicker());

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
            return;
        }

        // --------------------
        // RANGED
        // --------------------
        if (distance <= shootRange + 1 && projectile != null)
        {
            if (!HasLineOfSight(targetPos))
                return;

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
                AudioSource.PlayClipAtPoint(shootSound, upperBody.position, 1f);
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

    IEnumerator MeleeIndicatorFlicker()
    {
        if (meleeIndicator == null)
            yield break;

        meleeIndicator.SetActive(true);

        float timer = 0f;
        bool state = true;

        while (timer < meleeIndicatorDuration)
        {
            state = !state;
            meleeIndicator.SetActive(state);

            yield return new WaitForSeconds(flickerInterval);
            timer += flickerInterval;
        }

        meleeIndicator.SetActive(false);
    }

    bool HasLineOfSight(Vector3 targetPos)
    {
        Vector3 origin = transform.position + Vector3.up * 1.2f; // enemy eye height
        Vector3 dir = (targetPos - origin).normalized;
        float distance = Vector3.Distance(origin, targetPos);

        if (Physics.Raycast(origin, dir, out RaycastHit hit, distance, visionMask))
        {
            return hit.transform.CompareTag("Player") || hit.transform.CompareTag("PlayerScooter");
        }

        return false;
    }

    bool HasLineOfSightFrom(Vector3 from, Vector3 targetPos)
    {
        Vector3 origin = from + Vector3.up * 1.2f;
        Vector3 dir = (targetPos - origin).normalized;
        float dist = Vector3.Distance(origin, targetPos);

        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist, visionMask))
        {
            return hit.transform.CompareTag("Player") || hit.transform.CompareTag("PlayerScooter");
        }

        return false;
    }


    void Reposition()
    {
        if (Time.time < lastRepositionTime + repositionCooldown)
            return;

        lastRepositionTime = Time.time;

        Vector3 playerAimPos = player.position + Vector3.up * aimHeight;

        for (int i = 0; i < 8; i++)
        {
            Vector3 randomDir = Random.insideUnitSphere * repositionRadius;
            randomDir.y = 0f;

            Vector3 candidate = player.position + randomDir;

            if (!NavMesh.SamplePosition(candidate, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
                continue;

            Vector3 rayOrigin = navHit.position + Vector3.up * 1.2f;
            Vector3 dirToPlayer = (playerAimPos - rayOrigin).normalized;
            float distance = Vector3.Distance(rayOrigin, playerAimPos);

            // Raycast toward player
            if (Physics.Raycast(rayOrigin, dirToPlayer, out RaycastHit hit, distance, visionMask))
            {
                // Repositions the enemy away from a wall
                if (!hit.transform.CompareTag("Player"))
                {
                    Vector3 offsetPos = hit.point + hit.normal * wallOffset;

                    if (NavMesh.SamplePosition(offsetPos, out NavMeshHit offsetHit, 1.5f, NavMesh.AllAreas))
                    {
                        // Final LOS check
                        if (HasLineOfSightFrom(offsetHit.position, playerAimPos))
                        {
                            agent.SetDestination(offsetHit.position);
                            return;
                        }
                    }
                }
                else
                {
                    // Direct LOS safe spot
                    agent.SetDestination(navHit.position);
                    return;
                }
            }
        }
    }

    void Strafe()
    {
        if (Time.time < nextStrafeTime)
            return;

        // Only strafe if LOS is clear
        Vector3 targetPos = player.position + Vector3.up * aimHeight;
        if (!HasLineOfSight(targetPos))
            return;

        float nextInterval = Random.Range(2f, 5f);
        nextStrafeTime = Time.time + nextInterval;

        // 33% chance to keep same direction
        if (Random.value > 0.33f)
            strafeSide *= -1;

        agent.speed = strafeSpeed;

        Vector3 fromPlayer = (transform.position - player.position).normalized;
        fromPlayer.y = 0f;

        Vector3 sideDir =
            Quaternion.Euler(0f, 90f * strafeSide, 0f) * fromPlayer;

        Vector3 desiredPos =
            player.position + sideDir * shootRange;

        if (NavMesh.SamplePosition(desiredPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }

        Debug.DrawLine(transform.position, desiredPos, Color.green, 1f);
    }


    void RotateUpperBodyTowardPlayer()
    {
        if (upperBody == null || player == null)
            return;

        Vector3 targetPos = player.position + Vector3.up * aimHeight;
        Vector3 dir = targetPos - upperBody.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        upperBody.rotation = Quaternion.Slerp(
            upperBody.rotation,
            targetRot,
            Time.deltaTime * bodyTurnSpeed
        );
    }


    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        AudioSource.PlayClipAtPoint(hitSound, upperBody.position, 1f);

        if (health <= 0)
            StartCoroutine(EnemyDie());
            
    }

    private IEnumerator EnemyDie()
    {
        int killReward = GetKillReward();
        PlayerStats.Instance.AddMoney(killReward);

        ShowEnemyBonusText(killReward);

        Instantiate(deathSmokeEffect, transform.position, Quaternion.identity);

        yield return new WaitForSeconds(0.1f);

        gameObject.SetActive(false);
    }

    private void ShowEnemyBonusText(int amount)
    {
        if (EnemyBonusTextPrefab == null || popupAnchor == null) return;

        GameObject popup = Instantiate(EnemyBonusTextPrefab, popupAnchor);

        RectTransform rt = popup.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;

        EnemyBonusText ebt = popup.GetComponent<EnemyBonusText>();
        if (ebt != null)
        {
            ebt.SetText($"<color=green>Enemy bonus!\n+ ${amount}</color>");
        }
    }

    private int GetKillReward()
    {
        // roll random number from 0-99 to determine precentage of bonus
        int roll = Random.Range(0, 100);

        if (roll < 50) return 2; // 50 %
        else if (roll < 75) return 3; // 25 %
        else if (roll < 90) return 4; // 15 %
        else return 5; // 10 %
    }

    private void OnDrawGizmosSelected()
    {
        // Attack range (red)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootRange);

        // Sight range (yellow)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        // Patrol range (blue)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, walkPointRange);
    }

    public void ResetEnemy()
    {
        // Reset health
        health = maxHealth;

        // Reset attack state
        alreadyAttacked = false;
        CancelInvoke(); // stops Invoke(nameof(ResetAttack))

        // Reset movement
        if (agent != null)
        {
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }

        // Reset patrol
        walkPointSet = false;

        // Reset timers
        nextStrafeTime = 0f;
        lastRepositionTime = 0f;

        transform.rotation = Quaternion.identity;

        Debug.Log("Enemy reset: " + gameObject.name);
    }
}
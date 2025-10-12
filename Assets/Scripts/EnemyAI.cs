using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRadius = 15f;
    public LayerMask targetLayer;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float waitTimeAtPoint = 1.5f;

    [Header("Attack")]
    public GameObject projectilePrefab;
    public Transform shootPoint;
    public float attackCooldown = 1.2f;
    public float projectileSpeed = 10f;
    public float shootingRange = 8f;

    [Header("Combat Movement")]
    public float stopDistance = 3f;
    private float patrolStoppingDistance = 0.1f; // для патруля

    private NavMeshAgent agent;
    private Transform player;
    private bool hasSpottedPlayer = false;

    private int currentPatrolIndex;
    private float waitTimer;
    private float attackTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = stopDistance;

        if (patrolPoints.Length > 0)
        {
            currentPatrolIndex = 0;
            agent.SetDestination(patrolPoints[0].position);
        }
    }

    void Update()
    {
        attackTimer -= Time.deltaTime;

        if (!hasSpottedPlayer)
        {
            DetectPlayer();
        }

        if (hasSpottedPlayer)
        {
            FollowAndAttackPlayer();
        }
        else
        {
            Patrol();
        }
    }

    void DetectPlayer()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, targetLayer);
        foreach (var hit in hits)
        {
            if (hit.transform == transform) continue;

            Vector3 eyePos = transform.position + Vector3.up * 1.0f; // глаз врага
            Vector3 dir = (hit.transform.position - eyePos).normalized;
            float distance = Vector3.Distance(eyePos, hit.transform.position);

            // проверяем, что нет препятствий
            if (!Physics.Raycast(eyePos, dir, distance, ~targetLayer))
            {
                player = hit.transform;
                hasSpottedPlayer = true;
                Debug.Log("[EnemyAI] Player spotted!");
                return;
            }
        }
    }


    void FollowAndAttackPlayer()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > stopDistance)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
        else
        {
            agent.isStopped = true;
        }

        // Поворот к игроку
        Vector3 lookDir = player.position - transform.position;
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 8f);

        // Стрельба
        if (distance <= shootingRange && attackTimer <= 0f)
        {
            Attack();
        }
    }

    void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        Vector3 targetPoint = patrolPoints[currentPatrolIndex].position;
        float distanceToPoint = Vector3.Distance(transform.position, targetPoint);

        // Если достигли точки (с учётом небольшого порога)
        if (distanceToPoint <= 0.1f)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTimeAtPoint)
            {
                // Смена на следующую точку
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                agent.destination = patrolPoints[currentPatrolIndex].position;
                agent.stoppingDistance = 0f; // для патруля подходить вплотную
                waitTimer = 0f;
            }
            agent.isStopped = true; // ждем на точке
        }
        else
        {
            // Движение к текущей точке
            agent.isStopped = false;
            agent.stoppingDistance = 0f;
            agent.destination = targetPoint;
        }
    }


    void Attack()
    {
        attackTimer = attackCooldown;
        if (projectilePrefab && shootPoint && player != null)
        {
            Vector3 dir = (player.position - shootPoint.position).normalized;
            GameObject proj = Instantiate(projectilePrefab, shootPoint.position, Quaternion.LookRotation(dir));
            if (proj.TryGetComponent<Rigidbody>(out var rb))
                rb.velocity = dir * projectileSpeed;

            Debug.Log("[EnemyAI] Attacked player!");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootingRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}

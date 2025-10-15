using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRadius = 15f;
    public LayerMask targetLayer;
    public LayerMask obstructionMask; // слои, мешающие обзору

    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float waitTimeAtPoint = 1.5f;

    [Header("Combat Movement")]
    public float stopDistance = 3f;
    private float patrolStoppingDistance = 0f;

    private NavMeshAgent agent;
    private Transform player;
    private bool hasSpottedPlayer = false;

    private int currentPatrolIndex;
    private float waitTimer;

    // Для связи со стрельбой
    public Transform PlayerTarget => player;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = patrolStoppingDistance;

        if (patrolPoints.Length > 0)
        {
            currentPatrolIndex = 0;
            agent.SetDestination(patrolPoints[0].position);
        }
    }

    void Update()
    {
        if (!hasSpottedPlayer)
        {
            DetectPlayer();
        }

        if (hasSpottedPlayer && player != null)
        {
            FollowPlayer();
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

            Vector3 eyePos = transform.position + Vector3.up * 1.0f;
            Vector3 dir = (hit.transform.position - eyePos).normalized;
            float distance = Vector3.Distance(eyePos, hit.transform.position);

            // Проверяем наличие препятствий между врагом и целью
            if (!Physics.Raycast(eyePos, dir, distance, obstructionMask))
            {
                player = hit.transform;
                hasSpottedPlayer = true;
                agent.stoppingDistance = stopDistance;
                Debug.Log("[EnemyAI] Player spotted!");
                return;
            }
        }
    }

    void FollowPlayer()
    {
        if (agent == null || !agent.isOnNavMesh)
            return;
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

        // Плавно поворачиваемся к игроку
        Vector3 lookDir = player.position - transform.position;
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * 8f);
        }
    }

    void Patrol()
    {
        if (agent == null || !agent.isOnNavMesh || patrolPoints.Length == 0)
            return;
        if (patrolPoints.Length == 0) return;
        if (patrolPoints == null) return;
        Vector3 targetPoint = patrolPoints[currentPatrolIndex].position;
        float distanceToPoint = Vector3.Distance(transform.position, targetPoint);

        if (distanceToPoint <= 0.2f)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTimeAtPoint)
            {
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                agent.destination = patrolPoints[currentPatrolIndex].position;
                waitTimer = 0f;
            }
            agent.isStopped = true;
        }
        else
        {
            agent.isStopped = false;
            agent.stoppingDistance = patrolStoppingDistance;
            agent.destination = targetPoint;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}

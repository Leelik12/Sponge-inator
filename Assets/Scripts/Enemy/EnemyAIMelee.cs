using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class KamikazeEnemyAI : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRadius = 15f;
    public LayerMask targetLayer;
    public float eyeHeight = 0.6f;

    [Header("Movement")]
    public float stoppingDistance = 0.1f; // почти вплотную к игроку
    public float moveSpeed = 5f;

    private NavMeshAgent agent;
    private Transform player;
    private bool hasSpottedPlayer = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = stoppingDistance;
        agent.speed = moveSpeed;
        agent.updateRotation = true; // агент сам поворачиваетс€ к цели
        agent.isStopped = true;      // стоим на месте пока не заметим игрока
    }

    void Update()
    {
        if (!hasSpottedPlayer)
        {
            DetectPlayer();
        }

        if (hasSpottedPlayer && player != null)
        {
            // Ќачинаем преследовать игрока
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
    }

    void DetectPlayer()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, targetLayer);
        foreach (var hit in hits)
        {
            if (hit.transform == transform) continue;

            Vector3 eyePos = transform.position + Vector3.up * eyeHeight;
            Vector3 dir = (hit.transform.position - eyePos).normalized;
            float distance = Vector3.Distance(eyePos, hit.transform.position);

            // ѕроверка, что между врагом и игроком нет преп€тствий
            if (!Physics.Raycast(eyePos, dir, distance, ~targetLayer))
            {
                player = hit.transform;
                hasSpottedPlayer = true;
                agent.isStopped = false;
                Debug.Log("[KamikazeEnemyAI] Player spotted! Charging!");
                break;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.transform == player)
        {
            // Ќаносим урон игроку, если есть компонент здоровь€
            var health = player.GetComponent<GubkaWater>();
            if (health != null)
            {
                health.maxWaterCapacity--;
            }

            // ”ничтожаем себ€
            Destroy(gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}

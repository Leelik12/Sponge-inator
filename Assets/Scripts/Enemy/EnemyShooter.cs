using UnityEngine;

[RequireComponent(typeof(EnemyAI))]
public class EnemyShooter : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform shootPoint;
    public float gravity = 9.81f;
    public float launchAngle = 45f;
    public float cooldown = 2f;

    private EnemyAI ai;
    private float timer;

    void Start()
    {
        ai = GetComponent<EnemyAI>();
    }

    void Update()
    {
        timer -= Time.deltaTime;

        Transform target = ai.PlayerTarget;
        if (target != null && timer <= 0f)
        {
            ShootAtTarget(target);
            timer = cooldown;
        }
    }

    void ShootAtTarget(Transform target)
    {
        if (projectilePrefab == null || shootPoint == null) return;

        Vector3 startPos = shootPoint.position;
        Vector3 targetPos = target.position;

        Vector3 dir = targetPos - startPos;
        float yOffset = dir.y;
        dir.y = 0;

        float distance = dir.magnitude;
        float angleRad = launchAngle * Mathf.Deg2Rad;

        float velocity = Mathf.Sqrt(distance * gravity / Mathf.Sin(2 * angleRad));
        if (float.IsNaN(velocity)) return;

        Vector3 velocityVector = dir.normalized * velocity * Mathf.Cos(angleRad);
        velocityVector.y = velocity * Mathf.Sin(angleRad);

        GameObject proj = Instantiate(projectilePrefab, startPos, Quaternion.identity);
        if (proj.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.useGravity = true;
            rb.velocity = velocityVector;
        }
    }
}

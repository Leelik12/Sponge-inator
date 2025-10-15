using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class WaterJetProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float lifeTime = 3f; // ����� ����� �������
    [SerializeField] private ParticleSystem trailEffect; // ������ �����
    [SerializeField] private ParticleSystem impactEffect; // ������ ���������

    private LayerMask enemyLayer;
    private WaterJetAttack attackController;
    private WaterJetDamage damageComponent;

    private Rigidbody rb;
    private bool hasHit = false;

    public void Initialize(Vector3 start, Vector3 target, float launchForce,
                          float speed, int projectileDamage, LayerMask enemies,
                          WaterJetAttack controller, float range)
    {
        transform.position = start;
        Vector3 direction = (target - start).normalized;

        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = direction * launchForce; // ���� ������� ����� Rigidbody
        rb.useGravity = true; // ����� �������� ��� ��������� ����������
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        enemyLayer = enemies;
        attackController = controller;

        // ��������� ��������� �����
        damageComponent = gameObject.AddComponent<WaterJetDamage>();
        damageComponent.Initialize(projectileDamage, enemies, controller);

        if (trailEffect != null)
            trailEffect.Play();

        if (damageComponent != null)
            damageComponent.SetActive(true);

        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;
        hasHit = true;

        if (((1 << collision.gameObject.layer) & enemyLayer) != 0)
        {
            if (attackController != null)
                attackController.OnWaterJetHit(transform.position, collision.collider);
        }

        if (trailEffect != null)
            trailEffect.Stop();

        if (impactEffect != null)
        {
            ParticleSystem impact = Instantiate(impactEffect, transform.position, Quaternion.identity);
            impact.Play();
            Destroy(impact.gameObject, 2f);
        }

        if (damageComponent != null)
            damageComponent.SetActive(false);

        Destroy(gameObject, 0.1f);
    }

    public void ForceDestroy()
    {
        if (hasHit) return;
        hasHit = true;
        if (trailEffect != null)
            trailEffect.Stop();
        Destroy(gameObject, 0.1f);
    }
}

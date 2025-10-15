using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SredstvoJetProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float lifeTime = 3f; // ����� ����� �������
    [SerializeField] private ParticleSystem trailEffect; // ������ �����
    [SerializeField] private ParticleSystem impactEffect; // ������ ���������
    [SerializeField] private Animator animator;
    private Rigidbody rb;
    private LayerMask enemyLayer;
    private SredstvoAttack attackController;
    private SredstvoJetDamage damageComponent;

    private bool hasHit = false;

    /// <summary>
    /// ������������� ������� ����
    /// </summary>
    public void Initialize(
        Vector3 start,
        Vector3 target,
        float launchForce,
        float speed,
        int projectileDamage,
        LayerMask enemies,
        SredstvoAttack controller,
        float range)
    {
        animator = FindAnyObjectByType<Animator>();
        transform.position = start;
        Vector3 direction = (target - start).normalized;

        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.useGravity = true;

        // ��������� �������
        rb.linearVelocity = direction * launchForce;

        enemyLayer = enemies;
        attackController = controller;

        // ��������� ��������� �����
        damageComponent = gameObject.AddComponent<SredstvoJetDamage>();
        damageComponent.Initialize(projectileDamage, enemies, controller);
        damageComponent.SetActive(true);

        // ���������� �������
        if (trailEffect != null)
            trailEffect.Play();

        // ���������� �� �������
        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;
        hasHit = true;
        animator.SetBool("Plevok", false);
        // �������� ��������� �� �����
        if (((1 << collision.gameObject.layer) & enemyLayer) != 0)
        {
            if (attackController != null)
                attackController.OnSredstvoJetHit(transform.position, collision.collider);
        }

        // ������������� ����
        if (trailEffect != null)
            trailEffect.Stop();

        // ������ ���������
        if (impactEffect != null)
        {
            ParticleSystem impact = Instantiate(impactEffect, transform.position, Quaternion.identity);
            impact.Play();
            Destroy(impact.gameObject, 2f);
        }

        // ��������� ����
        if (damageComponent != null)
            damageComponent.SetActive(false);

        // ���������� ������
        Destroy(gameObject, 0.1f);
    }

    /// <summary>
    /// �������������� ����������� (��������, ��� ������ �����)
    /// </summary>
    public void ForceDestroy()
    {
        if (hasHit) return;
        hasHit = true;

        if (trailEffect != null)
            trailEffect.Stop();

        Destroy(gameObject, 0.1f);
    }

    public bool HasHit() => hasHit;
}

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SredstvoJetProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float lifeTime = 3f; // Время жизни снаряда
    [SerializeField] private ParticleSystem trailEffect; // Эффект следа
    [SerializeField] private ParticleSystem impactEffect; // Эффект попадания

    private Rigidbody rb;
    private LayerMask enemyLayer;
    private SredstvoAttack attackController;
    private SredstvoJetDamage damageComponent;

    private bool hasHit = false;

    /// <summary>
    /// Инициализация снаряда мыла
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
        transform.position = start;
        Vector3 direction = (target - start).normalized;

        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.useGravity = true;

        // Добавляем импульс
        rb.velocity = direction * launchForce;

        enemyLayer = enemies;
        attackController = controller;

        // Добавляем компонент урона
        damageComponent = gameObject.AddComponent<SredstvoJetDamage>();
        damageComponent.Initialize(projectileDamage, enemies, controller);
        damageComponent.SetActive(true);

        // Визуальные эффекты
        if (trailEffect != null)
            trailEffect.Play();

        // Уничтожаем по времени
        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;
        hasHit = true;

        // Проверка попадания по врагу
        if (((1 << collision.gameObject.layer) & enemyLayer) != 0)
        {
            if (attackController != null)
                attackController.OnSredstvoJetHit(transform.position, collision.collider);
        }

        // Останавливаем след
        if (trailEffect != null)
            trailEffect.Stop();

        // Эффект попадания
        if (impactEffect != null)
        {
            ParticleSystem impact = Instantiate(impactEffect, transform.position, Quaternion.identity);
            impact.Play();
            Destroy(impact.gameObject, 2f);
        }

        // Отключаем урон
        if (damageComponent != null)
            damageComponent.SetActive(false);

        // Уничтожаем снаряд
        Destroy(gameObject, 0.1f);
    }

    /// <summary>
    /// Принудительное уничтожение (например, при отмене атаки)
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

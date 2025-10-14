using UnityEngine;

public class SredstvoJetProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float lifeTime = 3f; // Время жизни снаряда
    [SerializeField] private ParticleSystem trailEffect; // Эффект следа
    [SerializeField] private ParticleSystem impactEffect; // Эффект попадания

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float arcHeight;
    private float speed;
    private float damage;
    private LayerMask enemyLayer;
    private SredstvoAttack attackController;
    private float maxRange;

    private float progress = 0f;
    private bool isLaunched = false;
    private bool hasHit = false;
    private float flightTime;

    public void Initialize(Vector3 start, Vector3 target, float arc, float projectileSpeed,
                          float projectileDamage, LayerMask enemies, SredstvoAttack controller, float range)
    {
        startPosition = start;
        targetPosition = target;
        arcHeight = arc;
        speed = projectileSpeed;
        damage = projectileDamage;
        enemyLayer = enemies;
        attackController = controller;
        maxRange = range;

        // Вычисляем время полета на основе расстояния и скорости
        float distance = Vector3.Distance(startPosition, targetPosition);
        flightTime = distance / speed;

        // Устанавливаем начальную позицию
        transform.position = startPosition;

        // Направляем снаряд в сторону цели
        transform.LookAt(targetPosition);
    }

    public void Launch()
    {
        isLaunched = true;

        if (trailEffect != null)
            trailEffect.Play();

        // Автоматическое уничтожение через время
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (!isLaunched || hasHit) return;

        // Обновляем прогресс полета
        progress += Time.deltaTime / flightTime;

        // Параболическая траектория
        float parabola = 1.0f - 4.0f * (progress - 0.5f) * (progress - 0.5f);
        Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, progress);
        currentPos.y += parabola * arcHeight;

        transform.position = currentPos;

        // Плавно поворачиваем снаряд в направлении движения
        if (progress < 0.95f) // Не поворачиваем в самом конце
        {
            Vector3 moveDirection = (currentPos - transform.position).normalized;
            if (moveDirection != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(moveDirection);
        }

        // Проверяем столкновения
        CheckCollisions();

        // Если достигли цели или превысили максимальную дистанцию
        if (progress >= 1f && !hasHit)
        {
            OnReachedTarget();
        }

        // Дополнительная проверка на превышение дистанции
        float distanceTraveled = Vector3.Distance(startPosition, transform.position);
        if (distanceTraveled >= maxRange && !hasHit)
        {
            OnMaxRangeReached();
        }
    }

    private void CheckCollisions()
    {
        // Проверяем столкновения с помощью сферического каста
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 0.3f, enemyLayer);
        foreach (var hitCollider in hitColliders)
        {
            OnHit(transform.position, hitCollider);
            break; // Обрабатываем только первое столкновение
        }
    }

    private void OnHit(Vector3 hitPoint, Collider hitCollider)
    {
        if (hasHit) return;

        hasHit = true;

        // Останавливаем след
        if (trailEffect != null)
            trailEffect.Stop();

        // Создаем эффект попадания
        if (impactEffect != null)
        {
            ParticleSystem impact = Instantiate(impactEffect, hitPoint, Quaternion.identity);
            impact.Play();
            Destroy(impact.gameObject, 2f);
        }

        // Уведомляем контроллер атаки о попадании
        if (attackController != null)
        {
            attackController.OnWaterJetHit(hitPoint, hitCollider);
        }

        // Уничтожаем снаряд
        Destroy(gameObject, 0.1f);
    }

    private void OnReachedTarget()
    {
        if (hasHit) return;

        // Если не попали во врага, но достигли целевой точки
        OnHit(targetPosition, null);
    }

    private void OnMaxRangeReached()
    {
        if (hasHit) return;

        // Снаряд достиг максимальной дистанции
        Vector3 endPoint = transform.position;
        OnHit(endPoint, null);
    }

    void OnTriggerEnter(Collider other)
    {
        // Дополнительная проверка триггеров
        if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            OnHit(transform.position, other);
        }
    }
}
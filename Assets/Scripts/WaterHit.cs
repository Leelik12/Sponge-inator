using UnityEngine;

public class WaterJetProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float lifeTime = 3f; // Время жизни снаряда
    [SerializeField] private ParticleSystem trailEffect; // Эффект следа
    [SerializeField] private ParticleSystem impactEffect; // Эффект попадания

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float arcHeight;
    private float speed;
    private LayerMask enemyLayer;
    private WaterJetAttack attackController;
    private float maxRange;

    private float progress = 0f;
    private bool isLaunched = false;
    private bool hasHit = false;
    private float flightTime;

    // Компонент для нанесения урона
    private WaterJetDamage damageComponent;

    public void Initialize(Vector3 start, Vector3 target, float arc, float projectileSpeed,
                          int projectileDamage, LayerMask enemies, WaterJetAttack controller, float range)
    {
        startPosition = start;
        targetPosition = target;
        arcHeight = arc;
        speed = projectileSpeed;
        maxRange = range;
        enemyLayer = enemies;
        attackController = controller;

        // Вычисляем время полета на основе расстояния и скорости
        float distance = Vector3.Distance(startPosition, targetPosition);
        flightTime = distance / speed;

        // Устанавливаем начальную позицию
        transform.position = startPosition;

        // Направляем снаряд в сторону цели
        transform.LookAt(targetPosition);

        // Добавляем компонент для нанесения урона
        damageComponent = gameObject.AddComponent<WaterJetDamage>();
        damageComponent.Initialize(projectileDamage, enemies, controller);
    }

    public void Launch()
    {
        isLaunched = true;

        if (trailEffect != null)
            trailEffect.Play();

        // Активируем компонент урона
        if (damageComponent != null)
            damageComponent.SetActive(true);

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

        // Проверяем столкновения через компонент урона
        if (damageComponent != null && damageComponent.HasHit())
        {
            OnHit(transform.position);
            return;
        }

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

    private void OnHit(Vector3 hitPoint)
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

        // Деактивируем компонент урона
        if (damageComponent != null)
            damageComponent.SetActive(false);

        // Уничтожаем снаряд
        Destroy(gameObject, 0.1f);
    }

    private void OnReachedTarget()
    {
        if (hasHit) return;
        OnHit(targetPosition);
    }

    private void OnMaxRangeReached()
    {
        if (hasHit) return;
        OnHit(transform.position);
    }

    // Метод для принудительного уничтожения снаряда
    public void ForceDestroy()
    {
        OnHit(transform.position);
    }

    // Методы для получения состояния
    public bool IsLaunched() => isLaunched;
    public bool HasHit() => hasHit;
    public float GetProgress() => progress;
}
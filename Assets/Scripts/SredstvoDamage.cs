using UnityEngine;

public class SredstvoJetDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float detectionRadius = 0.3f; // Радиус обнаружения врагов
    [SerializeField] private bool debugMode = false; // Режим отладки

    private int damage;
    private LayerMask enemyLayer;
    private SredstvoAttack attackController;
    private bool isActive = false;
    private bool hasHit = false;
    private Collider lastHitCollider;

    // Кэш для оптимизации
    private Collider[] hitColliders = new Collider[10];

    public void Initialize(int projectileDamage, LayerMask enemies, SredstvoAttack controller)
    {
        damage = projectileDamage;
        enemyLayer = enemies;
        attackController = controller;
        isActive = false;
        hasHit = false;
    }

    public void SetActive(bool active)
    {
        isActive = active;
        if (!active)
        {
            hasHit = false;
            lastHitCollider = null;
        }
    }

    void Update()
    {
        if (!isActive || hasHit) return;

        CheckForEnemies();
    }

    void FixedUpdate()
    {
        if (!isActive || hasHit) return;

        // Дополнительная проверка в FixedUpdate для более точной физики
        CheckForEnemies();
    }

    private void CheckForEnemies()
    {
        // Проверяем столкновения с помощью сферического каста
        int enemyCount = Physics.OverlapSphereNonAlloc(transform.position, detectionRadius, hitColliders, enemyLayer);

        for (int i = 0; i < enemyCount; i++)
        {
            Collider hitCollider = hitColliders[i];
            if (hitCollider != null && hitCollider != lastHitCollider)
            {
                OnEnemyHit(hitCollider);
                break; // Обрабатываем только первое столкновение
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isActive || hasHit) return;

        // Дополнительная проверка триггеров
        if (((1 << other.gameObject.layer) & enemyLayer) != 0 && other != lastHitCollider)
        {
            OnEnemyHit(other);
        }
    }

    private void OnEnemyHit(Collider enemyCollider)
    {
        if (hasHit) return;

        hasHit = true;
        lastHitCollider = enemyCollider;

        if (debugMode)
            Debug.Log($"SredstvoJet hit enemy: {enemyCollider.name}");

        // Наносим урон врагу
        ApplyDamage(enemyCollider);

        // Уведомляем контроллер атаки о попадании
        if (attackController != null)
        {
            attackController.OnSredstvoJetHit(transform.position, enemyCollider);
        }

        // Отключаем дальнейшие проверки
        isActive = false;
    }

    private void ApplyDamage(Collider enemyCollider)
    {
        if (enemyCollider == null) return;

        EnemyHealth enemyHealth = enemyCollider.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);

            if (debugMode)
                Debug.Log($"Applied {damage} damage to {enemyCollider.name}");

            // Можно добавить дополнительные эффекты при попадании
            ApplySpecialEffects(enemyCollider);
        }
        else
        {
            if (debugMode)
                Debug.Log($"No EnemyHealth component on {enemyCollider.name}");
        }
    }

    private void ApplySpecialEffects(Collider enemyCollider)
    {
        // Здесь можно добавить специальные эффекты для моющего средства
        // Например, замедление, отравление, очистку и т.д.
    }

    // Методы для получения состояния
    public bool HasHit() => hasHit;
    public bool IsActive() => isActive;
    public float GetDamage() => damage;

    // Метод для принудительного нанесения урона (например, при Area of Effect)
    public void ForceDamage(Collider enemyCollider)
    {
        if (hasHit) return;
        OnEnemyHit(enemyCollider);
    }

    // Метод для изменения радиуса обнаружения во время выполнения
    public void SetDetectionRadius(float newRadius)
    {
        detectionRadius = Mathf.Max(0.1f, newRadius);
    }


    void OnDestroy()
    {
        // Очистка
        hitColliders = null;
        lastHitCollider = null;
    }

    // Визуализация радиуса обнаружения в редакторе
    private void OnDrawGizmosSelected()
    {
        if (debugMode && isActive)
        {
            Gizmos.color = hasHit ? Color.red : Color.green;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }
}
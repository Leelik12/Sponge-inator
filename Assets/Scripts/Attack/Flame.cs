using UnityEngine;

public class FlameStreamController : MonoBehaviour
{
    [Header("Flame Stream Settings")]
    [SerializeField] private Collider flameCollider; // Триггер коллайдер для обнаружения врагов
    [SerializeField] private float damageTickRate = 0.2f; // Как часто наносится урон

    private FlamethrowerAttack flamethrowerAttack;
    private float damageTimer = 0f;
    private bool isActive = false;
    private int damagePerSecond;

    // Список врагов которые сейчас в зоне поражения
    private System.Collections.Generic.List<Collider> enemiesInRange = new System.Collections.Generic.List<Collider>();

    public void Initialize(FlamethrowerAttack attack, int damage)
    {
        flamethrowerAttack = attack;
        damagePerSecond = damage;

        if (flameCollider == null)
            flameCollider = GetComponent<Collider>();

        if (flameCollider != null)
            flameCollider.isTrigger = true;
    }

    public void SetFlameActive(bool active)
    {
        isActive = active;

        if (flameCollider != null)
            flameCollider.enabled = active;

        if (!active)
        {
            // Очищаем список врагов при деактивации
            enemiesInRange.Clear();
        }
    }

    void Update()
    {
        if (!isActive || flamethrowerAttack == null) return;

        // Наносим урон врагам в зоне с определенной частотой
        damageTimer += Time.deltaTime;
        if (damageTimer >= damageTickRate)
        {
            damageTimer = 0f;
            DamageEnemiesInRange();
        }
    }

    private void DamageEnemiesInRange()
    {
        int tickDamage = Mathf.RoundToInt(damagePerSecond * damageTickRate);

        for (int i = enemiesInRange.Count - 1; i >= 0; i--)
        {
            Collider enemy = enemiesInRange[i];
            if (enemy != null)
            {
                flamethrowerAttack.OnEnemyHit(enemy);
            }
            else
            {
                // Удаляем уничтоженных врагов из списка
                enemiesInRange.RemoveAt(i);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;

        // Проверяем что это враг (можно добавить проверку по тегу или слою)
        EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
        if (enemyHealth != null && !enemiesInRange.Contains(other))
        {
            enemiesInRange.Add(other);
            Debug.Log($"Enemy entered flame: {other.name}");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!isActive) return;

        // Удаляем врага из списка при выходе из зоны
        if (enemiesInRange.Contains(other))
        {
            enemiesInRange.Remove(other);
            Debug.Log($"Enemy left flame: {other.name}");
        }
    }

    void OnDestroy()
    {
        enemiesInRange.Clear();
    }
}
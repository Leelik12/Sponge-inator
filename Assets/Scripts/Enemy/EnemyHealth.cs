using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Death Settings")]
    public GameObject deathEffect; // эффект смерти (например, частицы)
    public bool destroyOnDeath = true;
    public float destroyDelay = 1.5f;

    [Header("Debug")]
    public bool showDebugLogs = false;

    public HeroVoice voice;
    void Start()
    {
        voice = FindAnyObjectByType<HeroVoice>();
        currentHealth = maxHealth;
    }

    /// <summary>
    /// Наносит урон врагу.
    /// </summary>
    /// <param name="damage">Количество урона</param>
    public void TakeDamage(int damage)
    {
        if (damage <= 0f) return;

        currentHealth -= damage;
        if (showDebugLogs)
            Debug.Log($"[EnemyHealth] {gameObject.name} получил {damage} урона. Осталось {currentHealth} HP.");

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        voice.SayenemyDied();
        if (showDebugLogs)
            Debug.Log($"[EnemyHealth] {gameObject.name} погиб.");

        // Эффект смерти
        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        // Если есть анимация — можно вызвать её тут (через Animator)
        // Animator anim = GetComponent<Animator>();
        // if (anim) anim.SetTrigger("Die");
        StaticHolder.LastScore++;
        if (destroyOnDeath)
            Destroy(gameObject, destroyDelay);
    }

    /// <summary>
    /// Восстанавливает здоровье (опционально)
    /// </summary>
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    public float GetHealthPercent()
    {
        return currentHealth / maxHealth;
    }
}

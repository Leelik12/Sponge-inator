using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Death Settings")]
    public GameObject deathEffect;
    public bool destroyOnDeath = true;
    public float destroyDelay = 1.5f;

    [Header("Debug")]
    public bool showDebugLogs = false;

    private WaveManager waveManager;
    public HeroVoice voice;

    // ƒќЅј¬№ Ё“” ѕ≈–≈ћ≈ЌЌ”ё
    private bool isDead = false;

    void Start()
    {
        voice = FindAnyObjectByType<HeroVoice>();
        currentHealth = maxHealth;
        waveManager = FindObjectOfType<WaveManager>();
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0f || isDead) return; // ƒќЅј¬№ ѕ–ќ¬≈– ” isDead

        currentHealth -= damage;
        if (showDebugLogs)
            Debug.Log($"[EnemyHealth] {gameObject.name} получил {damage} урона. ќсталось {currentHealth} HP.");
        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null)
        {
            // игрок должен быть известен твоей системе урона Ч например, через Singleton PlayerController
            Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player != null)
                ai.OnDamaged(player);
        }
        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        // ƒќЅј¬№ Ё“” ѕ–ќ¬≈– ”
        if (isDead)
        {
            if (showDebugLogs)
                Debug.Log($"[EnemyHealth] {gameObject.name} уже мертв, повторный вызов Die() игнорирован.");
            return;
        }
        isDead = true;

        voice.SayenemyDied();
        if (showDebugLogs)
            Debug.Log($"[EnemyHealth] {gameObject.name} погиб.");

        if (waveManager != null)
        {
            waveManager.OnEnemyKilled();
        }

        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        StaticHolder.LastScore++;
        DisableColliders();

        if (destroyOnDeath)
            Destroy(gameObject, destroyDelay);
    }

    void DisableColliders()
    {
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }

        Collider[] childColliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider in childColliders)
        {
            collider.enabled = false;
        }

        // ƒќЅј¬№ ќ“ Ћё„≈Ќ»≈ RIGIDBODY
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }
    }

    // ќстальные методы без изменений...
}
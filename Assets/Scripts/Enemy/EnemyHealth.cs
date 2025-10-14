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

    // ÄÎÁÀÂÜ İÒÓ ÏÅĞÅÌÅÍÍÓŞ
    private bool isDead = false;

    void Start()
    {
        voice = FindAnyObjectByType<HeroVoice>();
        currentHealth = maxHealth;
        waveManager = FindObjectOfType<WaveManager>();
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0f || isDead) return; // ÄÎÁÀÂÜ ÏĞÎÂÅĞÊÓ isDead

        currentHealth -= damage;
        if (showDebugLogs)
            Debug.Log($"[EnemyHealth] {gameObject.name} ïîëó÷èë {damage} óğîíà. Îñòàëîñü {currentHealth} HP.");

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        // ÄÎÁÀÂÜ İÒÓ ÏĞÎÂÅĞÊÓ
        if (isDead)
        {
            if (showDebugLogs)
                Debug.Log($"[EnemyHealth] {gameObject.name} óæå ìåğòâ, ïîâòîğíûé âûçîâ Die() èãíîğèğîâàí.");
            return;
        }
        isDead = true;

        voice.SayenemyDied();
        if (showDebugLogs)
            Debug.Log($"[EnemyHealth] {gameObject.name} ïîãèá.");

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

        // ÄÎÁÀÂÜ ÎÒÊËŞ×ÅÍÈÅ RIGIDBODY
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }
    }

    // Îñòàëüíûå ìåòîäû áåç èçìåíåíèé...
}
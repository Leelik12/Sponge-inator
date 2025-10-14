using UnityEngine;

public class BurnEffect : MonoBehaviour
{
    [Header("Burn Effect Settings")]
    [SerializeField] private ParticleSystem burnParticles;
    [SerializeField] private int burnDamagePerSecond = 5;
    [SerializeField] private float burnDuration = 3f;

    private EnemyHealth enemyHealth;
    private bool isBurning = false;
    private float burnTimer = 0f;
    private float damageTickTimer = 0f;

    void Start()
    {
        enemyHealth = GetComponent<EnemyHealth>();
    }

    void Update()
    {
        if (isBurning)
        {
            burnTimer -= Time.deltaTime;
            damageTickTimer += Time.deltaTime;

            // Наносим урон каждую секунду
            if (damageTickTimer >= 1f)
            {
                damageTickTimer = 0f;
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(burnDamagePerSecond);
                }
            }

            // Завершаем эффект горения
            if (burnTimer <= 0f)
            {
                StopBurning();
            }
        }
    }

    public void Ignite(float duration)
    {
        isBurning = true;
        burnTimer = duration;

        if (burnParticles != null)
        {
            burnParticles.Play();
        }
    }

    public void StopBurning()
    {
        isBurning = false;

        if (burnParticles != null)
        {
            burnParticles.Stop();
        }
    }

    public bool IsBurning()
    {
        return isBurning;
    }
}
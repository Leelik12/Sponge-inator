using UnityEngine;

public class FlamethrowerAttack : MonoBehaviour
{
    [Header("Flamethrower Settings")]
    [SerializeField] private Transform flameStartPoint; // Точка старта огнемета
    [SerializeField] private GameObject flameStreamPrefab; // Префаб потока огня
    [SerializeField] private float flameRange = 10f; // Дальность огнемета
    [SerializeField] private float flameWidth = 2f; // Ширина потока огня
    [SerializeField] private float gasCostPerSecond = 2f; // Расход бензина в секунду
    [SerializeField] private int damagePerSecond = 15; // Урон в секунду
    [SerializeField] private LayerMask enemyLayer; // Слой врагов

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem flameParticles; // Частицы огня
    [SerializeField] private Light flameLight; // Свет от огня
    [SerializeField] private AudioSource flameAudio; // Звук огнемета

    [Header("Attack Settings")]
    [SerializeField] private float maxFlameDuration = 5f; // Максимальная продолжительность непрерывной атаки
    [SerializeField] private float overheatCooldown = 3f; // Время перезарядки после перегрева

    private GubkaGas gubkaGas;
    private bool isFlameActive = false;
    private float currentFlameDuration = 0f;
    private bool isOverheated = false;
    private float cooldownTimer = 0f;

    // Для обнаружения врагов в потоке
    private Collider[] enemyColliders = new Collider[10];
    private float damageTickRate = 0.2f; // Как часто наносится урон (раз в 0.2 секунды)
    private float damageTickTimer = 0f;

    public HeroVoice speaker;

    void Start()
    {
        speaker = GetComponent<HeroVoice>();
        gubkaGas = GetComponent<GubkaGas>();

        if (flameStartPoint == null)
        {
            flameStartPoint = transform;
            Debug.LogWarning("FlameStartPoint not assigned! Using player transform.");
        }

        // Настраиваем визуальные эффекты
        if (flameParticles != null)
        {
            var main = flameParticles.main;
            main.startLifetime = flameRange / main.startSpeed.constant;
        }

        SetFlameEffects(false);
    }

    void Update()
    {
        // Обработка перегрева
        if (isOverheated)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                isOverheated = false;
                currentFlameDuration = 0f;
                Debug.Log("Flamethrower cooled down!");
            }
            return;
        }

        // Начало атаки огнеметом
        if (Input.GetMouseButtonDown(1) && !isFlameActive) // Правая кнопка мыши для огнемета
        {
            TryStartFlamethrower();
        }

        // Продолжение атаки
        if (isFlameActive)
        {
            if (Input.GetMouseButton(1))
            {
                UpdateFlamethrower();
            }
            else
            {
                StopFlamethrower();
            }
        }

        // Остановка атаки при отпускании кнопки
        if (Input.GetMouseButtonUp(1) && isFlameActive)
        {
            StopFlamethrower();
        }
    }

    private void TryStartFlamethrower()
    {
        if (isOverheated) return;

        // Проверяем достаточно ли бензина для начала атаки
        if (gubkaGas.currentGas >= gasCostPerSecond * 0.1f) // Минимальное количество для начала
        {
            StartFlamethrower();
        }
        else
        {
            Debug.Log("Not enough gas for flamethrower!");
            // Можно добавить звук или визуальный эффект недостатка бензина
        }
    }

    private void StartFlamethrower()
    {
        isFlameActive = true;
        SetFlameEffects(true);

        speaker.SayAttack(); // Голосовая реплика атаки

        Debug.Log("Flamethrower activated!");
    }

    private void UpdateFlamethrower()
    {
        if (!isFlameActive || isOverheated) return;

        // Проверяем достаточно ли бензина
        float gasCostThisFrame = gasCostPerSecond * Time.deltaTime;
        if (gubkaGas.currentGas >= gasCostThisFrame)
        {
            // Расходуем бензин
            gubkaGas.currentGas -= gasCostThisFrame;
            gubkaGas.UpdateUI();

            // Обновляем длительность атаки
            currentFlameDuration += Time.deltaTime;

            // Наносим урон врагам в зоне действия
            DamageEnemiesInFlame();

            // Проверяем перегрев
            if (currentFlameDuration >= maxFlameDuration)
            {
                OverheatFlamethrower();
                return;
            }

            // Обновляем визуальные эффекты
            UpdateFlameEffects();
        }
        else
        {
            // Недостаточно бензина - останавливаем огнемет
            StopFlamethrower();
            Debug.Log("Out of gas!");
        }
    }

    private void DamageEnemiesInFlame()
    {
        damageTickTimer += Time.deltaTime;

        if (damageTickTimer >= damageTickRate)
        {
            damageTickTimer = 0f;

            // Находим всех врагов в конической области перед игроком
            Vector3 flameDirection = flameStartPoint.forward;
            Vector3 flameStart = flameStartPoint.position;

            int enemyCount = Physics.OverlapSphereNonAlloc(
                flameStart + flameDirection * flameRange * 0.5f,
                flameRange * 0.5f,
                enemyColliders,
                enemyLayer
            );

            for (int i = 0; i < enemyCount; i++)
            {
                Collider enemyCollider = enemyColliders[i];
                if (enemyCollider != null && IsEnemyInFlameCone(enemyCollider.transform.position))
                {
                    ApplyDamageToEnemy(enemyCollider);
                }
            }
        }
    }

    private bool IsEnemyInFlameCone(Vector3 enemyPosition)
    {
        Vector3 flameDirection = flameStartPoint.forward;
        Vector3 flameStart = flameStartPoint.position;
        Vector3 toEnemy = enemyPosition - flameStart;

        // Проверяем расстояние
        if (toEnemy.magnitude > flameRange) return false;

        // Проверяем угол (ширину конуса)
        float angle = Vector3.Angle(flameDirection, toEnemy.normalized);
        float maxAngle = Mathf.Atan2(flameWidth * 0.5f, flameRange) * Mathf.Rad2Deg;

        return angle <= maxAngle;
    }

    private void ApplyDamageToEnemy(Collider enemyCollider)
    {
        EnemyHealth enemyHealth = enemyCollider.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            int tickDamage = Mathf.RoundToInt(damagePerSecond * damageTickRate);
            enemyHealth.TakeDamage(tickDamage);

            // Можно добавить эффект горения на врага
            BurnEffect burnEffect = enemyCollider.GetComponent<BurnEffect>();
            if (burnEffect != null)
            {
                burnEffect.Ignite(2f); // Поджигаем на 2 секунды
            }
        }
    }

    private void UpdateFlameEffects()
    {
        // Обновляем интенсивность эффектов в зависимости от продолжительности атаки
        float intensity = Mathf.Clamp01(currentFlameDuration / maxFlameDuration);

        if (flameLight != null)
        {
            flameLight.intensity = Mathf.Lerp(1f, 2f, intensity);
        }

        if (flameParticles != null)
        {
            var emission = flameParticles.emission;
            emission.rateOverTime = Mathf.Lerp(50f, 100f, intensity);
        }
    }

    private void OverheatFlamethrower()
    {
        isOverheated = true;
        isFlameActive = false;
        cooldownTimer = overheatCooldown;

        SetFlameEffects(false);

        Debug.Log("Flamethrower overheated! Cooling down...");

        // Можно добавить визуальный эффект перегрева
    }

    private void StopFlamethrower()
    {
        if (!isFlameActive) return;

        isFlameActive = false;
        SetFlameEffects(false);

        Debug.Log("Flamethrower deactivated");
    }

    private void SetFlameEffects(bool active)
    {
        // Частицы огня
        if (flameParticles != null)
        {
            if (active) flameParticles.Play();
            else flameParticles.Stop();
        }

        // Свет
        if (flameLight != null)
        {
            flameLight.enabled = active;
        }

        // Звук
        if (flameAudio != null)
        {
            if (active) flameAudio.Play();
            else flameAudio.Stop();
        }
    }

    // Метод для принудительной остановки огнемета (например, при смерти игрока)
    public void ForceStopFlamethrower()
    {
        StopFlamethrower();
        isOverheated = false;
        currentFlameDuration = 0f;
    }

    // Метод для получения информации о состоянии огнемета
    public float GetHeatLevel()
    {
        return currentFlameDuration / maxFlameDuration;
    }

    public bool IsActive()
    {
        return isFlameActive;
    }

    public bool IsOverheated()
    {
        return isOverheated;
    }

    // Визуализация зоны поражения в редакторе
    private void OnDrawGizmosSelected()
    {
        if (flameStartPoint != null)
        {
            Gizmos.color = Color.red;
            Vector3 flameStart = flameStartPoint.position;
            Vector3 flameDirection = flameStartPoint.forward;

            // Рисуем конус огнемета
            Vector3 endPoint = flameStart + flameDirection * flameRange;
            float endRadius = flameWidth * 0.5f;

            Gizmos.DrawLine(flameStart, endPoint);
            Gizmos.DrawWireSphere(endPoint, endRadius);

            // Рисуем боковые границы конуса
            Vector3 right = flameStartPoint.right * endRadius;
            Vector3 up = flameStartPoint.up * endRadius;

            Gizmos.DrawLine(flameStart, endPoint + right);
            Gizmos.DrawLine(flameStart, endPoint - right);
            Gizmos.DrawLine(flameStart, endPoint + up);
            Gizmos.DrawLine(flameStart, endPoint - up);
        }
    }
}
using UnityEngine;

public class FlamethrowerAttack : MonoBehaviour
{
    [Header("Flamethrower Settings")]
    [SerializeField] private Transform flameStartPoint; // Точка старта огнемета
    [SerializeField] private GameObject flameStreamPrefab; // Префаб потока огня (должен иметь триггер коллайдер)
    [SerializeField] private float gasCostPerSecond = 2f; // Расход бензина в секунду
    [SerializeField] private int damagePerSecond = 15; // Урон в секунду

    [Header("Attack Settings")]
    [SerializeField] private float maxFlameDuration = 5f; // Максимальная продолжительность непрерывной атаки
    [SerializeField] private float overheatCooldown = 3f; // Время перезарядки после перегрева

    private GubkaGas gubkaGas;
    private bool isFlameActive = false;
    private float currentFlameDuration = 0f;
    private bool isOverheated = false;
    private float cooldownTimer = 0f;

    // Для визуализации струи
    private GameObject currentFlameStream;
    private FlameStreamController currentFlameController;

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

        // Начало атаки огнеметом - ЛЕВАЯ кнопка мыши
        if (Input.GetMouseButtonDown(0) && !isFlameActive)
        {
            TryStartFlamethrower();
        }

        // Продолжение атаки
        if (isFlameActive)
        {
            if (Input.GetMouseButton(0))
            {
                UpdateFlamethrower();
            }
            else
            {
                StopFlamethrower();
            }
        }

        // Остановка атаки при отпускании кнопки
        if (Input.GetMouseButtonUp(0) && isFlameActive)
        {
            StopFlamethrower();
        }
    }

    private void TryStartFlamethrower()
    {
        if (isOverheated) return;

        // Проверяем достаточно ли бензина для начала атаки
        if (gubkaGas.currentGas >= gasCostPerSecond * 0.1f)
        {
            StartFlamethrower();
        }
        else
        {
            Debug.Log("Not enough gas for flamethrower!");
        }
    }

    private void StartFlamethrower()
    {
        isFlameActive = true;

        // Создаем визуальную струю огня
        CreateFlameStream();

        if (speaker != null)
            speaker.SayAttack();

        Debug.Log("Flamethrower activated!");
    }

    private void CreateFlameStream()
    {
        // Создаем префаб струи если он назначен
        if (flameStreamPrefab != null && currentFlameStream == null)
        {
            currentFlameStream = Instantiate(flameStreamPrefab, flameStartPoint.position, Quaternion.identity);
            currentFlameStream.transform.SetParent(flameStartPoint);

            // Получаем контроллер струи и настраиваем его
            currentFlameController = currentFlameStream.GetComponent<FlameStreamController>();
            if (currentFlameController != null)
            {
                currentFlameController.Initialize(this, damagePerSecond);
                currentFlameController.SetFlameActive(true);
            }
            else
            {
                Debug.LogError("FlameStreamPrefab doesn't have FlameStreamController component!");
            }
        }
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
            if (gubkaGas != null)
                gubkaGas.UpdateUI();

            // Обновляем длительность атаки
            currentFlameDuration += Time.deltaTime;

            // Обновляем визуальную струю
            UpdateFlameStream();

            // Проверяем перегрев
            if (currentFlameDuration >= maxFlameDuration)
            {
                OverheatFlamethrower();
                return;
            }
        }
        else
        {
            // Недостаточно бензина - останавливаем огнемет
            StopFlamethrower();
            Debug.Log("Out of gas!");
        }
    }

    private void UpdateFlameStream()
    {
        // Обновляем позицию и направление префаба струи
        if (currentFlameStream != null)
        {
            currentFlameStream.transform.position = flameStartPoint.position;
            currentFlameStream.transform.rotation = Quaternion.LookRotation(GetFlameDirection());
        }
    }

    private Vector3 GetFlameDirection()
    {
        // Поворачиваем направление на 90 градусов влево (против часовой стрелки)
        return Quaternion.Euler(0f, -90f, 0f) * flameStartPoint.forward;
    }

    // Метод который будет вызываться из FlameStreamController при попадании
    public void OnEnemyHit(Collider enemyCollider)
    {
        ApplyDamageToEnemy(enemyCollider);
    }

    private void ApplyDamageToEnemy(Collider enemyCollider)
    {
        if (enemyCollider == null) return;

        EnemyHealth enemyHealth = enemyCollider.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damagePerSecond);
            Debug.Log($"Applied {damagePerSecond} damage to {enemyCollider.name}");

            // Можно добавить эффект горения на врага
            BurnEffect burnEffect = enemyCollider.GetComponent<BurnEffect>();
            if (burnEffect != null)
            {
                burnEffect.Ignite(2f);
            }
        }
    }

    private void OverheatFlamethrower()
    {
        isOverheated = true;
        isFlameActive = false;
        cooldownTimer = overheatCooldown;

        DestroyFlameStream();

        Debug.Log("Flamethrower overheated! Cooling down...");
    }

    private void StopFlamethrower()
    {
        if (!isFlameActive) return;

        isFlameActive = false;
        DestroyFlameStream();

        Debug.Log("Flamethrower deactivated");
    }

    private void DestroyFlameStream()
    {
        if (currentFlameController != null)
        {
            currentFlameController.SetFlameActive(false);
            currentFlameController = null;
        }

        if (currentFlameStream != null)
        {
            Destroy(currentFlameStream);
            currentFlameStream = null;
        }
    }

    // Метод для принудительной остановки огнемета
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
}
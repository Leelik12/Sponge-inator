using UnityEngine;

public class SredstvoAttack : MonoBehaviour
{
    [Header("Water Jet Settings")]
    [SerializeField] private Transform jetStartPoint; // Точка старта струи
    [SerializeField] private GameObject waterJetPrefab; // Префаб струи воды
    [SerializeField] private float jetSpeed = 15f; // Скорость струи
    [SerializeField] private float arcHeight = 3f; // Высота дуги параболы
    [SerializeField] private float waterCost = 1f; // Количество воды на одну атаку
    [SerializeField] private int damageAmount = 10; // Урон от струи
    [SerializeField] private LayerMask enemyLayer; // Слой врагов

    [Header("Attack Range")]
    [SerializeField] private float attackRange = 20f; // Максимальная дистанция атаки
    [SerializeField] private bool showRangeInEditor = true; // Показать диапазон в редакторе

    [Header("Aiming Settings")]
    [SerializeField] private float wheelSensitivity = 2f; // Чувствительность колесика мыши
    [SerializeField] private float maxAimAngle = 35f; // Максимальный угол отклонения прицела (в градусах)
    [SerializeField] private bool invertWheelAim = false; // Инвертировать колесико мыши

    [Header("Trajectory Settings")]
    [SerializeField] private LineRenderer trajectoryRenderer; // LineRenderer для отрисовки траектории
    [SerializeField] private int trajectoryPoints = 20; // Количество точек траектории
    [SerializeField] private Color validTrajectoryColor = Color.cyan; // Цвет когда достаточно воды
    [SerializeField] private Color invalidTrajectoryColor = Color.red; // Цвет когда недостаточно воды

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem waterSplashEffect; // Эффект брызг

    private GubkaClear gubkaClear;
    private Camera playerCamera;
    private bool isAttacking = false;
    private bool isAiming = false; // Флаг прицеливания

    private Vector3 currentAimDirection; // Текущее направление прицела
    private Vector3 baseAimDirection; // Базовое направление (вперёд от игрока)
    private Vector3 savedAimDirection; // Сохраненное направление прицела
    private bool hasSavedAim = false; // Флаг сохранения прицела

    // Только вертикальное прицеливание через колесико мыши
    private float currentVerticalAngle = 0f;
    private float savedVerticalAngle = 0f;
    private float lastMouseWheelValue = 0f; // Для отслеживания изменений колесика

    // Свойство для доступа к дистанции атаки из других скриптов
    public float AttackRange => attackRange;

    public HeroVoice speaker;

    void Start()
    {
        speaker = GetComponent<HeroVoice>();
        gubkaClear = GetComponent<GubkaClear>();
        playerCamera = Camera.main;

        if (jetStartPoint == null)
        {
            jetStartPoint = transform;
            Debug.LogWarning("JetStartPoint not assigned! Using player transform.");
        }

        // Настраиваем LineRenderer если он есть
        if (trajectoryRenderer != null)
        {
            trajectoryRenderer.positionCount = trajectoryPoints;
            trajectoryRenderer.enabled = false;
        }

        // Инициализируем направления прицеливания с поворотом на -90 градусов
        baseAimDirection = Quaternion.Euler(0, -90, 0) * transform.forward; // Поворачиваем на -90 градусов
        currentAimDirection = baseAimDirection;
        savedAimDirection = baseAimDirection;

        // Инициализируем последнее значение колесика
        lastMouseWheelValue = Input.mouseScrollDelta.y;
    }

    private void OnEnable()
    {
        // Настраиваем LineRenderer если он есть
        if (trajectoryRenderer != null)
        {
            trajectoryRenderer.positionCount = trajectoryPoints;
            trajectoryRenderer.enabled = false;
        }
    }

    void Update()
    {
        // Обработка прицеливания (правая кнопка мыши)
        HandleAiming();

        // Проверяем ввод для атаки (левая кнопка мыши) - можно атаковать даже во время прицеливания
        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            TryShootWaterJet();
        }
    }

    private void HandleAiming()
    {
        // Начало прицеливания
        if (Input.GetMouseButtonDown(1))
        {
            isAiming = true;
            // Обновляем базовое направление при каждом начале прицеливания
            UpdateBaseAimDirection();

            // Восстанавливаем сохраненное направление прицела, если оно есть
            if (hasSavedAim)
            {
                currentVerticalAngle = savedVerticalAngle;
                UpdateAimDirectionFromAngle();
            }
            else
            {
                currentVerticalAngle = 0f;
                currentAimDirection = baseAimDirection;
            }

            if (trajectoryRenderer != null)
            {
                trajectoryRenderer.enabled = true;
            }

            // Обновляем траекторию сразу при начале прицеливания
            UpdateTrajectory();
        }

        // Обновление прицеливания во время зажатой правой кнопки
        if (isAiming)
        {
            UpdateAimDirection();
            UpdateTrajectory();

            // Конец прицеливания при отпускании правой кнопки
            if (Input.GetMouseButtonUp(1))
            {
                // Сохраняем текущее направление прицела
                savedVerticalAngle = currentVerticalAngle;
                savedAimDirection = currentAimDirection;
                hasSavedAim = true;

                isAiming = false;
                if (trajectoryRenderer != null)
                {
                    trajectoryRenderer.enabled = false;
                }
            }
        }
    }

    private void UpdateBaseAimDirection()
    {
        // Всегда обновляем базовое направление относительно текущего поворота игрока
        baseAimDirection = Quaternion.Euler(0, -90, 0) * transform.forward;
        baseAimDirection.Normalize();
    }

    private void UpdateAimDirection()
    {
        // Получаем ввод от колесика мыши с проверкой на реальное изменение
        float currentMouseWheel = Input.mouseScrollDelta.y;

        // Фильтруем случайные колебания и ложные срабатывания
        if (Mathf.Abs(currentMouseWheel) > 0.01f && currentMouseWheel != lastMouseWheelValue)
        {
            float mouseWheel = currentMouseWheel * wheelSensitivity;

            // Инвертируем управление, если нужно
            if (invertWheelAim)
            {
                mouseWheel = -mouseWheel;
            }

            // Обновляем вертикальный угол только при реальном изменении колесика
            currentVerticalAngle += mouseWheel;

            // Ограничиваем угол для предотвращения тряски
            currentVerticalAngle = Mathf.Clamp(currentVerticalAngle, -maxAimAngle, maxAimAngle);

            // Обновляем направление прицела на основе угла
            UpdateAimDirectionFromAngle();
        }

        // Сохраняем текущее значение колесика для следующего кадра
        lastMouseWheelValue = currentMouseWheel;
    }

    private void UpdateAimDirectionFromAngle()
    {
        // Вычисляем новое направление только по вертикали
        // Используем локальную правую ось игрока для вертикального вращения
        Vector3 right = transform.right; // Локальная правая ось игрока

        // Начинаем с актуального базового направления
        currentAimDirection = baseAimDirection;

        // Только вертикальное вращение - вокруг локальной правой оси игрока
        currentAimDirection = Quaternion.AngleAxis(currentVerticalAngle, right) * currentAimDirection;
        currentAimDirection.Normalize();
    }

    private void UpdateTrajectory()
    {
        if (trajectoryRenderer == null) return;

        Vector3 targetPoint = GetTargetPoint();
        Vector3 startPos = jetStartPoint.position;

        // Рассчитываем точки траектории
        for (int i = 0; i < trajectoryPoints; i++)
        {
            float progress = (float)i / (trajectoryPoints - 1);

            // Параболическая формула (такая же как в WaterJetProjectile)
            float parabola = 1.0f - 4.0f * (progress - 0.5f) * (progress - 0.5f);
            Vector3 currentPos = Vector3.Lerp(startPos, targetPoint, progress);
            currentPos.y += parabola * arcHeight;

            trajectoryRenderer.SetPosition(i, currentPos);
        }

        // Меняем цвет в зависимости от наличия моющего средства
        bool hasEnoughClear = gubkaClear.currentClear >= waterCost;
        trajectoryRenderer.startColor = hasEnoughClear ? validTrajectoryColor : invalidTrajectoryColor;
        trajectoryRenderer.endColor = hasEnoughClear ? validTrajectoryColor : invalidTrajectoryColor;
    }

    private void TryShootWaterJet()
    {
        // Проверяем достаточно ли моющего средства для атаки
        if (gubkaClear.currentClear >= waterCost)
        {
            ShootWaterJet();
            gubkaClear.currentClear -= waterCost;
            gubkaClear.UpdateUI(); // Обновляем UI моющего средства

            // Сохраняем направление прицела после выстрела
            savedVerticalAngle = currentVerticalAngle;
            savedAimDirection = currentAimDirection;
            hasSavedAim = true;
        }
        else
        {
            Debug.Log("Not enough clear!");
            // Можно добавить звук или визуальный эффект недостатка моющего средства
        }
    }

    private void ShootWaterJet()
    {
        if (speaker != null)
            speaker.SayAttack();

        isAttacking = true;

        // Создаем струю моющего средства
        GameObject SredstvoJet = Instantiate(waterJetPrefab, jetStartPoint.position, Quaternion.identity);
        SredstvoJetProjectile projectile = SredstvoJet.GetComponent<SredstvoJetProjectile>();

        if (projectile != null)
        {
            // Получаем точку прицеливания
            Vector3 targetPoint = GetTargetPoint();

            // Настраиваем параметры струи
            projectile.Initialize(jetStartPoint.position, targetPoint, arcHeight, jetSpeed, damageAmount, enemyLayer, this, attackRange);

            // Запускаем струю
            projectile.Launch();
        }

        // Воспроизводим эффект брызг
        if (waterSplashEffect != null)
        {
            waterSplashEffect.Play();
        }

        isAttacking = false;
    }

    private Vector3 GetTargetPoint()
    {
        Vector3 startPoint = jetStartPoint.position;

        // Если прицеливаемся, используем текущее направление прицела
        if (isAiming || hasSavedAim)
        {
            Vector3 directionToUse = isAiming ? currentAimDirection : savedAimDirection;
            Ray ray = new Ray(startPoint, directionToUse);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, attackRange, ~0, QueryTriggerInteraction.Ignore))
            {
                return hit.point;
            }

            return startPoint + directionToUse * attackRange;
        }
        else
        {
            // Стандартное прицеливание - вправо от игрока (повернуто на -90 градусов)
            Vector3 defaultDirection = Quaternion.Euler(0, -90, 0) * transform.forward;
            Ray ray = new Ray(startPoint, defaultDirection);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, attackRange, ~0, QueryTriggerInteraction.Ignore))
            {
                return hit.point;
            }

            return startPoint + defaultDirection * attackRange;
        }
    }

    // Метод для вызова при попадании струи (будет вызываться из SredstvoJetProjectile)
    public void OnWaterJetHit(Vector3 hitPosition, Collider hitEnemy)
    {
        // Наносим урон врагу
        if (hitEnemy != null)
        {
            EnemyHealth enemyHealth = hitEnemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damageAmount);
            }
        }

        // Дополнительные эффекты при попадании
        Debug.Log("Sredstvo jet hit at: " + hitPosition);

        // Можно добавить дополнительные эффекты: звук, частицы и т.д.
    }

    // Метод для получения текущего количества моющего средства (для визуальных эффектов)
    public float GetCurrentClearNormalized()
    {
        return gubkaClear.currentClear / gubkaClear.maxClearCapacity;
    }

    // Метод для сброса сохраненного прицела (например, при смене цели)
    public void ResetSavedAim()
    {
        hasSavedAim = false;
        savedAimDirection = Quaternion.Euler(0, -90, 0) * transform.forward;
        savedVerticalAngle = 0f;
    }

    // Метод для принудительной установки вертикального угла
    public void SetVerticalAngle(float angle)
    {
        currentVerticalAngle = Mathf.Clamp(angle, -maxAimAngle, maxAimAngle);
        UpdateAimDirectionFromAngle();
    }

    // Метод для получения текущего вертикального угла
    public float GetCurrentVerticalAngle()
    {
        return currentVerticalAngle;
    }

    // Визуализация диапазона атаки и сектора прицеливания в редакторе
    private void OnDrawGizmosSelected()
    {
        if (showRangeInEditor && jetStartPoint != null)
        {
            Vector3 startPoint = jetStartPoint.position;

            // Базовое направление (повернуто на -90 градусов вправо)
            Vector3 baseDirection = Quaternion.Euler(0, -90, 0) * transform.forward;

            // Диапазон атаки - сфера
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(startPoint, attackRange);

            // Основной луч (белый)
            Gizmos.color = Color.white;
            Gizmos.DrawRay(startPoint, baseDirection * attackRange);

            // Сектор прицеливания (только вертикальный) - синий
            Gizmos.color = Color.blue;
            Vector3 right = transform.right; // Локальная правая ось для вертикального вращения

            // Верхняя граница сектора
            Vector3 upBound = Quaternion.AngleAxis(maxAimAngle, right) * baseDirection;
            Gizmos.DrawRay(startPoint, upBound * attackRange);

            // Нижняя граница сектора
            Vector3 downBound = Quaternion.AngleAxis(-maxAimAngle, right) * baseDirection;
            Gizmos.DrawRay(startPoint, downBound * attackRange);

            // Дуга между границами сектора
            DrawVerticalArcGizmo(startPoint, baseDirection, right, maxAimAngle, attackRange);

            // Отображаем сохраненное направление прицела (если есть) - зеленый
            if (hasSavedAim)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(startPoint, savedAimDirection * attackRange * 0.7f);
            }
        }
    }

    // Вспомогательный метод для рисования вертикальной дуги сектора прицеливания
    private void DrawVerticalArcGizmo(Vector3 center, Vector3 forward, Vector3 axis, float angle, float radius)
    {
        int segments = 12;
        float angleStep = (angle * 2) / segments;

        Vector3 previousPoint = center + Quaternion.AngleAxis(-angle, axis) * forward * radius;

        for (int i = 1; i <= segments; i++)
        {
            float currentAngle = -angle + angleStep * i;
            Vector3 nextPoint = center + Quaternion.AngleAxis(currentAngle, axis) * forward * radius;
            Gizmos.DrawLine(previousPoint, nextPoint);
            previousPoint = nextPoint;
        }
    }
}
using UnityEngine;

public class WaterJetAttack : MonoBehaviour
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
    [SerializeField] private float aimSensitivity = 0.1f; // Уменьшенная чувствительность прицеливания
    [SerializeField] private float maxAimAngle = 35f; // Максимальный угол отклонения прицела (в градусах)
    [SerializeField] private bool invertVerticalAim = false; // Опция для инвертирования вертикального прицеливания

    [Header("Trajectory Settings")]
    [SerializeField] private LineRenderer trajectoryRenderer; // LineRenderer для отрисовки траектории
    [SerializeField] private int trajectoryPoints = 20; // Количество точек траектории
    [SerializeField] private Color validTrajectoryColor = Color.cyan; // Цвет когда достаточно воды
    [SerializeField] private Color invalidTrajectoryColor = Color.red; // Цвет когда недостаточно воды

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem waterSplashEffect; // Эффект брызг

    private GubkaWater gubkaWater;
    private Camera playerCamera;
    private bool isAttacking = false;
    private bool isAiming = false; // Флаг прицеливания

    private Vector3 currentAimDirection; // Текущее направление прицела
    private Vector3 baseAimDirection; // Базовое направление (вперёд от игрока)
    private Vector3 savedAimDirection; // Сохраненное направление прицела
    private bool hasSavedAim = false; // Флаг сохранения прицела

    // Для плавного прицеливания без тряски
    private float currentHorizontalAngle = 0f;
    private float currentVerticalAngle = 0f;
    private float savedHorizontalAngle = 0f;
    private float savedVerticalAngle = 0f;

    // Свойство для доступа к дистанции атаки из других скриптов
    public float AttackRange => attackRange;

    void Start()
    {
        gubkaWater = GetComponent<GubkaWater>();
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

        // Инициализируем направления прицеливания
        baseAimDirection = transform.forward;
        currentAimDirection = baseAimDirection;
        savedAimDirection = baseAimDirection;
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
            baseAimDirection = transform.forward; // Запоминаем текущее направление игрока

            // Восстанавливаем сохраненное направление прицела, если оно есть
            if (hasSavedAim)
            {
                currentHorizontalAngle = savedHorizontalAngle;
                currentVerticalAngle = savedVerticalAngle;
                UpdateAimDirectionFromAngles();
            }
            else
            {
                currentHorizontalAngle = 0f;
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
                savedHorizontalAngle = currentHorizontalAngle;
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

    private void UpdateAimDirection()
    {
        // Получаем движение мыши с уменьшенной чувствительностью
        float mouseX = Input.GetAxis("Mouse X") * aimSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * aimSensitivity;

        // Инвертируем вертикальное управление, если нужно
        if (invertVerticalAim)
        {
            mouseY = -mouseY;
        }

        // Обновляем углы
        currentHorizontalAngle += mouseX;
        currentVerticalAngle += mouseY;

        // Ограничиваем углы для предотвращения тряски
        currentHorizontalAngle = Mathf.Clamp(currentHorizontalAngle, -maxAimAngle, maxAimAngle);
        currentVerticalAngle = Mathf.Clamp(currentVerticalAngle, -maxAimAngle, maxAimAngle);

        // Обновляем направление прицела на основе углов
        UpdateAimDirectionFromAngles();
    }

    private void UpdateAimDirectionFromAngles()
    {
        // Вычисляем новое направление с использованием сферических координат
        // Это предотвращает тряску при достижении границ
        Vector3 right = Vector3.Cross(Vector3.up, baseAimDirection).normalized;
        Vector3 up = Vector3.up;

        currentAimDirection = baseAimDirection;

        // Горизонтальное вращение - вокруг оси Y (вверх)
        currentAimDirection = Quaternion.AngleAxis(currentHorizontalAngle, up) * currentAimDirection;

        // Вертикальное вращение - вокруг локальной правой оси
        // Обратите внимание: положительный угол поднимает прицел вверх
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

        // Меняем цвет в зависимости от наличия воды
        bool hasEnoughWater = gubkaWater.currentWater >= waterCost;
        trajectoryRenderer.startColor = hasEnoughWater ? validTrajectoryColor : invalidTrajectoryColor;
        trajectoryRenderer.endColor = hasEnoughWater ? validTrajectoryColor : invalidTrajectoryColor;
    }

    private void TryShootWaterJet()
    {
        // Проверяем достаточно ли воды для атаки
        if (gubkaWater.currentWater >= waterCost)
        {
            ShootWaterJet();
            gubkaWater.currentWater -= waterCost;
            gubkaWater.UpdateUI(); // Обновляем UI воды

            // Сохраняем направление прицела после выстрела
            savedHorizontalAngle = currentHorizontalAngle;
            savedVerticalAngle = currentVerticalAngle;
            savedAimDirection = currentAimDirection;
            hasSavedAim = true;
        }
        else
        {
            Debug.Log("Not enough water!");
            // Можно добавить звук или визуальный эффект недостатка воды
        }
    }

    private void ShootWaterJet()
    {
        isAttacking = true;

        // Создаем струю воды
        GameObject waterJet = Instantiate(waterJetPrefab, jetStartPoint.position, Quaternion.identity);
        WaterJetProjectile projectile = waterJet.GetComponent<WaterJetProjectile>();

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
            // Стандартное прицеливание - прямо перед игроком
            Ray ray = new Ray(startPoint, transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, attackRange, ~0, QueryTriggerInteraction.Ignore))
            {
                return hit.point;
            }

            return startPoint + transform.forward * attackRange;
        }
    }

    // Метод для вызова при попадании струи (будет вызываться из WaterJetProjectile)
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
        Debug.Log("Water jet hit at: " + hitPosition);

        // Можно добавить дополнительные эффекты: звук, частицы и т.д.
    }

    // Метод для получения текущего количества воды (для визуальных эффектов)
    public float GetCurrentWaterNormalized()
    {
        return gubkaWater.currentWater / gubkaWater.maxWaterCapacity;
    }

    // Метод для сброса сохраненного прицела (например, при смене цели)
    public void ResetSavedAim()
    {
        hasSavedAim = false;
        savedAimDirection = transform.forward;
        savedHorizontalAngle = 0f;
        savedVerticalAngle = 0f;
    }

    // Визуализация диапазона атаки и сектора прицеливания в редакторе
    private void OnDrawGizmosSelected()
    {
        if (showRangeInEditor)
        {
            // Диапазон атаки
            Gizmos.color = Color.yellow;
            if (jetStartPoint != null)
            {
                Gizmos.DrawWireSphere(jetStartPoint.position, attackRange);
            }

            // Сектор прицеливания
            Gizmos.color = Color.blue;
            Vector3 startPoint = jetStartPoint != null ? jetStartPoint.position : transform.position;
            Vector3 baseDirection = transform.forward;

            // Рисуем линии, ограничивающие сектор прицеливания
            float angleInRadians = maxAimAngle * Mathf.Deg2Rad;
            Vector3 leftBound = Quaternion.AngleAxis(-maxAimAngle, Vector3.up) * baseDirection;
            Vector3 rightBound = Quaternion.AngleAxis(maxAimAngle, Vector3.up) * baseDirection;

            Gizmos.DrawRay(startPoint, leftBound * attackRange);
            Gizmos.DrawRay(startPoint, rightBound * attackRange);

            // Дуга сектора
            DrawArcGizmo(startPoint, baseDirection, maxAimAngle, attackRange);

            // Отображаем сохраненное направление прицела (если есть)
            if (hasSavedAim)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(startPoint, savedAimDirection * attackRange * 0.7f);
            }
        }
    }

    // Вспомогательный метод для рисования дуги сектора прицеливания
    private void DrawArcGizmo(Vector3 center, Vector3 forward, float angle, float radius)
    {
        int segments = 20;
        float angleStep = (angle * 2) / segments;

        Vector3 previousPoint = center + Quaternion.AngleAxis(-angle, Vector3.up) * forward * radius;

        for (int i = 1; i <= segments; i++)
        {
            float currentAngle = -angle + angleStep * i;
            Vector3 nextPoint = center + Quaternion.AngleAxis(currentAngle, Vector3.up) * forward * radius;
            Gizmos.DrawLine(previousPoint, nextPoint);
            previousPoint = nextPoint;
        }
    }
}
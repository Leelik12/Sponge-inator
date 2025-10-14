using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FirstPersonLook : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform character;
    [SerializeField] private GubkaMovement playerMovement; // Ссылка на скрипт движения

    [Header("Mouse Settings")]
    public float sensitivity = 2;
    public float smoothing = 1.5f;

    [Header("UI References")]
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TextMeshProUGUI sensitivityText;
    [SerializeField] private string sensitivityFormat = "Чувствительность: {0:F1}";

    private Vector2 velocity;
    private Vector2 frameVelocity;
    private Vector2 initialRotation; // Сохраняем начальное вращение

    // Локальные переменные для камеры
    private Vector3 initialLocalCameraPosition;
    private Quaternion initialLocalCameraRotation;
    private Vector3 initialLocalCameraEuler;

    // Коэффициент коррекции для WebGL
    private float webGLCorrection = 1f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        // Получаем ссылку на скрипт движения если не назначен
        if (playerMovement == null)
        {
            playerMovement = character.GetComponent<GubkaMovement>();
            if (playerMovement == null)
            {
                playerMovement = FindAnyObjectByType<GubkaMovement>();
                Debug.LogWarning("GubkaMovement not assigned in inspector, searching in scene...");
            }
        }

        // Сохраняем начальные локальные параметры камеры
        SaveInitialCameraLocalTransforms();

        // Сохраняем начальное вращение камеры и персонажа
        initialRotation.x = character.localEulerAngles.y;
        initialRotation.y = -transform.localEulerAngles.x; // Инвертируем для согласованности

        // Инициализируем velocity начальным вращением
        velocity = initialRotation;

        // Автоопределение платформы
#if UNITY_WEBGL && !UNITY_EDITOR
            webGLCorrection = 0.5f;
#else
        webGLCorrection = 1f;
#endif

        // Загружаем сохраненную чувствительность
        LoadSensitivity();

        // Настраиваем UI
        SetupUI();

        // Применяем начальное вращение
        ApplyRotation();

        // Убеждаемся, что скрипт движения использует правильную камеру
        if (playerMovement != null)
        {
            Camera mainCamera = GetComponent<Camera>();
            if (mainCamera == null)
                mainCamera = Camera.main;

            playerMovement.SetPlayerCamera(mainCamera);
            Debug.Log("Camera reference set in player movement");
        }
        else
        {
            Debug.LogError("Player movement reference not found!");
        }
    }

    void Update()
    {
        if (!enabled) return;

        // Применяем коррекцию для WebGL
        float adjustedSensitivity = sensitivity * webGLCorrection;

        Vector2 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        Vector2 rawFrameVelocity = Vector2.Scale(mouseDelta, Vector2.one * adjustedSensitivity);
        frameVelocity = Vector2.Lerp(frameVelocity, rawFrameVelocity, 1 / smoothing);

        velocity += frameVelocity;
        velocity.y = Mathf.Clamp(velocity.y, -80, 70);

        ApplyRotation();
    }

    // Сохраняем начальные локальные трансформы камеры
    private void SaveInitialCameraLocalTransforms()
    {
        initialLocalCameraPosition = transform.localPosition;
        initialLocalCameraRotation = transform.localRotation;
        initialLocalCameraEuler = transform.localEulerAngles;

        Debug.Log($"Сохранены локальные параметры камеры: " +
                 $"Position: {initialLocalCameraPosition}, " +
                 $"Euler: {initialLocalCameraEuler}");
    }

    // Восстанавливаем локальные трансформы камеры
    public void ResetCameraToInitialLocal()
    {
        transform.localPosition = initialLocalCameraPosition;
        transform.localRotation = initialLocalCameraRotation;

        Debug.Log("Локальные трансформы камеры восстановлены");
    }

    // Получаем текущие локальные параметры камеры
    public Vector3 GetCurrentLocalPosition()
    {
        return transform.localPosition;
    }

    public Quaternion GetCurrentLocalRotation()
    {
        return transform.localRotation;
    }

    public Vector3 GetCurrentLocalEuler()
    {
        return transform.localEulerAngles;
    }

    // Устанавливаем локальную позицию камеры
    public void SetCameraLocalPosition(Vector3 localPosition)
    {
        transform.localPosition = localPosition;
    }

    // Устанавливаем локальное вращение камеры
    public void SetCameraLocalRotation(Quaternion localRotation)
    {
        transform.localRotation = localRotation;
    }

    // Устанавливаем локальное вращение камеры через углы Эйлера
    public void SetCameraLocalEuler(Vector3 localEuler)
    {
        transform.localEulerAngles = localEuler;
    }

    // Плавное изменение локальной позиции камеры
    public void LerpCameraLocalPosition(Vector3 targetLocalPosition, float speed = 5f)
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetLocalPosition, Time.deltaTime * speed);
    }

    // Плавное изменение локального вращения камеры
    public void LerpCameraLocalRotation(Quaternion targetLocalRotation, float speed = 5f)
    {
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetLocalRotation, Time.deltaTime * speed);
    }

    // Вынесено в отдельный метод для переиспользования
    private void ApplyRotation()
    {
        // Вращаем только камеру по вертикали
        transform.localRotation = Quaternion.AngleAxis(-velocity.y, Vector3.right);

        // Вращаем персонажа по горизонтали
        character.localRotation = Quaternion.AngleAxis(velocity.x, Vector3.up);

        // Синхронизируем направление движения с направлением камеры
        SyncMovementWithCamera();
    }

    private void SyncMovementWithCamera()
    {
        // Этот метод гарантирует, что направление движения синхронизировано с камерой
        // Дополнительная логика синхронизации может быть добавлена здесь при необходимости
        if (playerMovement != null)
        {
            // Принудительно обновляем ссылку на камеру если она была утеряна
            Camera currentCamera = GetComponent<Camera>();
            if (currentCamera == null)
                currentCamera = Camera.main;

            playerMovement.SetPlayerCamera(currentCamera);
        }
    }

    // Настройка UI элементов
    private void SetupUI()
    {
        if (sensitivitySlider != null)
        {
            sensitivitySlider.minValue = 0.1f;
            sensitivitySlider.maxValue = 5f;
            sensitivitySlider.value = sensitivity;
            sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        }

        UpdateSensitivityText();
    }

    // Обработчик изменения слайдера
    private void OnSensitivityChanged(float newValue)
    {
        sensitivity = newValue;
        SaveSensitivity();
        UpdateSensitivityText();
    }

    // Обновление текста чувствительности
    private void UpdateSensitivityText()
    {
        if (sensitivityText != null)
        {
            sensitivityText.text = string.Format(sensitivityFormat, sensitivity);
        }
    }

    // Сохранение настроек
    private void SaveSensitivity()
    {
        PlayerPrefs.SetFloat("MouseSensitivity", sensitivity);
        PlayerPrefs.Save();
    }

    // Загрузка настроек
    private void LoadSensitivity()
    {
        if (PlayerPrefs.HasKey("MouseSensitivity"))
        {
            sensitivity = PlayerPrefs.GetFloat("MouseSensitivity");
        }
        else
        {
            sensitivity = 2f;
        }
    }

    // Публичные методы для управления чувствительностью
    public void SetSensitivity(float newSensitivity)
    {
        sensitivity = Mathf.Clamp(newSensitivity, 0.1f, 5f);

        if (sensitivitySlider != null)
        {
            sensitivitySlider.SetValueWithoutNotify(sensitivity);
        }

        SaveSensitivity();
        UpdateSensitivityText();
    }

    public float GetSensitivity()
    {
        return sensitivity;
    }

    // Методы управления вращением
    public void ResetRotation(Vector2 newRotation)
    {
        velocity = newRotation;
        frameVelocity = Vector2.zero;
        ApplyRotation();
    }

    public void ResetToDefault()
    {
        ResetRotation(initialRotation); // Возвращаем начальное вращение
    }

    public void ResetCameraCompletely()
    {
        // Полный сброс камеры: вращение + локальные трансформы
        ResetRotation(initialRotation);
        ResetCameraToInitialLocal();
    }

    public void ResetToZero()
    {
        ResetRotation(Vector2.zero); // Альтернативный метод для сброса в ноль
    }

    public void SetRotation(float xRotation, float yRotation)
    {
        ResetRotation(new Vector2(xRotation, yRotation));
    }

    // Метод для принудительной установки коррекции WebGL
    public void SetWebGLCorrection(float correction)
    {
        webGLCorrection = correction;
    }

    // Новый метод для получения текущей скорректированной чувствительности
    public float GetCurrentAdjustedSensitivity()
    {
        return sensitivity * webGLCorrection;
    }

    // Метод для получения текущего вращения
    public Vector2 GetCurrentRotation()
    {
        return velocity;
    }

    // Метод для получения начальных локальных параметров
    public Vector3 GetInitialLocalPosition()
    {
        return initialLocalCameraPosition;
    }

    public Quaternion GetInitialLocalRotation()
    {
        return initialLocalCameraRotation;
    }

    public Vector3 GetInitialLocalEuler()
    {
        return initialLocalCameraEuler;
    }

    // Метод для принудительной установки ссылки на движение
    public void SetPlayerMovement(GubkaMovement movement)
    {
        playerMovement = movement;
        if (playerMovement != null)
        {
            Camera currentCamera = GetComponent<Camera>();
            if (currentCamera == null)
                currentCamera = Camera.main;
            playerMovement.SetPlayerCamera(currentCamera);
        }
    }

    private void OnDestroy()
    {
        if (sensitivitySlider != null)
        {
            sensitivitySlider.onValueChanged.RemoveListener(OnSensitivityChanged);
        }
    }
}
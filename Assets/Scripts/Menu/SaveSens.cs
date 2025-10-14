using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SensitivitySettings : MonoBehaviour
{
    [Header("Mouse Settings")]
    public float sensitivity = 2;

    [Header("UI References")]
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TextMeshProUGUI sensitivityText;
    [SerializeField] private string sensitivityFormat = "Чувствительность: {0:F1}";

    void Start()
    {
        // Загружаем сохраненную чувствительность
        LoadSensitivity();

        // Настраиваем UI
        SetupUI();
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

    // Метод для сброса к значениям по умолчанию
    public void ResetToDefaultSensitivity()
    {
        SetSensitivity(2f);
    }

    // Метод для очень низкой чувствительности
    public void SetPreciseAimSensitivity()
    {
        SetSensitivity(0.3f);
    }

    // Метод для обычной чувствительности
    public void SetNormalSensitivity()
    {
        SetSensitivity(1.5f);
    }

    // Метод для высокой чувствительности
    public void SetHighSensitivity()
    {
        SetSensitivity(3f);
    }

    private void OnDestroy()
    {
        if (sensitivitySlider != null)
        {
            sensitivitySlider.onValueChanged.RemoveListener(OnSensitivityChanged);
        }
    }
}
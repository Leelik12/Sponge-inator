using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LiquidSelector : MonoBehaviour
{
    [Header("UI Images")]
    [SerializeField] private Image waterImage;
    [SerializeField] private Image detergentImage;
    [SerializeField] private Image gasolineImage;

    [Header("Управление жидкостями")]
    [SerializeField] private WaterJetAttack waterController;
    [SerializeField] private SredstvoAttack detergentController;
    [SerializeField] private FlamethrowerAttack gasolineController;

    [Header("Настройки")]
    [SerializeField] private bool enableImages = true; // Включить/выключить отображение Image

    private LiquidType currentLiquid = LiquidType.Water;

    public enum LiquidType
    {
        Water,
        Detergent,
        Gasoline
    }

    void Start()
    {
        // Инициализация - активируем воду по умолчанию
        SetLiquid(LiquidType.Water);
    }

    void Update()
    {
        // Обработка ввода для переключения жидкостей
        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            SetLiquid(LiquidType.Water);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            SetLiquid(LiquidType.Detergent);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            SetLiquid(LiquidType.Gasoline);
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            // Циклическое переключение
            CycleLiquid();
        }
    }

    public void SetLiquid(LiquidType liquidType)
    {
        // Отключаем все жидкости
        DeactivateAllLiquids();

        // Включаем выбранную жидкость
        switch (liquidType)
        {
            case LiquidType.Water:
                ActivateWater();
                break;
            case LiquidType.Detergent:
                ActivateDetergent();
                break;
            case LiquidType.Gasoline:
                ActivateGasoline();
                break;
        }

        currentLiquid = liquidType;
        Debug.Log($"Активирована жидкость: {liquidType}");
    }

    private void DeactivateAllLiquids()
    {
        // Отключаем UI Images
        if (enableImages)
        {
            if (waterImage != null) waterImage.enabled = false;
            if (detergentImage != null) detergentImage.enabled = false;
            if (gasolineImage != null) gasolineImage.enabled = false;
        }

        // Отключаем логику жидкостей
        if (waterController != null) waterController.enabled = false;
        if (detergentController != null) detergentController.enabled = false;
        if (gasolineController != null) gasolineController.enabled = false;
    }

    private void ActivateWater()
    {
        if (enableImages && waterImage != null) waterImage.enabled = true;
        if (waterController != null) waterController.enabled = true;
    }

    private void ActivateDetergent()
    {
        if (enableImages && detergentImage != null) detergentImage.enabled = true;
        if (detergentController != null) detergentController.enabled = true;
    }

    private void ActivateGasoline()
    {
        if (enableImages && gasolineImage != null) gasolineImage.enabled = true;
        if (gasolineController != null) gasolineController.enabled = true;
    }

    private void CycleLiquid()
    {
        LiquidType nextLiquid = currentLiquid switch
        {
            LiquidType.Water => LiquidType.Detergent,
            LiquidType.Detergent => LiquidType.Gasoline,
            LiquidType.Gasoline => LiquidType.Water,
            _ => LiquidType.Water
        };

        SetLiquid(nextLiquid);
    }

    // Публичные методы для кнопок UI
    public void SelectWater() => SetLiquid(LiquidType.Water);
    public void SelectDetergent() => SetLiquid(LiquidType.Detergent);
    public void SelectGasoline() => SetLiquid(LiquidType.Gasoline);

    // Получение текущей активной жидкости
    public LiquidType GetCurrentLiquid() => currentLiquid;

    // Получение текущего контроллера
    public MonoBehaviour GetCurrentController()
    {
        return currentLiquid switch
        {
            LiquidType.Water => waterController,
            LiquidType.Detergent => detergentController,
            LiquidType.Gasoline => gasolineController,
            _ => null
        };
    }

    // Методы для управления отображением Images
    public void SetImagesEnabled(bool enabled)
    {
        enableImages = enabled;
        UpdateImagesVisibility();
    }

    public bool GetImagesEnabled()
    {
        return enableImages;
    }

    private void UpdateImagesVisibility()
    {
        if (!enableImages)
        {
            // Выключаем все images
            if (waterImage != null) waterImage.enabled = false;
            if (detergentImage != null) detergentImage.enabled = false;
            if (gasolineImage != null) gasolineImage.enabled = false;
        }
        else
        {
            // Включаем только активную жидкость
            DeactivateAllLiquids();
            switch (currentLiquid)
            {
                case LiquidType.Water:
                    if (waterImage != null) waterImage.enabled = true;
                    break;
                case LiquidType.Detergent:
                    if (detergentImage != null) detergentImage.enabled = true;
                    break;
                case LiquidType.Gasoline:
                    if (gasolineImage != null) gasolineImage.enabled = true;
                    break;
            }
        }
    }

    // Метод для принудительного обновления UI
    public void RefreshUI()
    {
        UpdateImagesVisibility();
    }

    // Метод для скрытия/показа всех Images
    public void ShowAllImages()
    {
        if (waterImage != null) waterImage.enabled = true;
        if (detergentImage != null) detergentImage.enabled = true;
        if (gasolineImage != null) gasolineImage.enabled = true;
    }

    public void HideAllImages()
    {
        if (waterImage != null) waterImage.enabled = false;
        if (detergentImage != null) detergentImage.enabled = false;
        if (gasolineImage != null) gasolineImage.enabled = false;
    }
}
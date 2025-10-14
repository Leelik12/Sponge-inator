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
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color inactiveColor = new Color(1, 1, 1, 0.5f);

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
        // Отключаем UI
        if (waterImage != null) waterImage.color = inactiveColor;
        if (detergentImage != null) detergentImage.color = inactiveColor;
        if (gasolineImage != null) gasolineImage.color = inactiveColor;

        // Отключаем логику жидкостей
        if (waterController != null) waterController.enabled = false;
        if (detergentController != null) detergentController.enabled = false;
        if (gasolineController != null) gasolineController.enabled = false;
    }

    private void ActivateWater()
    {
        if (waterImage != null) waterImage.color = activeColor;
        if (waterController != null) waterController.enabled = true;
    }

    private void ActivateDetergent()
    {
        if (detergentImage != null) detergentImage.color = activeColor;
        if (detergentController != null) detergentController.enabled = true;
    }

    private void ActivateGasoline()
    {
        if (gasolineImage != null) gasolineImage.color = activeColor;
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
}
using UnityEngine;

public class GasSource : MonoBehaviour
{
    [Header("Gas Source Settings")]
    public float gasAmount = 5f;           // Сколько бензина содержит источник
    public float transferRate = 2f;        // Скорость передачи бензина губке (единиц в секунду)

    private bool isDepleted = false;       // Источник исчерпан?

    public float TransferGas(float requestAmount)
    {
        if (isDepleted)
            return 0f;

        // Сколько реально можем отдать (ограничиваем остатком)
        float given = Mathf.Min(requestAmount, gasAmount);
        gasAmount -= given;

        if (gasAmount <= 0f)
        {
            isDepleted = true;
            // Отключаем визуально, но не уничтожаем, чтобы губка не "проваливалась"
            gameObject.SetActive(false);
        }

        return given;
    }
}
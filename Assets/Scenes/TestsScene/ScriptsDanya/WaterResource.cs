using UnityEngine;

public class WaterSource : MonoBehaviour
{
    [Header("Water Source Settings")]
    public float waterAmount = 3f;        // Сколько воды содержит источник
    public float transferRate = 2f;       // Скорость передачи воды губке (единиц в секунду)

    private bool isDepleted = false;      // Источник исчерпан?

    public float TransferWater(float requestAmount)
    {
        if (isDepleted)
            return 0f;

        // Сколько реально можем отдать (ограничиваем остатком)
        float given = Mathf.Min(requestAmount, waterAmount);
        waterAmount -= given;

        if (waterAmount <= 0f)
        {
            isDepleted = true;
            // Можно отключить визуально, но не уничтожать, чтобы губка не "проваливалась"
            gameObject.SetActive(false);
        }

        return given;
    }
}
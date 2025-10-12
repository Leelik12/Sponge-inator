using UnityEngine;

public class ClearSource : MonoBehaviour
{
    [Header("Clear Source Settings")]
    [Tooltip("Количество моющего средства в этом участке")]
    public float clearAmount = 5f; // Объем моющего средства

    [Tooltip("Скорость, с которой оно действует на губку (единиц в секунду)")]
    public float transferRate = 3f;

    private bool isDepleted = false;

    /// <summary>
    /// Передает немного моющего средства губке и уменьшает запас.
    /// Возвращает, сколько реально было передано.
    /// </summary>
    public float TransferClear(float deltaTime)
    {
        if (isDepleted)
            return 0f;

        float transfer = transferRate * deltaTime;
        transfer = Mathf.Min(transfer, clearAmount);

        clearAmount -= transfer;

        if (clearAmount <= 0f)
        {
            isDepleted = true;
            Destroy(gameObject);
        }

        return transfer;
    }
}
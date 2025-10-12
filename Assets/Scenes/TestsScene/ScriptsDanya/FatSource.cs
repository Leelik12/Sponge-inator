using UnityEngine;

public class FatSource : MonoBehaviour
{
    [Header("Fat Source Settings")]
    [Tooltip("Сколько жира содержит этот участок (в единицах)")]
    public float fatAmount = 5f;

    [Tooltip("Скорость передачи жира губке (единиц в секунду)")]
    public float transferRate = 2f;

    private bool isDepleted = false; 

    public float TransferFat(float deltaTime)
    {
        if (isDepleted)
            return 0f;

        // сколько жира передаем за этот кадр
        float transfer = transferRate * deltaTime;

        // ограничиваем передачу остатком
        transfer = Mathf.Min(transfer, fatAmount);

        // вычитаем из запаса
        fatAmount -= transfer;

        // если жир закончился — уничтожаем
        if (fatAmount <= 0f)
        {
            isDepleted = true;
            Destroy(gameObject);
        }

        return transfer;
    }
}
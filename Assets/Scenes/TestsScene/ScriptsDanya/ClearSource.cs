using UnityEngine;

public class ClearSource : MonoBehaviour
{
    [Header("Clear Source Settings")]
    public float clearAmount = 5f; // Îבתול למ‏שודמ סנוהסעגא
    public float transferRate = 3f;

    private bool isDepleted = false;

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
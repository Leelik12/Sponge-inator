using UnityEngine;

public class GasSource : MonoBehaviour
{
    [Header("Gas Settings")]
    public float gasAmount = 5f;           // сколько бензина в этом источнике
    public bool destroyWhenEmpty = true;   // удалять ли объект после опустошения

    // Передаёт бензин губке
    public float TransferGas(float request)
    {
        float given = Mathf.Min(request, gasAmount);
        gasAmount -= given;

        if (destroyWhenEmpty && gasAmount <= 0f)
            Destroy(gameObject);

        return given;
    }
}
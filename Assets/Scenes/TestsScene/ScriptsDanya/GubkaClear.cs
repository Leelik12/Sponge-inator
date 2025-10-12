using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GubkaMovement))]
[RequireComponent(typeof(GubkaFat))]
public class GubkaClear : MonoBehaviour
{
    [Header("Clear Settings")]
    public float absorbSpeed = 1.5f;           // скорость впитывания моющего средства
    public float maxClearCapacity = 10f;       // максимальный запас моющего средства
    public float cleanEfficiency = 1f;         // эффективность очистки (1 ед Clear = 1 ед жира)

    [Header("UI")]
    public Slider clearBar;                    // слайдер запаса моющего средства

    [HideInInspector] public float currentClear = 0f;

    private GubkaFat fat;
    private GubkaMovement movement;

    private bool isNeutralizing = false;

    void Start()
    {
        fat = GetComponent<GubkaFat>();
        movement = GetComponent<GubkaMovement>();

        if (clearBar != null)
        {
            clearBar.minValue = 0f;
            clearBar.maxValue = maxClearCapacity;
            clearBar.value = currentClear;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Clear"))
        {
            ClearSource source = other.GetComponent<ClearSource>();
            if (source == null)
                return;

            // если есть место под Clear — впитываем
            if (currentClear < maxClearCapacity)
            {
                float request = Time.deltaTime * absorbSpeed;
                float absorbed = source.TransferClear(request);

                if (absorbed > 0f)
                    AbsorbClear(absorbed);
            }
        }
    }

    public void AbsorbClear(float amount)
    {
        // если есть жир — моющее сначала его очищает
        if (fat.currentFat > 0f)
        {
            float cleanAmount = amount * cleanEfficiency;

            // снимаем жир и расходуем Clear пропорционально
            float actualClean = Mathf.Min(fat.currentFat, cleanAmount);
            fat.currentFat -= actualClean;

            // если жир был меньше, остаток идёт в запас Clear
            float leftoverClear = amount - actualClean / cleanEfficiency;
            if (leftoverClear > 0f)
                currentClear = Mathf.Min(currentClear + leftoverClear, maxClearCapacity);
        }
        else
        {
            // если жира нет — Clear просто накапливается
            currentClear = Mathf.Min(currentClear + amount, maxClearCapacity);
        }

        UpdateUI();
        fat.UpdateUI();
        fat.AbsorbFat(0f); // пересчёт трения и скорости
    }

    public void TryNeutralizeFat(float fatAmount)
    {
        // предотвращаем рекурсию
        if (isNeutralizing)
            return;

        isNeutralizing = true;

        // если есть Clear — тратим его на нейтрализацию жира
        if (currentClear > 0f)
        {
            float neutralized = Mathf.Min(currentClear, fatAmount);
            currentClear -= neutralized;
            fatAmount -= neutralized;
        }

        // если остался жир — добавляем его в губку
        if (fatAmount > 0f)
            fat.AddFatDirectly(fatAmount); // вызываем безопасный метод

        UpdateUI();
        isNeutralizing = false;
    }

    private void UpdateUI()
    {
        if (clearBar != null)
            clearBar.value = currentClear;
    }
}
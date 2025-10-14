using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GubkaMovement))]
public class GubkaGas : MonoBehaviour
{
    [Header("Gas Settings")]
    public float maxGasCapacity = 10f;     // максимальное количество бензина
    public float absorbSpeed = 3f;         // скорость впитывания бензина

    [Header("UI")]
    public Slider gasBar;                  // слайдер для отображения бензина

    [HideInInspector] public float currentGas = 0f;

    private GubkaMovement movement;

    public HeroVoice player;
    void Start()
    {
        player = GetComponent<HeroVoice>();
        movement = GetComponent<GubkaMovement>();

        if (gasBar != null)
        {
            gasBar.minValue = 0f;
            gasBar.maxValue = maxGasCapacity;
            gasBar.value = currentGas;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Gas"))
        {
            if (currentGas >= maxGasCapacity)
                return;

            GasSource source = other.GetComponent<GasSource>();
            if (source != null)
            {
                float request = Time.deltaTime * absorbSpeed;
                float absorbed = source.TransferGas(request);

                if (absorbed > 0f)
                    AbsorbGas(absorbed);
            }
        }
    }

    private void AbsorbGas(float amount)
    {
        player.SaypickupGas();
        currentGas = Mathf.Min(currentGas + amount, maxGasCapacity);
        UpdateUI();
    }

    internal void UpdateUI()
    {
        if (gasBar != null)
            gasBar.value = currentGas;
    }
}
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI; // понадобится для вывода уровня воды

[RequireComponent(typeof(GubkaMovement))]
public class GubkaWater : MonoBehaviour
{
    [Header("Water Settings")]
    public float maxWaterCapacity = 10f;     // Максимальное количество воды
    public float absorbSpeed = 3f;           // Скорость впитывания воды
    public float waterGrowthFactor = 0.1f;   // Насколько увеличивается размер при полном насыщении
    public float waterSlowFactor = 0.3f;     // Замедление при полном насыщении

    [Header("UI")]
    public Slider waterBar; // ссылка на UI-слайдер, показывающий уровень воды
    public GameObject Lose;
    public GameObject Win;
    [HideInInspector] public float currentWater = 3f;
    public GubkaFat gubkaFat;
    public float fatDamageScale;
    private GubkaMovement movement;
    private Vector3 baseScale;
    private float baseSpeed;
    public HeroVoice player;

    void Start()
    {
        gubkaFat = GetComponent<GubkaFat>();
        player = GetComponent<HeroVoice>();
        movement = GetComponent<GubkaMovement>();
        baseScale = transform.localScale;
        baseSpeed = movement.moveSpeed;
        currentWater = 3f;
        if (waterBar != null)
        {
            waterBar.minValue = 0f;
            waterBar.maxValue = maxWaterCapacity;
            waterBar.value = currentWater;
        }
        StartCoroutine(FatDamage());
    }
    private void Update()
    {
        PlayerDie();
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            // Если губка полна — ничего не происходит
            if (currentWater >= maxWaterCapacity)
                return;

            // Пытаемся получить компонент WaterSource
            WaterSource source = other.GetComponent<WaterSource>();
            if (source != null)
            {
                // Сколько губка может впитать в этот кадр
                float request = Time.deltaTime * absorbSpeed;

                // Сколько реально получила из источника
                float absorbed = source.TransferWater(request);

                // Добавляем эту воду в губку
                if (absorbed > 0f)
                    AbsorbWater(absorbed);
            }
        }
    }

    public void AbsorbWater(float amount)
    {
        if (currentWater >= maxWaterCapacity)
            return;
        player.SaypickupWater();
        currentWater = Mathf.Min(currentWater + amount, maxWaterCapacity);
        UpdateScaleAndSpeed();
        UpdateUI();
    }

    public void UpdateScaleAndSpeed()
    {
        // Увеличиваем размер губки в зависимости от количества воды
        float growth = 1f + (currentWater / maxWaterCapacity) * waterGrowthFactor;
        movement.waterScaleMultiplier = Vector3.one * growth;

        // Замедляем движение (только вычисляем свой фактор)
        float slow = 1f - (currentWater / maxWaterCapacity) * waterSlowFactor;

        // Применяем только свой вклад в общую скорость
        movement.SetWaterSlowFactor(slow);
    }

    internal void UpdateUI()
    {
        if (waterBar != null)
            waterBar.value = currentWater;
    }
    public void PlayerDie()
    {
        if(currentWater <= 0)
        {
            Lose.SetActive(true);
            Time.timeScale = 0f;
        }
    }
    IEnumerator FatDamage()
    {
        while (currentWater > 0f)
        {
            currentWater -= fatDamageScale * gubkaFat.currentFat;
            UpdateUI();
            yield return new WaitForSeconds(1f);
        }
    }
}
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GubkaMovement))]
public class GubkaFat : MonoBehaviour
{
    [Header("Fat Settings")]
    public float maxFatCapacity = 10f;      
    public float absorbSpeed = 3f;          
    public float slowFactor = 0.6f;          
    public float frictionIncrease = 1.5f;    

    [Header("UI")]
    public Slider fatBar;                   

    [HideInInspector] public float currentFat = 0f;

    private GubkaMovement movement;
    private Rigidbody rb;
    private float baseSpeed;
    private PhysicsMaterial baseMaterial;
    private PhysicsMaterial fatMaterial;

    void Start()
    {
        movement = GetComponent<GubkaMovement>();
        rb = GetComponent<Rigidbody>();

        baseSpeed = movement.moveSpeed;

       
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            baseMaterial = col.material;
            fatMaterial = new PhysicsMaterial("FatMaterial");
            fatMaterial.dynamicFriction = 1f;
            fatMaterial.staticFriction = 1f;
        }

        if (fatBar != null)
        {
            fatBar.minValue = 0f;
            fatBar.maxValue = maxFatCapacity;
            fatBar.value = currentFat;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Fat"))
        {
            if (currentFat >= maxFatCapacity)
                return;

            FatSource source = other.GetComponent<FatSource>();
            if (source != null)
            {
                float absorbed = source.TransferFat(Time.deltaTime);
                if (absorbed > 0f)
                    AbsorbFat(absorbed);
            }
        }
    }

    public void AbsorbFat(float amount)
    {
        // ѕровер€ем, есть ли моющее средство, которое может нейтрализовать жир
        GubkaClear clear = GetComponent<GubkaClear>();
        if (clear != null)
        {
            clear.TryNeutralizeFat(amount);
            return;
        }

        if (currentFat >= maxFatCapacity)
            return;

        currentFat = Mathf.Min(currentFat + amount, maxFatCapacity);
        UpdateMovement();
        UpdateUI();
    }
    public void AddFatDirectly(float amount)
    {
        currentFat = Mathf.Min(currentFat + amount, maxFatCapacity);
        UpdateMovement();
        UpdateUI();
    }

    private void UpdateMovement()
    {
        // ¬ычисл€ем замедление от жира
        float slowMultiplier = 1f - (currentFat / maxFatCapacity) * slowFactor;

        // ѕримен€ем только свой вклад в общую скорость
        movement.SetFatSlowFactor(slowMultiplier);

        // ƒополнительно увеличиваем трение при жире
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            float frictionLerp = currentFat / maxFatCapacity;
            PhysicsMaterial mat = new PhysicsMaterial("DynamicFatMat");
            mat.dynamicFriction = Mathf.Lerp(0.6f, 1f, frictionLerp);
            mat.staticFriction = Mathf.Lerp(0.6f, 1f, frictionLerp);
            col.material = mat;
        }
    }

    public void UpdateUI()
    {
        if (fatBar != null)
            fatBar.value = currentFat;
    }
}
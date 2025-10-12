//GubkaMovement
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class GubkaMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 100f;
    public float wobbleAmount = 0.2f;   // сила деформации
    public float wobbleSpeed = 6f;      // скорость "желе"-эффекта
    public float squishRecovery = 5f;   // скорость возврата к нормали

    public float baseMoveSpeed = 5f;
    private float waterSlowFactor = 1f;
    private float fatSlowFactor = 1f;

    private Rigidbody rb;
    private Vector3 defaultScale;
    private Vector3 velocitySmoothing;
    private float wobbleTime;

    [HideInInspector] public Vector3 waterScaleMultiplier = Vector3.one;

    private void UpdateMoveSpeed()
    {
        moveSpeed = baseMoveSpeed * waterSlowFactor * fatSlowFactor;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        defaultScale = transform.localScale;

        // чтобы при старте база скорости совпадала с исходным значением
        baseMoveSpeed = moveSpeed;

        UpdateMoveSpeed();
    }

    void FixedUpdate()
    {
        HandleMovement();
        ApplyJellyEffect();
    }

    void HandleMovement()
    {
        float moveInput = 0f;
        if (Input.GetKey(KeyCode.W)) moveInput = 1f;
        if (Input.GetKey(KeyCode.S)) moveInput = -1f;

        float turnInput = 0f;
        if (Input.GetKey(KeyCode.A)) turnInput = -1f;
        if (Input.GetKey(KeyCode.D)) turnInput = 1f;

        // Инверсия при движении назад
        if (moveInput < 0f)
            turnInput = -turnInput;

        // Поворот
        Quaternion turnOffset = Quaternion.Euler(0f, turnInput * rotationSpeed * Time.fixedDeltaTime, 0f);
        rb.MoveRotation(rb.rotation * turnOffset);

        // Движение
        Vector3 move = transform.forward * moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);

        // Сохраняем "скорость" для анимации желе
        velocitySmoothing = Vector3.Lerp(velocitySmoothing, move, 0.2f);
    }

    void ApplyJellyEffect()
    {
        // Небольшое покачивание при движении
        wobbleTime += Time.deltaTime * (velocitySmoothing.magnitude * wobbleSpeed + 1f);
        float wobble = Mathf.Sin(wobbleTime) * wobbleAmount * velocitySmoothing.magnitude;

        // Деформация по оси Y (высота)
        Vector3 squish = new Vector3(
            defaultScale.x + wobble,
            defaultScale.y - Mathf.Abs(wobble) * 2f,
            defaultScale.z + wobble
        );

        // Плавное восстановление
        Vector3 finalScale = Vector3.Scale(squish, waterScaleMultiplier);
        transform.localScale = Vector3.Lerp(transform.localScale, finalScale, Time.deltaTime * squishRecovery);
    }

    public void SetWaterSlowFactor(float factor)
    {
        waterSlowFactor = Mathf.Clamp(factor, 0.1f, 1f);
        UpdateMoveSpeed();
    }

    public void SetFatSlowFactor(float factor)
    {
        fatSlowFactor = Mathf.Clamp(factor, 0.1f, 1f);
        UpdateMoveSpeed();
    }
}

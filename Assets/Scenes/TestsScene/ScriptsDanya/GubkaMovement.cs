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

    [Header("Jump Settings")]
    public float jumpForce = 5f;        // сила прыжка
    public float groundCheckDistance = 0.6f; // дистанция для проверки земли
    public LayerMask groundLayer;       // слои, считающиеся землёй

    public float baseMoveSpeed = 5f;
    private float waterSlowFactor = 1f;
    private float fatSlowFactor = 1f;

    private Rigidbody rb;
    private Vector3 defaultScale;
    private Vector3 velocitySmoothing;
    private float wobbleTime;

    [HideInInspector] public Vector3 waterScaleMultiplier = Vector3.one;

    private bool isGrounded;
    private Camera playerCamera; // Ссылка на камеру

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
        baseMoveSpeed = moveSpeed;

        // Получаем ссылку на камеру
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = FindAnyObjectByType<Camera>();
            Debug.LogWarning("Main camera not found, searching for any camera...");
        }

        UpdateMoveSpeed();
    }

    void FixedUpdate()
    {
        HandleMovement();
        ApplyJellyEffect();
        CheckGround();
    }

    void Update()
    {
        HandleJumpInput();
    }

    void HandleMovement()
    {
        Vector3 move = Vector3.zero;

        // Получаем направление движения ОТНОСИТЕЛЬНО КАМЕРЫ
        Vector3 cameraForward = playerCamera.transform.forward;
        Vector3 cameraRight = playerCamera.transform.right;

        // Игнорируем наклон камеры по Y для движения
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // Движение относительно камеры
        if (Input.GetKey(KeyCode.W)) move += cameraForward;    // Вперед относительно камеры
        if (Input.GetKey(KeyCode.S)) move += -cameraForward;   // Назад относительно камеры
        if (Input.GetKey(KeyCode.A)) move += -cameraRight;     // Влево относительно камеры  
        if (Input.GetKey(KeyCode.D)) move += cameraRight;      // Вправо относительно камеры

        // Нормализуем вектор, чтобы диагональное движение не было быстрее
        if (move != Vector3.zero)
            move = move.normalized;

        // Применяем скорость и время
        move *= moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);

        // Плавно поворачиваем персонажа в направлении движения (если движется)
        if (move.magnitude > 0.1f)
        {
            Vector3 lookDirection = new Vector3(move.x, 0f, move.z);
            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            }
        }

        velocitySmoothing = Vector3.Lerp(velocitySmoothing, move, 0.2f);
    }

    void HandleJumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    void CheckGround()
    {
        // Луч вниз от центра
        isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, groundCheckDistance, groundLayer);
    }

    void ApplyJellyEffect()
    {
        wobbleTime += Time.deltaTime * (velocitySmoothing.magnitude * wobbleSpeed + 1f);
        float wobble = Mathf.Sin(wobbleTime) * wobbleAmount * velocitySmoothing.magnitude;

        Vector3 squish = new Vector3(
            defaultScale.x + wobble,
            defaultScale.y - Mathf.Abs(wobble) * 2f,
            defaultScale.z + wobble
        );

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

    // Метод для принудительной установки камеры (на случай изменений во время выполнения)
    public void SetPlayerCamera(Camera newCamera)
    {
        playerCamera = newCamera;
    }
}
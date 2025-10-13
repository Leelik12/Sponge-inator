using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;
    public float followSmoothness = 10f;

    [Header("Orbit Settings")]
    public float rotationSpeed = 120f;
    public float pitchMin = -20f;
    public float pitchMax = 60f;

    [Header("View Tilt")]
    public float cameraTilt = 22f;

    private float initialYaw;
    private float initialPitch;
    private float yaw;
    private float pitch;
    private bool isOrbiting = false;
    private Vector3 followOffset;

    void Start()
    {
        if (target == null) return;

        // —охран€ем начальное смещение камеры относительно цели
        followOffset = transform.position - target.position;

        // —охран€ем начальные углы вращени€ камеры
        Vector3 initialEuler = transform.eulerAngles;
        initialYaw = initialEuler.y;
        initialPitch = initialEuler.x;

        // ”станавливаем текущие углы равными начальным
        yaw = initialYaw;
        pitch = initialPitch;
    }

    void LateUpdate()
    {
        if (target == null) return;

        if (Input.GetMouseButtonDown(2))
        {
            // Ќачинаем орбиту, исход€ из текущего положени€ камеры
            Vector3 angles = transform.eulerAngles;
            yaw = angles.y;
            pitch = angles.x;
            if (pitch > 180f) pitch -= 360f; // корректируем диапазон
            isOrbiting = true;
        }
        else if (Input.GetMouseButtonUp(2))
        {
            isOrbiting = false;
        }

        if (isOrbiting)
            OrbitAroundTarget();
        else
            FollowTarget();
    }

    void OrbitAroundTarget()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        yaw += mouseX * rotationSpeed * Time.deltaTime;
        pitch -= mouseY * rotationSpeed * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 desiredPosition = target.position + rotation * followOffset;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * followSmoothness);
        transform.LookAt(target.position + Vector3.up * 1.0f);
    }

    void FollowTarget()
    {
        // ѕросто следуем за целью, сохран€€ начальное смещение и вращение
        Vector3 desiredPosition = target.position + followOffset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * followSmoothness);

        // —охран€ем начальное вращение камеры
        Quaternion desiredRotation = Quaternion.Euler(initialPitch, initialYaw, 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, Time.deltaTime * followSmoothness);
    }
}
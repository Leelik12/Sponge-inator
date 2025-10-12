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
    public float cameraTilt = 22f; // наклон вниз в градусах

    private float yaw;
    private float pitch = 10f;
    private bool isOrbiting = false;
    private Vector3 followOffset;

    void Start()
    {
        if (target == null) return;

        // Вычисляем смещение относительно цели
        followOffset = Quaternion.Inverse(target.rotation) * (transform.position - target.position);

        // Настраиваем начальный yaw/pitch
        Vector3 dir = (transform.position - target.position).normalized;
        yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        pitch = Mathf.Asin(dir.y) * Mathf.Rad2Deg;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // --- Управление вращением по колесику мыши ---
        if (Input.GetMouseButtonDown(2))
        {
            // Начинаем орбиту, исходя из текущего положения камеры
            Vector3 dir = (transform.position - target.position).normalized;
            yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            pitch = Mathf.Asin(dir.y) * Mathf.Rad2Deg;
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
        Vector3 rotatedOffset = target.rotation * followOffset;
        Vector3 desiredPosition = target.position + rotatedOffset;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * followSmoothness);

        // Добавляем постоянный наклон вниз
        Quaternion tiltRotation = Quaternion.Euler(cameraTilt, 0f, 0f);
        Quaternion desiredRotation = target.rotation * tiltRotation;

        transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, Time.deltaTime * followSmoothness);
    }
}
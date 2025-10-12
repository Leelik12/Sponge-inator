using UnityEngine;

[DisallowMultipleComponent]
public class HopBetweenExtremes_X : MonoBehaviour
{
    [Header("Hop geometry (local space)")]
    public float swayDistance = 0.3f;     // амплитуда по X
    public float arcHeight = 0.25f;       // высота прыжка
    public float hopDuration = 0.35f;     // длительность одного прыжка
    public float tiltAngle = 10f;         // угол наклона при прыжке

    [Header("Behaviour")]
    public bool startOnAwake = true;

    private Vector3 localLeft;
    private Vector3 localRight;
    private Vector3 baseLocalPos;
    private Quaternion baseLocalRot;
    private bool hopping = false;
    private float hopTimer = 0f;
    private bool goingToRight = true;

    void Start()
    {
        baseLocalPos = transform.localPosition;
        baseLocalRot = transform.localRotation;

        localLeft = baseLocalPos + new Vector3(-swayDistance, 0f, 0f);
        localRight = baseLocalPos + new Vector3(+swayDistance, 0f, 0f);

        if (startOnAwake)
        {
            hopping = true;
            hopTimer = 0f;
            goingToRight = true;
        }
    }

    void Update()
    {
        if (!hopping) return;

        hopTimer += Time.deltaTime / Mathf.Max(0.0001f, hopDuration);
        float t = Mathf.Clamp01(hopTimer);

        Vector3 from = goingToRight ? localLeft : localRight;
        Vector3 to = goingToRight ? localRight : localLeft;

        float smoothT = Mathf.SmoothStep(0f, 1f, t);
        Vector3 newLocal = Vector3.Lerp(from, to, smoothT);

        // траектория прыжка (арка)
        float yOffset = 4f * arcHeight * t * (1f - t);
        newLocal.y = Mathf.Max(baseLocalPos.y, baseLocalPos.y + yOffset);

        transform.localPosition = newLocal;

        // наклон (вперёд/назад по X)
        if (tiltAngle != 0f)
        {
            float tilt = Mathf.Sin(Mathf.PI * smoothT) * tiltAngle;
            transform.localRotation = baseLocalRot * Quaternion.Euler(tilt, 0f, 0f);
        }

        // конец прыжка меняем направление
        if (hopTimer >= 1f)
        {
            hopTimer = 0f;
            goingToRight = !goingToRight;
            transform.localPosition = goingToRight ? localLeft : localRight;
        }
    }

    public void StartHopping()
    {
        if (!hopping)
        {
            hopping = true;
            hopTimer = 0f;
            float distToLeft = Vector3.Distance(transform.localPosition, localLeft);
            float distToRight = Vector3.Distance(transform.localPosition, localRight);
            goingToRight = distToLeft <= distToRight;
        }
    }

    public void StopHoppingAndReset(bool snapToBase = true)
    {
        hopping = false;
        hopTimer = 0f;
        if (snapToBase)
        {
            transform.localPosition = baseLocalPos;
            transform.localRotation = baseLocalRot;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 basePos = Application.isPlaying ? baseLocalPos : transform.localPosition;
        Vector3 left = basePos + new Vector3(-swayDistance, 0f, 0f);
        Vector3 right = basePos + new Vector3(+swayDistance, 0f, 0f);

        Gizmos.DrawWireSphere(transform.parent != null ? transform.parent.TransformPoint(left) : transform.TransformPoint(left), 0.05f);
        Gizmos.DrawWireSphere(transform.parent != null ? transform.parent.TransformPoint(right) : transform.TransformPoint(right), 0.05f);
    }
#endif
}

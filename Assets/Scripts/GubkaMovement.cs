
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class GubkaMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 100f;

    [Header("Jelly Settings")]
    public float wobbleAmount = 0.2f;
    public float wobbleSpeed = 6f;
    public float squishRecovery = 5f;

    [Header("Climbing Settings")]
    public float surfaceCheckDistance = 1.2f;
    public float surfaceTransitionSpeed = 5f;
    public float climbStickForce = 15f;
    public KeyCode climbKey = KeyCode.E;

    private Rigidbody rb;
    private Vector3 defaultScale;
    private Vector3 velocitySmoothing;
    private float wobbleTime;
    private Vector3 currentSurfaceNormal = Vector3.up;
    private bool isTransitioning = false;
    private bool isClimbing = false;
    private bool canTransition = false;
    private Vector3 nextSurfaceNormal;
    private Vector3 nextSurfacePoint;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        defaultScale = transform.localScale;
    }

    void FixedUpdate()
    {
        if (!isTransitioning)
        {
            CheckForTransitionOpportunity();
            HandleClimbInput();
            HandleMovement();
        }

        ApplyJellyEffect();

        if (isClimbing)
            rb.AddForce(-currentSurfaceNormal * climbStickForce, ForceMode.Acceleration);
        else
            rb.AddForce(Physics.gravity, ForceMode.Acceleration);
    }

    // Проверка возможности перехода
    void CheckForTransitionOpportunity()
    {
        canTransition = false;

        // === 1. Если на полу — проверяем стену впереди ===
        if (!isClimbing)
        {
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, surfaceCheckDistance))
            {
                float angle = Vector3.Angle(Vector3.up, hit.normal);
                if (angle > 45f && angle < 140f)
                {
                    canTransition = true;
                    nextSurfaceNormal = hit.normal;
                    nextSurfacePoint = hit.point;
                }
            }
        }
        // === 2. Если на стене — проверяем горизонтальную под собой ===
        else
        {
            bool wallAhead = Physics.Raycast(transform.position, transform.forward, surfaceCheckDistance);
            bool groundBelow = Physics.Raycast(transform.position, -transform.up, out RaycastHit downHit, surfaceCheckDistance * 1.5f);

            // если стена закончилась, а под губкой уже горизонталь
            if (!wallAhead && groundBelow)
            {
                float angle = Vector3.Angle(Vector3.up, downHit.normal);
                if (angle < 45f)
                {
                    canTransition = true;
                    nextSurfaceNormal = downHit.normal;
                    nextSurfacePoint = downHit.point;
                }
            }
        }
    }

    // Реакция на нажатие клавиши E
    void HandleClimbInput()
    {
        if (Input.GetKeyDown(climbKey) && canTransition)
        {
            StartCoroutine(TransitionToSurface(nextSurfaceNormal, nextSurfacePoint));
            isClimbing = !isClimbing;
        }
    }

    // Плавный переход между поверхностями
    IEnumerator TransitionToSurface(Vector3 targetNormal, Vector3 contactPoint)
    {
        isTransitioning = true;

        Quaternion startRot = transform.rotation;
        Quaternion endRot = Quaternion.FromToRotation(transform.up, targetNormal) * transform.rotation;

        Vector3 startPos = transform.position;
        Vector3 endPos = contactPoint + targetNormal * 0.5f;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * surfaceTransitionSpeed;
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        currentSurfaceNormal = targetNormal;
        isTransitioning = false;
    }

    void HandleMovement()
    {
        float moveInput = 0f;
        if (Input.GetKey(KeyCode.W)) moveInput = 1f;
        if (Input.GetKey(KeyCode.S)) moveInput = -1f;

        float turnInput = 0f;
        if (Input.GetKey(KeyCode.A)) turnInput = -1f;
        if (Input.GetKey(KeyCode.D)) turnInput = 1f;

        if (moveInput < 0f)
            turnInput = -turnInput;

        // направления в плоскости поверхности
        Vector3 forwardOnSurface = Vector3.ProjectOnPlane(transform.forward, currentSurfaceNormal).normalized;
        Vector3 rightOnSurface = Vector3.Cross(currentSurfaceNormal, forwardOnSurface).normalized;

        // барьер — если впереди конец стены
        bool blocked = false;
        if (isClimbing)
        {
            // Проверяем, есть ли поверхность впереди
            if (!Physics.Raycast(transform.position, forwardOnSurface, surfaceCheckDistance))
            {
                blocked = true; // впереди пустота
            }
        }

        // вращение
        Quaternion localTurn = Quaternion.AngleAxis(turnInput * rotationSpeed * Time.fixedDeltaTime, currentSurfaceNormal);
        Vector3 newForward = localTurn * forwardOnSurface;
        Quaternion targetRot = Quaternion.LookRotation(newForward, currentSurfaceNormal);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, 0.2f));

        // движение (если не заблокировано)
        if (!blocked)
        {
            Vector3 moveDir = forwardOnSurface * moveInput;
            Vector3 move = moveDir * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + move);

            velocitySmoothing = Vector3.Lerp(velocitySmoothing, move, 0.2f);
        }
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

        transform.localScale = Vector3.Lerp(transform.localScale, squish, Time.deltaTime * squishRecovery);
    }

    private void OnDrawGizmosSelected()
    {
        // Для визуализации лучей
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, transform.forward * surfaceCheckDistance);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, -transform.up * surfaceCheckDistance);
    }
}
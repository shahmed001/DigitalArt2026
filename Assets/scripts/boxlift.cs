using UnityEngine;
using System.Collections;

public class BoxLift : MonoBehaviour
{
    [Header("Lift Settings")]
    [SerializeField] private float liftSpeed = 2f;
    [SerializeField] private float targetHeight = 10f;

    [Tooltip("Optional: drag roof GameObject here instead of setting height manually")]
    [SerializeField] private Transform roofTarget;

    [Tooltip("Offset added to final Y position (negative = lower). Useful if box pivot is at center.")]
    [SerializeField] private float arrivalOffset = 0f;

    [Header("State")]
    [SerializeField] private bool isLifting = false;
    [SerializeField] private bool hasArrived = false;

    private Rigidbody rb;
    private float finalTargetY;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        RecalculateTarget();
    }

    // Call this if you move the roof target at runtime
    public void RecalculateTarget()
    {
        finalTargetY = roofTarget != null ? roofTarget.position.y : targetHeight;
        finalTargetY += arrivalOffset; // Apply the offset
    }

    public void StartLift()
    {
        if (isLifting || hasArrived)
        {
            Debug.Log("[BoxLift] Already lifting or arrived — ignoring");
            return;
        }

        RecalculateTarget(); // Recalculate in case target moved
        Debug.Log("[BoxLift] Starting lift to Y = " + finalTargetY);
        StartCoroutine(LiftRoutine());
    }

    public void LowerBox()
    {
        if (isLifting) return;
        hasArrived = false;
        StartCoroutine(LowerRoutine());
    }

    private IEnumerator LiftRoutine()
    {
        isLifting = true;
        if (rb != null) rb.isKinematic = true;

        while (transform.position.y < finalTargetY - 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                new Vector3(transform.position.x, finalTargetY, transform.position.z),
                liftSpeed * Time.deltaTime
            );
            yield return null;
        }

        // Snap exactly to adjusted target
        transform.position = new Vector3(
            transform.position.x,
            finalTargetY,
            transform.position.z
        );

        isLifting = false;
        hasArrived = true;
        Debug.Log("[BoxLift] ✅ Reached Y = " + finalTargetY);
    }

    private IEnumerator LowerRoutine()
    {
        isLifting = true;
        float groundY = 0f; // Change this if your floor isn't at Y=0

        while (transform.position.y > groundY + 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                new Vector3(transform.position.x, groundY, transform.position.z),
                liftSpeed * Time.deltaTime
            );
            yield return null;
        }

        transform.position = new Vector3(transform.position.x, groundY, transform.position.z);

        if (rb != null) rb.isKinematic = false;
        isLifting = false;
    }
}
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartSnapZone : MonoBehaviour
{
    [SerializeField] private SnapInteractable snapInteractable;

    [Tooltip("Only these objects can snap here. Leave empty to allow all.")]
    [SerializeField] private List<GameObject> allowedObjects = new List<GameObject>();

    private SnapInteractor currentSnappedInteractor;
    private IInteractable currentGrabInteractable; // FIX: use interface, works for ALL grab types
    private Rigidbody currentRigidbody;
    private bool wasGrabbed = false;
    private bool isSnapped = false;

    private void OnEnable()
    {
        snapInteractable.WhenSelectingInteractorViewAdded += OnObjectSnapped;
        snapInteractable.WhenSelectingInteractorViewRemoved += OnObjectUnsnapped;
    }

    private void OnDisable()
    {
        snapInteractable.WhenSelectingInteractorViewAdded -= OnObjectSnapped;
        snapInteractable.WhenSelectingInteractorViewRemoved -= OnObjectUnsnapped;
    }

    private void Update()
    {
        if (!isSnapped) return;
        if (currentSnappedInteractor == null) return;
        if (currentGrabInteractable == null) return;

        bool isCurrentlyGrabbed = IsBeingGrabbed();

        if (isCurrentlyGrabbed && !wasGrabbed)
        {
            wasGrabbed = true;
            Debug.Log("[SmartSnapZone] Grab detected — releasing snap");
            ReleaseSnap();
        }
        else if (!isCurrentlyGrabbed && wasGrabbed)
        {
            wasGrabbed = false;
            Debug.Log("[SmartSnapZone] Hand released — re-enabling snap");
            var interactor = currentSnappedInteractor;
            ResetState();
            StartCoroutine(ReenableSnapInteractor(interactor));
        }
    }

    private void OnObjectSnapped(IInteractorView view)
    {
        if (view is not SnapInteractor snapInteractor) return;

        if (!IsAllowed(snapInteractor))
        {
            Debug.Log($"[SmartSnapZone] Rejected: {snapInteractor.transform.root.name}");
            StartCoroutine(ForceRelease(snapInteractor));
            return;
        }

        Debug.Log($"[SmartSnapZone] Snapped: {snapInteractor.transform.root.name}");

        currentSnappedInteractor = snapInteractor;
        wasGrabbed = false;
        isSnapped = true;

        // Find Rigidbody anywhere in hierarchy
        currentRigidbody =
            snapInteractor.GetComponentInParent<Rigidbody>() ??
            snapInteractor.transform.root.GetComponentInChildren<Rigidbody>();

        // FIX: search for ALL grab interactable types using IInteractable interface
        currentGrabInteractable = FindGrabInteractable(snapInteractor.transform.root);

        if (currentGrabInteractable == null)
            Debug.LogWarning("[SmartSnapZone] No grab interactable found on: "
                             + snapInteractor.transform.root.name);
        else
            Debug.Log("[SmartSnapZone] Found grab interactable: "
                      + (currentGrabInteractable as MonoBehaviour)?.name);
    }

    private void OnObjectUnsnapped(IInteractorView view)
    {
        if (!wasGrabbed)
        {
            Debug.Log("[SmartSnapZone] Unsnapped externally");
            ResetState();
        }
    }

    private void ReleaseSnap()
    {
        // FIX: force kinematic OFF so hand can move the object
        if (currentRigidbody != null)
        {
            currentRigidbody.isKinematic = false;
            currentRigidbody.linearVelocity = Vector3.zero;
            currentRigidbody.angularVelocity = Vector3.zero;
        }

        if (currentSnappedInteractor != null)
            currentSnappedInteractor.gameObject.SetActive(false);
    }

    private bool IsBeingGrabbed()
    {
        if (currentGrabInteractable == null) return false;
        // IInteractable.SelectingInteractorViews works for ALL interactable types
        foreach (var _ in currentGrabInteractable.SelectingInteractorViews)
            return true;
        return false;
    }

    // FIX: finds HandGrabInteractable OR DistanceHandGrabInteractable OR any IInteractable
    private IInteractable FindGrabInteractable(Transform root)
    {
        // Try HandGrabInteractable first
        var hgi = root.GetComponentInChildren<HandGrabInteractable>();
        if (hgi != null) return hgi;

        // Try DistanceHandGrabInteractable (your setup uses this)
        var dgi = root.GetComponentInChildren<DistanceHandGrabInteractable>();
        if (dgi != null) return dgi;

        // Try GrabInteractable as fallback
        var gi = root.GetComponentInChildren<GrabInteractable>();
        if (gi != null) return gi;

        return null;
    }

    public bool IsObjectAllowed(SnapInteractor interactor) => IsAllowed(interactor);

    private IEnumerator ForceRelease(SnapInteractor interactor)
    {
        interactor.gameObject.SetActive(false);
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        if (interactor != null)
            interactor.gameObject.SetActive(true);
    }

    private IEnumerator ReenableSnapInteractor(SnapInteractor interactor)
    {
        yield return new WaitForSeconds(0.4f);
        if (interactor != null)
            interactor.gameObject.SetActive(true);
    }

    private void ResetState()
    {
        currentSnappedInteractor = null;
        currentGrabInteractable = null;
        currentRigidbody = null;
        isSnapped = false;
        wasGrabbed = false;
    }

    private bool IsAllowed(SnapInteractor interactor)
    {
        if (allowedObjects == null || allowedObjects.Count == 0) return true;
        Transform t = interactor.transform;
        while (t != null)
        {
            if (allowedObjects.Contains(t.gameObject)) return true;
            t = t.parent;
        }
        return false;
    }
}
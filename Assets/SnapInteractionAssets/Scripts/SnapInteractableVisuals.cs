using Oculus.Interaction;
using System.Collections;
using UnityEngine;

public class SnapInteractableVisuals : MonoBehaviour
{
    [SerializeField] private SnapInteractable snapInteractable;
    [SerializeField] private Material hoverMaterial;

    private GameObject currentInteractorGameObject;
    private SnapInteractor currentInteractor;
    private SmartSnapZone snapZoneFilter;

    private void Awake()
    {
        snapZoneFilter = GetComponent<SmartSnapZone>();
    }

    private void OnEnable()
    {
        snapInteractable.WhenInteractorAdded.Action += WhenInteractorAdded_Action;
        snapInteractable.WhenSelectingInteractorViewAdded += SnapInteractable_WhenSelectingInteractorViewAdded;
        snapInteractable.WhenInteractorViewRemoved += SnapInteractable_WhenInteractorViewRemoved;
        snapInteractable.WhenInteractorViewAdded += SnapInteractable_WhenInteractorViewAdded;
    }

    private void OnDisable()
    {
        snapInteractable.WhenInteractorAdded.Action -= WhenInteractorAdded_Action;
        snapInteractable.WhenSelectingInteractorViewAdded -= SnapInteractable_WhenSelectingInteractorViewAdded;
        snapInteractable.WhenInteractorViewRemoved -= SnapInteractable_WhenInteractorViewRemoved;
        snapInteractable.WhenInteractorViewAdded -= SnapInteractable_WhenInteractorViewAdded;
    }

    private void WhenInteractorAdded_Action(SnapInteractor obj)
    {
        // Don't show ghost for non-allowed objects
        if (!IsAllowedInteractor(obj)) return;

        if (currentInteractor == null)
            currentInteractor = obj;
        else if (currentInteractor != obj)
        {
            currentInteractor = obj;
            Destroy(currentInteractorGameObject);
            currentInteractorGameObject = null;
        }
        else
            return;

        SetupGhostModel(obj);
    }

    private void SnapInteractable_WhenSelectingInteractorViewAdded(IInteractorView obj)
    {
        // FIX: only hide ghost if the snapping object is the allowed one
        if (obj is SnapInteractor si && !IsAllowedInteractor(si)) return;
        currentInteractorGameObject?.SetActive(false);
    }

    private void SnapInteractable_WhenInteractorViewAdded(IInteractorView obj)
    {
        // FIX: check if this specific interactor is allowed before showing ghost
        if (obj is SnapInteractor snapInteractor)
        {
            if (!IsAllowedInteractor(snapInteractor)) return;

            // FIX: only show ghost if this is our tracked interactor
            if (snapInteractor != currentInteractor) return;
        }

        currentInteractorGameObject?.SetActive(true);
    }

    private void SnapInteractable_WhenInteractorViewRemoved(IInteractorView obj)
    {
        // FIX: only react if this is our tracked allowed interactor
        if (obj is SnapInteractor si)
        {
            if (!IsAllowedInteractor(si)) return;

            // Clean up if our tracked interactor left
            if (si == currentInteractor)
            {
                currentInteractor = null;
                if (currentInteractorGameObject != null)
                    currentInteractorGameObject.SetActive(false);
            }
            return;
        }

        currentInteractorGameObject?.SetActive(false);
    }

    // FIX: single method handles both filter and null checks
    private bool IsAllowedInteractor(SnapInteractor interactor)
    {
        if (interactor == null) return false;
        if (snapZoneFilter == null) return true;
        return snapZoneFilter.IsObjectAllowed(interactor);
    }

    private void SetupGhostModel(SnapInteractor interactor)
    {
        Transform interactorParent = interactor.transform.parent;
        if (interactorParent == null) return;

        currentInteractorGameObject = new GameObject(interactorParent.name + "_Ghost");
        currentInteractorGameObject.transform.SetParent(transform, false);

        Vector3 objWorldScale = interactorParent.lossyScale;
        Vector3 zoneWorldScale = transform.lossyScale;
        currentInteractorGameObject.transform.localScale = new Vector3(
            objWorldScale.x / zoneWorldScale.x,
            objWorldScale.y / zoneWorldScale.y,
            objWorldScale.z / zoneWorldScale.z
        );

        currentInteractorGameObject.transform.localPosition =
            -interactor.transform.localPosition;
        currentInteractorGameObject.transform.localRotation =
            Quaternion.Inverse(interactor.transform.localRotation);

        var parentMesh = interactorParent.GetComponent<MeshFilter>();
        if (parentMesh != null)
        {
            currentInteractorGameObject.AddComponent<MeshFilter>().mesh = parentMesh.mesh;
            currentInteractorGameObject.AddComponent<MeshRenderer>().material = hoverMaterial;
        }

        foreach (Transform child in interactorParent)
        {
            foreach (var item in child.GetComponentsInChildren<MeshFilter>())
            {
                var newGo = new GameObject(item.name);
                newGo.transform.SetParent(currentInteractorGameObject.transform, false);
                newGo.transform.localPosition = item.transform.localPosition;
                newGo.transform.localRotation = item.transform.localRotation;
                newGo.transform.localScale = item.transform.localScale;
                newGo.AddComponent<MeshFilter>().mesh = item.mesh;
                newGo.AddComponent<MeshRenderer>().material = hoverMaterial;
            }
        }
    }
}
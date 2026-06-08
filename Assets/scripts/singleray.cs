using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UnityEngine;

public class SingleRayManager : MonoBehaviour
{
    [SerializeField] private DistanceHandGrabInteractor leftInteractor;
    [SerializeField] private DistanceHandGrabInteractor rightInteractor;

    [SerializeField] private GameObject leftRayVisual;
    [SerializeField] private GameObject rightRayVisual;

    // Which hand was first to get a candidate
    private bool leftWasFirst = false;

    private void Update()
    {
        bool leftHas = leftInteractor.HasCandidate;
        bool rightHas = rightInteractor.HasCandidate;

        if (leftHas && rightHas)
        {
            // Both pointing at something — keep whichever got there first
            leftRayVisual.SetActive(leftWasFirst);
            rightRayVisual.SetActive(!leftWasFirst);
        }
        else if (leftHas)
        {
            leftWasFirst = true;
            leftRayVisual.SetActive(true);
            rightRayVisual.SetActive(false);
        }
        else if (rightHas)
        {
            leftWasFirst = false;
            leftRayVisual.SetActive(false);
            rightRayVisual.SetActive(true);
        }
        else
        {
            // Neither pointing at anything
            leftRayVisual.SetActive(false);
            rightRayVisual.SetActive(false);
        }
    }
}
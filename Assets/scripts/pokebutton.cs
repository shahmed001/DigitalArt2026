using Oculus.Interaction;
using UnityEngine;

public class PokeButton : MonoBehaviour
{
    [SerializeField] private PokeInteractable pokeInteractable;
    [SerializeField] private BoxLift boxLift;
    [SerializeField] private float pokeCooldown = 1f;
    private float lastPokeTime = -999f;

    void Start()
    {
        if (pokeInteractable == null)
        {
            Debug.LogError("[PokeButton] ❌ PokeInteractable is NOT assigned! Drag it into the Inspector.");
            return;
        }
        if (boxLift == null)
        {
            Debug.LogError("[PokeButton] ❌ BoxLift is NOT assigned!");
            return;
        }

        pokeInteractable.WhenStateChanged += OnStateChanged;
        Debug.Log("[PokeButton] ✅ Ready — waiting for poke");
    }

    void OnDestroy()
    {
        if (pokeInteractable != null)
            pokeInteractable.WhenStateChanged -= OnStateChanged;
    }

    private void OnStateChanged(InteractableStateChangeArgs args)
    {
        // Select = finger pressed the surface
        if (args.NewState == InteractableState.Select)
        {
            if (Time.time - lastPokeTime < pokeCooldown) return;
            lastPokeTime = Time.time;

            Debug.Log("[PokeButton] ✅ POKE DETECTED! Starting lift...");
            boxLift.StartLift();
        }
    }
}
using UnityEngine;

/// <summary>
/// Generic "death condition" trigger. Drop onto any hazard object built by
/// the obstacles workstream (water, crusher, spinning wheel, laser, moving
/// panel, etc.) - on contact with the active rat it reports the death to
/// RatLifeManager rather than each obstacle implementing its own
/// life-loss logic.
///
/// Kavish - Game systems (primary responsibility).
/// </summary>
[RequireComponent(typeof(Collider))]
public class HazardTrigger : MonoBehaviour
{
    [SerializeField] private RatLifeManager lifeManager;
    [SerializeField] private string ratTag = "Player";

    private void Awake()
    {
        if (lifeManager == null) lifeManager = FindFirstObjectByType<RatLifeManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerRatController rat = other.GetComponentInParent<PlayerRatController>();
        if (rat == null) return;
        lifeManager?.OnRatDied();
    }
}

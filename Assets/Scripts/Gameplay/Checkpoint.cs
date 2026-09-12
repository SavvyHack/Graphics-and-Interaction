using UnityEngine;

/// <summary>
/// Attach to a trigger volume placed at each checkpoint in the enclosure.
/// Updates RatLifeManager's respawn point when the active rat passes
/// through, notifies TavishPrototypeIntegration so the HUD checkpoint
/// label updates, and plays a positive confirmation cue per the GDD's
/// audio table.
///
/// Kavish - Game systems.
/// </summary>
[RequireComponent(typeof(Collider))]
public class Checkpoint : MonoBehaviour
{
    [Tooltip("1-based checkpoint number, shown on the HUD as 'Checkpoint N'.")]
    [SerializeField] private int checkpointNumber = 1;

    [SerializeField] private RatLifeManager lifeManager;
    [SerializeField] private TavishPrototypeIntegration integration;
    [SerializeField] private string ratTag = "Player";

    [Tooltip("If true, this checkpoint only fires once per attempt.")]
    [SerializeField] private bool triggerOnlyOnce = false;
    private bool hasTriggered;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void Awake()
    {
        if (lifeManager == null) lifeManager = FindFirstObjectByType<RatLifeManager>();
        if (integration == null) integration = FindFirstObjectByType<TavishPrototypeIntegration>();
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerRatController rat = other.GetComponentInParent<PlayerRatController>();
        if (rat == null) return;
        if (triggerOnlyOnce && hasTriggered) return;

        hasTriggered = true;
        lifeManager?.SetCheckpoint(transform.position, checkpointNumber);
        integration?.OnCheckpointReached(checkpointNumber);
        AudioManager.Instance?.PlayCheckpoint();
    }
}

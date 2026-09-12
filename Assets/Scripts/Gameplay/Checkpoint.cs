using UnityEngine;

/// <summary>
/// Checkpoint trigger for the three-rat life system.
/// The trigger can use a separate safe respawn transform so the next rat is
/// never spawned in the middle of the trigger volume.
///
/// Kavish - game systems.
/// </summary>
[RequireComponent(typeof(Collider))]
public class Checkpoint : MonoBehaviour
{
    [SerializeField] private int checkpointNumber = 1;
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private RatLifeManager lifeManager;
    [SerializeField] private bool triggerOnlyOnce = true;

    private bool hasTriggered;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void Awake()
    {
        if (lifeManager == null)
            lifeManager = FindFirstObjectByType<RatLifeManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerRatController>() == null)
            return;

        if (triggerOnlyOnce && hasTriggered)
            return;

        hasTriggered = true;
        Vector3 spawn = respawnPoint != null ? respawnPoint.position : transform.position;
        lifeManager?.SetCheckpoint(spawn, checkpointNumber);
        AudioManager.Instance?.PlayCheckpoint();
    }
}

using UnityEngine;

/// <summary>
/// Attach to the trigger volume at the enclosure exit. Fires the win
/// condition once the active rat reaches it, per the GDD's "reach the exit
/// with at least one rat remaining" win state.
///
/// Kavish - Game systems (primary responsibility).
/// </summary>
[RequireComponent(typeof(Collider))]
public class ExitTrigger : MonoBehaviour
{
    [SerializeField] private string ratTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        PlayerRatController rat = other.GetComponentInParent<PlayerRatController>();
        if (rat == null) return;
        GameManager.Instance?.WinGame();
    }
}

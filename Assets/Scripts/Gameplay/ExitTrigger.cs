using UnityEngine;

/// <summary>
/// Win-condition trigger: reaching the enclosure exit with any rat remaining
/// completes the attempt.
///
/// Kavish - game systems.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ExitTrigger : MonoBehaviour
{
    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerRatController>() != null)
            GameManager.Instance?.WinGame();
    }
}

using UnityEngine;

/// <summary>
/// Generic life-loss trigger. Attach it to water/runoff, lasers, crushers,
/// moving hazards, wheel arms, or any other hazard collider.
///
/// Kavish - game systems.
/// </summary>
[RequireComponent(typeof(Collider))]
public class HazardTrigger : MonoBehaviour
{
    [SerializeField] private RatLifeManager lifeManager;

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
        TryKill(other);
    }

    private void OnTriggerStay(Collider other)
    {
        // CharacterController contacts can occasionally begin already inside a
        // moving trigger. RatLifeManager's invulnerability window debounces this.
        TryKill(other);
    }

    private void TryKill(Collider other)
    {
        if (other.GetComponentInParent<PlayerRatController>() != null)
            lifeManager?.OnRatDied();
    }
}

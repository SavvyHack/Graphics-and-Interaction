using UnityEngine;

/// <summary>
/// Generic life-loss trigger. Attach it to water/runoff, lasers, crushers,
/// moving hazards, wheel arms, or any other hazard collider.
///
/// Game Systems - game systems.
/// </summary>
[RequireComponent(typeof(Collider))]
public class HazardTrigger : MonoBehaviour
{
    [SerializeField] private RatLifeManager lifeManager;
    public bool bypassShield;

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
        var player = other.GetComponentInParent<PlayerRatController>();
        if (player == null || lifeManager == null || lifeManager.Invulnerable || lifeManager.Finished ||
            (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)) return;
        var powers = player.GetComponent<RatPowerups>();
        // A phasing rat passes through machinery and energy; coolant and falls (bypassShield) still count.
        if (!bypassShield && powers != null && powers.Has(RatAugment.Phase)) return;
        if (!bypassShield && powers != null && powers.AbsorbHit()) return;
        lifeManager.OnRatDied();
    }
}

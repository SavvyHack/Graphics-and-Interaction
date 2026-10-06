using UnityEngine;

/// <summary>
/// Cracked floor hatch. A ground-pounding rat smashes it and keeps falling.
/// RatPowerups.ResetPowers restores every hatch when a new rat starts or the puzzle resets.
/// </summary>
[RequireComponent(typeof(Collider))]
public class BreakableHatch : MonoBehaviour
{
    [SerializeField] private GameObject intact;
    [SerializeField] private GameObject broken;
    private Collider solid;

    public bool IsBroken => solid != null && !solid.enabled;

    private void Awake()
    {
        solid = GetComponent<Collider>();
        Restore();
    }

    public bool Break()
    {
        if (IsBroken) return false;
        solid.enabled = false;
        if (intact != null) intact.SetActive(false);
        if (broken != null) broken.SetActive(true);
        AudioManager.Instance?.PlayCheckpoint();
        return true;
    }

    public void Restore()
    {
        if (solid == null) solid = GetComponent<Collider>();
        solid.enabled = true;
        if (intact != null) intact.SetActive(true);
        if (broken != null) broken.SetActive(false);
    }
}

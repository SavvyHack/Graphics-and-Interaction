using UnityEngine;

/// <summary>
/// Solid containment field. A rat with the Phase augment passes straight through it.
/// The field only turns solid again once the rat is clear, so it can never trap a rat inside.
/// </summary>
[RequireComponent(typeof(Collider))]
public class PhaseBarrier : MonoBehaviour
{
    [SerializeField] private Renderer field;
    [SerializeField, Range(0f, 1f)] private float phasedAlpha = 0.25f;
    private Collider solid;
    private Bounds area;
    private PlayerRatController player;
    private MaterialPropertyBlock block;

    public bool IsSolid => solid != null && solid.enabled;

    private void Awake()
    {
        solid = GetComponent<Collider>();
        area = solid.bounds;
        player = FindFirstObjectByType<PlayerRatController>();
        block = new MaterialPropertyBlock();
    }

    private void Update()
    {
        bool phasing = RatPowerups.Active != null && RatPowerups.Active.Has(RatAugment.Phase);
        if (phasing) solid.enabled = false;
        else if (!solid.enabled && !Overlapping()) solid.enabled = true;

        if (field != null)
        {
            // The shader's overall alpha fades while the field is passable.
            field.GetPropertyBlock(block);
            block.SetFloat("_Alpha", solid.enabled ? 0.86f : phasedAlpha);
            field.SetPropertyBlock(block);
        }
    }

    private bool Overlapping()
    {
        if (player == null) return false;
        var controller = player.GetComponent<CharacterController>();
        return controller != null && area.Intersects(controller.bounds);
    }
}

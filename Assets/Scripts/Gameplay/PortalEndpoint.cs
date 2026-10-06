using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Explicit fixed-pair transfer. Both endpoints and a clear, grounded arrival are required.</summary>
public class PortalEndpoint : MonoBehaviour
{
    [SerializeField] private PortalEndpoint partner;
    [SerializeField] private Transform arrival;
    [SerializeField] private string pairLabel = "A";
    [SerializeField] private RatLifeManager lives;
    [SerializeField] private FixedCameraFollow follow;
    private bool mustLeave;
    private float readyAt;
    public string Prompt { get; private set; }
    private void Update()
    {
        Prompt = null;
        if (lives == null || lives.Player == null || GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;
        PlayerRatController player = lives.Player;
        bool nearby = Vector2.Distance(player.transform.position, transform.position) < 1.25f;
        if (!nearby) { mustLeave = false; return; }
        if (mustLeave || Time.time < readyAt) return;
        Prompt = "E - Transfer " + pairLabel;
        if (Keyboard.current == null || !Keyboard.current.eKey.wasPressedThisFrame) return;
        if (partner == null || partner.partner != this || !partner.isActiveAndEnabled || partner.arrival == null || partner.gameObject.scene != gameObject.scene || !Clear(player, partner.arrival.position))
        { Prompt = "Destination blocked"; return; }
        player.Respawn(partner.arrival.position);
        partner.mustLeave = true; partner.readyAt = readyAt = Time.time + .25f;
        follow.SetTarget(player.transform, true);
        AudioManager.Instance?.PlayCheckpoint();
    }
    private static bool Clear(PlayerRatController player, Vector3 position)
    {
        CharacterController cc = player.GetComponent<CharacterController>();
        Vector3 centre = position + cc.center;
        float half = Mathf.Max(0, cc.height * .5f - cc.radius);
        foreach (Collider hit in Physics.OverlapCapsule(centre + Vector3.up * half, centre - Vector3.up * half, cc.radius * .95f, ~0, QueryTriggerInteraction.Collide))
        {
            if (hit == cc || hit.transform.IsChildOf(player.transform)) continue;
            if (!hit.isTrigger || hit.GetComponentInParent<HazardTrigger>() != null) return false;
        }
        return Physics.Raycast(position + Vector3.up * .1f, Vector3.down, .5f, ~0, QueryTriggerInteraction.Ignore);
    }
    public void ResetTransfer() { mustLeave = false; readyAt = 0; Prompt = null; }
}

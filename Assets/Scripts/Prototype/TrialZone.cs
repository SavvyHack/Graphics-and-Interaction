using UnityEngine;

public class TrialZone : MonoBehaviour
{
    public enum ZoneType { Hazard, Checkpoint, Exit }
    public ZoneType type;
    public int checkpointNumber;
    public Transform respawn;
    public RatTrialSession session;

    private void OnTriggerEnter(Collider other) { Visit(other); }
    private void OnTriggerStay(Collider other) { Visit(other); }
    private void Visit(Collider other)
    {
        if (session == null || other.GetComponent<PlayerRatController>() != session.player) return;
        if (type == ZoneType.Hazard) session.LoseRat();
        else if (type == ZoneType.Exit) session.Complete();
        else session.Checkpoint(checkpointNumber, respawn.position);
    }
}

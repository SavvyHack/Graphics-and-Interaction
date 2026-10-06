using UnityEngine;

/// <summary>One authored puzzle room per checkpoint; prior rooms keep their solved state.</summary>
public class PuzzleRoom : MonoBehaviour
{
    [SerializeField] private int checkpointNumber;
    [SerializeField] private PushBlock[] blocks;
    [SerializeField] private LatchedSwitch[] switches;
    [SerializeField] private PortalEndpoint[] portals;
    [SerializeField] private WarningHazard[] hazards;
    public int CheckpointNumber => checkpointNumber;
    public void ResetRoom()
    {
        foreach (PushBlock block in blocks) if (block != null) block.ResetToOrigin();
        foreach (LatchedSwitch latch in switches) if (latch != null) latch.SetOpen(false);
        foreach (PortalEndpoint portal in portals) if (portal != null) portal.ResetTransfer();
        foreach (WarningHazard hazard in hazards) if (hazard != null) hazard.ResetCycle();
        Physics.SyncTransforms();
    }
}

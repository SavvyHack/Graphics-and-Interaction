using UnityEngine;

/// <summary>
/// Small adapter that connects the camera and HUD to the rest of the prototype.
/// Other systems can call these public methods without Tavish's scripts needing
/// to know the exact implementation of the team's LifeManager or CheckpointManager.
/// </summary>
public class TavishPrototypeIntegration : MonoBehaviour
{
    [SerializeField] private FixedCameraFollow cameraFollow;
    [SerializeField] private PrototypeHUD hud;
    [SerializeField] private Transform[] rats;

    private void Start()
    {
        if (rats != null && rats.Length > 0 && rats[0] != null)
            ActivateRat(0, rats.Length, true);
    }

    /// <summary>
    /// activeRatIndex is zero-based: 0 = Rat 1, 1 = Rat 2, 2 = Rat 3.
    /// remainingRats is the number of lives still available.
    /// </summary>
    public void ActivateRat(int activeRatIndex, int remainingRats, bool snapCamera = true)
    {
        if (rats == null || activeRatIndex < 0 || activeRatIndex >= rats.Length)
        {
            Debug.LogWarning("TavishPrototypeIntegration: invalid active rat index.");
            return;
        }

        if (cameraFollow != null)
            cameraFollow.SetTarget(rats[activeRatIndex], snapCamera);

        if (hud != null)
            hud.SetRemainingRats(remainingRats);
    }

    public void OnCheckpointReached(int checkpointNumber)
    {
        if (hud != null)
            hud.SetCheckpoint(checkpointNumber);
    }

    public void OnGameCompleted()
    {
        if (hud != null)
            hud.ShowCompletion();
    }

    public void OnGameFailed()
    {
        if (hud != null)
            hud.ShowFailure();
    }
}

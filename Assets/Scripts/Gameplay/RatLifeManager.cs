using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Implements the three-rat life system from the GDD: the player begins
/// with three rats acting as lives, represented as separate rat GameObjects
/// (README section 3 - lives are physical rats, not an abstract counter).
///
/// When the active rat dies, the next rat in the array becomes controllable
/// and resumes from the most recent checkpoint. When all three rats are
/// lost, GameManager restarts the attempt from the beginning of the
/// enclosure.
///
/// Talks to TavishPrototypeIntegration exactly per its documented contract:
///   ActivateRat(index, remainingRats, snapCamera) - on every rat switch.
/// Checkpoint.cs calls integration.OnCheckpointReached() directly, and
/// GameManager calls OnGameCompleted()/OnGameFailed() directly, so this
/// script only owns the "which rat is alive/active" piece.
///
/// Kavish - Game systems (primary responsibility).
/// </summary>
public class RatLifeManager : MonoBehaviour
{
    [Header("Rats (size should be 3 - Rat 1, Rat 2, Rat 3)")]
    [Tooltip("The three rat GameObjects in scene order. Index 0 starts active.")]
    [SerializeField] private Transform[] rats;

    [Tooltip("Optional: where each waiting (not-yet-active) rat should stand " +
             "before release, matched by index to 'rats'. Leave an entry empty " +
             "to just hide that rat until it's needed.")]
    [SerializeField] private Transform[] waitingSpots;

    [Header("Checkpoint")]
    [Tooltip("Fallback spawn point if no checkpoint has been reached yet.")]
    [SerializeField] private Transform startPoint;
    private Vector3 lastCheckpointPosition;
    private int lastCheckpointNumber;

    [Header("Integration")]
    [Tooltip("Tavish's camera/HUD adapter. Auto-found if left empty.")]
    [SerializeField] private TavishPrototypeIntegration integration;

    [Header("Respawn")]
    [Tooltip("Short invulnerability window after a rat becomes active so it " +
             "can't be re-killed by the same hazard on the same frame.")]
    [SerializeField] private float respawnInvulnerabilityTime = 1.0f;
    private bool isInvulnerable;

    private int activeIndex = -1;
    private int livesRemaining;

    public int LivesRemaining => livesRemaining;
    public int MaxLives => rats != null ? rats.Length : 0;

    /// <summary>Fired whenever a rat is lost. Passes lives remaining.</summary>
    public event Action<int> OnLifeLost;

    /// <summary>Fired after the next rat has been placed at the checkpoint.</summary>
    public event Action OnRatRespawned;

    private void Awake()
    {
        if (integration == null)
        {
            integration = FindFirstObjectByType<TavishPrototypeIntegration>();
        }
    }

    private void Start()
    {
        if (rats == null || rats.Length == 0)
        {
            Debug.LogError("[RatLifeManager] No rats assigned - the life system can't run.");
            return;
        }

        livesRemaining = rats.Length;
        lastCheckpointPosition = startPoint != null ? startPoint.position : rats[0].position;
        lastCheckpointNumber = 0;

        // Rat 0 starts at the configured start point when one is assigned;
        // every other rat starts waiting (visible at a waiting spot, or
        // hidden if no waiting spot was assigned).
        for (int i = 1; i < rats.Length; i++)
        {
            PlaceWaiting(i);
        }

        ActivateRatInternal(0, teleportToCheckpoint: startPoint != null);
        integration?.ActivateRat(0, livesRemaining, true);
    }

    /// <summary>
    /// Moves the active respawn point. Called by Checkpoint.cs when the
    /// active rat passes through a checkpoint trigger.
    /// </summary>
    public void SetCheckpoint(Vector3 position, int checkpointNumber)
    {
        lastCheckpointPosition = position;
        lastCheckpointNumber = checkpointNumber;
    }

    /// <summary>
    /// Called by HazardTrigger (or any obstacle) when the active rat dies.
    /// Hides the dead rat, activates the next one at the last checkpoint,
    /// and hands off to GameManager once all rats are lost.
    /// </summary>
    public void OnRatDied()
    {
        if (isInvulnerable) return;
        if (activeIndex < 0 || activeIndex >= rats.Length) return;

        DeactivateCurrentRat();

        livesRemaining--;
        OnLifeLost?.Invoke(livesRemaining);
        AudioManager.Instance?.PlayRatLost();

        if (livesRemaining <= 0)
        {
            GameManager.Instance?.LoseGame();
            return;
        }

        activeIndex++;
        ActivateRatInternal(activeIndex, teleportToCheckpoint: true);
        integration?.ActivateRat(activeIndex, livesRemaining, true);
        StartCoroutine(InvulnerabilityWindow());
        OnRatRespawned?.Invoke();
    }

    // -----------------------------------------------------------------
    // Rat state helpers
    // -----------------------------------------------------------------

    private void ActivateRatInternal(int index, bool teleportToCheckpoint)
    {
        activeIndex = index;
        Transform rat = rats[index];
        rat.gameObject.SetActive(true);

        if (teleportToCheckpoint)
        {
            TeleportRat(rat, lastCheckpointPosition);
        }

        var controller = rat.GetComponent<PlayerRatController>();
        if (controller != null)
        {
            controller.ResetMotion();
            controller.enabled = true;
        }
    }

    private void DeactivateCurrentRat()
    {
        if (activeIndex < 0 || activeIndex >= rats.Length) return;

        Transform rat = rats[activeIndex];
        var controller = rat.GetComponent<PlayerRatController>();
        if (controller != null) controller.enabled = false;

        // Removed from play entirely - it's been "used up" as a life.
        rat.gameObject.SetActive(false);
    }

    private void PlaceWaiting(int index)
    {
        Transform rat = rats[index];
        var controller = rat.GetComponent<PlayerRatController>();
        if (controller != null) controller.enabled = false;

        bool hasWaitingSpot = waitingSpots != null
            && index < waitingSpots.Length
            && waitingSpots[index] != null;

        if (hasWaitingSpot)
        {
            rat.gameObject.SetActive(true);
            TeleportRat(rat, waitingSpots[index].position);
        }
        else
        {
            // No waiting area built yet - keep the rat hidden until it's needed.
            rat.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Repositions a rat while briefly disabling its CharacterController,
    /// which avoids the controller trying to resolve a collision against
    /// its old position on the same frame as the teleport.
    /// </summary>
    private void TeleportRat(Transform rat, Vector3 position)
    {
        var cc = rat.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        rat.position = position;

        if (cc != null) cc.enabled = true;
    }

    private IEnumerator InvulnerabilityWindow()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(respawnInvulnerabilityTime);
        isInvulnerable = false;
    }
}

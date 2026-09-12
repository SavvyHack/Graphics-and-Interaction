using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Project R.A.T. three-rat life system.
///
/// The current prototype uses one controllable rat GameObject plus two visible
/// waiting-rat props. Losing a life releases the next rat conceptually by hiding
/// one waiting prop and respawning the controllable rat at the latest checkpoint.
/// This preserves the GDD's physical three-rat presentation without duplicating
/// controller/camera setup for three separate playable objects.
///
/// Kavish - game systems.
/// </summary>
public class RatLifeManager : MonoBehaviour
{
    [Header("Player and visible lives")]
    [SerializeField] private PlayerRatController player;
    [Tooltip("The two visible rats waiting to be released. Order: Rat 2, Rat 3.")]
    [SerializeField] private GameObject[] waitingRats;

    [Header("Respawn")]
    [SerializeField] private Transform startPoint;
    [SerializeField] private float respawnInvulnerabilityTime = 0.75f;
    [SerializeField] private float killY = -4f;

    [Header("Optional presentation references")]
    [SerializeField] private FixedCameraFollow cameraFollow;
    [SerializeField] private PrototypeHUD hud;

    private Vector3 lastCheckpointPosition;
    private int lastCheckpointNumber;
    private int livesRemaining = 3;
    private bool invulnerable;
    private bool finished;

    public int LivesRemaining => livesRemaining;
    public int MaxLives => 3;
    public int LastCheckpointNumber => lastCheckpointNumber;
    public bool Finished => finished;

    public event Action<int> OnLifeLost;
    public event Action<int> OnCheckpointChanged;
    public event Action OnRatRespawned;

    private void Awake()
    {
        if (player == null)
            player = FindFirstObjectByType<PlayerRatController>();

        if (cameraFollow == null)
            cameraFollow = FindFirstObjectByType<FixedCameraFollow>();

        if (hud == null)
            hud = FindFirstObjectByType<PrototypeHUD>();
    }

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError("[RatLifeManager] No PlayerRatController found.");
            enabled = false;
            return;
        }

        livesRemaining = 3;
        finished = false;
        lastCheckpointNumber = 0;
        lastCheckpointPosition = startPoint != null ? startPoint.position : player.transform.position;

        if (startPoint != null)
            player.Respawn(lastCheckpointPosition);

        SetWaitingRatsVisible(true);
        cameraFollow?.SetTarget(player.transform, true);
        hud?.SetRemainingRats(livesRemaining);
        hud?.SetCheckpoint(0);
    }

    private void Update()
    {
        if (!finished && !invulnerable && player != null && player.transform.position.y < killY)
            OnRatDied();
    }

    /// <summary>
    /// Stores the latest checkpoint. Checkpoints only move forwards so walking
    /// back through an earlier trigger cannot overwrite a later safe respawn.
    /// </summary>
    public void SetCheckpoint(Vector3 position, int checkpointNumber)
    {
        if (finished || checkpointNumber <= lastCheckpointNumber)
            return;

        lastCheckpointPosition = position;
        lastCheckpointNumber = checkpointNumber;
        hud?.SetCheckpoint(checkpointNumber);
        OnCheckpointChanged?.Invoke(checkpointNumber);
    }

    /// <summary>
    /// Called by any hazard. One life is consumed, then the same controllable
    /// rat object is respawned as the next rat at the latest checkpoint.
    /// </summary>
    public void OnRatDied()
    {
        if (finished || invulnerable || player == null)
            return;

        livesRemaining = Mathf.Max(0, livesRemaining - 1);
        OnLifeLost?.Invoke(livesRemaining);
        hud?.SetRemainingRats(livesRemaining);
        AudioManager.Instance?.PlayRatLost();

        // When lives drop from 3 -> 2, Rat 2 leaves the waiting area.
        // When lives drop from 2 -> 1, Rat 3 leaves the waiting area.
        int releasedWaitingRat = 2 - livesRemaining;
        if (waitingRats != null && releasedWaitingRat >= 0 && releasedWaitingRat < waitingRats.Length)
        {
            if (waitingRats[releasedWaitingRat] != null)
                waitingRats[releasedWaitingRat].SetActive(false);
        }

        if (livesRemaining <= 0)
        {
            finished = true;
            player.enabled = false;
            GameManager.Instance?.LoseGame();
            return;
        }

        player.Respawn(lastCheckpointPosition);
        cameraFollow?.SetTarget(player.transform, true);
        StartCoroutine(InvulnerabilityWindow());
        OnRatRespawned?.Invoke();
    }

    public void MarkFinished()
    {
        finished = true;
        if (player != null)
            player.enabled = false;
    }

    private IEnumerator InvulnerabilityWindow()
    {
        invulnerable = true;
        yield return new WaitForSeconds(respawnInvulnerabilityTime);
        invulnerable = false;
    }

    private void SetWaitingRatsVisible(bool visible)
    {
        if (waitingRats == null)
            return;

        foreach (GameObject rat in waitingRats)
        {
            if (rat != null)
                rat.SetActive(visible);
        }
    }
}

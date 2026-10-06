using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Central attempt-state controller for Project R.A.T.
/// Owns Playing/Won/Lost state and restart behaviour while RatLifeManager owns
/// individual life loss and checkpoint respawning.
///
/// Game Systems - game systems.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        Playing,
        Won,
        Lost,
        Paused
    }

    [SerializeField] private GameState currentState = GameState.Playing;
    [SerializeField] private RatLifeManager lifeManager;
    [SerializeField] private PrototypeHUD hud;

    [Header("Restart")]
    [Tooltip("If enabled, a failed attempt automatically restarts after this delay.")]
    [SerializeField] private bool autoRestartAfterFailure = false;
    [SerializeField] private float restartDelay = 2f;

    public GameState CurrentState => currentState;

    public event Action OnGameWon;
    public event Action OnGameLost;
    public event Action OnGameRestarted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        hasCampaignUI = FindFirstObjectByType<CampaignSession>() != null;
        currentState = GameState.Playing;
        Time.timeScale = 1f;

        if (lifeManager == null)
            lifeManager = FindFirstObjectByType<RatLifeManager>();

        if (hud == null)
            hud = FindFirstObjectByType<PrototypeHUD>();
    }

    private void Update()
    {
        // Keeps the original prototype's R-to-restart behaviour after the
        // RatTrialSession harness is disabled.
        if (!hasCampaignUI && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            RestartAttempt();
    }

    private bool winRequested;
    private bool hasCampaignUI;
    public void RequestWin()
    {
        if (currentState == GameState.Playing) winRequested = true;
    }
    private void LateUpdate()
    {
        // Accept all physics-frame deaths and pickups before taking the result snapshot.
        if (!winRequested) return;
        winRequested = false;
        if (currentState == GameState.Playing && lifeManager != null && lifeManager.LivesRemaining > 0) WinGame();
    }
    public void SetPaused(bool paused)
    {
        if (paused && currentState == GameState.Playing) currentState = GameState.Paused;
        else if (!paused && currentState == GameState.Paused) currentState = GameState.Playing;
        else return;
        lifeManager?.Player?.ResetMotion();
        Time.timeScale = paused ? 0f : 1f;
    }

    public void WinGame()
    {
        if (currentState != GameState.Playing)
            return;

        currentState = GameState.Won;
        Time.timeScale = 0f;
        lifeManager?.MarkFinished();
        hud?.ShowCompletion();
        AudioManager.Instance?.PlayCompletion();
        OnGameWon?.Invoke();

        Debug.Log("[GameManager] Enclosure cleared.");
    }

    public void LoseGame()
    {
        if (currentState != GameState.Playing)
            return;

        currentState = GameState.Lost;
        Time.timeScale = 0f;
        hud?.ShowFailure();
        AudioManager.Instance?.PlayFailure();
        OnGameLost?.Invoke();

        Debug.Log("[GameManager] All three rats lost.");

        if (autoRestartAfterFailure)
            StartCoroutine(RestartAfterDelay());
    }

    private System.Collections.IEnumerator RestartAfterDelay()
    {
        yield return new WaitForSecondsRealtime(restartDelay);
        RestartAttempt();
    }

    public void RestartAttempt()
    {
        Time.timeScale = 1f;
        OnGameRestarted?.Invoke();
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.path);
    }

    private void OnDestroy() { if (Instance == this) Instance = null; }
}

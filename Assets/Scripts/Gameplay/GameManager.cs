using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Central attempt-state controller for Project R.A.T.
/// Owns Playing/Won/Lost state and restart behaviour while RatLifeManager owns
/// individual life loss and checkpoint respawning.
///
/// Kavish - game systems.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        Playing,
        Won,
        Lost
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
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            RestartAttempt();
    }

    public void WinGame()
    {
        if (currentState != GameState.Playing)
            return;

        currentState = GameState.Won;
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
        hud?.ShowFailure();
        AudioManager.Instance?.PlayFailure();
        OnGameLost?.Invoke();

        Debug.Log("[GameManager] All three rats lost.");

        if (autoRestartAfterFailure)
            Invoke(nameof(RestartAttempt), restartDelay);
    }

    public void RestartAttempt()
    {
        OnGameRestarted?.Invoke();
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.path);
    }
}

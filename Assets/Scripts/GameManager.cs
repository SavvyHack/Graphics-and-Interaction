using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central project/game manager for Project R.A.T.
/// Owns overall attempt state (Playing / Won / Lost) and coordinates the
/// win/lose flow between the life system, UI and audio, per the GDD's
/// "reach the exit with at least one rat remaining" win condition and
/// "all rats lost -> restart from beginning" fail condition.
///
/// Calls into TavishPrototypeIntegration.OnGameCompleted() / OnGameFailed()
/// so the HUD panels react without this script knowing anything about UI.
///
/// Kavish - Game systems (primary responsibility).
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

    [Header("State")]
    [SerializeField] private GameState currentState = GameState.Playing;
    public GameState CurrentState => currentState;

    [Header("References")]
    [Tooltip("Assign the RatLifeManager in the scene. Auto-found if left empty.")]
    [SerializeField] private RatLifeManager lifeManager;

    [Tooltip("Assign Tavish's integration adapter so win/lose can drive the HUD panels.")]
    [SerializeField] private TavishPrototypeIntegration integration;

    [Header("Restart Behaviour")]
    [Tooltip("Delay (seconds) before reloading the scene after all rats are lost.")]
    [SerializeField] private float restartDelay = 1.5f;

    public event Action OnGameWon;
    public event Action OnGameLost;
    public event Action OnGameRestarted;

    private void Awake()
    {
        // One GameManager per attempt/scene.
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
        {
            lifeManager = FindFirstObjectByType<RatLifeManager>();
        }
        if (integration == null)
        {
            integration = FindFirstObjectByType<TavishPrototypeIntegration>();
        }
    }

    /// <summary>
    /// Called by ExitTrigger when a rat reaches the exit with at least one
    /// life remaining. Marks the attempt as successful.
    /// </summary>
    public void WinGame()
    {
        if (currentState != GameState.Playing) return;

        currentState = GameState.Won;
        OnGameWon?.Invoke();

        AudioManager.Instance?.PlayCompletion();
        integration?.OnGameCompleted();

        Debug.Log("[GameManager] Attempt successful - exit reached.");
    }

    /// <summary>
    /// Called by RatLifeManager once the third rat has died. Marks the
    /// attempt as failed, shows the failure panel and schedules a restart.
    /// </summary>
    public void LoseGame()
    {
        if (currentState != GameState.Playing) return;

        currentState = GameState.Lost;
        OnGameLost?.Invoke();

        integration?.OnGameFailed();

        Debug.Log("[GameManager] All rats lost - restarting attempt.");
        Invoke(nameof(RestartAttempt), restartDelay);
    }

    /// <summary>
    /// Reloads the current scene, restarting the attempt from the beginning
    /// of the enclosure, per the GDD's fail-state rule.
    /// </summary>
    public void RestartAttempt()
    {
        OnGameRestarted?.Invoke();
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.buildIndex);
    }
}

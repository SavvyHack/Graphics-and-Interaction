using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Scene-bound attempt journal and navigation; gameplay owners retain lives and state.</summary>
[DefaultExecutionOrder(50)]
public class CampaignSession : MonoBehaviour
{
    [SerializeField] private int levelIndex;
    [SerializeField] private GameManager game;
    [SerializeField] private RatLifeManager lives;
    [SerializeField] private CampaignUI ui;
    public int LevelIndex => levelIndex;
    public RatAttempt Result { get; private set; }
    private float saveTimer;
    private bool ready, navigating;
    private void Start()
    {
        CampaignProfile.Begin(levelIndex);
        game.OnGameWon += Won; game.OnGameLost += Lost; lives.OnLifeLost += LifeLost;
        foreach (RatCosmetic rat in FindObjectsByType<RatCosmetic>(FindObjectsSortMode.None)) rat.Apply(CampaignProfile.Equipped);
        ready = true;
    }
    private void Update()
    {
        if (!ready || navigating || game.CurrentState != GameManager.GameState.Playing || lives.Invulnerable) return;
        CampaignProfile.AddTime(Time.deltaTime);
        saveTimer += Time.deltaTime;
        if (saveTimer >= 10) { saveTimer = 0; CampaignProfile.Save(); }
    }
    private void LifeLost(int remaining) => CampaignProfile.Death();
    private void Won() { Result = CampaignProfile.Finish("won", lives.LivesRemaining); ui.ShowResult(); }
    private void Lost() { Result = CampaignProfile.Finish("lost", 0); ui.ShowResult(); }
    public void LoadLevel(int index)
    {
        if (navigating || index < 0 || index >= CampaignCatalog.Count || index >= CampaignProfile.Data.unlocked) return;
        Navigate(CampaignCatalog.Scenes[index]);
    }
    public void Home() => Navigate("Assets/Scenes/Home.unity");
    private void Navigate(string path)
    {
        if (navigating) return;
        if (!Application.CanStreamedLevelBeLoaded(path)) { ui.ShowNotice("This level is unavailable. Check the build scene list."); return; }
        navigating = true;
        if (CampaignProfile.Playing) CampaignProfile.Finish("abandoned", 0);
        Time.timeScale = 1; AudioListener.pause = false;
        SceneManager.LoadScene(path);
    }
    private void OnApplicationFocus(bool focused)
    {
        if (!focused && ready && game.CurrentState == GameManager.GameState.Playing) ui.Pause();
    }
    private void OnApplicationPause(bool paused) { if (paused) OnApplicationFocus(false); }
    private void OnApplicationQuit() { if (ready) CampaignProfile.Save(); }
    private void OnDestroy()
    {
        if (game != null) { game.OnGameWon -= Won; game.OnGameLost -= Lost; }
        if (lives != null) lives.OnLifeLost -= LifeLost;
        if (ready && !navigating && CampaignProfile.Playing) CampaignProfile.Finish("abandoned", 0);
    }
}

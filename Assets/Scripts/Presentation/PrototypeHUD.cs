using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Minimal Project R.A.T. HUD: remaining rats, checkpoint indicator,
/// completion state and failure state.
/// </summary>
public class PrototypeHUD : MonoBehaviour
{
    [Header("Rat Lives")]
    [SerializeField] private Image[] ratIcons;
    [SerializeField] private Color activeRatColour = Color.white;
    [SerializeField] private Color lostRatColour = new Color(1f, 1f, 1f, 0.25f);

    [Header("Checkpoint")]
    [SerializeField] private TMP_Text checkpointText;
    [SerializeField] private string checkpointPrefix = "Checkpoint ";

    [Header("Panels")]
    [SerializeField] private GameObject gameplayHUD;
    [SerializeField] private GameObject completionPanel;
    [SerializeField] private GameObject failurePanel;

    private void Awake()
    {
        ResetHUD();
    }

    /// <summary>
    /// Updates the three rat icons. For example, passing 2 keeps the first two
    /// active and greys out the final icon.
    /// </summary>
    public void SetRemainingRats(int remaining)
    {
        if (ratIcons == null)
            return;

        remaining = Mathf.Clamp(remaining, 0, ratIcons.Length);

        for (int i = 0; i < ratIcons.Length; i++)
        {
            if (ratIcons[i] != null)
                ratIcons[i].color = i < remaining ? activeRatColour : lostRatColour;
        }
    }

    public void SetCheckpoint(int checkpointNumber)
    {
        if (checkpointText != null)
            checkpointText.text = checkpointPrefix + checkpointNumber;
    }

    public void SetCheckpointLabel(string label)
    {
        if (checkpointText != null)
            checkpointText.text = label;
    }

    public void ShowCompletion()
    {
        if (gameplayHUD != null) gameplayHUD.SetActive(false);
        if (failurePanel != null) failurePanel.SetActive(false);
        if (completionPanel != null) completionPanel.SetActive(true);
    }

    public void ShowFailure()
    {
        if (gameplayHUD != null) gameplayHUD.SetActive(false);
        if (completionPanel != null) completionPanel.SetActive(false);
        if (failurePanel != null) failurePanel.SetActive(true);
    }

    public void ResetHUD()
    {
        if (gameplayHUD != null) gameplayHUD.SetActive(true);
        if (completionPanel != null) completionPanel.SetActive(false);
        if (failurePanel != null) failurePanel.SetActive(false);

        SetRemainingRats(ratIcons != null ? ratIcons.Length : 3);
        SetCheckpoint(1);
    }

    /// <summary>
    /// Can be wired directly to Restart buttons in the Inspector.
    /// </summary>
    public void RestartCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

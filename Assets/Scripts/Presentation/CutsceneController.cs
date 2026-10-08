using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Drives the intro cutscene: shows one panel at a time, advances on Right Arrow or a click on the
/// next arrow, and loads the main menu when finished or skipped. Layout is authored in the Start scene.
/// </summary>
public class CutsceneController : MonoBehaviour
{
    [Header("Content")]
    [SerializeField] private CutsceneData data;
    [SerializeField] private string nextSceneName = "Home";

    [Header("UI references")]
    [SerializeField] private Image panelImage;
    [SerializeField] private Image incomingImage;
    [SerializeField] private TMP_Text captionText;
    [SerializeField] private Button nextButton;
    [SerializeField] private RectTransform nextArrow;
    [SerializeField] private Button skipButton;

    [Header("Timing")]
    [SerializeField] private float slideDuration = 0.35f;
    [SerializeField] private float arrowBobHeight = 10f;
    [SerializeField] private float arrowBobSpeed = 3.5f;

    private static TMP_FontAsset menuFont;
    private int index = -1;
    private bool sliding;
    private bool loading;
    private Vector2 arrowRest;

    private void Awake()
    {
        ApplyMenuFont();
        if (nextButton != null) nextButton.onClick.AddListener(OnNextClicked);
        if (skipButton != null) skipButton.onClick.AddListener(LoadNextScene);
        if (nextArrow != null) arrowRest = nextArrow.anchoredPosition;
    }

    private void Start()
    {
        if (data == null || data.panels.Count == 0) { LoadNextScene(); return; }
        ShowPanel(0, false);
    }

    private void Update()
    {
        if (nextArrow != null)
            nextArrow.anchoredPosition = arrowRest + Vector2.up * (Mathf.Sin(Time.unscaledTime * arrowBobSpeed) * arrowBobHeight);

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.rightArrowKey.wasPressedThisFrame) Advance();
    }

    private void OnNextClicked()
    {
        // Keep the button from holding keyboard/gamepad focus, so Space/Enter never advance.
        if (UnityEngine.EventSystems.EventSystem.current != null)
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        Advance();
    }

    private void Advance()
    {
        if (sliding || loading) return;
        if (index + 1 >= data.panels.Count) LoadNextScene();
        else ShowPanel(index + 1, true);
    }

    private void ShowPanel(int next, bool animate)
    {
        index = next;
        CutsceneData.Panel panel = data.panels[index];
        captionText.text = panel.caption;
        if (animate && panel.transition == CutsceneTransition.SlideIn && incomingImage != null)
            StartCoroutine(SlideIn(panel.image));
        else
            panelImage.sprite = panel.image;
    }

    private IEnumerator SlideIn(Sprite sprite)
    {
        sliding = true;
        RectTransform incoming = incomingImage.rectTransform;
        float width = ((RectTransform)incoming.parent).rect.width;
        incomingImage.sprite = sprite;
        incoming.anchoredPosition = new Vector2(width, 0);
        incomingImage.gameObject.SetActive(true);
        for (float t = 0; t < slideDuration; t += Time.unscaledDeltaTime)
        {
            float eased = 1f - Mathf.Pow(1f - t / slideDuration, 3f); // ease-out cubic
            incoming.anchoredPosition = new Vector2(width * (1f - eased), 0);
            yield return null;
        }
        panelImage.sprite = sprite;
        incomingImage.gameObject.SetActive(false);
        sliding = false;
    }

    private void LoadNextScene()
    {
        if (loading) return;
        loading = true;
        SceneManager.LoadScene(nextSceneName);
    }

    // Same menu typography as CampaignUI: Bouncy Bun, with the scene font as a glyph fallback.
    private void ApplyMenuFont()
    {
        if (menuFont == null)
        {
            Font source = Resources.Load<Font>("Fonts/BouncybunDemo-V4K8y");
            if (source != null)
            {
                menuFont = TMP_FontAsset.CreateFontAsset(source, 90, 9,
                    UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 2048, 2048,
                    AtlasPopulationMode.Dynamic, true);
                if (menuFont != null)
                {
                    menuFont.name = "Bouncy Bun Cutscene SDF";
                    menuFont.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset>();
                }
            }
            else Debug.LogError("Bouncy Bun is missing from Resources/Fonts.", this);
        }
        if (menuFont == null) return;
        TMP_FontAsset sceneFont = captionText != null ? captionText.font : null;
        if (sceneFont != null && sceneFont != menuFont && !menuFont.fallbackFontAssetTable.Contains(sceneFont))
            menuFont.fallbackFontAssetTable.Add(sceneFont);
        foreach (TMP_Text text in GetComponentsInChildren<TMP_Text>(true)) text.font = menuFont;
    }
}

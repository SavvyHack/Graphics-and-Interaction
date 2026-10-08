using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Creates the intro cutscene data asset and the Start scene, and puts Start before Home in Build Settings.
/// Never overwrites an existing Start scene or edited captions; delete the asset first to rebuild.
/// </summary>
public static class CutsceneAuthoring
{
    const string ArtFolder = "Assets/Art/Cutscene";
    const string DataPath = ArtFolder + "/IntroCutsceneData.asset";
    const string ArrowPath = ArtFolder + "/UI/NextArrow.png";
    public const string ScenePath = "Assets/Scenes/Start.unity";
    const string HomePath = "Assets/Scenes/Home.unity";

    static readonly Color Ink = new Color(.035f, .06f, .1f, .88f);
    static readonly Color Panel = new Color(.075f, .13f, .19f, .92f);
    static readonly Color Border = new Color(.6f, .72f, .78f, .9f);
    static readonly Color Cyan = new Color(.36f, .94f, .91f);

    static readonly (string caption, CutsceneTransition transition)[] Beats =
    {
        ("The year is 2176. Cities rise above the clouds, and nearly every part of life is measured, tested and optimised.", CutsceneTransition.HardCut),
        ("Deep inside one of those cities, a research facility runs experiments day and night.", CutsceneTransition.SlideIn),
        ("In one of its rooms stands a glass enclosure, built to observe how living things solve problems.", CutsceneTransition.HardCut),
        ("Inside is a course of ramps, tubes, wheels and moving obstacles. None of it was built for comfort.", CutsceneTransition.SlideIn),
        ("Three test subjects. One cautious. One restless. One curious.", CutsceneTransition.HardCut),
        ("Someone is watching. A timer is running. When it reaches zero, the experiment is complete.", CutsceneTransition.HardCut),
        ("But somewhere beyond the last obstacle, there is a way out.", CutsceneTransition.SlideIn),
    };

    [MenuItem("Project R.A.T./Cutscene/Build Intro Cutscene")]
    public static void Build()
    {
        if (System.IO.File.Exists(ScenePath))
        {
            Debug.LogWarning("Start scene already exists; not overwriting. Delete " + ScenePath + " to rebuild.");
            return;
        }
        if (EditorApplication.isPlayingOrWillChangePlaymode || !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        ImportSprites();
        CutsceneData data = CreateData();
        BuildScene(data);
        SetBuildOrder();
        Debug.Log("Intro cutscene built: " + ScenePath);
    }

    static void ImportSprites()
    {
        foreach (string path in Directory.GetPaths())
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer == null) continue;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = 4096;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
        }
    }

    static class Directory
    {
        public static IEnumerable<string> GetPaths()
        {
            for (int i = 1; i <= Beats.Length; i++) yield return $"{ArtFolder}/Panel_{i:00}.png";
            yield return ArrowPath;
        }
    }

    static CutsceneData CreateData()
    {
        var existing = AssetDatabase.LoadAssetAtPath<CutsceneData>(DataPath);
        if (existing != null) return existing;
        var data = ScriptableObject.CreateInstance<CutsceneData>();
        for (int i = 0; i < Beats.Length; i++)
            data.panels.Add(new CutsceneData.Panel
            {
                image = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtFolder}/Panel_{i + 1:00}.png"),
                caption = Beats[i].caption,
                transition = Beats[i].transition,
            });
        AssetDatabase.CreateAsset(data, DataPath);
        AssetDatabase.SaveAssets();
        return data;
    }

    static void BuildScene(CutsceneData data)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        // Replacing the scene unloads unused objects, which can orphan the just-created asset instance.
        data = AssetDatabase.LoadAssetAtPath<CutsceneData>(DataPath);

        var cam = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cam.tag = "MainCamera";
        var camera = cam.GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        camera.orthographic = true;
        camera.cullingMask = 0;

        var events = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();

        var canvasGo = new GameObject("Cutscene Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CutsceneController));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        Transform root = canvasGo.transform;

        // Picture area: top 75%, clipped so slide-ins never overflow.
        RectTransform picture = Rect("Picture Area", root, new Vector2(0, .25f), Vector2.one);
        picture.gameObject.AddComponent<Image>().color = Color.black;
        picture.gameObject.AddComponent<RectMask2D>();
        Image panelImage = Picture("Panel Image", picture, data.panels[0].image);
        Image incoming = Picture("Incoming Image", picture, null);
        incoming.gameObject.SetActive(false);

        // Skip button, top-right, always visible.
        RectTransform skipRect = Rect("Skip Button", picture, Vector2.one, Vector2.one);
        skipRect.pivot = Vector2.one; skipRect.sizeDelta = new Vector2(190, 70); skipRect.anchoredPosition = new Vector2(-36, -30);
        var skipBg = skipRect.gameObject.AddComponent<Image>(); skipBg.color = Color.white;
        var skip = skipRect.gameObject.AddComponent<Button>();
        skip.targetGraphic = skipBg;
        ColorBlock colors = skip.colors;
        colors.normalColor = Panel; colors.highlightedColor = new Color(.24f, .43f, .54f);
        colors.selectedColor = colors.normalColor; colors.pressedColor = new Color(.12f, .29f, .37f); colors.fadeDuration = .08f;
        skip.colors = colors;
        skip.navigation = new Navigation { mode = Navigation.Mode.None };
        Edges(skipRect, 2, Border);
        Label("Label", skipRect, "Skip", 34, TextAlignmentOptions.Center, Cyan);

        // Text box: bottom 25%.
        RectTransform box = Rect("Text Box", root, Vector2.zero, new Vector2(1, .25f));
        box.gameObject.AddComponent<Image>().color = Ink;
        Edges(box, 3, Border);
        TMP_Text caption = Label("Caption", box, "", 44, TextAlignmentOptions.MidlineLeft, Color.white);
        caption.rectTransform.offsetMin = new Vector2(80, 30);
        caption.rectTransform.offsetMax = new Vector2(-190, -30);
        caption.enableAutoSizing = true; caption.fontSizeMin = 28; caption.fontSizeMax = 44;
        caption.textWrappingMode = TextWrappingModes.Normal;

        // Next arrow: transparent hit area with a bobbing child (bobbing is driven by the controller).
        RectTransform hit = Rect("Next Button", box, new Vector2(1, 0), new Vector2(1, 0));
        hit.pivot = new Vector2(1, 0); hit.sizeDelta = new Vector2(150, 150); hit.anchoredPosition = new Vector2(-30, 20);
        var hitImg = hit.gameObject.AddComponent<Image>(); hitImg.color = new Color(1, 1, 1, 0.003f);
        var next = hit.gameObject.AddComponent<Button>();
        next.transition = Selectable.Transition.None; next.targetGraphic = hitImg;
        next.navigation = new Navigation { mode = Navigation.Mode.None };
        RectTransform arrow = Rect("Next Arrow", hit, new Vector2(.5f, .5f), new Vector2(.5f, .5f));
        arrow.sizeDelta = new Vector2(72, 72);
        var arrowImg = arrow.gameObject.AddComponent<Image>();
        arrowImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ArrowPath); arrowImg.raycastTarget = false;

        var so = new SerializedObject(canvasGo.GetComponent<CutsceneController>());
        so.FindProperty("data").objectReferenceValue = data;
        so.FindProperty("nextSceneName").stringValue = "Home";
        so.FindProperty("panelImage").objectReferenceValue = panelImage;
        so.FindProperty("incomingImage").objectReferenceValue = incoming;
        so.FindProperty("captionText").objectReferenceValue = caption;
        so.FindProperty("nextButton").objectReferenceValue = next;
        so.FindProperty("nextArrow").objectReferenceValue = arrow;
        so.FindProperty("skipButton").objectReferenceValue = skip;
        so.ApplyModifiedPropertiesWithoutUndo();

        caption.text = data.panels[0].caption;
        EditorSceneManager.SaveScene(scene, ScenePath);
    }

    static void SetBuildOrder()
    {
        var scenes = EditorBuildSettings.scenes.Where(s => s.path != ScenePath).ToList();
        scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
        // Home must follow Start directly.
        int home = scenes.FindIndex(s => s.path == HomePath);
        if (home > 1) { var h = scenes[home]; scenes.RemoveAt(home); scenes.Insert(1, h); }
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    static RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max)
    {
        var rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = min; rt.anchorMax = max; rt.offsetMin = rt.offsetMax = Vector2.zero;
        return rt;
    }

    static Image Picture(string name, Transform parent, Sprite sprite)
    {
        var img = Rect(name, parent, Vector2.zero, Vector2.one).gameObject.AddComponent<Image>();
        img.sprite = sprite; img.preserveAspect = true; img.raycastTarget = false;
        return img;
    }

    static TMP_Text Label(string name, Transform parent, string text, float size, TextAlignmentOptions align, Color color)
    {
        var t = Rect(name, parent, Vector2.zero, Vector2.one).gameObject.AddComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = size; t.alignment = align; t.color = color; t.raycastTarget = false;
        if (TMP_Settings.defaultFontAsset != null) t.font = TMP_Settings.defaultFontAsset;
        return t;
    }

    // Thin light border drawn as four edge strips inside the rect.
    static void Edges(RectTransform parent, float thickness, Color color)
    {
        void Strip(string n, Vector2 min, Vector2 max, Vector2 size, Vector2 pivot)
        {
            var rt = Rect(n, parent, min, max); rt.pivot = pivot; rt.sizeDelta = size;
            var img = rt.gameObject.AddComponent<Image>(); img.color = color; img.raycastTarget = false;
        }
        Strip("Border Top", new Vector2(0, 1), Vector2.one, new Vector2(0, thickness), new Vector2(.5f, 1));
        Strip("Border Bottom", Vector2.zero, new Vector2(1, 0), new Vector2(0, thickness), new Vector2(.5f, 0));
        Strip("Border Left", Vector2.zero, new Vector2(0, 1), new Vector2(thickness, 0), new Vector2(0, .5f));
        Strip("Border Right", new Vector2(1, 0), Vector2.one, new Vector2(thickness, 0), new Vector2(1, .5f));
    }
}

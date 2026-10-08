using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Resolution-scaled laboratory UI, shared by Home and gameplay scenes.</summary>
[DefaultExecutionOrder(100)]
public class CampaignUI : MonoBehaviour
{
    [SerializeField] private TMP_FontAsset font;
    private static TMP_FontAsset defaultMenuFont;
    [SerializeField] private CampaignSession session;
    [SerializeField] private GameManager game;
    [SerializeField] private RatLifeManager lives;
    [SerializeField] private GameObject ratDisplayPrefab;
    private RectTransform canvas, menu, content, hud;
    private TMP_Text hudText, noticeText, promptText, augmentText;
    private RatPowerups powers;
    private readonly System.Text.StringBuilder augmentStatus = new System.Text.StringBuilder();
    private Action back;
    private string page;
    private bool navigating;
    private GameObject previewRoot;
    private PortalEndpoint[] portals;
    private LatchedSwitch[] switches;
    private ScrollRect pageScroll;
    private RenderTexture previewTexture;
    private readonly Color ink = new Color(.035f, .06f, .1f, .98f);
    private readonly Color panel = new Color(.075f, .13f, .19f);
    private readonly Color cyan = new Color(.36f, .94f, .91f);
    private readonly Color muted = new Color(.6f, .72f, .78f);
    private void Awake()
    {
        // Bouncy Bun is the shared menu default. Keep scene fonts as glyph fallbacks.
        if (defaultMenuFont == null)
        {
            Font source = Resources.Load<Font>("Fonts/BouncybunDemo-V4K8y");
            if (source != null)
            {
                defaultMenuFont = TMP_FontAsset.CreateFontAsset(source, 90, 9,
                    UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 2048, 2048,
                    AtlasPopulationMode.Dynamic, true);
                if (defaultMenuFont != null)
                {
                    defaultMenuFont.name = "Bouncy Bun Menu SDF";
                    defaultMenuFont.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset>();
                }
            }
            else Debug.LogError("Default menu font Bouncy Bun is missing from Resources/Fonts.", this);
        }
        if (defaultMenuFont != null)
        {
            if (font != null && font != defaultMenuFont && !defaultMenuFont.fallbackFontAssetTable.Contains(font))
                defaultMenuFont.fallbackFontAssetTable.Add(font);
            font = defaultMenuFont;
        }
        CampaignSettings.Apply();
        var go = new GameObject("Campaign canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        go.transform.SetParent(transform, false); canvas = (RectTransform)go.transform;
        go.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 800);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            var events = new GameObject("Menu input", typeof(EventSystem), typeof(InputSystemUIInputModule));
            events.transform.SetParent(transform, false);
        }
        hud = Rect("HUD", canvas); Stretch(hud);
        var statusPanel=Rect("Status backing",hud);Place(statusPanel,new Vector2(.015f,.9f),new Vector2(.79f,.995f));statusPanel.gameObject.AddComponent<Image>().color=new Color(.02f,.04f,.06f,.78f);
        augmentText = Label(hud, "", 20); Place(augmentText.rectTransform, new Vector2(.025f,.67f), new Vector2(.43f,.90f));
        hudText = Label(hud, "", 24); Place(hudText.rectTransform, new Vector2(.025f,.91f), new Vector2(.75f,.985f));
        promptText = Label(hud, "", 23); Place(promptText.rectTransform, new Vector2(.15f,.035f), new Vector2(.85f,.095f)); promptText.alignment = TextAlignmentOptions.Center;
        noticeText = Label(canvas, "", 18); Place(noticeText.rectTransform, new Vector2(.05f,0), new Vector2(.95f,.04f)); noticeText.color = new Color(1,.72f,.38f); noticeText.alignment = TextAlignmentOptions.Center;
    }
    private void Start()
    {
        powers = FindFirstObjectByType<RatPowerups>();
        portals = FindObjectsByType<PortalEndpoint>(FindObjectsSortMode.None);
        switches = FindObjectsByType<LatchedSwitch>(FindObjectsSortMode.None);
        if (session == null) ShowHome(); else ShowHud();
    }
    private void Update()
    {
        noticeText.text = CampaignProfile.Notice ?? "";
        if (session != null && page == "HUD")
        {
            RatAttempt attempt = CampaignProfile.Data.active;
            hudText.text = $"{CampaignCatalog.Names[session.LevelIndex]}   /   RATS {lives.LivesRemaining}/3\nCheckpoint {lives.LastCheckpointNumber}   •   This attempt: {attempt?.coins ?? 0}   •   Wallet: {CampaignProfile.Wallet}";
            string prompt = "A/D move   •   Space jump   •   Shift sprint   •   Esc pause";
            foreach (PortalEndpoint portal in portals) if (!string.IsNullOrEmpty(portal.Prompt)) prompt = portal.Prompt;
            foreach (LatchedSwitch latch in switches) if (!string.IsNullOrEmpty(latch.Prompt)) prompt = latch.Prompt;
            promptText.text = prompt;
            augmentStatus.Clear();
            if (powers != null)
            {
                for (int i=0;i<RatPowerups.Names.Length;i++)
                    if (powers.Has((RatAugment)i)) augmentStatus.Append(RatPowerups.Names[i]).Append("  ").Append(Mathf.CeilToInt(powers.Remaining((RatAugment)i))).Append("s\n");
                if (powers.Has(RatAugment.Jetpack)) augmentStatus.Append("Fuel ").Append(Mathf.CeilToInt(100*powers.JetFuel/RatPowerups.MaxJetFuel)).Append("%  / hold Space\n");
                if (powers.Has(RatAugment.GravityPulse)) augmentStatus.Append("E / open a nearby magnetic gate\n");
                if (powers.Has(RatAugment.Dash)) augmentStatus.Append("Q / dash (once per jump)\n");
                if (powers.Has(RatAugment.WallJump)) augmentStatus.Append("Hold towards a wall, Space to kick off\n");
                if (powers.Has(RatAugment.Glide)) augmentStatus.Append("Hold Space while falling / ride updrafts\n");
                if (powers.Has(RatAugment.Phase)) augmentStatus.Append("Pass through violet fields and machinery\n");
                if (powers.Has(RatAugment.GroundPound)) augmentStatus.Append("S in mid-air / smash cracked hatches\n");
            }
            augmentText.text = augmentStatus.ToString();
        }
        if (Keyboard.current == null) return;
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (page == "HUD") Pause(); else back?.Invoke();
        }
        if (page == "HUD" && Keyboard.current.rKey.wasPressedThisFrame) { Pause(); Confirm("Restart this level?", "The current attempt will be recorded as abandoned. Collected coins stay saved.", () => session.LoadLevel(session.LevelIndex), ShowPause); }
    }
    private void ClearPreview()
    {
        if (previewRoot != null) Destroy(previewRoot);
        if (previewTexture != null) { previewTexture.Release(); Destroy(previewTexture); }
    }
    private void Page(string title, string subtitle, Action returnTo)
    {
        EventSystem.current.SetSelectedGameObject(null);
        ClearPreview();
        if (menu != null) { menu.gameObject.SetActive(false); Destroy(menu.gameObject); }
        hud.gameObject.SetActive(false); page = title; back = returnTo;
        menu = Rect("Page " + title, canvas); Stretch(menu); menu.gameObject.AddComponent<Image>().color = ink;
        var stripe = Rect("Header line", menu); Place(stripe, new Vector2(.055f,.865f), new Vector2(.945f,.87f)); stripe.gameObject.AddComponent<Image>().color = cyan;
        var heading = Label(menu, title.ToUpperInvariant(), 42); heading.name = "Page heading"; Place(heading.rectTransform, new Vector2(.06f,.89f), new Vector2(.82f,.97f));
        var sub = Label(menu, subtitle, 21); sub.color = muted; Place(sub.rectTransform, new Vector2(.06f,.795f), new Vector2(.94f,.86f));
        var viewport = Rect("Scroll viewport", menu); Place(viewport, new Vector2(.06f,.075f), new Vector2(.94f,.78f));
        viewport.gameObject.AddComponent<RectMask2D>();
        var scroll = viewport.gameObject.AddComponent<ScrollRect>(); scroll.horizontal = false; scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 36;
        pageScroll = scroll;
        viewport.gameObject.AddComponent<Image>().color = new Color(0,0,0,.001f);
        content = Rect("Content", viewport); content.anchorMin = new Vector2(0,1); content.anchorMax = Vector2.one; content.pivot = new Vector2(.5f,1); content.sizeDelta = Vector2.zero;
        var layout = content.gameObject.AddComponent<VerticalLayoutGroup>(); layout.spacing = 12; layout.padding = new RectOffset(6,6,6,10); layout.childControlHeight = true; layout.childForceExpandHeight = false; layout.childForceExpandWidth = true;
        content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.content = content; scroll.viewport = viewport;
        if (returnTo != null)
        {
            Button button = MakeButton(menu, "BACK / ESC", returnTo);
            Place((RectTransform)button.transform, new Vector2(.82f,.90f), new Vector2(.94f,.96f));
            var backLabel=button.GetComponentInChildren<TMP_Text>();backLabel.fontSize=18;backLabel.fontSizeMax=18;backLabel.fontSizeMin=14;backLabel.textWrappingMode=TextWrappingModes.NoWrap;
        }
        noticeText.transform.SetAsLastSibling();
    }
    private void Copy(string text, int size = 24, float height = 80)
    {
        TMP_Text label = Label(content, text, size);
        // Paragraphs grow with the font's measured height instead of clipping ascenders.
        label.enableAutoSizing = false;
        label.alignment = TextAlignmentOptions.TopLeft;
        var layout = label.gameObject.AddComponent<LayoutElement>();
        layout.minHeight = height;
        layout.preferredHeight = -1;
    }
    private Button Button(string text, Action click, bool enabled = true)
    {
        Button button = MakeButton(content, text, click); button.interactable = enabled;
        button.gameObject.AddComponent<LayoutElement>().preferredHeight = 54;
        if (enabled && EventSystem.current.currentSelectedGameObject == null) EventSystem.current.SetSelectedGameObject(button.gameObject);
        return button;
    }
    private Button MakeButton(Transform parent, string text, Action click)
    {
        RectTransform rect = Rect(text, parent);
        // Selectable.OnEnable applies its initial tint instantly. Configure while
        // inactive so rebuilt pages never fade from the default white tint.
        rect.gameObject.SetActive(false);
        var background = rect.gameObject.AddComponent<Image>();
        background.color = Color.white;
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = background;
        button.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = button.colors; colors.normalColor = panel; colors.highlightedColor = new Color(.24f,.43f,.54f); colors.selectedColor = colors.highlightedColor; colors.pressedColor = new Color(.12f,.29f,.37f); colors.disabledColor = new Color(.08f,.1f,.12f); colors.fadeDuration = .08f; button.colors = colors;
        // Selection persists after a click; Settings reserves persistent aqua for mute ON.
        if (page == "Settings")
        {
            colors.selectedColor = colors.normalColor;
            button.colors = colors;
        }
        TMP_Text label = Label(rect, text, 22); Stretch(label.rectTransform); label.rectTransform.offsetMin = new Vector2(18,6); label.rectTransform.offsetMax = new Vector2(-18,-6); label.alignment = TextAlignmentOptions.MidlineLeft;
        button.onClick.AddListener(() => { AudioManager.Instance?.PlayCheckpoint(); click(); });
        rect.gameObject.SetActive(true);
        return button;
    }
    private TMP_Text Label(Transform parent, string text, int size)
    {
        RectTransform rect = Rect("Text", parent); var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.font = font; label.fontSize = size; label.text = text; label.color = Color.white;
        label.enableAutoSizing = true;
        label.fontSizeMin = Mathf.Max(14, size * .75f);
        label.fontSizeMax = size;
        label.alignment = TextAlignmentOptions.MidlineLeft;
        label.margin = new Vector4(0, 3, 0, 3);
        label.lineSpacing = 4;
        label.textWrappingMode = TextWrappingModes.Normal; label.raycastTarget = false;
        return label;
    }
    private static RectTransform Rect(string name, Transform parent) { var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); r.SetParent(parent,false); return r; }
    private static void Stretch(RectTransform r) => Place(r, Vector2.zero, Vector2.one);
    private static void Place(RectTransform r, Vector2 min, Vector2 max) { r.anchorMin=min; r.anchorMax=max; r.offsetMin=r.offsetMax=Vector2.zero; }
    private void Focus(Button button) { EventSystem.current.SetSelectedGameObject(button.gameObject); }
    public void ShowNotice(string text) { Copy(text, 22, 70); }
    private void ShowHud()
    {
        ClearPreview(); if (menu != null) { menu.gameObject.SetActive(false); Destroy(menu.gameObject); }
        page = "HUD"; back = null; hud.gameObject.SetActive(true); EventSystem.current.SetSelectedGameObject(null);
    }
    public void Pause() { if (game == null || game.CurrentState != GameManager.GameState.Playing) return; game.SetPaused(true); CampaignProfile.Save(); ShowPause(); }
    private void ShowPause()
    {
        Page("Paused", "Take your time. The laboratory is on hold.", Resume);
        Focus(Button("Resume", Resume));
        Button("Settings", () => ShowSettings(ShowPause)); Button("Help", () => ShowHelp(ShowPause,0));
        Button("Reset current puzzle", () => { lives.ResetPuzzle(); Resume(); });
        Button("Restart level", () => Confirm("Restart this level?", "The current attempt ends. You keep collected coins and outfits.", () => session.LoadLevel(session.LevelIndex), ShowPause));
        Button("Home", () => Confirm("Leave the level?", "Continue restarts this level with the rats that entered it. Your wallet stays saved.", session.Home, ShowPause));
    }
    private void Resume() { game.SetPaused(false); ShowHud(); }
    private void Confirm(string title, string message, Action accept, Action cancel)
    {
        Page(title, message, cancel);
        Focus(Button("Cancel", cancel)); Button("Confirm", accept);
    }
    private void Load(int index)
    {
        if (session != null) { session.LoadLevel(index); return; }
        if (navigating || index >= CampaignProfile.Data.unlocked || index < 0) return;
        if (!Application.CanStreamedLevelBeLoaded(CampaignCatalog.Scenes[index])) { ShowNotice("Level unavailable in this build."); return; }
        navigating = true; Time.timeScale = 1; SceneManager.LoadScene(CampaignCatalog.Scenes[index]);
    }
    private void ShowHome()
    {
        Page("Project R.A.T.", "LABORATORY ESCAPE   /   Seven enclosures. Three rats. One way out.", null);
        StyleHome();
        RectTransform hero = Rect("Escape crew exhibit", content);
        hero.gameObject.AddComponent<LayoutElement>().preferredHeight = 190;
        hero.gameObject.AddComponent<Image>().color = new Color(.09f,.18f,.24f);
        HomeBlock(hero, "Exhibit accent", new Vector2(0,0), new Vector2(.008f,1), cyan);
        var crew = Label(hero, "MEET THE ESCAPE COMMITTEE", 18);
        crew.color = cyan;
        Place(crew.rectTransform, new Vector2(.035f,.77f), new Vector2(.7f,.98f));
        var tag = Label(hero, "SMALL PAWS.\nBIG PLANS.", 30);
        tag.fontStyle = FontStyles.Bold;
        Place(tag.rectTransform, new Vector2(.035f,.18f), new Vector2(.36f,.76f));
        RectTransform parent = content;
        content = hero;
        Preview(3,CampaignProfile.Equipped,150);
        if (ratDisplayPrefab != null)
            Place((RectTransform)hero.GetChild(hero.childCount-1), new Vector2(.36f,.08f), new Vector2(.97f,.86f));
        content = parent;
        Row(()=> {
            var start = Button("New game", () => { if (CampaignProfile.Data.hasCampaign) Confirm("Start a new campaign?", "Level unlocks reset. Coins, outfits, statistics and settings are kept.", () => { CampaignProfile.NewCampaign(); Load(0); }, ShowHome); else { CampaignProfile.NewCampaign(); Load(0); } });
            ColorBlock startColors = start.colors;
            startColors.normalColor = new Color(1f,.73f,.32f);
            startColors.highlightedColor = new Color(1f,.93f,.65f);
            startColors.selectedColor = startColors.highlightedColor;
            startColors.pressedColor = new Color(.85f,.52f,.16f);
            start.colors = startColors;
            start.GetComponentInChildren<TMP_Text>().color = ink;
            start.GetComponentInChildren<TMP_Text>().fontStyle = FontStyles.Bold;
            Button(CampaignProfile.Data.campaignComplete ? "Replay final level" : "Continue level from start", () => Load(CampaignProfile.Data.campaignComplete ? CampaignCatalog.Count-1 : CampaignProfile.Data.selected), CampaignProfile.Data.hasCampaign);
        });
        Row(()=> { Button("Laboratory map", ShowSelect); Button("Rat wardrobe", () => ShowWardrobe(false)); });
        Row(()=> { Button("Shop", () => ShowWardrobe(true)); Button("Statistics", ShowStats); });
        Row(()=> { Button("Settings", () => ShowSettings(ShowHome)); Button("Help", () => ShowHelp(ShowHome,0)); });
        Row(()=> {
            Button("Replay intro", () => {
                if (navigating) return;
                const string introScene = "Assets/Scenes/Start.unity";
                if (!Application.CanStreamedLevelBeLoaded(introScene))
                { ShowNotice("The intro is unavailable in this build."); return; }
                navigating = true;
                Time.timeScale = 1;
                SceneManager.LoadScene(introScene);
            });
            if (!Application.isEditor && Application.platform != RuntimePlatform.WebGLPlayer) Button("Quit", Application.Quit);
        });
    }
    private void StyleHome()
    {
        menu.GetComponent<Image>().color = new Color(.035f,.055f,.105f);
        RectTransform decoration = Rect("Laboratory confetti", menu);
        Stretch(decoration);
        decoration.SetAsFirstSibling();
        // Decorative graphics never intercept pointer input or participate in navigation.
        for (int i=0;i<18;i++)
        {
            float x = (i % 2 == 0) ? .018f : .966f;
            float y = .06f + (i/2)*.105f;
            RectTransform spark = HomeBlock(decoration, "Floating specimen", new Vector2(x,y), new Vector2(x+.014f,y+.022f),
                i%3==0 ? new Color(1,.73f,.32f,.55f) : new Color(.36f,.94f,.91f,.3f));
            spark.localRotation = Quaternion.Euler(0,0,i%2==0 ? 25 : -20);
        }
        TMP_Text title = menu.Find("Page heading").GetComponent<TMP_Text>();
        if (title != null)
        {
            title.text = "PROJECT <color=#FFD080>R.A.T.</color>";
            title.fontSize = 48;
            title.fontSizeMax = 48;
            title.fontSizeMin = 32;
            title.fontStyle = FontStyles.Bold;
            Place(title.rectTransform,new Vector2(.06f,.885f),new Vector2(.71f,.98f));
        }
    }
    private RectTransform HomeBlock(Transform parent, string name, Vector2 min, Vector2 max, Color color)
    {
        RectTransform rect = Rect(name,parent);
        Place(rect,min,max);
        var image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return rect;
    }
    private void Row(Action build)
    {
        RectTransform parent = content;
        RectTransform row = Rect("Menu row", parent); row.gameObject.AddComponent<LayoutElement>().preferredHeight=54;
        var layout=row.gameObject.AddComponent<HorizontalLayoutGroup>();layout.spacing=14;layout.childControlWidth=true;layout.childForceExpandWidth=true;
        content=row;build();content=parent;
    }
    private void ShowSelect()
    {
        Page("Laboratory map", "Follow the transfer network. Clear a chamber to unlock the next.", session == null ? (Action)ShowHome : ShowResult);
        RectTransform map = Rect("Circuit map", content); map.gameObject.AddComponent<LayoutElement>().preferredHeight = 420;
        Vector2[] nodes = { new Vector2(.12f,.77f), new Vector2(.37f,.77f), new Vector2(.62f,.77f), new Vector2(.87f,.77f), new Vector2(.87f,.24f), new Vector2(.50f,.24f), new Vector2(.13f,.24f) };
        for (int i=1; i<nodes.Length; i++)
        {
            RectTransform line = Rect("Transfer conduit", map); line.gameObject.AddComponent<Image>().color = i < CampaignProfile.Data.unlocked ? cyan : panel;
            Vector2 a=nodes[i-1], b=nodes[i];
            Place(line, Vector2.Min(a,b), Vector2.Max(a,b));
            if (Mathf.Approximately(a.y,b.y)) line.sizeDelta = new Vector2(0,4); else line.sizeDelta = new Vector2(4,0);
        }
        for (int i=0; i<CampaignCatalog.Count; i++)
        {
            int index=i; bool unlocked=i<CampaignProfile.Data.unlocked;
            RatLevelRecord r=CampaignProfile.Record(CampaignCatalog.Ids[i]);
            bool cleared=i<CampaignProfile.Data.unlocked-1 || (i==CampaignCatalog.Count-1 && CampaignProfile.Data.campaignComplete);
            string state=!unlocked ? "LOCKED" : cleared ? "CLEARED" : "READY";
            var button=MakeButton(map, $"{i+1:00}  {state}\n{CampaignCatalog.Names[i]}\n{CampaignProfile.Collected(i)}/20 unique coins", () => Load(index));
            button.interactable=unlocked;
            var rect=(RectTransform)button.transform; Place(rect,nodes[i]-new Vector2(.112f,.16f),nodes[i]+new Vector2(.112f,.16f));
            var mapLabel = button.GetComponentInChildren<TMP_Text>();
            mapLabel.fontSize=18; mapLabel.fontSizeMax=18; mapLabel.fontSizeMin=14;
            if(i==CampaignProfile.Data.selected) Focus(button);
        }
        Copy("Surviving rats carry into the next level. Retry restores the rats that entered that level. Coins return on a new attempt; outfits never protect a rat.",22,65);
    }
    private int previewOutfit;
    private void ShowWardrobe(bool shop)
    {
        Page(shop ? "Token exchange" : "Rat wardrobe", $"WALLET  {CampaignProfile.Wallet} tokens   /   Outfits change appearance only.",ShowHome);
        if (previewOutfit < 0 || previewOutfit >= CampaignCatalog.OutfitIds.Length ||
            (!shop && !CampaignProfile.Data.owned.Contains(CampaignCatalog.OutfitIds[previewOutfit])))
            previewOutfit = CampaignProfile.Equipped;

        // The preview belongs to the page, outside the scrolling outfit list.
        Place(pageScroll.viewport, new Vector2(.06f,.075f), new Vector2(.94f,.48f));
        Preview(1,previewOutfit);
        if (ratDisplayPrefab != null)
        {
            RectTransform exhibit = (RectTransform)content.GetChild(content.childCount-1);
            exhibit.SetParent(menu,false);
            Place(exhibit,new Vector2(.06f,.55f),new Vector2(.94f,.78f));
        }
        TMP_Text previewName = Label(menu,"PREVIEW / " + CampaignCatalog.OutfitNames[previewOutfit],20);
        Place(previewName.rectTransform,new Vector2(.06f,.49f),new Vector2(.94f,.54f));
        previewName.color = cyan;
        for(int i=0;i<CampaignCatalog.OutfitIds.Length;i++)
        {
            int index=i;
            bool owned=CampaignProfile.Data.owned.Contains(CampaignCatalog.OutfitIds[i]);
            if (!shop && !owned) continue;
            bool equipped=CampaignProfile.Equipped==i;
            int cost=CampaignCatalog.Prices[i];
            string status=equipped ? "EQUIPPED" : owned ? "OWNED" : cost+" tokens";
            RectTransform row = Rect("Outfit row / " + CampaignCatalog.OutfitNames[i],content);
            row.gameObject.AddComponent<LayoutElement>().preferredHeight=72;
            Button preview = MakeButton(row,CampaignCatalog.OutfitNames[i]+"   /   "+status,()=>{
                previewOutfit=index;
                previewName.text="PREVIEW / " + CampaignCatalog.OutfitNames[index];
                if(previewRoot != null)
                    foreach(RatCosmetic cosmetic in previewRoot.GetComponentsInChildren<RatCosmetic>()) cosmetic.Apply(index);
            });
            Place((RectTransform)preview.transform,Vector2.zero,new Vector2(.69f,1));
            if(EventSystem.current.currentSelectedGameObject==null) Focus(preview);
            Action reopen=()=>{
                ShowWardrobe(shop);
                Canvas.ForceUpdateCanvases();
                pageScroll.verticalNormalizedPosition=outfitScrollPosition;
                previousSelection=EventSystem.current.currentSelectedGameObject;
            };
            string actionLabel=shop ? (equipped ? "Equipped" : owned ? "Owned" : CampaignProfile.Wallet<cost ? "Need "+(cost-CampaignProfile.Wallet) : "Buy / "+cost) : equipped ? "Equipped" : "Equip";
            Button action=MakeButton(row,actionLabel,()=>{
                outfitScrollPosition=pageScroll.verticalNormalizedPosition;
                previewOutfit=index;
                if(!shop) { CampaignProfile.Equip(index); reopen(); return; }
                Confirm("Purchase outfit?",$"{CampaignCatalog.OutfitNames[index]} costs {cost} tokens. Buying does not automatically equip it.",()=>{CampaignProfile.Buy(index);reopen();},reopen);
            });
            action.interactable=shop ? !owned && CampaignProfile.Wallet>=cost : !equipped;
            Place((RectTransform)action.transform,new Vector2(.71f,0),Vector2.one);
            action.GetComponentInChildren<TMP_Text>().alignment=TextAlignmentOptions.Midline;
        }
    }
    private float outfitScrollPosition=1;
    private void Preview(int count,int outfit,float height=220,bool escaped=false)
    {
        if(ratDisplayPrefab==null) return;
        RectTransform area=Rect("Rat preview",content); area.gameObject.AddComponent<LayoutElement>().preferredHeight=height;
        area.gameObject.AddComponent<Image>().color=panel;
        RectTransform picture=Rect("Proportional preview",area);
        var fit=picture.gameObject.AddComponent<AspectRatioFitter>();fit.aspectMode=AspectRatioFitter.AspectMode.FitInParent;fit.aspectRatio=3.2f;
        var image=picture.gameObject.AddComponent<RawImage>(); image.raycastTarget=false;
        previewTexture=new RenderTexture(1024,320,24){antiAliasing=4}; previewTexture.Create(); image.texture=previewTexture;
        previewRoot=new GameObject("Rat display studio"); previewRoot.transform.position=new Vector3(10000,100,0);
        var cameraObject=new GameObject("Preview camera",typeof(Camera)); cameraObject.transform.SetParent(previewRoot.transform,false);
        var camera=cameraObject.GetComponent<Camera>(); camera.transform.localPosition=new Vector3(0,.35f,-6); camera.orthographic=true;camera.orthographicSize=1.4f;
        camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=panel;camera.targetTexture=previewTexture;camera.cullingMask=1<<31;camera.farClipPlane=20;
        if(escaped)
        {
            camera.backgroundColor=count==3?new Color(.055f,.22f,.25f):count==2?new Color(.22f,.16f,.11f):new Color(.07f,.12f,.20f);
            var moon=GameObject.CreatePrimitive(PrimitiveType.Sphere);Destroy(moon.GetComponent<Collider>());moon.transform.SetParent(previewRoot.transform,false);moon.transform.localPosition=new Vector3(3,1.35f,3);moon.transform.localScale=Vector3.one*.65f;moon.layer=31;
            var ground=GameObject.CreatePrimitive(PrimitiveType.Cube);Destroy(ground.GetComponent<Collider>());ground.transform.SetParent(previewRoot.transform,false);ground.transform.localPosition=new Vector3(0,-.77f,1);ground.transform.localScale=new Vector3(15,.2f,2);ground.layer=31;
        }
        for(int i=0;i<count;i++)
        {
            GameObject rat=Instantiate(ratDisplayPrefab,previewRoot.transform);
            rat.transform.localPosition=new Vector3((i-(count-1)*.5f)*1.8f,.05f,0);rat.transform.localRotation=Quaternion.Euler(0,65,0);rat.transform.localScale=Vector3.one*1.8f;
            foreach(Transform t in rat.GetComponentsInChildren<Transform>(true)) t.gameObject.layer=31;
            foreach(RatCosmetic cosmetic in rat.GetComponentsInChildren<RatCosmetic>()){cosmetic.Apply(outfit);cosmetic.enabled=true;}
        }
    }
    private void ShowStats()
    {
        Page("Statistics","A run means one level attempt. Pauses, menus and loading do not count as active time.",ShowHome);
        int attempts=0,wins=0,losses=0,abandons=0,deaths=0; double seconds=0;
        foreach(RatLevelRecord r in CampaignProfile.Data.levels){attempts+=r.attempts;wins+=r.wins;losses+=r.failures;abandons+=r.abandoned;deaths+=r.deaths;seconds+=r.seconds;}
        Copy($"{attempts} attempts   /   {wins} cleared   /   {losses} failed   /   {abandons} abandoned\n{deaths} rats lost   /   {CampaignProfile.TimeText(seconds)} active time   /   {CampaignProfile.Data.campaignClears} campaign clears",24,90);
        Copy($"Coins earned: {CampaignProfile.Data.earned}   •   Unique found: {CampaignProfile.Data.coins.Count}/140   •   Spent: {CampaignProfile.Data.spent}   •   Wallet: {CampaignProfile.Wallet}",22,45);
        for(int i=0;i<CampaignCatalog.Count;i++)
        {
            RatLevelRecord r=CampaignProfile.Record(CampaignCatalog.Ids[i]);
            string best=r.wins==0 ? "—" : CampaignProfile.TimeText(r.bestSeconds);
            string rats=r.wins==0 ? "—" : r.bestRats+"/3";
            Copy($"{i+1:00}  {CampaignCatalog.Names[i]}    |    Tokens {CampaignProfile.Collected(i)}/20\nAttempts {r.attempts} • Wins {r.wins} • Failures {r.failures} • Abandoned {r.abandoned} • Deaths {r.deaths}\nTime {CampaignProfile.TimeText(r.seconds)} • Best clear {best} • Best survivors {rats}",21,105);
        }
    }
    private void ShowHelp(Action returnTo,int tab)
    {
        Page("Field manual","Everything you need to leave the laboratory.",returnTo);
        Row(()=> { Button("Controls",()=>ShowHelp(returnTo,0)); Button("Power-ups",()=>ShowHelp(returnTo,3)); });
        Row(()=> { Button("Field guide",()=>ShowHelp(returnTo,1)); Button("Our story",()=>ShowHelp(returnTo,2)); });
        if(tab==0)
        {
            Copy("A / D or arrow keys — move\nSpace — jump (hold to fly with jetpack or glide)\nShift — sprint\nE — magnetic pulse\nQ — dash (with DASH)\nS / Down — ground pound (with GROUND POUND)\nEscape — pause / go back\nR — confirm a level restart",26,300);
            Copy("Checkpoints save a safe place for the next rat, but never restore lost lives. Pause → Reset current puzzle restores the room without spending a rat.",24,110);
        }
        else if(tab==1)
        {
            Copy("THREE RATS\nOne rat moves at a time. A hazard costs one life. The next rat returns to the latest checkpoint; running out ends the attempt. Survivors carry forward; retries restore only the rats that entered this level.",23,105);
            Copy("HAZARDS\nAvoid water and rotating sweep arms. Ride shuttles from a safe deck. For the new energy gates: wait while the column is visible; the amber beacon warns before it becomes dangerous. Cross when the column is absent.",23,125);
            Copy("TOKENS & OUTFITS\nGold tokens reward optional detours. Each can be collected once per attempt, then returns on retry or replay. Spend them in the shop, then equip a design in the wardrobe. Outfits never provide immunity or change movement.",23,125);
            Copy("AUGMENTATION LAB / LEVEL 1\nClimb the five-tier enclosure. Sprint over the first gap; take SPEED for the long jump, DOUBLE JUMP for the right ledge, then SHIELD for the gallery laser. Hold Space with JETPACK to climb the left shaft. Use E with GRAVITY PULSE at the magnetic wall. Slow the wheel and ride the elevator towards the upper exit.",23,190);
            Copy("REACTOR DIVIDE / LEVEL 2\nDouble-jump the first coolant gap and sprint with SPEED across the second. Cross the shielded laser and pulse the gate on the return gallery. Jet up the left shaft, ride the two shuttles, then jet up the right shaft. Double-jump the final upper gap and take SHIELD before the exit laser.",23,185);
            Copy("RELAY ARCHIVE / LEVEL 3  -  NEW: DASH\nPress Q in mid-air to burst forwards, once per jump. Practise over the safe trench, then dash the coolant, the wheel and the gaps. Take SPEED before the long floor 3 gap: speed plus dash clears it.",23,150);
            Copy("COOLANT FOUNDRY / LEVEL 4  -  NEW: WALL JUMP\nWalk under a chimney's inner wall and jump at the far wall. Hold towards a wall and press Space to kick back and forth until you clear the top. Slow Time makes the coolant shuttles easier to board.",23,150);
            Copy("SCANNER GALLERY / LEVEL 5  -  NEW: GLIDE\nHold Space while falling to drift slowly. Gliding into a pale updraft column lifts you; at the top, glide sideways onto the next floor. Time your glides past the scanners.",23,150);
            Copy("CONTAINMENT CORE / LEVEL 6  -  NEW: PHASE\nFor 7 seconds the rat passes through violet containment fields, fire, lasers, electric gates and wheels. Coolant still drowns a phasing rat, so jump the pits. A field stays open until you are clear of it.",23,150);
            Copy("ESCAPE SPIRE / LEVEL 7  -  NEW: GROUND POUND\nJump, then press S to slam down and smash a cracked, gold-edged hatch. Crawl through the service channel below. Every augment returns for the final climb: pound through the vault roof to reach the exit.",23,150);
            Copy("TEMPORARY AUGMENTS\nCyan orb stations recharge after 3 seconds. Jetpack: 12s with limited fuel; double jump: 18s; shield: 18s, absorbs one hit; speed: 12s; gravity pulse: one E use within 6m, expires after 20s; slow time: 10s, slows machinery but not your rat; dash: 15s; wall jump: 18s; glide: 15s; phase: 7s; ground pound: 15s. The HUD shows remaining time and fuel. Falls and deep coolant bypass shields. A new rat loses powers and replenishes stations. Reset current puzzle also clears powers. Magnetic gates stay open for the attempt.",23,235);
            Copy("Gold coins add to your saved wallet and return on a new attempt; cyan orbs are temporary powers. New Game resets level unlocks but keeps tokens, outfits and lifetime statistics.",23,90);
        }
        else if(tab==3)
        {
            Copy("POWER-UPS / TEMPORARY AUGMENTS",28,50);
            Copy("Touch a cyan station to collect its power. Different powers can be combined; collecting the same one refreshes its timer. Watch the HUD for remaining time. Stations normally recharge after 3 seconds.",23,110);
            for (int i=0;i<RatPowerups.Names.Length;i++)
                Copy($"<color=#FFD080>{RatPowerups.Names[i]} / {RatPowerups.Durations[i]:0}s</color>\n{RatPowerups.Hints[i]}",23,100);
            Copy("LIMITS & RESET\nJetpack fuel can run out before its timer. Shield absorbs one hit; falls and deep coolant bypass it. Gravity Pulse is consumed when you press E, so get within 6 metres of a gate first. Phase does not protect against coolant or falls.\n\nLosing a rat or resetting the puzzle clears all powers and replenishes stations. Checkpoints do not preserve powers. Outfits and gold tokens do not grant powers.",23,260);
        }
        else Copy("Behind the glass of a quiet research laboratory, three rats have learned the rhythm of the tests. Doors click, warning lamps blink, and every enclosure promises another reward token.\n\nTonight, the transfer equipment has been left running. One rat ventures ahead while the others wait their turn. Move the laboratory's blocks, outwit its security systems and use its transfer pads to find a way outside.\n\nEvery rat that makes it through is a reason to celebrate.",26,400);
    }
    private void ShowSettings(Action returnTo)
    {
        Page("Settings","Changes apply immediately. Audio and graphics preferences stay saved.",()=>{CampaignProfile.Save();returnTo();});
        RatSettings s=CampaignProfile.Data.settings;
        TokenSettings();
        Slider("Master volume",s.master,v=>s.master=v);
        Slider("Music / ambience",s.music,v=>s.music=v);
        Slider("Sound effects",s.effects,v=>s.effects=v);
        Button muteButton = Button(s.mute ? "Mute all: ON" : "Mute all: OFF",()=>{s.mute=!s.mute;CampaignSettings.Apply();CampaignProfile.Save();ShowSettings(returnTo);});
        if (s.mute)
        {
            ColorBlock muteColors = muteButton.colors;
            muteColors.normalColor = muteColors.highlightedColor;
            muteColors.selectedColor = muteColors.normalColor;
            muteButton.colors = muteColors;
        }
        Button("Test sound"+(s.mute||s.master==0||s.effects==0 ? " (silent at current settings)" : ""),()=>AudioManager.Instance?.PlayCheckpoint());
        string[] quality={"Performance","Balanced (4× anti-aliasing)","High (8× anti-aliasing)"};
        Button("Graphics: "+quality[s.quality],()=>{s.quality=(s.quality+1)%3;CampaignSettings.Apply();CampaignProfile.Save();ShowSettings(returnTo);});
        Button("VSync: "+(s.vSync?"ON":"OFF"),()=>{s.vSync=!s.vSync;CampaignSettings.Apply();CampaignProfile.Save();ShowSettings(returnTo);});
        if(Application.platform!=RuntimePlatform.WebGLPlayer) Button("Fullscreen: "+(s.fullscreen?"ON":"OFF"),()=>{s.fullscreen=!s.fullscreen;CampaignSettings.Apply();CampaignProfile.Save();ShowSettings(returnTo);});
        Button("Reduced decorative motion: "+(s.reducedMotion?"ON":"OFF"),()=>{s.reducedMotion=!s.reducedMotion;CampaignProfile.Save();ShowSettings(returnTo);});
        Button("Restore audio defaults",()=>{s.master=.8f;s.music=.5f;s.effects=.8f;s.mute=false;CampaignSettings.Apply();CampaignProfile.Save();ShowSettings(returnTo);});
        if(session==null) Button("Data — erase all saved data",()=>Confirm("Erase all data?","This clears level progress, tokens, outfits, statistics and settings. This cannot be undone.",()=>{CampaignProfile.Erase();CampaignSettings.Apply();ShowHome();},()=>ShowSettings(returnTo)));
    }
    private void TokenSettings()
    {
        TMP_Text balance = Label(content, $"TOKENS / Wallet: {CampaignProfile.Wallet}", 24);
        balance.gameObject.AddComponent<LayoutElement>().preferredHeight = 40;
        RectTransform entry = Rect("Token amount", content);
        entry.gameObject.SetActive(false);
        entry.gameObject.AddComponent<LayoutElement>().preferredHeight = 54;
        var background = entry.gameObject.AddComponent<Image>(); background.color = panel;
        RectTransform viewport = Rect("Input viewport", entry); Stretch(viewport);
        viewport.offsetMin = new Vector2(18,4); viewport.offsetMax = new Vector2(-18,-4);
        viewport.gameObject.AddComponent<RectMask2D>();
        TMP_Text value = Label(viewport, "", 24); Stretch(value.rectTransform);
        TMP_Text placeholder = Label(viewport, "Enter number of tokens", 24); Stretch(placeholder.rectTransform); placeholder.color = muted;
        value.alignment = TextAlignmentOptions.MidlineLeft;
        placeholder.alignment = TextAlignmentOptions.MidlineLeft;
        var input = entry.gameObject.AddComponent<TMP_InputField>();
        input.targetGraphic = background;
        input.textViewport = viewport;
        input.textComponent = (TextMeshProUGUI)value;
        input.placeholder = placeholder;
        input.contentType = TMP_InputField.ContentType.IntegerNumber;
        input.lineType = TMP_InputField.LineType.SingleLine;
        input.characterLimit = 10;
        entry.gameObject.SetActive(true);
        TMP_Text feedback = null;
        Button("Add tokens", () => {
            if (!int.TryParse(input.text, out int amount) || amount <= 0)
            { feedback.text = "Enter a positive whole number."; return; }
            if (!CampaignProfile.AddTokens(amount))
            { feedback.text = $"Amount too large. You can add up to {int.MaxValue - CampaignProfile.Data.earned} more tokens."; return; }
            balance.text = $"TOKENS / Wallet: {CampaignProfile.Wallet}";
            feedback.text = $"Added {amount} tokens.";
            input.text = "";
        });
        feedback = Label(content, "Added tokens stay in your wallet and can be spent in the shop.", 20);
        feedback.gameObject.AddComponent<LayoutElement>().preferredHeight = 55;
    }
    private void Slider(string title,float value,Action<float> change)
    {
        RectTransform row=Rect(title,content);row.gameObject.AddComponent<LayoutElement>().preferredHeight=70;
        TMP_Text label=Label(row,$"{title}  {Mathf.RoundToInt(value*100)}%",23);Place(label.rectTransform,new Vector2(0,.5f),Vector2.one);
        RectTransform track=Rect("Track",row);Place(track,new Vector2(.01f,.07f),new Vector2(.97f,.35f));track.gameObject.AddComponent<Image>().color=panel;
        RectTransform fill=Rect("Fill",track);Stretch(fill);var fillImage=fill.gameObject.AddComponent<Image>();fillImage.color=cyan;
        RectTransform handle=Rect("Handle",track);handle.sizeDelta=new Vector2(22,32);handle.gameObject.AddComponent<Image>().color=Color.white;
        var slider=track.gameObject.AddComponent<UnityEngine.UI.Slider>();slider.fillRect=fill;slider.handleRect=handle;slider.targetGraphic=handle.GetComponent<Image>();slider.minValue=0;slider.maxValue=20;slider.wholeNumbers=true;slider.value=Mathf.RoundToInt(value*20);
        track.gameObject.AddComponent<SettingsSliderSave>();
        slider.onValueChanged.AddListener(v=>{change(v/20);label.text=$"{title}  {v*5:0}%";CampaignSettings.Apply();});
    }
    public void ShowResult()
    {
        RatAttempt result=session.Result;
        if(result==null)return;
        bool won=result.outcome=="won", final=won&&session.LevelIndex==CampaignCatalog.Count-1;
        string heading=!won?"Trial ended":result.survivors==3?"All paws accounted for!":result.survivors==2?"Two rats, one way out!":"A narrow escape!";
        Page(heading,final?"YOU ESCAPED THE LABORATORY":won?"Chamber cleared. The next route is open.":"No rats remain in this attempt. Retry with the rats that entered this level.",session.Home);
        if(won) Preview(result.survivors,CampaignProfile.Equipped,180,final);
        Copy($"Rats remaining this level: {result.survivors}/3\nTime {CampaignProfile.TimeText(result.seconds)}   •   Rats lost {result.deaths}   •   Tokens this attempt {result.coins}",25,90);
        if(won&&!final) Focus(Button("Next level",()=>session.LoadLevel(session.LevelIndex+1)));
        Button(won?"Replay level":$"Retry with {lives.EntryLives} "+(lives.EntryLives==1?"rat":"rats"),()=>session.LoadLevel(session.LevelIndex));
        if(final) Button("Laboratory map",ShowSelect);
        Button("Home",session.Home);
    }
    private void OnDestroy(){ClearPreview();}

    private GameObject previousSelection;
    private void LateUpdate()
    {
        GameObject selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        if (selected == null || selected == previousSelection || pageScroll == null || !selected.transform.IsChildOf(content)) return;
        previousSelection = selected;
        Canvas.ForceUpdateCanvases();
        Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(pageScroll.viewport, selected.transform);
        Rect visible = pageScroll.viewport.rect;
        Vector2 offset = content.anchoredPosition;
        if (bounds.min.y < visible.yMin) offset.y += visible.yMin - bounds.min.y;
        else if (bounds.max.y > visible.yMax) offset.y -= bounds.max.y - visible.yMax;
        content.anchoredPosition = offset;
    }
}

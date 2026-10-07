using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object=UnityEngine.Object;

/// <summary>Optional focused verification for the authored campaign; never regenerates scenes.</summary>
[InitializeOnLoad]
public static class CampaignValidation
{
    private static IEnumerator checks;
    private static readonly System.Collections.Generic.Stack<IEnumerator> nested = new System.Collections.Generic.Stack<IEnumerator>();
    private static int frames;
    private static double deadline;
    private static Keyboard keyboard;
    private static string output;
    private static Key[] heldKeys = new Key[0];
    static CampaignValidation(){EditorApplication.update+=StartChecks;}
    private static void Require(bool condition,string message){if(!condition)throw new Exception("Campaign check: "+message);}
    private static T Field<T>(object target,string name)=>(T)target.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(target);
    private static T[] All<T>() where T:Object=>Object.FindObjectsByType<T>(FindObjectsSortMode.None);
    private static T One<T>() where T:Object=>Object.FindFirstObjectByType<T>();

    [MenuItem("Project R.A.T./Campaign/Validate saved scenes and profile")]
    public static void ValidateAssets()
    {
        ActiveGlassValidation.VerifyCameraFollow();
        string[] built=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray();
        Require(built.Length==8&&built[0]=="Assets/Scenes/Home.unity","Home plus seven playable scenes must be enabled.");
        foreach(string path in CampaignCatalog.Scenes)Require(built.Contains(path),"Missing build target "+path);
        foreach(string path in built)
        {
            EditorSceneManager.OpenScene(path);
            foreach(var t in All<Transform>())Require(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)==0,"Missing script on "+t.name);
            Require(All<Camera>().Count(c=>c.enabled)==1&&All<AudioListener>().Count(a=>a.enabled)==1,"Exactly one camera and listener: "+path);
            Require(One<CampaignUI>()!=null&&Field<TMP_FontAsset>(One<CampaignUI>(),"font")!=null,"UI font assigned: "+path);
            if(path.EndsWith("Home.unity")){Require(All<PlayerRatController>().Length==0,"Home cannot start an attempt.");continue;}
            int index=Array.IndexOf(CampaignCatalog.Scenes,path);
            Require(All<PlayerRatController>().Length==1&&All<RatLifeManager>().Length==1&&All<GameManager>().Length==1&&All<CampaignSession>().Length==1,"Exactly one gameplay owner per level.");
            Require(!All<RatTrialSession>().Any(s=>s.enabled),"Legacy life system must be disabled.");
            var life=One<RatLifeManager>();Require(Field<PlayerRatController>(life,"player")!=null&&Field<FixedCameraFollow>(life,"cameraFollow")!=null,"Explicit life references.");
            var coins=All<CoinPickup>();Require(coins.Length==20&&coins.Select(c=>c.StableId).Distinct().Count()==20,"Twenty distinct coins in "+path);
            Require(coins.All(c=>c.StableId.StartsWith(CampaignCatalog.Ids[index]+":")),"Coin identity belongs to its catalog level.");
            Physics.SyncTransforms();
            foreach(Checkpoint checkpoint in All<Checkpoint>())
            {
                Transform spawn=Field<Transform>(checkpoint,"respawnPoint");Require(spawn!=null,"Checkpoint safe spawn assigned.");
                Require(Physics.Raycast(spawn.position+Vector3.up*.1f,Vector3.down,.65f,~(1<<2),QueryTriggerInteraction.Ignore),"Checkpoint has supported floor: "+spawn.name);
            }
            foreach(PortalEndpoint portal in All<PortalEndpoint>())
            {
                var partner=Field<PortalEndpoint>(portal,"partner");Require(partner!=null&&Field<PortalEndpoint>(partner,"partner")==portal,"Reciprocal portal pair.");
                Transform arrival=Field<Transform>(portal,"arrival");Require(arrival!=null,"Portal arrival exists.");
                Require(Physics.Raycast(arrival.position+Vector3.up*.1f,Vector3.down,.65f,~(1<<2),QueryTriggerInteraction.Ignore),"Portal arrival has floor.");
            }
            Debug.Log("RAT_CAMPAIGN_SCENE_OK: "+path);
        }
        ValidateProfile();
        EditorSceneManager.OpenScene("Assets/Scenes/Home.unity");
        Debug.Log("RAT_CAMPAIGN_ASSETS_OK: references, build catalog, 140 unique tokens, supported checkpoints/arrivals, profile invariants.");
    }
    private static void ValidateProfile()
    {
        string key="ProjectRAT.Validation."+Guid.NewGuid().ToString("N");CampaignProfile.UseValidationProfile(key);
        try
        {
            CampaignProfile.Begin(0);CampaignProfile.AddTime(2);
            for(int i=0;i<20;i++)Require(CampaignProfile.Collect("enclosure:"+i),"Unique token accepted.");
            Require(!CampaignProfile.Collect("enclosure:0")&&CampaignProfile.Wallet==20,"Duplicate token ignored.");
            Require(CampaignProfile.Buy(1)&&!CampaignProfile.Buy(1)&&CampaignProfile.Wallet==10,"Purchase is idempotent.");
            Require(!CampaignProfile.Buy(3),"Insufficient funds cannot buy.");CampaignProfile.Equip(1);CampaignProfile.Death();CampaignProfile.Finish("won",2);
            Require(CampaignProfile.Finish("won",2)==null&&CampaignProfile.Data.unlocked==2,"Terminal outcome commits once.");
            CampaignProfile.Begin(0);CampaignProfile.AddTime(3);CampaignProfile.Save();CampaignProfile.UseValidationProfile(key);
            var r=CampaignProfile.Record("enclosure");Require(r.attempts==2&&r.wins==1&&r.abandoned==1&&r.deaths==1&&r.seconds==5,"Interrupted attempt recovers exactly once.");
            CampaignProfile.UseValidationProfile(key);Require(CampaignProfile.Record("enclosure").abandoned==1,"Repeated reload cannot abandon twice.");
            CampaignProfile.NewCampaign();Require(CampaignProfile.Wallet==10&&CampaignProfile.Equipped==1&&CampaignProfile.Collected(0)==20&&CampaignProfile.Data.unlocked==1,"New game retains economy/stats.");
            CampaignProfile.Data.settings.master=.35f;CampaignProfile.Save();CampaignProfile.UseValidationProfile(key);Require(Mathf.Approximately(CampaignProfile.Data.settings.master,.35f),"Settings persist.");
            CampaignProfile.Erase();Require(CampaignProfile.Wallet==0&&CampaignProfile.Equipped==0&&CampaignProfile.Record("enclosure").attempts==0,"Explicit erase restores coherent defaults.");
            Debug.Log("RAT_PROFILE_TRANSACTIONS_OK");
        }
        finally{CampaignProfile.EndValidationProfile();}
    }
    public static void RunPlayChecks()
    {
        ValidateAssets();
        CampaignProfile.UseValidationProfile("ProjectRAT.Validation."+Guid.NewGuid().ToString("N"));
        SessionState.SetBool("RAT.Campaign.Checks",true);EditorSceneManager.playModeStartScene=null;EditorApplication.EnterPlaymode();
    }
    private static void StartChecks()
    {
        if(!SessionState.GetBool("RAT.Campaign.Checks",false)||!EditorApplication.isPlaying||checks!=null)return;
        try
        {
            output=Path.GetFullPath("Logs/CampaignReview");Directory.CreateDirectory(output);
            keyboard=InputSystem.AddDevice<Keyboard>();keyboard.MakeCurrent();
            heldKeys=new Key[0];InputSystem.onBeforeUpdate+=FeedInput;
            Application.runInBackground=true;InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            Time.captureDeltaTime=1f/60f;
            deadline=EditorApplication.timeSinceStartup+1200;checks=Scenario();nested.Clear();frames=0;
            var driver=new GameObject("Campaign verification driver (not saved)");Object.DontDestroyOnLoad(driver);driver.AddComponent<RatPlaytestDriver>().step=Tick;
        }
        catch(Exception ex){Debug.LogException(ex);Finish(1);}
    }
    private static void Tick()
    {
        try
        {
            if(EditorApplication.timeSinceStartup>deadline)throw new Exception("Campaign checks exceeded twenty minutes.");
            if(frames-->0)return;
            while(!checks.MoveNext())
            {
                if(nested.Count==0){Debug.Log("RAT_CAMPAIGN_PLAYMODE_OK");Finish(0);return;}
                checks=nested.Pop();
            }
            if(checks.Current is IEnumerator child){nested.Push(checks);checks=child;frames=0;}
            else frames=checks.Current is int n?n:1;
        }
        catch(Exception ex){Debug.LogException(ex);Finish(1);}
    }
    private static void Press(params Key[] keys){heldKeys=keys;}
    private static void FeedInput()
    {
        if(keyboard==null || InputState.currentUpdateType!=InputUpdateType.Dynamic)return;
        // Editor input updates otherwise consume queued events before the game loop.
        InputState.Change(keyboard,new KeyboardState(heldKeys));keyboard.MakeCurrent();
    }
    private static void Click(string label)
    {
        Button button=All<Button>().FirstOrDefault(b=>b.interactable&&b.GetComponentInChildren<TMP_Text>()?.text==label);
        Require(button!=null,"Button available: "+label);button.onClick.Invoke();
    }
    private static string Page=>Field<string>(One<CampaignUI>(),"page");
    private static IEnumerator Scenario()
    {
        yield return 20;
        Require(Page=="Project R.A.T."&&!CampaignProfile.Playing,"Home boots without starting an attempt.");Capture("01-home");
        Click("Laboratory map");yield return 5;Require(Page=="Laboratory map","Map opens.");Capture("02-level-map");Click("BACK / ESC");yield return 3;
        Click("Shop");yield return 5;Require(Page=="Token exchange","Shop opens.");Capture("03-shop");Click("BACK / ESC");yield return 3;
        Click("Help");yield return 3;Capture("04-help");Click("BACK / ESC");yield return 3;
        Click("Settings");yield return 3;Capture("05-settings");Click("BACK / ESC");yield return 3;
        Click("Statistics");yield return 3;Capture("06-statistics");Click("BACK / ESC");yield return 3;
        Click("New game");yield return 25;
        Require(One<CampaignSession>().LevelIndex==0&&One<RatLifeManager>().LivesRemaining==3,"New game enters enclosure with three rats.");Capture("07-enclosure");
        Debug.Log("RAT_INPUT_BEFORE: page="+Page+" state="+GameManager.Instance.CurrentState+" keyboard="+Keyboard.current?.deviceId);
        Press(Key.Escape);yield return 3;
        Debug.Log("RAT_INPUT_HELD: page="+Page+" state="+GameManager.Instance.CurrentState+" current="+Keyboard.current?.deviceId+" test="+keyboard.deviceId+" down="+keyboard.escapeKey.isPressed);
        Press();yield return 3;
        Require(GameManager.Instance.CurrentState==GameManager.GameState.Paused&&Page=="Paused","Escape pauses; actual page="+Page+" state="+GameManager.Instance.CurrentState);
        double time=CampaignProfile.Data.active.seconds;Vector3 position=One<PlayerRatController>().transform.position;
        Press(Key.Space,Key.D);yield return 15;Press();
        Require(CampaignProfile.Data.active.seconds==time&&Vector3.Distance(position,One<PlayerRatController>().transform.position)<.001f,"Paused input and timer do not advance.");Capture("08-pause");
        Click("Settings");yield return 3;Click("BACK / ESC");yield return 3;Require(Page=="Paused"&&Time.timeScale==0,"Settings return remains paused.");Click("Resume");yield return 5;
        var life=One<RatLifeManager>();
        for(int i=0;i<3;i++){life.OnRatDied();yield return 3;while(life.Invulnerable)yield return 1;}
        Require(Page=="Trial ended"&&CampaignProfile.Record("enclosure").failures==1,"Three losses produce one failure screen.");Capture("09-failure");Click("Retry with 3 rats");yield return 20;
        Require(One<RatLifeManager>().LivesRemaining==3&&CampaignProfile.Record("enclosure").attempts==2,"Retry starts exactly one new attempt.");
        var coin=All<CoinPickup>().OrderBy(c=>c.transform.position.x).First();One<PlayerRatController>().Respawn(coin.transform.position-Vector3.up*.25f);yield return 8;
        Require(CampaignProfile.Wallet==1,"Real trigger collects a token once.");
        yield return ReferenceRouteChecks.Check(0,Press);
        yield return EnclosureRouteChecks.Wheel(Press,27.5f,16.6f,true,"L1 time the rotation deck wheel");
        GameManager.Instance.RequestWin();yield return 4;Require(One<CampaignSession>().Result.survivors==3,"Win snapshots remaining rats.");Click("Next level");yield return 20;
        Require(One<CampaignSession>().LevelIndex==1,"Next opens Reactor Divide.");
        Capture("level-2");yield return ReferenceRouteChecks.Check(1,Press);
        GameManager.Instance.RequestWin();yield return 4;Click("Next level");yield return 20;
        Require(One<CampaignSession>().LevelIndex==2,"Next opens Relay Archive.");
        for(int index=2;index<CampaignCatalog.Count;index++)
        {
            Require(One<CampaignSession>().LevelIndex==index,"Catalog order.");Capture("level-"+(index+1));
            yield return EnclosureRouteChecks.Check(index,Press);
            GameManager.Instance.RequestWin();yield return 4;
            Require(One<CampaignSession>().Result.survivors==3,"Fresh stage result has three rats.");
            if(index<CampaignCatalog.Count-1){Click("Next level");yield return 20;}
        }
        Require(CampaignProfile.Data.campaignComplete&&CampaignProfile.Data.campaignClears==1,"Last catalog entry completes campaign once.");Capture("10-ending-three");
        for(int losses=1;losses<=2;losses++)
        {
            Click("Replay level");yield return 20;life=One<RatLifeManager>();
            for(int j=0;j<losses;j++){life.OnRatDied();yield return 3;while(life.Invulnerable)yield return 1;}
            GameManager.Instance.RequestWin();yield return 4;
            Require(One<CampaignSession>().Result.survivors==3-losses,"Survivor ending variant.");Capture(losses==1?"11-ending-two":"12-ending-one");
        }
        Require(CampaignProfile.Data.campaignClears==1,"Replaying ending cannot inflate campaign clears.");
        foreach(var record in CampaignProfile.Data.levels)Require(record.attempts==record.wins+record.failures+record.abandoned,"Closed attempt accounting invariant.");
        Click("Home");yield return 20;Click("Statistics");yield return 5;Capture("13-statistics-populated");
    }
    private static void Capture(string name)
    {
        if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)return;
        Camera camera=Camera.main;Canvas canvas=One<CampaignUI>().GetComponentInChildren<Canvas>();
        var previousMode=canvas.renderMode;var previousCamera=canvas.worldCamera;RenderTexture previousTarget=camera.targetTexture;RenderTexture previousActive=RenderTexture.active;
        var target=new RenderTexture(1600,1000,24){antiAliasing=4};var image=new Texture2D(1600,1000,TextureFormat.RGB24,false);
        try
        {
            camera.targetTexture=target;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;
            image.ReadPixels(new Rect(0,0,1600,1000),0,0);image.Apply();File.WriteAllBytes(Path.Combine(output,name+".png"),image.EncodeToPNG());
        }
        finally{canvas.renderMode=previousMode;canvas.worldCamera=previousCamera;camera.targetTexture=previousTarget;RenderTexture.active=previousActive;Object.DestroyImmediate(image);Object.DestroyImmediate(target);}
    }
    private static void Finish(int code)
    {
        SessionState.SetBool("RAT.Campaign.Checks",false);Time.timeScale=1;Time.captureDeltaTime=0;
        InputSystem.onBeforeUpdate-=FeedInput;
        if(keyboard!=null)InputSystem.RemoveDevice(keyboard);
        // Suppress teardown accounting before returning to the real profile namespace.
        if(CampaignProfile.Playing)CampaignProfile.Finish("abandoned",0);
        foreach(CampaignSession session in All<CampaignSession>())typeof(CampaignSession).GetField("ready",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(session,false);
        CampaignProfile.EndValidationProfile();EditorApplication.Exit(code);
    }
}

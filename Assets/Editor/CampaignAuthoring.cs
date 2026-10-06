using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

/// <summary>Explicit one-time authoring. New campaign paths must not already exist.</summary>
public static class CampaignAuthoring
{
    private static Material steel, dark, cyan, amber, red, gold, white;
    private static TMP_FontAsset font;
    private static GameObject playerPrefab, displayPrefab;
    private static RatLifeManager lives;
    private static FixedCameraFollow follow;
    private static Transform root;
    private static int coinNumber, levelIndex;
    private static readonly string[] NewPaths = CampaignCatalog.Scenes.Skip(1).Concat(new[]{"Assets/Scenes/Home.unity"}).ToArray();

    [MenuItem("Project R.A.T./Campaign/Create seven-level campaign (new scenes only)")]
    public static void CreateCampaign()
    {
        if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
        foreach(string path in NewPaths) if(File.Exists(path)) throw new InvalidOperationException("Refusing to overwrite an authored scene: "+path);
        PrepareMaterials(); CreateAmbient();
        EditorSceneManager.OpenScene(PrototypeAssetPaths.PrototypeScene);
        var player=Object.FindFirstObjectByType<PlayerRatController>();
        AddCosmetic(player.gameObject);
        playerPrefab=PrefabUtility.SaveAsPrefabAsset(player.gameObject,"Assets/Prefabs/CampaignRat.prefab");
        Transform model=(Transform)new SerializedObject(player).FindProperty("model").objectReferenceValue;
        var display=Object.Instantiate(model.gameObject);display.name="Rat display";display.transform.position=Vector3.zero;display.transform.rotation=Quaternion.identity;
        AddCosmetic(display);
        displayPrefab=PrefabUtility.SaveAsPrefabAsset(display,"Assets/Prefabs/RatDisplay.prefab");Object.DestroyImmediate(display);
        WireExisting();
        CreateLevel(1,new[]{"portal","crate"});
        CreateLevel(2,new[]{"crate","portal","timing"});
        CreateLevel(3,new[]{"timing","portal","timing"});
        CreateLevel(4,new[]{"scanner","ascent","crate"});
        CreateLevel(5,new[]{"portal","crate","scanner"});
        CreateLevel(6,new[]{"crate","timing","portal","ascent"});
        CreateHome();
        var build=new List<EditorBuildSettingsScene>{new EditorBuildSettingsScene("Assets/Scenes/Home.unity",true)};
        build.AddRange(CampaignCatalog.Scenes.Select(p=>new EditorBuildSettingsScene(p,true)));
        build.AddRange(EditorBuildSettings.scenes.Where(s=>!build.Any(b=>b.path==s.path)).Select(s=>new EditorBuildSettingsScene(s.path,false)));
        EditorBuildSettings.scenes=build.ToArray();
        AssetDatabase.SaveAssets();
        Debug.Log("RAT_CAMPAIGN_AUTHORED: Home + seven playable scenes, 140 unique coins.");
    }
    internal static void PrepareMaterials()
    {
        steel=Mat("Campaign steel",new Color(.20f,.30f,.39f));dark=Mat("Campaign navy",new Color(.035f,.075f,.12f));
        cyan=Mat("Campaign cyan",new Color(.15f,.9f,.86f),true);amber=Mat("Campaign amber",new Color(1,.61f,.12f),true);
        red=Mat("Campaign hazard",new Color(1,.13f,.22f),true);gold=Mat("Campaign token",new Color(1,.72f,.16f),true);white=Mat("Campaign cloth",new Color(.87f,.89f,.83f));
        font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if(font==null)throw new Exception("Missing project TMP font.");
    }
    private static Material Mat(string name,Color color,bool glow=false)
    {
        string path="Assets/Materials/Environment/"+name+".mat";
        Material m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(m!=null)return m;
        m=new Material(Shader.Find("Standard")){name=name,color=color};m.SetFloat("_Glossiness",.35f);
        if(glow){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",color*.35f);}
        AssetDatabase.CreateAsset(m,path);return m;
    }
    internal static void AddCosmetic(GameObject rat)
    {
        if(rat.GetComponent<RatCosmetic>()!=null)return;
        Transform model=rat.GetComponentsInChildren<Transform>().FirstOrDefault(t=>t.name=="Model") ?? rat.transform;
        RatCosmetic cosmetic=rat.AddComponent<RatCosmetic>();
        Set(cosmetic,"fur",rat.GetComponentsInChildren<Renderer>().Where(r=>r.name=="Body"||r.name=="Head").Cast<Object>().ToArray());
        GameObject scarf=model.Find("Teal lab scarf")?.gameObject;
        if(scarf==null){scarf=Box("Teal lab scarf",Vector3.zero,new Vector3(.57f,.13f,.18f),cyan,false,model);scarf.transform.localPosition=new Vector3(0,.09f,.29f);scarf.transform.localRotation=Quaternion.identity;}
        GameObject vest=model.Find("Technician vest")?.gameObject;
        if(vest==null){vest=Box("Technician vest",Vector3.zero,new Vector3(.51f,.48f,.38f),white,false,model);vest.transform.localPosition=new Vector3(0,-.02f,-.02f);vest.transform.localRotation=Quaternion.identity;}
        if(vest.transform.Find("ID patch")==null){var badge=Box("ID patch",Vector3.zero,new Vector3(.02f,.12f,.16f),cyan,false,vest.transform);badge.transform.localPosition=new Vector3(-.51f,.15f,.2f);}
        Set(cosmetic,"scarf",scarf);Set(cosmetic,"vest",vest);scarf.SetActive(false);vest.SetActive(false);
    }
    internal static void WireAdapted(int index, RatLifeManager life, Vector2[] positions)
    {
        PrepareMaterials();
        displayPrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/RatDisplay.prefab");
        root=new GameObject("Campaign tokens").transform;levelIndex=index;coinNumber=0;lives=life;
        WireFlow(index,Object.FindFirstObjectByType<GameManager>(),life);
        foreach(Vector2 point in positions)Coin(point.x,point.y);
    }
    private static void WireExisting()
    {
        if(Object.FindFirstObjectByType<CampaignSession>()!=null)return;
        root=new GameObject("Campaign additions").transform;levelIndex=0;coinNumber=0;
        lives=Object.FindFirstObjectByType<RatLifeManager>();follow=Object.FindFirstObjectByType<FixedCameraFollow>();
        foreach(Transform t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None)) if(t.name.StartsWith("Waiting rat",StringComparison.Ordinal))AddCosmetic(t.gameObject);
        WireFlow(0,Object.FindFirstObjectByType<GameManager>(),lives);
        foreach(TextMesh old in Object.FindObjectsByType<TextMesh>(FindObjectsSortMode.None))
        {
            string text=old.text;float size=Mathf.Clamp(old.characterSize*old.fontSize,2.8f,6);Color color=old.color;GameObject go=old.gameObject;
            Object.DestroyImmediate(old);var oldRenderer=go.GetComponent<MeshRenderer>();if(oldRenderer!=null)Object.DestroyImmediate(oldRenderer);
            var tmp=go.AddComponent<TextMeshPro>();tmp.font=font;tmp.fontSize=size;tmp.text=text;tmp.color=color;tmp.alignment=TextAlignmentOptions.Center;tmp.rectTransform.sizeDelta=new Vector2(36,3);tmp.textWrappingMode=TextWrappingModes.NoWrap;
        }
        Vector2[] points={new Vector2(1,.7f),new Vector2(7,1.1f),new Vector2(12,1.5f),new Vector2(17,1.5f),new Vector2(24,3.3f),new Vector2(29.5f,2.1f),new Vector2(36,2.1f),new Vector2(45,2.1f),new Vector2(50.5f,3),new Vector2(54.5f,3.9f),new Vector2(59,4.8f),new Vector2(83,4.8f)};
        foreach(Vector2 p in points)Coin(p.x,p.y);
        OptionalCoins(2,0);OptionalCoins(46,1.4f);
        Camera.main.allowMSAA=true;Camera.main.allowHDR=false;
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
    }
    private static void CreateLevel(int index,string[] modules)
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);levelIndex=index;coinNumber=0;
        root=new GameObject(CampaignCatalog.Names[index]).transform;
        float length=modules.Length*30;
        var manager=new GameObject("Game systems");var game=manager.AddComponent<GameManager>();lives=manager.AddComponent<RatLifeManager>();
        GameObject rat=(GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);rat.name="PlayerRat";rat.transform.position=new Vector3(1,.08f,0);
        var start=Point("Release point",rat.transform.position);
        Set(lives,"player",rat.GetComponent<PlayerRatController>());Set(lives,"startPoint",start);Set(lives,"killY",-4f);
        var waiting=new Object[2];
        for(int i=0;i<2;i++){var prop=(GameObject)PrefabUtility.InstantiatePrefab(displayPrefab);prop.name="Waiting rat "+(i+2);prop.transform.position=new Vector3(-1-i,.42f,.5f);prop.transform.rotation=Quaternion.Euler(0,90,0);waiting[i]=prop;}
        Set(lives,"waitingRats",waiting);Set(game,"lifeManager",lives);
        Camera camera=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener)).GetComponent<Camera>();camera.tag="MainCamera";camera.transform.position=new Vector3(5,3.5f,-22);camera.orthographic=true;camera.orthographicSize=5.4f;camera.backgroundColor=new Color(.025f,.045f,.07f);camera.clearFlags=CameraClearFlags.SolidColor;camera.allowHDR=false;camera.allowMSAA=true;camera.cullingMask=~(1<<31);
        follow=camera.gameObject.AddComponent<FixedCameraFollow>();Set(follow,"target",rat.transform);Set(follow,"followOffset",new Vector2(2.5f,1.8f));Set(follow,"clampToBounds",true);Set(follow,"minBounds",new Vector2(5,3.5f));Set(follow,"maxBounds",new Vector2(length-5,8.5f));Set(lives,"cameraFollow",follow);
        var light=new GameObject("Cool key light").AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.2f;light.transform.rotation=Quaternion.Euler(35,-25,0);
        RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.49f,.59f,.68f);RenderSettings.fog=false;
        Box("Lab backing",new Vector3(length/2,4,2.3f),new Vector3(length+8,18,.3f),dark,false);
        for(int i=0;i<=length/5;i++){Box("Panel seam",new Vector3(i*5,4,2.05f),new Vector3(.06f,17,.08f),steel,false);Box("Ceiling route light",new Vector3(i*5,8.5f,1.8f),new Vector3(2.7f,.06f,.1f),cyan,false);}
        Box("Start boundary",new Vector3(-3,4,0),new Vector3(.3f,18,3),steel,true);Box("End boundary",new Vector3(length+2,4,0),new Vector3(.3f,18,3),steel,true);
        float height=0;
        var positions=new List<Vector2>();
        for(int room=0;room<modules.Length;room++)
        {
            float x=room*30;float y=height;
            Transform roomRoot=new GameObject($"Room {room+1} - {modules[room]}").transform;roomRoot.SetParent(root);
            Transform outer=root;root=roomRoot;
            var baseline=root.gameObject.AddComponent<PuzzleRoom>();Set(baseline,"checkpointNumber",room);
            if(room>0)CheckpointAt(room,x+1,y);
            if(modules[room]=="timing")
            {
                Platform(x-3,x+10,y);Platform(x+16,x+30,y);
                GameObject shuttle=Platform(x+10,x+13,y);var motion=shuttle.AddComponent<TrialMovingPlatform>();motion.travel=new Vector3(3,0,0);motion.period=6;
                HazardBox("Coolant channel",new Vector3(x+13,y-1,0),new Vector3(6,.5f,2.5f),cyan);
                TimedGate(x+23,y,false);
                Label("SHUTTLE / WAIT FOR A SAFE CROSSING",x+13,y+5,3.1f);
            }
            else if(modules[room]=="ascent")
            {
                Platform(x-3,x+6,y);Platform(x+6,x+12,y+.8f);Platform(x+13.5f,x+19,y+1.6f);Platform(x+20.5f,x+30,y+2.4f);
                HazardBox("Ascent coolant",new Vector3(x+18,y-1,0),new Vector3(24,.5f,2.5f),cyan);height+=2.4f;
                Label("ASCENT / FOLLOW THE LIT EDGES",x+15,y+6,3.4f);
            }
            else
            {
                Platform(x-3,x+30,y);
                if(modules[room]=="portal")
                {
                    Box("Transfer partition",new Vector3(x+12,y+3,0),new Vector3(.55f,6,3),steel,true);
                    Pair(x+8,x+17,y,"A"+room);
                    Switch(x+20,x+26,y);
                    Label("TRANSFER / E AT THE MATCHING PAD",x+12,y+6.8f,3.1f);
                }
                if(modules[room]=="crate"){CratePuzzle(x,y);Label("CRATE / MATCH THE PLATE AND LASER",x+15,y+5,3.2f);}
                if(modules[room]=="scanner"){TimedGate(x+11,y,true);Label("SCANNER / WAIT OUTSIDE THE MARKED ZONE",x+15,y+5,3.1f);}
            }
            float[] coinX=modules[room]=="portal"?new[]{3f,5,7,19,23,28}:modules[room]=="crate"?new[]{3f,5,6,15,23,28}:modules[room]=="ascent"?new[]{3f,8,10,15,17,25}:new[]{3f,6,8,18,20,27};
            foreach(float dx in coinX){float cy=y+.65f;if(modules[room]=="ascent")cy+=dx>=20.5f?2.4f:dx>=13.5f?1.6f:dx>=6?.8f:0;positions.Add(new Vector2(x+dx,cy));}
            if(room==0||room==modules.Length-1)OptionalCoins(x+5,y);
            Set(baseline,"blocks",root.GetComponentsInChildren<PushBlock>(true).Cast<Object>().ToArray());Set(baseline,"switches",root.GetComponentsInChildren<LatchedSwitch>(true).Cast<Object>().ToArray());Set(baseline,"portals",root.GetComponentsInChildren<PortalEndpoint>(true).Cast<Object>().ToArray());Set(baseline,"hazards",root.GetComponentsInChildren<WarningHazard>(true).Cast<Object>().ToArray());
            root=outer;
        }
        // Twelve route tokens distributed over the complete level; eight are on raised detours.
        for(int i=0;i<12;i++){Vector2 p=positions[Mathf.RoundToInt(i*(positions.Count-1)/11f)];Coin(p.x,p.y);}
        var exit=Box("Exit trigger",new Vector3(length-1,height+1,0),new Vector3(1,2,2),cyan,false);var trigger=exit.AddComponent<BoxCollider>();trigger.isTrigger=true;exit.AddComponent<ExitTrigger>();
        Label(index==6?"OUTSIDE / ESCAPE":"TRANSFER COMPLETE >",length-4,height+3.4f,4);
        WireFlow(index,game,lives);
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),CampaignCatalog.Scenes[index]);
    }
    private static void CreateHome()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        root=new GameObject("Home").transform;
        Camera camera=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener)).GetComponent<Camera>();camera.tag="MainCamera";camera.backgroundColor=new Color(.03f,.05f,.08f);camera.clearFlags=CameraClearFlags.SolidColor;camera.cullingMask=~(1<<31);camera.allowHDR=false;
        var light=new GameObject("Menu lighting").AddComponent<Light>();light.type=LightType.Directional;light.transform.rotation=Quaternion.Euler(35,-25,0);RenderSettings.ambientLight=new Color(.55f,.65f,.7f);
        var ui=new GameObject("Campaign interface").AddComponent<CampaignUI>();Set(ui,"font",font);Set(ui,"ratDisplayPrefab",displayPrefab);
        ConfigureAudio(new GameObject("Menu audio"));
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),"Assets/Scenes/Home.unity");
    }
    internal static void WireFlow(int index,GameManager game,RatLifeManager life)
    {
        GameObject go=new GameObject("Campaign flow and interface");
        var session=go.AddComponent<CampaignSession>();var ui=go.AddComponent<CampaignUI>();
        Set(session,"levelIndex",index);Set(session,"game",game);Set(session,"lives",life);Set(session,"ui",ui);
        Set(ui,"font",font);Set(ui,"session",session);Set(ui,"game",game);Set(ui,"lives",life);Set(ui,"ratDisplayPrefab",displayPrefab);
        AudioManager audio=Object.FindFirstObjectByType<AudioManager>();ConfigureAudio(audio!=null?audio.gameObject:new GameObject("Level audio"));
        if(Camera.main!=null){Camera.main.cullingMask=~(1<<31);Camera.main.allowHDR=false;Camera.main.allowMSAA=true;}
    }
    private static void ConfigureAudio(GameObject go)
    {
        var audio=go.GetComponent<AudioManager>();if(audio==null)audio=go.AddComponent<AudioManager>();
        var source=go.GetComponent<AudioSource>();if(source==null)source=go.AddComponent<AudioSource>();source.playOnAwake=false;source.spatialBlend=0;
        Set(audio,"audioSource",source);
        string[] names={"Jump","Checkpoint","RatLost","Completion","Failure"};string[] fields={"jumpClip","checkpointClip","ratLostClip","completionClip","failureClip"};
        for(int i=0;i<names.Length;i++)Set(audio,fields[i],AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/"+names[i]+".wav"));
        var music=new GameObject("Laboratory ambience").AddComponent<AudioSource>();music.transform.SetParent(go.transform);music.clip=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/LaboratoryAmbient.wav");music.loop=true;music.playOnAwake=false;music.spatialBlend=0;Set(audio,"musicSource",music);
    }
    private static void Pair(float a,float b,float y,string label)
    {
        var first=Pad(a,y,label);var second=Pad(b,y,label);
        Set(first,"partner",second);Set(second,"partner",first);
        Set(first,"arrival",Point("Arrival "+label+" left",new Vector3(a-1.8f,y+.08f,0)));
        Set(second,"arrival",Point("Arrival "+label+" right",new Vector3(b+1.8f,y+.08f,0)));
    }
    private static PortalEndpoint Pad(float x,float y,string label)
    {
        var pad=Point("Transfer pad "+label,new Vector3(x,y+.08f,0));var portal=pad.gameObject.AddComponent<PortalEndpoint>();
        Set(portal,"lives",lives);Set(portal,"follow",follow);Set(portal,"pairLabel",label);
        Box("Pad lit base",new Vector3(x,y+.02f,0),new Vector3(1.2f,.06f,1.8f),cyan,false,pad);
        Box("Transfer antenna",new Vector3(x,y+1.1f,.9f),new Vector3(.12f,2.2f,.12f),cyan,false,pad);
        Label("[ "+label+" ]  E",x,y+2.6f,3.3f,pad);return portal;
    }
    private static void Switch(float x,float gateX,float y)
    {
        var shutter=Box("Latched security shutter",new Vector3(gateX,y+2,0),new Vector3(.45f,4,2.4f),steel,true);
        var console=Box("Shutter switch",new Vector3(x,y+.45f,.65f),new Vector3(.65f,.9f,.4f),amber,false);var latch=console.AddComponent<LatchedSwitch>();
        Set(latch,"lives",lives);Set(latch,"shutter",shutter);Set(latch,"indicator",console.GetComponent<Renderer>());Set(latch,"openMaterial",cyan);Set(latch,"closedMaterial",amber);
        Box("Switch cable",new Vector3((x+gateX)/2,y+.05f,.85f),new Vector3(gateX-x,.06f,.07f),amber,false);Label("E / RELEASE",x,y+1.9f,2.8f);
    }
    private static void CratePuzzle(float x,float y)
    {
        GameObject block=Box("Track crate",new Vector3(x+8,y+.55f,0),new Vector3(1,1.1f,1.2f),amber,true);block.AddComponent<Rigidbody>();var push=block.AddComponent<PushBlock>();push.travelLeft=0;push.travelRight=4;
        var laser=Point("Plate-linked laser",Vector3.zero).gameObject.AddComponent<LinkedLaser>();laser.emitter=Point("Laser emitter",new Vector3(x+18,y+.12f,0));laser.receiver=Point("Laser receiver",new Vector3(x+18,y+4.2f,0));
        var beam=Box("Linked lethal beam",new Vector3(x+18,y+2,0),Vector3.one,red,false);var collider=beam.AddComponent<BoxCollider>();collider.isTrigger=true;var hazard=beam.AddComponent<HazardTrigger>();Set(hazard,"lifeManager",lives);
        laser.beam=beam.transform;laser.beamRenderer=beam.GetComponent<Renderer>();laser.lethalCollider=collider;laser.beamWidth=.18f;laser.activeLight=red;laser.safeLight=cyan;laser.deviceLights=new Renderer[0];
        var plate=Point("Crate-only pressure plate",new Vector3(x+12,y+.55f,0)).gameObject;var sensor=plate.AddComponent<BoxCollider>();sensor.isTrigger=true;sensor.size=new Vector3(1.5f,1.2f,1.8f);var pressure=plate.AddComponent<PressurePlate>();pressure.laser=laser;
        var top=Box("Plate top",new Vector3(x+12,y+.03f,0),new Vector3(1.5f,.06f,1.7f),amber,false,plate.transform);pressure.top=top.transform;pressure.indicator=top.GetComponent<Renderer>();pressure.idleMaterial=amber;pressure.pressedMaterial=cyan;
        Box("Plate cable",new Vector3(x+15,y+.02f,.9f),new Vector3(6,.04f,.06f),amber,false);Label("CRATE → PLATE",x+10,y+2.4f,2.8f);
    }
    private static void TimedGate(float x,float y,bool sweep)
    {
        var device=Point(sweep?"Sweeping scanner":"Energy gate",new Vector3(x,y,0));
        var volume=HazardBox("Active energy column",new Vector3(x,y+1.7f,0),new Vector3(.4f,3.4f,2),red,device);
        var beacon=Box("Phase beacon",new Vector3(x,y+3.8f,.8f),new Vector3(.6f,.22f,.2f),cyan,false,device);
        var cycle=device.gameObject.AddComponent<WarningHazard>();Set(cycle,"activeVolume",volume);Set(cycle,"beacon",beacon.GetComponent<Renderer>());Set(cycle,"safe",cyan);Set(cycle,"warning",amber);Set(cycle,"danger",red);Set(cycle,"sweep",sweep?new Vector3(6,0,0):Vector3.zero);Set(cycle,"activeSeconds",sweep?3f:1.5f);
        Box("Marked hazard zone",new Vector3(x+(sweep?3:0),y+.01f,.85f),new Vector3(sweep?7:2,.04f,.25f),amber,false,device);
        Label("WAIT  /  CROSS WHEN CLEAR",x+(sweep?3:0),y+4.5f,2.6f,device);
    }
    private static void CheckpointAt(int number,float x,float y)
    {
        var spawn=Point("Checkpoint "+number+" safe point",new Vector3(x,y+.08f,0));
        var trigger=Point("Checkpoint "+number,new Vector3(x,y+1,0)).gameObject;var collider=trigger.AddComponent<BoxCollider>();collider.isTrigger=true;collider.size=new Vector3(.8f,2,2.4f);
        var checkpoint=trigger.AddComponent<Checkpoint>();Set(checkpoint,"checkpointNumber",number);Set(checkpoint,"respawnPoint",spawn);Set(checkpoint,"lifeManager",lives);
        Box("Checkpoint beacon",new Vector3(x,y+.75f,1),new Vector3(.13f,1.5f,.13f),cyan,false);
    }
    private static void OptionalCoins(float centre,float floor)
    {
        Platform(centre-2.25f,centre+2.25f,floor+1.5f);
        Label("OPTIONAL TOKENS",centre,floor+3.1f,2.4f);
        for(int i=0;i<4;i++)Coin(centre-1.5f+i,floor+2.15f);
    }
    private static void Coin(float x,float y)
    {
        var go=Point("Token "+coinNumber,new Vector3(x,y,0)).gameObject;
        var collider=go.AddComponent<SphereCollider>();collider.radius=.32f;collider.isTrigger=true;
        var visual=GameObject.CreatePrimitive(PrimitiveType.Cylinder);visual.name="Gold reward token";Object.DestroyImmediate(visual.GetComponent<Collider>());visual.transform.SetParent(go.transform,false);visual.transform.localScale=new Vector3(.4f,.045f,.4f);visual.transform.localRotation=Quaternion.Euler(90,0,0);visual.GetComponent<Renderer>().sharedMaterial=gold;
        var mark=Box("Token stripe",Vector3.zero,new Vector3(.06f,.23f,.03f),white,false,go.transform);mark.transform.localPosition=new Vector3(0,0,-.1f);
        var pickup=go.AddComponent<CoinPickup>();Set(pickup,"stableId",CampaignCatalog.Ids[levelIndex]+":"+coinNumber++);Set(pickup,"visual",visual.transform);Set(pickup,"lives",lives);
    }
    private static GameObject Platform(float left,float right,float y)
    {
        var deck=Box("Walkable deck",new Vector3((left+right)/2,y-.2f,0),new Vector3(right-left,.4f,2.6f),steel,true);
        Box("Lit walkable edge",new Vector3((left+right)/2,y+.015f,-1.31f),new Vector3(right-left,.045f,.055f),cyan,false);return deck;
    }
    private static GameObject HazardBox(string name,Vector3 position,Vector3 scale,Material material,Transform parent=null)
    {
        var go=Box(name,position,scale,material,false,parent);var collider=go.AddComponent<BoxCollider>();collider.isTrigger=true;Set(go.AddComponent<HazardTrigger>(),"lifeManager",lives);return go;
    }
    private static GameObject Box(string name,Vector3 position,Vector3 scale,Material material,bool solid,Transform parent=null)
    {
        var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent??root,true);go.transform.position=position;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=material;
        if(!solid)Object.DestroyImmediate(go.GetComponent<Collider>());return go;
    }
    private static Transform Point(string name,Vector3 position)
    {var point=new GameObject(name).transform;point.SetParent(root,true);point.position=position;return point;}
    private static void Label(string text,float x,float y,float size,Transform parent=null)
    {
        var go=new GameObject(text);go.transform.SetParent(parent??root,true);go.transform.position=new Vector3(x,y,-1.5f);var label=go.AddComponent<TextMeshPro>();label.font=font;label.fontSize=size;label.color=new Color(.65f,.86f,.89f);label.alignment=TextAlignmentOptions.Center;label.text=text;label.rectTransform.sizeDelta=new Vector2(29,2);label.textWrappingMode=TextWrappingModes.NoWrap;
    }
    internal static void Set(Object obj,string name,object value)
    {
        var so=new SerializedObject(obj);var p=so.FindProperty(name);if(p==null)throw new Exception(obj.GetType()+" has no field "+name);
        if(value == null)p.objectReferenceValue=null;
        else if(value is Object o)p.objectReferenceValue=o;
        else if(value is Object[] array){p.arraySize=array.Length;for(int i=0;i<array.Length;i++)p.GetArrayElementAtIndex(i).objectReferenceValue=array[i];}
        else if(value is Vector2 v2)p.vector2Value=v2;else if(value is Vector3 v3)p.vector3Value=v3;
        else if(value is bool b)p.boolValue=b;else if(value is int i)p.intValue=i;else if(value is float f)p.floatValue=f;else if(value is string s)p.stringValue=s;
        else throw new Exception("Unsupported serialized value: "+name);
        so.ApplyModifiedPropertiesWithoutUndo();
    }
    private static void CreateAmbient()
    {
        string path="Assets/Audio/LaboratoryAmbient.wav";if(File.Exists(path))return;
        const int rate=22050,seconds=8;int samples=rate*seconds;
        using(var w=new BinaryWriter(File.Create(path)))
        {
            w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write(36+samples*2);w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));w.Write(16);w.Write((short)1);w.Write((short)1);w.Write(rate);w.Write(rate*2);w.Write((short)2);w.Write((short)16);w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write(samples*2);
            for(int i=0;i<samples;i++){double t=(double)i/rate;double tone=.09*Math.Sin(2*Math.PI*110*t)+.04*Math.Sin(2*Math.PI*165*t)+.025*Math.Sin(2*Math.PI*220*t);double pulse=.8+.2*Math.Cos(2*Math.PI*t/seconds);w.Write((short)(tone*pulse*short.MaxValue));}
        }
        AssetDatabase.ImportAsset(path);
    }
}

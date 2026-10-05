using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LaserPuzzleUpgrade
{
    private const string Course = "08 - Pressure Plate Laser Puzzle";
    private static Material metal, dark, gold, green, red, beamMat;
    [MenuItem("Project R.A.T./Game Systems/Add Pressure Plate Laser Puzzle")]
    public static void Upgrade()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.path.EndsWith("/RatEnclosure.unity") && !RatLevelSlots.IsLevelSlot(scene.path))
        {
            Debug.LogError("Open your edited RatEnclosure scene before running this upgrade.");
            return;
        }
        if (GameObject.Find(Course) != null)
        {
            Debug.Log("Puzzle already installed. Existing edits preserved.");
            return;
        }
        var life = Object.FindFirstObjectByType<RatLifeManager>();
        var exit = Object.FindFirstObjectByType<ExitTrigger>();
        if (life == null || exit == null)
        {
            Debug.LogError("Install Game Systems systems first. Need RatLifeManager and ExitTrigger.");
            return;
        }
        // A scene backup is made before any upgrade edits, including unsaved edits.
        string backup = AssetDatabase.GenerateUniqueAssetPath("Assets/Scenes/RatEnclosure_BeforeLaserPuzzle.unity");
        if (!EditorSceneManager.SaveScene(scene, backup, true)) return;
        Undo.IncrementCurrentGroup();
        int undo = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Add pressure plate puzzle");
        metal = Mat("PuzzleMetal", new Color(.24f,.34f,.44f));
        dark = Mat("PuzzleFrame", new Color(.08f,.12f,.17f));
        gold = Mat("PuzzleGold", new Color(1f,.58f,.07f));
        green = Mat("PuzzleSafe", new Color(.13f,1f,.4f));
        red = Mat("PuzzleLive", new Color(1f,.05f,.03f));
        beamMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Hazards/LaserBarrier.mat");
        if (beamMat == null) { Debug.LogError("Missing LaserBarrier material."); return; }
        // Attach to the right edge of the actual edited exit platform.
        float x = exit.transform.position.x + 1.5f;
        float floor = exit.transform.position.y - 1.1f;
        if (Physics.Raycast(exit.transform.position, Vector3.down, out RaycastHit ground,
            8f, ~0, QueryTriggerInteraction.Ignore))
        {
            floor = ground.point.y;
            x = ground.collider.bounds.max.x;
        }
        Transform root = new GameObject(Course).transform;
        Undo.RegisterCreatedObjectUndo(root.gameObject, "Puzzle course");
        Platform(root, "Crate test deck", x, x+13f, floor);
        Platform(root, "Fire terrace", x+13f, x+18f, floor+1.2f);
        Platform(root, "Upper gate and exit", x+19.4f, x+26f, floor+2.4f);
        Box(root,"Upper emitter mount",new Vector3(x+12f,floor+3.6f,1f),new Vector3(1.6f,.24f,1f),metal,false);
        // A dedicated checkpoint prevents replaying the old course after puzzle deaths.
        int number = Object.FindObjectsByType<Checkpoint>(FindObjectsSortMode.None)
            .Select(c => new SerializedObject(c).FindProperty("checkpointNumber").intValue).DefaultIfEmpty(0).Max()+1;
        var spawn = new GameObject("Puzzle respawn").transform;
        spawn.SetParent(root); spawn.position = new Vector3(x+1f,floor+.08f,0f);
        var checkpoint = new GameObject("Checkpoint "+number).AddComponent<Checkpoint>();
        checkpoint.transform.SetParent(root); checkpoint.transform.position = spawn.position+Vector3.up;
        var cpBox = checkpoint.GetComponent<Collider>() as BoxCollider;
        if (cpBox == null) cpBox = checkpoint.gameObject.AddComponent<BoxCollider>();
        cpBox.isTrigger = true; cpBox.size = new Vector3(.8f,2f,2f);
        Set(checkpoint,"checkpointNumber",number); Set(checkpoint,"respawnPoint",spawn); Set(checkpoint,"lifeManager",life);
        var crate = Box(root,"12 - Pushable Block",new Vector3(x+3f,floor+.47f,0f),new Vector3(.9f,.9f,.9f),metal,true);
        crate.AddComponent<PushBlock>().travelRight = 3f;
        Box(crate.transform,"Crate cross brace A",crate.transform.position+new Vector3(0,0,-.47f),new Vector3(.1f,1f,.05f),gold,false).transform.rotation=Quaternion.Euler(0,0,45);
        Box(crate.transform,"Crate cross brace B",crate.transform.position+new Vector3(0,0,-.49f),new Vector3(.1f,1f,.05f),gold,false).transform.rotation=Quaternion.Euler(0,0,-45);
        var plateObject = new GameObject("13 - Pressure Plate");
        plateObject.transform.SetParent(root); plateObject.transform.position = new Vector3(x+6f,floor,0f);
        var sensor = plateObject.AddComponent<BoxCollider>(); sensor.isTrigger=true;
        sensor.center = new Vector3(0,.5f,0); sensor.size = new Vector3(1.1f,1.1f,1.3f);
        var plate = plateObject.AddComponent<PressurePlate>();
        var plateTop = Box(plateObject.transform,"Pressure plate top",new Vector3(x+6f,floor+.025f,0),new Vector3(1.15f,.05f,1.15f),gold,false);
        plate.top=plateTop.transform; plate.indicator=plateTop.GetComponent<Renderer>(); plate.idleMaterial=gold; plate.pressedMaterial=green;
        var laser = new GameObject("11 - Linked Laser Devices").AddComponent<LinkedLaser>();
        laser.transform.SetParent(root);
        laser.activeLight=red; laser.safeLight=green;
        Transform a = Device(laser.transform,"Lower emitter",new Vector3(x+9f,floor+.15f,0f),out Renderer lampA);
        Transform b = Device(laser.transform,"Upper receiver",new Vector3(x+12f,floor+3.8f,0f),out Renderer lampB);
        laser.emitter=a; laser.receiver=b; laser.deviceLights=new[]{lampA,lampB};
        var beam = GameObject.CreatePrimitive(PrimitiveType.Quad);
        beam.name="Diagonal laser beam"; beam.transform.SetParent(laser.transform);
        Object.DestroyImmediate(beam.GetComponent<Collider>());
        beam.GetComponent<Renderer>().sharedMaterial=beamMat;
        laser.beam=beam.transform; laser.beamRenderer=beam.GetComponent<Renderer>();
        laser.lethalCollider=beam.AddComponent<BoxCollider>(); laser.lethalCollider.isTrigger=true;
        beam.AddComponent<HazardTrigger>();
        var rb=beam.AddComponent<Rigidbody>(); rb.isKinematic=true; rb.useGravity=false;
        plate.laser=laser;
        // Retain old objects for reference; replace their role with the linked device.
        var oldLaser=GameObject.Find("11 - Laser Barrier");
        if(oldLaser!=null) { Undo.RecordObject(oldLaser,"Retire old laser"); oldLaser.SetActive(false); }
        // Deliberate upgrade-only placements on different elevations.
        MoveHazard("09 - Fire Emitter",new Vector3(x+16.5f,floor+1.82f,0f));
        MoveHazard("10 - Electric Gate",new Vector3(x+21.5f,floor+2.4f,0f));
        Vector3 oldExit=exit.transform.position;
        Vector3 shift=new Vector3(x+24.5f,floor+3.5f,0)-oldExit;
        foreach(Transform prop in exit.transform.parent.Cast<Transform>().ToArray())
        {
            if(prop==exit.transform || prop.name=="Escape hatch" ||
               (Mathf.Abs(prop.position.x-oldExit.x)<1.6f &&
                (prop.name=="Hatch indicator" || prop.name=="Door handle" || prop.name.Contains("ESCAPE"))))
            { Undo.RecordObject(prop,"Move exit"); prop.position+=shift; }
        }
        var boundary=GameObject.Find("Exit boundary");
        if(boundary!=null) { Undo.RecordObject(boundary.transform,"Extend boundary"); boundary.transform.position=new Vector3(x+26.3f,3f,0); }
        Extend("Lower enclosure rail",x+26.5f); Extend("Top enclosure rail",x+26.5f);
        Extend("One-way observation window",x+26.5f); Extend("Observation window recess",x+26.5f);
        Extend("Upper window bevel",x+26.5f); Extend("Window sill",x+26.5f); Extend("Window sill highlight",x+26.5f);
        // Front glass can be intentionally narrowed in the edited scene; append a new pane.
        var glass=GameObject.Find("Front observation glass");
        if(glass!=null) Box(root,"Puzzle observation glass",new Vector3(x+13f,floor+2f,-2.3f),new Vector3(26f,12.8f,.025f),glass.GetComponent<Renderer>().sharedMaterial,false);
        var follow=Object.FindFirstObjectByType<FixedCameraFollow>();
        if(follow!=null) { var so=new SerializedObject(follow); so.FindProperty("maxBounds").vector2Value=new Vector2(x+21f,floor+4f); so.ApplyModifiedProperties(); }
        Label(root,"PUSH CRATE ONTO PLATE",new Vector3(x+4f,floor+2.3f,1.5f));
        Label(root,"LASER INTERLOCK",new Vector3(x+10.5f,floor+5f,1.5f));
        Label(root,"UPPER THERMAL TRIAL",new Vector3(x+16f,floor+4.5f,1.5f));
        Undo.CollapseUndoOperations(undo);
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
        Selection.activeGameObject=root.gameObject;
        Debug.Log("GAME_SYSTEMS_PUZZLE_OK: scene saved. Backup: "+backup);
    }
    private static void MoveHazard(string name,Vector3 pos)
    {
        var go=GameObject.Find(name); if(go==null)return;
        Undo.RecordObject(go.transform,"Place hazard on terrace"); go.transform.position=pos;
    }
    private static Material Mat(string name,Color color)
    {
        string path="Assets/Materials/Hazards/"+name+".mat";
        var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(mat==null) {mat=new Material(Shader.Find("Unlit/Color"));mat.color=color;AssetDatabase.CreateAsset(mat,path);}
        return mat;
    }
    private static GameObject Box(Transform parent,string name,Vector3 pos,Vector3 size,Material mat,bool solid)
    {
        var go=GameObject.CreatePrimitive(PrimitiveType.Cube); go.name=name;
        go.transform.position=pos; go.transform.localScale=size; go.transform.SetParent(parent,true);
        go.GetComponent<Renderer>().sharedMaterial=mat;
        if(!solid) Object.DestroyImmediate(go.GetComponent<Collider>());
        return go;
    }
    private static void Platform(Transform root,string name,float left,float right,float top)
    {
        Box(root,name,new Vector3((left+right)/2,top-.25f,0),new Vector3(right-left,.5f,2.6f),metal,true);
        Box(root,name+" edge",new Vector3((left+right)/2,top-.04f,-1.32f),new Vector3(right-left,.05f,.05f),gold,false);
    }
    private static Transform Device(Transform root,string name,Vector3 pos,out Renderer light)
    {
        var device=new GameObject(name).transform; device.SetParent(root); device.position=pos;
        Box(device,"Device casing",pos+Vector3.forward*.2f,new Vector3(.55f,.55f,.4f),dark,false);
        var lens=GameObject.CreatePrimitive(PrimitiveType.Sphere); lens.name="Device lens";
        lens.transform.position=pos; lens.transform.localScale=Vector3.one*.26f; lens.transform.SetParent(device,true);
        Object.DestroyImmediate(lens.GetComponent<Collider>()); light=lens.GetComponent<Renderer>(); light.sharedMaterial=red;
        return device;
    }
    private static void Extend(string name,float right)
    {
        var go=GameObject.Find(name); if(go==null)return;
        var t=go.transform; Undo.RecordObject(t,"Extend enclosure");
        float left=t.position.x-t.lossyScale.x*.5f;
        var p=t.position;p.x=(left+right)*.5f;t.position=p;
        var s=t.localScale;s.x=(right-left)/(t.parent==null?1f:t.parent.lossyScale.x);t.localScale=s;
    }
    private static void Set(Object obj,string key,Object value)
    {var so=new SerializedObject(obj);so.FindProperty(key).objectReferenceValue=value;so.ApplyModifiedProperties();}
    private static void Set(Object obj,string key,int value)
    {var so=new SerializedObject(obj);so.FindProperty(key).intValue=value;so.ApplyModifiedProperties();}
    private static void Label(Transform root,string text,Vector3 pos)
    {var t=new GameObject(text).AddComponent<TextMesh>();t.transform.SetParent(root);t.transform.position=pos;t.text=text;t.characterSize=.045f;t.fontSize=64;t.anchor=TextAnchor.MiddleCenter;t.color=Color.white;}
}

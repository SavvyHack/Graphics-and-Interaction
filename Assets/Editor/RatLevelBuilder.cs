using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>Creates ordinary editable scene objects; no runtime geometry generation.</summary>
public static class RatLevelBuilder
{
    private const string Folder = PrototypeAssetPaths.Scenes;
    private static Transform root, section;
    private static Material steel, navy, pale, cyan, orange, red, dark, rat, pink;
    private static RatTrialSession session;

    [MenuItem("Project R.A.T./Create Prototype Level")]
    public static void Create()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Directory.CreateDirectory(Folder);
        Directory.CreateDirectory(PrototypeAssetPaths.EnvironmentMaterials);
        Directory.CreateDirectory(PrototypeAssetPaths.CharacterMaterials);
        AssetDatabase.Refresh();
        steel = Mat("Steel", "#3E576E"); navy = Mat("Structure", "#182633");
        pale = Mat("Trim", "#8BB6B4"); cyan = Mat("Route light", "#6FE8FF", true);
        orange = Mat("Caution", "#F28C28", true); red = Mat("Hazard", "#FF4B4B", true);
        dark = Mat("Recess", "#182733"); rat = Mat("Rat ivory", "#FAFAF3"); pink = Mat("Rat ears", "#CE9290");
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        root = new GameObject("Observation Enclosure - Prototype 01").transform;
        section = root;
        session = new GameObject("Playtest Session").AddComponent<RatTrialSession>();
        session.transform.SetParent(root);
        Group("00 - Enclosure");
        Box("Lower enclosure rail", new Vector3(40.5f, -2.8f, -.5f), new Vector3(89, .3f, 4), navy, false);
        Box("Top enclosure rail", new Vector3(40.5f, 9.8f, 0), new Vector3(89, .3f, 5), navy, false);
        Box("Start boundary", new Vector3(-3.8f, 3, 0), new Vector3(.4f, 14, 4), navy);
        Box("Exit boundary", new Vector3(84.8f, 3, 0), new Vector3(.4f, 14, 4), navy);
        Material glass = AssetDatabase.LoadAssetAtPath<Material>(PrototypeAssetPaths.GlassMaterial);
        if (glass != null)
        {
            string glassPath = PrototypeAssetPaths.LevelMaterial("Observation glass");
            Material observation = AssetDatabase.LoadAssetAtPath<Material>(glassPath);
            if (observation == null) { observation = new Material(glass); AssetDatabase.CreateAsset(observation, glassPath); }
            EditorUtility.SetDirty(observation);
            Box("Front observation glass", new Vector3(40.5f, 3.2f, -2.3f), new Vector3(88, 12.8f, .025f), observation, false);
        }
        Zone("Fall catch", new Vector3(40.5f, -3.2f, 0), new Vector3(90, .6f, 5), TrialZone.ZoneType.Hazard);

        Group("01 - Acclimation / forgiving jumps");
        Platform("Release deck", -3.5f, 5, 0);
        Platform("Jump 1", 6.5f, 9.5f, .4f);
        Platform("Jump 2", 11, 14, .8f);
        Platform("Ramp approach", 15.4f, 19, .8f);
        Water(5, 6.5f); Water(9.5f, 11); Water(14, 15.4f);
        Label("01   ACCLIMATION", 1f, 5.6f);
        Label("SHORT JUMPS  /  FOLLOW THE PLATFORM EDGES", 4.5f, 4.7f, .045f);
        Door("Release hatch", -2f, 0, false);
        session.startPoint = Point("Start", new Vector3(0, .08f, 0));
        Checkpoint(1, 17, .8f);

        Group("02 - Habitat conduit / ramp and tube");
        // Twenty-six degrees, below the controller's 45-degree slope limit.
        GameObject ramp = Box("Continuous ramp", new Vector3(21, 1.55f, 0), new Vector3(4.5f, .3f, 2.6f), steel);
        ramp.transform.rotation = Quaternion.Euler(0, 0, 23.6f);
        Platform("Tube walkway", 23, 28, 2.6f);
        for (float x = 23.3f; x < 28; x += .9f)
        {
            Box("Tube rear rib", new Vector3(x, 3.8f, .95f), new Vector3(.12f, 2.4f, .12f), pale, false);
            Box("Tube roof rib", new Vector3(x, 5, 0), new Vector3(.12f, .12f, 2.1f), pale, false);
        }
        Box("Conduit roof", new Vector3(25.5f, 5.15f, 0), new Vector3(5.2f, .2f, 2.6f), steel);
        Platform("Wheel approach", 28, 31, 1.4f);
        Label("02   HABITAT CONDUIT", 23f, 6.2f);
        Checkpoint(2, 29.4f, 1.4f);

        Group("03 - Wheel / observe then cross");
        Platform("Wheel runway", 31, 37, 1.4f);
        Transform wheel = Point("Rotating sweep", new Vector3(33.8f, 3.1f, .25f));
        wheel.gameObject.AddComponent<TrialWheel>();
        var wheelBody = wheel.gameObject.AddComponent<Rigidbody>();
        wheelBody.isKinematic = true; wheelBody.useGravity = false;
        // Decorative rim sits behind the movement plane. Only the two bright sweep arms are hazards.
        for (int i = 0; i < 36; i++)
        {
            float angle = i * 10 * Mathf.Deg2Rad;
            GameObject segment = Box("Wheel rim", wheel.position + new Vector3(Mathf.Cos(angle) * 1.8f, Mathf.Sin(angle) * 1.8f, 1.1f), new Vector3(.34f, .12f, .18f), orange, false);
            segment.transform.rotation = Quaternion.Euler(0, 0, i * 10 + 90);
            segment.transform.SetParent(wheel, true);
        }
        for (int i = 0; i < 2; i++)
        {
            GameObject arm = Box("Sweep arm", wheel.position + new Vector3(0, i == 0 ? .9f : -.9f, 0), new Vector3(.12f, 1.55f, .9f), orange, false);
            arm.transform.SetParent(wheel, true);
            BoxCollider collider = arm.AddComponent<BoxCollider>(); collider.isTrigger = true;
            TrialZone hazard = arm.AddComponent<TrialZone>(); hazard.session = session;
        }
        Label("03   ROTATION TRIAL", 33.6f, 6.2f);
        Label("CROSS WHEN CLEAR", 33.6f, 5.4f, .04f);
        Checkpoint(3, 36.1f, 1.4f);

        Group("04 - Shuttle / ride the moving platform");
        Water(37, 44);
        GameObject shuttle = Platform("Shuttle platform", 37, 39.5f, 1.4f);
        shuttle.AddComponent<TrialMovingPlatform>().travel = new Vector3(4.5f, 0, 0);
        Box("Shuttle guide rail", new Vector3(40.5f, .55f, 1.6f), new Vector3(7.5f, .12f, .12f), pale, false);
        Platform("Shuttle landing", 44, 48, 1.4f);
        Label("04   TRANSFER", 40.5f, 6.2f);
        Label("BOARD. RIDE. STEP OFF.", 40.5f, 5.3f, .04f);
        Checkpoint(4, 46, 1.4f);

        Group("05 - Ascent / combine the jumps");
        Platform("Ascent step 1", 49.4f, 52, 2.3f);
        Platform("Ascent step 2", 53.4f, 56, 3.2f);
        Platform("Exit deck", 57.4f, 63.5f, 4.1f);
        Water(48, 57.4f);
        Door("Escape hatch", 61.3f, 4.1f, true);
        Zone("Exit trigger", new Vector3(61.3f, 5.2f, 0), new Vector3(1.3f, 2.2f, 2.4f), TrialZone.ZoneType.Exit);
        Label("05   EXIT ASCENT", 54, 7f);
        Label("ESCAPE  >", 60.5f, 7.3f, .08f);

        Group("06 - Rats");
        Transform player = Rat("PlayerRat", session.startPoint.position, true);
        session.player = player.GetComponent<PlayerRatController>();
        session.waitingRats = new GameObject[] { Rat("Waiting rat 2", new Vector3(-1.5f, .02f, .5f), false).gameObject, Rat("Waiting rat 3", new Vector3(-2.5f, .02f, .8f), false).gameObject };
        Camera camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
        camera.tag = "MainCamera";
        camera.transform.position = new Vector3(5, 3.5f, -22);
        camera.orthographic = true; camera.orthographicSize = 5.4f;
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Color.black;
        FixedCameraFollow follow = camera.gameObject.AddComponent<FixedCameraFollow>();
        Set(follow, "target", player); Set(follow, "followOffset", new Vector2(2.5f, 1.8f));
        Set(follow, "clampToBounds", true); Set(follow, "minBounds", new Vector2(5f, 3.5f));
        Set(follow, "maxBounds", new Vector2(79f, 4.8f)); session.cameraFollow = follow;
        Light light = new GameObject("Cool key light").AddComponent<Light>();
        light.type = LightType.Directional; light.intensity = 1.2f;
        light.transform.rotation = Quaternion.Euler(35, -25, 0);
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(.55f, .65f, .74f);
        RenderSettings.fog = false;
        ObservationStyle.ApplyToOpenScene();
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), PrototypeAssetPaths.PrototypeScene);
        // Preserve StartScene; select this complete level as the playable build entry.
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(PrototypeAssetPaths.PrototypeScene, true), new EditorBuildSettingsScene(PrototypeAssetPaths.StarterScene, false) };
        AssetDatabase.SaveAssets();
        Validate();
        string output = Environment.GetEnvironmentVariable("RAT_OUTPUT");
        if (!string.IsNullOrEmpty(output))
        {
            Directory.CreateDirectory(output);
            Capture(camera, Path.Combine(output, "level-start.png"), new Vector3(5, 3.5f, -22), 5.4f);
            Capture(camera, Path.Combine(output, "level-overview.png"), new Vector3(30, 3.2f, -22), 7.6f, 3200, 700);
            Export();
        }
        Debug.Log("RAT_LEVEL_BUILD_OK");
    }

    public static void Export()
    {
        string output = Environment.GetEnvironmentVariable("RAT_OUTPUT");
        if (string.IsNullOrEmpty(output)) throw new Exception("Set RAT_OUTPUT to the delivery folder.");
        AssetDatabase.ExportPackage(new[] { Folder, PrototypeAssetPaths.Materials, "Assets/Editor", "Assets/Scripts", "Assets/Shaders" }, Path.Combine(output, "Rat-Enclosure.unitypackage"), ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);
        Debug.Log("RAT_EXPORT_OK");
    }

    private static Material Mat(string name, string hex, bool emission = false)
    {
        string path = PrototypeAssetPaths.LevelMaterial(name);
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null) { mat = new Material(Shader.Find("Standard")); AssetDatabase.CreateAsset(mat, path); }
        ColorUtility.TryParseHtmlString(hex, out Color color); mat.color = color;
        mat.SetFloat("_Glossiness", .3f);
        if (emission) { mat.EnableKeyword("_EMISSION"); mat.SetColor("_EmissionColor", color * .6f); }
        EditorUtility.SetDirty(mat); return mat;
    }
    private static void Group(string name) { section = new GameObject(name).transform; section.SetParent(root); }
    private static Transform Point(string name, Vector3 position) { var p = new GameObject(name).transform; p.SetParent(section); p.position = position; return p; }
    private static GameObject Box(string name, Vector3 position, Vector3 size, Material material, bool solid = true)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name; box.transform.SetParent(section); box.transform.position = position; box.transform.localScale = size;
        box.GetComponent<Renderer>().sharedMaterial = material;
        if (!solid) UnityEngine.Object.DestroyImmediate(box.GetComponent<Collider>());
        return box;
    }
    private static GameObject Platform(string name, float left, float right, float top)
    {
        GameObject p = Box(name, new Vector3((left + right) / 2, top - .25f, 0), new Vector3(right - left, .5f, 2.6f), steel);
        GameObject trim = Box("Illuminated walkable edge", new Vector3((left + right) / 2, top - .06f, -1.32f), new Vector3(right - left - .14f, .055f, .04f), cyan, false);
        trim.transform.SetParent(p.transform, true);
        GameObject brace = Box("Underside brace", new Vector3((left + right) / 2, top - .61f, .3f), new Vector3((right - left) * .75f, .22f, 1.7f), navy, false);
        brace.transform.SetParent(p.transform, true);
        return p;
    }
    private static void Water(float left, float right)
    {
        Box("Electric runoff", new Vector3((left + right) / 2, -1.9f, 0), new Vector3(right - left, .13f, 2.7f), cyan, false);
        Zone("Runoff hazard", new Vector3((left + right) / 2, -2f, 0), new Vector3(right - left, .4f, 3), TrialZone.ZoneType.Hazard);
    }
    private static TrialZone Zone(string name, Vector3 pos, Vector3 size, TrialZone.ZoneType type)
    {
        Transform t = Point(name, pos); BoxCollider collider = t.gameObject.AddComponent<BoxCollider>();
        collider.size = size; collider.isTrigger = true;
        TrialZone zone = t.gameObject.AddComponent<TrialZone>(); zone.type = type; zone.session = session; return zone;
    }
    private static void Checkpoint(int number, float x, float y)
    {
        Transform spawn = Point("Checkpoint " + number + " respawn", new Vector3(x, y + .08f, 0));
        TrialZone zone = Zone("Checkpoint " + number, new Vector3(x, y + 1, 0), new Vector3(.7f, 2, 2.5f), TrialZone.ZoneType.Checkpoint);
        zone.checkpointNumber = number; zone.respawn = spawn;
        Box("Checkpoint beacon", new Vector3(x, y + .8f, 1f), new Vector3(.12f, 1.6f, .12f), cyan, false);
        Label("CP " + number, x, y + 2.1f, .09f);
    }
    private static void Door(string name, float x, float y, bool exit)
    {
        Box(name, new Vector3(x, y + 1.35f, 1.8f), new Vector3(1.9f, 2.7f, .35f), dark, false);
        Box("Hatch indicator", new Vector3(x, y + 2.7f, 1.55f), new Vector3(1.5f, .12f, .08f), exit ? cyan : orange, false);
        Box("Door handle", new Vector3(x + .5f, y + 1.3f, 1.5f), new Vector3(.06f, .55f, .06f), pale, false);
    }
    private static void Label(string text, float x, float y, float size = .065f)
    {
        TextMesh label = Point(text, new Vector3(x, y, 1.9f)).gameObject.AddComponent<TextMesh>();
        label.text = text; label.characterSize = size; label.fontSize = 64;
        label.anchor = TextAnchor.MiddleCenter; label.color = new Color(.78f, .89f, .93f);
    }
    private static Transform Rat(string name, Vector3 position, bool playable)
    {
        Transform actor = Point(name, position);
        Transform previous = section; section = actor;
        Transform model = Point("Model", position + new Vector3(0, .42f, 0));
        // Model points along local +Z, matching the existing controller's +/-90 degree facing.
        Shape("Body", model, new Vector3(0, 0, 0), new Vector3(.48f, .5f, .85f), rat);
        Shape("Head", model, new Vector3(0, .08f, .43f), new Vector3(.37f, .38f, .4f), rat);
        Shape("Nose", model, new Vector3(0, .04f, .65f), Vector3.one * .1f, pink);
        for (int sign = -1; sign <= 1; sign += 2)
        {
            Shape("Ear", model, new Vector3(sign * .17f, .3f, .32f), new Vector3(.16f, .22f, .09f), pink);
            Shape("Eye", model, new Vector3(sign * .17f, .16f, .47f), Vector3.one * .055f, dark);
            Shape("Foot", model, new Vector3(sign * .21f, -.33f, .22f), new Vector3(.13f, .1f, .25f), pink);
            Shape("Foot", model, new Vector3(sign * .21f, -.33f, -.27f), new Vector3(.13f, .1f, .25f), pink);
        }
        for (int i = 0; i < 6; i++) Shape("Tail", model, new Vector3(Mathf.Sin(i * .5f) * .12f, -.14f, -.45f - i * .12f), new Vector3(.08f - i * .008f, .07f, .19f), pink);
        model.localRotation = Quaternion.Euler(0, 90, 0);
        if (playable)
        {
            CharacterController controller = actor.gameObject.AddComponent<CharacterController>();
            controller.height = .9f; controller.radius = .27f; controller.center = new Vector3(0, .45f, 0);
            controller.stepOffset = .22f; controller.skinWidth = .025f; controller.minMoveDistance = 0f;
            Transform feet = Point("GroundCheck", position + new Vector3(0, .1f, 0));
            PlayerRatController movement = actor.gameObject.AddComponent<PlayerRatController>();
            actor.gameObject.layer = 2; // Exclude self from ground queries.
            Set(movement, "model", model); Set(movement, "groundCheck", feet); Set(movement, "groundLayers", 1);
            Set(movement, "groundCheckRadius", .13f);
        }
        section = previous; return actor;
    }
    private static void Shape(string name, Transform parent, Vector3 local, Vector3 scale, Material material)
    {
        GameObject shape = GameObject.CreatePrimitive(PrimitiveType.Sphere); shape.name = name;
        UnityEngine.Object.DestroyImmediate(shape.GetComponent<Collider>());
        shape.transform.SetParent(parent, false); shape.transform.localPosition = local; shape.transform.localScale = scale;
        shape.GetComponent<Renderer>().sharedMaterial = material;
    }
    private static void Set(UnityEngine.Object target, string field, object value)
    {
        SerializedObject obj = new SerializedObject(target); SerializedProperty p = obj.FindProperty(field);
        if (value is UnityEngine.Object o) p.objectReferenceValue = o;
        else if (value is Vector2 v) p.vector2Value = v;
        else if (value is bool b) p.boolValue = b;
        else if (value is int i) p.intValue = i;
        else if (value is float f) p.floatValue = f;
        obj.ApplyModifiedPropertiesWithoutUndo();
    }
    public static void Validate()
    {
        Physics.SyncTransforms();
        foreach (TrialZone zone in UnityEngine.Object.FindObjectsByType<TrialZone>(FindObjectsSortMode.None))
        {
            if (zone.session == null) throw new Exception("Unwired zone: " + zone.name);
            if (zone.type != TrialZone.ZoneType.Checkpoint) continue;
            if (zone.respawn == null || !Physics.Raycast(zone.respawn.position + Vector3.up * .15f, Vector3.down, .5f, 1, QueryTriggerInteraction.Ignore))
                throw new Exception("Checkpoint lacks safe floor: " + zone.name);
        }
        if (UnityEngine.Object.FindObjectsByType<PlayerRatController>(FindObjectsSortMode.None).Length != 1) throw new Exception("Expected one playable rat.");
        Debug.Log("RAT_LEVEL_VALIDATION_OK: checkpoint floors, zone references, player count.");
    }
    private static void Capture(Camera camera, string file, Vector3 position, float size, int width = 1600, int height = 900)
    {
        Vector3 previous = camera.transform.position; float previousSize = camera.orthographicSize;
        camera.transform.position = position; camera.orthographicSize = size;
        RenderTexture target = new RenderTexture(width, height, 24);
        camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
        Texture2D image = new Texture2D(width, height, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, width, height), 0, 0); image.Apply(); File.WriteAllBytes(file, image.EncodeToPNG());
        camera.targetTexture = null; RenderTexture.active = null;
        UnityEngine.Object.DestroyImmediate(image); UnityEngine.Object.DestroyImmediate(target);
        camera.transform.position = previous; camera.orthographicSize = previousSize;
    }
}

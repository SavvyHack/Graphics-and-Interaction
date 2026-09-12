#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// One-click wiring for Kavish's game systems, original SFX and animated
/// hazard shader. It converts the existing RatTrialSession/TrialZone harness
/// in RatEnclosure into the team-facing GameManager/RatLifeManager triggers.
///
/// Run: Project R.A.T. > Kavish > Install or Refresh Kavish Systems
/// </summary>
public static class KavishIntegrationSetup
{
    private const string ScenePath = "Assets/Scenes/RatEnclosure.unity";
    private const string HazardShaderPath = "Assets/Shaders/KavishAnimatedEnergyHazard.shader";
    private const string HazardMaterialPath = "Assets/Materials/Environment/KavishEnergyHazard.mat";
    private const string FireShaderPath = "Assets/Shaders/KavishFirePlume.shader";
    private const string LaserShaderPath = "Assets/Shaders/KavishLaserPulse.shader";
    private const string FireMaterialPath = "Assets/Materials/Hazards/KavishFireHazard.mat";
    private const string GateMaterialPath = "Assets/Materials/Hazards/KavishElectricGate.mat";
    private const string LaserMaterialPath = "Assets/Materials/Hazards/KavishLaserBarrier.mat";
    private const string HazardPrefabFolder = "Assets/Prefabs/Hazards";
    private const string HazardCourseName = "07 - Kavish Hazard Course (Assets 09-11)";

    [MenuItem("Project R.A.T./Kavish/Install or Refresh Kavish Systems")]
    public static void Install()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("[Kavish setup] Exit Play mode before refreshing the saved level.");
            return;
        }
        AssetDatabase.Refresh();
        if (!TryGetEditableScene(out Scene scene)) return;

        GameObject sessionObject = GameObject.Find("Playtest Session");
        PlayerRatController player = Object.FindFirstObjectByType<PlayerRatController>();
        FixedCameraFollow cameraFollow = Object.FindFirstObjectByType<FixedCameraFollow>();
        GameObject start = GameObject.Find("Start");
        GameObject waiting2 = GameObject.Find("Waiting rat 2");
        GameObject waiting3 = GameObject.Find("Waiting rat 3");

        if (sessionObject == null || player == null || start == null)
        {
            Debug.LogError("[Kavish setup] Expected prototype objects were not found. " +
                           "Need: Playtest Session, PlayerRat and Start.");
            return;
        }

        // Disable the temporary harness but leave it in the scene so the team
        // can restore it if required for comparison/testing.
        RatTrialSession trialSession = sessionObject.GetComponent<RatTrialSession>();
        if (trialSession != null)
            trialSession.enabled = false;

        GameManager gameManager = GetOrAdd<GameManager>(sessionObject);
        RatLifeManager lifeManager = GetOrAdd<RatLifeManager>(sessionObject);
        AudioSource audioSource = GetOrAdd<AudioSource>(sessionObject);
        AudioManager audioManager = GetOrAdd<AudioManager>(sessionObject);

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        ConfigureLifeManager(lifeManager, player, start.transform, cameraFollow, waiting2, waiting3);
        ConfigureGameManager(gameManager, lifeManager);
        ConfigureAudio(audioManager, audioSource);
        ConvertTrialZones(lifeManager);
        ApplyHazardMaterial();
        // Refresh is wiring-only. Geometry, transforms, cycle settings and
        // prefab assets belong to the level designer and must be preserved.

        EditorUtility.SetDirty(sessionObject);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("KAVISH_SETUP_OK: gameplay and audio wiring refreshed; current level layout preserved.");
    }

    private static bool TryGetEditableScene(out Scene scene)
    {
        // Reuse the loaded scene, including unsaved position/platform edits.
        // Reopening it from disk here would discard those edits.
        scene = SceneManager.GetSceneByPath(ScenePath);
        if (scene.IsValid() && scene.isLoaded)
        {
            SceneManager.SetActiveScene(scene);
            return true;
        }
        if (!Application.isBatchMode &&
            !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return false;
        scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        return true;
    }

    [MenuItem("Project R.A.T./Kavish/Rebuild Default Hazard Course (Resets Layout)")]
    public static void RebuildDefaultHazardCourse()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (!Application.isBatchMode && !EditorUtility.DisplayDialog(
            "Rebuild default hazard course?",
            "This replaces the generated hazard section and its prefabs, and resets enclosure and camera bounds. Save a separate scene copy first if you need your custom layout.",
            "Rebuild", "Cancel")) return;
        if (!TryGetEditableScene(out Scene scene)) return;
        RatLifeManager life = Object.FindFirstObjectByType<RatLifeManager>();
        if (life == null)
        {
            Debug.LogError("[Kavish setup] Run Install or Refresh Kavish Systems first.");
            return;
        }
        ExtendLevelAndInstallHazards(scene, life, Object.FindFirstObjectByType<FixedCameraFollow>());
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("KAVISH_REBUILD_OK: default hazard course rebuilt.");
    }

    [MenuItem("Project R.A.T./Kavish/Validate Kavish Systems")]
    public static void Validate()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != ScenePath)
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        GameManager gm = Object.FindFirstObjectByType<GameManager>();
        RatLifeManager life = Object.FindFirstObjectByType<RatLifeManager>();
        AudioManager audio = Object.FindFirstObjectByType<AudioManager>();
        HazardTrigger[] hazards = Object.FindObjectsByType<HazardTrigger>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Checkpoint[] checkpoints = Object.FindObjectsByType<Checkpoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        ExitTrigger[] exits = Object.FindObjectsByType<ExitTrigger>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        KavishTimedHazard[] timedHazards = Object.FindObjectsByType<KavishTimedHazard>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        int electricObjects = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Count(r => r.gameObject.name == "Electric runoff" &&
                        r.sharedMaterial != null &&
                        r.sharedMaterial.shader != null &&
                        r.sharedMaterial.shader.name == "ProjectRAT/Kavish/AnimatedEnergyHazard");

        bool modularHazardsPresent = GameObject.Find("09 - Fire Emitter") != null &&
                                     GameObject.Find("10 - Electric Gate") != null &&
                                     GameObject.Find("11 - Laser Barrier") != null;
        bool extendedExitPresent = exits.Any(exit => exit.transform.position.x > 80f);
        bool shadersPresent = Shader.Find("ProjectRAT/Kavish/FirePlume") != null &&
                              Shader.Find("ProjectRAT/Kavish/AnimatedEnergyHazard") != null &&
                              Shader.Find("ProjectRAT/Kavish/LaserPulse") != null;
        SerializedObject audioObject = audio != null ? new SerializedObject(audio) : null;
        bool jumpAudioPresent = audioObject != null && audioObject.FindProperty("jumpClip").objectReferenceValue != null;

        bool ok = gm != null && life != null && audio != null && hazards.Length > 0 &&
                  checkpoints.Length >= 5 && exits.Length > 0 && electricObjects > 0 &&
                  timedHazards.Length == 3 && modularHazardsPresent && extendedExitPresent &&
                  shadersPresent && jumpAudioPresent;

        if (ok)
            Debug.Log($"KAVISH_VALIDATION_OK: extended exit, {timedHazards.Length} modular hazards, {hazards.Length} hazard triggers, {checkpoints.Length} checkpoints, three hazard shaders and jump SFX.");
        else
            Debug.LogError($"KAVISH_VALIDATION_FAILED: GameManager={gm != null}, RatLifeManager={life != null}, AudioManager={audio != null}, hazards={hazards.Length}, checkpoints={checkpoints.Length}, exits={exits.Length}, timedHazards={timedHazards.Length}, modularHazards={modularHazardsPresent}, extendedExit={extendedExitPresent}, shaders={shadersPresent}, jumpAudio={jumpAudioPresent}, animatedRunoff={electricObjects}.");
    }

    [MenuItem("Project R.A.T./Kavish/Restore Original Trial Harness")]
    public static void RestoreHarness()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        RatTrialSession session = Object.FindFirstObjectByType<RatTrialSession>(FindObjectsInactive.Include);
        if (session != null)
            session.enabled = true;

        foreach (TrialZone zone in Object.FindObjectsByType<TrialZone>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            zone.enabled = true;

        foreach (GameManager component in Object.FindObjectsByType<GameManager>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            component.enabled = false;
        foreach (RatLifeManager component in Object.FindObjectsByType<RatLifeManager>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            component.enabled = false;
        foreach (HazardTrigger component in Object.FindObjectsByType<HazardTrigger>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            component.enabled = false;
        foreach (Checkpoint component in Object.FindObjectsByType<Checkpoint>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            component.enabled = false;
        foreach (ExitTrigger component in Object.FindObjectsByType<ExitTrigger>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            component.enabled = false;

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("KAVISH_RESTORE_OK: original RatTrialSession/TrialZone harness is enabled again.");
    }

    private static T GetOrAdd<T>(GameObject gameObject) where T : Component
    {
        T existing = gameObject.GetComponent<T>();
        return existing != null ? existing : Undo.AddComponent<T>(gameObject);
    }

    private static void ConfigureLifeManager(
        RatLifeManager lifeManager,
        PlayerRatController player,
        Transform start,
        FixedCameraFollow cameraFollow,
        GameObject waiting2,
        GameObject waiting3)
    {
        SerializedObject so = new SerializedObject(lifeManager);
        so.FindProperty("player").objectReferenceValue = player;
        so.FindProperty("startPoint").objectReferenceValue = start;
        so.FindProperty("cameraFollow").objectReferenceValue = cameraFollow;

        SerializedProperty waiting = so.FindProperty("waitingRats");
        waiting.arraySize = 2;
        waiting.GetArrayElementAtIndex(0).objectReferenceValue = waiting2;
        waiting.GetArrayElementAtIndex(1).objectReferenceValue = waiting3;
        so.ApplyModifiedPropertiesWithoutUndo();
        lifeManager.enabled = true;
    }

    private static void ConfigureGameManager(GameManager manager, RatLifeManager lifeManager)
    {
        SerializedObject so = new SerializedObject(manager);
        so.FindProperty("lifeManager").objectReferenceValue = lifeManager;
        so.ApplyModifiedPropertiesWithoutUndo();
        manager.enabled = true;
    }

    private static void ConfigureAudio(AudioManager manager, AudioSource source)
    {
        SerializedObject so = new SerializedObject(manager);
        so.FindProperty("audioSource").objectReferenceValue = source;
        so.FindProperty("jumpClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Kavish_Jump.wav");
        so.FindProperty("checkpointClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Kavish_Checkpoint.wav");
        so.FindProperty("ratLostClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Kavish_RatLost.wav");
        so.FindProperty("completionClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Kavish_Completion.wav");
        so.FindProperty("failureClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Kavish_Failure.wav");
        so.ApplyModifiedPropertiesWithoutUndo();
        manager.enabled = true;
    }

    private static void ConvertTrialZones(RatLifeManager lifeManager)
    {
        TrialZone[] zones = Object.FindObjectsByType<TrialZone>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (TrialZone zone in zones)
        {
            GameObject go = zone.gameObject;

            switch (zone.type)
            {
                case TrialZone.ZoneType.Hazard:
                {
                    HazardTrigger hazard = GetOrAdd<HazardTrigger>(go);
                    SerializedObject so = new SerializedObject(hazard);
                    so.FindProperty("lifeManager").objectReferenceValue = lifeManager;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    hazard.enabled = true;
                    break;
                }
                case TrialZone.ZoneType.Checkpoint:
                {
                    Checkpoint checkpoint = GetOrAdd<Checkpoint>(go);
                    SerializedObject so = new SerializedObject(checkpoint);
                    so.FindProperty("checkpointNumber").intValue = zone.checkpointNumber;
                    so.FindProperty("respawnPoint").objectReferenceValue = zone.respawn;
                    so.FindProperty("lifeManager").objectReferenceValue = lifeManager;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    checkpoint.enabled = true;
                    break;
                }
                case TrialZone.ZoneType.Exit:
                {
                    ExitTrigger exit = GetOrAdd<ExitTrigger>(go);
                    exit.enabled = true;
                    break;
                }
            }

            zone.enabled = false;
            EditorUtility.SetDirty(go);
        }
    }

    private static void ApplyHazardMaterial()
    {
        Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(HazardShaderPath);
        if (shader == null)
        {
            Debug.LogError("[Kavish setup] Animated hazard shader was not imported: " + HazardShaderPath);
            return;
        }

        Material material = AssetDatabase.LoadAssetAtPath<Material>(HazardMaterialPath);
        if (material == null)
        {
            material = new Material(shader)
            {
                name = "KavishEnergyHazard"
            };
            material.SetColor("_BaseColor", new Color(0.03f, 0.22f, 0.32f, 0.82f));
            material.SetColor("_EnergyColor", new Color(0.44f, 0.91f, 1.0f, 1f));
            material.SetFloat("_Speed", 2.4f);
            material.SetFloat("_Scale", 12f);
            material.SetFloat("_StripeWidth", 0.16f);
            material.SetFloat("_Intensity", 1.7f);
            material.SetFloat("_WaveHeight", 0.035f);
            material.SetFloat("_WaveFrequency", 7f);
            material.SetFloat("_Alpha", 0.86f);
            AssetDatabase.CreateAsset(material, HazardMaterialPath);
        }
        else if (material.shader != shader)
        {
            material.shader = shader;
            EditorUtility.SetDirty(material);
        }

        Renderer[] renderers = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Renderer renderer in renderers)
        {
            if (renderer.gameObject.name != "Electric runoff")
                continue;

            renderer.sharedMaterial = material;
            EditorUtility.SetDirty(renderer);
        }
    }

    private static void ExtendLevelAndInstallHazards(Scene scene, RatLifeManager lifeManager, FixedCameraFollow cameraFollow)
    {
        Transform root = GameObject.Find("Observation Enclosure - Prototype 01")?.transform;
        Transform exitSection = GameObject.Find("05 - Ascent / combine the jumps")?.transform;
        if (root == null || exitSection == null)
        {
            Debug.LogError("[Kavish setup] Cannot extend the level: root or section 05 was not found.");
            return;
        }

        // The original exit sits at X=61.3. Move all direct exit props and the
        // trigger to the far end exactly once, leaving the original deck intact.
        GameObject exitTrigger = GameObject.Find("Exit trigger");
        if (exitTrigger != null && exitTrigger.transform.position.x < 70f)
        {
            for (int index = 0; index < exitSection.childCount; index++)
            {
                Transform item = exitSection.GetChild(index);
                if (item.name != "Exit deck" && item.position.x > 60f)
                    item.position += Vector3.right * 21.5f;
            }
        }

        ResizeBox("Lower enclosure rail", new Vector3(40.5f, -2.8f, -0.5f), new Vector3(89f, 0.3f, 4f));
        ResizeBox("Top enclosure rail", new Vector3(40.5f, 9.8f, 0f), new Vector3(89f, 0.3f, 5f));
        ResizeBox("Front observation glass", new Vector3(40.5f, 3.2f, -2.3f), new Vector3(88f, 12.8f, 0.025f));
        ResizeBox("Exit boundary", new Vector3(84.8f, 3f, 0f), new Vector3(0.4f, 14f, 4f));

        GameObject fallCatch = GameObject.Find("Fall catch");
        if (fallCatch != null)
        {
            fallCatch.transform.position = new Vector3(40.5f, -3.2f, 0f);
            BoxCollider fallCollider = fallCatch.GetComponent<BoxCollider>();
            if (fallCollider != null)
                fallCollider.size = new Vector3(90f, 0.6f, 5f);
        }

        if (cameraFollow != null)
        {
            SerializedObject cameraObject = new SerializedObject(cameraFollow);
            cameraObject.FindProperty("maxBounds").vector2Value = new Vector2(79f, 4.8f);
            cameraObject.ApplyModifiedPropertiesWithoutUndo();
        }

        Transform oldCourse = root.Find(HazardCourseName);
        if (oldCourse != null)
            Undo.DestroyObjectImmediate(oldCourse.gameObject);

        Transform course = new GameObject(HazardCourseName).transform;
        Undo.RegisterCreatedObjectUndo(course.gameObject, "Create Kavish hazard course");
        course.SetParent(root, false);

        Material steel = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Environment/Steel.mat");
        Material structure = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Environment/Structure.mat");
        Material trim = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Environment/Trim.mat");
        Material caution = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Environment/Caution.mat");
        Material red = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Environment/Hazard.mat");
        Material fire = EnsureMaterial(FireMaterialPath, FireShaderPath);
        Material gate = EnsureMaterial(GateMaterialPath, HazardShaderPath);
        Material laser = EnsureMaterial(LaserMaterialPath, LaserShaderPath);
        System.IO.Directory.CreateDirectory(HazardPrefabFolder);
        AssetDatabase.Refresh();

        CreatePlatform(course, "Extended test deck", 63.5f, 70.3f, 4.1f, steel, structure, trim);
        CreateRunoffGap(course, "Extended runoff gap A", 70.3f, 72f, lifeManager);
        CreatePlatform(course, "Gate landing", 72f, 77.5f, 4.1f, steel, structure, trim);
        CreateRunoffGap(course, "Extended runoff gap B", 77.5f, 79f, lifeManager);
        CreatePlatform(course, "Final escape deck", 79f, 84.5f, 4.1f, steel, structure, trim);

        CreateCheckpoint(course, lifeManager, 5, new Vector3(73f, 4.18f, 0f), trim);
        CreateFireEmitter(course, lifeManager, fire, steel, structure, caution);
        CreateElectricGate(course, lifeManager, gate, structure, caution);
        CreateLaserBarrier(course, lifeManager, laser, structure, red);
        CreateLabel(course, "06   HAZARD TESTING", new Vector3(72.5f, 7.7f, 1.9f), 0.065f);
        CreateLabel(course, "TIME THE FIRE, GATE AND LASERS", new Vector3(75f, 6.9f, 1.9f), 0.042f);

        LaboratoryBackdrop.Apply(scene);
    }

    private static Material EnsureMaterial(string materialPath, string shaderPath)
    {
        Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);
        Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (shader == null)
        {
            Debug.LogError("[Kavish setup] Shader was not imported: " + shaderPath);
            return material;
        }
        if (material == null)
        {
            material = new Material(shader) { name = System.IO.Path.GetFileNameWithoutExtension(materialPath) };
            AssetDatabase.CreateAsset(material, materialPath);
        }
        else if (material.shader != shader)
        {
            material.shader = shader;
            EditorUtility.SetDirty(material);
        }
        return material;
    }

    private static void ResizeBox(string name, Vector3 position, Vector3 scale)
    {
        GameObject item = GameObject.Find(name);
        if (item == null)
            return;
        item.transform.position = position;
        item.transform.localScale = scale;
        EditorUtility.SetDirty(item);
    }

    private static GameObject CreateBox(Transform parent, string name, Vector3 position, Vector3 scale, Material material, bool collider)
    {
        GameObject item = GameObject.CreatePrimitive(PrimitiveType.Cube);
        item.name = name;
        item.transform.position = position;
        item.transform.localScale = scale;
        item.transform.SetParent(parent, true);
        item.GetComponent<Renderer>().sharedMaterial = material;
        if (!collider)
            Object.DestroyImmediate(item.GetComponent<Collider>());
        return item;
    }

    private static GameObject CreateQuad(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject item = GameObject.CreatePrimitive(PrimitiveType.Quad);
        item.name = name;
        item.transform.position = position;
        item.transform.localScale = scale;
        item.transform.SetParent(parent, true);
        item.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(item.GetComponent<Collider>());
        return item;
    }

    private static GameObject CreateCylinder(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject item = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        item.name = name;
        item.transform.position = position;
        item.transform.localScale = scale;
        item.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        item.transform.SetParent(parent, true);
        item.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(item.GetComponent<Collider>());
        return item;
    }

    private static void CreatePlatform(Transform parent, string name, float left, float right, float top, Material steel, Material structure, Material trim)
    {
        float centre = (left + right) * 0.5f;
        GameObject platform = CreateBox(parent, name, new Vector3(centre, top - 0.25f, 0f), new Vector3(right - left, 0.5f, 2.6f), steel, true);
        CreateBox(platform.transform, "Illuminated walkable edge", new Vector3(centre, top - 0.06f, -1.32f), new Vector3(right - left - 0.14f, 0.055f, 0.04f), trim, false);
        CreateBox(platform.transform, "Underside brace", new Vector3(centre, top - 0.61f, 0.3f), new Vector3((right - left) * 0.75f, 0.22f, 1.7f), structure, false);
    }

    private static void CreateRunoffGap(Transform parent, string name, float left, float right, RatLifeManager lifeManager)
    {
        Material runoff = AssetDatabase.LoadAssetAtPath<Material>(HazardMaterialPath);
        float centre = (left + right) * 0.5f;
        CreateBox(parent, name + " surface", new Vector3(centre, -1.9f, 0f), new Vector3(right - left, 0.13f, 2.7f), runoff, false);
        CreateHazardTrigger(parent, name + " trigger", new Vector3(centre, -2f, 0f), new Vector3(right - left, 0.4f, 3f), lifeManager);
    }

    private static GameObject CreateHazardTrigger(Transform parent, string name, Vector3 position, Vector3 size, RatLifeManager lifeManager)
    {
        GameObject trigger = new GameObject(name);
        trigger.transform.SetParent(parent, false);
        trigger.transform.position = position;
        BoxCollider collider = trigger.AddComponent<BoxCollider>();
        collider.size = size;
        collider.isTrigger = true;
        HazardTrigger hazard = trigger.AddComponent<HazardTrigger>();
        SerializedObject hazardObject = new SerializedObject(hazard);
        hazardObject.FindProperty("lifeManager").objectReferenceValue = lifeManager;
        hazardObject.ApplyModifiedPropertiesWithoutUndo();
        return trigger;
    }

    private static void ConfigureCycle(GameObject owner, GameObject activeGroup, float activeDuration, float inactiveDuration, float phaseOffset)
    {
        KavishTimedHazard cycle = owner.AddComponent<KavishTimedHazard>();
        SerializedObject cycleObject = new SerializedObject(cycle);
        SerializedProperty activeObjects = cycleObject.FindProperty("activeObjects");
        activeObjects.arraySize = 1;
        activeObjects.GetArrayElementAtIndex(0).objectReferenceValue = activeGroup;
        cycleObject.FindProperty("activeDuration").floatValue = activeDuration;
        cycleObject.FindProperty("inactiveDuration").floatValue = inactiveDuration;
        cycleObject.FindProperty("phaseOffset").floatValue = phaseOffset;
        cycleObject.FindProperty("startActive").boolValue = true;
        cycleObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateFireEmitter(Transform parent, RatLifeManager lifeManager, Material fire, Material steel, Material structure, Material caution)
    {
        GameObject root = new GameObject("09 - Fire Emitter");
        root.transform.SetParent(parent, false);
        root.transform.position = new Vector3(67.9f, 4.72f, 0f);
        CreateBox(root.transform, "Emitter housing", new Vector3(67.9f, 4.72f, 0.85f), new Vector3(0.72f, 1.18f, 0.58f), structure, false);
        CreateBox(root.transform, "Emitter face", new Vector3(67.48f, 4.72f, 0.76f), new Vector3(0.15f, 0.78f, 0.42f), steel, false);
        CreateCylinder(root.transform, "Emitter nozzle", new Vector3(67.25f, 4.72f, 0.4f), new Vector3(0.19f, 0.28f, 0.19f), steel);
        CreateBox(root.transform, "Fire warning light", new Vector3(67.46f, 5.08f, -0.42f), new Vector3(0.12f, 0.12f, 0.05f), caution, false);

        GameObject active = new GameObject("Fire jet active");
        active.transform.SetParent(root.transform, false);
        CreateQuad(active.transform, "Animated fire plume", new Vector3(66.05f, 4.72f, -0.72f), new Vector3(2.65f, 1.05f, 1f), fire);
        CreateHazardTrigger(active.transform, "Fire plume trigger", new Vector3(66.2f, 4.72f, 0f), new Vector3(2.25f, 0.9f, 2.2f), lifeManager);
        ConfigureCycle(root, active, 1.35f, 1.25f, 0.15f);
        SaveModularPrefab(root, HazardPrefabFolder + "/09 Fire Emitter.prefab");
    }

    private static void CreateElectricGate(Transform parent, RatLifeManager lifeManager, Material gate, Material structure, Material caution)
    {
        GameObject root = new GameObject("10 - Electric Gate");
        root.transform.SetParent(parent, false);
        root.transform.position = new Vector3(75.15f, 4.1f, 0f);
        CreateBox(root.transform, "Gate left post", new Vector3(74.65f, 5.35f, 0.75f), new Vector3(0.18f, 2.55f, 0.38f), structure, false);
        CreateBox(root.transform, "Gate right post", new Vector3(75.65f, 5.35f, 0.75f), new Vector3(0.18f, 2.55f, 0.38f), structure, false);
        CreateBox(root.transform, "Gate left capacitor", new Vector3(74.65f, 4.28f, 0.62f), new Vector3(0.42f, 0.16f, 0.54f), caution, false);
        CreateBox(root.transform, "Gate right capacitor", new Vector3(75.65f, 4.28f, 0.62f), new Vector3(0.42f, 0.16f, 0.54f), caution, false);

        GameObject active = new GameObject("Electric field active");
        active.transform.SetParent(root.transform, false);
        CreateQuad(active.transform, "Animated electric field", new Vector3(75.15f, 5.35f, -0.72f), new Vector3(0.88f, 2.2f, 1f), gate);
        CreateHazardTrigger(active.transform, "Electric gate trigger", new Vector3(75.15f, 5.25f, 0f), new Vector3(0.9f, 2.2f, 2.2f), lifeManager);
        ConfigureCycle(root, active, 1.55f, 1.2f, 0.65f);
        SaveModularPrefab(root, HazardPrefabFolder + "/10 Electric Gate.prefab");
    }

    private static void CreateLaserBarrier(Transform parent, RatLifeManager lifeManager, Material laser, Material structure, Material red)
    {
        GameObject root = new GameObject("11 - Laser Barrier");
        root.transform.SetParent(parent, false);
        root.transform.position = new Vector3(81.05f, 4.1f, 0f);
        CreateBox(root.transform, "Laser left post", new Vector3(80.05f, 5.1f, 0.75f), new Vector3(0.22f, 2.05f, 0.42f), structure, false);
        CreateBox(root.transform, "Laser right post", new Vector3(82.05f, 5.1f, 0.75f), new Vector3(0.22f, 2.05f, 0.42f), structure, false);
        CreateBox(root.transform, "Laser left emitter", new Vector3(80.18f, 5.1f, 0.42f), new Vector3(0.18f, 0.28f, 0.18f), red, false);
        CreateBox(root.transform, "Laser right emitter", new Vector3(81.92f, 5.1f, 0.42f), new Vector3(0.18f, 0.28f, 0.18f), red, false);

        GameObject active = new GameObject("Laser array active");
        active.transform.SetParent(root.transform, false);
        for (int beam = 0; beam < 4; beam++)
        {
            float y = 4.55f + beam * 0.36f;
            CreateQuad(active.transform, "Animated laser beam " + (beam + 1), new Vector3(81.05f, y, -0.72f), new Vector3(1.82f, 0.16f, 1f), laser);
        }
        CreateHazardTrigger(active.transform, "Laser barrier trigger", new Vector3(81.05f, 5.1f, 0f), new Vector3(1.75f, 1.45f, 2.2f), lifeManager);
        ConfigureCycle(root, active, 1.8f, 0.85f, 1.1f);
        SaveModularPrefab(root, HazardPrefabFolder + "/11 Laser Barrier.prefab");
    }

    private static void SaveModularPrefab(GameObject sceneObject, string prefabPath)
    {
        GameObject prefabSource = Object.Instantiate(sceneObject);
        prefabSource.name = sceneObject.name;
        prefabSource.transform.SetParent(null);
        prefabSource.transform.position = Vector3.zero;

        // Prefabs cannot retain scene-object references. HazardTrigger resolves
        // the active RatLifeManager automatically at runtime.
        foreach (HazardTrigger hazard in prefabSource.GetComponentsInChildren<HazardTrigger>(true))
        {
            SerializedObject hazardObject = new SerializedObject(hazard);
            hazardObject.FindProperty("lifeManager").objectReferenceValue = null;
            hazardObject.ApplyModifiedPropertiesWithoutUndo();
        }

        PrefabUtility.SaveAsPrefabAsset(prefabSource, prefabPath);
        Object.DestroyImmediate(prefabSource);
    }

    private static void CreateCheckpoint(Transform parent, RatLifeManager lifeManager, int number, Vector3 respawnPosition, Material trim)
    {
        GameObject respawn = new GameObject("Checkpoint " + number + " respawn");
        respawn.transform.SetParent(parent, false);
        respawn.transform.position = respawnPosition;

        GameObject trigger = new GameObject("Checkpoint " + number);
        trigger.transform.SetParent(parent, false);
        trigger.transform.position = respawnPosition + Vector3.up * 0.92f;
        BoxCollider collider = trigger.AddComponent<BoxCollider>();
        collider.size = new Vector3(0.7f, 2f, 2.5f);
        collider.isTrigger = true;
        Checkpoint checkpoint = trigger.AddComponent<Checkpoint>();
        SerializedObject checkpointObject = new SerializedObject(checkpoint);
        checkpointObject.FindProperty("checkpointNumber").intValue = number;
        checkpointObject.FindProperty("respawnPoint").objectReferenceValue = respawn.transform;
        checkpointObject.FindProperty("lifeManager").objectReferenceValue = lifeManager;
        checkpointObject.ApplyModifiedPropertiesWithoutUndo();
        CreateBox(parent, "Checkpoint " + number + " beacon", respawnPosition + new Vector3(0f, 0.72f, 1f), new Vector3(0.12f, 1.5f, 0.12f), trim, false);
    }

    private static void CreateLabel(Transform parent, string text, Vector3 position, float characterSize)
    {
        GameObject labelObject = new GameObject(text);
        labelObject.transform.SetParent(parent, false);
        labelObject.transform.position = position;
        TextMesh label = labelObject.AddComponent<TextMesh>();
        label.text = text;
        label.characterSize = characterSize;
        label.fontSize = 64;
        label.anchor = TextAnchor.MiddleCenter;
        label.color = new Color(0.67f, 0.74f, 0.73f);
    }
}
#endif

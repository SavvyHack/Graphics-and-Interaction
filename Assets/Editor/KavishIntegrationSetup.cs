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

    [MenuItem("Project R.A.T./Kavish/Install or Refresh Kavish Systems")]
    public static void Install()
    {
        AssetDatabase.Refresh();
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

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

        EditorUtility.SetDirty(sessionObject);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("KAVISH_SETUP_OK: three-rat lives, checkpoints, hazards, exit, SFX and animated energy material are wired in RatEnclosure.");
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

        int electricObjects = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Count(r => r.gameObject.name == "Electric runoff" &&
                        r.sharedMaterial != null &&
                        r.sharedMaterial.shader != null &&
                        r.sharedMaterial.shader.name == "ProjectRAT/Kavish/AnimatedEnergyHazard");

        bool ok = gm != null && life != null && audio != null && hazards.Length > 0 &&
                  checkpoints.Length >= 4 && exits.Length > 0 && electricObjects > 0;

        if (ok)
            Debug.Log($"KAVISH_VALIDATION_OK: {hazards.Length} hazards, {checkpoints.Length} checkpoints, {exits.Length} exit trigger(s), {electricObjects} animated hazard renderers.");
        else
            Debug.LogError($"KAVISH_VALIDATION_FAILED: GameManager={gm != null}, RatLifeManager={life != null}, AudioManager={audio != null}, hazards={hazards.Length}, checkpoints={checkpoints.Length}, exits={exits.Length}, animatedRenderers={electricObjects}.");
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
}
#endif

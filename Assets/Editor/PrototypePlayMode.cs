using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Pressing Play launches the saved prototype; no level generation is required.</summary>
[InitializeOnLoad]
public static class PrototypePlayMode
{
    public const string ScenePath = "Assets/Scenes/Home.unity";
    private const string MenuPath = "Project R.A.T./Start Play Mode in Home";
    // Project-specific preference, defaulting on for a fresh supervisor checkout.
    private static string PreferenceKey => "ProjectRAT.PrototypePlayMode." + Application.dataPath;

    static PrototypePlayMode()
    {
        EditorApplication.delayCall += Configure;
        EditorSceneManager.sceneOpened += (scene, mode) => Configure();
    }

    private static void Configure()
    {
        string activePath = SceneManager.GetActiveScene().path;
        if (activePath == ScenePath || activePath == CutsceneAuthoring.ScenePath || System.Array.IndexOf(CampaignCatalog.Scenes, activePath) >= 0 || RatLevelSlots.IsLevelSlot(activePath) ||
            !EditorPrefs.GetBool(PreferenceKey, true))
        {
            EditorSceneManager.playModeStartScene = null;
            return;
        }
        var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) ?? AssetDatabase.LoadAssetAtPath<SceneAsset>(PrototypeAssetPaths.PrototypeScene);
        if (scene == null)
        {
            Debug.LogError("The saved entry scene is missing: " + ScenePath);
            return;
        }
        EditorSceneManager.playModeStartScene = scene;
    }

    [MenuItem(MenuPath)]
    private static void Toggle()
    {
        EditorPrefs.SetBool(PreferenceKey, !EditorPrefs.GetBool(PreferenceKey, true));
        Configure();
    }

    [MenuItem(MenuPath, true)]
    private static bool ValidateMenu()
    {
        Menu.SetChecked(MenuPath, EditorPrefs.GetBool(PreferenceKey, true));
        return !EditorApplication.isPlayingOrWillChangePlaymode;
    }
}

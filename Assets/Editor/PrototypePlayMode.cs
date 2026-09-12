using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Pressing Play launches the saved prototype; no level generation is required.</summary>
[InitializeOnLoad]
public static class PrototypePlayMode
{
    public const string ScenePath = PrototypeAssetPaths.PrototypeScene;
    private const string MenuPath = "Project R.A.T./Start Play Mode in Prototype";
    // Project-specific preference, defaulting on for a fresh supervisor checkout.
    private static string PreferenceKey => "ProjectRAT.PrototypePlayMode." + Application.dataPath;

    static PrototypePlayMode()
    {
        EditorApplication.delayCall += Configure;
    }

    private static void Configure()
    {
        if (!EditorPrefs.GetBool(PreferenceKey, true))
        {
            EditorSceneManager.playModeStartScene = null;
            return;
        }
        var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
        if (scene == null)
        {
            Debug.LogError("The saved prototype scene is missing: " + ScenePath);
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

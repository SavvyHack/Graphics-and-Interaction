using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Scene slots store complete editable Unity scenes, not runtime save-game data.
public static class RatLevelSlots
{
    public static bool IsLevelSlot(string path) =>
        path == PathFor(1) || path == PathFor(2) || path == PathFor(3);
    private static string PathFor(int number) => "Assets/Scenes/Level" + number + ".unity";
    [MenuItem("Project R.A.T./Level 1/Save Scene")]
    public static void Save1() => Save(1);
    [MenuItem("Project R.A.T./Level 1/Load Scene")]
    public static void Load1() => Load(1);
    [MenuItem("Project R.A.T./Level 2/Save Scene")]
    public static void Save2() => Save(2);
    [MenuItem("Project R.A.T./Level 2/Load Scene")]
    public static void Load2() => Load(2);
    [MenuItem("Project R.A.T./Level 3/Save Scene")]
    public static void Save3() => Save(3);
    [MenuItem("Project R.A.T./Level 3/Load Scene")]
    public static void Load3() => Load(3);

    private static bool CanEdit()
    {
        if (!EditorApplication.isPlayingOrWillChangePlaymode) return true;
        Debug.LogWarning("Exit Play mode before saving or loading a level slot.");
        return false;
    }
    private static void Save(int number)
    {
        if (!CanEdit()) return;
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded) return;
        string path = PathFor(number);
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null &&
            scene.path != path && !EditorUtility.DisplayDialog("Replace Level " + number + "?",
                "Replace this saved level with a copy of the current active scene?",
                "Replace", "Cancel")) return;
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");
        // Copy mode leaves the source scene open and keeps its identity.
        if (!EditorSceneManager.SaveScene(scene, path, scene.path != path)) return;
        var scenes = EditorBuildSettings.scenes.ToList();
        int index = scenes.FindIndex(item => item.path == path);
        if (index < 0) scenes.Add(new EditorBuildSettingsScene(path, true));
        else scenes[index].enabled = true;
        EditorBuildSettings.scenes = scenes.ToArray();
        AssetDatabase.SaveAssets();
        Debug.Log("Saved Level " + number + " to " + path + ". Use Load Scene to edit that copy.");
    }
    private static void Load(int number)
    {
        if (!CanEdit()) return;
        string path = PathFor(number);
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null)
        {
            Debug.LogWarning("Level " + number + " has not been saved yet. Use its Save Scene command first.");
            return;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        // The startup hook also recognizes slots, so this survives compilation/restart.
        EditorSceneManager.playModeStartScene = null;
        Debug.Log("Loaded Level " + number + ". Play runs this scene.");
    }
}

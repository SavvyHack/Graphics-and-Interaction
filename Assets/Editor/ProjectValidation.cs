using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Checks that the organised project still resolves its scene and material references.</summary>
public static class ProjectValidation
{
    [MenuItem("Project R.A.T./Validate Project References")]
    public static void Validate()
    {
        var firstScene = EditorBuildSettings.scenes.FirstOrDefault(scene => scene.enabled);
        if (firstScene == null || firstScene.path != PrototypeAssetPaths.PrototypeScene)
            throw new Exception("The prototype must remain the first enabled build scene.");

        foreach (string path in new[] { PrototypeAssetPaths.PrototypeScene, PrototypeAssetPaths.StarterScene })
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null)
                throw new Exception("Missing scene: " + path);
            Scene scene = SceneManager.GetSceneByPath(path);
            bool openedForCheck = !scene.IsValid() || !scene.isLoaded;
            if (openedForCheck) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            try
            {
                foreach (Transform item in scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true)))
                {
                    if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(item.gameObject) != 0)
                        throw new Exception("Missing script on " + path + ": " + item.name);
                    var renderer = item.GetComponent<Renderer>();
                    if (renderer != null && renderer.sharedMaterials.Any(material => material == null || material.shader == null))
                        throw new Exception("Missing material or shader on " + path + ": " + item.name);
                }
            }
            finally
            {
                if (openedForCheck) EditorSceneManager.CloseScene(scene, true);
            }
        }

        foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { PrototypeAssetPaths.Materials }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null || material.shader == null || ShaderUtil.ShaderHasError(material.shader))
                throw new Exception("Invalid material or shader: " + path);
        }
        Debug.Log("RAT_PROJECT_REFERENCES_OK: build entry, both scenes, scripts, materials and shaders.");
    }
}

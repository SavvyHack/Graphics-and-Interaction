using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>Applies the observation-room art direction without rebuilding gameplay objects.</summary>
public static class ObservationStyle
{
    private const string Materials = PrototypeAssetPaths.EnvironmentMaterials + "/";
    private const string Prototype = PrototypeAssetPaths.PrototypeScene;
    private const string Starter = PrototypeAssetPaths.StarterScene;

    [MenuItem("Project R.A.T./Apply Observation Style to Both Scenes")]
    public static void Apply()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var setup = EditorSceneManager.GetSceneManagerSetup();
        ConfigureMaterials();
        foreach (string path in new[] { Prototype, Starter })
        {
            var scene = EditorSceneManager.OpenScene(path);
            ApplyToOpenScene();
            EditorSceneManager.SaveScene(scene);
        }
        AssetDatabase.SaveAssets();
        if (!Application.isBatchMode) EditorSceneManager.RestoreSceneManagerSetup(setup);
        Debug.Log("RAT_OBSERVATION_STYLE_OK");
    }

    public static void ConfigureMaterials()
    {
        FlatMaterial("Floor", Color.black);
        FlatMaterial("Steel", new Color(.16f, .20f, .22f));
        FlatMaterial("Structure", new Color(.20f, .24f, .25f));
        FlatMaterial("Recess", new Color(.04f, .06f, .065f));
        FlatMaterial("Trim", new Color(.38f, .46f, .47f));
        FlatMaterial("Route light", new Color(.59f, .68f, .66f));
        FlatMaterial("Runoff", new Color(.18f, .68f, .8f));
        foreach (string path in new[] { PrototypeAssetPaths.GlassMaterial, Materials + "Observation glass.mat" })
        {
            Material glass = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (glass == null) continue;
            glass.renderQueue = 3100;
            glass.SetColor("_GlassTint", new Color(.88f, .97f, 1));
            glass.SetColor("_FresnelColor", new Color(.8f, .94f, 1));
            glass.SetColor("_ShimmerColor", new Color(1f, .98f, .91f));
            glass.SetFloat("_BaseAlpha", .06f);
            glass.SetFloat("_FresnelPower", 5);
            glass.SetFloat("_FresnelStrength", .7f);
            glass.SetFloat("_ShimmerStrength", .32f);
            glass.SetFloat("_ShimmerScale", 12);
            glass.SetFloat("_BarWidth", .48f);
            glass.SetFloat("_BarFeather", .06f);
            glass.SetFloat("_BarSlant", -.55f);
            glass.SetFloat("_RatParallax", .55f);
            glass.SetFloat("_DistortionStrength", .001f);
            glass.SetFloat("_EdgeWidth", .002f);
            EditorUtility.SetDirty(glass);
        }
    }

    internal static Material FlatMaterial(string name, Color colour)
    {
        string path = Materials + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Unlit/Color"));
            AssetDatabase.CreateAsset(material, path);
        }
        material.shader = Shader.Find("Unlit/Color");
        material.shaderKeywords = Array.Empty<string>();
        material.color = colour;
        EditorUtility.SetDirty(material);
        return material;
    }

    public static void ApplyToOpenScene()
    {
        ConfigureMaterials();
        var scene = SceneManager.GetActiveScene();
        var objects = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true)).ToArray();
        Material platformMaterial = AssetDatabase.LoadAssetAtPath<Material>(Materials + "Steel.mat");
        Material trimMaterial = AssetDatabase.LoadAssetAtPath<Material>(Materials + "Trim.mat");
        Material runoff = AssetDatabase.LoadAssetAtPath<Material>(Materials + "Runoff.mat");
        foreach (Transform item in objects)
        {
            if (item == null) continue;
            // These objects were decorative, with no colliders or gameplay scripts.
            if (item.name == "Rear wall" || item.name == "Recessed wall panel"
                || item.name == "Wall seam" || item.name == "Overhead light")
            {
                UnityEngine.Object.DestroyImmediate(item.gameObject);
                continue;
            }
            var renderer = item.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                if (item.name == "Electric runoff") renderer.sharedMaterial = runoff;
                if (item.name == "Illuminated walkable edge")
                {
                    Vector3 scale = item.localScale;
                    scale.y = .065f; // Parent platform height is .5: a .0325-unit lip.
                    item.localScale = scale;
                }
                if (item.name == "Lower enclosure rail")
                    renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(Materials + "Floor.mat");
                if (renderer.sharedMaterial != null && renderer.sharedMaterial.shader.name == "ProjectRAT/Tavish/GlassEnclosure")
                {
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
                    renderer.receiveShadows = false;
                    // Starter glass was behind the rat. Put the observation pane in front.
                    if (scene.path == Starter && item.name == "GlassEnclosure")
                        item.position = new Vector3(item.position.x, item.position.y, -2.3f);
                }
                // The original starter scene uses built-in white cubes for its platforms.
                if (scene.path == Starter && IsStarterPlatform(item))
                {
                    renderer.sharedMaterial = platformMaterial;
                    Outline(item, trimMaterial);
                }
                // Give the black ramp a white walkable edge as well.
                if (item.name == "Continuous ramp") Outline(item, trimMaterial);
            }
            var label = item.GetComponent<TextMesh>();
            if (label != null)
            {
                label.color = new Color(.67f, .74f, .73f);
                label.text = label.text.Replace("CYAN EDGES", "PLATFORM EDGES").Replace("WHITE EDGES", "PLATFORM EDGES");
                item.name = item.name.Replace("CYAN EDGES", "PLATFORM EDGES").Replace("WHITE EDGES", "PLATFORM EDGES");
            }
            var camera = item.GetComponent<Camera>();
            if (camera != null)
            {
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(.32f, .38f, .39f);
            }
        }
        RenderSettings.skybox = null;
        RenderSettings.fog = false;
        LaboratoryBackdrop.Apply(scene);
        EditorSceneManager.MarkSceneDirty(scene);
    }

    private static bool IsStarterPlatform(Transform item)
    {
        // Restrict this to the group's actual platform hierarchy; water, rat,
        // checkpoint flag and the spinning wheel retain their own materials.
        for (Transform ancestor = item; ancestor != null; ancestor = ancestor.parent)
            if (ancestor.name.Contains("Platform") || ancestor.name == "Ramp") return true;
        return false;
    }

    private static void Outline(Transform platform, Material trimMaterial)
    {
        Transform existing = platform.Find("White platform outline") ?? platform.Find("Platform edge trim");
        var line = existing != null ? existing.GetComponent<LineRenderer>() : new GameObject("Platform edge trim").AddComponent<LineRenderer>();
        line.name = "Platform edge trim";
        if (existing == null) line.transform.SetParent(platform, false);
        line.useWorldSpace = false;
        line.sharedMaterial = trimMaterial;
        line.widthMultiplier = .015f;
        line.loop = true;
        line.positionCount = 4;
        line.SetPositions(new[] { new Vector3(-.5f, .5f, -.505f), new Vector3(.5f, .5f, -.505f),
            new Vector3(.5f, -.5f, -.505f), new Vector3(-.5f, -.5f, -.505f) });
        line.shadowCastingMode = ShadowCastingMode.Off;
        line.receiveShadows = false;
    }

    // Batch entry: apply in place, validate references, then render with the real shader.
    public static void ApplyAndCapture()
    {
        Apply();
        CapturePreviews();
    }

    public static void ApplyAndVerify()
    {
        Apply();
        VerifyAndPlaytest();
    }

    public static void CapturePreviews()
    {
        string output = Environment.GetEnvironmentVariable("RAT_OUTPUT");
        if (string.IsNullOrEmpty(output)) throw new Exception("Set RAT_OUTPUT to the preview folder.");
        Directory.CreateDirectory(output);
        EditorSceneManager.OpenScene(Prototype);
        RatLevelBuilder.Validate();
        Camera camera = Camera.main;
        Capture(camera, Path.Combine(output, "observation-start.png"), new Vector3(5, 3.5f, -22), 5.4f, 1600, 900);
        Capture(camera, Path.Combine(output, "observation-transfer.png"), new Vector3(39, 3.5f, -22), 5.4f, 1600, 900);
        Capture(camera, Path.Combine(output, "observation-overview.png"), new Vector3(30, 3.2f, -22), 7.6f, 3200, 700);
        EditorSceneManager.OpenScene(Starter);
        camera = Camera.main;
        Capture(camera, Path.Combine(output, "observation-starter.png"), camera.transform.position, camera.orthographicSize, 1600, 900);
        Shader shader = Shader.Find("ProjectRAT/Tavish/GlassEnclosure");
        if (shader == null || !shader.isSupported || ShaderUtil.ShaderHasError(shader))
            throw new Exception("Observation glass shader failed to compile on this renderer.");
        Debug.Log("RAT_OBSERVATION_RENDER_OK");
    }

    public static void VerifyAndPlaytest()
    {
        ProjectValidation.Validate();
        CapturePreviews();
        RatLevelSmokeTests.RunFromStarter();
    }

    private static void Capture(Camera camera, string file, Vector3 position, float size, int width, int height)
    {
        Vector3 previous = camera.transform.position;
        float previousSize = camera.orthographicSize;
        var previousTarget = camera.targetTexture;
        var previousActive = RenderTexture.active;
        var target = new RenderTexture(width, height, 24);
        var image = new Texture2D(width, height, TextureFormat.RGB24, false);
        try
        {
            camera.transform.position = position;
            camera.orthographicSize = size;
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            image.Apply();
            File.WriteAllBytes(file, image.EncodeToPNG());
        }
        finally
        {
            camera.transform.position = previous;
            camera.orthographicSize = previousSize;
            camera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;
            UnityEngine.Object.DestroyImmediate(image);
            UnityEngine.Object.DestroyImmediate(target);
        }
    }
}

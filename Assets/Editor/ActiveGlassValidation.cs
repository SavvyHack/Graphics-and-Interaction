using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Render checks for rat-driven reflections, followed by the existing gameplay checks.</summary>
public static class ActiveGlassValidation
{
    public static void VerifyAndPlaytest()
    {
        ProjectValidation.Validate();
        string output = Environment.GetEnvironmentVariable("RAT_OUTPUT");
        if (string.IsNullOrEmpty(output)) throw new Exception("Set RAT_OUTPUT to the preview folder.");
        Directory.CreateDirectory(output);
        foreach (string scene in new[] { PrototypeAssetPaths.PrototypeScene, PrototypeAssetPaths.StarterScene })
            CheckScene(scene, output);
        RatLevelSmokeTests.RunFromStarter();
    }

    private static void CheckScene(string scene, string output)
    {
        EditorSceneManager.OpenScene(scene);
        Camera camera = Camera.main;
        FixedCameraFollow follow = camera.GetComponent<FixedCameraFollow>();
        Transform originalTarget = follow.Target;
        var firstRat = new GameObject("Reflection test target A");
        var secondRat = new GameObject("Reflection test target B");
        try
        {
            // Hold the camera and level still: only the selected target changes.
            firstRat.transform.position = new Vector3(0f, .08f, 0f);
            follow.SetTarget(firstRat.transform);
            RequireTarget(firstRat.transform.position);
            Color32[] start = Capture(camera, Path.Combine(output, Path.GetFileNameWithoutExtension(scene) + "-glass-start.png"));
            Color32[] still = Capture(camera, null);
            Require(Difference(start, still) < .00001f, "Stationary reflections must not scroll.");

            secondRat.transform.position = new Vector3(5f, .08f, 0f);
            follow.SetTarget(secondRat.transform);
            RequireTarget(secondRat.transform.position);
            Color32[] moved = Capture(camera, Path.Combine(output, Path.GetFileNameWithoutExtension(scene) + "-glass-moved.png"));
            Require(Difference(start, moved) > .001f, "Changing active rat X must visibly move the bars with a stationary camera.");

            secondRat.transform.position += Vector3.up * 2f;
            follow.SetTarget(secondRat.transform);
            RequireTarget(secondRat.transform.position);
            Color32[] jumped = Capture(camera, null);
            Require(Difference(moved, jumped) > .0003f, "Rat jump height must visibly shift the bars.");

            secondRat.SetActive(false);
            follow.SetTarget(secondRat.transform);
            Require(Shader.GetGlobalVector("_GlassActiveRatPosition") == Vector4.zero, "Inactive targets must be cleared.");
            follow.SetTarget(null);
            Require(Shader.GetGlobalVector("_GlassActiveRatPosition") == Vector4.zero, "Missing targets must be cleared.");
            Shader shader = Shader.Find("ProjectRAT/Tavish/GlassEnclosure");
            Require(shader != null && shader.isSupported && !ShaderUtil.ShaderHasError(shader), "Glass must compile on the actual renderer.");
            Debug.Log("RAT_ACTIVE_GLASS_OK: " + scene + " | stationary, horizontal movement, jumping, target switch and cleanup.");
        }
        finally
        {
            follow.SetTarget(originalTarget);
            UnityEngine.Object.DestroyImmediate(firstRat);
            UnityEngine.Object.DestroyImmediate(secondRat);
        }
    }

    private static void RequireTarget(Vector3 expected)
    {
        Vector4 actual = Shader.GetGlobalVector("_GlassActiveRatPosition");
        Require(Vector3.Distance(new Vector3(actual.x, actual.y, actual.z), expected) < .0001f && actual.w == 1f,
            "Shader must receive the selected rat's world position immediately.");
    }

    private static float Difference(Color32[] first, Color32[] second)
    {
        double total = 0;
        for (int i = 0; i < first.Length; i++)
            total += Math.Abs(first[i].r - second[i].r) + Math.Abs(first[i].g - second[i].g) + Math.Abs(first[i].b - second[i].b);
        return (float)(total / (first.Length * 3.0 * 255.0));
    }

    private static Color32[] Capture(Camera camera, string path)
    {
        RenderTexture previousTarget = camera.targetTexture;
        RenderTexture previousActive = RenderTexture.active;
        var target = new RenderTexture(1600, 900, 24);
        var image = new Texture2D(1600, 900, TextureFormat.RGB24, false);
        try
        {
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            image.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0);
            image.Apply();
            if (path != null) File.WriteAllBytes(path, image.EncodeToPNG());
            return image.GetPixels32();
        }
        finally
        {
            camera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;
            UnityEngine.Object.DestroyImmediate(image);
            UnityEngine.Object.DestroyImmediate(target);
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new Exception("Active glass: " + message);
    }
}

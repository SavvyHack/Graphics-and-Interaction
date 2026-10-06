using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Render checks for rat-driven reflections, followed by the existing gameplay checks.</summary>
public static class ActiveGlassValidation
{
    private static FixedCameraFollow runtimeFollow;
    private static PlayerRatController runtimePlayer;
    private static int cameraFrame;
    private static Vector3 pausedPosition;

    public static void VerifyCameraPlayMode()
    {
        VerifyCameraFollow();
        CampaignProfile.UseValidationProfile("ProjectRAT.Validation.Camera."+Guid.NewGuid().ToString("N"));
        EditorSceneManager.OpenScene(PrototypeAssetPaths.PrototypeScene);
        EditorSceneManager.playModeStartScene = null;
        SessionState.SetBool("RatCameraCheck", true);
        EditorApplication.EnterPlaymode();
    }

    [InitializeOnLoadMethod]
    private static void RegisterCameraCheck()
    {
        EditorApplication.update += StartCameraCheck;
    }

    private static void StartCameraCheck()
    {
        if (!SessionState.GetBool("RatCameraCheck", false) || !EditorApplication.isPlaying || runtimeFollow != null) return;
        runtimeFollow = UnityEngine.Object.FindFirstObjectByType<FixedCameraFollow>();
        runtimePlayer = UnityEngine.Object.FindFirstObjectByType<PlayerRatController>();
        if (runtimeFollow == null || runtimePlayer == null) return;
        cameraFrame = 0;
        Application.runInBackground = true;
        new GameObject("Camera check driver (not saved)").AddComponent<RatPlaytestDriver>().step = CheckCameraFrame;
    }

    private static void CheckCameraFrame()
    {
        try
        {
            Vector3 position = runtimeFollow.transform.position;
            Require(!float.IsNaN(position.x) && !float.IsInfinity(position.x) &&
                !float.IsNaN(position.y) && !float.IsInfinity(position.y), "Follow position must remain finite.");
            Require(Mathf.Abs(position.z + 22) < .0001f, "Runtime depth must stay at the authored -22.");
            cameraFrame++;
            if (cameraFrame == 5)
            {
                runtimePlayer.enabled = false;
                runtimePlayer.Respawn(new Vector3(20, 2, 0));
                runtimeFollow.SetTarget(runtimePlayer.transform, true);
                pausedPosition = runtimeFollow.transform.position;
                Require(Vector3.Distance(pausedPosition, new Vector3(22.5f, 3.8f, -22)) < .001f, "Runtime snap must use scene offset and bounds.");
                Time.timeScale = 0;
            }
            if (cameraFrame > 5 && cameraFrame <= 30)
                Require(Vector3.Distance(position, pausedPosition) < .001f, "Paused camera must stay still.");
            if (cameraFrame == 30)
            {
                Time.timeScale = 1;
                runtimePlayer.Respawn(new Vector3(35, 3, 0));
            }
            if (cameraFrame == 90)
            {
                Require(position.x > pausedPosition.x, "Follow must resume after zero-delta frames without poisoned velocity.");
                runtimePlayer.gameObject.SetActive(false);
            }
            if (cameraFrame == 92)
            {
                Require(Shader.GetGlobalVector("_GlassActiveRatPosition") == Vector4.zero, "Inactive rat must clear the runtime shader target.");
                runtimePlayer.gameObject.SetActive(true);
                UnityEngine.Object.FindFirstObjectByType<RatLifeManager>().OnRatDied();
                Require(runtimeFollow.Target == runtimePlayer.transform, "Life loss must keep the active player as follow target.");
            }
            if (cameraFrame >= 96)
            {
                Debug.Log("RAT_CAMERA_PLAYMODE_OK: authored scene, snap, 25 paused frames, resume, inactive target and life-loss retarget.");
                FinishCameraCheck(0);
            }
        }
        catch (Exception error) { Debug.LogException(error); FinishCameraCheck(1); }
    }

    private static void FinishCameraCheck(int code)
    {
        SessionState.SetBool("RatCameraCheck", false);
        Time.timeScale = 1;
        if(CampaignProfile.Playing)CampaignProfile.Finish("abandoned",0);
        foreach(var session in UnityEngine.Object.FindObjectsByType<CampaignSession>(FindObjectsSortMode.None))
            typeof(CampaignSession).GetField("ready",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(session,false);
        CampaignProfile.EndValidationProfile();
        EditorApplication.Exit(code);
    }

    [MenuItem("Project R.A.T./Validate Camera Follow")]
    public static void VerifyCameraFollow()
    {
        // A transient object also exercises SetTarget before the first runtime Awake.
        var cameraObject = new GameObject("Camera validation (temporary)");
        var rat = new GameObject("Camera target (temporary)");
        try
        {
            cameraObject.transform.position = new Vector3(5, 3.5f, -22);
            FixedCameraFollow follow = cameraObject.AddComponent<FixedCameraFollow>();
            var fields = new SerializedObject(follow);
            fields.FindProperty("clampToBounds").boolValue = true;
            fields.FindProperty("minBounds").vector2Value = new Vector2(5, 3.5f);
            fields.FindProperty("maxBounds").vector2Value = new Vector2(79, 4.8f);
            fields.ApplyModifiedPropertiesWithoutUndo();
            rat.transform.position = new Vector3(100, 50, 7);
            follow.SetTarget(rat.transform, true);
            Require(cameraObject.transform.position == new Vector3(79, 4.8f, -22), "Snap must clamp X/Y and preserve depth before Awake.");
            RequireTarget(rat.transform.position);
            fields.Update();
            fields.FindProperty("followVertically").boolValue = false;
            fields.FindProperty("minBounds").vector2Value = new Vector2(79, 20);
            fields.FindProperty("maxBounds").vector2Value = new Vector2(5, 30);
            fields.ApplyModifiedPropertiesWithoutUndo();
            rat.transform.position = new Vector3(-100, -50, 0);
            follow.SetTarget(rat.transform, true);
            Require(cameraObject.transform.position == new Vector3(5, 3.5f, -22), "Reversed bounds must normalize; fixed height must stay authored.");
            follow.SetTarget(null, true);
            Require(Shader.GetGlobalVector("_GlassActiveRatPosition") == Vector4.zero, "Null target must clear glass state.");
            Debug.Log("RAT_CAMERA_FOLLOW_OK: pre-Awake snap, depth, bounds, fixed height, shader target and cleanup.");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(cameraObject);
            UnityEngine.Object.DestroyImmediate(rat);
        }
    }

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

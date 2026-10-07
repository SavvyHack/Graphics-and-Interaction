using System;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// Explicit, isolated menu check. Does not save or regenerate scenes.
[InitializeOnLoad]
public static class MenuFontValidation
{
    private static int frames, step;
    private static bool pageReady;
    private static readonly string[] Pages = { "Home", "Map", "Shop", "Wardrobe", "Controls", "Powerups", "Guide", "Story", "Settings", "Statistics" };
    static MenuFontValidation() { EditorApplication.update += Tick; }
    public static void Run()
    {
        CampaignProfile.UseValidationProfile("ProjectRAT.Validation.Font." + Guid.NewGuid().ToString("N"));
        EditorSceneManager.OpenScene("Assets/Scenes/Home.unity");
        EditorSceneManager.playModeStartScene = null;
        SessionState.SetBool("RAT.FontCheck", true);
        EditorApplication.EnterPlaymode();
    }
    private static void Invoke(CampaignUI ui, string method, params object[] args) =>
        typeof(CampaignUI).GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(ui, args);
    private static void Tick()
    {
        if (!SessionState.GetBool("RAT.FontCheck", false) || !EditorApplication.isPlaying || ++frames < 30) return;
        frames = 0;
        try
        {
            var ui = Object.FindFirstObjectByType<CampaignUI>();
            Action home = () => Invoke(ui, "ShowHome");
            if (step == Pages.Length) { Debug.Log("RAT_MENU_FONT_OK"); Finish(0); return; }
            if (pageReady)
            {
                Capture(ui, 1600, 1000);
                Capture(ui, 900, 1000);
                step++; pageReady = false; return;
            }
            switch (step)
            {
                case 0: home(); break;
                case 1: Invoke(ui, "ShowSelect"); break;
                case 2: Invoke(ui, "ShowWardrobe", true); break;
                case 3: Invoke(ui, "ShowWardrobe", false); break;
                case 4: Invoke(ui, "ShowHelp", home, 0); break;
                case 5: Invoke(ui, "ShowHelp", home, 3); break;
                case 6: Invoke(ui, "ShowHelp", home, 1); break;
                case 7: Invoke(ui, "ShowHelp", home, 2); break;
                case 8: Invoke(ui, "ShowSettings", home); break;
                case 9: Invoke(ui, "ShowStats"); break;
            }
            pageReady = true;
        }
        catch (Exception ex) { Debug.LogException(ex); Finish(1); }
    }
    private static void Capture(CampaignUI ui, int width, int height)
    {
        var camera = Camera.main;
        var canvas = ui.GetComponentInChildren<Canvas>();
        var mode = canvas.renderMode; var oldCamera = canvas.worldCamera;
        var oldTarget = camera.targetTexture; var active = RenderTexture.active;
        var target = new RenderTexture(width, height, 24);
        var image = new Texture2D(width, height, TextureFormat.RGB24, false);
        try
        {
            camera.targetTexture = target; canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera; canvas.planeDistance = 1;
            canvas.GetComponent<CanvasScaler>().SendMessage("Update");
            Canvas.ForceUpdateCanvases();
            foreach (var label in ui.GetComponentsInChildren<TMP_Text>())
            {
                if (label.font == null || label.font.name != "Bouncy Bun Menu SDF") throw new Exception("Wrong font: " + label.text);
                label.ForceMeshUpdate();
                if (label.isTextOverflowing) Debug.LogWarning("FONT_OVERFLOW " + Pages[step] + " " + width + ": " + label.text);
            }
            camera.Render(); RenderTexture.active = target;
            image.ReadPixels(new Rect(0, 0, width, height), 0, 0); image.Apply();
            Directory.CreateDirectory("Logs/FontReview");
            File.WriteAllBytes("Logs/FontReview/" + Pages[step] + "-" + width + ".png", image.EncodeToPNG());
        }
        finally
        {
            canvas.renderMode = mode; canvas.worldCamera = oldCamera;
            camera.targetTexture = oldTarget; RenderTexture.active = active;
            Object.DestroyImmediate(target); Object.DestroyImmediate(image);
        }
    }
    private static void Finish(int code)
    {
        SessionState.SetBool("RAT.FontCheck", false);
        CampaignProfile.EndValidationProfile();
        EditorApplication.Exit(code);
    }
}

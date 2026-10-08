using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using Object = UnityEngine.Object;

/// <summary>
/// Restyles the world-space signs in every campaign level so they are easy to read:
/// the menus' Bouncy Bun font, a thick dark outline and soft shadow, colour by meaning,
/// larger small labels, and placement in front of the observation glass so its glare
/// cannot wash them out. Colliding signs are separated. Only TextMeshPro sign components are changed. Safe to re-run.
///   Yellow - instructions and arrows      White - power-up names, titles, RELEASE, EXIT
///   Mint   - checkpoint numbers (matches the checkpoint lamps)
/// </summary>
public static class LevelLabelStyle
{
    private const string SourceFontPath = "Assets/Resources/Fonts/BouncybunDemo-V4K8y.otf";
    private const string FontAssetPath = "Assets/Resources/Fonts/Level Sign SDF.asset";
    private const string MaterialPath = "Assets/Materials/Campaign/Level sign.mat";
    private const string FallbackPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
    private const string Glyphs = " ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789'+,-./:<>^!?()&";
    // In front of the observation glass (z -2.35), behind the frame fasteners (z -2.58).
    private const float SignZ = -2.45f;

    public static readonly Color Instruction = new Color(1f, 0.84f, 0.29f);
    public static readonly Color Name = Color.white;
    public static readonly Color CheckpointColour = new Color(0.55f, 1f, 0.77f);

    [MenuItem("Project RAT/Restyle Level Labels")]
    public static void ApplyAll()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        TMP_FontAsset font = SignFont();
        Material material = SignMaterial(font);
        foreach (string path in CampaignCatalog.Scenes) Apply(path, font, material);
        AssetDatabase.SaveAssets();
        Debug.Log("RAT_LABELS_RESTYLED");
    }

    private static void Apply(string path, TMP_FontAsset font, Material material)
    {
        var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        int count = 0;
        foreach (TextMeshPro label in Object.FindObjectsByType<TextMeshPro>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            label.font = font;
            label.fontSharedMaterial = material;
            label.fontStyle = FontStyles.Normal;
            string text = label.text.Trim();
            if (text.StartsWith("CP ", StringComparison.Ordinal))
            {
                label.color = CheckpointColour;
                label.fontSize = 3.4f;
            }
            else if (RatPowerups.Names.Contains(text))
            {
                label.color = Name;
                label.fontSize = 3.6f;
            }
            else if (text == "RELEASE" || text == "EXIT" || text.StartsWith("R.A.T.", StringComparison.Ordinal) ||
                     (text.Length > 2 && char.IsDigit(text[0]) && char.IsDigit(text[1])))
            {
                label.color = Name; // Titles keep their size.
            }
            else
            {
                label.color = Instruction;
                label.fontSize = Mathf.Max(label.fontSize, 4.2f);
            }
            var rect = label.rectTransform;
            rect.anchoredPosition3D = new Vector3(rect.anchoredPosition3D.x, rect.anchoredPosition3D.y, SignZ - rect.parent.position.z);
            EditorUtility.SetDirty(label);
            count++;
        }
        int moved = SeparateOverlaps();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"RAT_LABELS_RESTYLED {count} in {path} (overlaps fixed: {moved})");
    }

    /// <summary>
    /// Bolder, larger signs can collide (two stations side by side). A decorative arrow lying on
    /// text is hidden; any other colliding sign is lifted above the one to its left.
    /// </summary>
    private static int SeparateOverlaps()
    {
        var labels = Object.FindObjectsByType<TextMeshPro>(FindObjectsSortMode.None).OrderBy(l => l.transform.position.x).ToList();
        int fixedCount = 0;
        foreach (TextMeshPro arrow in labels.Where(IsArrow).ToList())
            if (labels.Any(other => !IsArrow(other) && other.gameObject.activeInHierarchy && Box(arrow).Overlaps(Box(other))))
            {
                arrow.gameObject.SetActive(false);
                fixedCount++;
            }
        var signs = labels.Where(l => !IsArrow(l) && l.gameObject.activeInHierarchy).ToList();
        for (int pass = 0; pass < 4; pass++)
        {
            bool changed = false;
            for (int i = 0; i < signs.Count; i++)
                for (int j = i + 1; j < signs.Count; j++)
                {
                    Rect a = Box(signs[i]), b = Box(signs[j]);
                    if (!a.Overlaps(b)) continue;
                    var rect = signs[j].rectTransform;
                    rect.anchoredPosition += new Vector2(0f, a.yMax - b.yMin + 0.12f);
                    EditorUtility.SetDirty(signs[j]);
                    fixedCount++; changed = true;
                }
            if (!changed) break;
        }
        return fixedCount;
    }

    private static bool IsArrow(TextMeshPro label) => label.text.Trim() == "^" || label.text.Trim() == "v";

    /// <summary>World-space rectangle covered by the rendered glyphs, slightly padded.</summary>
    private static Rect Box(TextMeshPro label)
    {
        label.ForceMeshUpdate();
        Bounds bounds = label.textBounds;
        Vector3 min = label.transform.TransformPoint(bounds.min), max = label.transform.TransformPoint(bounds.max);
        return Rect.MinMaxRect(Mathf.Min(min.x, max.x) - 0.08f, Mathf.Min(min.y, max.y) - 0.05f, Mathf.Max(min.x, max.x) + 0.08f, Mathf.Max(min.y, max.y) + 0.05f);
    }

    /// <summary>Saved, static TMP font asset made from the menu font, so builds never need the source file.</summary>
    private static TMP_FontAsset SignFont()
    {
        var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        if (existing != null) return existing;
        Font source = AssetDatabase.LoadAssetAtPath<Font>(SourceFontPath) ?? throw new Exception("Missing " + SourceFontPath);
        TMP_FontAsset font = TMP_FontAsset.CreateFontAsset(source, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, false);
        font.name = "Level Sign SDF";
        font.TryAddCharacters(Glyphs, out string missing);
        if (!string.IsNullOrEmpty(missing)) Debug.LogWarning("Level sign font falls back to Liberation Sans for: " + missing);
        font.atlasPopulationMode = AtlasPopulationMode.Static;
        // Missing glyphs (if any) come from the default TMP font.
        font.fallbackFontAssetTable = new List<TMP_FontAsset> { AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FallbackPath) };
        AssetDatabase.CreateAsset(font, FontAssetPath);
        font.atlasTextures[0].name = "Level Sign Atlas";
        AssetDatabase.AddObjectToAsset(font.atlasTextures[0], font);
        font.material.name = "Level Sign Atlas Material";
        AssetDatabase.AddObjectToAsset(font.material, font);
        EditorUtility.SetDirty(font);
        AssetDatabase.SaveAssets();
        return font;
    }

    /// <summary>Thick dark-navy outline plus a soft drop shadow: readable over light glare and dark panels.</summary>
    private static Material SignMaterial(TMP_FontAsset font)
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (material == null)
        {
            material = new Material(font.material);
            AssetDatabase.CreateAsset(material, MaterialPath);
        }
        material.shader = font.material.shader;
        material.mainTexture = font.atlasTextures[0];
        material.SetFloat("_FaceDilate", 0.1f);
        material.SetColor("_OutlineColor", new Color(0.03f, 0.06f, 0.12f, 1f));
        material.SetFloat("_OutlineWidth", 0.28f);
        material.EnableKeyword("UNDERLAY_ON");
        material.SetColor("_UnderlayColor", new Color(0f, 0f, 0f, 0.65f));
        material.SetFloat("_UnderlayOffsetX", 0.6f);
        material.SetFloat("_UnderlayOffsetY", -0.6f);
        material.SetFloat("_UnderlaySoftness", 0.35f);
        EditorUtility.SetDirty(material);
        return material;
    }
}

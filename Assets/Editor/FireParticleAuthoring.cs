using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Builds the fire-trap particle prefab and places one in every fire VFX socket.
/// Safe to re-run: the prefab keeps its GUID and old instances are replaced.
/// Every particle setting lives in BuildPrefab so the effect is easy to read and tune.
/// </summary>
public static class FireParticleAuthoring
{
    private const string MaterialPath = "Assets/Materials/Hazards/FireParticle.mat";
    private const string PrefabPath = "Assets/Prefabs/Hazards/FireParticles.prefab";
    private const string SocketName = "Fire VFX socket";
    private static string[] Scenes => CampaignCatalog.Scenes;

    [MenuItem("Project RAT/Build Fire Particles")]
    public static void Build()
    {
        GameObject prefab = BuildPrefab(BuildMaterial());
        foreach (string path in Scenes) PlaceInScene(path, prefab);
        AssetDatabase.SaveAssets();
        Debug.Log("RAT_FIRE_PARTICLES_BUILT");
    }

    private static Material BuildMaterial()
    {
        // Additive blending: particles only ever brighten what is behind them, so overlapping particles
        // glow like real flame and the soft texture edges never leave dark halos.
        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (material == null)
        {
            material = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));
            AssetDatabase.CreateAsset(material, MaterialPath);
        }
        material.shader = Shader.Find("Legacy Shaders/Particles/Additive");
        material.mainTexture = AssetDatabase.GetBuiltinExtraResource<Texture2D>("Default-Particle.psd"); // Soft round dot.
        material.SetColor("_TintColor", new Color(0.5f, 0.5f, 0.5f, 0.5f));
        EditorUtility.SetDirty(material);
        return material;
    }

    private static GameObject BuildPrefab(Material material)
    {
        var go = new GameObject("Fire Particles");
        var ps = go.AddComponent<ParticleSystem>();
        go.AddComponent<FireParticles>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // Main: short-lived, fast particles. Each value is a random range so no two flames match.
        var main = ps.main;
        main.duration = 1f;
        main.loop = true;
        main.prewarm = true; // Flame is already full height the moment the trap turns on.
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.75f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2.6f, 3.4f); // Travels ~1.3-2.5m: across the 2m damage box.
        main.gravityModifier = -0.35f; // Negative gravity: hot flames drift slightly upwards as they travel.
        main.startSize = new ParticleSystem.MinMaxCurve(0.45f, 0.8f);
        // Start colour is multiplied by the colour-over-lifetime gradient, so keep it near white for a small tint variation.
        main.startColor = new ParticleSystem.MinMaxGradient(Color.white, new Color(1f, 0.8f, 0.55f));
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.playOnAwake = true;
        main.maxParticles = 60; // Hard cap keeps WebGL cost bounded.

        var emission = ps.emission;
        emission.rateOverTime = 50f;

        // Shape: a narrow cone at the nozzle mouth. Cones emit along local Z, so turn Z to face left (-X),
        // the direction the existing shader plume blows out of the nozzle.
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 10f;
        shape.radius = 0.25f;
        shape.rotation = new Vector3(0f, -90f, 0f);

        // Colour over lifetime: bright yellow core cools to dark red and fades out at the top.
        var colour = ps.colorOverLifetime;
        colour.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(new Color(1f, 0.95f, 0.6f), 0f), new GradientColorKey(new Color(1f, 0.45f, 0.1f), 0.4f), new GradientColorKey(new Color(0.6f, 0.1f, 0.05f), 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.05f), new GradientAlphaKey(0.9f, 0.5f), new GradientAlphaKey(0f, 1f) });
        colour.color = gradient;

        // Size over lifetime: tongues of flame taper to a point as they travel.
        var size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.3f));

        // Noise: gentle random turbulence makes the flames flicker and sway instead of rising in straight lines.
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.35f;
        noise.frequency = 1.2f;
        noise.scrollSpeed = 1f;
        noise.quality = ParticleSystemNoiseQuality.Low; // Cheapest noise for WebGL.

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabPath);
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static void PlaceInScene(string path, GameObject prefab)
    {
        var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        int placed = 0;
        // Collect sockets first: replacing old instances destroys transforms the search also returned.
        var sockets = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(t => t.name.StartsWith(SocketName)).ToList();
        foreach (Transform socket in sockets)
        {
            socket.name = SocketName;
            // Already holds exactly one instance of this prefab: leave it, so unchanged scenes stay unchanged.
            if (socket.childCount == 1 && PrefabUtility.GetCorrespondingObjectFromSource(socket.GetChild(0).gameObject) == prefab) continue;
            for (int i = socket.childCount - 1; i >= 0; i--) Object.DestroyImmediate(socket.GetChild(i).gameObject);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, socket);
            // Socket is at the vent's base centre. Move to the nozzle mouth on the right (+1.1m x, +0.5m y),
            // and 0.3m towards the camera so particles draw over the shader plume.
            instance.transform.localPosition = new Vector3(1.1f, 0.5f, -0.3f);
            placed++;
        }
        if (placed == 0) return;
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"RAT_FIRE_PARTICLES_PLACED {placed} in {path}");
    }
}

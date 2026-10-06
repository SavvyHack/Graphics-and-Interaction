using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

/// <summary>Adapts copies of the two reference scenes. Never rebuilds their geometry.</summary>
// Layout source: https://github.com/SavvyHack/Graphics-and-Interaction/tree/11bf1fb1e1a05e778ecbed0d6a9e4beb3ec54ba9
public static class ReferenceLevelAdaptation
{
    private static T[] All<T>() where T : Object => Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    private static void Set(Object owner, string name, object value) => CampaignAuthoring.Set(owner,name,value);

    // Narrow repair for the different prop naming in the imported scenes; no geometry regenerated.
    public static void WireWaitingCosmetics()
    {
        CampaignAuthoring.PrepareMaterials();
        for(int index=0;index<2;index++)
        {
            EditorSceneManager.OpenScene(CampaignCatalog.Scenes[index]);
            foreach(Transform t in All<Transform>().Where(t=>t.name.StartsWith("Waiting subject",StringComparison.OrdinalIgnoreCase)))CampaignAuthoring.AddCosmetic(t.gameObject);
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }
        AssetDatabase.SaveAssets();
    }
    [MenuItem("Project R.A.T./Campaign/Adapt imported first two levels (once)")]
    public static void Adapt()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        for (int index=0;index<2;index++)
        {
            EditorSceneManager.OpenScene(CampaignCatalog.Scenes[index]);
            if (All<CampaignSession>().Length>0) throw new InvalidOperationException("Already adapted; refusing to overwrite authored additions.");
            foreach (Transform t in All<Transform>()) GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
            var life=All<RatLifeManager>().Single();var player=All<PlayerRatController>().Single();
            foreach(var legacy in All<PrototypeHUD>()) { legacy.enabled=false;Set(life,"hud",null); }
            Camera camera=All<Camera>().Single();camera.transform.SetPositionAndRotation(new Vector3(10,6,-35),Quaternion.identity);
            camera.orthographic=true;camera.orthographicSize=6.8f;
            var follow=camera.GetComponent<FixedCameraFollow>() ?? camera.gameObject.AddComponent<FixedCameraFollow>();
            Set(follow,"target",player.transform);Set(follow,"followOffset",new Vector2(0,1.8f));Set(follow,"followVertically",true);
            Set(follow,"clampToBounds",true);Set(follow,"minBounds",new Vector2(10,6));Set(follow,"maxBounds",new Vector2(34,22));Set(follow,"minimumHorizontalView",20f);
            Set(life,"cameraFollow",follow);
            CampaignAuthoring.PrepareMaterials();
            CampaignAuthoring.AddCosmetic(player.gameObject);
            foreach(Transform t in All<Transform>().Where(t=>t.name.StartsWith("Waiting rat",StringComparison.OrdinalIgnoreCase)||t.name.StartsWith("Waiting subject",StringComparison.OrdinalIgnoreCase))) CampaignAuthoring.AddCosmetic(t.gameObject);
            Material glass=AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Environment/Observation glass.mat");
            Material rat=AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Characters/RatStylizedLighting.mat");
            Material cyan=AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Environment/Campaign cyan.mat");
            foreach(Renderer r in All<Renderer>())
            {
                if(r.name=="Front observation glass") r.sharedMaterial=glass;
                if(r.name=="Body"||r.name=="Head") r.sharedMaterial=rat;
            }
            foreach(RatPickup station in All<RatPickup>())
                foreach(Renderer r in station.GetComponentsInChildren<Renderer>())r.sharedMaterial=cyan;
            var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            foreach(TextMesh old in All<TextMesh>())
            {
                string text=old.text;Color color=old.color;float size=old.characterSize*old.fontSize;GameObject go=old.gameObject;
                Object.DestroyImmediate(old);Object.DestroyImmediate(go.GetComponent<MeshRenderer>());
                var label=go.AddComponent<TextMeshPro>();label.font=font;label.text=text;label.fontSize=size;label.color=color;
                label.alignment=TextAlignmentOptions.Center;label.textWrappingMode=TextWrappingModes.NoWrap;label.rectTransform.sizeDelta=new Vector2(40,3);
            }
            // Stable IDs 0..19 retain existing purchases and token progress across replacement layouts.
            Vector2[] points=index==0 ? new[]{
                new Vector2(5,1.7f),new Vector2(9,1.7f),new Vector2(16,1.7f),new Vector2(21,1.7f),
                new Vector2(32,1.7f),new Vector2(41,5),new Vector2(35,6.9f),new Vector2(29,6.9f),
                new Vector2(21,6.9f),new Vector2(9,6.9f),new Vector2(11,12.1f),new Vector2(24,12.1f),
                new Vector2(36,12.1f),new Vector2(34,17.3f),new Vector2(23,17.3f),new Vector2(10,17.3f),
                new Vector2(13,23.6f),new Vector2(19,23.6f),new Vector2(30,23.6f),new Vector2(36,23.6f)
            } : new[]{
                new Vector2(4,1.7f),new Vector2(9,2.8f),new Vector2(18,1.7f),new Vector2(23,2.8f),
                new Vector2(33,1.7f),new Vector2(41,5),new Vector2(37,6.9f),new Vector2(28,6.9f),
                new Vector2(18,6.9f),new Vector2(7,6.9f),new Vector2(6,12.1f),new Vector2(9,13.2f),
                new Vector2(21,13.2f),new Vector2(24,14.3f),new Vector2(41,17.7f),new Vector2(38,22.5f),
                new Vector2(30,23.6f),new Vector2(18,23.6f),new Vector2(12,23.6f),new Vector2(6,23.6f)
            };
            CampaignAuthoring.WireAdapted(index,life,points);
            foreach(CampaignHazardCycle cycle in All<CampaignHazardCycle>())
            {
                if(cycle.live==null || cycle.live.GetComponentsInChildren<Transform>(true).All(t=>t.name!="Flame plume"))continue;
                var anchor=new GameObject("Fire VFX socket").transform;
                anchor.SetParent(cycle.live.transform,false);anchor.localPosition=new Vector3(0,-.5f,-.8f);
                // Child activation follows the exact visible/lethal phase; no independent hazard timer.
            }
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("RAT_REFERENCE_ADAPTED: "+CampaignCatalog.Scenes[index]);
        }
        string[] paths=new[]{"Assets/Scenes/Home.unity"}.Concat(CampaignCatalog.Scenes).ToArray();
        EditorBuildSettings.scenes=paths.Select(p=>new EditorBuildSettingsScene(p,true))
            .Concat(EditorBuildSettings.scenes.Where(s=>!paths.Contains(s.path)).Select(s=>new EditorBuildSettingsScene(s.path,false))).ToArray();
        AssetDatabase.SaveAssets();EditorSceneManager.OpenScene("Assets/Scenes/Home.unity");
    }
}

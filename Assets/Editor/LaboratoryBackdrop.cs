using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>Saved, non-colliding laboratory scenery behind the existing playable course.</summary>
public static class LaboratoryBackdrop
{
    public static void Apply(Scene scene)
    {
        Transform root = null;
        foreach (GameObject item in scene.GetRootGameObjects())
            if (item.name == "Laboratory backdrop") root = item.transform;
        if (root == null)
        {
            root = new GameObject("Laboratory backdrop").transform;
            SceneManager.MoveGameObjectToScene(root.gameObject, scene);
        }
        Material wall = ObservationStyle.FlatMaterial("Wall panels", new Color(.32f, .38f, .39f));
        Material weld = ObservationStyle.FlatMaterial("Wall weld", new Color(.27f, .32f, .33f));
        Material frame = ObservationStyle.FlatMaterial("Window frame", new Color(.12f, .17f, .18f));
        Material bevel = ObservationStyle.FlatMaterial("Frame bevel", new Color(.40f, .47f, .48f));
        Material mirror = ObservationStyle.FlatMaterial("Mirror glass", new Color(.085f, .145f, .16f));
        Material sheen = ObservationStyle.FlatMaterial("Mirror sheen", new Color(.11f, .175f, .19f));
        Material floor = ObservationStyle.FlatMaterial("Floor", Color.black);
        Material lamp = ObservationStyle.FlatMaterial("Lab light", new Color(.54f, .63f, .61f));

        bool starter = scene.path == PrototypeAssetPaths.StarterScene;
        float left = starter ? -13f : -3.1f;
        float right = starter ? 95f : 63.1f;
        float centre = (left + right) * .5f;
        float width = right - left;
        float bottom = starter ? .9f : 1.8f;
        float top = starter ? 5.6f : 7.2f;
        float middle = (bottom + top) * .5f;
        const float floorTop = -.65f;

        Box(root, "Laboratory wall", new Vector3(centre, 5, 5.3f), new Vector3(260, 13, .4f), wall);
        Box(root, "Black laboratory floor", new Vector3(centre, floorTop - 10, 4.9f), new Vector3(1000, 20, .4f), floor);
        Box(root, "Wall base joint", new Vector3(centre, floorTop + .035f, 4.7f), new Vector3(260, .065f, .07f), frame);

        // A continuous dark observation pane sits inside a substantial recessed frame.
        // The backing is opaque: the observer room behind this one-way screen is hidden.
        Box(root, "Observation window recess", new Vector3(centre, middle, 4.9f), new Vector3(width + .48f, top - bottom + .48f, .3f), frame);
        Box(root, "One-way observation window", new Vector3(centre, middle, 4.65f), new Vector3(width, top - bottom, .08f), mirror);
        Box(root, "Upper window bevel", new Vector3(centre, top + .06f, 4.5f), new Vector3(width + .2f, .09f, .12f), bevel);
        Box(root, "Window sill", new Vector3(centre, bottom - .13f, 4.4f), new Vector3(width + .5f, .18f, .52f), frame);
        Box(root, "Window sill highlight", new Vector3(centre, bottom - .045f, 4.12f), new Vector3(width + .5f, .035f, .04f), bevel);

        for (int index = 0; left + index * 14f <= right; index++)
        {
            float x = left + index * 14f;
            Box(root, "Window mullion " + index, new Vector3(x, middle, 4.4f), new Vector3(.16f, top - bottom + .3f, .26f), frame);
            // Seams stop at the window frame. They are small dark welds, not a foreground grid.
            Box(root, "Upper wall weld " + index, new Vector3(x, (top + 12) * .5f, 5.05f), new Vector3(.018f, 12 - top, .035f), weld);
            Box(root, "Lower wall weld " + index, new Vector3(x, (bottom + floorTop) * .5f, 5.05f), new Vector3(.018f, bottom - floorTop, .035f), weld);
            if (x + 6 < right)
            {
                Box(root, "Light fixture " + index, new Vector3(x + 5, top + .78f, 4.9f), new Vector3(2.7f, .22f, .18f), frame);
                Box(root, "Recessed lab light " + index, new Vector3(x + 5, top + .74f, 4.78f), new Vector3(2.35f, .045f, .05f), lamp);
                // Restrained stationary reflection on the dark mirror surface.
                var reflection = Box(root, "Mirror reflection " + index,
                    new Vector3(x + 5.2f, middle + .25f, 4.56f), new Vector3(.40f, (top - bottom) * .72f, .025f), sheen);
                reflection.localRotation = Quaternion.Euler(0, 0, 19);
            }
        }
        Box(root, "Right window jamb", new Vector3(right, middle, 4.4f), new Vector3(.16f, top - bottom + .3f, .26f), frame);
    }

    private static Transform Box(Transform root, string name, Vector3 position, Vector3 size, Material material)
    {
        Transform item = root.Find(name);
        if (item == null)
        {
            item = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            item.name = name;
            item.SetParent(root, false);
            Object.DestroyImmediate(item.GetComponent<Collider>());
        }
        item.localPosition = position;
        item.localScale = size;
        var renderer = item.GetComponent<Renderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        return item;
    }
}

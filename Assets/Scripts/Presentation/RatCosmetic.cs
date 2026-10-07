using UnityEngine;

/// <summary>Visual-only fur colours: no colliders or gameplay modifiers.</summary>
public class RatCosmetic : MonoBehaviour
{
    [SerializeField] private Renderer[] fur;
    [SerializeField] private GameObject scarf, vest;
    private MaterialPropertyBlock block;
    private bool applied, rainbow;
    private int luxuryFinish;
    private Color currentTint;
    private Transform patternFrame;
    private Material[][] originalMaterials;
    private Material stripeMaterial;
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int RainbowBlendId = Shader.PropertyToID("_RainbowBlend");
    private static readonly int RainbowPhaseId = Shader.PropertyToID("_RainbowPhase");
    private static readonly int RainbowFrameId = Shader.PropertyToID("_RainbowWorldToRat");
    public void Apply(int outfit)
    {
        applied = true;
        // Gameplay turns the Model child, not the controller root. Using the root
        // projects stripes through body depth and produces rings in the side view.
        patternFrame = transform.Find("Model");
        if (patternFrame == null) patternFrame = transform;
        string id = outfit >= 0 && outfit < CampaignCatalog.OutfitIds.Length ? CampaignCatalog.OutfitIds[outfit] : "classic";
        rainbow = id == "rainbow";
        luxuryFinish = id == "gold" ? 1 : id == "diamond" ? 2 : 0;
        ConfigureStripeMaterials();
        Color tint;
        switch (id)
        {
            case "copper_patch": tint = new Color(.78f,.43f,.23f); break;
            case "silver": tint = new Color(.68f,.75f,.83f); break;
            case "gold": tint = new Color(1f,.66f,.12f); break;
            case "diamond": tint = new Color(.42f,.92f,1f); break;
            case "rainbow": tint = Color.white; break;
            default: tint = new Color(.96f,.96f,.91f); break;
        }
        currentTint = tint;
        Tint(tint);
        // Retain serialized accessory references for existing prefabs, but retire their looks.
        if (scarf != null) scarf.SetActive(false);
        if (vest != null) vest.SetActive(false);
    }
    // Update after movement so the shared stripe frame follows the rat without lag.
    // Unscaled time keeps previews and paused gameplay scrolling as well.
    private void LateUpdate()
    {
        if (rainbow || luxuryFinish != 0) Tint(currentTint);
    }
    private void Tint(Color tint)
    {
        if (block == null) block = new MaterialPropertyBlock();
        if (fur == null) return;
        foreach (Renderer part in fur)
        {
            if (part == null) continue;
            part.GetPropertyBlock(block);
            block.SetColor(ColorId, tint);
            block.SetColor("_BaseColor", tint);
            block.SetFloat(RainbowBlendId, rainbow ? 1f : 0f);
            block.SetFloat("_LuxuryFinish", luxuryFinish);
            block.SetFloat(RainbowPhaseId, Mathf.Repeat(Time.unscaledTime / 4f, 1f));
            block.SetMatrix(RainbowFrameId, patternFrame.worldToLocalMatrix);
            part.SetPropertyBlock(block);
        }
    }
    private void Start() { if (!applied) Apply(CampaignProfile.Equipped); }
    private void ConfigureStripeMaterials()
    {
        if (fur == null) return;
        if (originalMaterials == null)
        {
            originalMaterials = new Material[fur.Length][];
            for (int i = 0; i < fur.Length; i++)
                if (fur[i] != null) originalMaterials[i] = fur[i].sharedMaterials;
        }
        for (int i = 0; i < fur.Length; i++)
        {
            if (fur[i] == null || originalMaterials[i] == null) continue;
            var materials = (Material[])originalMaterials[i].Clone();
            for (int j = 0; (rainbow || luxuryFinish != 0) && j < materials.Length; j++)
            {
                if (materials[j] != null && materials[j].HasProperty(RainbowBlendId)) continue;
                if (stripeMaterial == null)
                {
                    // This shader is included by the campaign scenes' rat materials.
                    Shader shader = Shader.Find("Custom/RatStylizedLighting");
                    if (shader == null) { Debug.LogError("Cosmetic rat shader is missing.", this); return; }
                    stripeMaterial = new Material(shader) { name = "Special outfit fur (runtime)" };
                }
                materials[j] = stripeMaterial;
            }
            fur[i].sharedMaterials = materials;
        }
    }
    private void OnDestroy()
    {
        if (originalMaterials != null && fur != null)
            for (int i = 0; i < fur.Length; i++)
                if (fur[i] != null && originalMaterials[i] != null) fur[i].sharedMaterials = originalMaterials[i];
        if (stripeMaterial != null) Destroy(stripeMaterial);
    }
}

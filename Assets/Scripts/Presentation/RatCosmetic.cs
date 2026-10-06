using UnityEngine;

/// <summary>Visual-only accessories: no colliders or gameplay modifiers.</summary>
public class RatCosmetic : MonoBehaviour
{
    [SerializeField] private Renderer[] fur;
    [SerializeField] private GameObject scarf, vest;
    private MaterialPropertyBlock block;
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    public void Apply(int outfit)
    {
        if (block == null) block = new MaterialPropertyBlock();
        Color tint = outfit == 1 ? new Color(.78f, .43f, .23f) : new Color(.96f, .96f, .91f);
        foreach (Renderer part in fur)
        {
            if (part == null) continue;
            part.GetPropertyBlock(block); block.SetColor(ColorId, tint); block.SetColor("_BaseColor", tint); part.SetPropertyBlock(block);
        }
        if (scarf != null) scarf.SetActive(outfit == 2);
        if (vest != null) vest.SetActive(outfit == 3);
    }
    private void Start() => Apply(CampaignProfile.Equipped);
}

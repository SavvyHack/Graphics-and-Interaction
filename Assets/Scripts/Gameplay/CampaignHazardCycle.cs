// Adapted from https://github.com/SavvyHack/Graphics-and-Interaction/blob/11bf1fb1e1a05e778ecbed0d6a9e4beb3ec54ba9/Assets/Scripts/Gameplay/CampaignHazardCycle.cs
using UnityEngine;

public class CampaignHazardCycle : MonoBehaviour
{
    public GameObject live;
    public Renderer indicator;
    public Material safeMaterial, warningMaterial, liveMaterial;
    public float onSeconds = 1.5f, offSeconds = 2.4f, phase;
    private float elapsed;
    public bool IsLive { get; private set; }
    private void OnEnable() { elapsed = phase; Apply(); }
    private void Update() { elapsed += Time.deltaTime * RatPowerups.WorldScale; Apply(); }
    private void Apply()
    {
        float t = Mathf.Repeat(elapsed, onSeconds + offSeconds);
        IsLive = t < onSeconds;
        if (live != null && live.activeSelf != IsLive) live.SetActive(IsLive);
        if (indicator != null) indicator.sharedMaterial = IsLive ? liveMaterial : t > onSeconds + offSeconds - .65f ? warningMaterial : safeMaterial;
    }
}

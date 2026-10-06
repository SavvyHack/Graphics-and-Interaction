using UnityEngine;
using UnityEngine.EventSystems;

public static class CampaignSettings
{
    public static void Apply()
    {
        RatSettings s = CampaignProfile.Data.settings;
        AudioListener.volume = s.mute ? 0 : s.master;
        // Built-in forward rendering: full-resolution textures and MSAA, no new pipeline.
        QualitySettings.antiAliasing = s.quality == 0 ? 0 : s.quality == 1 ? 4 : 8;
        QualitySettings.globalTextureMipmapLimit = 0;
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
        QualitySettings.shadowResolution = s.quality == 2 ? ShadowResolution.High : ShadowResolution.Medium;
        QualitySettings.shadows = s.quality == 0 ? ShadowQuality.Disable : ShadowQuality.All;
        QualitySettings.vSyncCount = s.vSync ? 1 : 0;
        Application.targetFrameRate = s.vSync ? -1 : 120;
        if (!Application.isEditor && Application.platform != RuntimePlatform.WebGLPlayer) Screen.fullScreen = s.fullscreen;
        AudioManager.Instance?.ApplySettings();
    }
}

/// <summary>Flush slider changes on a completed pointer or keyboard adjustment.</summary>
public class SettingsSliderSave : MonoBehaviour, IPointerUpHandler, IEndDragHandler, IDeselectHandler
{
    public void OnPointerUp(PointerEventData e) => CampaignProfile.Save();
    public void OnEndDrag(PointerEventData e) => CampaignProfile.Save();
    public void OnDeselect(BaseEventData e) => CampaignProfile.Save();
}

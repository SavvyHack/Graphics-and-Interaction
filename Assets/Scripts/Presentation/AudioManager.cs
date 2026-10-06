using UnityEngine;

/// <summary>
/// Small one-shot audio service for Game Systems's prototype systems.
/// The supplied clips are original synthesized placeholders and can be replaced
/// later without changing gameplay code.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip checkpointClip;
    [SerializeField] private AudioClip ratLostClip;
    [SerializeField] private AudioClip completionClip;
    [SerializeField] private AudioClip failureClip;
    [SerializeField] private AudioSource musicSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        ApplySettings();
        if (musicSource != null && musicSource.clip != null) musicSource.Play();
    }

    public void PlayJump() => Play(jumpClip);
    public void PlayCheckpoint() => Play(checkpointClip);
    public void PlayRatLost() => Play(ratLostClip);
    public void PlayCompletion() => Play(completionClip);
    public void PlayFailure() => Play(failureClip);
    public void ApplySettings()
    {
        RatSettings settings = CampaignProfile.Data.settings;
        AudioListener.volume = settings.mute ? 0 : settings.master;
        if (audioSource != null) audioSource.volume = settings.effects;
        if (musicSource != null) musicSource.volume = settings.music;
    }
    private void OnDestroy() { if (Instance == this) Instance = null; }

    private void Play(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}

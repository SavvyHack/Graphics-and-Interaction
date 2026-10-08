using UnityEngine;

/// <summary>
/// Small one-shot audio service for Game Systems's prototype systems.
/// The supplied clips are original synthesized placeholders and can be replaced
/// later without changing gameplay code. The coin cue is a user-supplied clip
/// loaded from Resources (see ResolveCoinClip).
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    // Shared coin pickup cue, loaded only when no per-scene clip was assigned.
    private const string CoinClipResourcePath = "Audio/coin_received";

    // Coin cue loudness relative to the other effects (1 = the clip's own level).
    // PlayOneShot multiplies this by the AudioSource volume, which the Settings page
    // drives with the SFX slider. 0.6 is about -4.4 dB; 0.5 is about -6 dB.
    private const float CoinVolumeScale = 0.6f;

    // Augment pickup cues relative to the other effects. Each augment gets its own
    // synthesized cue (see AugmentCues); this is the shared level for all eleven.
    private const float AugmentVolumeScale = 0.85f;


    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip checkpointClip;
    [SerializeField] private AudioClip ratLostClip;
    [SerializeField] private AudioClip completionClip;
    [SerializeField] private AudioClip failureClip;
    // Optional explicit override. Leave empty to use Resources/Audio/coin_received.
    [SerializeField] private AudioClip coinClip;
    [SerializeField] private AudioClip augmentClip;
    [SerializeField] private AudioSource musicSource;

    private AudioClip coinResourceClip;
    private bool coinResourceResolved;

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
    public void PlayCoin() => Play(ResolveCoinClip(), CoinVolumeScale);

    public void PlayAugment(RatAugment kind) => Play(AugmentCues.Resolve(kind), AugmentVolumeScale);

    public void ApplySettings()
    {
        RatSettings settings = CampaignProfile.Data.settings;
        AudioListener.volume = settings.mute ? 0 : settings.master;
        if (audioSource != null) audioSource.volume = settings.effects;
        if (musicSource != null) musicSource.volume = settings.music;
    }
    private void OnDestroy() { if (Instance == this) Instance = null; }

    private void Play(AudioClip clip) => Play(clip, 1f);

    private void Play(AudioClip clip, float volumeScale)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip, volumeScale);
    }

    /// <summary>
    /// Token pickups use Assets/Resources/Audio/coin_received.mp3, so every level
    /// plays it without per-scene wiring. A scene may still assign coinClip in the
    /// Inspector to override it. This mirrors how the shared menu font is resolved.
    /// </summary>
    private AudioClip ResolveCoinClip()
    {
        if (coinClip != null) return coinClip;

        if (!coinResourceResolved)
        {
            coinResourceResolved = true;
            coinResourceClip = Resources.Load<AudioClip>(CoinClipResourcePath);
            if (coinResourceClip == null)
                Debug.LogWarning($"AudioManager: no coin clip assigned and Resources/{CoinClipResourcePath} is missing; token pickups will be silent.");
        }

        return coinResourceClip;
    }
}

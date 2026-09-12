using UnityEngine;

/// <summary>
/// Small one-shot audio service for Kavish's prototype systems.
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

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void PlayJump() => Play(jumpClip);
    public void PlayCheckpoint() => Play(checkpointClip);
    public void PlayRatLost() => Play(ratLostClip);
    public void PlayCompletion() => Play(completionClip);
    public void PlayFailure() => Play(failureClip);

    private void Play(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}

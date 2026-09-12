using UnityEngine;

/// <summary>
/// Optional audio bridge for the prototype game systems. Assign clips in the
/// Inspector when audio assets are available; missing clips are ignored safely.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip checkpointClip;
    [SerializeField] private AudioClip ratLostClip;
    [SerializeField] private AudioClip completionClip;

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

    public void PlayCheckpoint() => Play(checkpointClip);
    public void PlayRatLost() => Play(ratLostClip);
    public void PlayCompletion() => Play(completionClip);

    private void Play(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}

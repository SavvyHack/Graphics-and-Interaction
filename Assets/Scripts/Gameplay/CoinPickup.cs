using UnityEngine;

[DefaultExecutionOrder(75)]
[RequireComponent(typeof(Collider))]
public class CoinPickup : MonoBehaviour
{
    [SerializeField] private string stableId;
    [SerializeField] private Transform visual;
    [SerializeField] private RatLifeManager lives;
    private bool collected;
    public string StableId => stableId;
    // Lifetime discovery is for the map. Rewards respawn for each new attempt.
    private void Start() { if (CampaignProfile.PickedThisAttempt(stableId)) gameObject.SetActive(false); }
    private void Update()
    {
        if (visual != null && !CampaignProfile.Data.settings.reducedMotion) visual.Rotate(0, 75 * Time.deltaTime, 0, Space.World);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (collected || GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing || lives == null || other.GetComponentInParent<PlayerRatController>() != lives.Player) return;
        if (!CampaignProfile.Collect(stableId)) return;
        collected = true;
        AudioManager.Instance?.PlayCoin();
        gameObject.SetActive(false);
    }
}

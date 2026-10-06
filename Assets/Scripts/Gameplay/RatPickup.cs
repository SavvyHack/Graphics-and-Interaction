// Adapted from https://github.com/SavvyHack/Graphics-and-Interaction/blob/11bf1fb1e1a05e778ecbed0d6a9e4beb3ec54ba9/Assets/Scripts/Gameplay/RatPickup.cs
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class RatPickup : MonoBehaviour
{
    public RatAugment kind;
    public Transform visual;
    public float rechargeSeconds = 3f;
    private float cooldown;
    private Vector3 origin;
    private RatPowerups attractedTo;
    public bool Available => cooldown <= 0;
    private void Awake() { origin = transform.position; GetComponent<SphereCollider>().isTrigger = true; }
    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;
        cooldown = Mathf.Max(0, cooldown - Time.deltaTime);
        if (visual != null)
        {
            visual.gameObject.SetActive(Available);
            if (!CampaignProfile.Data.settings.reducedMotion)
            {
                visual.localPosition = Vector3.up * Mathf.Sin(Time.time * 2.2f + origin.x) * .1f;
                visual.Rotate(0, 35 * Time.deltaTime, 0);
            }
        }
        if (attractedTo != null && Available)
        {
            transform.position = Vector3.MoveTowards(transform.position, attractedTo.transform.position + Vector3.up * .5f, 12 * Time.deltaTime);
            if (Vector3.Distance(transform.position, attractedTo.transform.position + Vector3.up * .5f) < .6f) Take(attractedTo);
        }
    }
    private void OnTriggerEnter(Collider other) { Visit(other); }
    private void OnTriggerStay(Collider other) { Visit(other); }
    private void Visit(Collider other)
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;
        RatPowerups powers = other.GetComponentInParent<RatPowerups>();
        if (powers != null && Available) Take(powers);
    }
    private void Take(RatPowerups powers)
    {
        powers.Collect(kind);
        cooldown = rechargeSeconds;
        attractedTo = null;
        transform.position = origin;
        if (visual != null) visual.gameObject.SetActive(false);
    }
    public void Attract(RatPowerups powers) { if (Available) attractedTo = powers; }
    public void Replenish() { cooldown = 0; attractedTo = null; transform.position = origin; }
}

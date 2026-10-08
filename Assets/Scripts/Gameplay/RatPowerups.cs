// Adapted from https://github.com/SavvyHack/Graphics-and-Interaction/blob/11bf1fb1e1a05e778ecbed0d6a9e4beb3ec54ba9/Assets/Scripts/Gameplay/RatPowerups.cs
using System;
using UnityEngine;
using UnityEngine.InputSystem;

// New augments are appended so existing serialized RatPickup kinds keep their values.
public enum RatAugment { Jetpack, DoubleJump, Shield, SpeedBoost, GravityPulse, SlowTime, Dash, WallJump, Glide, Phase, GroundPound }

/// <summary>Temporary, independent augmentations. World time never changes player input or movement.</summary>
[DefaultExecutionOrder(-150)]
public class RatPowerups : MonoBehaviour
{
    public static RatPowerups Active { get; private set; }
    public static float WorldScale => Active != null && Active.Has(RatAugment.SlowTime) ? .28f : 1f;
    public static readonly string[] Names = { "JETPACK", "DOUBLE JUMP", "SHIELD", "SPEED BOOST", "GRAVITY PULSE", "SLOW TIME", "DASH", "WALL JUMP", "GLIDE", "PHASE", "GROUND POUND" };
    public static readonly string[] Hints = {
        "Hold SPACE in the air to fly. Watch your fuel.", "Press SPACE again in the air to jump a second time.",
        "Absorbs one hazard hit. Keep moving through the barrier.", "Move faster. Jump at the edge to clear the long gap.",
        "Press E near a magnetic gate to push it open and attract orbs.", "Machinery slows down. Your movement stays at full speed.",
        "Press Q to burst forwards. One dash per jump.", "Hold towards a wall to slide, then press SPACE to kick off it.",
        "Hold SPACE while falling to glide. Updrafts lift a gliding rat.", "Pass through violet barriers and machinery. Coolant still drowns.",
        "Press S in mid-air to slam down and smash cracked hatches." };
    public static readonly float[] Durations = { 12, 18, 18, 12, 20, 10, 15, 18, 15, 7, 15 };
    private readonly float[] remaining = new float[Names.Length];
    private RatLifeManager lives;
    private float shieldGrace;
    private float pulseVisual;
    public Transform shieldVisual, jetVisual, pulseRing;
    public Transform dashVisual, glideVisual, phaseVisual;
    private PlayerRatController controller;
    public const float MaxJetFuel = 1.55f;
    public float JetFuel { get; private set; }
    public float SpeedMultiplier => Has(RatAugment.SpeedBoost) ? 1.65f : 1f;
    public event Action<RatAugment> Collected;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetActive() { Active = null; }
    private void Awake() { Active = this; controller = GetComponent<PlayerRatController>(); }
    private void Start()
    {
        lives = FindFirstObjectByType<RatLifeManager>();
        if (lives != null) lives.OnLifeLost += OnLifeLost;
    }
    private void OnDestroy()
    {
        if (lives != null) lives.OnLifeLost -= OnLifeLost;
        if (Active == this) Active = null;
    }
    public bool Has(RatAugment kind) => remaining[(int)kind] > 0;
    public float Remaining(RatAugment kind) => remaining[(int)kind];
    public void Collect(RatAugment kind)
    {
        remaining[(int)kind] = Durations[(int)kind];
        if (kind == RatAugment.Jetpack) JetFuel = MaxJetFuel;
        AudioManager.Instance?.PlayAugment(kind);
        Collected?.Invoke(kind);
    }
    private void Update()
    {
        if (Time.deltaTime <= 0 || (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)) return;
        for (int i = 0; i < remaining.Length; i++) remaining[i] = Mathf.Max(0, remaining[i] - Time.deltaTime);
        shieldGrace = Mathf.Max(0, shieldGrace - Time.deltaTime);
        pulseVisual = Mathf.Max(0, pulseVisual - Time.deltaTime);
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) Pulse();
        if (shieldVisual != null) shieldVisual.gameObject.SetActive(Has(RatAugment.Shield) || shieldGrace > 0);
        if (jetVisual != null) jetVisual.gameObject.SetActive(Has(RatAugment.Jetpack) && JetFuel > 0 && Keyboard.current != null && Keyboard.current.spaceKey.isPressed);
        if (pulseRing != null)
        {
            pulseRing.gameObject.SetActive(pulseVisual > 0);
            pulseRing.localScale = Vector3.one * Mathf.Lerp(12, .3f, pulseVisual / .5f);
        }
        if (dashVisual != null) dashVisual.gameObject.SetActive(controller != null && controller.IsDashing);
        if (glideVisual != null) glideVisual.gameObject.SetActive(controller != null && controller.IsGliding);
        if (phaseVisual != null) phaseVisual.gameObject.SetActive(Has(RatAugment.Phase));
    }
    public bool UseJet(float delta)
    {
        if (!Has(RatAugment.Jetpack) || JetFuel <= 0) return false;
        JetFuel = Mathf.Max(0, JetFuel - delta);
        return true;
    }
    public bool AbsorbHit()
    {
        if (shieldGrace > 0) return true;
        if (!Has(RatAugment.Shield)) return false;
        remaining[(int)RatAugment.Shield] = 0;
        shieldGrace = 1.25f;
        AudioManager.Instance?.PlayCheckpoint();
        return true;
    }
    public bool Pulse()
    {
        if (!Has(RatAugment.GravityPulse)) return false;
        remaining[(int)RatAugment.GravityPulse] = 0;
        pulseVisual = .5f;
        foreach (CampaignPulseGate gate in FindObjectsByType<CampaignPulseGate>(FindObjectsSortMode.None))
            if (Vector3.Distance(transform.position, gate.transform.position) < 6) gate.Repel();
        foreach (RatPickup orb in FindObjectsByType<RatPickup>(FindObjectsSortMode.None))
            if (Vector3.Distance(transform.position, orb.transform.position) < 6) orb.Attract(this);
        AudioManager.Instance?.PlayCheckpoint();
        return true;
    }
    private void OnLifeLost(int unused) { ResetPowers(); }
    public void ResetPowers()
    {
        Array.Clear(remaining, 0, remaining.Length);
        JetFuel = shieldGrace = pulseVisual = 0;
        if (shieldVisual != null) shieldVisual.gameObject.SetActive(false);
        if (jetVisual != null) jetVisual.gameObject.SetActive(false);
        if (pulseRing != null) pulseRing.gameObject.SetActive(false);
        if (dashVisual != null) dashVisual.gameObject.SetActive(false);
        if (glideVisual != null) glideVisual.gameObject.SetActive(false);
        if (phaseVisual != null) phaseVisual.gameObject.SetActive(false);
        // Smashed hatches return so a new rat finds the room as it was.
        foreach (BreakableHatch hatch in FindObjectsByType<BreakableHatch>(FindObjectsInactive.Include, FindObjectsSortMode.None)) hatch.Restore();
        // Stations replenish on a new life, including those immediately before a checkpoint.
        foreach (RatPickup orb in FindObjectsByType<RatPickup>(FindObjectsSortMode.None)) orb.Replenish();
    }
}


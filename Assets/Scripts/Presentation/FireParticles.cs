using UnityEngine;

/// <summary>
/// Keeps a fire trap's particles in step with the trap. The prefab sits inside the
/// hazard's live object, so it only plays while the fire is lethal. Pause needs no
/// code: the system runs on scaled time and freezes when Time.timeScale is 0.
/// This script only matches Slow Time, which slows machinery but not Time.timeScale.
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class FireParticles : MonoBehaviour
{
    private ParticleSystem.MainModule main;

    private void Awake() { main = GetComponent<ParticleSystem>().main; }

    private void Update() { main.simulationSpeed = RatPowerups.WorldScale; }
}

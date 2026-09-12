using UnityEngine;

/// <summary>
/// Cycles a modular hazard between dangerous and safe states. The visual effect
/// and trigger colliders are enabled together, so the gameplay state always
/// matches what the player can see.
/// </summary>
[DisallowMultipleComponent]
public class KavishTimedHazard : MonoBehaviour
{
    [SerializeField] private GameObject[] activeObjects;
    [SerializeField, Min(0.1f)] private float activeDuration = 1.5f;
    [SerializeField, Min(0.1f)] private float inactiveDuration = 1.0f;
    [SerializeField, Min(0f)] private float phaseOffset;
    [SerializeField] private bool startActive = true;

    private bool? currentState;

    public bool IsActive => currentState ?? startActive;

    private void OnEnable()
    {
        currentState = null;
        UpdateState();
    }

    private void Update()
    {
        UpdateState();
    }

    private void OnDisable()
    {
        SetActiveState(false);
    }

    private void UpdateState()
    {
        float period = Mathf.Max(0.2f, activeDuration + inactiveDuration);
        float timeInCycle = Mathf.Repeat(Time.time + phaseOffset, period);
        bool active = startActive
            ? timeInCycle < activeDuration
            : timeInCycle >= inactiveDuration;
        SetActiveState(active);
    }

    private void SetActiveState(bool active)
    {
        if (currentState == active)
            return;

        currentState = active;
        if (activeObjects == null)
            return;

        foreach (GameObject item in activeObjects)
        {
            if (item != null && item.activeSelf != active)
                item.SetActive(active);
        }
    }
}

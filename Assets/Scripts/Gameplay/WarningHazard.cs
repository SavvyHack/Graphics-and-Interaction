using UnityEngine;

/// <summary>Three readable phases with matching visible and lethal volumes.</summary>
public class WarningHazard : MonoBehaviour
{
    [SerializeField] private GameObject activeVolume;
    [SerializeField] private Renderer beacon;
    [SerializeField] private Material safe, warning, danger;
    [SerializeField] private float safeSeconds = 3f, warningSeconds = 1f, activeSeconds = 1.5f;
    [SerializeField] private Vector3 sweep;
    private float elapsed;
    private Vector3 origin;
    private void Awake() { origin=activeVolume.transform.localPosition; ResetCycle(); }
    private void Update()
    {
        if(GameManager.Instance!=null && GameManager.Instance.CurrentState!=GameManager.GameState.Playing) return;
        elapsed+=Time.deltaTime;
        float phase=Mathf.Repeat(elapsed,safeSeconds+warningSeconds+activeSeconds);
        bool lethal=phase>=safeSeconds+warningSeconds;
        activeVolume.SetActive(lethal);
        beacon.sharedMaterial=lethal?danger:phase>=safeSeconds?warning:safe;
        activeVolume.transform.localPosition=origin+(lethal?sweep*Mathf.Clamp01((phase-safeSeconds-warningSeconds)/activeSeconds):Vector3.zero);
    }
    public void ResetCycle() { elapsed=0;activeVolume.SetActive(false);activeVolume.transform.localPosition=origin;beacon.sharedMaterial=safe; }
}

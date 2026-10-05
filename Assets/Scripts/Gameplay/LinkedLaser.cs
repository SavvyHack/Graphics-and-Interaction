using UnityEngine;

// The same two endpoints drive both the visible beam and the lethal volume.
[ExecuteAlways]
public class LinkedLaser : MonoBehaviour
{
    public Transform emitter;
    public Transform receiver;
    public Transform beam;
    public Renderer beamRenderer;
    public BoxCollider lethalCollider;
    public Renderer[] deviceLights;
    public Material activeLight;
    public Material safeLight;
    [Min(0.01f)] public float beamWidth = 0.12f;
    private bool suppressed;
    public void SetSuppressed(bool value)
    {
        suppressed = value;
        Apply();
    }
    private void OnEnable() { Apply(); }
    private void LateUpdate() { Apply(); }
    private void OnDisable()
    {
        if (beamRenderer != null) beamRenderer.enabled = false;
        if (lethalCollider != null) lethalCollider.enabled = false;
    }
    private void Apply()
    {
        if (emitter == null || receiver == null || beam == null) return;
        Vector3 delta = receiver.position - emitter.position;
        float length = delta.magnitude;
        beam.position = (emitter.position + receiver.position) * 0.5f;
        beam.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
        // Beam quad is unit-sized, with local X between the endpoints.
        beam.localScale = new Vector3(length, beamWidth, 1f);
        bool active = isActiveAndEnabled && !suppressed && length > 0.01f;
        if (beamRenderer != null) beamRenderer.enabled = active;
        if (lethalCollider != null)
        {
            lethalCollider.size = new Vector3(1f, 1f, 1.4f);
            lethalCollider.enabled = active;
        }
        if (deviceLights != null)
            foreach (Renderer light in deviceLights)
                if (light != null) light.sharedMaterial = active ? activeLight : safeLight;
    }
}

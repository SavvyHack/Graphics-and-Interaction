using UnityEngine;

/// <summary>Predictable shuttle motion. The rat controller consumes Delta once per frame.</summary>
[DefaultExecutionOrder(-100)]
public class TrialMovingPlatform : MonoBehaviour
{
    public Vector3 travel = new Vector3(5f, 0f, 0f);
    [Min(1f)] public float period = 6f;
    public Vector3 Delta { get; private set; }
    private Vector3 origin;
    private float elapsed;

    private void Awake() { origin = transform.position; }
    private void Update()
    {
        elapsed += Time.deltaTime;
        Vector3 next = origin + travel * (0.5f - 0.5f * Mathf.Cos(elapsed * Mathf.PI * 2f / period));
        Delta = next - transform.position;
        transform.position = next;
    }
}

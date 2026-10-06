using UnityEngine;

// A heavy sliding crate, constrained to a short, supported puzzle track.
// Uses casts rather than unconstrained physics so it cannot be lost off a ledge.
[RequireComponent(typeof(BoxCollider), typeof(Rigidbody))]
public class PushBlock : MonoBehaviour
{
    [Range(0.1f, 1f)] public float speedMultiplier = 0.55f;
    public float travelLeft = 1f;
    public float travelRight = 3f;
    private Vector3 origin;
    private BoxCollider box;
    private Rigidbody body;
    private void Awake()
    {
        origin = transform.position;
        box = GetComponent<BoxCollider>();
        body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
        body.constraints = RigidbodyConstraints.FreezeRotation;
    }
    public float Push(float requested, Collider pusher)
    {
        float target = Mathf.Clamp(transform.position.x + requested,
            origin.x - travelLeft, origin.x + travelRight);
        float delta = target - transform.position.x;
        if (Mathf.Abs(delta) < 0.0001f) return 0f;
        Vector3 direction = Vector3.right * Mathf.Sign(delta);
        Bounds bounds = box.bounds;
        float allowed = Mathf.Abs(delta);
        // Shrink the sweep slightly so the supporting deck is not a side obstacle.
        foreach (RaycastHit hit in Physics.BoxCastAll(bounds.center,
            bounds.extents * 0.96f, direction, Quaternion.identity,
            allowed + 0.03f, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider == box || hit.collider == pusher ||
                hit.collider.transform.IsChildOf(transform)) continue;
            allowed = Mathf.Min(allowed, Mathf.Max(0f, hit.distance - 0.03f));
        }
        body.position += direction * allowed;
        Physics.SyncTransforms();
        return allowed * Mathf.Sign(delta);
    }
    public void ResetToOrigin()
    {
        body.position = origin;
        transform.position = origin;
    }
}

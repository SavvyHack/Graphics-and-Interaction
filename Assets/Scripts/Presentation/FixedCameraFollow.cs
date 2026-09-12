using UnityEngine;

/// <summary>
/// Fixed side-on 2.5D camera for Project R.A.T.
/// The camera follows the active rat while preserving its original Z position
/// and rotation so the game always keeps the outside-the-glass perspective.
/// </summary>
public class FixedCameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Follow")]
    [SerializeField] private Vector2 followOffset = new Vector2(2.0f, 1.0f);
    [SerializeField, Min(0.01f)] private float smoothTime = 0.15f;
    [SerializeField] private bool followVertically = true;

    [Header("Optional Camera Bounds")]
    [SerializeField] private bool clampToBounds = false;
    [SerializeField] private Vector2 minBounds = new Vector2(-100f, -100f);
    [SerializeField] private Vector2 maxBounds = new Vector2(100f, 100f);

    private Vector3 velocity;
    private float fixedZ;
    private Quaternion fixedRotation;
    private float fixedY;

    private static readonly int GlassActiveRatPosition = Shader.PropertyToID("_GlassActiveRatPosition");

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetGlassTarget()
    {
        Shader.SetGlobalVector(GlassActiveRatPosition, Vector4.zero);
    }

    public Transform Target => target;

    private void Awake()
    {
        fixedZ = transform.position.z;
        fixedY = transform.position.y;
        fixedRotation = transform.rotation;
    }

    private void LateUpdate()
    {
        UpdateGlassTarget();
        if (target == null)
            return;

        float desiredX = target.position.x + followOffset.x;
        float desiredY = followVertically
            ? target.position.y + followOffset.y
            : fixedY;

        if (clampToBounds)
        {
            desiredX = Mathf.Clamp(desiredX, minBounds.x, maxBounds.x);
            desiredY = Mathf.Clamp(desiredY, minBounds.y, maxBounds.y);
        }

        Vector3 desiredPosition = new Vector3(desiredX, desiredY, fixedZ);

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            smoothTime
        );

        // Prevent any accidental rotation caused by other scripts or hierarchy changes.
        transform.rotation = fixedRotation;
    }

    /// <summary>
    /// Call this when the active rat changes.
    /// </summary>
    public void SetTarget(Transform newTarget, bool snapImmediately = false)
    {
        target = newTarget;
        velocity = Vector3.zero;
        UpdateGlassTarget();

        if (snapImmediately && target != null)
        {
            float newY = followVertically ? target.position.y + followOffset.y : fixedY;
            float newX = target.position.x + followOffset.x;

            if (clampToBounds)
            {
                newX = Mathf.Clamp(newX, minBounds.x, maxBounds.x);
                newY = Mathf.Clamp(newY, minBounds.y, maxBounds.y);
            }

            transform.position = new Vector3(newX, newY, fixedZ);
            transform.rotation = fixedRotation;
        }
    }

    // Both the prototype session and team life manager already switch this target.
    // Publish after player movement, without material instances or scene searches.
    private void UpdateGlassTarget()
    {
        if (target == null || !target.gameObject.activeInHierarchy || !isActiveAndEnabled)
        {
            ResetGlassTarget();
            return;
        }

        Vector3 position = target.position;
        Shader.SetGlobalVector(GlassActiveRatPosition, new Vector4(position.x, position.y, position.z, 1f));
    }

    private void OnDisable()
    {
        ResetGlassTarget();
    }
}

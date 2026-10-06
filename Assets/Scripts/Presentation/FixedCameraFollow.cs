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
    [Tooltip("Keep the rat and nearby route visible when the game window is narrow.")]
    [SerializeField, Min(0f)] private float minimumHorizontalView = 16f;

    [Header("Optional Camera Bounds")]
    [SerializeField] private bool clampToBounds = false;
    [SerializeField] private Vector2 minBounds = new Vector2(-100f, -100f);
    [SerializeField] private Vector2 maxBounds = new Vector2(100f, 100f);

    private Vector3 velocity;
    private float fixedZ;
    private Quaternion fixedRotation;
    private float fixedY;
    private bool initialized;
    private Camera viewCamera;
    private float authoredSize;

    private static readonly int GlassActiveRatPosition = Shader.PropertyToID("_GlassActiveRatPosition");

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetGlassTarget()
    {
        Shader.SetGlobalVector(GlassActiveRatPosition, Vector4.zero);
    }

    public Transform Target => target;

    private void Awake()
    {
        InitializePose();
    }

    private void InitializePose()
    {
        if (initialized) return;
        fixedZ = transform.position.z;
        fixedY = transform.position.y;
        fixedRotation = transform.rotation;
        viewCamera = GetComponent<Camera>();
        if (viewCamera != null) authoredSize = viewCamera.orthographicSize;
        initialized = true;
    }

    private void LateUpdate()
    {
        if (viewCamera != null && viewCamera.orthographic)
            viewCamera.orthographicSize = Mathf.Max(authoredSize, minimumHorizontalView / (2f * Mathf.Max(.1f, viewCamera.aspect)));
        UpdateGlassTarget();
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            velocity = Vector3.zero;
            return;
        }

        // Smooth only on the gameplay plane. Depth and rotation are hard invariants.
        // SmoothDamp can produce a non-finite velocity for zero delta time.
        Vector3 next = transform.position;
        if (Time.deltaTime > 0f)
            next = Vector3.SmoothDamp(next, DesiredPosition(),
                ref velocity, Mathf.Max(0.01f, smoothTime), Mathf.Infinity, Time.deltaTime);
        next.z = fixedZ;
        if (!followVertically) next.y = fixedY;
        if (clampToBounds) next = ClampPosition(next);
        velocity.z = 0f;
        transform.SetPositionAndRotation(next, fixedRotation);
    }

    private Vector3 DesiredPosition()
    {
        float desiredX = target.position.x + followOffset.x;
        float desiredY = followVertically
            ? target.position.y + followOffset.y
            : fixedY;

        Vector3 desiredPosition = new Vector3(desiredX, desiredY, fixedZ);
        return clampToBounds ? ClampPosition(desiredPosition) : desiredPosition;
    }

    private Vector3 ClampPosition(Vector3 position)
    {
        position.x = Mathf.Clamp(position.x, Mathf.Min(minBounds.x, maxBounds.x), Mathf.Max(minBounds.x, maxBounds.x));
        if (followVertically)
            position.y = Mathf.Clamp(position.y, Mathf.Min(minBounds.y, maxBounds.y), Mathf.Max(minBounds.y, maxBounds.y));
        return position;
    }

    /// <summary>
    /// Call this when the active rat changes.
    /// </summary>
    public void SetTarget(Transform newTarget, bool snapImmediately = false)
    {
        InitializePose();
        target = newTarget;
        velocity = Vector3.zero;
        UpdateGlassTarget();

        if (snapImmediately && target != null)
        {
            transform.SetPositionAndRotation(DesiredPosition(), fixedRotation);
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
        velocity = Vector3.zero;
        ResetGlassTarget();
    }
}

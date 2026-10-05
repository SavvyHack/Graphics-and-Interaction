using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class PressurePlate : MonoBehaviour
{
    public LinkedLaser laser;
    public Transform top;
    public Renderer indicator;
    public Material idleMaterial;
    public Material pressedMaterial;
    public bool IsPressed { get; private set; }
    private BoxCollider sensor;
    private Vector3 topRest;
    private void Awake()
    {
        sensor = GetComponent<BoxCollider>();
        sensor.isTrigger = true;
        if (top != null) topRest = top.localPosition;
        if (laser != null) laser.SetSuppressed(false);
    }
    private void FixedUpdate()
    {
        bool pressed = false;
        Vector3 scale = transform.lossyScale;
        Vector3 half = Vector3.Scale(sensor.size * 0.5f,
            new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z)));
        foreach (Collider item in Physics.OverlapBox(transform.TransformPoint(sensor.center),
            half, transform.rotation, ~0, QueryTriggerInteraction.Ignore))
        {
            if (item.GetComponentInParent<PushBlock>() == null) continue;
            // Require the crate centre on the plate, not just a touching corner.
            Vector3 local = transform.InverseTransformPoint(item.bounds.center) - sensor.center;
            if (Mathf.Abs(local.x) <= sensor.size.x * 0.5f &&
                Mathf.Abs(local.z) <= sensor.size.z * 0.5f) pressed = true;
        }
        IsPressed = pressed;
        if (laser != null) laser.SetSuppressed(pressed);
        if (top != null) top.localPosition = topRest + (pressed ? Vector3.down * 0.045f : Vector3.zero);
        if (indicator != null) indicator.sharedMaterial = pressed ? pressedMaterial : idleMaterial;
    }
    private void OnDisable()
    {
        if (laser != null) laser.SetSuppressed(false);
    }
}

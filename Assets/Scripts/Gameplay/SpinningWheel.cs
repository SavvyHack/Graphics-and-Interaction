using System.Runtime.CompilerServices;
using UnityEngine;

public class SpinningWheel : MonoBehaviour
{

    [SerializeField] private float rotationSpeed = 45f;
    [SerializeField] private bool usePhysics = true;

    private Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!usePhysics || rb == null)
        {
            transform.Rotate(0f, 0f, -1 * rotationSpeed * Time.deltaTime);
        }
    }

    void FixedUpdate()
    {
        if (usePhysics && rb != null)
        {
            Quaternion deltaRotation = Quaternion.Euler(0f, 0f, -1 * rotationSpeed * Time.deltaTime);
            rb.MoveRotation(rb.rotation * deltaRotation);
        }
    }
}

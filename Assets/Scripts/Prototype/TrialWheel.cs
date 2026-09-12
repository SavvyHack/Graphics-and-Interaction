using UnityEngine;

public class TrialWheel : MonoBehaviour
{
    public float degreesPerSecond = 55f;
    private void Update() { transform.Rotate(0f, 0f, degreesPerSecond * Time.deltaTime); }
}

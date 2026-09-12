#if UNITY_EDITOR
using UnityEngine;

// Added only by the batch test runner. Excluded from player builds and saved scenes.
[DefaultExecutionOrder(1000)]
public class RatPlaytestDriver : MonoBehaviour
{
    public System.Action step;
    private void Update() { step?.Invoke(); }
}
#endif

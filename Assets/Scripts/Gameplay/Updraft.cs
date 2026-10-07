using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rising air column. It only lifts a rat that is gliding (Glide augment, holding Space);
/// everyone else falls through it normally. PlayerRatController asks Headroom() each frame.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class Updraft : MonoBehaviour
{
    private static readonly List<Updraft> active = new List<Updraft>();
    private BoxCollider column;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetList() { active.Clear(); }

    private void Awake()
    {
        column = GetComponent<BoxCollider>();
        column.isTrigger = true;
    }

    private void OnEnable() { active.Add(this); }
    private void OnDisable() { active.Remove(this); }

    /// <summary>Height left to the top of the column containing point, or -1 when outside every column.</summary>
    public static float Headroom(Vector3 point)
    {
        float best = -1f;
        foreach (Updraft updraft in active)
            if (updraft.column.bounds.Contains(point)) best = Mathf.Max(best, updraft.column.bounds.max.y - point.y);
        return best;
    }
}

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rising air column. It only lifts a rat that is gliding (Glide augment, holding Space);
/// everyone else falls through it normally. PlayerRatController asks Lifts() each frame.
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

    public static bool Lifts(Vector3 point)
    {
        foreach (Updraft updraft in active)
            if (updraft.column.bounds.Contains(point)) return true;
        return false;
    }
}

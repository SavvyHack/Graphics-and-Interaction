// Adapted from https://github.com/SavvyHack/Graphics-and-Interaction/blob/11bf1fb1e1a05e778ecbed0d6a9e4beb3ec54ba9/Assets/Scripts/Gameplay/CampaignPulseGate.cs
using UnityEngine;

/// <summary>Magnetic panel is held open after a pulse; it cannot reclose on a rat.</summary>
public class CampaignPulseGate : MonoBehaviour
{
    public GameObject barrier;
    public Transform panel;
    private bool opened;
    private Vector3 closedPosition;
    public bool IsOpen => opened;
    private void Awake() { if (panel != null) closedPosition = panel.localPosition; }
    public void Repel() { opened = true; if (barrier != null) barrier.SetActive(false); }
    private void Update()
    {
        if (opened && panel != null)
            panel.localPosition = Vector3.MoveTowards(panel.localPosition, closedPosition + Vector3.up * 3.5f, Time.deltaTime * 6);
    }
}

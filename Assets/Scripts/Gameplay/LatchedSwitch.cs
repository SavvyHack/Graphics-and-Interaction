using UnityEngine;
using UnityEngine.InputSystem;

public class LatchedSwitch : MonoBehaviour
{
    [SerializeField] private GameObject shutter;
    [SerializeField] private Renderer indicator;
    [SerializeField] private Material openMaterial, closedMaterial;
    [SerializeField] private RatLifeManager lives;
    public bool Open { get; private set; }
    public string Prompt { get; private set; }
    private void Update()
    {
        Prompt = null;
        if (Open || lives == null || lives.Player == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;
        if (Vector2.Distance(lives.Player.transform.position, transform.position) > 1.5f) return;
        Prompt = "E - Release shutter";
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) { SetOpen(true); AudioManager.Instance?.PlayCheckpoint(); }
    }
    public void SetOpen(bool value)
    {
        Open = value;
        shutter.SetActive(!value);
        indicator.sharedMaterial = value ? openMaterial : closedMaterial;
    }
}

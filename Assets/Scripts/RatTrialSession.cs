using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>Small, replaceable playtest harness for the level. No dependency on a future team life manager.</summary>
public class RatTrialSession : MonoBehaviour
{
    public PlayerRatController player;
    public Transform startPoint;
    public GameObject[] waitingRats;
    public FixedCameraFollow cameraFollow;
    public PrototypeHUD teamHUD;
    public int RemainingRats { get; private set; } = 3;
    public int CheckpointNumber { get; private set; }
    public bool Finished { get; private set; }
    private Vector3 respawn;
    private float protectedUntil;
    private string message = "Follow the illuminated platform edges to the exit.";
    private GUIStyle titleStyle, textStyle, smallStyle;

    private void Start()
    {
        respawn = startPoint.position;
        cameraFollow.SetTarget(player.transform, true);
        if (teamHUD != null) teamHUD.ResetHUD();
    }
    private void Update()
    {
        if (!Finished && player.transform.position.y < -4f) LoseRat();
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            SceneManager.LoadScene(SceneManager.GetActiveScene().path);
    }
    public void Checkpoint(int number, Vector3 position)
    {
        if (Finished || number <= CheckpointNumber) return;
        CheckpointNumber = number;
        respawn = position;
        message = "Checkpoint secured. The next rat will start here.";
        if (teamHUD != null) teamHUD.SetCheckpoint(number);
    }
    public void LoseRat()
    {
        if (Finished || Time.time < protectedUntil) return;
        RemainingRats--;
        if (teamHUD != null) teamHUD.SetRemainingRats(RemainingRats);
        if (RemainingRats == 0)
        {
            Finished = true;
            player.enabled = false;
            message = "ALL RATS LOST";
            if (teamHUD != null) teamHUD.ShowFailure();
            return;
        }
        int released = 2 - RemainingRats;
        if (waitingRats != null && released < waitingRats.Length) waitingRats[released].SetActive(false);
        player.Respawn(respawn);
        cameraFollow.SetTarget(player.transform, true);
        protectedUntil = Time.time + 0.7f;
        message = "Next rat released at the last checkpoint.";
    }
    public void Complete()
    {
        if (Finished) return;
        Finished = true;
        player.enabled = false;
        message = "ENCLOSURE CLEARED";
        if (teamHUD != null) teamHUD.ShowCompletion();
    }
    private void OnGUI()
    {
        if (titleStyle == null)
        {
            titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold };
            textStyle = new GUIStyle(GUI.skin.label) { fontSize = 16 };
            smallStyle = new GUIStyle(GUI.skin.label) { fontSize = 12 };
            titleStyle.normal.textColor = textStyle.normal.textColor = new Color(.94f, .98f, 1f);
            smallStyle.normal.textColor = new Color(.65f, .78f, .84f);
        }
        Matrix4x4 previous = GUI.matrix;
        float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * scale);
        float width = Screen.width / scale;
        float height = Screen.height / scale;
        GUI.Box(new Rect(18, 18, 315, 96), GUIContent.none);
        GUI.Label(new Rect(34, 27, 280, 34), "PROJECT R.A.T.", titleStyle);
        GUI.Label(new Rect(34, 66, 285, 26), "RATS  " + RemainingRats + " / 3     |     CHECKPOINT  " + CheckpointNumber, textStyle);
        GUI.Label(new Rect(width - 323, 29, 305, 26), "01 / OBSERVATION ENCLOSURE", textStyle);
        GUI.Box(new Rect(18, height - 67, width - 36, 49), GUIContent.none);
        GUI.Label(new Rect(34, height - 56, 620, 25), message, textStyle);
        GUI.Label(new Rect(width - 440, height - 53, 410, 25), "A / D  MOVE     SPACE  JUMP     SHIFT  SPRINT     R  RESTART", smallStyle);
        if (Finished)
        {
            GUI.Box(new Rect(width / 2 - 230, height / 2 - 72, 460, 144), GUIContent.none);
            GUI.Label(new Rect(width / 2 - 180, height / 2 - 47, 410, 38), message, titleStyle);
            GUI.Label(new Rect(width / 2 - 180, height / 2 + 5, 410, 30), "Press R to start a new three-rat attempt.", textStyle);
        }
        GUI.matrix = previous;
    }
}

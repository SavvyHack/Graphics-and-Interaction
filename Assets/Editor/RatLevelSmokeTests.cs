using System;
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

/// <summary>Batch play-mode checks against the real CharacterController and input system.</summary>
[InitializeOnLoad]
public static class RatLevelSmokeTests
{
    private static IEnumerator checks;
    private static Keyboard keyboard;
    private static PlayerRatController player;
    private static RatTrialSession session;
    private static double deadline;
    static RatLevelSmokeTests() { EditorApplication.update += Tick; }

    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/PrototypeLevel/RatEnclosure.unity");
        SessionState.SetBool("RatSmokeArmed", true);
        EditorApplication.EnterPlaymode();
    }
    private static void Tick()
    {
        if (!SessionState.GetBool("RatSmokeArmed", false) || !EditorApplication.isPlaying) return;
        if (checks != null) return;
        try
        {
            if (checks == null)
            {
                deadline = EditorApplication.timeSinceStartup + 120;
                Time.captureDeltaTime = 1f / 60f;
                Application.runInBackground = true;
                InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                keyboard = InputSystem.AddDevice<Keyboard>();
                player = UnityEngine.Object.FindFirstObjectByType<PlayerRatController>();
                session = UnityEngine.Object.FindFirstObjectByType<RatTrialSession>();
                checks = Check();
                new GameObject("Batch playtest driver").AddComponent<RatPlaytestDriver>().step = Advance;
            }
        }
        catch (Exception error) { Debug.LogException(error); Finish(1); }
    }
    private static void Advance()
    {
        try
        {
            if (EditorApplication.timeSinceStartup > deadline)
                throw new Exception("Playtest timed out at " + player.transform.position + ", D=" + Keyboard.current.dKey.isPressed);
            if (!checks.MoveNext()) Finish(0);
        }
        catch (Exception error) { Debug.LogException(error); Finish(1); }
    }
    private static void Finish(int result)
    {
        SessionState.SetBool("RatSmokeArmed", false);
        if (keyboard != null) InputSystem.RemoveDevice(keyboard);
        Debug.Log(result == 0 ? "RAT_PLAYTEST_OK" : "RAT_PLAYTEST_FAILED");
        EditorApplication.Exit(result);
    }
    private static void Input(params Key[] keys) { InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys)); keyboard.MakeCurrent(); }
    private static IEnumerator Wait(float seconds)
    {
        float end = Time.time + seconds;
        while (Time.time < end) yield return null;
    }
    private static IEnumerator Check()
    {
        IEnumerator wait = Wait(.5f); while (wait.MoveNext()) yield return null;
        Require(player.GetComponent<CharacterController>().isGrounded, "Rat spawns on the release deck");
        // Each gap is exercised with the existing walking speed; sprint is unnecessary.
        float[,] jumps = { {4, 0, 4.7f, 7.8f, .4f}, {8.5f,.4f,9.2f,12.3f,.8f}, {13,.8f,13.7f,16.7f,.8f}, {47,1.4f,47.7f,50.7f,2.3f}, {51,2.3f,51.7f,54.7f,3.2f}, {55,3.2f,55.7f,58.7f,4.1f} };
        for (int i = 0; i < jumps.GetLength(0); i++)
        {
            Input(); player.Respawn(new Vector3(jumps[i,0], jumps[i,1] + .06f, 0));
            wait = Wait(.15f); while (wait.MoveNext()) yield return null;
            Input(Key.D);
            while (player.transform.position.x < jumps[i,2]) yield return null;
            Debug.Log("RAT_JUMP_TAKEOFF: " + i + " at " + player.transform.position);
            Input(Key.D, Key.Space); yield return null; yield return null; Input(Key.D);
            float jumpDeadline = Time.time + 1.5f;
            while (player.transform.position.x < jumps[i,3])
            {
                if (Time.time > jumpDeadline) throw new Exception("Jump failed to reach landing at " + player.transform.position);
                yield return null;
            }
            Input(); wait = Wait(.7f); while (wait.MoveNext()) yield return null;
            Require(player.transform.position.y >= jumps[i,4] - .1f && player.GetComponent<CharacterController>().isGrounded, "Walking jump " + (i + 1));
        }
        player.Respawn(new Vector3(18, .86f, 0)); Input(Key.D);
        while (player.transform.position.x < 26) yield return null;
        Input(); wait = Wait(.2f); while (wait.MoveNext()) yield return null;
        Require(player.transform.position.y > 2.5f, "Ramp joins tube walkway without snagging");
        TrialMovingPlatform shuttle = UnityEngine.Object.FindFirstObjectByType<TrialMovingPlatform>();
        player.Respawn(new Vector3(shuttle.transform.position.x, 1.46f, 0));
        wait = Wait(.3f); while (wait.MoveNext()) yield return null;
        float offset = player.transform.position.x - shuttle.transform.position.x;
        wait = Wait(3f); while (wait.MoveNext()) yield return null;
        Require(Mathf.Abs(player.transform.position.x - shuttle.transform.position.x - offset) < .35f, "Shuttle carries a standing rat");
        player.Respawn(new Vector3(46, 1.46f, 0));
        wait = Wait(.25f); while (wait.MoveNext()) yield return null;
        Require(session.CheckpointNumber == 4, "Checkpoint trigger secures safe landing");
        int lives = session.RemainingRats;
        player.Respawn(new Vector3(40, -1.7f, 0));
        wait = Wait(.3f); while (wait.MoveNext()) yield return null;
        Require(session.RemainingRats == lives - 1 && Mathf.Abs(player.transform.position.x - 46) < .2f, "Hazard costs one rat and respawns at checkpoint");
        player.Respawn(new Vector3(61.3f, 4.16f, 0));
        wait = Wait(.25f); while (wait.MoveNext()) yield return null;
        Require(session.Finished && !player.enabled, "Exit completes the course");
    }
    private static void Require(bool result, string name)
    {
        if (!result) throw new Exception("FAILED: " + name + " at " + player.transform.position);
        Debug.Log("RAT_CHECK_PASS: " + name);
    }
}

using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

/// <summary>
/// Developer traversal checks for the rebuilt levels 3-7, driven by simulated keyboard input.
/// Each segment respawns the rat at a known spot, grants the augment it needs, performs the
/// move and checks where it landed and that no rat was lost. Negative checks confirm a gap or
/// barrier is impassable without its augment. These are not human playtest evidence.
/// </summary>
public static class EnclosureRouteChecks
{
    private const float T1 = 1f, T2 = 6.2f, T3 = 11.4f, T4 = 16.6f, T5 = 21.8f;
    private static PlayerRatController player;
    private static CharacterController body;
    private static RatPowerups powers;
    private static RatLifeManager lives;
    private static Action<Key[]> input;

    private static Vector3 P => player.transform.position;
    private static bool Grounded => body.isGrounded;
    private static bool Won => GameManager.Instance.CurrentState == GameManager.GameState.Won;
    private static void Press(params Key[] keys) => input(keys);
    private static void Require(bool ok, string message) { if (!ok) throw new Exception(message + " (rat at " + P + ")"); Debug.Log("RAT_ROUTE_OK: " + message); }
    // Game time stops when a level ends or pauses; fail fast instead of waiting forever.
    private static void Running(string description) { if (Time.timeScale == 0f) throw new Exception(description + ": game time stopped (state " + GameManager.Instance.CurrentState + ") at " + P); }
    private static IEnumerator Wait(float seconds) { float until = Time.time + seconds; while (Time.time < until) { Running("Wait"); yield return 0; } }
    private static IEnumerator Until(Func<bool> condition, float seconds, string description)
    { float until = Time.time + seconds; while (!condition()) { Running(description); if (Time.time > until) throw new Exception(description + " timed out at " + P); yield return 0; } }
    private static IEnumerator Tap(Key[] held, Key tap) { Press(held.Append(tap).ToArray()); yield return null; yield return null; Press(held); }

    private static IEnumerator Spawn(float x, float y, params RatAugment[] grant)
    {
        Press(); powers.ResetPowers(); player.Respawn(new Vector3(x, y + 0.08f, 0));
        foreach (RatAugment kind in grant) powers.Collect(kind);
        yield return Wait(0.25f);
    }

    private static IEnumerator Settle(float landingY, string message, int livesBefore)
    {
        Press(); yield return Until(() => Grounded || Won, 3f, message + " settling");
        // A final crossing may carry the rat straight into the exit, which ends the level.
        if (Won) { Require(lives.LivesRemaining == livesBefore, message + " (reached the exit)"); yield break; }
        yield return Wait(0.3f);
        Require(lives.LivesRemaining == livesBefore && Grounded && P.y >= landingY - 0.15f,
            message + $" [lives {lives.LivesRemaining}/{livesBefore}, grounded {Grounded}]");
    }

    /// <summary>Sprint, jump at takeoff, dash at the apex, land past target.</summary>
    private static IEnumerator JumpDash(float from, float y, float takeoff, float target, float landingY, string message, bool speed = false)
    {
        var grant = speed ? new[] { RatAugment.Dash, RatAugment.SpeedBoost } : new[] { RatAugment.Dash };
        yield return Spawn(from, y, grant); int count = lives.LivesRemaining;
        bool right = target > from; Key[] run = { right ? Key.D : Key.A, Key.LeftShift };
        Press(run); yield return Until(() => right ? P.x >= takeoff : P.x <= takeoff, 4f, message + " approach");
        yield return Tap(run, Key.Space); yield return Wait(0.34f); yield return Tap(run, Key.Q);
        yield return Until(() => right ? P.x >= target : P.x <= target, 3f, message + " crossing");
        yield return Settle(landingY, message, count);
    }

    /// <summary>The same jump without dash must fall short of the far deck.</summary>
    private static IEnumerator FallsShort(float from, float y, float takeoff, float farEdge, string message)
    {
        yield return Spawn(from, y); bool right = farEdge > from; Key[] run = { right ? Key.D : Key.A, Key.LeftShift };
        Press(run); yield return Until(() => right ? P.x >= takeoff : P.x <= takeoff, 4f, message + " approach");
        yield return Tap(run, Key.Space); yield return Wait(0.6f); Press();
        yield return Until(() => Grounded || lives.Invulnerable, 3f, message + " fall");
        Require(right ? P.x < farEdge : P.x > farEdge, message);
        if (lives.LivesRemaining < 3) throw new Exception("Negative check cost a rat: " + message);
    }

    private static IEnumerator Jump(float from, float y, float takeoff, float target, float landingY, string message, bool doubleJump = false, params RatAugment[] extra)
    {
        yield return Spawn(from, y, doubleJump ? extra.Append(RatAugment.DoubleJump).ToArray() : extra); int count = lives.LivesRemaining;
        bool right = target > from; Key[] run = { right ? Key.D : Key.A, Key.LeftShift };
        Press(run); yield return Until(() => right ? P.x >= takeoff : P.x <= takeoff, 4f, message + " approach");
        yield return Tap(run, Key.Space);
        if (doubleJump) { yield return Wait(0.32f); yield return Tap(run, Key.Space); }
        yield return Until(() => right ? P.x >= target : P.x <= target, 3f, message + " crossing");
        yield return Settle(landingY, message, count);
    }

    /// <summary>Straight up a shaft (double jump, or holding Space with the jetpack), then onto the deck above.</summary>
    private static IEnumerator Climb(float x, float y, Key exit, float clearY, float landingY, string message, RatAugment power)
    {
        yield return Spawn(x, y, power); int count = lives.LivesRemaining;
        bool jet = power == RatAugment.Jetpack;
        if (jet) Press(Key.Space);
        else { yield return Tap(new Key[0], Key.Space); yield return Wait(0.3f); yield return Tap(new Key[0], Key.Space); }
        yield return Until(() => P.y >= clearY, 3f, message + " rise");
        if (jet) Press(exit, Key.Space); else Press(exit);
        yield return Until(() => Mathf.Abs(P.x - x) > 3f, 2f, message + " step across");
        yield return Settle(landingY, message, count);
    }

    /// <summary>Kick between the chimney's outer and inner walls until clear of the inner wall's top.</summary>
    private static IEnumerator WallClimb(float x, float floor, float outerFace, float innerFace, float topY, string message)
    {
        yield return Spawn(x, floor, RatAugment.WallJump); int count = lives.LivesRemaining;
        Key toOuter = outerFace > innerFace ? Key.D : Key.A, toInner = toOuter == Key.D ? Key.A : Key.D;
        Key toward = toOuter;
        yield return Tap(new[] { toward }, Key.Space);
        for (int kick = 0; kick < 12 && P.y < topY + 0.25f; kick++)
        {
            float face = toward == toOuter ? outerFace : innerFace;
            yield return Until(() => Mathf.Abs(P.x - face) < 0.42f || P.y >= topY + 0.25f, 1.5f, message + " wall contact " + kick);
            if (P.y >= topY + 0.25f) break;
            yield return null;
            toward = toward == Key.D ? Key.A : Key.D;
            yield return Tap(new[] { toward }, Key.Space);
        }
        Press(toInner);
        yield return Until(() => Mathf.Abs(P.x - innerFace) > 1.2f && (innerFace > outerFace ? P.x > innerFace : P.x < innerFace), 2f, message + " exit");
        yield return Settle(topY, message, count);
    }

    /// <summary>Without the augment the same chimney cannot be climbed.</summary>
    private static IEnumerator ChimneyBlocked(float x, float floor, float topY, string message)
    {
        yield return Spawn(x, floor, RatAugment.DoubleJump);
        yield return Tap(new Key[0], Key.Space); yield return Wait(0.3f); yield return Tap(new Key[0], Key.Space);
        yield return Wait(1.2f);
        Require(P.y < topY - 0.5f, message);
    }

    private static IEnumerator Glide(float from, float y, float takeoff, float target, float landingY, string message, params RatAugment[] extra)
    {
        yield return Spawn(from, y, extra.Append(RatAugment.Glide).ToArray()); int count = lives.LivesRemaining;
        bool right = target > from; Key[] run = { right ? Key.D : Key.A }; // Walking, not sprinting.
        Press(run); yield return Until(() => right ? P.x >= takeoff : P.x <= takeoff, 4f, message + " approach");
        Key[] glide = run.Append(Key.Space).ToArray();
        Press(glide); yield return null; yield return null;
        yield return Until(() => right ? P.x >= target : P.x <= target, 6f, message + " crossing");
        yield return Settle(landingY, message, count);
    }

    /// <summary>Jump into an updraft holding Space, rise, then glide sideways onto the deck above.</summary>
    private static IEnumerator Rise(float x, float y, float riseTo, Key exit, float landingX, float landingY, string message)
    {
        yield return Spawn(x, y, RatAugment.Glide); int count = lives.LivesRemaining;
        Press(Key.Space); yield return Until(() => P.y >= riseTo, 4f, message + " rise");
        Press(exit, Key.Space);
        yield return Until(() => exit == Key.D ? P.x >= landingX : P.x <= landingX, 3f, message + " drift");
        yield return Settle(landingY, message, count);
    }

    private static IEnumerator Walk(float x, string message, params RatAugment[] grant)
    {
        int count = lives.LivesRemaining; bool right = x > P.x; foreach (RatAugment kind in grant) powers.Collect(kind);
        Press(right ? Key.D : Key.A);
        yield return Until(() => right ? P.x >= x : P.x <= x, 8f, message);
        Press(); yield return Wait(0.2f);
        Require(lives.LivesRemaining == count, message);
    }

    private static IEnumerator Blocked(float from, float y, Key direction, float wallX, string message)
    {
        yield return Spawn(from, y); Press(direction); yield return Wait(1.5f); Press();
        Require(direction == Key.D ? P.x < wallX : P.x > wallX, message);
    }

    /// <summary>Jump from a hatch, slam down with S, and end up below it with the hatch broken.</summary>
    private static IEnumerator Pound(float x, float y, float belowY, string message)
    {
        yield return Spawn(x, y, RatAugment.GroundPound); int count = lives.LivesRemaining;
        BreakableHatch hatch = Object.FindObjectsByType<BreakableHatch>(FindObjectsSortMode.None).OrderBy(h => Vector2.Distance(h.transform.position, new Vector2(x, y))).First();
        yield return Tap(new Key[0], Key.Space); yield return Wait(0.3f); yield return Tap(new Key[0], Key.S);
        yield return Until(() => P.y < belowY, 2f, message + " drop");
        yield return Settle(belowY - 1.2f, message, count);
        Require(hatch.IsBroken && P.y < belowY, message + " smashes the hatch");
        powers.ResetPowers();
        Require(!hatch.IsBroken, "A new rat finds the hatch restored");
    }

    private static void Init(Action<Key[]> press)
    {
        input = press;
        player = Object.FindFirstObjectByType<PlayerRatController>(); body = player.GetComponent<CharacterController>();
        powers = player.GetComponent<RatPowerups>(); lives = Object.FindFirstObjectByType<RatLifeManager>();
    }

    /// <summary>
    /// Wait beside a wheel until its arm has just swung clear of vertical, then sprint underneath
    /// with no augments. Proves the wheel is passable at normal speed.
    /// </summary>
    private static IEnumerator WheelCross(float hubX, float top, bool leftward, string message)
    {
        yield return Spawn(leftward ? hubX + 2.1f : hubX - 2.1f, top); int count = lives.LivesRemaining;
        TrialWheel wheel = Object.FindObjectsByType<TrialWheel>(FindObjectsSortMode.None)
            .OrderBy(w => Vector2.Distance(w.transform.position, new Vector2(hubX, top + 2.1f))).First();
        yield return Until(() => { float a = Mathf.Repeat(wheel.transform.eulerAngles.z, 180f); return a > 45f && a < 60f; }, 6f, message + " timing");
        Press(leftward ? Key.A : Key.D, Key.LeftShift);
        yield return Until(() => leftward ? P.x <= hubX - 2.2f : P.x >= hubX + 2.2f, 2f, message + " crossing");
        yield return Settle(top, message, count);
    }

    /// <summary>Wheel check for a reference level (1-2), called from CampaignValidation.</summary>
    public static IEnumerator Wheel(Action<Key[]> press, float hubX, float top, bool leftward, string message)
    {
        Init(press);
        yield return WheelCross(hubX, top, leftward, message);
        Press(); powers.ResetPowers();
    }

    public static IEnumerator Check(int index, Action<Key[]> press)
    {
        Init(press);
        Require(Object.FindObjectsByType<Checkpoint>(FindObjectsSortMode.None).Length >= 7, "Stage " + (index + 1) + " has its checkpoints");
        switch (index)
        {
            case 2: yield return RelayArchive(); break;
            case 3: yield return CoolantFoundry(); break;
            case 4: yield return ScannerGallery(); break;
            case 5: yield return ContainmentCore(); break;
            case 6: yield return EscapeSpire(); break;
        }
        Press(); powers.ResetPowers();
    }

    private static IEnumerator RelayArchive()
    {
        yield return FallsShort(7.5f, T1, 8.6f, 16f, "L3 practice gap needs a dash");
        yield return JumpDash(7.5f, T1, 8.6f, 16.6f, 2.6f, "L3 dash onto the raised landing");
        yield return JumpDash(19.5f, 2.6f, 21.6f, 31.6f, T1, "L3 dash over the coolant");
        yield return Jump(37f, T1, 38f, 40.2f, 4.3f, "L3 double jump onto the perch", true);
        yield return Climb(41.5f, 4.3f, Key.A, 6.6f, T2, "L3 perch to floor 2", RatAugment.DoubleJump);
        yield return JumpDash(34f, T2, 33f, 23.8f, T2, "L3 dash across the floor 2 gap");
        yield return Jump(8.5f, T2, 5.6f, 3.4f, 9.5f, "L3 double jump onto the left perch", true);
        yield return Climb(2.2f, 9.5f, Key.D, 11.7f, T3, "L3 left perch to floor 3", RatAugment.DoubleJump);
        yield return JumpDash(6.5f, T3, 10.2f, 25f, T3, "L3 speed dash across floor 3", true);
        yield return WheelCross(35.5f, T4, true, "L3 time the floor 4 wheel");
        yield return JumpDash(32.6f, T4, 31.3f, 22.1f, T4, "L3 dash across the floor 4 gap");
        yield return Climb(3f, T4, Key.D, 22.4f, T5, "L3 jetpack shaft to floor 5", RatAugment.Jetpack);
        yield return JumpDash(26f, T5, 27.7f, 36.6f, T5, "L3 dash over the extraction pit");
    }

    private static IEnumerator CoolantFoundry()
    {
        yield return Jump(5f, T1, 7.7f, 14.6f, T1, "L4 first coolant jump");
        yield return ChimneyBlocked(41.4f, T1, T2, "L4 chimney cannot be climbed without wall jump");
        yield return WallClimb(41.4f, T1, 42.6f, 40.2f, T2, "L4 wall jump up the first chimney");
        yield return JumpDash(34f, T2, 32.3f, 23.5f, T2, "L4 dash over the floor 2 pit");
        yield return WallClimb(2f, T2, 0.7f, 3.2f, T3, "L4 wall jump up the left chimney");
        yield return WallClimb(41.4f, T3, 42.6f, 40.2f, T4, "L4 wall jump to floor 4");
        yield return WheelCross(35f, T4, true, "L4 time the floor 4 wheel");
        yield return JumpDash(32.6f, T4, 30.3f, 21.5f, T4, "L4 dash across the floor 4 gap");
        yield return WallClimb(2f, T4, 0.7f, 3.2f, T5, "L4 wall jump to floor 5");
        yield return JumpDash(18.4f, T5, 19.7f, 27.6f, T5, "L4 dash over the extraction pit");
    }

    private static IEnumerator ScannerGallery()
    {
        yield return FallsShort(10.5f, 3f, 11.2f, 20f, "L5 practice gap needs a glide");
        yield return Glide(10.2f, 3f, 11.2f, 20.6f, 2.6f, "L5 walking glide onto the landing");
        yield return Rise(33f, T1, 7.5f, Key.A, 30.2f, T2, "L5 updraft to floor 2");
        yield return Rise(2.5f, T2, 12.6f, Key.D, 5.2f, T3, "L5 updraft to floor 3");
        yield return Glide(9f, T3, 11.7f, 26.1f, T3, "L5 walking glide through the gallery updraft");
        yield return WallClimb(41.4f, T3, 42.6f, 40.2f, T4, "L5 wall jump to floor 4");
        yield return WheelCross(34f, T4, true, "L5 time the floor 4 wheel");
        yield return Glide(30f, T4, 28.3f, 12.4f, T4, "L5 walking glide over coolant using the updraft");
        yield return Rise(2.5f, T4, 22.9f, Key.D, 5.2f, T5, "L5 updraft to floor 5");
    }

    private static IEnumerator ContainmentCore()
    {
        yield return Blocked(8.5f, T1, Key.D, 9.8f, "L6 containment field blocks a rat without phase");
        yield return Spawn(8.5f, T1);
        yield return Walk(12.5f, "L6 phase through the first field", RatAugment.Phase);
        yield return Spawn(14f, T1);
        yield return Walk(22.6f, "L6 phase through the cell's fire and laser", RatAugment.Phase);
        yield return Jump(22.2f, T1, 23.3f, 29.8f, T1, "L6 phase out and jump the floor 1 coolant", false, RatAugment.Phase);
        yield return Spawn(37f, T1);
        yield return Walk(41.4f, "L6 phase into the chimney", RatAugment.Phase);
        yield return WallClimb(41.4f, T1, 42.6f, 40.2f, T2, "L6 wall jump to floor 2");
        yield return Spawn(37.5f, T2);
        yield return Walk(26.5f, "L6 phase through the field chain", RatAugment.Phase);
        yield return Jump(29f, T2, 26.1f, 19.5f, T2, "L6 phase out and jump the floor 2 pit", false, RatAugment.Phase);
        yield return WheelCross(13f, T2, true, "L6 time the floor 2 wheel");
        yield return Climb(3f, T2, Key.D, 12f, T3, "L6 jetpack shaft to floor 3", RatAugment.Jetpack);
        yield return Jump(15f, T3, 15.8f, 22.6f, T3, "L6 jump the sealed pit");
        yield return Rise(41.5f, T3, 17.4f, Key.A, 39f, T4, "L6 updraft to floor 4");
        yield return WheelCross(34.5f, T4, true, "L6 time the floor 4 wheel");
        yield return JumpDash(32.6f, T4, 31.3f, 22.4f, T4, "L6 dash across the floor 4 gap");
        yield return Spawn(19f, T4);
        yield return Walk(7.6f, "L6 phase through the floor 4 gauntlet", RatAugment.Phase);
        yield return WallClimb(2f, T4, 0.7f, 3.2f, T5, "L6 wall jump to floor 5");
        yield return Spawn(7.5f, T5);
        yield return Walk(21f, "L6 phase through the core breach", RatAugment.Phase);
        yield return JumpDash(26.5f, T5, 27.7f, 35.6f, T5, "L6 dash over the extraction pit");
    }

    private static IEnumerator EscapeSpire()
    {
        yield return Blocked(12f, 3.4f, Key.D, 14.8f, "L7 bulkhead blocks the walkway");
        yield return Pound(14f, 3.4f, 1.5f, "L7 pound through the practice hatch");
        yield return Spawn(16.5f, 0.3f);
        yield return Walk(14.2f, "L7 crawl under the bulkhead");
        yield return Jump(17f, 0.3f, 20.3f, 22f, T1, "L7 climb out of the trench");
        yield return WheelCross(34.5f, T1, false, "L7 time the floor 1 wheel");
        yield return Jump(37f, T1, 38f, 40.2f, 4.3f, "L7 double jump onto the perch", true);
        yield return Climb(41.5f, 4.3f, Key.A, 6.6f, T2, "L7 perch to floor 2", RatAugment.DoubleJump);
        yield return Pound(31f, T2, 4.6f, "L7 pound into the service channel");
        yield return Spawn(31f, 3.7f);
        yield return Walk(23.2f, "L7 crawl under the channel bulkhead");
        yield return Jump(23.4f, 3.7f, 23f, 22f, 4.7f, "L7 hop onto the channel step");
        yield return Jump(22f, 4.7f, 21.3f, 19.6f, T2, "L7 climb out of the channel");
        yield return WallClimb(2f, T2, 0.7f, 3.2f, T3, "L7 wall jump to floor 3");
        yield return Glide(22f, T3, 23.7f, 38.8f, T3, "L7 walking glide to the far chimney using the updraft");
        yield return WallClimb(41.4f, T3, 42.6f, 40.2f, T4, "L7 wall jump to floor 4");
        yield return WheelCross(28.5f, T4, true, "L7 time the floor 4 wheel");
        yield return Pound(20f, T4, 15f, "L7 pound into the core channel");
        yield return Spawn(20f, 14.1f);
        yield return Walk(10f, "L7 crawl under the core bulkhead");
        yield return Jump(10f, 14.1f, 9.4f, 8.2f, 15.1f, "L7 hop onto the core step");
        yield return Jump(8.2f, 15.1f, 7.4f, 5.6f, T4, "L7 climb out onto the jet ledge");
        yield return Climb(2.5f, T4, Key.D, 22.4f, T5, "L7 jetpack to floor 5", RatAugment.Jetpack);
        yield return Spawn(7f, T5);
        yield return Walk(18.5f, "L7 phase through the last gauntlet", RatAugment.Phase);
        yield return JumpDash(25.5f, T5, 26.7f, 33.6f, T5, "L7 dash over the last pit");
        yield return Jump(34.6f, T5, 36.2f, 37.6f, 25f, "L7 double jump onto the vault roof", true);
        yield return Spawn(40f, 25f, RatAugment.GroundPound);
        yield return Tap(new Key[0], Key.Space); yield return Wait(0.3f); yield return Tap(new Key[0], Key.S);
        yield return Until(() => GameManager.Instance.CurrentState == GameManager.GameState.Won, 3f, "L7 pound into the exit vault");
        Require(lives.LivesRemaining == 3, "L7 pounding through the vault roof reaches the exit");
    }
}

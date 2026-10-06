using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Object=UnityEngine.Object;

// Traversal scenarios adapted from https://github.com/SavvyHack/Graphics-and-Interaction/blob/11bf1fb1e1a05e778ecbed0d6a9e4beb3ec54ba9/Assets/Editor/CampaignPlaytests.cs
// Optional developer checks use a temporary profile; they are not human playtest evidence.
public static class ReferenceRouteChecks
{
    private static PlayerRatController player;
    private static RatPowerups powers;
    private static RatLifeManager lives;
    private static Action<Key[]> input;
    private static void Input(params Key[] keys) => input(keys);
    private static void Require(bool ok,string message){if(!ok)throw new Exception(message);Debug.Log("RAT_ROUTE_OK: "+message);}
    private static IEnumerator Wait(float seconds){float until=Time.time+seconds;while(Time.time<until)yield return 0;}
    private static IEnumerator Until(Func<bool> condition,float seconds,string description)
    {float until=Time.time+seconds;while(!condition()){if(Time.time>until)throw new Exception(description+" at "+player.transform.position);yield return 0;}}
    private static IEnumerator Spawn(float x, float y)
    {
        Input(); player.Respawn(new Vector3(x, y + .08f, 0)); yield return Wait(.22f);
    }
    private static IEnumerator Walk(float x)
    {
        bool right = x > player.transform.position.x; Input(right ? Key.D : Key.A);
        yield return Until(() => right ? player.transform.position.x >= x : player.transform.position.x <= x, 12, "Walk to " + x); Input(); yield return Wait(.22f);
    }
    private static IEnumerator Jump(float from, float y, float takeoff, float target, float landingY, bool doubleJump = false)
    {
        int initialLives = lives.LivesRemaining; yield return Spawn(from, y); bool right = target > from; Key key = right ? Key.D : Key.A; Input(key);
        yield return Until(() => right ? player.transform.position.x >= takeoff : player.transform.position.x <= takeoff, 3, "Jump approach");
        Input(key, Key.Space); yield return null; yield return null; Input(key);
        if (doubleJump) { yield return Wait(.30f); Input(key, Key.Space); yield return null; yield return null; Input(key); }
        yield return Until(() => right ? player.transform.position.x >= target : player.transform.position.x <= target, 2.5f, "Jump crossing");
        Input(); yield return Wait(.75f); Require(lives.LivesRemaining == initialLives && (right ? player.transform.position.x >= target - .1f : player.transform.position.x <= target + .1f) && player.transform.position.y >= landingY - .15f && player.GetComponent<CharacterController>().isGrounded, "Playable jump " + from + " -> " + target + " at deck " + landingY + " (actual " + player.transform.position + ")");
    }
    private static IEnumerator Fly(float x, float y, float targetX, float targetY)
    {
        yield return Spawn(x, y); powers.Collect(RatAugment.Jetpack); Input(Key.Space);
        yield return Until(() => player.transform.position.y > targetY + .3f, 2.2f, "Jetpack climb");
        Input(Key.Space, targetX > x ? Key.D : Key.A);
        yield return Until(() => targetX > x ? player.transform.position.x >= targetX : player.transform.position.x <= targetX, 1.2f, "Jetpack landing approach");
        Input(); yield return Until(() => player.GetComponent<CharacterController>().isGrounded, 2f, "Jetpack settling"); Require(player.transform.position.y >= targetY - .12f, "Jetpack shaft reaches deck " + targetY + " (actual " + player.transform.position + ")");
    }
    private static IEnumerator SuspendedRoute()
    {
        var a = GameObject.Find("Suspended platform A").GetComponent<TrialMovingPlatform>();
        var b = GameObject.Find("Suspended platform B").GetComponent<TrialMovingPlatform>();
        int count = lives.LivesRemaining;
        yield return Spawn(9.2f, 11.4f);
        yield return Until(() => a.transform.position.x < 13.4f, 35, "First shuttle boarding window");
        Input(Key.D, Key.Space); yield return null; yield return null; Input(Key.D);
        yield return Until(() => player.transform.position.x >= a.transform.position.x - .2f, 2, "Board first shuttle");
        Input(); yield return Until(() => player.GetComponent<CharacterController>().isGrounded, 2, "First shuttle landing");
        Require(lives.LivesRemaining == count && Mathf.Abs(player.transform.position.x - a.transform.position.x) < 1.7f, "Board first suspended platform from the real ledge");
        yield return Until(() => a.transform.position.x > 17.8f, 35, "First shuttle transfer window");
        Input(Key.D, Key.Space); yield return null; yield return null; Input(Key.D);
        yield return Until(() => player.transform.position.x > 21, 2, "First shuttle dismount"); Input(); yield return Wait(.8f);
        Require(lives.LivesRemaining == count && player.transform.position.y > 12.4f, "First suspended platform connects to central island");
        yield return Walk(23.4f);
        yield return Until(() => b.transform.position.x < 28.7f, 12, "Second shuttle boarding window");
        Input(Key.D, Key.Space); yield return null; yield return null; Input(Key.D); yield return Wait(.30f);
        Input(Key.D, Key.Space); yield return null; yield return null; Input(Key.D);
        yield return Until(() => player.transform.position.x >= b.transform.position.x - .2f, 2, "Board second shuttle"); Input();
        yield return Until(() => player.GetComponent<CharacterController>().isGrounded, 2, "Second shuttle landing");
        Require(lives.LivesRemaining == count && Mathf.Abs(player.transform.position.x - b.transform.position.x) < 1.7f, "Double jump boards second suspended platform");
        yield return Until(() => b.transform.position.x > 36.7f, 12, "Second shuttle upper stop");
        Input(Key.D, Key.Space); yield return null; yield return null; Input(Key.D); yield return Wait(.30f);
        Input(Key.D, Key.Space); yield return null; yield return null; Input(Key.D);
        yield return Until(() => player.transform.position.x > 41, 2, "Second shuttle high landing"); Input(); yield return Wait(.9f);
        Require(lives.LivesRemaining == count && player.transform.position.y > 16.9f && player.GetComponent<CharacterController>().isGrounded, "Second suspended platform connects to high right landing");
    }

    public static IEnumerator Check(int index, Action<Key[]> press)
    {
        input=press;player=Object.FindFirstObjectByType<PlayerRatController>();powers=player.GetComponent<RatPowerups>();lives=Object.FindFirstObjectByType<RatLifeManager>();
        Require(powers!=null,"Augments wired in stage "+(index+1));
        if(index==0)
        {
            yield return Jump(9,1,10.4f,15,1);
            yield return Spawn(18,1);Require(powers.Has(RatAugment.SpeedBoost),"Contact grants speed");
            yield return Jump(20,1,21.5f,30,1);powers.ResetPowers();powers.Collect(RatAugment.DoubleJump);
            yield return Jump(37.6f,1,38.7f,40.7f,4.3f,true);
            yield return Jump(41.5f,4.3f,40,36.8f,6.2f);
            yield return Spawn(32.2f,6.2f);Require(powers.Has(RatAugment.Shield),"Shield station contact");
            int count=lives.LivesRemaining;yield return Walk(24.8f);Require(lives.LivesRemaining==count&&!powers.Has(RatAugment.Shield),"Shield crosses laser once");
            yield return Fly(6.8f,6.2f,9.5f,11.4f);
            yield return Spawn(15,11.4f);yield return Walk(17.8f);Input(Key.E);yield return 2;Input();yield return Wait(.3f);
            Require(Object.FindFirstObjectByType<CampaignPulseGate>().IsOpen&&!powers.Has(RatAugment.GravityPulse),"Pulse opens gate and consumes charge");yield return Walk(22);
            yield return Spawn(25,11.4f);Require(powers.Has(RatAugment.SlowTime)&&RatPowerups.WorldScale<.3f&&Time.timeScale==1,"Slow machinery retains player time");
            var lift=Object.FindFirstObjectByType<TrialMovingPlatform>();Input();player.Respawn(new Vector3(lift.transform.position.x,lift.GetComponent<Collider>().bounds.max.y+.05f,0));yield return Wait(.3f);
            float offset=player.transform.position.y-lift.transform.position.y;yield return Wait(1.4f);Require(Mathf.Abs(player.transform.position.y-lift.transform.position.y-offset)<.18f,"Elevator carries rat vertically");
            powers.ResetPowers();yield return Fly(7,16.6f,10,21.8f);
        }
        else
        {
            powers.Collect(RatAugment.DoubleJump);yield return Jump(8,1,9.3f,17,1,true);
            yield return Spawn(20.5f,1);yield return Jump(22,1,23.5f,32,1);powers.ResetPowers();
            powers.Collect(RatAugment.DoubleJump);yield return Jump(38,1,39,41,4.3f,true);yield return Jump(41.5f,4.3f,40,36.8f,6.2f);
            yield return Fly(3.8f,6.2f,6.6f,11.4f);
            yield return SuspendedRoute();
            yield return Fly(42,17,37.8f,21.8f);powers.Collect(RatAugment.DoubleJump);
            yield return Jump(28,21.8f,26.5f,17.8f,21.8f,true);
            yield return Spawn(14,21.8f);int count=lives.LivesRemaining;yield return Walk(8);Require(lives.LivesRemaining==count,"Final shield scan crossing");
        }
        Input();
        var camera=Camera.main;var follow=camera.GetComponent<FixedCameraFollow>();
        Require(follow.Target==player.transform && Mathf.Abs(camera.transform.position.z+35)<.001f,"Camera follows active rat at fixed depth");
        float aspect=camera.aspect;camera.aspect=.75f;yield return Wait(.1f);
        Require(camera.orthographicSize*2*camera.aspect>=19.99f,"Narrow window keeps minimum route width");
        camera.aspect=aspect;yield return Wait(.1f);
        GameManager.Instance.SetPaused(true);Vector3 cameraPosition=camera.transform.position;yield return 20;
        Require(Vector3.Distance(cameraPosition,camera.transform.position)<.001f,"Paused camera remains still");
        GameManager.Instance.SetPaused(false);yield return Wait(.1f);
        Vector3 view=camera.WorldToViewportPoint(player.transform.position);
        Require(view.x>0 && view.x<1 && view.y>0 && view.y<1 && !float.IsNaN(camera.transform.position.x),"Rat visible after narrow view and pause/resume");
        Require(lives.LivesRemaining==3,"Traversal checks retain three rats");
        powers.ResetPowers();Require(RatPowerups.WorldScale==1,"Reset clears world slowdown");
    }
}

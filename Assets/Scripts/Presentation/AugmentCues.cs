using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pickup cues for the eleven augments, synthesized in code so each power-up reads as a
/// different event without shipping eleven audio files. They are placeholders, not a final
/// sound design: drop a real clip at Assets/Resources/Audio/Augments/&lt;augment_id&gt;
/// (for example slow_time.mp3) and it replaces the synthesized cue for that augment.
///
/// SlowTime is the deliberate exception to "short and bright": a long downward glide, the
/// tape-slowing trope, so the pickup itself hints that machinery is about to drag.
///
/// AI-assisted placeholder content. Check the course AI policy before treating this as
/// assessed audio work, and audition every cue before shipping.
/// </summary>
public static class AugmentCues
{
    private const int SampleRate = 44100;
    private static readonly Dictionary<RatAugment, AudioClip> cache = new Dictionary<RatAugment, AudioClip>();

    private struct Recipe
    {
        public float duration;   // seconds
        public float startHz;    // glide start frequency
        public float endHz;      // glide end frequency
        public float glide;      // 1 = linear; >1 holds the start, then drops
        public float attack;     // seconds of fade-in
        public float decay;      // exponential falloff, 1/seconds
        public float noise;      // 0..1 breathy layer instead of pure tone
        public float harmonic;   // 0..1 second harmonic, adds body
        public float detune;     // Hz offset of a second oscillator, gives beating
        public int pulses;       // 1 = one hit; >1 = repeated chirps
        public float gain;       // peak level of the finished clip
    }

    /// <summary>Returns the cue for an augment: a Resources override when present, else the synthesized placeholder.</summary>
    public static AudioClip Resolve(RatAugment kind)
    {
        if (cache.TryGetValue(kind, out AudioClip cached) && cached != null) return cached;
        string id = Id(kind);
        AudioClip imported = Resources.Load<AudioClip>("Audio/Augments/" + id);
        AudioClip clip = imported != null ? imported : Synthesize(kind, id);
        cache[kind] = clip;
        return clip;
    }

    /// <summary>RatAugment.SlowTime becomes "slow_time", matching the Resources override path.</summary>
    public static string Id(RatAugment kind)
    {
        string name = kind.ToString();
        var id = new System.Text.StringBuilder(name.Length + 4);
        for (int i = 0; i < name.Length; i++)
        {
            if (i > 0 && char.IsUpper(name[i])) id.Append('_');
            id.Append(char.ToLowerInvariant(name[i]));
        }
        return id.ToString();
    }

    private static Recipe Get(RatAugment kind) => kind switch
    {
        // Rising thruster swell.
        RatAugment.Jetpack => new Recipe { duration = .45f, startHz = 180, endHz = 700, glide = 1.25f, attack = .02f, decay = 3.2f, noise = .45f, harmonic = .15f, pulses = 1, gain = .5f },
        // Two chirps, the second higher: hop, then hop again.
        RatAugment.DoubleJump => new Recipe { duration = .38f, startHz = 420, endHz = 880, glide = 1f, attack = .004f, decay = 3.5f, noise = .1f, harmonic = .2f, pulses = 2, gain = .5f },
        // Warm, slow, low: something solid closing around the rat.
        RatAugment.Shield => new Recipe { duration = .6f, startHz = 230, endHz = 275, glide = 1f, attack = .05f, decay = 2.4f, noise = .04f, harmonic = .35f, detune = 3f, pulses = 1, gain = .5f },
        // Fast upward zip.
        RatAugment.SpeedBoost => new Recipe { duration = .3f, startHz = 320, endHz = 1500, glide = 1f, attack = .006f, decay = 4.5f, noise = .25f, harmonic = .1f, pulses = 1, gain = .5f },
        // Sub-bass whump as the gate is pushed away.
        RatAugment.GravityPulse => new Recipe { duration = .55f, startHz = 220, endHz = 55, glide = 1.2f, attack = .004f, decay = 3.4f, noise = .2f, harmonic = .5f, pulses = 1, gain = .55f },
        // The flagship: a long, accelerating downward glide with a slow tail and a
        // slight beat, so the pickup reads as time dragging to a crawl.
        RatAugment.SlowTime => new Recipe { duration = .9f, startHz = 880, endHz = 150, glide = 1.7f, attack = .01f, decay = 1.1f, noise = .12f, harmonic = .25f, detune = 5f, pulses = 1, gain = .5f },
        // Short bright forward burst.
        RatAugment.Dash => new Recipe { duration = .28f, startHz = 1200, endHz = 480, glide = 1f, attack = .004f, decay = 5f, noise = .75f, harmonic = .05f, pulses = 1, gain = .5f },
        // Crisp kick off the wall.
        RatAugment.WallJump => new Recipe { duration = .18f, startHz = 520, endHz = 940, glide = 1f, attack = .003f, decay = 8f, noise = .15f, harmonic = .25f, pulses = 1, gain = .5f },
        // Airy wind swell, slow to rise.
        RatAugment.Glide => new Recipe { duration = .7f, startHz = 300, endHz = 520, glide = 1f, attack = .25f, decay = 1.4f, noise = .85f, pulses = 1, gain = .45f },
        // Beating detune reads as something passing through: ethereal, not solid.
        RatAugment.Phase => new Recipe { duration = .75f, startHz = 320, endHz = 480, glide = 1f, attack = .08f, decay = 1.8f, noise = .3f, harmonic = .2f, detune = 14f, pulses = 1, gain = .45f },
        // Heavy impact from above.
        RatAugment.GroundPound => new Recipe { duration = .6f, startHz = 170, endHz = 42, glide = 1.2f, attack = .002f, decay = 3.2f, noise = .35f, harmonic = .6f, pulses = 1, gain = .6f },
        _ => new Recipe { duration = .3f, startHz = 500, endHz = 700, glide = 1f, attack = .01f, decay = 4f, noise = .2f, pulses = 1, gain = .5f }
    };

    private static AudioClip Synthesize(RatAugment kind, string id)
    {
        Recipe r = Get(kind);
        int count = Mathf.Max(256, Mathf.RoundToInt(r.duration * SampleRate));
        var samples = new float[count];
        float release = Mathf.Min(.08f, r.duration * .25f);
        float phase = 0f, detunedPhase = 0f, peak = 0f;
        // Local seeded generator: avoids perturbing UnityEngine.Random, and keeps a cue identical between runs.
        var noiseRandom = new System.Random((int)kind * 7919 + 17);

        for (int i = 0; i < count; i++)
        {
            float u = (float)i / (count - 1);
            float t = u * r.duration;
            float hz = Mathf.LerpUnclamped(r.startHz, r.endHz, Mathf.Pow(u, Mathf.Max(.05f, r.glide)));
            phase += hz / SampleRate;
            detunedPhase += (hz + r.detune) / SampleRate;

            float value = Mathf.Sin(phase * 2f * Mathf.PI);
            if (r.harmonic > 0f) value += r.harmonic * Mathf.Sin(phase * 4f * Mathf.PI);
            if (r.detune != 0f) value += .5f * Mathf.Sin(detunedPhase * 2f * Mathf.PI);
            if (r.noise > 0f)
            {
                float white = (float)(noiseRandom.NextDouble() * 2.0 - 1.0);
                value = value * (1f - r.noise) + white * r.noise;
            }

            float envelope = r.attack > 0f && t < r.attack ? t / r.attack : 1f;
            envelope *= Mathf.Exp(-r.decay * t);
            if (r.pulses > 1)
            {
                float local = u * r.pulses % 1f;
                envelope *= Mathf.Sin(local * Mathf.PI);
            }
            envelope *= Mathf.Clamp01((r.duration - t) / release);

            value *= envelope;
            samples[i] = value;
            peak = Mathf.Max(peak, Mathf.Abs(value));
        }

        float scale = peak > 0f ? r.gain / peak : 0f;
        for (int i = 0; i < count; i++) samples[i] *= scale;
        samples[count - 1] = 0f; // a zero final sample guarantees no click at the tail

        AudioClip clip = AudioClip.Create($"Augment {id} (procedural)", count, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
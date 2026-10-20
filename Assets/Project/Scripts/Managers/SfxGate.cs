// Pure helpers behind SoundDirector, kept free of AudioSources so the rules can be unit tested.

// Per-event guard: a retrigger cooldown plus a voice limit. Ten rapid taps must read as ten taps, not
// as one smeared wall of sound, so a start is refused while the last one is too recent or while every
// voice of that event is still ringing.
public sealed class SfxGate
{
    private readonly float cooldown;
    private readonly float[] voiceEnds;
    private float lastStart = float.NegativeInfinity;

    public SfxGate(int voiceLimit, float cooldownSeconds)
    {
        cooldown = cooldownSeconds < 0f ? 0f : cooldownSeconds;
        voiceEnds = new float[voiceLimit < 1 ? 1 : voiceLimit];
        for (int i = 0; i < voiceEnds.Length; i++) voiceEnds[i] = float.NegativeInfinity;
    }

    // `now` and `clipLength` are in seconds. Returns true, and books a voice, when the sound may play.
    public bool TryStart(float now, float clipLength)
    {
        if (now - lastStart < cooldown) return false;

        for (int i = 0; i < voiceEnds.Length; i++)
        {
            if (voiceEnds[i] > now) continue;

            voiceEnds[i] = now + (clipLength < 0f ? 0f : clipLength);
            lastStart = now;
            return true;
        }

        return false;
    }
}

public static class SfxVariation
{
    // Maps a 0..1 random number to a pitch multiplier within +/- `cents` (100 cents = one semitone).
    public static float PitchFromUnit(float unit01, float cents)
    {
        float u = unit01 < 0f ? 0f : (unit01 > 1f ? 1f : unit01);
        return (float)System.Math.Pow(2.0, ((u * 2f - 1f) * cents) / 1200.0);
    }

    // Round-robin with a random start: picks one of `count` variants, never the one played last.
    // `last` is -1 before anything has played.
    public static int NextIndex(int count, int last, float unit01)
    {
        if (count <= 1) return 0;

        float u = unit01 < 0f ? 0f : (unit01 > 0.9999f ? 0.9999f : unit01);
        if (last < 0 || last >= count) return (int)(u * count);

        int pick = (int)(u * (count - 1));
        return pick >= last ? pick + 1 : pick;
    }

    public static float DbToGain(float db)
    {
        return (float)System.Math.Pow(10.0, db / 20.0);
    }
}

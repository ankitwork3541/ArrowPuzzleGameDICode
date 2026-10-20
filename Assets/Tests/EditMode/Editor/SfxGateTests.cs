using NUnit.Framework;

public class SfxGateTests
{
    [Test]
    public void TryStart_FirstCall_Allows()
    {
        var gate = new SfxGate(voiceLimit: 2, cooldownSeconds: 0.1f);

        Assert.IsTrue(gate.TryStart(now: 0f, clipLength: 0.3f));
    }

    [Test]
    public void TryStart_InsideCooldown_Refuses()
    {
        var gate = new SfxGate(voiceLimit: 4, cooldownSeconds: 0.1f);
        gate.TryStart(0f, 0.3f);

        Assert.IsFalse(gate.TryStart(0.05f, 0.3f));
    }

    [Test]
    public void TryStart_AfterCooldown_Allows()
    {
        var gate = new SfxGate(voiceLimit: 4, cooldownSeconds: 0.1f);
        gate.TryStart(0f, 0.3f);

        Assert.IsTrue(gate.TryStart(0.11f, 0.3f));
    }

    [Test]
    public void TryStart_AllVoicesBusy_Refuses()
    {
        var gate = new SfxGate(voiceLimit: 2, cooldownSeconds: 0f);
        gate.TryStart(0f, 1f);
        gate.TryStart(0.1f, 1f);

        Assert.IsFalse(gate.TryStart(0.2f, 1f));
    }

    [Test]
    public void TryStart_VoiceFreesWhenClipEnds_Allows()
    {
        var gate = new SfxGate(voiceLimit: 1, cooldownSeconds: 0f);
        gate.TryStart(0f, 0.3f);

        Assert.IsFalse(gate.TryStart(0.2f, 0.3f));
        Assert.IsTrue(gate.TryStart(0.31f, 0.3f));
    }

    [Test]
    public void TryStart_TenRapidTapsFortyMsApart_NeverExceedsVoiceLimit()
    {
        var gate = new SfxGate(voiceLimit: 4, cooldownSeconds: 0.04f);
        var starts = new System.Collections.Generic.List<float>();

        // Ten taps 41 ms apart on a 0.3 s clip: the first four play, the next ones are refused until a
        // voice has finished, so no more than four are ever ringing at once.
        for (int i = 0; i < 10; i++)
        {
            float now = i * 0.041f;
            if (gate.TryStart(now, 0.3f)) starts.Add(now);
        }

        Assert.Less(starts.Count, 10, "some taps must be refused");
        Assert.GreaterOrEqual(starts.Count, 4, "the first four taps must all play");

        foreach (float start in starts)
        {
            int ringing = 0;
            foreach (float other in starts)
            {
                if (other <= start && start - other < 0.3f) ringing++;
            }

            Assert.LessOrEqual(ringing, 4, "more than four voices at t=" + start);
        }
    }

    [Test]
    public void TryStart_ZeroCooldownSingleVoice_SecondImmediateCallRefused()
    {
        var gate = new SfxGate(voiceLimit: 1, cooldownSeconds: 0f);
        gate.TryStart(0f, 0.2f);

        Assert.IsFalse(gate.TryStart(0f, 0.2f));
    }

    [Test]
    public void Constructor_NonPositiveVoiceLimit_StillHasOneVoice()
    {
        var gate = new SfxGate(voiceLimit: 0, cooldownSeconds: -1f);

        Assert.IsTrue(gate.TryStart(0f, 0.1f));
    }
}

public class SfxVariationTests
{
    [TestCase(0f, 70f)]
    [TestCase(0.5f, 70f)]
    [TestCase(1f, 70f)]
    [TestCase(0.25f, 85f)]
    [TestCase(0.9f, 50f)]
    public void PitchFromUnit_AnyInput_StaysInsideTheCentsRange(float unit, float cents)
    {
        float pitch = SfxVariation.PitchFromUnit(unit, cents);
        float lo = (float)System.Math.Pow(2.0, -cents / 1200.0);
        float hi = (float)System.Math.Pow(2.0, cents / 1200.0);

        Assert.GreaterOrEqual(pitch, lo - 1e-6f);
        Assert.LessOrEqual(pitch, hi + 1e-6f);
    }

    [Test]
    public void PitchFromUnit_MiddleOfRange_IsUnchangedPitch()
    {
        Assert.AreEqual(1f, SfxVariation.PitchFromUnit(0.5f, 70f), 1e-5f);
    }

    [Test]
    public void PitchFromUnit_ZeroCents_AlwaysOne()
    {
        Assert.AreEqual(1f, SfxVariation.PitchFromUnit(0.13f, 0f), 1e-6f);
        Assert.AreEqual(1f, SfxVariation.PitchFromUnit(0.97f, 0f), 1e-6f);
    }

    [Test]
    public void PitchFromUnit_OutOfRangeInput_IsClamped()
    {
        float lo = SfxVariation.PitchFromUnit(0f, 70f);
        float hi = SfxVariation.PitchFromUnit(1f, 70f);

        Assert.AreEqual(lo, SfxVariation.PitchFromUnit(-5f, 70f), 1e-6f);
        Assert.AreEqual(hi, SfxVariation.PitchFromUnit(9f, 70f), 1e-6f);
    }

    [Test]
    public void PitchFromUnit_SeventyCents_IsAboutFourPercent()
    {
        float hi = SfxVariation.PitchFromUnit(1f, 70f);

        Assert.AreEqual(1.0413f, hi, 0.001f);
    }

    [Test]
    public void NextIndex_SingleVariant_AlwaysZero()
    {
        Assert.AreEqual(0, SfxVariation.NextIndex(1, -1, 0.7f));
        Assert.AreEqual(0, SfxVariation.NextIndex(1, 0, 0.7f));
    }

    [Test]
    public void NextIndex_NeverRepeatsTheLastPick()
    {
        for (int count = 2; count <= 4; count++)
        {
            for (int last = 0; last < count; last++)
            {
                for (float u = 0f; u <= 1f; u += 0.05f)
                {
                    int pick = SfxVariation.NextIndex(count, last, u);

                    Assert.AreNotEqual(last, pick, "count " + count + " last " + last + " u " + u);
                    Assert.GreaterOrEqual(pick, 0);
                    Assert.Less(pick, count);
                }
            }
        }
    }

    [Test]
    public void NextIndex_NothingPlayedYet_CanPickEveryVariant()
    {
        var seen = new System.Collections.Generic.HashSet<int>();
        for (float u = 0f; u <= 1f; u += 0.01f) seen.Add(SfxVariation.NextIndex(3, -1, u));

        Assert.AreEqual(3, seen.Count);
    }

    [Test]
    public void DbToGain_MinusSixDb_IsAboutHalf()
    {
        Assert.AreEqual(0.501f, SfxVariation.DbToGain(-6f), 0.001f);
    }
}

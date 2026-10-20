using UnityEngine.Serialization;
using UnityEngine;
using UnityEngine.Audio;

// Plays every sound effect and the two result jingles.
//
// One-shots go through a small pool of voices (AudioSources) instead of PlayOneShot, because a
// one-shot cannot carry its own pitch: the pool is what makes the small pitch variation, the per-event
// retrigger cooldown and the per-event voice limit possible (see SfxGate). Clips of the same event can
// be listed as alternates and are picked round-robin, never the same one twice in a row.
public class SoundDirector : MonoBehaviour
{
    public static SoundDirector Instance;

    // Kept so the scene's serialized reference survives; one-shots now play on the voice pool below.
    public AudioSource sfxSource;

    // Win/lose jingles only, so they can be stopped when the card is dismissed or Sound is switched off.
    public AudioSource jingleSource;
    public AudioMixerGroup sfxMixerGroup;

    // Optional own mixer group for the jingles. When it is empty the jingle shares the SFX group.
    public AudioMixerGroup jingleMixerGroup;

    [FormerlySerializedAs("arrowClickSuccessClip")]
    public AudioClip dragonTapClip;
    [FormerlySerializedAs("arrowClickBlockedClip")]
    public AudioClip dragonBumpClip;

    // Every UI button: Play, Hint, Restart, Back, the nav and the settings links. One clip for all of
    // them on purpose: a per-button sound reads as noise, not feedback.
    public AudioClip uiClickClip;

    // The win card's fanfare. Several seconds long, unlike the rest, so it wants Compressed In
    // Memory rather than Decompress On Load.
    public AudioClip levelCompleteClip;

    public AudioClip levelFailedClip;

    // Padlocked dragons: a dry rattle when a locked one is tapped, a bright bell when its lock opens.
    public AudioClip lockShakeClip;
    public AudioClip unlockClip;

    // Alternate takes of the same event, picked round-robin together with the clip above.
    public AudioClip[] dragonTapAlt;
    public AudioClip[] dragonBumpAlt;
    public AudioClip[] lockShakeAlt;

    public AudioClip dragonExitClip;
    public AudioClip heartLostClip;
    public AudioClip hintPulseClip;
    public AudioClip levelEntranceClip;
    public AudioClip mapNodeTapClip;
    public AudioClip sheetOpenClip;
    public AudioClip sheetCloseClip;
    public AudioClip toggleOnClip;
    public AudioClip toggleOffClip;

    private enum Sfx
    {
        Tap, Bump, UiClick, LockShake, Unlock, Exit, HeartLost, Hint, Entrance, MapNode, Sheet,
        ToggleOn, ToggleOff, Count
    }

    private const int VoiceCount = 8;

    // Gameplay one-shots end long before a jingle, so ducking is only about the stray late sound:
    // while a jingle plays, effects sit 6 dB under it.
    private const float JingleDuckDb = -6f;

    // The heart-lost tick follows the bump by this long, so the two never stack into one blur.
    private const double HeartLostDelay = 0.12;

    // Pitch spread per event in cents (100 = a semitone), voice limit and retrigger cooldown, from
    // production/audio/lantern-dragons-sfx/sfx-spec.md.
    private static readonly float[] Cents = { 70f, 50f, 50f, 85f, 0f, 70f, 0f, 0f, 50f, 50f, 50f, 0f, 0f };
    private static readonly int[] Voices = { 4, 2, 2, 1, 1, 3, 1, 1, 1, 1, 1, 1, 1 };
    private static readonly float[] Cooldowns = { 0.04f, 0.15f, 0.06f, 0.25f, 0.15f, 0.08f, 0.20f, 0.50f, 0.20f, 0.10f, 0.20f, 0.06f, 0.06f };

    private SfxGate[] gates;
    private int[] lastPick;
    private AudioSource[] voices;
    private float[] voiceFreeAt;

    void Awake()
    {
        Instance = this;

        if (sfxSource != null) sfxSource.outputAudioMixerGroup = sfxMixerGroup;

        if (jingleSource != null)
        {
            jingleSource.outputAudioMixerGroup = jingleMixerGroup != null ? jingleMixerGroup : sfxMixerGroup;
        }

        gates = new SfxGate[(int)Sfx.Count];
        lastPick = new int[(int)Sfx.Count];
        for (int i = 0; i < gates.Length; i++)
        {
            gates[i] = new SfxGate(Voices[i], Cooldowns[i]);
            lastPick[i] = -1;
        }

        voices = new AudioSource[VoiceCount];
        voiceFreeAt = new float[VoiceCount];
        for (int i = 0; i < VoiceCount; i++)
        {
            GameObject go = new GameObject("SfxVoice" + i);
            go.transform.SetParent(transform, false);

            AudioSource voice = go.AddComponent<AudioSource>();
            voice.playOnAwake = false;
            voice.spatialBlend = 0f;
            voice.outputAudioMixerGroup = sfxMixerGroup;
            voices[i] = voice;
        }
    }

    // OptionsManager's Awake has run by now, whatever the scene's Awake order is.
    void Start()
    {
        if (OptionsManager.Instance != null) OptionsManager.Instance.SoundChanged += OnSoundChanged;
    }

    void OnDestroy()
    {
        if (OptionsManager.Instance != null) OptionsManager.Instance.SoundChanged -= OnSoundChanged;
        if (Instance == this) Instance = null;
    }

    // Switching Sound off must silence a jingle that is already ringing, not just future sounds.
    private void OnSoundChanged(bool on)
    {
        if (on) return;

        StopJingle();
        for (int i = 0; i < voices.Length; i++) voices[i].Stop();
    }

    private static bool SoundOn
    {
        get { return OptionsManager.Instance != null && OptionsManager.Instance.SoundEnabled; }
    }

    public void PlayDragonClickSuccess() { PlaySfx(Sfx.Tap, dragonTapClip, dragonTapAlt, 0.0); }
    public void PlayDragonClickBlocked() { PlaySfx(Sfx.Bump, dragonBumpClip, dragonBumpAlt, 0.0); }
    public void PlayUIClick() { PlaySfx(Sfx.UiClick, uiClickClip, null, 0.0); }
    public void PlayLockShake() { PlaySfx(Sfx.LockShake, lockShakeClip, lockShakeAlt, 0.0); }
    public void PlayUnlock() { PlaySfx(Sfx.Unlock, unlockClip, null, 0.0); }
    public void PlayDragonExit() { PlaySfx(Sfx.Exit, dragonExitClip, null, 0.0); }
    public void PlayHeartLost() { PlaySfx(Sfx.HeartLost, heartLostClip, null, HeartLostDelay); }
    public void PlayHintPulse() { PlaySfx(Sfx.Hint, hintPulseClip, null, 0.0); }
    public void PlayLevelEntrance() { PlaySfx(Sfx.Entrance, levelEntranceClip, null, 0.0); }
    public void PlayMapNodeTap() { PlaySfx(Sfx.MapNode, mapNodeTapClip, null, 0.0); }
    public void PlaySheetOpen() { PlaySfx(Sfx.Sheet, sheetOpenClip, null, 0.0); }
    public void PlaySheetClose() { PlaySfx(Sfx.Sheet, sheetCloseClip, null, 0.0); }

    // Options switches: on is the higher tick. Turning Sound itself off stays silent because PlaySfx
    // checks the new value.
    public void PlayToggle(bool on)
    {
        if (on) PlaySfx(Sfx.ToggleOn, toggleOnClip, null, 0.0);
        else PlaySfx(Sfx.ToggleOff, toggleOffClip, null, 0.0);
    }

    public void PlayLevelComplete() { PlayJingle(levelCompleteClip); }
    public void PlayLevelFailed() { PlayJingle(levelFailedClip); }

    public void StopJingle()
    {
        if (jingleSource != null) jingleSource.Stop();
    }

    void PlayJingle(AudioClip clip)
    {
        if (clip == null || jingleSource == null) return;
        if (!SoundOn) return;

        jingleSource.Stop();
        jingleSource.clip = clip;
        jingleSource.pitch = 1f;
        jingleSource.Play();
    }

    void PlaySfx(Sfx id, AudioClip primary, AudioClip[] alternates, double delaySeconds)
    {
        if (!SoundOn || gates == null) return;

        AudioClip clip = Pick(id, primary, alternates);
        if (clip == null) return;

        float now = Time.realtimeSinceStartup;
        if (!gates[(int)id].TryStart(now, clip.length + (float)delaySeconds)) return;

        int slot = FreeVoice(now);
        AudioSource voice = voices[slot];
        voiceFreeAt[slot] = now + (float)delaySeconds + clip.length;

        voice.Stop();
        voice.clip = clip;
        voice.pitch = SfxVariation.PitchFromUnit(Random.value, Cents[(int)id]);
        voice.volume = jingleSource != null && jingleSource.isPlaying ? SfxVariation.DbToGain(JingleDuckDb) : 1f;

        if (delaySeconds > 0.0) voice.PlayScheduled(AudioSettings.dspTime + delaySeconds);
        else voice.Play();
    }

    // The voice that frees up first, so a busy moment steals the one closest to finished.
    int FreeVoice(float now)
    {
        int best = 0;
        for (int i = 0; i < voices.Length; i++)
        {
            if (voiceFreeAt[i] <= now) return i;
            if (voiceFreeAt[i] < voiceFreeAt[best]) best = i;
        }

        return best;
    }

    AudioClip Pick(Sfx id, AudioClip primary, AudioClip[] alternates)
    {
        int extra = alternates != null ? alternates.Length : 0;
        if (extra == 0) return primary;

        int index = SfxVariation.NextIndex(1 + extra, lastPick[(int)id], Random.value);
        lastPick[(int)id] = index;

        AudioClip clip = index == 0 ? primary : alternates[index - 1];
        return clip != null ? clip : primary;
    }
}

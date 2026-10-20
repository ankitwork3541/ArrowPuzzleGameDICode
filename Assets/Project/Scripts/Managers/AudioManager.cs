using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource sfxSource;

    // Win/lose jingles only, so they can be stopped when the card is dismissed. PlayOneShot sounds
    // cannot be stopped individually, and stopping sfxSource would cut the button click too.
    public AudioSource jingleSource;
    public AudioMixerGroup sfxMixerGroup;

    public AudioClip arrowClickSuccessClip;
    public AudioClip arrowClickBlockedClip;

    // Every UI button: Play, Hint, Restart, Back, the nav, the settings switches and rows. One clip
    // for all of them on purpose — a per-button sound reads as noise, not feedback.
    public AudioClip uiClickClip;

    // The win card's fanfare. Several seconds long, unlike the rest, so it wants Compressed In
    // Memory rather than Decompress On Load — see the import note in Docs/Game-Design.md §12.
    public AudioClip levelCompleteClip;

    public AudioClip levelFailedClip;

    // Padlocked dragons: a dry rattle when a locked one is tapped, a bright bell when its lock opens.
    public AudioClip lockShakeClip;
    public AudioClip unlockClip;

    void Awake()
    {
        Instance = this;
        sfxSource.outputAudioMixerGroup = sfxMixerGroup;
        if (jingleSource != null) jingleSource.outputAudioMixerGroup = sfxMixerGroup;
    }

    public void PlayArrowClickSuccess()
    {
        PlaySfx(arrowClickSuccessClip);
    }

    public void PlayArrowClickBlocked()
    {
        PlaySfx(arrowClickBlockedClip);
    }

    public void PlayUIClick()
    {
        PlaySfx(uiClickClip);
    }

    public void PlayLockShake()
    {
        PlaySfx(lockShakeClip);
    }

    public void PlayUnlock()
    {
        PlaySfx(unlockClip);
    }

    public void PlayLevelComplete()
    {
        PlayJingle(levelCompleteClip);
    }

    public void PlayLevelFailed()
    {
        PlayJingle(levelFailedClip);
    }

    public void StopJingle()
    {
        if (jingleSource != null) jingleSource.Stop();
    }

    void PlayJingle(AudioClip clip)
    {
        if (clip == null || jingleSource == null) return;
        if (!SettingsManager.Instance.SoundEnabled) return;

        jingleSource.Stop();
        jingleSource.clip = clip;
        jingleSource.Play();
    }

    void PlaySfx(AudioClip clip)
    {
        if (clip == null) return;
        if (!SettingsManager.Instance.SoundEnabled) return;
        sfxSource.PlayOneShot(clip);
    }
}

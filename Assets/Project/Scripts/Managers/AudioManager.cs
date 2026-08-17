using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource sfxSource;
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

    void Awake()
    {
        Instance = this;
        sfxSource.outputAudioMixerGroup = sfxMixerGroup;
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

    public void PlayLevelComplete()
    {
        PlaySfx(levelCompleteClip);
    }

    public void PlayLevelFailed()
    {
        PlaySfx(levelFailedClip);
    }

    void PlaySfx(AudioClip clip)
    {
        if (clip == null) return;
        if (!SettingsManager.Instance.SoundEnabled) return;
        sfxSource.PlayOneShot(clip);
    }
}

using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource sfxSource;
    public AudioMixerGroup sfxMixerGroup;

    public AudioClip arrowClickSuccessClip;
    public AudioClip arrowClickBlockedClip; // Inspector ma pachi assign karishu jyare sound made

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

    void PlaySfx(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }
}

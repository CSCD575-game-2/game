using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Music")]
    public AudioClip backgroundMusic;

    [Header("SFX")]
    public AudioClip buttonClick;
    public AudioClip deployShip;
    public AudioClip battleStart;
    public AudioClip battleEnd;

    private bool muted = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        PlayMusic();
    }

    public void PlayMusic()
    {
        if (musicSource && backgroundMusic)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlayButtonClick()
    {
        PlaySFX(buttonClick);
    }

    public void SetVolume(float volume)
    {
        musicSource.volume = volume;
    }

    public void ToggleMute()
    {
        muted = !muted;
        AudioListener.volume = muted ? 0f : 1f;
    }

    private void PlaySFX(AudioClip clip)
    {
        if (sfxSource && clip)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}
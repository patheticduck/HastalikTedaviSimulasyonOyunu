using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Music")]
    public AudioClip mainMenuMusic;
    public AudioClip[] gameplayMusics;

    [Header("SFX")]
    public AudioClip clickSound;
    public AudioClip winSound;
    public AudioClip loseSound;
    public AudioClip bookOpenCloseSound;
    public AudioClip pageTurnSound;

    [Header("Volume Values")]
    [Range(0f, 1f)] public float mainMenuVolume = 1f;
    [Range(0f, 1f)] public float gameplayVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    private Coroutine gameplayRoutine;
    private bool isMainMenuMusicPlaying = false;
    private bool isGameplayMusicPlaying = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ApplySFXVolume();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayMainMenuMusic()
    {
        StopGameplayMusic();

        if (mainMenuMusic == null || musicSource == null)
            return;

        isMainMenuMusicPlaying = true;
        isGameplayMusicPlaying = false;

        musicSource.Stop();
        musicSource.volume = mainMenuVolume;
        musicSource.clip = mainMenuMusic;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlayGameplayMusic()
    {
        if (musicSource == null || gameplayMusics == null || gameplayMusics.Length == 0)
            return;

        isMainMenuMusicPlaying = false;
        isGameplayMusicPlaying = true;

        StopGameplayMusic();
        gameplayRoutine = StartCoroutine(GameplayMusicLoop());
    }

    private IEnumerator GameplayMusicLoop()
    {
        int index = 0;

        while (true)
        {
            AudioClip clip = gameplayMusics[index];

            if (clip != null)
            {
                musicSource.Stop();
                musicSource.volume = gameplayVolume;
                musicSource.clip = clip;
                musicSource.loop = false;
                musicSource.Play();

                yield return new WaitForSeconds(clip.length);
            }
            else
            {
                yield return null;
            }

            index++;

            if (index >= gameplayMusics.Length)
                index = 0;
        }
    }

    public void StopGameplayMusic()
    {
        if (gameplayRoutine != null)
        {
            StopCoroutine(gameplayRoutine);
            gameplayRoutine = null;
        }
    }

    public void StopAllMusic()
    {
        StopGameplayMusic();

        isMainMenuMusicPlaying = false;
        isGameplayMusicPlaying = false;

        if (musicSource != null)
            musicSource.Stop();
    }

    public void PlayClick()
    {
        PlaySFX(clickSound);
    }

    public void PlayWin()
    {
        PlaySFX(winSound);
    }

    public void PlayLose()
    {
        PlaySFX(loseSound);
    }

    public void PlayBookOpenClose()
    {
        PlaySFX(bookOpenCloseSound);
    }

    public void PlayPageTurn()
    {
        PlaySFX(pageTurnSound);
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null)
            return;

        sfxSource.volume = sfxVolume;
        sfxSource.PlayOneShot(clip);
    }

    public void SetMainMenuVolume(float value)
    {
        mainMenuVolume = Mathf.Clamp01(value);

        if (musicSource != null && isMainMenuMusicPlaying)
            musicSource.volume = mainMenuVolume;
    }

    public void SetGameplayVolume(float value)
    {
        gameplayVolume = Mathf.Clamp01(value);

        if (musicSource != null && isGameplayMusicPlaying)
            musicSource.volume = gameplayVolume;
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        ApplySFXVolume();
    }

    private void ApplySFXVolume()
    {
        if (sfxSource != null)
            sfxSource.volume = sfxVolume;
    }
}
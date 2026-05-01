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

    private Coroutine gameplayRoutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // RuntimeSaveManager gibi sahneler arası korunur
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // =========================
    // 🎵 MAIN MENU MUSIC
    // =========================
    public void PlayMainMenuMusic()
    {
        StopGameplayMusic();

        if (mainMenuMusic == null || musicSource == null)
            return;

        musicSource.Stop();
        musicSource.clip = mainMenuMusic;
        musicSource.loop = true;
        musicSource.Play();
    }

    // =========================
    // 🎮 GAMEPLAY MUSIC
    // =========================
    public void PlayGameplayMusic()
    {
        if (gameplayMusics == null || gameplayMusics.Length == 0)
            return;

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

        if (musicSource != null)
            musicSource.Stop();
    }

    // =========================
    // 🔊 SFX
    // =========================
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

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null)
            return;

        sfxSource.PlayOneShot(clip);
    }
}
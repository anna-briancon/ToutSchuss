using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;      // Pour les musiques
    public AudioSource sfxSource;        // Pour les sons courts

    [Header("Musiques")]
    public AudioClip menuMusic;          // Musique menu + game over
    public AudioClip gameMusic;          // Musique pendant le jeu
    public AudioClip gameOverSound;      // Son de game over

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    public void PlayMenuMusic()
    {
        musicSource.loop = true;
        musicSource.clip = menuMusic;
        musicSource.Play();
    }

    public void PlayGameMusic()
    {
        musicSource.loop = true;
        musicSource.clip = gameMusic;
        musicSource.Play();
    }

    public void PlayGameOver()
    {
        musicSource.Stop();
        sfxSource.PlayOneShot(gameOverSound);
        // Attend la fin du son game over puis relance la musique menu
        StartCoroutine(PlayMenuMusicAfterDelay(gameOverSound.length));
    }

    IEnumerator PlayMenuMusicAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        PlayMenuMusic();
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }
}
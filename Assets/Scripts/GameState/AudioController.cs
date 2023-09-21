using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Sound
{
    Win,
    Lose,
    Jump,
    Dash,
    ButtonPress,
    PlatformSpawn,
    CatSpawn,
    CatClose
}

public class AudioController : MonoBehaviour
{
    public AudioSource currentMusic;
    private List<AudioSource> sounds;

    public IEnumerator InitializeAudio()
    {
        while (!currentMusic)
        {
            currentMusic = GameObject.Find("Music").GetComponent<AudioSource>();
            yield return null;
        }
        currentMusic.Play();
        sounds = GameObject.Find("Sounds").GetComponentsInChildren<AudioSource>().ToList();
    }

    public void StopMusic()
    {
        if (currentMusic)
            currentMusic.Pause();
        currentMusic = null;
    }

    public void ToggleMusic(bool isPaused)
    {
        if (currentMusic)
        {
            if (isPaused)
            {
                currentMusic.Pause();
            }
            else
            {
                currentMusic.Play();
            }
        }
    }

    public void PlaySound(Sound soundCondition)
    {
        var sound = FindSound(soundCondition);
        sound.Play();
    }

    public void StopSound(Sound soundCondition)
    {
        var sound = FindSound(soundCondition);
        sound.Stop();
    }

    private AudioSource FindSound(Sound soundCondition)
    {
        return sounds.FirstOrDefault(s => s.gameObject.name.Equals(soundCondition.ToString()));
    }
}

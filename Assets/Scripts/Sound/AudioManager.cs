using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] List<AudioSource> audioSourcePool;

    [SerializeField] float globalVolume = 0.5f; //Temp until settings are added
    const string NOT_ENOUGH_SOURCES_ERROR = "Not enough audio sources for the requested clips! Consider adding more to this gameObject to avoid expensive AddComponent.";

    static AudioManager instance;
    public static AudioManager Instance
    {
        get { return instance; }
    }

    List<AudioSource> musicSources = new List<AudioSource>(); //Save sources playing music here so we don't touch them

    const float RANDOM_PITCH_RANGE = 0.25f;

    void OnEnable()
    {
        if (instance != null)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(gameObject);
        InitSources();
    }

    public void PlayAudioClip(AudioClip ClipToPlay, bool RandomPitch = false)
    {
        AudioSource FreeSource = GetFreeSourceFromPool();
        if (RandomPitch)
        {
            FreeSource.pitch -= Random.Range(-RANDOM_PITCH_RANGE, RANDOM_PITCH_RANGE);
        }
        FreeSource.PlayOneShot(ClipToPlay);
    }

    public void PlayLoopedAudioClip(AudioClip ClipToPlay, bool OnlyPermitOne = true, bool EndLoop = false, bool IsMusic = false)
    {
        if (OnlyPermitOne || EndLoop)
        {
            for (int s = 0; s < audioSourcePool.Count; s++)
            {
                if (audioSourcePool[s].clip == ClipToPlay
                    && audioSourcePool[s].loop)
                {
                    if (EndLoop)
                    {
                        if (IsMusic)
                        {
                            musicSources.Remove(audioSourcePool[s]);
                        }
                        audioSourcePool[s].Stop();
                        audioSourcePool[s].loop = false;
                        audioSourcePool[s].clip = null;
                    }
                    return;
                }
            }
            if (EndLoop)
            {
                return;
            }
        }

        AudioSource FreeSource = GetFreeSourceFromPool();
        FreeSource.loop = true;
        FreeSource.clip = ClipToPlay;
        if (IsMusic)
        {
            musicSources.Add(FreeSource);
        }
        FreeSource.Play();
    }

    void CleanList()
    {
        for(int a = 0; a < musicSources.Count; a++)
        {
            if (!musicSources[a])
            {
                musicSources.Remove(musicSources[a]);
            }
        }
    }

    void Update()
    {
        CleanList();
    }

    public void StopAllLoops()
    {
        for (int s = 0; s < audioSourcePool.Count; s++)
        {
            if (audioSourcePool[s].loop && !musicSources.Contains(audioSourcePool[s]))
            {
                audioSourcePool[s].Stop();
                audioSourcePool[s].loop = false;
                audioSourcePool[s].clip = null;
            }
        }
    }

    void InitSources()
    {
        for (int s = 0; s < audioSourcePool.Count; s++)
        {
            if (audioSourcePool[s] == null)
            {
                continue;
            }

            audioSourcePool[s].volume = globalVolume;
        }
    }

    AudioSource GetFreeSourceFromPool()
    {
        for (int s = 0; s < audioSourcePool.Count; s++)
        {
            if (audioSourcePool[s] == null)
            {
                continue;
            }

            if (!audioSourcePool[s].isPlaying)
            {
                audioSourcePool[s].pitch = 1;
                audioSourcePool[s].clip = null;
                audioSourcePool[s].loop = false;
                return audioSourcePool[s];
            }
        }

        //Last resort if there are no free sources
        AudioSource NewSource = this.AddComponent<AudioSource>();
        NewSource.volume = globalVolume;
        audioSourcePool.Add(NewSource);
        Debug.LogWarning(NOT_ENOUGH_SOURCES_ERROR);
        return NewSource;
    }
}

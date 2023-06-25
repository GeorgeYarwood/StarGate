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

    void Start()
    {
        if (instance != null)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }

        InitSources();
    }

    public void PlayAudioClip(AudioClip ClipToPlay)
    {
        GetFreeSourceFromPool().PlayOneShot(ClipToPlay);
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
        for(int s = 0; s < audioSourcePool.Count; s++)
        {
            if (audioSourcePool[s] == null)
            {
                continue;
            }

            if (!audioSourcePool[s].isPlaying)
            {
                return audioSourcePool[s];
            }
        }

        //Last resort if there are no free sources
        AudioSource NewSource = this.AddComponent<AudioSource>();
        NewSource.volume = globalVolume;
        audioSourcePool.Add(NewSource);
        Debug.Log(NOT_ENOUGH_SOURCES_ERROR);
        return NewSource;
    }
}

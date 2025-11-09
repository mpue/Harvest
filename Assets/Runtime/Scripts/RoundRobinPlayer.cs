using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

public class RoundRobinPlayer : MonoBehaviour
{
    public AudioClip[] clips;
    public int numAudioSources;
    private AudioSource[] sources;
    public AudioMixerGroup mixerGroup;

    private int currentSourceIndex = 0;
    private Dictionary<string, AudioClip> clipList;

    void Start()
    {
        sources = new AudioSource[numAudioSources];

        for (int i = 0; i < sources.Length; i++)
        {
            sources[i] = gameObject.AddComponent<AudioSource>();
            sources[i].pitch = 1;
            sources[i].spatialize = false;
            sources[i].playOnAwake = false;
            sources[i].volume = 1;
            sources[i].outputAudioMixerGroup = mixerGroup;
        }

        clipList = new Dictionary<string, AudioClip>();

        foreach (AudioClip clip in clips)
        {
            clipList.Add(clip.name, clip);
        }

    }

    public void Play(string clipName)
    {
        sources[currentSourceIndex].clip = clipList[clipName];
        sources[currentSourceIndex].Play();
        currentSourceIndex = (currentSourceIndex + 1) % sources.Length;
    }

}

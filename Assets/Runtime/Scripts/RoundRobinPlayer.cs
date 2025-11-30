using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class RoundRobinPlayer : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip[] clips;

    [Header("AudioSource Settings")]
    [SerializeField] private int numAudioSources = 8;
    [SerializeField] private AudioMixerGroup mixerGroup;
    [SerializeField] private AudioManager.AudioCategory category = AudioManager.AudioCategory.WeaponSounds;

    [Header("Playback Settings")]
    [SerializeField] private bool use3DSound = true;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;
    [SerializeField, Range(0f, 1f)] private float pitchVariation = 0.1f;
    [SerializeField] private bool randomizeClipSelection = false;

    [Header("Performance")]
    [SerializeField] private bool useAudioManager = true;
    [SerializeField] private int maxConcurrentSounds = 16;

    private AudioSource[] sources;
    private int currentSourceIndex = 0;
    private Dictionary<string, AudioClip> clipList;
    private int activeSoundCount = 0;

    void Start()
    {
        InitializeAudioSources();
        BuildClipDictionary();
    }

    /// <summary>
    /// Initialize all AudioSources
    /// </summary>
    private void InitializeAudioSources()
    {
        sources = new AudioSource[numAudioSources];

        for (int i = 0; i < sources.Length; i++)
        {
            sources[i] = gameObject.AddComponent<AudioSource>();
            sources[i].playOnAwake = false;
            sources[i].volume = volume;
            sources[i].spatialBlend = use3DSound ? 1f : 0f;

            // Setup with AudioManager if available
            if (useAudioManager && AudioManager.Instance != null)
            {
                AudioManager.Instance.SetupAudioSource(sources[i], category);
            }
            else if (mixerGroup != null)
            {
                sources[i].outputAudioMixerGroup = mixerGroup;
            }
        }

        Debug.Log($"RoundRobinPlayer '{gameObject.name}': Initialized {numAudioSources} AudioSources");
    }

    /// <summary>
    /// Build clip dictionary for fast lookup
    /// </summary>
    private void BuildClipDictionary()
    {
        clipList = new Dictionary<string, AudioClip>();

        if (clips != null)
        {
            foreach (AudioClip clip in clips)
            {
                if (clip != null && !clipList.ContainsKey(clip.name))
                {
                    clipList.Add(clip.name, clip);
                }
            }
        }

        Debug.Log($"RoundRobinPlayer '{gameObject.name}': Loaded {clipList.Count} audio clips");
    }

    /// <summary>
    /// Play clip by name
    /// </summary>
    public void Play(string clipName)
    {
        if (clipList == null || !clipList.ContainsKey(clipName))
        {
            Debug.LogWarning($"RoundRobinPlayer: Clip '{clipName}' not found!");
            return;
        }

        PlayClip(clipList[clipName]);
    }

    /// <summary>
    /// Play random clip from array
    /// </summary>
    public void PlayRandom()
    {
        if (clips == null || clips.Length == 0)
        {
            Debug.LogWarning("RoundRobinPlayer: No clips available!");
            return;
        }

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        PlayClip(clip);
    }

    /// <summary>
    /// Play specific clip
    /// </summary>
    public void PlayClip(AudioClip clip)
    {
        if (clip == null) return;

        // Limit concurrent sounds for performance
        if (activeSoundCount >= maxConcurrentSounds)
        {
            // Find and stop oldest playing source
            StopOldestSound();
        }

        // Get next available source
        AudioSource source = GetNextAvailableSource();

        if (source != null)
        {
            source.clip = clip;
            source.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
            source.Play();

            activeSoundCount++;
            StartCoroutine(TrackSoundCompletion(source));
        }
    }

    /// <summary>
    /// Get next available AudioSource in round-robin fashion
    /// </summary>
    private AudioSource GetNextAvailableSource()
    {
        int startIndex = currentSourceIndex;

        // Try to find non-playing source
        for (int i = 0; i < sources.Length; i++)
        {
            int index = (startIndex + i) % sources.Length;

            if (!sources[index].isPlaying)
            {
                currentSourceIndex = (index + 1) % sources.Length;
                return sources[index];
            }
        }

        // All sources are playing - use round-robin regardless
        AudioSource source = sources[currentSourceIndex];
        currentSourceIndex = (currentSourceIndex + 1) % sources.Length;

        return source;
    }

    /// <summary>
    /// Stop oldest playing sound
    /// </summary>
    private void StopOldestSound()
    {
        for (int i = 0; i < sources.Length; i++)
        {
            if (sources[i].isPlaying)
            {
                sources[i].Stop();
                activeSoundCount--;
                return;
            }
        }
    }

    /// <summary>
    /// Track when sound completes
    /// </summary>
    private IEnumerator TrackSoundCompletion(AudioSource source)
    {
        while (source != null && source.isPlaying)
        {
            yield return null;
        }

        activeSoundCount = Mathf.Max(0, activeSoundCount - 1);
    }

    /// <summary>
    /// Add clip at runtime
    /// </summary>
    public void AddClip(AudioClip clip)
    {
        if (clip == null || clipList.ContainsKey(clip.name)) return;

        clipList.Add(clip.name, clip);
    }

    /// <summary>
    /// Set volume for all sources
    /// </summary>
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);

        if (sources != null)
        {
            foreach (AudioSource source in sources)
            {
                if (source != null)
                {
                    source.volume = volume;
                }
            }
        }
    }

    /// <summary>
    /// Stop all sounds
    /// </summary>
    public void StopAll()
    {
        if (sources != null)
        {
            foreach (AudioSource source in sources)
            {
                if (source != null && source.isPlaying)
                {
                    source.Stop();
                }
            }
        }

        activeSoundCount = 0;
    }

    /// <summary>
    /// Get active sound count
    /// </summary>
    public int GetActiveSoundCount()
    {
        return activeSoundCount;
    }

    /// <summary>
    /// Check if a specific clip exists
    /// </summary>
    public bool HasClip(string clipName)
    {
        return clipList != null && clipList.ContainsKey(clipName);
    }
}

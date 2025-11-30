using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Centralized Audio Manager for RTS Game
/// Manages AudioMixerGroups and provides utilities for AudioSource setup
/// Supports AudioSource pooling for large battles
/// </summary>
public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                // we only want one AudioManager in the scene
                instance = FindFirstObjectByType<AudioManager>();

                if (instance == null)
                {
                    GameObject obj = new GameObject("AudioManager");
                    instance = obj.AddComponent<AudioManager>();
                    DontDestroyOnLoad(obj);
                }
            }
            return instance;
        }
    }

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer mainMixer;

    [Header("Mixer Groups")]
    [SerializeField] private AudioMixerGroup masterGroup;
    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;
    [SerializeField] private AudioMixerGroup uiGroup;
    [SerializeField] private AudioMixerGroup unitSoundsGroup;
    [SerializeField] private AudioMixerGroup weaponSoundsGroup;
    [SerializeField] private AudioMixerGroup ambientGroup;

    [Header("Default Settings")]
    [SerializeField] private AudioMixerGroup defaultMixerGroup;
    [SerializeField] private bool assignMixerGroupOnAudioSourceCreation = true;

    [Header("Volume Settings")]
    [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float uiVolume = 1f;

    [Header("AudioSource Pooling")]
    [SerializeField] private bool usePooling = true;
    [SerializeField] private int initialPoolSize = 20;
    [SerializeField] private int maxPoolSize = 50;
    [SerializeField] private bool autoExpandPool = true;

    // AudioSource pools per category
    private Dictionary<AudioCategory, Queue<AudioSource>> audioSourcePools;
    private Dictionary<AudioCategory, List<AudioSource>> activeAudioSources;
    private GameObject poolContainer;

    // Audio category enum
    public enum AudioCategory
    {
        Master,
        Music,
        SFX,
        UI,
        UnitSounds,
        WeaponSounds,
        Ambient
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Set default mixer group if not assigned
        if (defaultMixerGroup == null && sfxGroup != null)
        {
            defaultMixerGroup = sfxGroup;
        }

        // Initialize pooling system
        InitializeAudioSourcePools();

        ApplyVolumeSettings();
    }

    /// <summary>
    /// Initialize AudioSource pools for all categories
    /// </summary>
    private void InitializeAudioSourcePools()
    {
        if (!usePooling) return;

        // Create container for pooled AudioSources
        poolContainer = new GameObject("AudioSourcePool");
        poolContainer.transform.SetParent(transform);

        audioSourcePools = new Dictionary<AudioCategory, Queue<AudioSource>>();
        activeAudioSources = new Dictionary<AudioCategory, List<AudioSource>>();

        // Initialize pools for each category
        foreach (AudioCategory category in System.Enum.GetValues(typeof(AudioCategory)))
        {
            audioSourcePools[category] = new Queue<AudioSource>();
            activeAudioSources[category] = new List<AudioSource>();

            // Pre-create initial pool for weapon sounds (most used in battles)
            int poolSize = category == AudioCategory.WeaponSounds ? initialPoolSize : initialPoolSize / 4;

            for (int i = 0; i < poolSize; i++)
            {
                CreatePooledAudioSource(category);
            }
        }

        Debug.Log($"AudioManager: Initialized {initialPoolSize} pooled AudioSources for weapons");
    }

    /// <summary>
    /// Create a new pooled AudioSource
    /// </summary>
    private AudioSource CreatePooledAudioSource(AudioCategory category)
    {
        GameObject obj = new GameObject($"PooledAudio_{category}");
        obj.transform.SetParent(poolContainer.transform);
        obj.SetActive(false);

        AudioSource source = obj.AddComponent<AudioSource>();
        source.playOnAwake = false;
        SetupAudioSource(source, category);

        audioSourcePools[category].Enqueue(source);
        return source;
    }

    /// <summary>
    /// Get an AudioSource from the pool
    /// </summary>
    private AudioSource GetPooledAudioSource(AudioCategory category)
    {
        if (!usePooling)
        {
            // Fallback to non-pooled behavior
            GameObject tempObj = new GameObject($"TempAudio_{category}");
            tempObj.transform.SetParent(transform);
            return CreateAudioSource(tempObj, category);
        }

        AudioSource source = null;

        // Try to get from pool
        if (audioSourcePools[category].Count > 0)
        {
            source = audioSourcePools[category].Dequeue();
        }
        // Expand pool if allowed
        else if (autoExpandPool && GetTotalPoolSize(category) < maxPoolSize)
        {
            source = CreatePooledAudioSource(category);
            audioSourcePools[category].Dequeue(); // Remove it from pool queue
        }
        // Pool is full and can't expand - reuse oldest active source
        else
        {
            Debug.LogWarning($"AudioSource pool for {category} exhausted! Reusing oldest source.");
            if (activeAudioSources[category].Count > 0)
            {
                source = activeAudioSources[category][0];
                activeAudioSources[category].RemoveAt(0);
                source.Stop();
            }
            else
            {
                // Emergency fallback
                source = CreatePooledAudioSource(category);
            }
        }

        if (source != null)
        {
            source.gameObject.SetActive(true);
            activeAudioSources[category].Add(source);
        }

        return source;
    }

    /// <summary>
    /// Return AudioSource to pool
    /// </summary>
    private void ReturnToPool(AudioSource source, AudioCategory category)
    {
        if (source == null || !usePooling) return;

        source.Stop();
        source.clip = null;
        source.gameObject.SetActive(false);

        if (activeAudioSources[category].Contains(source))
        {
            activeAudioSources[category].Remove(source);
        }

        if (!audioSourcePools[category].Contains(source))
        {
            audioSourcePools[category].Enqueue(source);
        }
    }

    /// <summary>
    /// Get total pool size for a category
    /// </summary>
    private int GetTotalPoolSize(AudioCategory category)
    {
        return audioSourcePools[category].Count + activeAudioSources[category].Count;
    }

    /// <summary>
    /// Get mixer group by category
    /// </summary>
    public AudioMixerGroup GetMixerGroup(AudioCategory category)
    {
        switch (category)
        {
            case AudioCategory.Master:
                return masterGroup;
            case AudioCategory.Music:
                return musicGroup;
            case AudioCategory.SFX:
                return sfxGroup;
            case AudioCategory.UI:
                return uiGroup;
            case AudioCategory.UnitSounds:
                return unitSoundsGroup != null ? unitSoundsGroup : sfxGroup;
            case AudioCategory.WeaponSounds:
                return weaponSoundsGroup != null ? weaponSoundsGroup : sfxGroup;
            case AudioCategory.Ambient:
                return ambientGroup != null ? ambientGroup : sfxGroup;
            default:
                return defaultMixerGroup;
        }
    }

    /// <summary>
    /// Get default mixer group
    /// </summary>
    public AudioMixerGroup GetDefaultMixerGroup()
    {
        return defaultMixerGroup;
    }

    /// <summary>
    /// Setup an AudioSource with mixer group
    /// </summary>
    public void SetupAudioSource(AudioSource audioSource, AudioCategory category = AudioCategory.SFX)
    {
        if (audioSource == null) return;

        AudioMixerGroup mixerGroup = GetMixerGroup(category);
        if (mixerGroup != null)
        {
            audioSource.outputAudioMixerGroup = mixerGroup;
        }
    }

    /// <summary>
    /// Create and setup a new AudioSource component
    /// </summary>
    public AudioSource CreateAudioSource(GameObject target, AudioCategory category = AudioCategory.SFX, bool playOnAwake = false)
    {
        if (target == null) return null;

        AudioSource audioSource = target.AddComponent<AudioSource>();
        audioSource.playOnAwake = playOnAwake;
        SetupAudioSource(audioSource, category);

        return audioSource;
    }

    /// <summary>
    /// Assign mixer groups to all AudioSources in scene
    /// </summary>
    public void AssignMixerGroupsToAllAudioSources(AudioCategory category = AudioCategory.SFX)
    {
        AudioSource[] allAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        int count = 0;

        foreach (AudioSource audioSource in allAudioSources)
        {
            if (audioSource.outputAudioMixerGroup == null)
            {
                SetupAudioSource(audioSource, category);
                count++;
            }
        }

        Debug.Log($"AudioManager: Assigned mixer groups to {count} AudioSources");
    }

    /// <summary>
    /// Assign specific mixer group to all AudioSources in scene
    /// </summary>
    public void AssignSpecificMixerGroupToAll(AudioMixerGroup mixerGroup)
    {
        if (mixerGroup == null)
        {
            Debug.LogWarning("AudioManager: Cannot assign null mixer group");
            return;
        }

        AudioSource[] allAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        int count = 0;

        foreach (AudioSource audioSource in allAudioSources)
        {
            audioSource.outputAudioMixerGroup = mixerGroup;
            count++;
        }

        Debug.Log($"AudioManager: Assigned '{mixerGroup.name}' to {count} AudioSources");
    }

    /// <summary>
    /// Find AudioSources without mixer groups
    /// </summary>
    public List<AudioSource> FindAudioSourcesWithoutMixerGroup()
    {
        List<AudioSource> sourcesWithoutGroup = new List<AudioSource>();
        AudioSource[] allAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);

        foreach (AudioSource audioSource in allAudioSources)
        {
            if (audioSource.outputAudioMixerGroup == null)
            {
                sourcesWithoutGroup.Add(audioSource);
            }
        }

        return sourcesWithoutGroup;
    }

    /// <summary>
    /// Apply volume settings to mixer
    /// </summary>
    private void ApplyVolumeSettings()
    {
        if (mainMixer == null) return;

        mainMixer.SetFloat("MasterVolume", Mathf.Log10(masterVolume) * 20);
        mainMixer.SetFloat("MusicVolume", Mathf.Log10(musicVolume) * 20);
        mainMixer.SetFloat("SFXVolume", Mathf.Log10(sfxVolume) * 20);
        mainMixer.SetFloat("UIVolume", Mathf.Log10(uiVolume) * 20);
    }

    /// <summary>
    /// Set master volume
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        if (mainMixer != null)
        {
            mainMixer.SetFloat("MasterVolume", Mathf.Log10(masterVolume) * 20);
        }
    }

    /// <summary>
    /// Set music volume
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (mainMixer != null)
        {
            mainMixer.SetFloat("MusicVolume", Mathf.Log10(musicVolume) * 20);
        }
    }

    /// <summary>
    /// Set SFX volume
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (mainMixer != null)
        {
            mainMixer.SetFloat("SFXVolume", Mathf.Log10(sfxVolume) * 20);
        }
    }

    /// <summary>
    /// Set UI volume
    /// </summary>
    public void SetUIVolume(float volume)
    {
        uiVolume = Mathf.Clamp01(volume);
        if (mainMixer != null)
        {
            mainMixer.SetFloat("UIVolume", Mathf.Log10(uiVolume) * 20);
        }
    }

    /// <summary>
    /// Play a one-shot sound with automatic mixer group assignment
    /// Uses pooling for better performance in large battles
    /// </summary>
    public void PlayOneShot(AudioClip clip, Vector3 position, AudioCategory category = AudioCategory.SFX, float volume = 1f)
    {
        if (clip == null) return;

        if (usePooling)
        {
            AudioSource source = GetPooledAudioSource(category);
            if (source != null)
            {
                source.transform.position = position;
                source.clip = clip;
                source.volume = volume;
                source.spatialBlend = 1f; // 3D sound
                source.Play();

                StartCoroutine(ReturnToPoolWhenFinished(source, category));
            }
        }
        else
        {
            // Non-pooled fallback
            GameObject tempObj = new GameObject("TempAudio_" + clip.name);
            tempObj.transform.position = position;
            tempObj.transform.SetParent(transform);

            AudioSource audioSource = CreateAudioSource(tempObj, category);
            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.spatialBlend = 1f;
            audioSource.Play();

            StartCoroutine(DestroyAfterPlaying(tempObj, audioSource));
        }
    }

    /// <summary>
    /// Play a 2D one-shot sound with automatic mixer group assignment
    /// Uses pooling for better performance
    /// </summary>
    public void PlayOneShot2D(AudioClip clip, AudioCategory category = AudioCategory.UI, float volume = 1f)
    {
        if (clip == null) return;

        if (usePooling)
        {
            AudioSource source = GetPooledAudioSource(category);
            if (source != null)
            {
                source.transform.position = transform.position;
                source.clip = clip;
                source.volume = volume;
                source.spatialBlend = 0f; // 2D sound
                source.Play();

                StartCoroutine(ReturnToPoolWhenFinished(source, category));
            }
        }
        else
        {
            // Non-pooled fallback
            GameObject tempObj = new GameObject("TempAudio2D_" + clip.name);
            tempObj.transform.SetParent(transform);

            AudioSource audioSource = CreateAudioSource(tempObj, category);
            audioSource.spatialBlend = 0f;
            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.Play();

            StartCoroutine(DestroyAfterPlaying(tempObj, audioSource));
        }
    }

    /// <summary>
    /// Play one-shot from array (random selection) - optimized for battles
    /// </summary>
    public void PlayOneShotRandom(AudioClip[] clips, Vector3 position, AudioCategory category = AudioCategory.WeaponSounds, float volume = 1f)
    {
        if (clips == null || clips.Length == 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        PlayOneShot(clip, position, category, volume);
    }

    /// <summary>
    /// Coroutine to return AudioSource to pool when finished
    /// </summary>
    private IEnumerator ReturnToPoolWhenFinished(AudioSource source, AudioCategory category)
    {
        if (source == null) yield break;

        // Wait until audio finishes playing
        while (source != null && source.isPlaying)
        {
            yield return null;
        }

        // Small buffer
        yield return new WaitForSeconds(0.05f);

        // Return to pool
        ReturnToPool(source, category);
    }

    /// <summary>
    /// Coroutine to destroy audio GameObject after sound finishes playing
    /// </summary>
    private IEnumerator DestroyAfterPlaying(GameObject obj, AudioSource source)
    {
        if (source == null || obj == null)
        {
            yield break;
        }

        // Wait until the audio source stops playing
        while (source != null && source.isPlaying)
        {
            yield return null;
        }

        // Add a small buffer to ensure sound fully completes
        yield return new WaitForSeconds(0.1f);

        // Safely destroy the object
        if (obj != null)
        {
            Destroy(obj);
        }
    }
}

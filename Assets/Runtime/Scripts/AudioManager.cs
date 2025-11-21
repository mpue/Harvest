using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

/// <summary>
/// Centralized Audio Manager for RTS Game
/// Manages AudioMixerGroups and provides utilities for AudioSource setup
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

        ApplyVolumeSettings();
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
    /// </summary>
    public void PlayOneShot(AudioClip clip, Vector3 position, AudioCategory category = AudioCategory.SFX, float volume = 1f)
    {
        if (clip == null) return;

        GameObject tempObj = new GameObject("TempAudio");
        tempObj.transform.position = position;

        AudioSource audioSource = CreateAudioSource(tempObj, category);
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();

        Destroy(tempObj, clip.length + 0.1f);
    }

    /// <summary>
    /// Play a 2D one-shot sound with automatic mixer group assignment
    /// </summary>
    public void PlayOneShot2D(AudioClip clip, AudioCategory category = AudioCategory.UI, float volume = 1f)
    {
        if (clip == null) return;

        GameObject tempObj = new GameObject("TempAudio2D");
        AudioSource audioSource = CreateAudioSource(tempObj, category);
        audioSource.spatialBlend = 0f; // 2D sound
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();

        Destroy(tempObj, clip.length + 0.1f);
    }
}

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// High-performance audio player for weapon fire sounds in large battles
/// Uses object pooling and intelligent sound prioritization
/// </summary>
public class WeaponAudioPlayer : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private int maxConcurrentSounds = 32;
    [SerializeField] private bool usePrioritySystem = true;
    [SerializeField] private float minTimeBetweenSameSounds = 0.05f;

    [Header("Distance-Based Settings")]
    [SerializeField] private bool useDistancePriority = true;
    [SerializeField] private float maxAudibleDistance = 100f;
    [SerializeField] private bool cullDistantSounds = true;

    [Header("Performance")]
    [SerializeField] private bool useAudioManager = true;
    [SerializeField] private AudioManager.AudioCategory category = AudioManager.AudioCategory.WeaponSounds;

    private Dictionary<AudioClip, float> lastPlayTimes;
    private Transform listenerTransform;
    private static WeaponAudioPlayer instance;

    public static WeaponAudioPlayer Instance
    {
        get
        {
if (instance == null)
 {
  GameObject obj = new GameObject("WeaponAudioPlayer");
     instance = obj.AddComponent<WeaponAudioPlayer>();
      DontDestroyOnLoad(obj);
            }
            return instance;
        }
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

   lastPlayTimes = new Dictionary<AudioClip, float>();
    }

    void Start()
    {
        // Find audio listener
        AudioListener listener = FindObjectOfType<AudioListener>();
     if (listener != null)
        {
          listenerTransform = listener.transform;
        }
    else
        {
          // Fallback to main camera
  if (Camera.main != null)
      {
     listenerTransform = Camera.main.transform;
     }
        }

        Debug.Log($"WeaponAudioPlayer initialized with max {maxConcurrentSounds} concurrent sounds");
    }

    /// <summary>
    /// Play weapon fire sound with intelligent culling and prioritization
    /// </summary>
    public void PlayWeaponSound(AudioClip clip, Vector3 position, float volume = 1f)
    {
if (clip == null) return;

        // Distance culling
     if (cullDistantSounds && listenerTransform != null)
        {
float distance = Vector3.Distance(position, listenerTransform.position);
            if (distance > maxAudibleDistance)
            {
            return; // Too far away, don't play
    }

   // Distance-based volume adjustment
    if (useDistancePriority)
       {
        float distanceFactor = 1f - Mathf.Clamp01(distance / maxAudibleDistance);
       volume *= distanceFactor;
            }
        }

        // Rate limiting for same sound
        if (lastPlayTimes.ContainsKey(clip))
        {
      float timeSinceLastPlay = Time.time - lastPlayTimes[clip];
    if (timeSinceLastPlay < minTimeBetweenSameSounds)
    {
   return; // Too soon, skip this sound
  }
        }

        // Play using AudioManager if available
        if (useAudioManager && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayOneShot(clip, position, category, volume);
        }
        else
        {
        // Fallback to AudioSource.PlayClipAtPoint
        AudioSource.PlayClipAtPoint(clip, position, volume);
        }

// Update last play time
    lastPlayTimes[clip] = Time.time;
    }

    /// <summary>
    /// Play random weapon sound from array
    /// </summary>
public void PlayWeaponSoundRandom(AudioClip[] clips, Vector3 position, float volume = 1f)
    {
        if (clips == null || clips.Length == 0) return;

  AudioClip clip = clips[Random.Range(0, clips.Length)];
     PlayWeaponSound(clip, position, volume);
    }

    /// <summary>
    /// Set max concurrent sounds limit
    /// </summary>
    public void SetMaxConcurrentSounds(int max)
    {
 maxConcurrentSounds = Mathf.Max(1, max);
    }

    /// <summary>
    /// Clear play time cache
    /// </summary>
    public void ClearCache()
    {
        lastPlayTimes.Clear();
    }

    /// <summary>
    /// Get distance to listener
    /// </summary>
    public float GetDistanceToListener(Vector3 position)
    {
    if (listenerTransform == null) return 0f;
        return Vector3.Distance(position, listenerTransform.position);
    }

    /// <summary>
    /// Check if position is within audible range
    /// </summary>
    public bool IsAudible(Vector3 position)
    {
    if (!cullDistantSounds) return true;
        return GetDistanceToListener(position) <= maxAudibleDistance;
    }
}

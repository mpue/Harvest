using UnityEngine;

/// <summary>
/// Helper component to easily add Round-Robin audio to any GameObject
/// Attach this to weapons, units, or buildings for optimized sound playback
/// </summary>
[RequireComponent(typeof(RoundRobinPlayer))]
public class AudioPlayerHelper : MonoBehaviour
{
    [Header("Quick Setup")]
 [SerializeField] private AudioClip[] soundClips;
    [SerializeField] private int numberOfAudioSources = 4;
    [SerializeField] private AudioManager.AudioCategory soundCategory = AudioManager.AudioCategory.SFX;

    [Header("Playback Options")]
    [SerializeField] private bool playOnStart = false;
    [SerializeField] private bool randomizeOnPlay = true;
    [SerializeField] private float playInterval = 1f;

    private RoundRobinPlayer roundRobinPlayer;
    private float nextPlayTime = 0f;

    void Awake()
    {
        roundRobinPlayer = GetComponent<RoundRobinPlayer>();
        if (roundRobinPlayer == null)
     {
            roundRobinPlayer = gameObject.AddComponent<RoundRobinPlayer>();
        }

        // Configure the RoundRobinPlayer
      ConfigurePlayer();
    }

    void Start()
    {
        if (playOnStart)
        {
  PlaySound();
        }
    }

    void Update()
    {
        if (playOnStart && Time.time >= nextPlayTime)
        {
            PlaySound();
            nextPlayTime = Time.time + playInterval;
        }
    }

    /// <summary>
    /// Configure the RoundRobinPlayer with settings
    /// </summary>
    private void ConfigurePlayer()
    {
      // Use reflection to set private fields if needed
  // For now, clips need to be set in inspector
        if (soundClips != null && soundClips.Length > 0)
        {
            roundRobinPlayer.clips = soundClips;
        }
    }

    /// <summary>
    /// Play a sound (random or specific)
    /// </summary>
    public void PlaySound()
    {
        if (roundRobinPlayer == null) return;

      if (randomizeOnPlay)
        {
     roundRobinPlayer.PlayRandom();
   }
        else if (soundClips != null && soundClips.Length > 0)
        {
          roundRobinPlayer.PlayClip(soundClips[0]);
      }
    }

    /// <summary>
    /// Play specific clip by index
    /// </summary>
    public void PlaySound(int index)
    {
        if (soundClips == null || index < 0 || index >= soundClips.Length) return;
        roundRobinPlayer.PlayClip(soundClips[index]);
    }

    /// <summary>
    /// Play specific clip by name
    /// </summary>
    public void PlaySound(string clipName)
    {
        if (roundRobinPlayer != null)
  {
     roundRobinPlayer.Play(clipName);
        }
}

/// <summary>
    /// Stop all sounds
    /// </summary>
    public void StopAll()
    {
        if (roundRobinPlayer != null)
        {
roundRobinPlayer.StopAll();
     }
    }

    /// <summary>
    /// Add clip at runtime
    /// </summary>
    public void AddClip(AudioClip clip)
    {
        if (roundRobinPlayer != null)
  {
 roundRobinPlayer.AddClip(clip);
        }
    }

    /// <summary>
    /// Set playback volume
    /// </summary>
    public void SetVolume(float volume)
    {
        if (roundRobinPlayer != null)
        {
  roundRobinPlayer.SetVolume(volume);
        }
    }
}

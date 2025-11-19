using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Audio player that plays multiple audio clips in random order with playlist looping
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class RandomPlaylistAudioPlayer : MonoBehaviour
{
    [Header("Playlist Settings")]
    [SerializeField] private AudioClip[] playlist;
    [SerializeField] private bool playOnAwake = true;
    [SerializeField] private bool loopPlaylist = true;
    [SerializeField] private bool shuffleOnLoop = true; // Re-shuffle when playlist loops
    
 [Header("Playback Settings")]
    [SerializeField] private float delayBetweenTracks = 0f; // Delay in seconds between tracks
    [SerializeField] private bool avoidConsecutiveRepeats = true; // Avoid playing the same track twice in a row when looping
    
    [Header("Audio Mixer Integration")]
    [SerializeField] private bool useAudioManager = true;
    [SerializeField] private AudioManager.AudioCategory audioCategory = AudioManager.AudioCategory.Music;
    
    [Header("Debug")]
[SerializeField] private bool showDebugLogs = false;
    
    private AudioSource audioSource;
    private List<AudioClip> shuffledPlaylist;
    private int currentTrackIndex = -1;
    private float delayTimer = 0f;
    private bool isWaitingForNextTrack = false;
    private AudioClip lastPlayedClip = null;
    
    // Properties
    public bool IsPlaying => audioSource != null && audioSource.isPlaying;
    public bool IsPaused { get; private set; }
  public AudioClip CurrentClip => audioSource?.clip;
    public int CurrentTrackIndex => currentTrackIndex;
    public int PlaylistLength => playlist?.Length ?? 0;
  public float Progress => audioSource != null && audioSource.clip != null ? audioSource.time / audioSource.clip.length : 0f;
    
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false; // We handle playback manually
        
        // Setup audio mixer if AudioManager is available
        if (useAudioManager && AudioManager.Instance != null)
        {
          AudioManager.Instance.SetupAudioSource(audioSource, audioCategory);
        }
    }
    
    void Start()
    {
        if (playOnAwake && playlist != null && playlist.Length > 0)
        {
    Play();
        }
    }
    
    void Update()
    {
        if (isWaitingForNextTrack)
        {
            delayTimer += Time.deltaTime;
    if (delayTimer >= delayBetweenTracks)
{
                isWaitingForNextTrack = false;
    delayTimer = 0f;
    PlayNextTrack();
            }
        }
    else if (!audioSource.isPlaying && !IsPaused && currentTrackIndex >= 0)
{
            // Current track finished, move to next
       if (delayBetweenTracks > 0f)
            {
    isWaitingForNextTrack = true;
         delayTimer = 0f;
            }
  else
    {
  PlayNextTrack();
            }
      }
    }
    
    /// <summary>
    /// Start playing the playlist
    /// </summary>
    public void Play()
  {
        if (playlist == null || playlist.Length == 0)
      {
            LogWarning("Cannot play: Playlist is empty");
     return;
        }
        
        if (IsPaused)
        {
            // Resume from pause
audioSource.UnPause();
  IsPaused = false;
      Log("Resumed playback");
        }
        else
      {
          // Start from beginning
       ShufflePlaylist();
      currentTrackIndex = -1;
          PlayNextTrack();
        }
    }
    
    /// <summary>
    /// Pause playback
    /// </summary>
    public void Pause()
  {
      if (audioSource.isPlaying)
 {
       audioSource.Pause();
  IsPaused = true;
            Log("Paused playback");
        }
    }
    
    /// <summary>
    /// Stop playback
    /// </summary>
    public void Stop()
    {
        audioSource.Stop();
        IsPaused = false;
  currentTrackIndex = -1;
        isWaitingForNextTrack = false;
        delayTimer = 0f;
      Log("Stopped playback");
    }
    
    /// <summary>
    /// Skip to the next track
    /// </summary>
    public void SkipToNext()
    {
        if (playlist == null || playlist.Length == 0)
      {
            LogWarning("Cannot skip: Playlist is empty");
       return;
        }
        
        isWaitingForNextTrack = false;
        delayTimer = 0f;
        PlayNextTrack();
    }
    
  /// <summary>
    /// Restart the current track
    /// </summary>
    public void RestartCurrentTrack()
    {
        if (currentTrackIndex >= 0 && currentTrackIndex < shuffledPlaylist.Count)
        {
            PlayTrack(shuffledPlaylist[currentTrackIndex]);
        }
    }
    
    /// <summary>
    /// Set the playlist
    /// </summary>
    public void SetPlaylist(AudioClip[] newPlaylist)
    {
  if (newPlaylist == null || newPlaylist.Length == 0)
        {
 LogWarning("Cannot set empty playlist");
          return;
        }
        
   bool wasPlaying = IsPlaying;
        Stop();
        
        playlist = newPlaylist;
        
 if (wasPlaying)
        {
            Play();
    }
    }
    
    /// <summary>
    /// Add a clip to the playlist
    /// </summary>
    public void AddToPlaylist(AudioClip clip)
  {
        if (clip == null)
        {
     LogWarning("Cannot add null clip to playlist");
         return;
        }
        
        List<AudioClip> newPlaylist = new List<AudioClip>(playlist ?? new AudioClip[0]);
  newPlaylist.Add(clip);
        playlist = newPlaylist.ToArray();
    
        Log($"Added '{clip.name}' to playlist");
    }
    
    /// <summary>
    /// Remove a clip from the playlist
    /// </summary>
    public void RemoveFromPlaylist(AudioClip clip)
    {
        if (playlist == null || clip == null) return;
      
      List<AudioClip> newPlaylist = new List<AudioClip>(playlist);
        newPlaylist.Remove(clip);
    playlist = newPlaylist.ToArray();
        
 Log($"Removed '{clip.name}' from playlist");
    }
    
    /// <summary>
    /// Clear the playlist
  /// </summary>
    public void ClearPlaylist()
    {
      Stop();
      playlist = new AudioClip[0];
      shuffledPlaylist = new List<AudioClip>();
        Log("Cleared playlist");
    }
    
    /// <summary>
 /// Shuffle the playlist
    /// </summary>
    private void ShufflePlaylist()
    {
        if (playlist == null || playlist.Length == 0)
        {
 shuffledPlaylist = new List<AudioClip>();
       return;
        }
      
        // Create a new shuffled list using Fisher-Yates shuffle
        shuffledPlaylist = new List<AudioClip>(playlist);
        System.Random rng = new System.Random();
        
   int n = shuffledPlaylist.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            AudioClip temp = shuffledPlaylist[k];
            shuffledPlaylist[k] = shuffledPlaylist[n];
            shuffledPlaylist[n] = temp;
     }
        
        // Avoid repeating the last played clip if possible
        if (avoidConsecutiveRepeats && lastPlayedClip != null && shuffledPlaylist.Count > 1)
        {
  if (shuffledPlaylist[0] == lastPlayedClip)
  {
      // Swap with a random position that's not the first
       int swapIndex = rng.Next(1, shuffledPlaylist.Count);
    AudioClip temp = shuffledPlaylist[0];
           shuffledPlaylist[0] = shuffledPlaylist[swapIndex];
       shuffledPlaylist[swapIndex] = temp;
 }
        }
        
     Log("Shuffled playlist");
    }
    
    /// <summary>
    /// Play the next track in the shuffled playlist
    /// </summary>
    private void PlayNextTrack()
    {
        if (shuffledPlaylist == null || shuffledPlaylist.Count == 0)
        {
    LogWarning("Cannot play next track: Shuffled playlist is empty");
    return;
 }
        
        currentTrackIndex++;
        
        // Check if we've reached the end of the playlist
    if (currentTrackIndex >= shuffledPlaylist.Count)
   {
            if (loopPlaylist)
            {
         Log("Playlist finished, looping...");
 
  if (shuffleOnLoop)
        {
          ShufflePlaylist();
      }
       
       currentTrackIndex = 0;
            }
       else
   {
       Log("Playlist finished");
   Stop();
    return;
            }
        }
        
     AudioClip clipToPlay = shuffledPlaylist[currentTrackIndex];
   PlayTrack(clipToPlay);
    }
    
    /// <summary>
    /// Play a specific track
    /// </summary>
 private void PlayTrack(AudioClip clip)
    {
        if (clip == null)
        {
         LogWarning("Cannot play null clip");
            return;
        }
        
     audioSource.clip = clip;
        audioSource.Play();
        lastPlayedClip = clip;
   IsPaused = false;
   
        Log($"Now playing: '{clip.name}' ({currentTrackIndex + 1}/{shuffledPlaylist.Count})");
    }
    
    /// <summary>
    /// Set volume (0-1)
    /// </summary>
    public void SetVolume(float volume)
    {
     if (audioSource != null)
        {
        audioSource.volume = Mathf.Clamp01(volume);
        }
    }
    
    /// <summary>
    /// Get volume (0-1)
    /// </summary>
    public float GetVolume()
    {
        return audioSource != null ? audioSource.volume : 0f;
    }
    
    private void Log(string message)
    {
        if (showDebugLogs)
        {
   Debug.Log($"[RandomPlaylistAudioPlayer] {message}");
        }
    }
    
    private void LogWarning(string message)
    {
        if (showDebugLogs)
{
            Debug.LogWarning($"[RandomPlaylistAudioPlayer] {message}");
        }
    }
}

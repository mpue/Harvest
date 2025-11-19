using UnityEngine;

/// <summary>
/// Example script demonstrating how to use RandomPlaylistAudioPlayer
/// </summary>
public class RandomPlaylistAudioPlayerExample : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RandomPlaylistAudioPlayer audioPlayer;
    
    [Header("Example Audio Clips")]
    [SerializeField] private AudioClip[] exampleClips;
    
    void Start()
    {
        // Get or create the audio player
        if (audioPlayer == null)
        {
            audioPlayer = GetComponent<RandomPlaylistAudioPlayer>();
 }
        
        // Example: Set playlist at runtime
        if (exampleClips != null && exampleClips.Length > 0)
        {
            audioPlayer.SetPlaylist(exampleClips);
        }
  }
    
    void Update()
    {
        // Example keyboard controls for testing
    if (Input.GetKeyDown(KeyCode.Space))
        {
            // Toggle play/pause
  if (audioPlayer.IsPlaying)
         {
    audioPlayer.Pause();
}
         else
            {
   audioPlayer.Play();
       }
        }
        
     if (Input.GetKeyDown(KeyCode.N))
        {
// Skip to next track
        audioPlayer.SkipToNext();
        }
    
        if (Input.GetKeyDown(KeyCode.S))
        {
    // Stop playback
  audioPlayer.Stop();
}
        
 if (Input.GetKeyDown(KeyCode.R))
     {
            // Restart current track
    audioPlayer.RestartCurrentTrack();
     }
    }
    
    // Example methods that can be called from UI buttons
    public void OnPlayButtonClicked()
    {
        audioPlayer.Play();
    }
    
    public void OnPauseButtonClicked()
    {
        audioPlayer.Pause();
    }
    
    public void OnStopButtonClicked()
    {
        audioPlayer.Stop();
    }
 
    public void OnNextButtonClicked()
    {
    audioPlayer.SkipToNext();
    }
    
    public void OnVolumeChanged(float value)
    {
      audioPlayer.SetVolume(value);
    }
}

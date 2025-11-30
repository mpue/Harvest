using UnityEngine;

/// <summary>
/// Example setup script for Audio System in large battles
/// Attach this to a manager GameObject in your scene
/// </summary>
public class AudioSystemSetupExample : MonoBehaviour
{
[Header("Quick Setup")]
    [SerializeField] private bool autoSetup = true;

    [Header("AudioManager Settings")]
    [SerializeField] private bool enablePooling = true;
    [SerializeField] private int poolSize = 30;
    [SerializeField] private int maxPool = 100;

    [Header("Weapon Audio Settings")]
    [SerializeField] private int maxConcurrentWeaponSounds = 32;
    [SerializeField] private float weaponAudioDistance = 100f;

    void Start()
    {
        if (autoSetup)
        {
         SetupAudioSystem();
   }
    }

    /// <summary>
    /// Setup the audio system for optimal performance
    /// </summary>
    public void SetupAudioSystem()
    {
        // Ensure AudioManager exists
        if (AudioManager.Instance != null)
        {
            Debug.Log("? AudioManager initialized");
        }

        // Ensure WeaponAudioPlayer exists
        if (WeaponAudioPlayer.Instance != null)
        {
            WeaponAudioPlayer.Instance.SetMaxConcurrentSounds(maxConcurrentWeaponSounds);
   Debug.Log($"? WeaponAudioPlayer initialized with {maxConcurrentWeaponSounds} concurrent sounds");
        }

        Debug.Log("=== Audio System Setup Complete ===");
        Debug.Log($"Pooling: {enablePooling}");
        Debug.Log($"Pool Size: {poolSize} (max: {maxPool})");
        Debug.Log($"Max Weapon Sounds: {maxConcurrentWeaponSounds}");
        Debug.Log($"Weapon Audio Distance: {weaponAudioDistance}m");
 }

    /// <summary>
/// Example: Add RoundRobinPlayer to all weapons
  /// </summary>
    [ContextMenu("Add RoundRobinPlayer to All Weapons")]
  public void AddRoundRobinToWeapons()
    {
   Weapon[] weapons = FindObjectsOfType<Weapon>();
        int count = 0;

        foreach (Weapon weapon in weapons)
        {
   if (weapon.GetComponent<RoundRobinPlayer>() == null)
       {
          RoundRobinPlayer player = weapon.gameObject.AddComponent<RoundRobinPlayer>();
              // Configure player here if needed
    count++;
            }
  }

   Debug.Log($"Added RoundRobinPlayer to {count} weapons");
    }

    /// <summary>
    /// Example: Setup AudioManager on all AudioSources
    /// </summary>
    [ContextMenu("Setup AudioManager on All AudioSources")]
    public void SetupAllAudioSources()
    {
        if (AudioManager.Instance != null)
        {
  AudioManager.Instance.AssignMixerGroupsToAllAudioSources(AudioManager.AudioCategory.SFX);
            Debug.Log("AudioManager setup complete for all AudioSources");
        }
    }

    /// <summary>
    /// Test: Play test sound
    /// </summary>
[ContextMenu("Test: Play Random Sound")]
    public void TestPlaySound()
    {
    if (AudioManager.Instance != null)
        {
    Debug.Log("Testing audio system...");
     // You would need a test clip here
    // AudioManager.Instance.PlayOneShot(testClip, transform.position);
        }
    }

    void OnGUI()
    {
if (!Application.isPlaying) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("=== Audio System Stats ===");

   // Show active audio sources if available
   if (AudioManager.Instance != null)
   {
            GUILayout.Label("AudioManager: Active");
        }

        if (WeaponAudioPlayer.Instance != null)
        {
 GUILayout.Label("WeaponAudioPlayer: Active");
        }

    // Count RoundRobinPlayers
        RoundRobinPlayer[] players = FindObjectsOfType<RoundRobinPlayer>();
        GUILayout.Label($"RoundRobinPlayers: {players.Length}");

        int totalActiveSounds = 0;
        foreach (var player in players)
        {
            totalActiveSounds += player.GetActiveSoundCount();
        }
        GUILayout.Label($"Active Sounds: {totalActiveSounds}");

        GUILayout.EndArea();
    }
}

# Random Playlist Audio Player

Ein Audio-Player-Component für Unity, der mehrere AudioClips in zufälliger Reihenfolge mit Playlist-Loop abspielt.

## Features

- ? Spielt mehrere AudioClips in zufälliger Reihenfolge ab
- ? Playlist-Looping mit optionalem Re-Shuffle bei jedem Loop
- ? Vermeidung von aufeinanderfolgenden Wiederholungen beim Looping
- ? Konfigurierbare Verzögerung zwischen Tracks
- ? Integration mit dem bestehenden AudioManager
- ? Play, Pause, Stop, Skip und Restart Funktionen
- ? Playlist-Management zur Laufzeit (Add, Remove, Clear)
- ? Debug-Logging

## Setup

### 1. Grundlegende Einrichtung

1. Füge die `RandomPlaylistAudioPlayer` Komponente zu einem GameObject hinzu
2. Die Komponente fügt automatisch eine `AudioSource` Komponente hinzu
3. Füge deine AudioClips zum `Playlist` Array hinzu
4. Konfiguriere die Einstellungen nach Bedarf

### 2. Inspector-Einstellungen

#### Playlist Settings
- **Playlist**: Array von AudioClips, die abgespielt werden sollen
- **Play On Awake**: Startet die Wiedergabe automatisch beim Start
- **Loop Playlist**: Wenn aktiviert, wird die Playlist endlos wiederholt
- **Shuffle On Loop**: Wenn aktiviert, wird die Playlist bei jedem Loop neu gemischt

#### Playback Settings
- **Delay Between Tracks**: Verzögerung in Sekunden zwischen den Tracks (Standard: 0)
- **Avoid Consecutive Repeats**: Vermeidet, dass derselbe Track zweimal hintereinander beim Looping gespielt wird

#### Audio Mixer Integration
- **Use Audio Manager**: Nutzt den AudioManager für Mixer-Integration
- **Audio Category**: Kategorie für die AudioMixer-Gruppe (Music, SFX, etc.)

#### Debug
- **Show Debug Logs**: Aktiviert Debug-Ausgaben in der Console

## Verwendung

### Über den Inspector

```
1. Erstelle ein neues GameObject (z.B. "BackgroundMusicPlayer")
2. Füge die RandomPlaylistAudioPlayer Komponente hinzu
3. Ziehe deine AudioClips in das Playlist Array
4. Aktiviere "Play On Awake" für automatische Wiedergabe
5. Aktiviere "Loop Playlist" für endlose Wiedergabe
```

### Per Code

```csharp
using UnityEngine;

public class MusicController : MonoBehaviour
{
    [SerializeField] private RandomPlaylistAudioPlayer musicPlayer;
  [SerializeField] private AudioClip[] musicTracks;
    
    void Start()
    {
    // Playlist setzen
        musicPlayer.SetPlaylist(musicTracks);
        
      // Wiedergabe starten
        musicPlayer.Play();
    }
    
    public void ToggleMusic()
    {
        if (musicPlayer.IsPlaying)
        {
            musicPlayer.Pause();
   }
   else
        {
    musicPlayer.Play();
        }
    }
    
    public void NextTrack()
    {
      musicPlayer.SkipToNext();
    }
    
    public void SetMusicVolume(float volume)
    {
        musicPlayer.SetVolume(volume);
    }
}
```

### UI-Integration

```csharp
// Im Inspector: Verknüpfe diese Methoden mit UI Buttons

public void OnPlayButton()
{
    musicPlayer.Play();
}

public void OnPauseButton()
{
    musicPlayer.Pause();
}

public void OnStopButton()
{
    musicPlayer.Stop();
}

public void OnNextButton()
{
    musicPlayer.SkipToNext();
}

public void OnVolumeSlider(float value)
{
    musicPlayer.SetVolume(value);
}
```

## API-Referenz

### Properties

```csharp
bool IsPlaying { get; }           // Gibt zurück, ob der Player gerade abspielt
bool IsPaused { get; }  // Gibt zurück, ob der Player pausiert ist
AudioClip CurrentClip { get; }     // Der aktuell spielende AudioClip
int CurrentTrackIndex { get; }     // Index des aktuellen Tracks in der Playlist
int PlaylistLength { get; }     // Anzahl der Tracks in der Playlist
float Progress { get; }       // Fortschritt des aktuellen Tracks (0-1)
```

### Methoden

```csharp
// Wiedergabesteuerung
void Play()    // Startet oder setzt die Wiedergabe fort
void Pause()              // Pausiert die Wiedergabe
void Stop()      // Stoppt die Wiedergabe
void SkipToNext()    // Springt zum nächsten Track
void RestartCurrentTrack()         // Startet den aktuellen Track neu

// Playlist-Management
void SetPlaylist(AudioClip[] newPlaylist)  // Setzt eine neue Playlist
void AddToPlaylist(AudioClip clip)    // Fügt einen Clip zur Playlist hinzu
void RemoveFromPlaylist(AudioClip clip)    // Entfernt einen Clip aus der Playlist
void ClearPlaylist()     // Leert die Playlist

// Lautstärkeregelung
void SetVolume(float volume)       // Setzt die Lautstärke (0-1)
float GetVolume()         // Gibt die aktuelle Lautstärke zurück
```

## Beispiele

### Beispiel 1: Hintergrundmusik

```csharp
public class BackgroundMusicManager : MonoBehaviour
{
    private RandomPlaylistAudioPlayer musicPlayer;
    
    void Awake()
    {
 musicPlayer = GetComponent<RandomPlaylistAudioPlayer>();
    }
    
    void Start()
    {
        // Musik automatisch starten (wenn Play On Awake nicht aktiviert)
     musicPlayer.Play();
    }
}
```

### Beispiel 2: Dynamische Playlist

```csharp
public class DynamicMusicController : MonoBehaviour
{
    [SerializeField] private RandomPlaylistAudioPlayer musicPlayer;
  [SerializeField] private AudioClip[] calmMusic;
    [SerializeField] private AudioClip[] actionMusic;
    
    public void SwitchToCalmMusic()
    {
        musicPlayer.SetPlaylist(calmMusic);
    }
    
    public void SwitchToActionMusic()
    {
        musicPlayer.SetPlaylist(actionMusic);
    }
}
```

### Beispiel 3: UI-Musik-Player

```csharp
public class UIMusicPlayer : MonoBehaviour
{
    [SerializeField] private RandomPlaylistAudioPlayer musicPlayer;
    [SerializeField] private Text trackNameText;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider progressSlider;
    
    void Update()
    {
   // Track-Namen anzeigen
   if (musicPlayer.CurrentClip != null)
        {
    trackNameText.text = musicPlayer.CurrentClip.name;
     }
        
        // Fortschritt anzeigen
        progressSlider.value = musicPlayer.Progress;
    }
    
    public void OnVolumeChanged()
    {
        musicPlayer.SetVolume(volumeSlider.value);
    }
}
```

### Beispiel 4: Tastatursteuerung (für Testing)

Siehe `RandomPlaylistAudioPlayerExample.cs`:
- **Leertaste**: Play/Pause Toggle
- **N**: Nächster Track
- **S**: Stop
- **R**: Aktuellen Track neu starten

## Tipps

1. **Performance**: Der Player verwendet Unity's Standard-AudioSource und ist sehr performant
2. **Speicher**: Alle AudioClips werden im Speicher gehalten - verwende AudioClip Import Settings entsprechend
3. **Audio Manager**: Aktiviere "Use Audio Manager" für bessere Audio-Mixer-Integration
4. **Shuffle-Algorithmus**: Verwendet Fisher-Yates Shuffle für echte Randomisierung
5. **Loop-Vermeidung**: "Avoid Consecutive Repeats" verhindert, dass der letzte Track der vorherigen Loop-Iteration als erster in der nächsten gespielt wird

## Troubleshooting

**Problem**: Keine Audiowiedergabe
- Stelle sicher, dass AudioClips im Playlist Array zugewiesen sind
- Überprüfe, ob die AudioSource aktiviert ist
- Prüfe die Lautstärke-Einstellungen

**Problem**: Clips werden nicht gemischt
- Stelle sicher, dass mehr als 1 Clip in der Playlist ist
- Prüfe, ob "Shuffle On Loop" aktiviert ist

**Problem**: Audio ist zu leise
- Prüfe die AudioSource-Lautstärke
- Prüfe die AudioMixer-Einstellungen im AudioManager
- Verwende SetVolume() oder den Volume-Slider

## Integration mit bestehendem AudioManager

Der Player integriert sich automatisch mit dem bestehenden AudioManager:

```csharp
// Automatische Integration
[SerializeField] private bool useAudioManager = true;
[SerializeField] private AudioManager.AudioCategory audioCategory = AudioManager.AudioCategory.Music;
```

Dies stellt sicher, dass:
- Die richtige AudioMixerGroup zugewiesen wird
- Volume-Kontrollen über den AudioManager funktionieren
- Konsistenz mit anderen Audio im Spiel gewährleistet ist

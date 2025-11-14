# Audio Mixer Management System

Ein umfassendes System zur Verwaltung von AudioMixerGroups in deinem Unity RTS-Projekt.

## Übersicht

Das Audio Mixer Management System bietet:
- **AudioManager** - Zentraler Manager für alle Audio-Einstellungen
- **AudioMixerAssignmentWindow** - Editor-Tool zur Zuweisung von MixerGroups
- **Automatische Integration** - Alle AudioSources werden automatisch mit MixerGroups verknüpft
- **Unit Editor Integration** - Audio-Settings direkt im Unit Editor

## Komponenten

### 1. AudioManager (Runtime)

Zentraler Singleton-Manager für Audio-Verwaltung.

#### Setup

1. **Automatisch**: Wird beim ersten Zugriff erstellt
2. **Manuell**: `GameObject > Create Empty` ? `Add Component > AudioManager`
3. **Via Editor**: `Tools > Audio Mixer Assignment > Create AudioManager`

#### Inspector-Einstellungen

```
Audio Mixer
  ?? Main Mixer        - Haupt-AudioMixer Asset
  
Mixer Groups
  ?? Master Group        - Master-Kanal
  ?? Music Group         - Musik
  ?? SFX Group           - Sound Effects (Standard)
  ?? UI Group            - UI-Sounds
  ?? Unit Sounds Group   - Unit-Sounds (Auswahl, Bewegung)
  ?? Weapon Sounds Group - Waffen-Sounds
  ?? Ambient Group  - Umgebungssounds

Default Settings
  ?? Default Mixer Group              - Standard-Gruppe (normalerweise SFX)
  ?? Assign On AudioSource Creation   - Auto-Zuweisung bei Erstellung

Volume Settings
  ?? Master Volume (0-1)
  ?? Music Volume (0-1)
  ?? SFX Volume (0-1)
  ?? UI Volume (0-1)
```

#### AudioMixer Setup

**Erstelle einen AudioMixer:**

1. **Project-Fenster**: Rechtsklick ? `Create > Audio Mixer`
2. Name: "MainAudioMixer"
3. **Mixer-Fenster öffnen**: `Window > Audio > Audio Mixer`

**Erstelle Mixer-Gruppen:**

```
Master
??? Music
??? SFX
?   ??? UnitSounds
?   ??? WeaponSounds
??? UI
??? Ambient
```

**Exposed Parameters erstellen:**

1. Im Mixer-Fenster, klicke auf Gruppe (z.B. "Master")
2. Im Inspector: `Volume` ? Rechtsklick ? `Expose 'Volume' to script`
3. Im Mixer-Fenster: `Exposed Parameters` (oben rechts)
4. Umbenennen zu: "MasterVolume", "MusicVolume", "SFXVolume", "UIVolume"

**AudioManager zuweisen:**

1. AudioManager GameObject auswählen
2. Inspector:
   - `Main Mixer` ? MainAudioMixer Asset zuweisen
   - Alle Mixer Groups zuweisen (aus dem Mixer ziehen)

### 2. AudioMixerAssignmentWindow (Editor)

Editor-Fenster zur Verwaltung von MixerGroup-Zuweisungen.

#### Öffnen

**Menü**: `Tools > Audio Mixer Assignment`

#### Features

##### Quick Actions

- **Refresh AudioSources List** - Aktualisiert Liste aller AudioSources
- **Status-Anzeige** - Zeigt Anzahl der AudioSources mit/ohne MixerGroup

##### Assignment Methods

**Method 1: AudioManager Categories**

Verwendet vordefinierte Kategorien aus dem AudioManager:

```csharp
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
```

**Buttons:**
- `Assign to Sources Without Group` - Nur nicht-zugewiesene AudioSources
- `Assign to ALL Sources` - Überschreibt alle bestehenden Zuweisungen

**Method 2: Direct MixerGroup Assignment**

Direkte Zuweisung eines spezifischen MixerGroup Assets:

1. MixerGroup aus Project-Fenster ziehen
2. Zuweisungs-Button klicken

##### AudioSources Overview

**AudioSources Without MixerGroup**
- Liste aller AudioSources ohne Zuweisung
- `Select` - Wählt GameObject in Hierarchy
- `Assign` - Weist ausgewähltes MixerGroup zu

**All AudioSources**
- Vollständige Liste aller AudioSources in der Scene
- Zeigt aktuelles MixerGroup
- `Select` - Navigation zu GameObject

### 3. Unit Editor Integration

Der Unit Editor wurde um Audio-Management erweitert.

#### Audio Settings Sektion

**AudioManager Status**
- Warnung wenn kein AudioManager vorhanden
- `Create AudioManager` Button zur schnellen Erstellung

**AudioSources on Unit**
- Liste aller AudioSources auf der Unit
- Zeigt aktuelles MixerGroup
- `Assign UnitSounds` Button - Schnelle Zuweisung

**Unit Sound Settings**
- Hit Sounds (Array)
- Death Sounds (Array)
- Audio Volume

## Verwendung

### Workflow: Neues Projekt Setup

1. **AudioMixer erstellen**
   ```
   Project > Create > Audio Mixer
   Name: "MainAudioMixer"
   ```

2. **Mixer-Gruppen erstellen**
   - Öffne Audio Mixer Window
   - Erstelle Hierarchie (siehe oben)
   - Exposed Parameters erstellen

3. **AudioManager erstellen**
   ```
   Tools > Audio Mixer Assignment
   Button: "Create AudioManager"
   ```

4. **AudioMixer zuweisen**
   - AudioManager GameObject auswählen
- Main Mixer und alle Groups zuweisen

5. **Bestehende AudioSources zuweisen**
   ```
   Tools > Audio Mixer Assignment
   Method 1: Category > SFX
   Button: "Assign to Sources Without Group"
   ```

### Workflow: Neue Unit erstellen

**Option 1: Automatisch (Empfohlen)**

Units die über den AudioManager erstellt werden, erhalten automatisch MixerGroups:

```csharp
// Health, UnitSelector, etc. verwenden AudioManager automatisch
// Keine manuelle Zuweisung nötig!
```

**Option 2: Unit Editor**

1. Unit im Unit Editor öffnen
2. `Audio Settings` Sektion
3. `Assign UnitSounds` für jede AudioSource klicken

**Option 3: AudioMixerAssignmentWindow**

1. `Tools > Audio Mixer Assignment`
2. Unit mit AudioSources in Scene platzieren
3. `Refresh AudioSources List`
4. Category auswählen (z.B. UnitSounds)
5. `Assign to Sources Without Group`

### Code-Integration

#### AudioSource mit AudioManager erstellen

```csharp
// Statt:
audioSource = gameObject.AddComponent<AudioSource>();

// Verwende:
if (AudioManager.Instance != null)
{
    audioSource = AudioManager.Instance.CreateAudioSource(
        gameObject, 
        AudioManager.AudioCategory.UnitSounds, 
   false // playOnAwake
    );
}
else
{
    audioSource = gameObject.AddComponent<AudioSource>();
}
```

#### Bestehende AudioSource setup

```csharp
audioSource = GetComponent<AudioSource>();
if (audioSource == null)
{
 audioSource = gameObject.AddComponent<AudioSource>();
}

// MixerGroup zuweisen
if (AudioManager.Instance != null)
{
    AudioManager.Instance.SetupAudioSource(
        audioSource, 
   AudioManager.AudioCategory.WeaponSounds
    );
}
```

#### One-Shot Sounds mit MixerGroup

```csharp
// 3D Sound
AudioManager.Instance.PlayOneShot(
clip, 
    position, 
    AudioManager.AudioCategory.SFX, 
    volume: 1f
);

// 2D Sound (UI)
AudioManager.Instance.PlayOneShot2D(
    clip, 
    AudioManager.AudioCategory.UI, 
    volume: 0.8f
);
```

### Kategorie-Zuordnung

Empfohlene Zuordnung der Audio-Kategorien:

| Komponente | Kategorie | Verwendung |
|-----------|-----------|------------|
| Health | UnitSounds | Hit/Death Sounds |
| UnitSelector | UnitSounds | Selection/Move Sounds |
| Weapon | WeaponSounds | Fire/Reload Sounds |
| Projectile | WeaponSounds | Impact Sounds |
| UI Buttons | UI | Click/Hover Sounds |
| Music Player | Music | Background Music |
| Environment | Ambient | Wind, Water, etc. |

## API-Referenz

### AudioManager

#### Properties
```csharp
AudioManager.Instance  // Singleton-Instanz
```

#### Methods

**GetMixerGroup**
```csharp
AudioMixerGroup GetMixerGroup(AudioCategory category)
```
Gibt MixerGroup für Kategorie zurück.

**SetupAudioSource**
```csharp
void SetupAudioSource(AudioSource audioSource, AudioCategory category = AudioCategory.SFX)
```
Weist AudioSource eine MixerGroup zu.

**CreateAudioSource**
```csharp
AudioSource CreateAudioSource(GameObject target, AudioCategory category = AudioCategory.SFX, bool playOnAwake = false)
```
Erstellt und konfiguriert neue AudioSource.

**AssignMixerGroupsToAllAudioSources**
```csharp
void AssignMixerGroupsToAllAudioSources(AudioCategory category = AudioCategory.SFX)
```
Weist allen AudioSources ohne MixerGroup eine Kategorie zu.

**PlayOneShot**
```csharp
void PlayOneShot(AudioClip clip, Vector3 position, AudioCategory category = AudioCategory.SFX, float volume = 1f)
```
Spielt 3D One-Shot Sound mit automatischer MixerGroup.

**PlayOneShot2D**
```csharp
void PlayOneShot2D(AudioClip clip, AudioCategory category = AudioCategory.UI, float volume = 1f)
```
Spielt 2D One-Shot Sound mit automatischer MixerGroup.

**Volume Control**
```csharp
void SetMasterVolume(float volume)  // 0-1
void SetMusicVolume(float volume)
void SetSFXVolume(float volume)
void SetUIVolume(float volume)
```

## Integrierte Komponenten

Das System ist bereits in folgende Komponenten integriert:

? **UnitSelector** - Selection/Move Sounds ? UnitSounds
? **Health** - Hit/Death Sounds ? UnitSounds
? **Projectile** - Impact Sounds ? WeaponSounds
? **RoundRobinPlayer** - Bereits MixerGroup-Support

## Troubleshooting

### AudioSources haben keine MixerGroup

**Problem**: AudioSources spielen Sound, aber ohne Mixer-Effekte

**Lösung**:
1. `Tools > Audio Mixer Assignment`
2. `Refresh AudioSources List`
3. Prüfe "AudioSources Without MixerGroup" Liste
4. Wähle Kategorie und klicke `Assign to Sources Without Group`

### AudioManager nicht gefunden

**Problem**: `AudioManager.Instance == null`

**Lösung**:
1. `Tools > Audio Mixer Assignment`
2. Klicke `Create AudioManager` (oder Warnung im Unit Editor)
3. AudioMixer und Groups zuweisen

### Sounds zu leise/laut trotz Volume-Einstellungen

**Problem**: Volume-Änderungen haben keinen Effekt

**Lösung**:
1. Prüfe ob AudioMixer im AudioManager zugewiesen ist
2. Prüfe Exposed Parameters im Mixer ("MasterVolume", etc.)
3. Prüfe Mixer-Gruppen-Hierarchie

### MixerGroup verschwindet nach Play-Mode

**Problem**: Zuweisungen gehen verloren

**Lösung**:
- Änderungen nur im Edit-Mode machen
- Prefab-Changes über Prefab-Editing
- `EditorUtility.SetDirty()` wird automatisch aufgerufen

### One-Shot Sounds haben keine MixerGroup

**Problem**: `AudioSource.PlayClipAtPoint` ignoriert MixerGroups

**Lösung**:
```csharp
// Statt:
AudioSource.PlayClipAtPoint(clip, position);

// Verwende:
AudioManager.Instance.PlayOneShot(clip, position, AudioManager.AudioCategory.SFX);
```

## Best Practices

### 1. AudioManager Always First

Erstelle AudioManager **bevor** du Units erstellst:
- Neue AudioSources erhalten automatisch MixerGroups
- Keine manuelle Nacharbeit nötig

### 2. Konsistente Kategorien

Verwende dieselbe Kategorie für ähnliche Sounds:
- Alle Unit-Sounds ? `UnitSounds`
- Alle Waffen ? `WeaponSounds`
- Alle UI ? `UI`

### 3. Scene Setup Checklist

Bei neuer Scene:
```
? AudioManager vorhanden?
? AudioMixer zugewiesen?
? MixerGroups konfiguriert?
? Exposed Parameters erstellt?
? Bestehende AudioSources zugewiesen?
```

### 4. Prefab Updates

Bei Prefab-Änderungen:
1. Prefab im Prefab-Mode öffnen
2. AudioSource-Änderungen vornehmen
3. AudioMixerAssignmentWindow nutzen oder
4. Unit Editor Audio-Settings verwenden

### 5. Testing

Test-Checkliste:
```
? Sounds spielen ab?
? Volume-Slider funktionieren?
? Mixer-Effekte (EQ, Reverb) hörbar?
? Kategorien korrekt zugeordnet?
```

## Performance

### Optimierungen

- AudioManager ist Singleton ? Kein Overhead
- MixerGroup-Zuweisungen zur Edit-Time ? Keine Runtime-Kosten
- One-Shot Helper erstellen temporäre GameObjects (automatisch zerstört)

### Memory Management

- Temporäre AudioSources werden automatisch aufgeräumt
- Keine Memory-Leaks durch MixerGroup-Referenzen
- Prefab-Instanzen teilen MixerGroup-Referenzen

## Migration Guide

### Von bestehendem Projekt

1. **Backup erstellen**
2. **AudioManager-System importieren**
   - AudioManager.cs
   - AudioMixerAssignmentWindow.cs
3. **AudioMixer erstellen und konfigurieren**
4. **AudioManager in Scene platzieren**
5. **Bestehende Scripts aktualisieren**
   ```
   Tools > Audio Mixer Assignment
   Assign to ALL Sources (Kategorie wählen)
   ```
6. **Testen**

### Code-Updates

Ersetze überall wo AudioSources erstellt werden:

**Alt:**
```csharp
audioSource = gameObject.AddComponent<AudioSource>();
audioSource.playOnAwake = false;
audioSource.volume = 1f;
```

**Neu:**
```csharp
if (AudioManager.Instance != null)
{
    audioSource = AudioManager.Instance.CreateAudioSource(gameObject, AudioManager.AudioCategory.SFX);
}
else
{
    audioSource = gameObject.AddComponent<AudioSource>();
}
audioSource.volume = 1f;
```

## Erweiterungen

### Eigene Kategorien hinzufügen

1. **AudioManager.cs erweitern:**
```csharp
public enum AudioCategory
{
    // ...existing...
    MyCustomCategory  // Hinzufügen
}
```

2. **MixerGroup im AudioManager:**
```csharp
[SerializeField] private AudioMixerGroup myCustomGroup;
```

3. **GetMixerGroup erweitern:**
```csharp
case AudioCategory.MyCustomCategory:
    return myCustomGroup;
```

### Runtime Volume Control UI

```csharp
using UnityEngine.UI;

public class VolumeControl : MonoBehaviour
{
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    void Start()
    {
        masterSlider.onValueChanged.AddListener(AudioManager.Instance.SetMasterVolume);
        musicSlider.onValueChanged.AddListener(AudioManager.Instance.SetMusicVolume);
    sfxSlider.onValueChanged.AddListener(AudioManager.Instance.SetSFXVolume);
    }
}
```

## Siehe auch

- `UnitEditor_README.md` - Unit Editor Dokumentation
- `WeaponSystem_README.md` - Waffen-System
- `CombatSystem_README.md` - Kampf-System
- Unity Audio Mixer Documentation

## Support

Bei Fragen oder Problemen:
1. Console auf Errors prüfen
2. AudioManager Inspector prüfen (alle Groups zugewiesen?)
3. AudioMixerAssignmentWindow zur Diagnose nutzen
4. Troubleshooting-Sektion in dieser Dokumentation

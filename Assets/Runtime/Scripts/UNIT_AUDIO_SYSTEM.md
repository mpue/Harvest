# Unit Audio System

## Übersicht

Das Unit Audio System ermöglicht es, sowohl globale als auch unit-spezifische Sounds für Selektion und Bewegungsbefehle zu verwenden. Der `UnitSelector` prüft automatisch, ob eine Unit eigene Sounds zur Verfügung stellt, bevor die Standard-Sounds verwendet werden.

## Features

- ? **Unit-spezifische Sounds**: Jede Unit kann eigene Selection/Move Sounds haben
- ? **Fallback auf Standard-Sounds**: Automatische Verwendung von Default-Sounds wenn keine unit-spezifischen vorhanden sind
- ? **Zufallswiedergabe**: Sounds werden zufällig aus dem Array ausgewählt
- ? **Intelligente Priorisierung**: Unit-Sounds haben Vorrang vor Selector-Sounds
- ? **Multi-Selection Support**: Bei Box-Selection wird Sound der ersten passenden Unit verwendet

## Hierarchie

```
UnitSelector (Globale Default-Sounds)
    ? prüft
BaseUnit (Unit-spezifische Sounds)
    ? wenn vorhanden
Spielt Unit-Sound
    ? sonst
Spielt Default-Sound
```

## Setup

### Schritt 1: UnitSelector (Globale Defaults)

Im Inspector des `UnitSelector` GameObject:

```
Audio Feedback:
  Unit Select Sounds:
    - Element 0: DefaultSelectSound1.wav
    - Element 1: DefaultSelectSound2.wav
  - Element 2: DefaultSelectSound3.wav
  
  Unit Move Sounds:
    - Element 0: DefaultMoveSound1.wav
    - Element 1: DefaultMoveSound2.wav
    
  Audio Volume: 1.0
  Audio Category: UnitSounds
```

**Diese Sounds werden verwendet wenn:**
- Unit keine eigenen Sounds hat
- Unit-Sound Array leer ist
- Als Fallback für alle Units

### Schritt 2: BaseUnit (Unit-spezifische Sounds)

Im Inspector der jeweiligen Unit (z.B. Soldier, Tank):

```
Audio:
  Unit Select Sounds:
    - Element 0: SoldierSelect1.wav
    - Element 1: SoldierSelect2.wav
  - Element 2: SoldierSelect3.wav
  
  Unit Move Sounds:
    - Element 0: SoldierMove1.wav
    - Element 1: SoldierMove2.wav
```

**Diese Sounds haben Vorrang:**
- Werden bevorzugt verwendet wenn vorhanden
- Überschreiben die Default-Sounds des UnitSelector
- Können leer gelassen werden für Fallback

## Verwendung

### Automatisch (Kein Code nötig)

Das System funktioniert vollständig automatisch:

```csharp
// Beim Klick auf Unit:
// 1. UnitSelector prüft unit.HasCustomSelectSounds
// 2. Wenn true ? unit.UnitSelectSounds wird verwendet
// 3. Wenn false ? unitSelector.unitSelectSounds wird verwendet

// Beim Move-Befehl:
// 1. UnitSelector prüft alle selektierten Units
// 2. Sucht erste Unit mit HasCustomMoveSounds
// 3. Verwendet deren Sounds, sonst Default
```

### Programmatisch

```csharp
// Prüfen ob Unit eigene Sounds hat:
BaseUnit unit = GetComponent<BaseUnit>();

if (unit.HasCustomSelectSounds)
{
    // Unit hat eigene Selection Sounds
    AudioClip[] sounds = unit.UnitSelectSounds;
}

if (unit.HasCustomMoveSounds)
{
    // Unit hat eigene Move Sounds
    AudioClip[] sounds = unit.UnitMoveSounds;
}

// Manuell Sound abspielen:
UnitSelector selector = FindObjectOfType<UnitSelector>();
// Verwendet automatisch unit-spezifischen Sound falls vorhanden
```

## Beispiele

### Beispiel 1: Soldier mit eigenen Sounds

**Setup:**
```
SoldierUnit (BaseUnit)
  Audio:
    Unit Select Sounds:
      - SoldierYessir1.wav
      - SoldierYessir2.wav
- SoldierReadyForOrders.wav
    Unit Move Sounds:
      - SoldierMovingOut.wav
      - SoldierOnMyWay.wav
      - SoldierAffirmative.wav
```

**Verhalten:**
- Klick auf Soldier ? Spielt zufällig einen der 3 Selection Sounds
- Move-Befehl ? Spielt zufällig einen der 3 Move Sounds
- Überschreibt UnitSelector Defaults komplett

### Beispiel 2: Tank ohne eigene Sounds

**Setup:**
```
TankUnit (BaseUnit)
  Audio:
    Unit Select Sounds: (leer)
    Unit Move Sounds: (leer)
```

**Verhalten:**
- Klick auf Tank ? Spielt UnitSelector.unitSelectSounds
- Move-Befehl ? Spielt UnitSelector.unitMoveSounds
- Verwendet Defaults weil keine unit-spezifischen Sounds

### Beispiel 3: Mixed Selection

**Setup:**
```
Selektion enthält:
- 2x Soldier (mit eigenen Sounds)
- 1x Tank (ohne eigene Sounds)
- 1x Harvester (mit eigenen Sounds)
```

**Verhalten:**
- Box-Selection ? Spielt Soldier Selection Sound (erste Unit mit Custom Sounds)
- Move-Befehl ? Spielt Soldier Move Sound (erste Unit mit Custom Move Sounds)

### Beispiel 4: Nur Defaults

**Setup:**
```
UnitSelector:
  Unit Select Sounds:
    - GenericSelect1.wav
    - GenericSelect2.wav
  Unit Move Sounds:
    - GenericMove1.wav
    - GenericMove2.wav

Alle Units: Keine eigenen Sounds
```

**Verhalten:**
- Jede Unit verwendet die Generic Sounds
- Einheitliches Sound-Erlebnis
- Einfach zu pflegen

## Priorisierung

### Selection Sounds

```
1. Unit.UnitSelectSounds (wenn HasCustomSelectSounds)
   ? falls nicht vorhanden
2. UnitSelector.unitSelectSounds (Default)
   ? falls auch nicht vorhanden
3. Kein Sound
```

### Move Sounds

```
1. Erste selektierte Unit.UnitMoveSounds (wenn HasCustomMoveSounds)
   ? falls keine Unit mit Custom Sounds
2. UnitSelector.unitMoveSounds (Default)
   ? falls auch nicht vorhanden
3. Kein Sound
```

## Code-Implementierung

### BaseUnit.cs - Properties

```csharp
[Header("Audio")]
[SerializeField] private AudioClip[] unitSelectSounds;
[SerializeField] private AudioClip[] unitMoveSounds;

// Properties
public AudioClip[] UnitSelectSounds => unitSelectSounds;
public AudioClip[] UnitMoveSounds => unitMoveSounds;
public bool HasCustomSelectSounds => unitSelectSounds != null && unitSelectSounds.Length > 0;
public bool HasCustomMoveSounds => unitMoveSounds != null && unitMoveSounds.Length > 0;
```

### UnitSelector.cs - Sound Methoden

```csharp
/// <summary>
/// Plays selection sound - prefers unit-specific sound over default
/// </summary>
private void PlaySelectionSound(BaseUnit unit)
{
    if (audioSource == null) return;
  
    // Check if unit has custom selection sounds
    if (unit != null && unit.HasCustomSelectSounds)
    {
 PlayRandomSound(unit.UnitSelectSounds);
    }
    else
    {
      // Fall back to default selector sounds
        PlayRandomSound(unitSelectSounds);
    }
}

/// <summary>
/// Plays move command sound - prefers unit-specific sound over default
/// </summary>
private void PlayMoveSound()
{
    if (audioSource == null || selectedUnits.Count == 0) return;
    
    // Try to find first selected unit with custom move sounds
    BaseUnit unitWithCustomSound = null;
    foreach (BaseUnit unit in selectedUnits)
    {
        if (unit != null && !unit.IsBuilding && unit.HasCustomMoveSounds)
   {
        unitWithCustomSound = unit;
            break;
     }
    }
    
    // Play unit-specific sound if available, otherwise use default
    if (unitWithCustomSound != null)
    {
 PlayRandomSound(unitWithCustomSound.UnitMoveSounds);
    }
    else
    {
        PlayRandomSound(unitMoveSounds);
    }
}
```

## Best Practices

### 1. Verwende aussagekräftige Sound-Namen

```
? Gut:
- SoldierSelect1_Yessir.wav
- SoldierMove1_MovingOut.wav
- TankSelect1_Ready.wav

? Schlecht:
- sound1.wav
- audio_clip.wav
- new_sound_2.wav
```

### 2. Konsistente Anzahl von Sounds

```
Empfohlen pro Unit:
- 3-5 Selection Sounds (für Variation)
- 3-5 Move Sounds (für Variation)

Zu wenig (1): Wird repetitiv
Zu viel (20+): Schwer zu verwalten
```

### 3. Defaults als Fallback

```csharp
// Immer Defaults im UnitSelector setzen:
UnitSelector:
  Unit Select Sounds: [Generic1, Generic2, Generic3]
  Unit Move Sounds: [Generic1, Generic2, Generic3]

// Units können dann optional überschreiben:
SpecialUnit:
  Unit Select Sounds: [Special1, Special2]
  Unit Move Sounds: [] // Verwendet Default
```

### 4. Sound-Kategorisierung

```
Militärische Units:
- Soldier: Stimme (Yes Sir!, Moving Out!)
- Tank: Motor + Stimme (Engine starting, Moving!)

Zivile Units:
- Harvester: Beeps/Mechanical Sounds
- Builder: Tool Sounds

Alle:
- Kurze Sounds (0.5-2 Sekunden)
- Klare Bestätigung
```

## Debugging

### Console Logs (optional hinzufügen)

```csharp
private void PlaySelectionSound(BaseUnit unit)
{
    if (audioSource == null) return;
    
    if (unit != null && unit.HasCustomSelectSounds)
    {
  Debug.Log($"Playing custom selection sound for {unit.UnitName}");
        PlayRandomSound(unit.UnitSelectSounds);
    }
    else
    {
        Debug.Log($"Playing default selection sound");
        PlayRandomSound(unitSelectSounds);
    }
}
```

### Prüf-Checkliste

- [ ] UnitSelector hat Default Sounds zugewiesen
- [ ] AudioSource auf UnitSelector vorhanden
- [ ] Audio Volume > 0
- [ ] Unit hat AudioClip[] im Inspector (optional)
- [ ] AudioClips sind importiert und zugewiesen
- [ ] AudioManager in Szene (optional, für Mixer Groups)

## Häufige Probleme

### Problem: Kein Sound beim Klick

**Diagnose:**
```
1. Prüfe UnitSelector.audioSource != null
2. Prüfe UnitSelector.audioVolume > 0
3. Prüfe unitSelectSounds Array nicht leer
4. Prüfe AudioClips zugewiesen
5. Prüfe Audio Listener in Szene
```

**Lösung:**
- Füge AudioSource zu UnitSelector hinzu
- Setze Volume auf 1.0
- Weise mindestens 1 AudioClip zu

### Problem: Unit-Sound wird nicht gespielt

**Diagnose:**
```csharp
BaseUnit unit = GetComponent<BaseUnit>();
Debug.Log($"Has Custom Select Sounds: {unit.HasCustomSelectSounds}");
Debug.Log($"Select Sounds Count: {unit.UnitSelectSounds?.Length ?? 0}");
```

**Lösung:**
- Prüfe ob AudioClips im BaseUnit Inspector zugewiesen sind
- Array muss mindestens 1 Element haben
- Element darf nicht null sein

### Problem: Immer gleicher Sound

**Ursache:**
- Nur 1 Sound im Array
- Random.Range immer gleicher Index

**Lösung:**
- Füge mehrere Sounds hinzu (3-5 empfohlen)
- Prüfe Array-Größe

### Problem: Sound zu laut/leise

**Lösung:**
```
UnitSelector:
  Audio Volume: 0.7 (reduzieren)

Oder AudioClip Import Settings:
  - Wähle AudioClip in Project
  - Inspector > Load Type: Compressed In Memory
  - Force To Mono: ? (optional)
  - Normalize: ? (für konsistente Lautstärke)
```

## Integration mit AudioManager

Wenn `AudioManager` vorhanden:

```csharp
void Start()
{
  // UnitSelector Setup:
    audioSource = AudioManager.Instance.CreateAudioSource(
        gameObject, 
        AudioManager.AudioCategory.UnitSounds, 
      false
    );
    
    audioSource.volume = audioVolume;
}
```

**Vorteile:**
- Automatisches Mixer Group Assignment
- Zentrale Lautstärke-Kontrolle
- Kategorie-basierte Organisation

## Erweiterte Anwendungsfälle

### Custom Sound für spezielle Aktionen

```csharp
public class CustomUnit : BaseUnit
{
    [Header("Special Sounds")]
    [SerializeField] private AudioClip[] attackSounds;
    [SerializeField] private AudioClip[] deathSounds;
    
    public void PlayAttackSound()
    {
        if (attackSounds != null && attackSounds.Length > 0)
        {
    AudioSource.PlayClipAtPoint(
       attackSounds[Random.Range(0, attackSounds.Length)], 
             transform.position
 );
        }
    }
}
```

### Fraktions-spezifische Sounds

```csharp
// In BaseUnit:
[Header("Faction Sounds")]
[SerializeField] private Team unitFaction;
[SerializeField] private bool useFactionSounds = true;

// In UnitSelector:
private Dictionary<Team, AudioClip[]> factionSelectSounds;
private Dictionary<Team, AudioClip[]> factionMoveSounds;

private void PlaySelectionSound(BaseUnit unit)
{
    if (unit.useFactionSounds && factionSelectSounds.ContainsKey(unit.TeamComponent.CurrentTeam))
    {
        PlayRandomSound(factionSelectSounds[unit.TeamComponent.CurrentTeam]);
        return;
    }
  
    if (unit.HasCustomSelectSounds)
    {
        PlayRandomSound(unit.UnitSelectSounds);
    return;
    }
    
    PlayRandomSound(unitSelectSounds);
}
```

## Zusammenfassung

### ? Vorteile

1. **Flexibilität**: Units können eigene Sounds haben oder Defaults verwenden
2. **Einfach**: Automatische Priorisierung ohne Code
3. **Wartbar**: Zentrale Defaults + optionale Überschreibungen
4. **Variation**: Zufallswiedergabe verhindert Monotonie
5. **Skalierbar**: Funktioniert mit 1 oder 100 verschiedenen Units

### ?? Verwendung

- **Global Defaults**: Im UnitSelector für alle Units
- **Unit-Spezifisch**: Im BaseUnit für besondere Units
- **Fallback**: Automatisch wenn keine Custom Sounds
- **Multi-Selection**: Intelligente Priorisierung

### ?? Quick Setup

1. UnitSelector: Weise Default Sounds zu
2. BaseUnit: Optional Custom Sounds zuweisen
3. Fertig! System funktioniert automatisch

Das System ist vollständig implementiert und einsatzbereit! ??

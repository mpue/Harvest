# Audio System für große Gefechte - Dokumentation

## Übersicht

Das erweiterte Audio-System verteilt Sounds im Round-Robin-Verfahren auf mehrere AudioSources, um sicherzustellen, dass in großen Gefechten alle Sounds korrekt abgespielt werden.

## Komponenten

### 1. **AudioManager** (erweitert)
Der zentrale Audio-Manager mit AudioSource-Pooling.

**Neue Features:**
- **AudioSource Pooling**: Wiederverwendbare AudioSources für bessere Performance
- **Automatische Pool-Expansion**: Bei Bedarf werden neue AudioSources erstellt
- **Kategorie-basierte Pools**: Separate Pools für Waffen, UI, Einheiten, etc.

**Konfiguration:**
```csharp
[Header("AudioSource Pooling")]
usePooling = true;       // Pooling aktivieren
initialPoolSize = 20;           // Anfangsgröße des Pools
maxPoolSize = 50;   // Maximale Pool-Größe
autoExpandPool = true;          // Automatisch erweitern bei Bedarf
```

**Verwendung:**
```csharp
// 3D Sound abspielen
AudioManager.Instance.PlayOneShot(clip, position, AudioManager.AudioCategory.WeaponSounds);

// 2D Sound abspielen
AudioManager.Instance.PlayOneShot2D(clip, AudioManager.AudioCategory.UI);

// Zufälligen Sound aus Array abspielen
AudioManager.Instance.PlayOneShotRandom(clips, position, AudioManager.AudioCategory.WeaponSounds);
```

---

### 2. **RoundRobinPlayer** (überarbeitet)
Verteilt Sounds auf mehrere AudioSources in rotierender Reihenfolge.

**Features:**
- Verhindert Sound-Cutoff durch Rotation
- Automatische AudioManager-Integration
- Pitch-Variation für natürlicheren Sound
- Performance-Tracking

**Setup:**
```csharp
// In Unity Inspector:
numAudioSources = 8;    // Anzahl der AudioSources
maxConcurrentSounds = 16;       // Max. gleichzeitige Sounds
pitchVariation = 0.1f;          // Zufällige Pitch-Variation (0-1)
use3DSound = true;              // 3D- oder 2D-Sound
category = WeaponSounds;        // Audio-Kategorie
```

**Verwendung:**
```csharp
// Auf GameObject mit RoundRobinPlayer
roundRobinPlayer.PlayRandom();          // Zufälligen Clip abspielen
roundRobinPlayer.Play("FireSound");     // Spezifischen Clip abspielen
roundRobinPlayer.PlayClip(audioClip);   // AudioClip direkt abspielen
roundRobinPlayer.StopAll();        // Alle Sounds stoppen
```

---

### 3. **WeaponAudioPlayer** (neu)
Spezialisierte Klasse für Waffen-Sounds mit intelligenter Priorisierung.

**Features:**
- **Distance Culling**: Zu weit entfernte Sounds werden nicht abgespielt
- **Rate Limiting**: Verhindert, dass derselbe Sound zu oft gleichzeitig spielt
- **Volume-Anpassung**: Basierend auf Distanz
- **Automatische Integration**: Nutzt AudioManager wenn verfügbar

**Setup:**
```csharp
// WeaponAudioPlayer ist ein Singleton - automatisch erstellt
maxConcurrentSounds = 32;       // Max. gleichzeitige Waffensounds
minTimeBetweenSameSounds = 0.05f; // Min. Zeit zwischen gleichen Sounds
maxAudibleDistance = 100f;      // Max. Hördistanz
cullDistantSounds = true;       // Zu weit entfernte Sounds ignorieren
```

**Verwendung:**
```csharp
// In Weapon-Klasse oder ähnlich:
WeaponAudioPlayer.Instance.PlayWeaponSound(fireSound, shotPoint.position);

// Mit Array (zufällige Auswahl):
WeaponAudioPlayer.Instance.PlayWeaponSoundRandom(fireSounds, position);

// Distanz prüfen:
bool isAudible = WeaponAudioPlayer.Instance.IsAudible(position);
```

---

### 4. **AudioPlayerHelper** (neu)
Helper-Komponente für schnelle Integration.

**Setup:**
```csharp
// In Unity Inspector:
soundClips = [clip1, clip2, ...];  // Array von Sounds
numberOfAudioSources = 4;        // Anzahl AudioSources
soundCategory = SFX;         // Kategorie
randomizeOnPlay = true;            // Zufällige Auswahl
```

**Verwendung:**
```csharp
// Einfach aufrufen:
audioPlayerHelper.PlaySound();      // Zufälligen Sound
audioPlayerHelper.PlaySound(0);          // Sound mit Index
audioPlayerHelper.PlaySound("Explosion");   // Sound nach Name
audioPlayerHelper.StopAll();       // Alle stoppen
```

---

## Integration in bestehende Systeme

### Für Waffen:

Die `Weapon.cs` wurde bereits aktualisiert und nutzt automatisch das neue System:

```csharp
// Alt (in Weapon.cs):
audioSource.PlayOneShot(fireSound);

// Neu (automatisch):
WeaponAudioPlayer.Instance.PlayWeaponSound(fireSound, shotPoint.position);
// Fallback zu AudioManager, dann zu AudioSource
```

### Für UI-Sounds:

```csharp
// In ProductionPanel.cs, BuildingPlacement.cs, etc.:
AudioManager.Instance.PlayOneShot2D(buttonClickSound, AudioManager.AudioCategory.UI);
```

### Für Einheiten-Sounds:

```csharp
// In Health.cs (bereits implementiert):
// Verwendet temporäre AudioSources mit AudioManager-Integration
```

---

## Best Practices

### 1. **Waffen in großen Gefechten:**
```csharp
// Verwende WeaponAudioPlayer für beste Performance
WeaponAudioPlayer.Instance.PlayWeaponSound(fireSound, position);
```

### 2. **Mehrere Sounds pro Objekt:**
```csharp
// Füge RoundRobinPlayer hinzu
RoundRobinPlayer player = gameObject.AddComponent<RoundRobinPlayer>();
player.clips = mySounds;
player.PlayRandom();
```

### 3. **UI-Sounds:**
```csharp
// Verwende AudioManager direkt
AudioManager.Instance.PlayOneShot2D(clickSound, AudioManager.AudioCategory.UI);
```

### 4. **Explosionen und Einmal-Sounds:**
```csharp
// AudioManager mit Pooling
AudioManager.Instance.PlayOneShot(explosionSound, position, AudioManager.AudioCategory.SFX);
```

---

## Performance-Tipps

### Optimale Einstellungen für große Schlachten:

**AudioManager:**
- `initialPoolSize = 30` (für 50+ Einheiten)
- `maxPoolSize = 100` (für 100+ Einheiten)
- `autoExpandPool = true`

**WeaponAudioPlayer:**
- `maxConcurrentSounds = 32`
- `cullDistantSounds = true`
- `maxAudibleDistance = 80-100`
- `minTimeBetweenSameSounds = 0.05`

**RoundRobinPlayer (pro Waffe):**
- `numAudioSources = 4-8`
- `maxConcurrentSounds = 16`

### Speicher-Management:

Das System verwendet Object Pooling, sodass AudioSources wiederverwendet werden:
- ? Keine Garbage Collection bei jedem Sound
- ? Konstante Anzahl von GameObjects
- ? Automatisches Cleanup nach Abspielen

---

## Debugging

### AudioManager Pool-Status überprüfen:

```csharp
// Im Code:
int totalWeaponSources = AudioManager.Instance.GetTotalPoolSize(AudioManager.AudioCategory.WeaponSounds);
Debug.Log($"Weapon AudioSources: {totalWeaponSources}");
```

### RoundRobinPlayer Status:

```csharp
int activeSounds = roundRobinPlayer.GetActiveSoundCount();
Debug.Log($"Active sounds: {activeSounds}");
```

### WeaponAudioPlayer Distanz-Check:

```csharp
bool audible = WeaponAudioPlayer.Instance.IsAudible(weaponPosition);
float distance = WeaponAudioPlayer.Instance.GetDistanceToListener(weaponPosition);
```

---

## Migration von altem Code

### Waffen:
```csharp
// Alt:
audioSource.PlayOneShot(fireSound);

// Neu:
WeaponAudioPlayer.Instance.PlayWeaponSound(fireSound, transform.position);
```

### UI:
```csharp
// Alt:
AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position);

// Neu:
AudioManager.Instance.PlayOneShot2D(clickSound, AudioManager.AudioCategory.UI);
```

### Generische Sounds:
```csharp
// Alt:
Instantiate(soundPrefab);

// Neu:
AudioManager.Instance.PlayOneShot(clip, position, AudioManager.AudioCategory.SFX);
```

---

## Häufige Probleme

**Problem:** Sounds werden trotzdem abgeschnitten
- **Lösung:** Erhöhe `numAudioSources` im RoundRobinPlayer
- **Lösung:** Erhöhe `maxPoolSize` im AudioManager

**Problem:** Zu viele Sounds gleichzeitig
- **Lösung:** Aktiviere `cullDistantSounds` im WeaponAudioPlayer
- **Lösung:** Reduziere `maxConcurrentSounds`

**Problem:** Performance-Einbrüche
- **Lösung:** Aktiviere `usePooling` im AudioManager
- **Lösung:** Reduziere `maxAudibleDistance` im WeaponAudioPlayer

---

## Zusammenfassung

? **AudioManager**: Zentrales Pooling-System für alle Sounds  
? **RoundRobinPlayer**: Verhindert Sound-Cutoff durch Rotation  
? **WeaponAudioPlayer**: Optimiert für große Gefechte  
? **AudioPlayerHelper**: Schnelle Integration  

Das System ist **rückwärtskompatibel** und funktioniert automatisch mit bestehendem Code!

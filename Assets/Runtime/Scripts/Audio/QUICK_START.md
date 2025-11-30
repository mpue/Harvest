# Audio System - Quick Start Guide

## ?? Schnellstart für große Gefechte

### Schritt 1: AudioManager einrichten (automatisch)

Der AudioManager wird automatisch erstellt, sobald Sie ihn verwenden. Für manuelle Konfiguration:

1. Erstelle ein leeres GameObject: `AudioManager`
2. Füge die `AudioManager` Komponente hinzu
3. Konfiguriere im Inspector:
   ```
   [AudioSource Pooling]
   ? Use Pooling: true
   Initial Pool Size: 30
   Max Pool Size: 100
   ? Auto Expand Pool: true
   ```

### Schritt 2: Für Waffen (automatisch integriert!)

Die `Weapon.cs` nutzt automatisch das neue System. Keine Änderungen nötig!

**Optional:** Für noch bessere Performance, füge `RoundRobinPlayer` zu Waffen hinzu:

1. Wähle alle Weapon-Prefabs
2. Füge Komponente hinzu: `RoundRobinPlayer`
3. Konfiguriere:
```
   Clips: [Deine Fire Sounds]
   Num Audio Sources: 8
   Max Concurrent Sounds: 16
   Category: WeaponSounds
   ? Use Audio Manager: true
   ```

### Schritt 3: Für große Schlachten (optional)

Erstelle einen `BattleAudioManager` GameObject:

1. Füge die `AudioSystemSetupExample` Komponente hinzu
2. Konfiguriere:
   ```
   ? Auto Setup: true
   Max Concurrent Weapon Sounds: 32
   Weapon Audio Distance: 100
   ```

### Schritt 4: Testen!

Starte eine große Schlacht und beobachte:
- ? Alle Sounds werden abgespielt
- ? Keine abrupten Enden
- ? Bessere Performance
- ? Automatisches Distance Culling

---

## ?? Für verschiedene Anwendungsfälle

### Nur für Waffen:
```csharp
// Bereits automatisch in Weapon.cs integriert!
// Nichts zu tun ??
```

### Für UI-Sounds:
```csharp
AudioManager.Instance.PlayOneShot2D(
    clickSound, 
    AudioManager.AudioCategory.UI
);
```

### Für Explosionen/Effekte:
```csharp
AudioManager.Instance.PlayOneShot(
    explosionSound, 
    position, 
    AudioManager.AudioCategory.SFX
);
```

### Für Einheiten mit vielen Sounds:
```csharp
// Füge RoundRobinPlayer hinzu
// Setze Clips im Inspector
// Nutze: roundRobinPlayer.PlayRandom()
```

---

## ?? Empfohlene Einstellungen

### Kleine Gefechte (< 20 Einheiten):
```
AudioManager:
  Initial Pool Size: 10
  Max Pool Size: 30

WeaponAudioPlayer:
  Max Concurrent Sounds: 16
```

### Mittlere Gefechte (20-50 Einheiten):
```
AudioManager:
  Initial Pool Size: 20
  Max Pool Size: 50

WeaponAudioPlayer:
  Max Concurrent Sounds: 24
```

### Große Gefechte (50+ Einheiten):
```
AudioManager:
  Initial Pool Size: 30
  Max Pool Size: 100

WeaponAudioPlayer:
  Max Concurrent Sounds: 32
  ? Cull Distant Sounds: true
  Max Audible Distance: 80
```

---

## ?? Troubleshooting

**Sounds werden abgeschnitten:**
- Erhöhe `numAudioSources` im RoundRobinPlayer (8-16)
- Erhöhe `maxPoolSize` im AudioManager

**Zu viele Sounds gleichzeitig:**
- Aktiviere Distance Culling
- Reduziere Max Audible Distance

**Performance-Probleme:**
- ? Use Pooling aktivieren
- Distance Culling aktivieren
- Max Concurrent Sounds reduzieren

---

## ? Checkliste

- [ ] AudioManager existiert (automatisch)
- [ ] WeaponAudioPlayer existiert (automatisch)
- [ ] Pooling aktiviert (empfohlen)
- [ ] Distance Culling aktiviert (für große Karten)
- [ ] RoundRobinPlayer auf Waffen (optional, für beste Performance)
- [ ] Einstellungen für Schlachtgröße angepasst

---

## ?? Monitoring (Runtime)

Füge `AudioSystemSetupExample` zu einem GameObject hinzu, um Runtime-Stats zu sehen:
- Anzahl aktiver RoundRobinPlayers
- Anzahl aktiver Sounds
- System-Status

---

## ?? Fertig!

Das System funktioniert **automatisch** mit Ihrem bestehenden Code!

Für maximale Performance in großen Schlachten:
1. ? Pooling aktivieren
2. ? Distance Culling aktivieren  
3. ? RoundRobinPlayer zu Waffen hinzufügen (optional)

**Viel Erfolg bei Ihren epischen Schlachten! ????**

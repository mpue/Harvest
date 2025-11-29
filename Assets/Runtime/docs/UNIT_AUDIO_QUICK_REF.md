# Unit Audio System - Quick Reference

## Schnellstart

### 1. UnitSelector Setup (Defaults)
```
UnitSelector GameObject > Inspector:
  Audio Feedback:
    Unit Select Sounds: [Sound1, Sound2, Sound3]
    Unit Move Sounds: [Sound1, Sound2, Sound3]
    Audio Volume: 1.0
```

### 2. BaseUnit Setup (Optional, Unit-spezifisch)
```
Unit GameObject > BaseUnit > Inspector:
  Audio:
    Unit Select Sounds: [CustomSound1, CustomSound2]
 Unit Move Sounds: [CustomSound1, CustomSound2]
```

### 3. Fertig!
- Unit mit eigenen Sounds ? Spielt Custom Sounds
- Unit ohne eigene Sounds ? Spielt Default Sounds
- Automatisch, kein Code nötig

## Priorisierung

```
1. Unit.UnitSelectSounds (wenn vorhanden)
   ?
2. UnitSelector.unitSelectSounds (Default)
   ?
3. Kein Sound
```

## Beispiele

### Soldier mit eigenen Sounds
```
SoldierUnit:
  Audio:
    Unit Select Sounds:
 - SoldierYessir.wav
      - SoldierReady.wav
    Unit Move Sounds:
  - SoldierMovingOut.wav
      - SoldierAffirmative.wav

? Spielt Soldier-Sounds
```

### Tank ohne eigene Sounds
```
TankUnit:
  Audio:
    Unit Select Sounds: (leer)
    Unit Move Sounds: (leer)

? Spielt UnitSelector Default-Sounds
```

## Prüfen

```csharp
BaseUnit unit = GetComponent<BaseUnit>();

// Hat eigene Selection Sounds?
bool hasCustom = unit.HasCustomSelectSounds;

// Hat eigene Move Sounds?
bool hasMove = unit.HasCustomMoveSounds;

// Sound Arrays
AudioClip[] select = unit.UnitSelectSounds;
AudioClip[] move = unit.UnitMoveSounds;
```

## Häufige Probleme

### Kein Sound
- [ ] UnitSelector hat AudioSource
- [ ] Audio Volume > 0
- [ ] Default Sounds zugewiesen
- [ ] Audio Listener in Szene

### Unit-Sound spielt nicht
- [ ] AudioClips im BaseUnit zugewiesen
- [ ] Array nicht leer
- [ ] Clips nicht null

### Immer gleicher Sound
- [ ] Mehrere Sounds zuweisen (3-5)
- [ ] Verschiedene AudioClips verwenden

## Best Practices

? **DO:**
- 3-5 Sounds pro Kategorie
- Defaults im UnitSelector setzen
- Kurze Sounds (0.5-2s)
- Klare Bestätigung

? **DON'T:**
- Nur 1 Sound (repetitiv)
- Keine Defaults (kein Fallback)
- Lange Sounds (>3s)
- Undeutliche Sounds

## Checkliste

### Minimum Setup
- [ ] UnitSelector hat AudioSource
- [ ] Default Select Sounds (3+)
- [ ] Default Move Sounds (3+)
- [ ] Audio Volume: 1.0

### Unit-spezifisch (Optional)
- [ ] BaseUnit Audio Header
- [ ] Custom Select Sounds (3+)
- [ ] Custom Move Sounds (3+)

## Siehe auch

- **Vollständige Doku:** `UNIT_AUDIO_SYSTEM.md`
- **AudioManager:** `AudioManager.cs`
- **UnitSelector:** `UnitSelector.cs`
- **BaseUnit:** `BaseUnit.cs`

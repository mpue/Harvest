# Animation System - Quick Start

## 5-Minuten Setup

### Schritt 1: Komponenten hinzufügen

Wähle deine Unit im Hierarchy aus und füge hinzu:

1. **Animator** (falls noch nicht vorhanden)
   - Component > Miscellaneous > Animator

2. **Animatable**
   - Component > Scripts > Animatable

3. **Controllable** (sollte bereits vorhanden sein)

### Schritt 2: Animator Controller erstellen

1. **Erstelle Animator Controller:**
   - Rechtsklick in Project > Create > Animator Controller
   - Benenne ihn z.B. "SoldierAnimator"

2. **Öffne Animator Window:**
   - Doppelklick auf den Controller
   - Oder Window > Animation > Animator

3. **Erstelle States:**
   - Rechtsklick > Create State > Empty
   - Benenne: "Idle"
   - Wiederhole für "Move"

4. **Füge Parameter hinzu:**
   - Im Animator-Fenster > Parameters Tab > +
   - Typ: Bool
   - Name: "IsMoving"

5. **Erstelle Transitions:**
   - Rechtsklick auf Idle > Make Transition > Klick auf Move
   - Klick auf Transition-Pfeil
   - Im Inspector: Conditions > + > IsMoving = true
   
   - Rechtsklick auf Move > Make Transition > Klick auf Idle
 - Klick auf Transition-Pfeil
   - Im Inspector: Conditions > + > IsMoving = false

6. **Weise Animation Clips zu:**
   - Klick auf "Idle" State
   - Im Inspector: Motion > Wähle Idle Animation Clip
   - Wiederhole für "Move" State mit Walk/Run Animation

### Schritt 3: Controller zuweisen

1. Wähle deine Unit im Hierarchy
2. Im Inspector: Animator > Controller
3. Ziehe deinen Animator Controller rein

### Schritt 4: Animatable konfigurieren

Im Inspector der Unit > Animatable:
```
Animation Parameters:
  Move Parameter Name: IsMoving
  Move Speed Parameter Name: MoveSpeed
  Idle State Name: Idle
  Move State Name: Move

Animation Settings:
  ? Use Bool Parameter
  ? Use Speed Parameter (optional)
  ? Use State Triggers
  Movement Speed Multiplier: 1
```

### Schritt 5: Controllable konfigurieren

Im Inspector der Unit > Controllable:
```
Animation:
  ? Use Animation
  ? Update Animation Speed
```

### Schritt 6: Testen

1. **Starte das Spiel**
2. **Bewege die Unit** (Rechtsklick oder MoveTo Befehl)
3. **Beobachte:**
   - Console: `? Animatable on UnitName initialized`
   - Animation sollte von Idle zu Move wechseln
   - Bei Stopp: Zurück zu Idle

## Fertig! ??

Die Unit sollte jetzt automatisch animiert werden bei Bewegung.

## Troubleshooting

### Animation spielt nicht ab

**Prüfe:**
1. ? Animator Controller zugewiesen?
2. ? Animation Clips in States zugewiesen?
3. ? Parameter "IsMoving" existiert?
4. ? Transitions haben Conditions?
5. ? Controllable > Use Animation aktiviert?

**Console-Check:**
- Sollte sehen: `? Animatable on ... initialized`
- Bei Debug Logging: `?? Animation state changed`

### Unit bewegt sich aber keine Animation

**Lösung:**
- Prüfe ob `Animatable` Komponente vorhanden ist
- Aktiviere Debug Logging in Animatable
- Prüfe Console für Error Messages

## Nächste Schritte

### Geschwindigkeits-basierte Animation (Optional)

Verwende einen Blend Tree für variable Walk/Run Speeds:

1. Im Animator: Lösche "Move" State
2. Rechtsklick > Create State > From New Blend Tree
3. Benenne: "Movement"
4. Doppelklick auf Blend Tree
5. Set Blend Type: 1D
6. Set Parameter: MoveSpeed
7. Füge Motions hinzu:
   - Idle (Threshold: 0)
   - Walk (Threshold: 2.5)
   - Run (Threshold: 5)

8. In Animatable:
 - ? Use Speed Parameter

### Combat Animations (Optional)

Füge Attack/Death States hinzu:

1. **Parameter hinzufügen:**
   - Attack (Trigger)
   - Die (Trigger)

2. **States erstellen:**
   - Attack State mit Animation
   - Death State mit Animation

3. **Transitions:**
   - Any State ? Attack (Condition: Attack trigger)
   - Any State ? Death (Condition: Die trigger)

4. **Code:**
```csharp
Animatable anim = unit.GetComponent<Animatable>();
anim.SetTrigger("Attack");
anim.SetTrigger("Die");
```

## Beispiel Animator Layout

```
??????????
?  Idle  ? ????? Entry
??????????
    ? IsMoving=true
??????????
?  Move  ?
??????????
? IsMoving=false
    ?????????????
  ?
     ?
    ??????????????
  ? Any State  ?
        ??????????????
 ? ?
  Attack  Die
         (trigger) (trigger)
```

## Siehe auch

- **Vollständige Dokumentation:** `ANIMATION_SYSTEM_GUIDE.md`
- **Code-Beispiele:** `Examples/AnimatedCombatUnit.cs`
- **Unity Animator Docs:** https://docs.unity3d.com/Manual/class-AnimatorController.html

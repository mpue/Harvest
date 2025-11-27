# Animation System für Units

## Übersicht

Das Animation-System ermöglicht es Units, Animationen basierend auf ihrem Bewegungszustand abzuspielen. Es besteht aus zwei Komponenten:

- **Animatable**: Verwaltet die Animator-Parameter und Animation-States
- **Controllable**: Setzt automatisch die Animation-States während der Bewegung

## Komponenten

### Animatable

Die `Animatable` Komponente ist das Interface zwischen Unity's Animator und dem Unit-Steuerungssystem.

#### Features

- ? Automatische Bewegungs-Animation
- ? Geschwindigkeits-basierte Animation (Blend Trees)
- ? Bool-Parameter oder State-Trigger Support
- ? Performance-optimiert mit gehashten Parameter-Namen
- ? Debug-Logging für Entwicklung

#### Setup

1. **Füge Komponenten hinzu:**
   ```
   Unit GameObject
   ??? Animator (Required)
   ??? Animatable
   ??? Controllable
```

2. **Konfiguriere Animatable im Inspector:**
   ```
   Animation Parameters:
   - Move Parameter Name: "IsMoving" (bool)
   - Move Speed Parameter Name: "MoveSpeed" (float)
   - Idle State Name: "Idle"
   - Move State Name: "Move"
   
   Animation Settings:
   - Use Bool Parameter: ? (empfohlen)
   - Use Speed Parameter: ? (für Blend Trees)
   - Use State Triggers: ? (optional)
   - Movement Speed Multiplier: 1.0
   ```

3. **Erstelle Animator Controller:**

   **Variante A: Mit Bool Parameter (Empfohlen)**
   ```
   Parameters:
   - IsMoving (bool)
   - MoveSpeed (float, optional)
   
   States:
   - Idle
   - Move (oder Blend Tree)
   
   Transitions:
   - Idle ? Move: Condition: IsMoving == true
   - Move ? Idle: Condition: IsMoving == false
   ```

   **Variante B: Mit Blend Tree (Für variable Geschwindigkeit)**
   ```
   Parameters:
   - IsMoving (bool)
   - MoveSpeed (float)
   
   States:
   - Idle
   - Move (Blend Tree)
  ??? Walk (MoveSpeed: 0.0 - 5.0)
     ??? Run (MoveSpeed: 5.0 - 10.0)
   ```

### Controllable (Erweitert)

Die `Controllable` Komponente steuert jetzt automatisch die Animationen.

#### Neue Inspector-Einstellungen

```
Animation:
- Use Animation: ? (Animation aktivieren)
- Update Animation Speed: ? (MoveSpeed Parameter aktualisieren)
```

#### Automatisches Verhalten

- **Bei MoveTo()**: Setzt `IsMoving = true`
- **Bei Stop()**: Setzt `IsMoving = false`
- **Bei Ziel erreicht**: Setzt `IsMoving = false`
- **Während Bewegung**: Aktualisiert `MoveSpeed` basierend auf aktueller Geschwindigkeit

## Verwendung

### Basis-Setup (Automatisch)

Wenn `Animatable` vorhanden ist, funktioniert alles automatisch:

```csharp
// Bewegung starten ? Animation wird automatisch gesetzt
controllable.MoveTo(destination);
// IsMoving = true, MoveSpeed wird aktualisiert

// Bewegung stoppen ? Animation stoppt automatisch
controllable.Stop();
// IsMoving = false, MoveSpeed = 0
```

### Manuelle Steuerung

Du kannst die Animation auch manuell steuern:

```csharp
Animatable animatable = unit.GetComponent<Animatable>();

// Bewegungs-State setzen
animatable.SetMoving(true);
animatable.SetMoving(false);

// Geschwindigkeit setzen
animatable.SetMovementSpeed(5.0f);

// Custom Parameter setzen
animatable.SetBool("IsAttacking", true);
animatable.SetFloat("Health", 75.0f);
animatable.SetTrigger("Die");

// Direkten State abspielen
animatable.PlayState("Attack");
```

### Zugriff über Controllable

```csharp
Controllable controllable = unit.GetComponent<Controllable>();
Animatable animatable = controllable.GetAnimatable();

if (animatable != null)
{
    animatable.SetTrigger("SpecialAction");
}

// Animation temporär deaktivieren
controllable.SetUseAnimation(false);
```

## Animator Controller Beispiele

### Beispiel 1: Simple Idle/Move

**Parameters:**
- `IsMoving` (bool)

**States & Transitions:**
```
[Idle] ?????IsMoving=true??????> [Move]
  ^         ?
  ?????????IsMoving=false?????????????
```

**Setup in Unity:**
1. Erstelle neuen Animator Controller
2. Füge "Idle" und "Move" States hinzu
3. Füge Parameter "IsMoving" (bool) hinzu
4. Erstelle Transitions mit Conditions

### Beispiel 2: Blend Tree mit Geschwindigkeiten

**Parameters:**
- `IsMoving` (bool)
- `MoveSpeed` (float)

**States:**
```
[Idle]
  ??> [Movement Blend Tree]
     ?? Idle (0.0)
 ?? Walk (2.5)
       ?? Run (5.0)
       ?? Sprint (7.5)
```

**Blend Tree Setup:**
1. Rechtsklick in Animator ? Create State ? From New Blend Tree
2. Doppelklick auf Blend Tree
3. Blend Type: 1D
4. Parameter: MoveSpeed
5. Füge Motion Clips hinzu mit entsprechenden Thresholds

### Beispiel 3: Mit Combat States

**Parameters:**
- `IsMoving` (bool)
- `MoveSpeed` (float)
- `IsAttacking` (bool)
- `Die` (trigger)

**States:**
```
      ????????????????????
           ?      Idle    ?
  ????????????????????
         ? IsMoving=true
   ????????????????????
           ?    Movement ?
           ?   (Blend Tree)   ?
           ????????????????????
           ? IsAttacking=true
    ????????????????????
   ?     Attack       ?????????
   ????????????????????  ?
 ? IsAttacking=false ?
      ?????????????????????
      
      Die (trigger) ? [Death]
```

## Performance-Optimierung

### Parameter Hashing

`Animatable` verwendet automatisch Parameter-Hashing für bessere Performance:

```csharp
// Langsam (bei jedem Aufruf String-Konvertierung):
animator.SetBool("IsMoving", true);

// Schnell (gehashter Zugriff):
int moveHash = Animator.StringToHash("IsMoving");
animator.SetBool(moveHash, true);
```

Dies geschieht automatisch in `Animatable.Awake()`.

### Update-Frequenz

Die `MoveSpeed` wird nur aktualisiert wenn:
- `updateAnimationSpeed = true`
- Unit sich bewegt (`hasTarget = true`)
- Animatable vorhanden ist

## Debug & Troubleshooting

### Debug Logging aktivieren

Im `Animatable` Inspector:
```
Debug:
- Debug Logging: ?
```

Console Output:
```
? Animatable on SoldierUnit initialized (Animator: SoldierAnimator)
?? SoldierUnit: Animation state changed - IsMoving: true
?? SoldierUnit: Animation speed set to 5.32
```

### Häufige Probleme

#### Animation spielt nicht ab

**Problem:** Unit bewegt sich aber Animation bleibt in Idle

**Lösungen:**
1. ? Prüfe ob `Animatable` Komponente vorhanden ist
2. ? Prüfe ob `Controllable > Use Animation` aktiviert ist
3. ? Prüfe ob Animator Controller zugewiesen ist
4. ? Prüfe Parameter-Namen im Animator (Case-Sensitive!)
5. ? Prüfe Transition Conditions

**Diagnose:**
```csharp
Animatable anim = unit.GetComponent<Animatable>();
if (anim == null) Debug.LogError("No Animatable component!");
if (anim.GetAnimator() == null) Debug.LogError("No Animator!");
if (anim.GetAnimator().runtimeAnimatorController == null) 
    Debug.LogError("No Animator Controller assigned!");
```

#### Animation zu schnell/langsam

**Problem:** Bewegungs-Animation läuft nicht synchron mit tatsächlicher Bewegung

**Lösungen:**
1. Passe `Movement Speed Multiplier` in Animatable an
2. Passe Blend Tree Thresholds an
3. Stelle sicher `Update Animation Speed` ist aktiviert

#### Falsche Parameter-Namen

**Fehler in Console:**
```
Parameter 'IsMoving' does not exist in the Animator
```

**Lösung:**
- Öffne Animator Controller
- Prüfe ob Parameter existiert
- Prüfe Schreibweise (Case-Sensitive!)
- Update `Animatable > Move Parameter Name`

## Checkliste: Unit Animation Setup

### Minimum Setup (Basis)
- [ ] GameObject hat `Animator` Komponente
- [ ] GameObject hat `Animatable` Komponente
- [ ] GameObject hat `Controllable` Komponente
- [ ] Animator Controller ist zugewiesen
- [ ] Parameter "IsMoving" (bool) existiert im Animator
- [ ] Idle State existiert
- [ ] Move State existiert
- [ ] Transitions zwischen Idle ? Move mit IsMoving Condition

### Erweitert (Mit Geschwindigkeit)
- [ ] Parameter "MoveSpeed" (float) existiert
- [ ] Move State ist ein Blend Tree
- [ ] Blend Tree hat Walk/Run Clips
- [ ] `Animatable > Use Speed Parameter` aktiviert
- [ ] `Controllable > Update Animation Speed` aktiviert

### Optional (Combat/Events)
- [ ] Attack States/Trigger
- [ ] Death State/Trigger
- [ ] Custom Trigger für Spezialaktionen

## Code-Beispiele

### Beispiel 1: Einfache Unit mit Walk/Idle

```csharp
// Setup (wird automatisch gemacht):
void Start()
{
    Controllable controllable = GetComponent<Controllable>();
    controllable.SetUseAnimation(true);
}

// Bewegung (Animation wird automatisch gesetzt):
void MoveToTarget(Vector3 target)
{
    GetComponent<Controllable>().MoveTo(target);
    // IsMoving wird automatisch auf true gesetzt
}
```

### Beispiel 2: Combat Unit mit Attack Animation

```csharp
public class CombatUnit : MonoBehaviour
{
    private Animatable animatable;
    private Controllable controllable;
    
    void Start()
    {
        animatable = GetComponent<Animatable>();
      controllable = GetComponent<Controllable>();
    }
    
    public void AttackTarget(Transform target)
    {
        // Stoppe Bewegung
      controllable.Stop();
        // Animation wird automatisch auf Idle gesetzt
        
        // Spiele Attack Animation
        if (animatable != null)
    {
            animatable.SetBool("IsAttacking", true);
   // oder:
            animatable.SetTrigger("Attack");
        }
    }
    
    public void StopAttack()
    {
        if (animatable != null)
        {
            animatable.SetBool("IsAttacking", false);
   }
  }
}
```

### Beispiel 3: Harvester mit Gather Animation

```csharp
public class AnimatedHarvester : HarvesterUnit
{
    private Animatable animatable;
    
    protected override void Awake()
    {
        base.Awake();
        animatable = GetComponent<Animatable>();
    }
    
    protected override void OnStartHarvesting()
    {
base.OnStartHarvesting();
        
   if (animatable != null)
     {
         animatable.SetBool("IsGathering", true);
   // Bewegungs-Animation wird automatisch gestoppt
        }
    }
    
    protected override void OnStopHarvesting()
    {
        base.OnStopHarvesting();
     
        if (animatable != null)
      {
            animatable.SetBool("IsGathering", false);
        }
    }
}
```

## Integration mit bestehendem Code

### HarvesterUnit

```csharp
// In HarvesterUnit.cs - Gathering State setzen:
private void StartGathering()
{
    Animatable anim = GetComponent<Animatable>();
    if (anim != null)
    {
   anim.SetBool("IsGathering", true);
    }
}
```

### WeaponController

```csharp
// In WeaponController.cs - Shooting Animation:
private void Fire()
{
    Animatable anim = GetComponent<Animatable>();
    if (anim != null)
    {
        anim.SetTrigger("Shoot");
    }
    // ... rest of fire logic
}
```

## Best Practices

1. **Parameter-Namen konsistent halten:**
   - Verwende einheitliche Namen: "IsMoving", "MoveSpeed", etc.
   - Case-Sensitive! "ismoving" ? "IsMoving"

2. **Bool vs Trigger:**
   - Bool: Für dauerhaften State (IsMoving, IsAttacking)
   - Trigger: Für einmalige Events (Die, Shoot)

3. **Blend Trees für variable Geschwindigkeit:**
   - Verwende MoveSpeed Parameter
   - Setze sinnvolle Thresholds (0, 2.5, 5, 7.5)

4. **Performance:**
   - Aktiviere Debug Logging nur während Entwicklung
   - Deaktiviere `Update Animation Speed` wenn nicht benötigt

5. **Null-Checks:**
   - Prüfe immer ob Animatable vorhanden ist
   - System funktioniert auch ohne Animatable

## Zusammenfassung

? **Automatisch:**
- Controllable setzt IsMoving basierend auf Bewegungszustand
- MoveSpeed wird basierend auf aktueller Geschwindigkeit aktualisiert
- Funktioniert mit NavMesh und manueller Bewegung

? **Flexibel:**
- Unterstützt Bool Parameter, Triggers und State-basiert
- Custom Parameter können jederzeit gesetzt werden
- Kann aktiviert/deaktiviert werden

? **Performance:**
- Parameter werden gehasht für schnellen Zugriff
- Update nur wenn notwendig
- Optionales Speed-Update

? **Debug-Friendly:**
- Ausführliche Logging-Option
- Klare Error-Messages
- Inspector-Konfiguration

# Weapon Animation System

## Übersicht

Das Weapon Animation System erweitert die `Weapon` Klasse um automatische Animation-Unterstützung für Ziel-Erfassung (Aim) und Schussabgabe (Fire).

## Features

- ? **Automatisches Aim Animation**: Setzt IsAiming basierend auf Ziel-Status
- ? **Fire Animation Trigger**: Triggert Fire-Animation bei jedem Schuss
- ? **Flexible Konfiguration**: Bool oder Float für Aim, Trigger für Fire
- ? **Optionale Aktivierung**: Kann deaktiviert werden wenn nicht benötigt
- ? **Parent-Component Support**: Sucht Animatable auf Parent GameObject

## Komponenten-Hierarchie

```
Unit GameObject
??? Animator
??? Animatable
??? WeaponController
??? Weapon (kann auch Child sein)
    ??? Turret
        ??? Barrel
    ??? ShotPoint
```

**Wichtig:** `Animatable` muss auf dem **Parent GameObject** sein (Unit), nicht auf dem Weapon selbst!

## Setup

### Schritt 1: Komponenten prüfen

Stelle sicher dass die Unit folgende Komponenten hat:
- ? `Animator` (auf Unit)
- ? `Animatable` (auf Unit)
- ? `WeaponController` (auf Unit)
- ? `Weapon` (auf Unit oder Child)

### Schritt 2: Weapon konfigurieren

Im Inspector des Weapon Components:

```
Animation:
  ? Use Animation
  Aim Parameter Name: "IsAiming"
  Fire Trigger Name: "Fire"
  ? Use Bool For Aim (empfohlen)
  ? Use Trigger For Fire (empfohlen)
```

### Schritt 3: Animator Controller erweitern

#### Parameter hinzufügen:

1. **IsAiming** (bool) - Für Ziel-Erfassung
2. **Fire** (trigger) - Für Schuss-Animation

#### States erstellen:

**Variante A: Einfaches Setup**
```
States:
- Idle
- Aiming
- Firing

Transitions:
- Idle ? Aiming: IsAiming == true
- Aiming ? Idle: IsAiming == false
- Any State ? Firing: Fire trigger (kurze Animation)
- Firing ? Previous State (automatisch nach Animation)
```

**Variante B: Mit Blend Tree (Bewegung + Aim)**
```
States:
- Movement Blend Tree
  ?? Idle
  ?? Walk
  ?? Run
- Aiming Blend Tree
  ?? Idle Aim
  ?? Walk Aim
  ?? Run Aim
- Firing

Parameters:
- IsMoving (bool)
- MoveSpeed (float)
- IsAiming (bool)
- Fire (trigger)

Transitions:
- Movement ? Aiming: IsAiming == true
- Aiming ? Movement: IsAiming == false
- Any State ? Firing: Fire trigger
```

## Automatisches Verhalten

### Aim Animation

**Wird automatisch gesetzt:**
- `SetTarget(target)` ? `IsAiming = true`
- `ClearTarget()` ? `IsAiming = false`
- Target verloren ? `IsAiming = false`

**Update in Update():**
```csharp
// Kontinuierliche Prüfung:
bool isAiming = currentTarget != null;
animatable.SetBool("IsAiming", isAiming);
```

### Fire Animation

**Wird getriggert bei jedem Schuss:**
```csharp
private void Fire()
{
    // ... Fire logic ...
    
    // Trigger animation
    animatable.SetTrigger("Fire");
    
    // ... Spawn projectile ...
}
```

## Code-Beispiele

### Beispiel 1: Basis-Setup (Automatisch)

```csharp
// Setup ist bereits vorhanden - funktioniert automatisch!

Weapon weapon = unit.GetComponentInChildren<Weapon>();

// Ziel setzen ? Aim Animation wird automatisch aktiviert
weapon.SetTarget(enemyTransform);
// IsAiming = true

// Schießen ? Fire Animation wird automatisch getriggert
weapon.TryFire();
// Fire trigger

// Ziel clearen ? Aim Animation wird automatisch deaktiviert
weapon.ClearTarget();
// IsAiming = false
```

### Beispiel 2: Manuelle Kontrolle

```csharp
Weapon weapon = unit.GetComponentInChildren<Weapon>();

// Manuell Aim Animation setzen
weapon.SetAimAnimation(true);

// Manuell Fire Animation triggern (ohne zu schießen)
weapon.TriggerFireAnimation();

// Animation temporär deaktivieren
weapon.SetUseAnimation(false);

// Später wieder aktivieren
weapon.SetUseAnimation(true);
```

### Beispiel 3: Custom Weapon mit spezieller Animation

```csharp
public class CustomWeapon : MonoBehaviour
{
    private Weapon weapon;
  private Animatable animatable;
    
    void Start()
    {
        weapon = GetComponent<Weapon>();
        animatable = weapon.GetAnimatable();
    }
    
    public void ChargedShot()
    {
        // Spiele Charge-Animation
      if (animatable != null)
        {
   animatable.SetTrigger("ChargeShot");
   }
        
        // Warte auf Animation
        Invoke(nameof(FireChargedShot), 1.5f);
    }
    
    private void FireChargedShot()
    {
      // Normale Fire Animation
        weapon.TriggerFireAnimation();
        
        // Custom Shot Logic
        // ...
    }
}
```

### Beispiel 4: Combat Unit mit Aim + Fire Animations

```csharp
public class AnimatedCombatUnit : MonoBehaviour
{
    private Weapon weapon;
    private Animatable animatable;
    private WeaponController weaponController;
    
    void Awake()
    {
        weapon = GetComponentInChildren<Weapon>();
  animatable = GetComponent<Animatable>();
        weaponController = GetComponent<WeaponController>();
    }
    
    void Update()
    {
        // Weapon kümmert sich automatisch um Aim/Fire Animations
      // WeaponController managed Target Acquisition
        
  // Optional: Custom Verhalten basierend auf States
 if (weapon != null && animatable != null)
        {
   bool isAiming = weaponController.HasTarget;
       
            // Custom Parameter für UI oder andere Systeme
   if (isAiming)
     {
       // Zeige Ziel-UI
            ShowTargetingUI();
            }
        }
    }
  
    private void ShowTargetingUI()
{
        // Custom UI Logic
    }
}
```

## Animator Controller Beispiele

### Beispiel 1: Einfacher Soldier

**Parameters:**
```
IsMoving (bool)
IsAiming (bool)
Fire (trigger)
```

**State Machine:**
```
Entry ? Idle

[Idle] ??IsMoving=true??> [Walk]
  ?  ?
  ?????IsMoving=false????????
  
  ? IsAiming=true
  ?
[Aiming] ??IsAiming=false??> zurück zu Idle/Walk
  
[Any State] ??Fire trigger??> [Firing]
  ?
  ???automatisch zurück nach Animation
```

**Setup:**
1. Idle State: Idle Animation
2. Walk State: Walk Animation
3. Aiming State: Aim Pose Animation
4. Firing State: Fire Animation (kurz, ~0.3s)

**Transitions:**
- Idle ? Walk: IsMoving condition
- Idle/Walk ? Aiming: IsAiming = true
- Aiming ? Idle/Walk: IsAiming = false
- Any State ? Firing: Fire trigger
- Firing ? Previous State: Exit Time (keine Condition)

### Beispiel 2: Tank mit Turret

**Parameters:**
```
IsAiming (bool)
Fire (trigger)
TurretRotation (float) - optional
```

**State Machine:**
```
Entry ? Idle

[Idle] ??IsAiming=true??> [Aiming]
  ?            ?
  ?????IsAiming=false????????
  
[Aiming] ??Fire trigger??> [Firing]
  ?           ?
  ???????automatisch??????????
```

**Setup:**
- Idle: Tank idle, turret neutral
- Aiming: Subtle aim pose (optional)
- Firing: Recoil animation

**Special:**
- Turret rotation kann über Weapon.turretTransform gesteuert werden
- Animation kann optional sein (nur Recoil bei Fire)

### Beispiel 3: Blend Tree für Movement + Aiming

**Parameters:**
```
IsMoving (bool)
MoveSpeed (float)
IsAiming (bool)
Fire (trigger)
```

**Blend Tree Structure:**
```
Movement Blend Tree (1D, MoveSpeed):
?? Idle (0.0)
?? Walk (2.5)
?? Run (5.0)

Aiming Blend Tree (1D, MoveSpeed):
?? Idle Aim (0.0)
?? Walk Aim (2.5)
?? Run Aim (5.0)
```

**Transitions:**
- Movement Blend ? Aiming Blend: IsAiming = true
- Aiming Blend ? Movement Blend: IsAiming = false
- Any State ? Firing: Fire trigger

**Vorteil:**
- Smooth blending zwischen Bewegungsgeschwindigkeiten
- Unit kann sich bewegen während sie zielt
- Realistische Animationen

## Inspector-Einstellungen

### Weapon Component

```
Animation:
  ? Use Animation      // Animation aktivieren
  Aim Parameter Name: "IsAiming" // Name des Bool/Float Parameters
  Fire Trigger Name: "Fire"      // Name des Trigger Parameters
  ? Use Bool For Aim  // Bool (true/false) oder Float (0-1)
  ? Use Trigger For Fire        // Trigger für Fire Animation
```

### Animatable Component (auf Parent)

```
Animation Parameters:
  Move Parameter Name: "IsMoving"
  Move Speed Parameter Name: "MoveSpeed"
  // ... Aim und Fire werden von Weapon gesteuert

Animation Settings:
  ? Use Bool Parameter
  ? Use Speed Parameter
  Movement Speed Multiplier: 1.0
```

## Debugging

### Debug Logs

Bei `useAnimation = true` werden folgende Logs ausgegeben:

**Initialisierung:**
```
? Weapon 'MainGun': Animation support enabled (Aim: IsAiming, Fire: Fire)
```

**Bei fehlender Animatable:**
```
?? Weapon 'MainGun': Animation enabled but no Animatable component found on parent
```

### Console-Logs beobachten

**Erfolgreiche Animation:**
```
? Weapon 'MainGun': Animation support enabled (Aim: IsAiming, Fire: Fire)
?? UnitName: Animation state changed - IsAiming: true
?? MainGun FIRED at Enemy!
```

### Häufige Probleme

#### Animation spielt nicht ab

**Problem:** Weapon hat Ziel aber keine Aim Animation

**Lösungen:**
1. ? Prüfe ob `Animatable` auf **Parent GameObject** ist (nicht auf Weapon!)
2. ? Prüfe ob Parameter "IsAiming" im Animator existiert
3. ? Prüfe ob `Use Animation` aktiviert ist
4. ? Prüfe Transitions im Animator

**Diagnose:**
```csharp
Weapon weapon = unit.GetComponentInChildren<Weapon>();
Animatable anim = weapon.GetAnimatable();

if (anim == null)
{
    Debug.LogError("Weapon has no Animatable on parent!");
}
```

#### Fire Animation triggert nicht

**Problem:** Schuss wird abgegeben aber keine Animation

**Lösungen:**
1. ? Prüfe ob "Fire" Trigger im Animator existiert
2. ? Prüfe Transition von "Any State" zu "Firing"
3. ? Prüfe ob `Use Trigger For Fire` aktiviert ist
4. ? Stelle sicher Fire State hat "Exit Time" für Rückkehr

#### Weapon findet Animatable nicht

**Problem:** Console zeigt "no Animatable component found on parent"

**Lösung:**
- Animatable muss auf dem **Unit GameObject** sein
- Weapon verwendet `GetComponentInParent<Animatable>()`
- Funktioniert auch wenn Weapon ein Child ist

**Hierarchie prüfen:**
```
Unit (mit Animatable) ?
??? Weapon ?
    ??? Turret
        ??? ShotPoint

NICHT:
Weapon (mit Animatable) ? - Falsch!
```

## Best Practices

### 1. Animatable auf Unit, nicht auf Weapon

```csharp
// ? Richtig
Unit
??? Animatable
??? Weapon

// ? Falsch
Weapon
??? Animatable
```

### 2. Kurze Fire Animationen

Fire Animation sollte kurz sein (0.2-0.5s):
- Lange Animationen blockieren schnelles Schießen
- Exit Time aktivieren für automatische Rückkehr
- Oder "Fire" State hat Transition zurück mit Exit Time

### 3. Blend Trees für glatte Übergänge

Verwende Blend Trees wenn Unit sich bewegt während sie schießt:
- Movement + Aim kombiniert
- Smooth blending basierend auf MoveSpeed

### 4. Use Bool für Aim, Trigger für Fire

**Empfohlen:**
- `IsAiming`: Bool (persistenter State)
- `Fire`: Trigger (einmaliges Event)

**Warum:**
- Bool für dauerhafte Zustände (Aim hält an)
- Trigger für Events (Schuss ist momentan)

### 5. Optional: Separate Aim/Fire Layers

Für komplexe Animationen:
- Base Layer: Movement
- Aim Layer: Aiming (Override, Weight 0-1)
- Fire Layer: Firing (Additive)

## Checkliste: Weapon Animation Setup

### Minimum Setup
- [ ] Unit hat `Animator` Component
- [ ] Unit hat `Animatable` Component
- [ ] Unit hat `Weapon` Component (oder Child)
- [ ] Animator Controller zugewiesen
- [ ] Parameter "IsAiming" (bool) existiert
- [ ] Parameter "Fire" (trigger) existiert
- [ ] Weapon > Animation > Use Animation ?
- [ ] Aiming State mit Animation
- [ ] Firing State mit Animation
- [ ] Transitions konfiguriert

### Erweitert (Optional)
- [ ] Blend Trees für Movement + Aiming
- [ ] Separate Animation Layers
- [ ] Custom Fire Animations (Recoil, Muzzle Flash)
- [ ] Animation Events für Sound/VFX Timing

## Performance

### Optimierungen

**Weapon Animation System ist performant:**
- ? Prüft nur bei State-Änderung (nicht jeden Frame)
- ? Verwendet Parameter Hashing (Animatable)
- ? Nur Update wenn `currentTarget != null`
- ? Trigger nur bei tatsächlichem Schuss

**Update-Frequenz:**
```csharp
// In Weapon.Update():
if (useAnimation && animatable != null)
{
    bool isAiming = currentTarget != null;
    
    // Nur update wenn State sich ändert
    if (isAiming != wasAiming)
    {
        animatable.SetBool(aimParameterName, isAiming);
        wasAiming = isAiming;
  }
}
```

## Integration mit anderen Systemen

### WeaponController

`WeaponController` setzt automatisch Targets:
```csharp
WeaponController weaponController = unit.GetComponent<WeaponController>();
// WeaponController ruft automatisch weapon.SetTarget() auf
// ? Weapon setzt automatisch IsAiming Animation
```

### Controllable (Movement)

Movement und Weapon Animations arbeiten zusammen:
```csharp
// Controllable setzt IsMoving
controllable.MoveTo(destination);
// IsMoving = true

// Weapon setzt IsAiming wenn Ziel vorhanden
weapon.SetTarget(enemy);
// IsAiming = true

// Beide Animationen können gleichzeitig laufen!
```

### AnimatedCombatUnit

Beispiel-Integration:
```csharp
public class AnimatedCombatUnit : MonoBehaviour
{
    private Animatable animatable;
    private Weapon weapon;
    
    void Update()
    {
        // Weapon managed Aim/Fire automatisch
        // Animatable empfängt alle Parameter
    
        // Custom: Health-basierte Animation
        float healthPercent = health / maxHealth;
   animatable.SetFloat("Health", healthPercent);
    }
}
```

## Zusammenfassung

### Automatisch (Kein Code nötig):
- ? `IsAiming` wird bei Target-Änderung gesetzt
- ? `Fire` wird bei jedem Schuss getriggert
- ? Integration mit Controllable Movement
- ? Works with WeaponController Target Acquisition

### Konfiguration (Inspector):
- ? Aktivierung: Use Animation
- ? Parameter-Namen anpassbar
- ? Bool oder Float für Aim
- ? Trigger für Fire

### Flexibilität (Code):
- ? Manuelle Kontrolle möglich
- ? Custom Animations mischbar
- ? Temporär deaktivierbar
- ? Zugriff auf Animatable

Das System ist vollständig integriert und einsatzbereit! ??

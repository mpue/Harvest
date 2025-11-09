# ?? RTS Weapon System - Dokumentation

## Übersicht

Ein modulares Waffensystem mit automatischem Zielen (Auto-Aim), Turret-Rotation und Team-basiertem Combat.

## ?? Komponenten

### 1. **TeamComponent.cs**
Definiert Team/Fraktion-Zugehörigkeit für Einheiten.

**Features:**
- Team-Enum: Player, Enemy, Ally, Neutral
- Freund/Feind-Erkennung
- Team-Farben (optional für visuelle Kennzeichnung)

**Setup:**
```csharp
// Füge zu jeder Unit hinzu
TeamComponent team = unit.AddComponent<TeamComponent>();
team.SetTeam(Team.Player); // oder Enemy, Ally, Neutral
```

---

### 2. **Weapon.cs**
Einzelne Waffe mit Auto-Aim und Projektil-Spawning.

**Features:**
- ? **Auto-Aim**: Turret dreht sich automatisch zum Ziel
- ? **Dual-Axis Rotation**: Turret (horizontal) + Barrel (vertikal)
- ? **Multiple Shot Points**: Unterstützt mehrere Rohre/Geschütze
- ? **Fire Rate Control**: Schussrate einstellbar
- ? **Range System**: Reichweiten-Begrenzung
- ? **Team Check**: Schießt nur auf Feinde
- ? **Visual/Audio**: Muzzle Flash, Sound Effects

**Hierarchy Setup:**
```
Tank (GameObject)
??? BaseUnit
??? TeamComponent
??? WeaponController
??? Weapon (GameObject)
    ??? Weapon (Script)
    ??? Turret (Transform) ? Dreht sich horizontal
    ?   ??? Barrel (Transform) ? Dreht sich vertikal
    ?       ??? ShotPoint_Left (Transform)
    ?       ??? ShotPoint_Right (Transform)
    ??? MuzzleFlash (ParticleSystem)
```

**Inspector Settings:**

| Setting | Beschreibung | Beispiel |
|---------|--------------|----------|
| **Weapon Name** | Name der Waffe | "Tank Cannon" |
| **Damage** | Schaden pro Treffer | 25 |
| **Fire Rate** | Schüsse pro Sekunde | 1.0 |
| **Range** | Max. Reichweite | 30m |
| **Projectile Speed** | Geschwindigkeit | 40 m/s |
| **Turret Transform** | Turm-Transform (horizontal) | Turret GameObject |
| **Barrel Transform** | Rohr-Transform (vertikal) | Barrel GameObject |
| **Turret Rotation Speed** | Drehgeschwindigkeit | 90°/s |
| **Shot Points** | Array von Spawn-Punkten | ShotPoint_Left, ShotPoint_Right |
| **Projectile Prefab** | Projektil Prefab | Bullet Prefab |

---

### 3. **WeaponController.cs**
Verwaltet mehrere Waffen, Target Acquisition und Firing Logic.

**Features:**
- ? **Auto Target Acquisition**: Findet automatisch Feinde in Reichweite
- ? **Multi-Weapon Support**: Steuert mehrere Waffen gleichzeitig
- ? **Priority Targeting**: Nächstes oder spezifisches Ziel
- ? **Auto Fire**: Automatisches Feuern auf Ziele
- ? **Team Filtering**: Nur Feinde werden angegriffen

**Inspector Settings:**

| Setting | Beschreibung | Empfohlen |
|---------|--------------|-----------|
| **Weapons** | Array von Weapon-Scripts | Auto-detect |
| **Auto Acquire Targets** | Automatisch Ziele suchen | ? |
| **Target Scan Interval** | Wie oft scannen (Sekunden) | 0.5s |
| **Target Layer Mask** | Layer für Ziele | "Selectable" |
| **Prioritize Closest Target** | Nächstes Ziel bevorzugen | ? |
| **Auto Fire** | Automatisch schießen | ? |
| **Fire Interval** | Pause zwischen Waffen-Feuern | 0.1s |

---

### 4. **Projectile.cs**
Projektil das sich bewegt und Schaden verursacht.

**Features:**
- ? Physics-basierte Bewegung
- ? Reichweiten-Limit
- ? Team-Check (keine Friendly Fire)
- ? Impact Effects (Partikel, Sound)
- ? Trail Renderer Support
- ? Optional: Gravity

**Prefab Setup:**
```
Projectile Prefab
??? Projectile (Script)
??? Rigidbody (keine Gravity)
??? Collider (Sphere/Box, isTrigger = true)
??? Trail Renderer (optional)
??? Mesh/Sprite (Visual)
```

**Inspector Settings:**

| Setting | Beschreibung | Beispiel |
|---------|--------------|----------|
| **Lifetime** | Max. Lebenszeit | 5s |
| **Use Gravity** | Physik-Gravity | ? (für Laser/Bullets) |
| **Destroy On Impact** | Bei Treffer zerstören | ? |
| **Impact Effect Prefab** | Explosion/Impact VFX | Explosion Prefab |
| **Impact Sound** | Treffer-Sound | ImpactSound.wav |

---

## ??? Setup Guide

### Schritt 1: Basis Unit mit Team

```csharp
GameObject tank = ...; // Dein Tank GameObject

// Komponenten hinzufügen
BaseUnit baseUnit = tank.AddComponent<BaseUnit>();
TeamComponent team = tank.AddComponent<TeamComponent>();
Controllable controllable = tank.AddComponent<Controllable>();

// Team konfigurieren
team.SetTeam(Team.Player); // oder Enemy
```

### Schritt 2: Weapon Setup

**Erstellen Sie die Hierarchy:**

1. **Turret GameObject** (Kind von Tank):
   - Position: Wo der Turm ist
   - Rotation: (0, 0, 0)

2. **Barrel GameObject** (Kind von Turret):
   - Position: Wo das Rohr ist
   - Rotation: (0, 0, 0)

3. **ShotPoint(s)** (Kind von Barrel):
   - Position: Mündung des Rohrs
   - Rotation: Forward = Schussrichtung

**Add Weapon Script:**

```csharp
GameObject weaponObj = new GameObject("Weapon");
weaponObj.transform.SetParent(tank.transform);

Weapon weapon = weaponObj.AddComponent<Weapon>();

// Referenzen zuweisen (im Inspector oder Code)
weapon.turretTransform = turretTransform;
weapon.barrelTransform = barrelTransform;
weapon.shotPoints = new Transform[] { shotPoint1, shotPoint2 };
weapon.projectilePrefab = projectilePrefab;
```

### Schritt 3: WeaponController hinzufügen

```csharp
WeaponController weaponController = tank.AddComponent<WeaponController>();
// Weapons werden automatisch gefunden (GetComponentsInChildren)
```

### Schritt 4: Projectile Prefab erstellen

**Im Unity Editor:**

1. Create ? 3D Object ? Sphere (Scale: 0.2, 0.2, 0.2)
2. Add Component ? Rigidbody
   - Use Gravity: ?
   - Is Kinematic: ?
3. Add Component ? Sphere Collider
   - Is Trigger: ?
4. Add Component ? Projectile Script
5. Optional: Add Trail Renderer
6. Save as Prefab: "Bullet"

**Assign im Weapon:**
- Projectile Prefab ? Bullet Prefab

---

## ?? Verwendung

### Automatischer Combat (Empfohlen)

```csharp
// Setup - einmal bei Start
GameObject tank = CreateTank();
tank.AddComponent<TeamComponent>().SetTeam(Team.Player);
tank.AddComponent<WeaponController>(); // Auto-Acquire & Auto-Fire = true

// Das war's! Tank schießt automatisch auf Feinde in Reichweite
```

### Manueller Combat

```csharp
WeaponController weaponController = tank.GetComponent<WeaponController>();

// Deaktiviere Auto-Features
weaponController.SetAutoAcquireTargets(false);
weaponController.SetAutoFire(false);

// Manuell Ziel setzen
Transform enemy = FindEnemy();
weaponController.SetTarget(enemy);

// Manuell feuern
weaponController.FireAtTarget(enemy);

// Feuer einstellen
weaponController.CeaseFire();
```

### Target Acquisition mit Raycast

```csharp
// Rechtsklick auf Feind = Angreifen
if (Input.GetMouseButtonDown(1))
{
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    RaycastHit hit;
    
    if (Physics.Raycast(ray, out hit))
    {
 BaseUnit targetUnit = hit.collider.GetComponent<BaseUnit>();
        TeamComponent targetTeam = hit.collider.GetComponent<TeamComponent>();
    
        if (targetUnit != null && targetTeam != null)
        {
  // Prüfe ob Feind
       TeamComponent myTeam = GetComponent<TeamComponent>();
            if (myTeam.IsEnemy(targetTeam))
            {
  // Angriff!
     WeaponController wc = GetComponent<WeaponController>();
    wc.SetTarget(targetUnit.transform);
            }
     }
    }
}
```

---

## ?? Integration mit UnitSelector

Update für **Attack-Move** Command:

```csharp
// In UnitSelector.cs - erweitern Sie HandleMoveCommand():

private void HandleMoveCommand()
{
 Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
    RaycastHit hit;

 if (Physics.Raycast(ray, out hit, Mathf.Infinity))
    {
  // Check if clicked on enemy unit
 BaseUnit targetUnit = hit.collider.GetComponent<BaseUnit>();
        TeamComponent targetTeam = hit.collider.GetComponent<TeamComponent>();
        
        if (targetUnit != null && targetTeam != null)
      {
 // Check if enemy
            bool isEnemy = false;
            foreach (BaseUnit selectedUnit in selectedUnits)
      {
   TeamComponent myTeam = selectedUnit.GetComponent<TeamComponent>();
     if (myTeam != null && myTeam.IsEnemy(targetTeam))
     {
         isEnemy = true;
       break;
     }
         }
         
   if (isEnemy)
  {
                // ATTACK COMMAND
                foreach (BaseUnit unit in selectedUnits)
      {
              WeaponController wc = unit.GetComponent<WeaponController>();
        if (wc != null)
            {
        wc.SetTarget(targetUnit.transform);
       }
 }
            return; // Don't move, just attack
            }
 }
        
    // Normal move command (existing code)
        // ...
    }
}
```

---

## ?? Erweiterte Features

### 1. Multiple Weapons auf einem Tank

```csharp
Tank
??? WeaponController
??? MainCannon (Weapon) - Hoher Schaden, langsam
?   ??? Turret ? Barrel ? ShotPoint
??? MachineGun (Weapon) - Niedriger Schaden, schnell
    ??? Turret ? Barrel ? ShotPoints (links & rechts)
```

Beide Waffen feuern automatisch auf das gleiche Ziel!

### 2. Verschiedene Projektil-Typen

**Rocket (mit Gravity):**
```csharp
// Projectile Script
useGravity = true;
```

**Laser (instant, kein Projektil):**
```csharp
// Eigene Weapon-Klasse
public class LaserWeapon : Weapon
{
 protected override void Fire()
    {
  // Instantaner Raycast statt Projektil
  RaycastHit hit;
    if (Physics.Raycast(shotPoint.position, shotPoint.forward, out hit, range))
{
         // Sofortiger Treffer
            DamageTarget(hit.collider);
        }
   
        // Laser-Linie zeichnen
        DrawLaser(shotPoint.position, hit.point);
 }
}
```

### 3. Homing Missiles

```csharp
public class HomingProjectile : Projectile
{
    [SerializeField] private float homingStrength = 5f;
 private Transform target;
    
    public void SetTarget(Transform target)
    {
  this.target = target;
    }
    
    void FixedUpdate()
    {
  if (target != null)
        {
   Vector3 direction = (target.position - transform.position).normalized;
          rb.velocity = Vector3.Lerp(rb.velocity, direction * speed, homingStrength * Time.fixedDeltaTime);
            transform.rotation = Quaternion.LookRotation(rb.velocity);
        }
  }
}
```

### 4. Burst Fire

```csharp
public class BurstWeapon : Weapon
{
    [SerializeField] private int burstCount = 3;
    [SerializeField] private float burstDelay = 0.1f;
    
    protected override void Fire()
 {
        StartCoroutine(BurstFireCoroutine());
    }
    
IEnumerator BurstFireCoroutine()
    {
        for (int i = 0; i < burstCount; i++)
        {
base.Fire();
yield return new WaitForSeconds(burstDelay);
        }
    }
}
```

---

## ?? Visual Effects Setup

### Muzzle Flash (Partikel)

1. Create ? Effects ? Particle System
2. Parent: Barrel GameObject
3. Position: ShotPoint position
4. Settings:
   - Duration: 0.1
   - Looping: ?
   - Play On Awake: ?
   - Start Lifetime: 0.1
   - Start Speed: 0
   - Start Size: 0.5
   - Emission Rate: 50
   - Shape: Cone

5. Im Weapon Script:
   - Muzzle Flash ? ParticleSystem Referenz

### Impact Effect

1. Create Explosion Prefab mit Particle System
2. Optional: Add Light Component (flackern)
3. Im Projectile Script:
   - Impact Effect Prefab ? Explosion Prefab

### Trail Renderer

1. Auf Projectile Prefab: Add Component ? Trail Renderer
2. Settings:
   - Time: 0.3s
   - Width: 0.1 ? 0.01
   - Material: Trail Material
   - Color: Gradient (hell ? transparent)

---

## ?? Performance Tipps

### Object Pooling für Projektile

```csharp
public class ProjectilePool : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int poolSize = 50;
    
    private Queue<GameObject> pool = new Queue<GameObject>();
    
    void Start()
    {
   for (int i = 0; i < poolSize; i++)
    {
            GameObject proj = Instantiate(projectilePrefab);
     proj.SetActive(false);
     pool.Enqueue(proj);
        }
    }
 
    public GameObject GetProjectile()
    {
     if (pool.Count > 0)
      {
            GameObject proj = pool.Dequeue();
  proj.SetActive(true);
            return proj;
        }
        return Instantiate(projectilePrefab);
    }

    public void ReturnProjectile(GameObject proj)
    {
        proj.SetActive(false);
        pool.Enqueue(proj);
    }
}
```

### Target Acquisition Optimization

```csharp
// In WeaponController
[SerializeField] private float targetScanInterval = 0.5f; // Nicht jeden Frame

// Für viele Units: Spatial Partitioning
// Nur Units in Nähe scannen
```

---

## ?? Troubleshooting

### Waffe dreht sich nicht

**Problem:** Turret Transform nicht zugewiesen
**Lösung:**
```csharp
// Im Inspector: Weapon ? Turret Transform ? Ziehe Turret GameObject rein
```

### Projektile treffen eigene Einheiten

**Problem:** Team Check fehlt oder falsch
**Lösung:**
```csharp
// Stelle sicher dass beide Units TeamComponent haben
// Überprüfe Team-Einstellungen
```

### Waffe feuert nicht

**Checkliste:**
- ? Target ist gesetzt
- ? Target ist in Range
- ? Turret is aimed (isAimed = true)
- ? Target ist Feind (Team Check)
- ? Fire Rate cooldown abgelaufen
- ? Projectile Prefab zugewiesen
- ? Shot Points vorhanden

**Debug:**
```csharp
// In Weapon.cs, TryFire():
Debug.Log($"Can fire: {CanFire()}");
Debug.Log($"Has target: {currentTarget != null}");
Debug.Log($"In range: {IsTargetInRange(currentTarget)}");
Debug.Log($"Is aimed: {isAimed}");
```

### Projektile gehen durch Wände

**Problem:** Collider zu klein oder fehlend
**Lösung:**
```csharp
// Projectile Prefab:
// - Collider Is Trigger = true
// - Rigidbody Collision Detection = Continuous Dynamic
// - Layer: "Projectile"
// - Walls Layer: Nicht in "Target Layer Mask"
```

---

## ?? Nächste Schritte

Das Weapon System ist bereit für:

? **Health System** - Units können Schaden nehmen und sterben
? **Damage Types** - Armor System (Kinetic, Energy, Explosive)
? **Special Weapons** - EMP, Area-of-Effect, Beam Weapons
? **Unit Abilities** - Special Attacks, Shield, Cloak
? **AI Behavior** - Defensive/Aggressive Stances
? **Formation Combat** - Units kämpfen in Formation

---

## ?? Quick Reference

### Minimal Setup für Combat-fähige Unit:

```csharp
1. BaseUnit ?
2. TeamComponent ?
3. WeaponController ?
4. Weapon (child GameObject) ?
 - Turret Transform ?
   - Shot Point ?
   - Projectile Prefab ?
5. Layer "Selectable" ?
```

### Schieß-Logik Flowchart:

```
WeaponController.Update()
  ?
Auto Acquire Target? 
  ? (Scan every 0.5s)
Find nearest enemy in range
  ?
Set target for all weapons
  ?
Auto Fire?
  ?
Weapon.TryFire()
?
Check: Can fire? In range? Aimed? Is enemy?
  ? (All YES)
Spawn Projectile at ShotPoint
  ?
Projectile flies ? OnTriggerEnter ? Damage target
```

---

**Viel Erfolg mit dem Combat System! ????**

# ?? Weapon System - Quick Start

## ?? Schnellstart (3 Minuten)

### Option 1: Automatisches Setup mit Helper

1. **WeaponSetupHelper erstellen:**
   - Create Empty GameObject: "WeaponSetup"
   - Add Component ? WeaponSetupHelper

2. **Test Combat Units erstellen:**
 - Im Inspector: Rechtsklick auf WeaponSetupHelper Script
   - Wähle "Create Test Combat Unit" (Player unit)
   - Wähle "Create Enemy Test Unit" (Enemy unit)

3. **Projektil erstellen:**
   - Rechtsklick auf Script ? "Create Projectile Prefab"
   - GameObject "Projectile" erscheint in Szene
   - Ziehe es in Project Fenster um Prefab zu erstellen
   - Lösche GameObject aus Szene

4. **Projektil zuweisen:**
   - Wähle "Combat Unit" ? Weapon (Child)
   - Inspector: Weapon Component
   - Projectile Prefab ? Ziehe Projectile Prefab rein

5. **Layer konfigurieren:**
   - Wähle beide Units
   - Layer: "Selectable"
   - WeaponController ? Target Layer Mask: "Selectable"

6. **Testen:**
   - Play drücken
   - Units schießen automatisch aufeinander! ??

---

### Option 2: Manuelles Setup

#### Schritt 1: Unit mit Team

```
Tank GameObject
??? BaseUnit ?
??? TeamComponent ? (Set Team: Player oder Enemy)
??? Controllable ?
??? Collider ?
```

#### Schritt 2: Turret Hierarchy erstellen

```
Tank
??? Weapon (Empty GameObject)
 ??? Turret (Empty GameObject)
    ??? Barrel (Empty GameObject)
            ??? ShotPoint (Empty GameObject)
```

**Positionen (Beispiel):**
- Weapon: (0, 0, 0)
- Turret: (0, 1, 0) - Auf Tank-Oberseite
- Barrel: (0, 0.5, 1) - Vorne am Turm
- ShotPoint: (0, 0, 0.5) - Mündung des Rohrs

#### Schritt 3: Weapon Script hinzufügen

1. Wähle "Weapon" GameObject
2. Add Component ? Weapon
3. Konfiguriere im Inspector:
   - Turret Transform ? Turret GameObject
 - Barrel Transform ? Barrel GameObject
   - Shot Points ? Array Size: 1 ? ShotPoint GameObject
   - Projectile Prefab ? (erstellen im nächsten Schritt)

#### Schritt 4: Projectile Prefab

1. **Erstellen:**
   - Create ? 3D Object ? Sphere
   - Scale: (0.2, 0.2, 0.2)
   - Name: "Bullet"

2. **Komponenten:**
   - Add Component ? Rigidbody
     - Use Gravity: ?
 - Sphere Collider bereits vorhanden
     - Is Trigger: ?
   - Add Component ? Projectile

3. **Material:**
   - Create ? Material ? "BulletMaterial"
   - Albedo: Gelb
   - Emission: Gelb (aktivieren)
- Zuweisen

4. **Als Prefab speichern:**
   - Ziehe "Bullet" GameObject in Project Fenster
   - Lösche GameObject aus Szene

5. **Zuweisen:**
   - Weapon Component ? Projectile Prefab ? Bullet Prefab

#### Schritt 5: WeaponController

1. Wähle Tank GameObject
2. Add Component ? WeaponController
3. Weapons Array wird automatisch gefüllt
4. Konfiguriere:
   - Auto Acquire Targets: ?
   - Auto Fire: ?
   - Target Layer Mask: "Selectable"

---

## ?? Konfiguration

### Weapon Settings

```
Typische Werte für verschiedene Waffentypen:
```

| Waffe | Damage | Fire Rate | Range | Speed |
|-------|--------|-----------|-------|-------|
| **Tank Cannon** | 50 | 0.5 (langsam) | 40m | 50 m/s |
| **Machine Gun** | 5 | 10 (schnell) | 25m | 80 m/s |
| **Artillery** | 100 | 0.2 (sehr langsam) | 60m | 30 m/s |
| **Anti-Air** | 15 | 5 | 50m | 100 m/s |
| **Laser** | 20 | 2 | 30m | 200 m/s |

### WeaponController Settings

**Standard RTS Setup:**
```
Auto Acquire Targets: ?
Target Scan Interval: 0.5s
Prioritize Closest Target: ?
Auto Fire: ?
Fire Interval: 0.1s
```

**Manueller Angriff (Spieler-gesteuert):**
```
Auto Acquire Targets: ?
Auto Fire: ?
(Dann per Code: weaponController.SetTarget(enemy))
```

---

## ?? Gameplay Integration

### Attack-Move Command

Fügen Sie in **UnitSelector.cs** hinzu:

```csharp
private void HandleMoveCommand()
{
    Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
    RaycastHit hit;

    if (Physics.Raycast(ray, out hit, Mathf.Infinity))
    {
   // Check if clicked on enemy
     BaseUnit clickedUnit = hit.collider.GetComponent<BaseUnit>();
 TeamComponent clickedTeam = hit.collider.GetComponent<TeamComponent>();
  
        if (clickedUnit != null && clickedTeam != null)
        {
  // Check if it's an enemy
            foreach (BaseUnit unit in selectedUnits)
       {
       TeamComponent myTeam = unit.GetComponent<TeamComponent>();
 if (myTeam != null && myTeam.IsEnemy(clickedTeam))
      {
      // ATTACK!
        WeaponController wc = unit.GetComponent<WeaponController>();
  if (wc != null)
           {
           wc.SetTarget(clickedUnit.transform);
 }
         }
  }
            return;
        }
        
        // Normal move (existing code)
        // ...
    }
}
```

### Stop Firing (S-Taste)

```csharp
void Update()
{
    // In UnitSelector oder InputManager
    if (Input.GetKeyDown(KeyCode.S))
    {
        foreach (BaseUnit unit in selectedUnits)
        {
       WeaponController wc = unit.GetComponent<WeaponController>();
            if (wc != null)
 {
        wc.CeaseFire();
   }
        }
    }
}
```

---

## ?? Visual Enhancements

### Muzzle Flash (Partikel)

1. **Erstellen:**
   - Wähle Barrel GameObject
   - Right Click ? Effects ? Particle System
 - Name: "MuzzleFlash"
   - Position: Am ShotPoint

2. **Settings:**
   - Duration: 0.1
   - Looping: ?
- Play On Awake: ?
- Start Lifetime: 0.1
   - Start Size: 0.5
   - Emission: 30

3. **Zuweisen:**
   - Weapon ? Muzzle Flash ? MuzzleFlash GameObject

### Impact Effect

1. **Erstellen:**
   - Create Empty: "ImpactEffect"
- Add Particle System
   - Settings: Explosion-ähnlich
   - Optional: Add Light (orange, flackern)

2. **Als Prefab speichern**

3. **Zuweisen:**
   - Projectile Prefab ? Impact Effect Prefab ? ImpactEffect

### Sounds

1. **Feuer-Sound:**
   - Import .wav/.mp3 Datei
   - Weapon ? Fire Sound ? Sound Datei

2. **Impact Sound:**
   - Projectile ? Impact Sound ? Sound Datei

---

## ?? Troubleshooting

### "Turret dreht sich nicht"
```
? Turret Transform zugewiesen?
? Target ist gesetzt?
? WeaponController: Auto Acquire = true?
```

### "Waffe feuert nicht"
```
Prüfe in Console:
- "Acquired target: ..." ? Target gefunden ?
- Nichts ? Keine Ziele gefunden
  ? Layer Mask prüfen!
  ? Team Settings prüfen!
```

**Debug Log hinzufügen:**
```csharp
// In WeaponController.OnTargetAcquired()
Debug.Log($"Target acquired: {target.name}, Distance: {Vector3.Distance(transform.position, target.position)}");
```

### "Projektile treffen nichts"
```
? Projectile hat Collider? (Is Trigger = true)
? Target hat Collider?
? Teams sind unterschiedlich? (Player vs Enemy)
? Layer nicht ignoriert in Physics Settings?
```

### "Units schießen auf Verbündete"
```
? Beide Units haben TeamComponent?
? Teams sind korrekt gesetzt? (Player vs Enemy)
? WeaponController findet ownerTeam?
   ? Muss auf GLEICHEM GameObject sein wie TeamComponent!
```

---

## ?? Checkliste - Combat Ready Unit

```
? BaseUnit Component
? TeamComponent (Team gesetzt)
? Controllable Component
? Collider (für Selektion & Treffer)
? Layer "Selectable"
? WeaponController Component
  ? Auto Acquire = true
  ? Auto Fire = true
  ? Target Layer Mask = "Selectable"
? Weapon (child GameObject)
  ? Turret Transform zugewiesen
  ? Shot Point(s) vorhanden
  ? Projectile Prefab zugewiesen
  ? Damage/Range/FireRate konfiguriert
? Projectile Prefab existiert
  ? Rigidbody
  ? Collider (Trigger)
  ? Projectile Script
```

---

## ?? Testing Scenario

### Quick Test:

1. **Zwei Combat Units erstellen:**
   - Player Unit bei (0, 0, 0)
   - Enemy Unit bei (10, 0, 0)

2. **Play drücken:**
   - Units sollten sich automatisch anvisieren
   - Turrets drehen sich
   - Schüsse fliegen
   - Console zeigt "Target acquired"

3. **Ergebnis:**
   - Units schießen aufeinander ?
   - Projektile spawnen ?
   - Turrets rotieren ?

### Advanced Test:

```csharp
// Test Script um 10 Player vs 10 Enemy Units zu spawnen
for (int i = 0; i < 10; i++)
{
    // Player side
    GameObject player = CreateCombatUnit(Team.Player);
    player.transform.position = new Vector3(-20 + i * 2, 0, 0);
    
    // Enemy side
    GameObject enemy = CreateCombatUnit(Team.Enemy);
    enemy.transform.position = new Vector3(20 - i * 2, 0, 0);
}
```

---

## ?? Nächste Schritte

**Das System ist bereit für:**

? Health System (Units können sterben)
? Damage Types (Armor, Shields)
? Special Weapons (AoE, Beam, Missiles)
? AI Behavior (Aggressive/Defensive)
? Cover System
? Fog of War

**Vorgeschlagene Reihenfolge:**
1. Health System (Units nehmen Schaden & sterben)
2. Attack-Move Command (Rechtsklick auf Feind)
3. Combat UI (Health Bars, Damage Numbers)
4. Sound & VFX Polish

---

**Happy Shooting! ????**

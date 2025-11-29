# Unit Editor Window - Vollständiges Setup-Guide

## Erforderliche Komponenten für spielfähige Unit

### Minimum-Anforderungen:
1. **BaseUnit** - Basis-Komponente
2. **TeamComponent** - Team-Zugehörigkeit  
3. **TeamVisualIndicator** - Visuelle Team-Anzeige
4. **Health** - Lebenspunkte
5. **Controllable** - Bewegungssteuerung (nicht für Buildings)
6. **WeaponController** - Kampfsystem
- Mit mindestens einem **Weapon** Child

### Hierarchie-Struktur:

```
UnitPrefab (GameObject)
??? BaseUnit Component
??? TeamComponent Component
??? TeamVisualIndicator Component
??? Health Component
??? Controllable Component
??? WeaponController Component
??? NavMeshAgent Component
??? Collider Component
??? Rigidbody Component
?
??? Model (GameObject) - 3D Model
?   ??? Visual Meshes
?   ??? ShotPoint (Transform)
?   ??? TurretTransform (Transform, optional)
?
??? Weapon (GameObject) - Child!
?   ??? Weapon Component
?       - shotPoints: ShotPoint Reference
?   - turretTransform: Turret Reference (optional)
?   - projectilePrefab: Assigned
?
??? HealthBar (GameObject) - Child!
?   ??? HealthBar Component
?       - healthComponent: Health Reference
?
??? SelectionIndicator (GameObject) - Child!
    ??? Visual Indicator Mesh
        - Referenced in BaseUnit.selectionIndicator
```

## Layer-Konfiguration

### Empfohlene Layer:
- **Default** (0) - Standard
- **Unit** (8) - Alle Units
- **Enemy** (9) - Feindliche Units
- **Player** (10) - Spieler Units
- **Building** (11) - Gebäude
- **Ground** (12) - Boden/Terrain

### Layer-Zuweisung:
```
Player Team Unit ? Layer: Player (10)
Enemy Team Unit ? Layer: Enemy (9)
Building ? Layer: Building (11)
```

## Team-Konfiguration

### Teams (Enum):
- **Player** - Spieler-Team
- **Enemy** - Gegner-Team
- **Neutral** - Neutral
- **Ally** - Verbündete

### Team-Einstellungen:
```csharp
TeamComponent:
  team: Player / Enemy / Neutral / Ally
  teamColor: Farbe für visuelle Anzeige
```

## WeaponController Setup

### WeaponController Konfiguration:
```
WeaponController:
  weapons: [Weapon] Array
  autoAcquireTargets: true
  targetScanInterval: 0.5
  targetLayerMask: Enemy Layer
  autoFire: true
```

### Weapon Child Setup:
```
Weapon GameObject (Child von Unit):
  Weapon Component:
  weaponName: "Main Gun"
    damage: 10
    fireRate: 1
    range: 20
    projectileSpeed: 30
    turretTransform: Reference zu Model/Turret
 shotPoints: [ShotPoint] Array
    projectilePrefab: Projectile Prefab
```

## HealthBar Setup

### HealthBar GameObject:
```
HealthBar (Child):
  Position: (0, 2, 0) - Über Unit
  HealthBar Component:
    healthComponent: Reference zu Health
    offset: Vector3(0, 2, 0)
    fillColor: Color.green
    backgroundColor: Color.red
    barWidth: 1.0
    barHeight: 0.1
```

## SelectionIndicator Setup

### SelectionIndicator GameObject:
```
SelectionIndicator (Child):
  Position: (0, 0.05, 0) - Am Boden
  Rotation: (90, 0, 0) - Flach
  Scale: (1, 0.05, 1)
  
  Mesh: Cylinder/Plane
  Material: Emissive/Transparent
  Color: Team Color with Alpha
  
  BaseUnit.selectionIndicator: Reference!
  Initial Active: false
```

## Vollständiges Setup-Prozedere

### Schritt 1: Basis-GameObject erstellen
```
GameObject > Create Empty
Name: "MyUnit"
```

### Schritt 2: Essential Components hinzufügen
```
1. Add Component > BaseUnit
   - unitName: "MyUnit"
   - isBuilding: false
   
2. Add Component > TeamComponent
   - team: Player
   - teamColor: Blue
   
3. Add Component > TeamVisualIndicator
   - indicatorType: ColorRing
   - showIndicator: true
   
4. Add Component > Health
   - maxHealth: 100
   - canRegenerate: true
 
5. Add Component > Controllable
 - moveSpeed: 5
 - useNavMesh: true
   
6. Add Component > NavMeshAgent
   - speed: 5
   - angularSpeed: 120
   
7. Add Component > WeaponController
   - autoAcquireTargets: true
   - autoFire: true
```

### Schritt 3: Physics Components
```
8. Add Component > Capsule Collider
   - height: 2
   - radius: 0.5
   
9. Add Component > Rigidbody
   - mass: 1
   - constraints: Freeze Rotation
```

### Schritt 4: Model hinzufügen
```
GameObject > Create Empty (as child)
Name: "Model"
? Füge 3D Model Prefab hinzu
? Erstelle ShotPoint Transform
? Erstelle TurretTransform (optional)
```

### Schritt 5: Weapon erstellen
```
GameObject > Create Empty (as child of Unit)
Name: "Weapon"

Add Component > Weapon
  weaponName: "Main Gun"
  damage: 10
  fireRate: 1
  range: 20
  projectileSpeed: 30
  shotPoints: [Reference zu Model/ShotPoint]
  projectilePrefab: [Projectile Prefab]
  
WeaponController:
  weapons: [Reference zu Weapon GameObject]
```

### Schritt 6: HealthBar erstellen
```
GameObject > UI > Image (as child)
Name: "HealthBar"
Position: (0, 2, 0)

Add Component > HealthBar
  healthComponent: [Reference zu Health]
  offset: (0, 2, 0)
```

### Schritt 7: SelectionIndicator erstellen
```
GameObject > 3D Object > Cylinder (as child)
Name: "SelectionIndicator"
Position: (0, 0.05, 0)
Rotation: (90, 0, 0)
Scale: (1, 0.05, 1)

BaseUnit.selectionIndicator: [Reference zu Indicator]
SetActive: false
```

### Schritt 8: Layer & Tag setzen
```
GameObject:
  Layer: Player (oder Enemy)
  Tag: Untagged (oder custom)
```

### Schritt 9: Prefab erstellen
```
Drag GameObject to Project
? Prefab erstellt
? Prefab Variante für Enemy (anderes Team/Layer)
```

## Unit Editor Window - Verbesserte Features

### Auto-Setup Button:
```
"Setup Complete Playable Unit"
? Fügt ALLE erforderlichen Komponenten hinzu
? Erstellt Hierarchie automatisch
? Konfiguriert Layer & Team
? Erstellt Weapon Child
? Erstellt HealthBar Child
? Erstellt SelectionIndicator Child
? Verknüpft alle References
```

### Validation System:
```
? BaseUnit vorhanden
? TeamComponent vorhanden
? TeamVisualIndicator vorhanden
? Health vorhanden
? Controllable vorhanden (nicht für Buildings)
? WeaponController vorhanden
? Mindestens 1 Weapon Child
? HealthBar vorhanden
? SelectionIndicator vorhanden
? Layer konfiguriert
? Team konfiguriert
? Alle References verknüpft
```

### Quick-Fix Buttons:
```
"Add Missing Components" - Fügt fehlende hinzu
"Create Weapon Child" - Erstellt Weapon GameObject
"Create HealthBar" - Erstellt HealthBar
"Create SelectionIndicator" - Erstellt Indicator
"Configure Layer & Team" - Setzt Layer/Team
"Validate Unit" - Prüft alle Requirements
```

## Checkliste für spielfähige Unit

### Components:
- [ ] BaseUnit
- [ ] TeamComponent
- [ ] TeamVisualIndicator
- [ ] Health
- [ ] Controllable
- [ ] NavMeshAgent
- [ ] WeaponController
- [ ] Collider
- [ ] Rigidbody

### Children:
- [ ] Weapon GameObject mit Weapon Component
- [ ] HealthBar GameObject
- [ ] SelectionIndicator GameObject

### References:
- [ ] BaseUnit.selectionIndicator ? SelectionIndicator
- [ ] WeaponController.weapons ? [Weapon]
- [ ] Weapon.shotPoints ? [ShotPoint]
- [ ] Weapon.projectilePrefab ? Prefab
- [ ] HealthBar.healthComponent ? Health

### Configuration:
- [ ] Layer gesetzt (Player/Enemy)
- [ ] Team gesetzt (Player/Enemy)
- [ ] TeamColor gesetzt
- [ ] Move Speed konfiguriert
- [ ] Health konfiguriert
- [ ] Weapon Damage konfiguriert
- [ ] Weapon Range konfiguriert

### Testing:
- [ ] Unit kann spawnen
- [ ] Unit kann selektiert werden
- [ ] Unit kann sich bewegen
- [ ] Unit kann schießen
- [ ] Unit kann Schaden nehmen
- [ ] HealthBar wird angezeigt
- [ ] SelectionIndicator funktioniert
- [ ] Team-Farbe wird angezeigt

## Troubleshooting

### Problem: Unit kann nicht schießen
**Lösung:**
- Prüfe Weapon Child vorhanden
- Prüfe WeaponController.weapons Array
- Prüfe Weapon.projectilePrefab zugewiesen
- Prüfe Weapon.shotPoints Array
- Prüfe Layer (Enemy Layer für Ziele)

### Problem: HealthBar nicht sichtbar
**Lösung:**
- Prüfe HealthBar GameObject existiert
- Prüfe HealthBar.healthComponent Reference
- Prüfe Position/Offset
- Prüfe Canvas/Scaling

### Problem: SelectionIndicator nicht sichtbar
**Lösung:**
- Prüfe BaseUnit.selectionIndicator Reference
- Prüfe Indicator Material/Color
- Prüfe Position (sollte am Boden sein)
- Prüfe Rotation (90° für flach)

### Problem: Unit bewegt sich nicht
**Lösung:**
- Prüfe Controllable vorhanden
- Prüfe NavMeshAgent vorhanden
- Prüfe NavMesh gebaked in Szene
- Prüfe Rigidbody Constraints

## Zusammenfassung

**Minimum für spielfähige Unit:**
1. 9 Components auf Unit GameObject
2. 3 Child GameObjects (Weapon, HealthBar, Indicator)
3. Layer & Team konfiguriert
4. Alle References verknüpft

**Unit Editor Window Verbesserungen:**
- Auto-Setup für komplette Unit
- Validation System
- Layer & Team Configuration
- Automatic Child Creation
- Reference Linking
- Quick-Fix Buttons

Mit diesen Verbesserungen kann das Unit Editor Window eine vollständig spielfähige Unit mit einem Klick erstellen! ?

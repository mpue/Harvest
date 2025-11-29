# Unit Editor Window - Quick Start Guide

## ?? Schnellstart: Spielfähige Unit in 1 Minute

### Option 1: Auto-Setup (Empfohlen)

```
1. Tools ? Unit Editor
2. Tab: "Create New Unit"
3. Unit Name eingeben
4. Team wählen (Player/Enemy)
5. "CREATE UNIT" Button
6. Fertig! ?
```

**Das wird automatisch erstellt:**
- ? Alle 9 erforderlichen Components
- ? Weapon Child GameObject
- ? HealthBar Child GameObject
- ? SelectionIndicator Child GameObject
- ? Layer & Team konfiguriert
- ? Alle References verknüpft

### Option 2: Existing Unit verbessern

```
1. Unit GameObject selektieren
2. Tools ? Unit Editor
3. Tab: "Edit Components"
4. Klick: "Setup Complete Playable Unit"
5. Fertig! ?
```

## ?? Erforderliche Komponenten-Checkliste

### Components auf Main GameObject:
- [ ] BaseUnit
- [ ] TeamComponent
- [ ] TeamVisualIndicator  
- [ ] Health
- [ ] Controllable (nicht für Buildings)
- [ ] NavMeshAgent (nicht für Buildings)
- [ ] WeaponController
- [ ] Collider (Capsule/Box)
- [ ] Rigidbody (nicht für Buildings)

### Child GameObjects:
- [ ] Weapon (mit Weapon Component)
- [ ] HealthBar (mit HealthBar Component)
- [ ] SelectionIndicator (Cylinder/Plane)

### Configuration:
- [ ] Layer: Player/Enemy
- [ ] Team: Player/Enemy
- [ ] References verknüpft

## ?? Unit Editor Window Features

### Tab 1: Create New Unit
**Erstellt komplette Unit von Grund auf**
- Unit Name
- Team auswählen
- Is Building checkbox
- Automatische Component-Erstellung
- Automatische Child-Erstellung

### Tab 2: Import 3D Model
**Importiert 3D Model und konfiguriert**
- Model auswählen
- Weapon Points zuweisen
- Turret Transforms zuweisen
- Selection Indicator zuweisen
- Auto-Detection Features

### Tab 3: Edit Components
**Bearbeitet existing Unit**
- Alle Components editierbar
- Quick Actions Buttons
- Validation System
- Missing Components hinzufügen

### Tab 4: Audio Settings
**Konfiguriert Audio**
- AudioManager Setup
- Unit Sounds
- AudioMixer Groups

## ? Quick Actions

### Setup Buttons:
```
"Setup Complete Playable Unit"
? Fügt ALLE Components hinzu
? Erstellt Weapon Child
? Erstellt HealthBar
? Erstellt SelectionIndicator
? Setzt Layer & Team

"Add Missing Components"
? Fügt nur fehlende Components hinzu

"Create Weapon Child"
? Erstellt Weapon GameObject

"Validate Unit"
? Prüft alle Requirements
? Zeigt Errors & Warnings
```

## ?? Layer Configuration

### Empfohlene Layer:
```
Player ? Layer 10
Enemy ? Layer 9
Building ? Layer 11
Ground ? Layer 12
```

### Layer Setup in Unity:
```
Edit ? Project Settings ? Tags and Layers
Layers:
  User Layer 8: Unit
  User Layer 9: Enemy
  User Layer 10: Player
  User Layer 11: Building
  User Layer 12: Ground
```

## ?? Weapon Setup

### Automatisch (empfohlen):
```
1. "Create Weapon Child" Button
2. Weapon GameObject wird erstellt
3. Weapon Component wird hinzugefügt
4. WeaponController.weapons wird verknüpft
```

### Manuell:
```
1. GameObject ? Create Empty (Child)
2. Name: "Weapon"
3. Add Component ? Weapon
4. Configure:
   - weaponName: "Main Gun"
   - damage: 10
   - fireRate: 1
   - range: 20
   - projectilePrefab: [Assign]
5. WeaponController.weapons: [Assign]
```

## ?? HealthBar Setup

### Automatisch:
```
"Setup Complete Playable Unit"
? Erstellt HealthBar automatisch
```

### Manuell:
```
1. GameObject ? Create Empty (Child)
2. Name: "HealthBar"
3. Position: (0, 2, 0)
4. Add Component ? HealthBar
5. Configure:
   - healthComponent: [Health Reference]
   - offset: (0, 2, 0)
```

## ?? SelectionIndicator Setup

### Automatisch:
```
"Setup Complete Playable Unit"
? Erstellt Indicator automatisch
```

### Manuell:
```
1. GameObject ? 3D Object ? Cylinder (Child)
2. Name: "SelectionIndicator"
3. Transform:
   - Position: (0, 0.05, 0)
   - Rotation: (90, 0, 0)
   - Scale: (1, 0.05, 1)
4. Material: Emissive/Transparent
5. Remove Collider
6. BaseUnit.selectionIndicator: [Assign]
7. SetActive: false
```

## ? Validation System

### Validation Check:
```
Tab: "Edit Components"
Button: "Validate Unit"

Zeigt:
? Valid Components
? Missing Components (Errors)
? Recommended Components (Warnings)
```

### Validation Beispiel:
```
? Unit is valid for gameplay!

Components Found:
? BaseUnit
? TeamComponent
? Health
? Controllable
? WeaponController
? Collider
? Rigidbody

Children:
? Weapon
? HealthBar
? SelectionIndicator (recommended)

Configuration:
? Layer: Player (10)
? Team: Player
```

## ?? Troubleshooting

### Problem: Unit kann nicht erstellt werden
**Lösung:**
- Prüfe Unit Name nicht leer
- Prüfe keine Null-References

### Problem: Weapon schießt nicht
**Lösung:**
- Prüfe Weapon Child vorhanden
- Prüfe WeaponController.weapons Array
- Prüfe projectilePrefab assigned
- Prüfe shotPoints Array

### Problem: HealthBar nicht sichtbar
**Lösung:**
- Prüfe HealthBar GameObject existiert
- Prüfe HealthBar.healthComponent Reference
- Prüfe Position (über Unit)

### Problem: Selection funktioniert nicht
**Lösung:**
- Prüfe selectionIndicator Reference
- Prüfe Indicator Material/Color
- Prüfe Initial SetActive: false

## ?? Erweiterte Features

### Team Konfiguration:
```csharp
TeamComponent:
  team: Player/Enemy/Neutral/Ally
  teamColor: Custom Color

TeamVisualIndicator:
  indicatorType: ColorRing/ShieldIcon/MaterialTint
  showIndicator: true
```

### Layer Konfiguration:
```csharp
// Automatisch basierend auf Team:
Player ? Layer 10 (Player)
Enemy ? Layer 9 (Enemy)
Neutral ? Layer 0 (Default)
```

### WeaponController:
```csharp
WeaponController:
  weapons: [Weapon Array]
  autoAcquireTargets: true
  targetScanInterval: 0.5
  targetLayerMask: Enemy Layer
  autoFire: true
  fireInterval: 0.1
```

## ?? Testing Checklist

Nach Unit-Erstellung:
- [ ] Unit spawnt in Szene
- [ ] Unit kann selektiert werden
- [ ] SelectionIndicator zeigt sich
- [ ] Unit kann bewegt werden (Rechtsklick)
- [ ] Unit findet Ziele automatisch
- [ ] Unit schießt auf Feinde
- [ ] HealthBar wird angezeigt
- [ ] Unit nimmt Schaden
- [ ] Unit stirbt bei 0 HP
- [ ] Team-Farbe wird angezeigt

## ?? Weitere Dokumentation

- **UNIT_EDITOR_COMPLETE_SETUP.md** - Vollständiges Setup-Guide
- **UnitEditorHelper.cs** - Helper-Funktionen
- **UnitEditorWindow.cs** - Main Editor Window

## ?? Best Practices

### DO:
- ? Verwende "Setup Complete Playable Unit"
- ? Validiere Unit vor Prefab-Erstellung
- ? Teste Unit in Szene vor Prefab
- ? Konfiguriere Layer & Team
- ? Assign Projectile Prefab

### DON'T:
- ? Manuelle Component-Erstellung (außer nötig)
- ? Vergessen Weapon Child zu erstellen
- ? Vergessen References zu verknüpfen
- ? Layer auf Default lassen
- ? Team nicht setzen

## Zusammenfassung

**1 Klick ? Spielfähige Unit:**
```
Tools ? Unit Editor
? Create New Unit
? Configure & Create
? Fertig! ?
```

**Features:**
- ? Auto-Setup
- ? Validation
- ? Quick Actions
- ? Layer & Team Config
- ? Component Management
- ? Child Creation
- ? Reference Linking

Mit dem Unit Editor Window können Sie in weniger als 1 Minute eine vollständig spielfähige, konfigurierte Unit erstellen! ???

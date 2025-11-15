# Unit Editor: 3D Model Import & Configuration - Quick Guide

## Übersicht

Der Unit Editor kann jetzt **3D-Modelle importieren** und **interaktiv Weapon-Points und Turrets zuweisen**!

## Features

? Import von 3D-Modell-Prefabs
? Interaktive Transform-Hierarchie-Ansicht
? Weapon Point Zuweisung
? Turret Transform Zuweisung
? Selection Indicator Zuweisung
? Auto-Detection (erkennt Namen wie "weapon", "turret", "gun", etc.)
? Apply to new or existing units
? Visual Feedback für alle Zuweisungen

## Workflow: 3D-Modell importieren

### Step 1: Öffne Unit Editor

```
Tools > Unit Editor
```

### Step 2: Import & Setup 3D Model Sektion

Die Sektion "Import & Setup 3D Model" befindet sich am Anfang des Editors (vor "Create New Unit").

### Step 3: Wähle 3D-Modell

1. **3D Model Prefab Field**: Ziehe dein 3D-Modell-Prefab in das Feld
2. **Auto-Scan**: Editor scannt automatisch nach Weapon/Turret-Transforms

### Step 4: Assign Transform Roles

**Interaktive Hierarchie-Ansicht:**

Für jedes Transform im Modell siehst du:
- Transform Name (mit Indent für Hierarchie)
- **[Weapon] Button** - Markiert als Weapon Point
- **[Turret] Button** - Markiert als Turret
- **[Indicator] Button** - Markiert als Selection Indicator
- **Status** - Zeigt [W], [T], [I] für zugewiesene Rollen

**Beispiel:**
```
Model Root
  Body
    [Weapon] [Turret] [Indicator]
  Turret_01             [T]
    Weapon_Point_Left              [W]
    Weapon_Point_Right             [W]
  SelectionRing           [I]
```

### Step 5: Review Assignments

**Weapon Points Liste:**
- Zeigt alle zugewiesenen Weapon Points
- [Remove] Button zum Entfernen

**Turrets Liste:**
- Zeigt alle zugewiesenen Turrets
- [Remove] Button zum Entfernen

**Selection Indicator:**
- Zeigt zugewiesenen Indicator

### Step 6: Apply to Unit

**Option A: Create New Unit with Model**

```
Button: "Create New Unit with Model" (grün)
```

Erstellt eine komplett neue Unit mit:
- BaseUnit Component
- TeamComponent
- Health
- Importiertes Modell als Child
- Weapon-Komponenten für jeden Weapon Point
- Automatische Turret-Zuweisung
- Selection Indicator verknüpft

**Option B: Apply to Selected Unit**

```
1. Target Unit: Wähle bestehende Unit aus
2. Button: "Apply to Selected Unit" (blau)
```

Ersetzt Visuals der bestehenden Unit und fügt Weapons hinzu.

## Auto-Detection

Der Editor erkennt automatisch Transforms anhand ihrer Namen:

### Weapon Points

Erkannt werden Namen mit:
- `weapon`
- `gun`
- `muzzle`
- `firepoint`
- `shotpoint`

Beispiele:
- `Weapon_Point_01`
- `Gun_Left`
- `MuzzleFlash`
- `FirePoint_Right`

### Turrets

Erkannt werden Namen mit:
- `turret`
- `barrel`
- `cannon`
- `rotating`

Beispiele:
- `Turret_Main`
- `Barrel_01`
- `Cannon_Rotating`

### Selection Indicators

Erkannt werden Namen mit:
- `indicator`
- `selection`
- `highlight`

Beispiele:
- `SelectionIndicator`
- `Selection_Ring`
- `Highlight_Circle`

## Quick Actions

**Auto-Detect Weapons** - Scannt alle Transforms nach Weapon-Namen
**Auto-Detect Turrets** - Scannt alle Transforms nach Turret-Namen
**Clear All** - Entfernt alle Zuweisungen

## Beispiel-Workflow: Tank mit Turret

### Vorbereitung

3D-Modell mit dieser Hierarchie:
```
Tank_Model (Prefab)
?? Body
?? Turret_Rotating
?  ?? Cannon_Barrel
?  ?? Gun_Left
?  ?? Gun_Right
?? SelectionRing
```

### Im Editor

1. **3D Model Prefab**: `Tank_Model` zuweisen
2. **Auto-Detection läuft**:
   - Weapon Points gefunden: `Gun_Left`, `Gun_Right`
   - Turrets gefunden: `Turret_Rotating`
   - Indicator gefunden: `SelectionRing`
3. **Review**:
   - ? 2 Weapon Points
   - ? 1 Turret
   - ? 1 Selection Indicator
4. **Create New Unit with Model**
5. **Ergebnis**:
   ```
   Tank_Model_Unit
   ?? BaseUnit
   ?? TeamComponent
   ?? Health
   ?? WeaponController (mit 2 Weapons)
   ?? Weapon_Gun_Left
   ?  ?? Weapon Component (turret: Turret_Rotating)
   ?? Weapon_Gun_Right
   ?  ?? Weapon Component (turret: Turret_Rotating)
   ?? Model
    ?? Tank_Model (instantiated)
   ```

## Manuelle Zuweisung

Falls Auto-Detection nicht alles findet:

1. **Scroll durch Hierarchie**
2. **Click [Weapon] / [Turret] / [Indicator]** bei gewünschtem Transform
3. **Status ändert sich** - [W], [T], [I] erscheint
4. **Review-Sektion** aktualisiert sich automatisch

## Weapon Configuration Details

Für jeden Weapon Point wird automatisch:
1. **GameObject** `Weapon_<PointName>` erstellt
2. **Weapon Component** hinzugefügt
3. **Turret Transform** zugewiesen (erster Turret in Liste)
4. **Shot Point** auf Weapon Point gesetzt
5. **WeaponController** aktualisiert

## Tipps

### 1. Naming Conventions

Benenne Transforms in deinem 3D-Modell konsistent:
```
? Weapon_Point_01, Weapon_Point_02
? Turret_Main, Turret_Secondary
? SelectionIndicator
```

### 2. Hierarchie-Struktur

Empfohlene Struktur für Fahrzeuge/Turrets:
```
Model
?? Body (statisch)
?? Turret (rotiert horizontal)
?  ?? Barrel (rotiert vertikal)
?  ?? WeaponPoint (wo Projektile spawnen)
?? SelectionRing (am Boden)
```

### 3. Multiple Weapons

Für mehrere Waffen:
- Erstelle mehrere Weapon Points
- Auto-Detection findet alle
- Jeder erhält eigene Weapon Component

### 4. Shared Turret

Mehrere Waffen können denselben Turret teilen:
- Weapon Point 1 ? Turret_Main
- Weapon Point 2 ? Turret_Main
- Beide Waffen rotieren gemeinsam

### 5. Testing

Nach Import:
1. **Play Mode** starten
2. **Unit selektieren**
3. **Weapons sollten funktionieren** (wenn konfiguriert)
4. **Turrets sollten zum Ziel rotieren**

## Fehlerbehebung

### "Could not find weapon point in instantiated model"

**Problem**: Transform-Name stimmt nicht überein

**Lösung**:
- Prüfe Schreibweise im Modell
- Stelle sicher, dass Prefab aktuell ist
- Verwende manuelle Zuweisung

### Weapon hat keinen Turret

**Problem**: Kein Turret zugewiesen oder gefunden

**Lösung**:
1. Stelle sicher, dass Turret in Liste ist
2. Turret-Namen prüfen (`turret`, `barrel`, etc.)
3. Manuell [Turret] Button klicken
4. Nach Reassign: Apply erneut

### Selection Indicator funktioniert nicht

**Problem**: Indicator nicht korrekt zugewiesen

**Lösung**:
1. Prüfe ob Indicator in Review-Sektion angezeigt wird
2. Im Scene-View: Prüfe ob Indicator-GameObject existiert
3. BaseUnit Inspector: Prüfe Selection Indicator Field

### Model wird nicht instantiated

**Problem**: Prefab oder Instantiation-Fehler

**Lösung**:
- Stelle sicher, dass 3D Model ein gültiges Prefab ist
- Prüfe Console auf Fehler
- Versuche direktes Instantiate im Scene

## Fortgeschrittene Verwendung

### Multiple Turrets

Für Einheiten mit mehreren unabhängigen Turrets:

1. **Weise Weapon Points zu**:
   - `Weapon_Turret1_Gun` ? [Weapon]
   - `Weapon_Turret2_Gun` ? [Weapon]

2. **Weise Turrets zu**:
   - `Turret_01` ? [Turret]
   - `Turret_02` ? [Turret]

3. **Nach Apply**:
   - Manuell im Inspector jedes Weapon korrekt zuweisen
   - Weapon 1 ? Turret_01
   - Weapon 2 ? Turret_02

### Weapon ohne Turret

Für statische Waffen (z.B. Raketen-Launcher):

1. **Weapon Point zuweisen**
2. **KEIN Turret zuweisen**
3. Weapon schießt in feste Richtung

### Custom Shot Points

Standardmäßig wird Weapon Point als Shot Point verwendet.

Für custom Shot Points:
1. **Apply Model** erst normal
2. **Manuell** im Weapon Inspector:
- Shot Points Array erweitern
   - Andere Transforms zuweisen

## Integration mit anderen Systemen

### Combat System

Erstellte Weapons sind sofort kampfbereit:
- WeaponController scannt automatisch nach Zielen
- Turrets rotieren zu Feinden
- Weapons feuern automatisch (wenn autoFire = true)

### Team System

Importierte Units erhalten automatisch:
- TeamComponent mit Player-Team
- Kann im Inspector geändert werden

### Selection System

Selection Indicator funktioniert automatisch:
- Wird bei Selektion angezeigt
- Versteckt wenn nicht selektiert

## Best Practices

### Modell-Vorbereitung

**In 3D-Software (Blender, Maya, etc.)**:

1. **Leere GameObjects** für Weapon Points erstellen
2. **Pivot Points** richtig setzen für Turrets
3. **Hierarchie** logisch strukturieren
4. **Namen** konsistent vergeben
5. **Als Prefab** exportieren

### Editor-Workflow

1. **Import**: Modell zuerst in Unity importieren
2. **Prefab**: Als Prefab speichern
3. **Test**: In Unit Editor laden
4. **Auto-Detect**: Laufen lassen
5. **Review**: Zuweisungen prüfen
6. **Adjust**: Manuell korrigieren
7. **Apply**: Unit erstellen
8. **Test**: Im Play Mode testen

### Performance

- **Weapon Components** nur bei Bedarf
- **Turrets** nur wenn wirklich rotiert werden muss
- **LODs** im Modell für große Einheiten

## Siehe auch

- `UnitEditor_README.md` - Vollständige Unit Editor Dokumentation
- `WeaponSystem_README.md` - Waffen-System Details
- `CombatSystem_README.md` - Kampf-System Integration

## Support

Bei Problemen:
1. Console auf Errors prüfen
2. Transform-Namen im Modell-Prefab prüfen
3. Auto-Detection verwenden oder manuell zuweisen
4. Review-Sektion zur Überprüfung nutzen

# Building Placement Setup Guide

## Problem: "Ich kann Gebäude nicht platzieren"

Wenn Sie Gebäude produzieren, aber nicht platzieren können, prüfen Sie folgende Punkte:

## 1. Ground Layer Setup

### Option A: Verwenden Sie "Default" Layer
Wenn Sie keinen speziellen Ground Layer haben:
```csharp
// In BuildingPlacement Inspector:
Ground Layer: Everything (oder Nothing mit Default ausgewählt)
```

### Option B: Erstellen Sie einen Ground Layer
1. Edit > Project Settings > Tags and Layers
2. Erstellen Sie einen neuen Layer "Ground"
3. Wählen Sie Ihren Boden (Terrain/Plane) aus
4. Setzen Sie Layer auf "Ground"
5. In BuildingPlacement Inspector:
   - Ground Layer: Ground

## 2. Kamera Setup

BuildingPlacement benötigt eine Kamera für Raycasting:

### Automatische Erkennung:
- Sucht nach `Camera.main` (Tagged als "MainCamera")
- Falls nicht gefunden: Sucht erste Camera in Scene

### Manuelle Zuweisung:
```
BuildingPlacement Component:
Target Camera: Drag your camera here
```

## 3. Boden-Collider

Der Boden MUSS einen Collider haben!

### Für Terrain:
- Terrain hat automatisch einen TerrainCollider

### Für Plane/Mesh:
- Fügen Sie MeshCollider oder BoxCollider hinzu
```
GameObject (Ground) 
  ?? MeshCollider (oder BoxCollider)
```

## 4. BuildingPlacement Component

Stellen Sie sicher, dass BuildingPlacement in der Scene existiert:

### Option A: Am GameManager
```
GameObject: GameManager
Components:
  - GameManager
  - BuildingPlacement
```

### Option B: Eigenes GameObject
```
GameObject: BuildingPlacementSystem
Components:
  - BuildingPlacement
  - BuildingPlacementUI (optional)
  - BuildingPlacementDebug (für Debugging)
```

## 5. Einstellungen prüfen

### BuildingPlacement Inspector:
```
Placement Settings:
  Ground Layer: [Everything oder Ihr Ground Layer]
  Placement Height: 0.1
  Grid Size: 1
  Snap To Grid: ?
  Target Camera: [Ihre Kamera oder leer für auto]

Visual Feedback:
  Valid Color: Green (0, 1, 0, 0.5)
  Invalid Color: Red (1, 0, 0, 0.5)

Collision Check:
  Collision Check Radius: 2
  Obstacle Layer: [Default oder spezifische Layer]

UI Feedback:
  Show Placement UI: ?
```

## 6. Debug-Modus aktivieren

Fügen Sie `BuildingPlacementDebug` hinzu um zu sehen was los ist:

```
1. Wählen Sie BuildingPlacement GameObject
2. Add Component > BuildingPlacementDebug
3. Inspector:
   - Show Raycast Debug: ?
   - Show Ground Hit Point: ?
   - Ground Layer: [Same als BuildingPlacement]
```

Das Debug-UI zeigt:
- ? Raycast Hit: True ? Boden wird erkannt
- ? Raycast Hit: False ? Problem mit Boden/Collider/Layer

## 7. Häufige Probleme und Lösungen

### Problem: "Keine Vorschau erscheint"
**Lösung:**
- Prüfen Sie ob Building Prefab existiert
- Prüfen Sie Console für Fehler
- BuildingPlacement.StartPlacement() wird aufgerufen?

### Problem: "Vorschau ist unsichtbar"
**Lösung:**
- Prefab hat Renderer?
- Materials werden geladen?
- Kamera sieht die Vorschau? (Check Layer Culling)

### Problem: "Vorschau bewegt sich nicht mit Maus"
**Lösung:**
- ? Keine Camera gefunden
- ? Ground hat keinen Collider
- ? Ground Layer stimmt nicht

### Problem: "Alles ist rot (invalid)"
**Lösung:**
- Kollision mit anderem Objekt?
- Nicht genug Energie?
- Obstacle Layer enthält Boden?

### Problem: "Links-Klick funktioniert nicht"
**Lösung:**
- Vorschau muss GRÜN sein
- Console checken für Warnings
- BuildingPlacement.CanPlace sollte true sein

## 8. Schnelltest

### A. Manueller Test:
```csharp
1. Scene öffnen
2. Gebäude im Headquarter produzieren
3. Warten bis fertig
4. BuildingPlacement.StartPlacement() wird automatisch aufgerufen
5. Maus über Boden bewegen ? Vorschau sollte erscheinen
6. Q/E ? Rotation testen
7. Grün? ? Links-Klick zum Platzieren
```

### B. Programmtischer Test:
```csharp
// Im Editor:
1. BuildingPlacement GameObject auswählen
2. Add Component > BuildingPlacementDebug
3. Right-Click auf Component ? Create Test Ground
4. Product Asset erstellen (Create > RTS > Production > Product)
5. Right-Click auf BuildingPlacementDebug ? Test Placement
```

## 9. Console Ausgaben

Wenn alles funktioniert sollten Sie sehen:

```
? Started placing [BuildingName]. Use mouse to position...
? Placed [BuildingName] at (x, y, z)
```

Bei Problemen:
```
? Cannot start placement: invalid product
? No camera found for building placement!
? No ground detected!
? Cannot place building here!
? Not enough energy to place building
```

## 10. Minimale funktionierende Setup

```
Scene Hierarchy:
?? Main Camera (Tag: MainCamera)
?? Ground (Plane mit Collider)
?? GameManager
?  ?? GameManager
?  ?? BuildingPlacement
?  ?? ResourceManager
?? Player1_Headquarter
?  ?? BaseUnit
?  ?? ProductionComponent
?  ?? BuildingComponent
?  ?? Health
?? Canvas (optional)
   ?? BuildingPlacementUI

Assets:
?? Products/
?  ?? EnergyBlock.asset (Product)
?  ?? DefenseTower.asset (Product)
?? Prefabs/
   ?? EnergyBlock.prefab
   ?? DefenseTower.prefab
```

## 11. Checkliste

Vor dem Platzieren prüfen:

- [ ] BuildingPlacement Component in Scene?
- [ ] Kamera vorhanden und aktiv?
- [ ] Boden hat Collider?
- [ ] Ground Layer korrekt eingestellt?
- [ ] Building Prefab hat Renderer?
- [ ] ResourceManager vorhanden?
- [ ] Genug Energie verfügbar?
- [ ] Product.IsBuilding = true?
- [ ] Product.Prefab zugewiesen?

## 12. Erweiterte Konfiguration

### Custom Ground Detection:
```csharp
// Wenn Sie mehrere Layers für Boden haben:
Ground Layer: Ground | Floor | Terrain
```

### Custom Obstacle Detection:
```csharp
// Nur mit bestimmten Objekten kollidieren:
Obstacle Layer: Buildings | Units | Obstacles
```

### Platzierungs-Gitter:
```csharp
Snap To Grid: ?
Grid Size: 2.0  // Größere Rasterfelder
```

## Support

Falls Probleme bestehen:
1. Aktivieren Sie BuildingPlacementDebug
2. Schauen Sie auf das Debug UI (links oben im Game View)
3. Prüfen Sie Console für Errors/Warnings
4. Verifizieren Sie dass "Raycast Hit: True" angezeigt wird

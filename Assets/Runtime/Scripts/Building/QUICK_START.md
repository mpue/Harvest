# Building System - Quick Start Guide

## Was wurde erstellt/verbessert?

### Neue Dateien:
1. **BuildingPlacement.cs** - Verbessert mit:
   - Automatische Kamera-Erkennung
   - Fallback für fehlende Materials
   - OnGUI UI-Feedback während Platzierung
- Bessere Debug-Ausgaben
   - Transparente Vorschau-Materials

2. **BuildingPlacementUI.cs** - Optional Canvas-basiertes UI
3. **BuildingPlacementDebug.cs** - Debug-Helper für Troubleshooting
4. **PLACEMENT_SETUP_GUIDE.md** - Detaillierte Setup-Anleitung

### Erweiterte Dateien:
- **BuildingComponent.cs** - Korrigiert UnityEvent subscription
- **Product.cs** - Erweitert um Building-Eigenschaften
- **ProductionComponent.cs** - Integration mit Building-System
- **ProductionPanel.cs** - Ressourcen- und Energie-Anzeige
- **ResourceManager.cs** - Energie-Management
- **GameManager.cs** - Headquarter Setup

## Sofort loslegen (5 Minuten Setup):

### 1. Scene Setup (1 Min):
```
Ihre Scene sollte haben:
- Eine Kamera (mit Tag "MainCamera")
- Einen Boden (Plane/Terrain mit Collider)
```

Falls nicht vorhanden:
- **Boden erstellen:** GameObject > 3D Object > Plane
- **Kamera prüfen:** Ihre Kamera hat Tag "MainCamera"?

### 2. GameManager Setup (1 Min):
```
1. Create Empty GameObject "GameManager"
2. Add Component > GameManager
3. Add Component > BuildingPlacement
4. Add Component > ResourceManager
```

Im Inspector:
```
BuildingPlacement:
  Ground Layer: Everything (Default) ?
  Snap To Grid: ?
  Show Placement UI: ?
  
[Andere Werte können Standard bleiben]
```

### 3. Debug aktivieren (30 Sek):
```
Auf GameManager GameObject:
  Add Component > BuildingPlacementDebug
  
Inspector:
  Show Raycast Debug: ?
  Show Ground Hit Point: ?
```

### 4. Product erstellen (1 Min):
```
1. Assets > Create > RTS > Production > Product
2. Name: "EnergyBlock"
3. Inspector:
   - Product Name: "Energy Block"
   - Is Building: ?
   - Building Type: EnergyBlock
   - Production Duration: 5
   - Energy Production: 10
   - Prefab: [Ihr Gebäude-Prefab]
```

### 5. Headquarter Setup (1 Min):
```
Ihr Headquarter Prefab braucht:
  - BaseUnit Component
  - ProductionComponent
  - BuildingComponent
  - Health Component
  
ProductionComponent Inspector:
  - Available Products: [Drag EnergyBlock Product hier rein]
```

### 6. Test (30 Sek):
```
1. Play drücken
2. Headquarter auswählen
3. Production Panel öffnen
4. Energy Block produzieren
5. Warten bis fertig
6. Vorschau sollte automatisch erscheinen
7. Maus bewegen ? Vorschau bewegt sich
8. Q/E ? Rotation
9. Links-Klick ? Platzieren (wenn grün)
```

## Steuerung beim Platzieren:

| Eingabe | Aktion |
|---------|--------|
| **Maus bewegen** | Gebäude positionieren |
| **Q** | Links rotieren |
| **E** | Rechts rotieren |
| **Links-Klick** | Platzieren (wenn grün) |
| **Rechts-Klick** | Abbrechen |
| **ESC** | Abbrechen |

## UI-Feedback:

### Oben im Game View:
```
???????????????????????????????????
? Placing: Energy Block          ?
? Ready to Place                  ?  ? GRÜN = OK
? Left Click to Place           ?
? Q/E: Rotate | Right Click: Cancel ?
???????????????????????????????????
```

Oder wenn ungültig:
```
???????????????????????????????????
? Placing: Energy Block           ?
? Invalid Location!               ?  ? ROT = Problem
? Q/E: Rotate | Right Click: Cancel ?
???????????????????????????????????
```

### Debug-Info (Links oben):
```
=== Building Placement Debug ===
Camera: Main Camera
Raycast Hit: True       ? Wichtig!
Hit Point: (10.5, 0, -5.2)
Ground Layer: -1
Is Placing: True
Product: Energy Block
```

## Troubleshooting:

### Vorschau erscheint nicht?
? Console prüfen:
- "Started placing [Name]" sollte erscheinen
- Falls nicht: ProductionComponent Problem
- Falls Fehler: Prefab fehlt oder ungültig

### Vorschau bewegt sich nicht?
? Debug-Info prüfen:
```
Raycast Hit: False  ? Problem!
```
**Lösung:**
- Boden hat Collider? (Inspector prüfen)
- Ground Layer korrekt? (Alles an lassen für Test)
- Kamera aktiv?

? Debug-Helper nutzen:
```
GameManager > BuildingPlacementDebug
  Right-Click > Create Test Ground
```

### Alles ist rot?
? Prüfen:
- Energie verfügbar? (nicht bei Energy Blocks)
- Kollision mit anderem Objekt?
- Zu nah an anderem Gebäude?

? Collision Radius anpassen:
```
BuildingPlacement:
  Collision Check Radius: 1.0  (kleiner)
```

### Links-Klick macht nichts?
? Muss GRÜN sein zum Platzieren!
? Console prüfen für Warnings
? Debug-Info: "Ready to Place" sollte erscheinen

## Energie-System Überblick:

### Start:
```
Max Energy: 20
Current: 0
Available: 20
```

### Nach Energy Block (+10):
```
Max Energy: 30
Current: 0
Available: 30
```

### Nach Defense Tower (-5):
```
Max Energy: 30
Current: 5
Available: 25
```

### Regel:
- **Energy Blocks:** Können immer gebaut werden
- **Andere Gebäude:** Nur wenn Available Energy > EnergyCost

## Production Panel Features:

Das ProductionPanel zeigt jetzt:
```
??????????????????????????????
? Energy: 15/30 [GRÜN]   ? ? Energie-Anzeige
? Food: 500      ?
? Wood: 500       ?
? Stone: 500       ?
? Gold: 500  ?
??????????????????????????????
? [Product Slots...]        ?
??????????????????????????????
```

Farben:
- **GRÜN:** Viel Energie (>30%)
- **GELB:** Wenig Energie (<30%)
- **ROT:** Keine Energie (0)

## Nächste Schritte:

### Mehr Gebäude erstellen:
```
1. Defense Tower:
   - Building Type: DefenseTower
   - Energy Cost: 5
   - Energy Production: 0

2. Barracks:
   - Building Type: ProductionFacility
   - Energy Cost: 3
   - Energy Production: 0
```

### Canvas UI erstellen (Optional):
```
1. Create > UI > Canvas
2. Add BuildingPlacementUI Component
3. Create Child: Panel (für Placement Info)
4. Create Text Fields:
   - Building Name
   - Status Text
   - Instructions
```

### Materialien verbessern:
```
Erstellen Sie:
- ValidPlacementMaterial (Grün, Transparent)
- InvalidPlacementMaterial (Rot, Transparent)

Zuweisen in BuildingPlacement:
  Valid Placement Material: [Ihr Material]
Invalid Placement Material: [Ihr Material]
```

## Performance Tipps:

1. **Layer nutzen:** Erstellen Sie "Ground" Layer für bessere Kontrolle
2. **Obstacle Layer:** Erstellen Sie "Buildings" Layer für Kollision
3. **Disable Debug:** BuildingPlacementDebug in Build deaktivieren
4. **Object Pooling:** Für häufiges Platzieren/Zerstören

## Erweiterte Features:

### Grid Snapping anpassen:
```csharp
BuildingPlacement:
  Grid Size: 2.0        // Größere Raster
  Snap To Grid: false   // Freie Platzierung
```

### Rotation Speed anpassen:
```csharp
BuildingPlacement:
  Rotation Speed: 180   // Schnellere Rotation
```

### Preview Height anpassen:
```csharp
BuildingPlacement:
  Placement Height: 0.5  // Höher über Boden
```

## Support & Debugging:

### Log-Ausgaben verstehen:

**Bei erfolgreicher Platzierung:**
```
? Started placing Energy Block. Use mouse to position...
? Placed Energy Block at (10, 0.1, -5)
? Energy Block providing 10 energy
```

**Bei Problemen:**
```
? Cannot start placement: invalid product
? Product oder Prefab fehlt

? No camera found for building placement!
? Kamera nicht gefunden

? Not enough energy to place building
? Bauen Sie mehr Energy Blocks

? Cannot place building here!
? Kollision oder ungültige Position
```

### Weitere Hilfe:

Siehe: **PLACEMENT_SETUP_GUIDE.md** für detaillierte Troubleshooting-Anleitung

## Zusammenfassung:

? Building Production im Headquarter
? Automatische Platzierungs-Vorschau
? Visuelles Feedback (Grün/Rot)
? Energie-Management
? Ressourcen-Kosten
? Grid-Snapping
? Rotation (Q/E)
? Debug-Tools
? UI-Feedback

**Ready to build!** ???

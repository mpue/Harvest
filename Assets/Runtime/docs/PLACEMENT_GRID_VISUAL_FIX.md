# ?? Placement Grid - Visual Fix Guide

## Problem: Grid sieht "kaputt" aus

Hier sind die häufigsten visuellen Probleme und ihre Lösungen:

## ?? Häufige visuelle Probleme:

### Problem 1: Grid ist unsichtbar oder fast transparent

**Symptome:**
- Grid ist nicht zu sehen
- Nur ganz schwache Linien
- Verschwindet komplett

**Lösung:**
```
PlacementGrid Inspector:
  Grid Color: Alpha ERHÖHEN
    Von: (1, 1, 1, 0.3)
    Auf: (1, 1, 1, 0.8)  ? Deutlich sichtbarer!
  
  Line Width: Erhöhen
    Von: 0.02
    Auf: 0.1 oder mehr
```

### Problem 2: Grid Linien sind verzerrt/gestreckt

**Symptome:**
- Linien sehen wellig aus
- Ungleichmäßige Dicke
- Verzerrungen

**Ursache:** Mesh-Generation Problem

**Lösung:**
```
PlacementGrid Inspector:
  Use Mesh Grid: ? (ausschalten)

? Verwendet LineRenderer statt Mesh
? Stabilere Darstellung
? Etwas weniger Performance aber besser sichtbar
```

### Problem 3: Grid flackert oder verschwindet

**Symptome:**
- Grid blinkt
- Erscheint/verschwindet zufällig
- Z-Fighting Effekte

**Lösung A: Grid Y Offset erhöhen**
```
PlacementGrid Inspector:
  Grid Y Offset: 0.5  ? Höher über Terrain
  
? Verhindert Z-Fighting mit Terrain
```

**Lösung B: Update Interval anpassen**
```
PlacementGrid Inspector:
  Update Interval: 0  ? Jedes Frame
  
? Smoother updates
```

### Problem 4: Grid hat falsche Farbe/Material

**Symptome:**
- Grid ist pink (Missing Shader)
- Grid ist schwarz
- Keine Transparenz

**Lösung A: Custom Shader verwenden**
```
1. Shader wurde bereits erstellt:
   Assets/Runtime/Shaders/PlacementGrid.shader

2. Neu kompilieren:
   Assets > Reimport All

3. Play Mode testen
   ? Console: "Using custom PlacementGrid shader" ?
```

**Lösung B: Material manuell zuweisen**
```
1. Material erstellen:
   Assets > Create > Material
   Name: "GridMaterial"

2. Material konfigurieren:
   Shader: Legacy Shaders > Particles > Alpha Blended
   Rendering Mode: Fade
   Color: White mit Alpha 0.5

3. PlacementGrid Inspector:
   Grid Material: [GridMaterial zuweisen]
```

### Problem 5: Grid zu groß/klein

**Symptome:**
- Grid Zellen sind riesig
- Grid ist viel zu fein
- Passt nicht zum Snap-to-Grid

**Lösung:**
```
PlacementGrid Inspector:
  Grid Size: [Anpassen an Building Size]
  
Für kleine Buildings (1x1 Meter):
  Grid Size: 1.0

Für mittlere Buildings (2x2 Meter):
  Grid Size: 2.0

Für große Buildings (5x5 Meter):
  Grid Size: 5.0

WICHTIG: Sollte gleich sein wie:
BuildingPlacement > Grid Size!
```

### Problem 6: Grid Performance Probleme

**Symptome:**
- FPS Drops beim Placement
- Lag beim Grid-Update
- Stutter

**Lösung A: Mesh Grid verwenden**
```
PlacementGrid Inspector:
  Use Mesh Grid: ?
  Update Interval: 2 oder höher
```

**Lösung B: Grid kleiner machen**
```
PlacementGrid Inspector:
  Grid Width: 30  (statt 50)
  Grid Height: 30 (statt 50)
  
? Weniger Linien = bessere Performance
```

### Problem 7: Grid nicht zentriert auf Building

**Symptome:**
- Grid verschoben
- Building nicht in Grid-Mitte
- Ungenau

**Lösung:**
```
Das Grid sollte automatisch zentriert sein.

Falls nicht:
1. PlacementGrid GameObject auswählen
2. Transform > Reset
3. Position: (0, 0, 0)
4. Rotation: (0, 0, 0)
5. Scale: (1, 1, 1)

Oder im Code:
gridObject.transform.localPosition = Vector3.zero;
```

## ?? Empfohlene Settings für beste Sichtbarkeit:

### Maximale Sichtbarkeit:
```
PlacementGrid Inspector:

Grid Settings:
  Grid Size: 2.5  ? Größere Zellen
  Grid Width: 30
  Grid Height: 30
  Grid Y Offset: 0.5  ? Deutlich über Terrain

Visual Settings:
  Grid Color: (1, 1, 1, 0.8)  ? Sehr sichtbar!
  Line Width: 0.1  ? Dicke Linien
  Show Center Lines: ?

Performance:
  Use Mesh Grid: ?  ? LineRenderer für Stabilität
  Update Interval: 0  ? Jedes Frame
```

### Balanced (Performance + Sichtbarkeit):
```
Grid Settings:
  Grid Size: 2.5
  Grid Width: 40
  Grid Height: 40
  Grid Y Offset: 0.2

Visual Settings:
  Grid Color: (1, 1, 1, 0.5)
  Line Width: 0.05
  Show Center Lines: ?

Performance:
  Use Mesh Grid: ?  ? Mesh für Performance
  Update Interval: 2
```

### Subtil (Professional Look):
```
Grid Settings:
  Grid Size: 1.0
  Grid Width: 50
  Grid Height: 50
  Grid Y Offset: 0.1

Visual Settings:
  Grid Color: (1, 1, 1, 0.3)  ? Dezent
  Line Width: 0.02
  Show Center Lines: ?

Fade Settings:
  Fade With Distance: ?
  Fade Start Distance: 15
  Fade End Distance: 30

Performance:
  Use Mesh Grid: ?
  Update Interval: 2
```

## ?? Debug Checkliste:

```
? Grid ist sichtbar?
  ? Alpha erhöhen auf 0.5-0.8
  ? Line Width erhöhen auf 0.05-0.1

? Grid flackert?
  ? Grid Y Offset auf 0.5
  ? Update Interval auf 0

? Grid ist pink (Missing Shader)?
  ? Material manuell zuweisen
  ? Shader: Particles/Alpha Blended

? Grid Performance Problem?
  ? Use Mesh Grid: ?
  ? Update Interval: 2
  ? Grid Width/Height reduzieren

? Grid passt nicht zu Snap?
  ? Grid Size == BuildingPlacement Grid Size
  ? Beide auf gleichen Wert setzen

? Grid nicht zentriert?
  ? Transform Reset
  ? Local Position: (0, 0, 0)
```

## ?? Schnelle Fixes:

### Quick Fix 1: Sichtbarkeit
```
Grid Color Alpha: 0.3 ? 0.8
Line Width: 0.02 ? 0.1
? Viel sichtbarer!
```

### Quick Fix 2: Stabilität
```
Use Mesh Grid: ? ? ?
? LineRenderer ist stabiler
```

### Quick Fix 3: Z-Fighting
```
Grid Y Offset: 0.1 ? 0.5
? Höher über Terrain
```

### Quick Fix 4: Performance
```
Grid Width: 50 ? 30
Grid Height: 50 ? 30
Update Interval: 0 ? 2
? Schneller!
```

## ?? Farbige Grids (Optional):

### Sci-Fi (Cyan):
```
Grid Color: (0, 1, 1, 0.5)  // Cyan
Center Line Color: (0, 1, 1, 0.8)
```

### Military (Green):
```
Grid Color: (0, 1, 0, 0.4)  // Green
Center Line Color: (0, 1, 0, 0.6)
```

### Fantasy (Gold):
```
Grid Color: (1, 0.8, 0, 0.4)  // Gold
Center Line Color: (1, 0.8, 0, 0.6)
```

### High-Tech (Blue):
```
Grid Color: (0, 0.5, 1, 0.5)  // Blue
Center Line Color: (0, 0.5, 1, 0.8)
```

## ?? Vergleich: Mesh vs LineRenderer

### Mesh Grid:
```
Vorteile:
? Bessere Performance (1 Draw Call)
  ? Weniger CPU Usage
  ? Gut für große Grids

Nachteile:
  ? Kann visuell Probleme haben
  ? Komplexer zu debuggen
```

### LineRenderer Grid:
```
Vorteile:
  ? Stabilere Visualisierung
  ? Einfacher zu kontrollieren
  ? Weniger visuelle Artefakte

Nachteile:
  ? Mehr Draw Calls
  ? Höhere CPU Last
  ? Nicht für sehr große Grids
```

**Empfehlung für Ihre Situation:**
```
Use Mesh Grid: ? (ausschalten)
? LineRenderer für stabilere Darstellung
? Dann Settings anpassen für beste Sichtbarkeit
```

## ?? Testing Workflow:

```
1. PlacementGrid Settings anpassen:
Grid Color Alpha: 0.8
   Line Width: 0.1
   Use Mesh Grid: ?

2. Play Mode starten

3. Building Placement aktivieren

4. Grid sichtbar und stabil?
   JA: ? Fertig!
   NEIN: Grid Y Offset auf 0.5

5. Immer noch Probleme?
   ? Material manuell zuweisen
   ? Shader: Particles/Alpha Blended

6. Performance OK?
   JA: ? Behalten!
   NEIN: Grid Width/Height reduzieren
```

## ?? Zusammenfassung:

**Häufigstes Problem:** Grid zu transparent
**Schnellste Lösung:** Grid Color Alpha auf 0.8, Line Width auf 0.1

**Zweit-häufigstes Problem:** Grid flackert
**Schnellste Lösung:** Grid Y Offset auf 0.5

**Dritt-häufigstes Problem:** Pink Grid (Missing Shader)
**Schnellste Lösung:** Material manuell zuweisen mit Particles/Alpha Blended

---

**Quick Settings für sofortige Sichtbarkeit:**
```
Grid Color: (1, 1, 1, 0.8)
Line Width: 0.1
Grid Y Offset: 0.5
Use Mesh Grid: ?
Update Interval: 0
```

**Testen Sie diese Settings und das Grid sollte perfekt aussehen!** ??

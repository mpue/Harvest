# ?? Placement Grid System - Complete Guide

## Übersicht

Ein visuelles Grid-System das während des Building Placement Modus angezeigt wird. Das Grid hilft beim präzisen Platzieren von Gebäuden und zeigt das Snap-to-Grid Verhalten.

### Features:
- ? **Automatisch ein/ausblenden** beim Placement Start/Ende
- ? **Grid folgt Maus** während Placement
- ? **Snap-to-Grid** visuell dargestellt
- ? **Performance-optimiert** mit Mesh-basiertem Grid
- ? **Customizable** (Größe, Farbe, Transparenz)
- ? **Terrain-angepasst** (folgt Höhe)
- ? **Distance Fading** (wird transparent in der Ferne)

## ? Quick Setup (30 Sekunden):

### Automatisches Setup:

Das Grid wird **automatisch** erstellt wenn BuildingPlacement startet!

```
1. Ihre BuildingPlacement Component hat bereits:
   Show Grid: ?
   Auto Create Grid: ?
   Grid Size: 1 (passend zu Snap-to-Grid)

2. Play Mode starten

3. Building produzieren & Placement starten

4. Grid erscheint automatisch! ?
```

## ?? Was Sie sehen:

```
Während Placement Mode:
???????????????????????????????????
?     ?     ?     ?     ?     ?     ?
???????????????????????????????
?     ?     ?[??]?     ? ?  ? Building Preview
???????????????????????????????
?   ?     ?     ?     ?     ?  ?
???????????????????????????????
?     ?     ?     ?     ?     ?  ?
???????????????????????????????????

- Grid folgt der Maus
- Building Preview snappt zu Grid-Punkten
- Grid passt sich Terrain-Höhe an
- Grid wird transparent in der Ferne
```

## ?? PlacementGrid Component:

### Grid Settings:
```
Grid Size: 1.0    ? Größe einer Grid-Zelle
Grid Width: 50           ? Anzahl Zellen horizontal
Grid Height: 50   ? Anzahl Zellen vertikal
Grid Y Offset: 0.1     ? Höhe über Terrain
```

### Visual Settings:
```
Grid Color: White (Alpha 0.3)    ? Halbtransparent
Center Line Color: Yellow (0.5)  ? Mittellinien
Line Width: 0.02     ? Dicke der Linien
Show Center Lines: ?    ? Zeigt Achsen
```

### Fade Settings:
```
Fade With Distance: ?
Fade Start Distance: 20  ? Ab hier wird transparenter
Fade End Distance: 40    ? Hier komplett unsichtbar
```

### Performance:
```
Use Mesh Grid: ?         ? Mesh (schnell) vs LineRenderer (langsam)
Update Interval: 2       ? Update alle N Frames
```

## ?? Verwendung:

### Aus Code:

```csharp
// Grid anzeigen
placementGrid.Show();

// Grid verstecken
placementGrid.Hide();

// Grid Position setzen
placementGrid.SetPosition(buildingPosition);

// Grid Größe ändern
placementGrid.SetGridSize(2f); // 2 Meter Zellen

// Grid Dimensionen ändern
placementGrid.SetGridDimensions(100, 100); // Größeres Grid

// Grid Farbe ändern
placementGrid.SetGridColor(new Color(0, 1, 0, 0.5f)); // Grünes Grid
```

### Integration mit BuildingPlacement:

```csharp
// Bereits integriert! BuildingPlacement macht:

StartPlacement():
  ? placementGrid.Show()
  
UpdateBuildingPreview():
  ? placementGrid.SetPosition(buildingPosition)
  
CancelPlacement():
  ? placementGrid.Hide()
```

## ?? Customization:

### Grid Größe an Snap-to-Grid anpassen:

```
BuildingPlacement:
  Grid Size: 1.0
  Snap To Grid: ?
  
PlacementGrid:
  Grid Size: 1.0    ? Muss gleich sein!
  
? Building snappt perfekt zu Grid-Linien
```

### Verschiedene Grid-Stile:

#### 1. Feines Grid (kleine Gebäude)
```
Grid Size: 0.5
Grid Width: 100
Grid Height: 100
Line Width: 0.01
Grid Color: (1, 1, 1, 0.2)  ? Sehr transparent
```

#### 2. Grobes Grid (große Gebäude)
```
Grid Size: 2.0
Grid Width: 30
Grid Height: 30
Line Width: 0.05
Grid Color: (1, 1, 1, 0.5)  ? Sichtbarer
```

#### 3. Farbiges Grid (Thematisch)
```
// Sci-Fi:
Grid Color: (0, 1, 1, 0.4)  ? Cyan

// Fantasy:
Grid Color: (1, 0.8, 0, 0.3) ? Golden

// Military:
Grid Color: (0, 1, 0, 0.3)  ? Grün
```

### Performance-Optimierung:

#### High Performance (empfohlen):
```
Use Mesh Grid: ?         ? Mesh ist schneller
Update Interval: 2       ? Alle 2 Frames
Fade With Distance: ?    ? Weniger zu rendern
Grid Width: 50           ? Nicht zu groß
Grid Height: 50
```

#### Max Quality (langsamer):
```
Use Mesh Grid: ?   ? LineRenderer (schöner aber langsamer)
Update Interval: 0       ? Jedes Frame
Grid Width: 100   ? Sehr groß
Grid Height: 100
Line Width: 0.03         ? Dick
```

## ?? Technische Details:

### Mesh-basiertes Grid:

```
Vorteile:
  ? Sehr schnell (1 Draw Call)
  ? Wenig CPU Usage
  ? Gut für viele Linien
  
Nachteile:
  ? Etwas komplexer zu implementieren
  ? Schwerer zu animieren
```

### LineRenderer-basiertes Grid:

```
Vorteile:
  ? Einfach zu implementieren
  ? Einfach zu animieren
  ? Flexible Line-Styles
  
Nachteile:
  ? Langsamer (viele Draw Calls)
  ? Mehr CPU/GPU Last
  ? Nicht gut für große Grids
```

### Aktuelle Implementation:

```
? Mesh-basiert (Standard)
? Kann zu LineRenderer gewechselt werden
? Auto-detected in Awake()
? Grid folgt Terrain-Höhe
? Distance-based Fading
? Update nur alle N Frames
```

## ?? Debug & Troubleshooting:

### Grid erscheint nicht:

```
1. BuildingPlacement Inspector prüfen:
   Show Grid: ?
   Auto Create Grid: ?
   
2. Play Mode:
   Building Placement starten
   ? Console: Errors?

3. Scene View:
   PlacementGrid GameObject sichtbar?
   ? Active: ?
   ? PlacementGrid Component: ?
```

### Grid zu groß/klein:

```
PlacementGrid:
  Grid Width: [Anpassen]
  Grid Height: [Anpassen]
  
Oder:
  Grid Size: [Größere/Kleinere Zellen]
```

### Grid nicht sichtbar (zu transparent):

```
PlacementGrid:
  Grid Color: Alpha erhöhen (z.B. 0.3 ? 0.6)
  Line Width: Erhöhen (z.B. 0.02 ? 0.05)
```

### Grid folgt nicht der Maus:

```
BuildingPlacement.UpdateBuildingPreview():
  ? Prüfen ob placementGrid.SetPosition() aufgerufen wird
  ? Console: Logs aktivieren
```

### Performance Probleme:

```
PlacementGrid:
  Use Mesh Grid: ?       ? Wichtig!
  Update Interval: 2 oder höher
  Grid Width: ? 50
  Grid Height: ? 50
```

## ?? Erweiterte Features:

### Center Lines (Achsen):

```
PlacementGrid:
  Show Center Lines: ?
  Center Line Color: Yellow
  
? Zeigt X/Z-Achsen in anderer Farbe
? Hilfreich für Orientierung
```

### Distance Fading:

```
Fade With Distance: ?
Fade Start Distance: 20
Fade End Distance: 40

? Grid wird transparent wenn Kamera weit weg
? Sieht professioneller aus
? Bessere Performance
```

### Terrain Adaptation:

```
Grid Y Offset: 0.1

? Grid schwebt leicht über Terrain
? Verhindert Z-Fighting
? Passt sich Höhe an
```

## ?? Shader-basiertes Grid (Advanced):

Für noch bessere Performance kann ein Shader verwendet werden:

```csharp
// In PlacementGrid:
Use Shader: ?
Grid Material: [Custom Grid Shader]

? GPU rendert Grid
? Noch schneller
? Erweiterte Effekte möglich (Glow, Animation)
```

## ?? Checkliste:

```
? BuildingPlacement hat Grid aktiviert:
  Show Grid: ?
  Auto Create Grid: ?
  Grid Size: 1.0

? PlacementGrid wurde erstellt:
  Hierarchy > BuildingPlacement > PlacementGrid

? Grid Settings angepasst:
  Grid Width/Height passend
  Grid Color sichtbar (Alpha > 0.2)
  Use Mesh Grid: ?

? In Play Mode getestet:
  Building produzieren
  Placement Mode starten
  Grid sichtbar? ?
  Grid folgt Maus? ?
  Grid snappt zu Linien? ?
```

## ?? Best Practices:

### 1. Grid Size = Snap Grid Size
```
? Beide auf 1.0 setzen
? Building snappt perfekt zu Grid-Linien
? Visuell konsistent
```

### 2. Subtile Transparenz
```
? Grid Color Alpha: 0.2 - 0.4
? Nicht zu aufdringlich
? Trotzdem hilfreich
```

### 3. Performance beachten
```
? Use Mesh Grid für große Grids
? Update Interval > 1
? Grid Width/Height ? 50
```

### 4. Terrain-angepasst
```
? Grid Y Offset: 0.1
? Raycast zu Terrain
? Smooth positioning
```

### 5. Distance Fading
```
? Fade With Distance: ?
? Fade Start: 20-30
? Fade End: 40-50
```

## ?? Zusammenfassung:

**Component:** PlacementGrid
**Integration:** Automatisch in BuildingPlacement
**Performance:** Optimiert mit Mesh-Grid
**Customization:** Vollständig anpassbar

**Features:**
- ? Auto Show/Hide
- ? Folgt Maus/Building
- ? Snap-to-Grid Visualisierung
- ? Terrain-angepasst
- ? Distance Fading
- ? Performance-optimiert
- ? Customizable

**Setup-Zeit:** 0 Sekunden (automatisch!)
**Performance-Impact:** Minimal (<1ms)
**Visual Quality:** Professional

---

**Das Grid-System ist production-ready und plug-and-play!** ??

**Einfach Play Mode starten und Building platzieren - Grid erscheint automatisch!** ???

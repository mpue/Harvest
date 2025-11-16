# ??? Minimap Fix Guide - Schwarzes Bild lösen

## ?? Problem: Minimap zeigt nur schwarzes Bild

### Häufigste Ursachen:

1. **Minimap Camera rendert nur "Minimap" Layer** ? HAUPTPROBLEM
2. Terrain/Objekte sind nicht auf dem Minimap Layer
3. RenderTexture fehlt oder ist falsch zugewiesen
4. Camera Position/Rotation ist falsch

## ? SOFORT-LÖSUNG (30 Sekunden):

### Option 1: Auto-Fix Tool (Empfohlen)

```
1. Tools > RTS > Minimap Fix & Setup

2. Click "?? Refresh Diagnostics"
   ? Lesen Sie den Report

3. Click "?? Complete Auto-Fix (Recommended)"
   ? Behebt automatisch die meisten Probleme

4. Fertig!
```

### Option 2: Manuelle Quick-Fix

```
1. Hierarchy: Finden Sie "MinimapCamera" (oder Kamera mit RenderTexture)

2. Inspector > Camera Component:
   Culling Mask: Everything (statt nur "Minimap")
   
3. Play Mode testen

? Minimap sollte JETZT etwas zeigen!
```

## ?? Detaillierte Lösungen:

### Problem 1: Camera Culling Mask

#### Symptom:
```
Camera.cullingMask = LayerMask.GetMask("Minimap")
? Rendert nur Objekte auf "Minimap" Layer
? Wenn nichts auf diesem Layer ist: SCHWARZ!
```

#### Lösung A: Alle Layer rendern (Quick Fix)
```
Minimap Camera auswählen:
  Culling Mask: Everything ?
  
oder im Script:
  minimapCamera.cullingMask = -1;
```

#### Lösung B: Objekte auf Minimap Layer setzen
```
1. Edit > Project Settings > Tags and Layers
2. Layer erstellen: "Minimap"
3. Terrain auswählen > Layer: Minimap
4. Alle Gebäude > Layer: Minimap
5. Alle Units > Layer: Minimap
```

### Problem 2: RenderTexture fehlt

#### Symptom:
```
Camera.targetTexture = null
? Camera rendert ins Void
? UI RawImage zeigt nichts
```

#### Lösung:
```
1. Project Window > Assets
2. Right-Click > Create > Render Texture
3. Name: "MinimapRT"
4. Size: 512x512

5. Minimap Camera auswählen:
   Target Texture: MinimapRT ? Zuweisen

6. UI RawImage auswählen:
Texture: MinimapRT ? Zuweisen
```

### Problem 3: Camera Position/Rotation falsch

#### Symptom:
```
Camera schaut in falsche Richtung
oder ist zu weit weg
```

#### Lösung:
```
Minimap Camera Transform:
  Position: (0, 100, 0)  ? Hoch über Welt
  Rotation: (90, 0, 0)   ? Nach unten schauen
  
Camera Component:
  Projection: Orthographic ?
  Size: 50 (oder je nach Weltgröße)
```

### Problem 4: Terrain nicht sichtbar

#### Symptom:
```
Camera ist OK, aber Terrain wird nicht gerendert
```

#### Lösung:
```
Option A: Terrain Layer ändern
  Terrain > Layer: Minimap

Option B: Camera rendert alle Layer
  Camera > Culling Mask: Everything

Option C: Terrain Material prüfen
  Terrain Settings > Material: Standard
  (Manche Shader sind nicht mit Minimap kompatibel)
```

## ?? Checkliste:

Gehen Sie diese Liste durch:

```
? Minimap Camera existiert?
  ? Hierarchy: MinimapCamera gefunden

? Camera Einstellungen OK?
  ? Orthographic: true
  ? Orthographic Size: 50
  ? Culling Mask: Everything (oder Minimap mit Objekten)
? Clear Flags: Solid Color
  ? Background: Black

? Camera Position OK?
  ? Y-Position: Hoch (z.B. 100)
  ? Rotation: (90, 0, 0) oder nach unten

? RenderTexture zugewiesen?
  ? Camera.targetTexture: MinimapRT
  ? RawImage.texture: MinimapRT

? UI RawImage vorhanden?
  ? Canvas > MinimapImage gefunden
  ? Active und visible

? Objekte sichtbar?
  ? Terrain vorhanden
  ? Auf richtigem Layer (wenn Culling Mask spezifisch)
```

## ?? Typische Setups:

### Setup 1: Render Everything (Einfachst)

```
Minimap Camera:
  Position: (WorldCenter.x, 100, WorldCenter.z)
  Rotation: (90, 0, 0)
  Projection: Orthographic
  Size: 50
  Culling Mask: Everything  ? Rendert alles!
  Clear Flags: Solid Color
  Background: Black or Terrain Color
  Target Texture: MinimapRT

Vorteile:
  ? Einfach
  ? Funktioniert sofort
  ? Keine Layer-Konfiguration nötig

Nachteile:
  ? Rendert auch UI, Effekte, etc.
```

### Setup 2: Minimap Layer Only (Performance)

```
1. Layer erstellen: "Minimap"

2. Minimap Camera:
   Culling Mask: Minimap  ? Nur dieser Layer!

3. Objekte auf Layer setzen:
   Terrain > Layer: Minimap
   Buildings > Layer: Minimap
   Units > Layer: Minimap (oder separates Icon-System)

Vorteile:
  ? Bessere Performance
  ? Kontrolle über was gerendert wird
  ? Kann verschiedene Visuals für Minimap haben

Nachteile:
  ? Mehr Setup-Aufwand
  ? Objekte müssen manuell zugewiesen werden
```

## ?? Empfohlener Workflow:

### Für Entwicklung/Testing:
```
1. Camera > Culling Mask: Everything
   ? Minimap funktioniert sofort
   ? Schnelles Iterieren

2. Später optimieren mit Layer-System
   ? Wenn Performance Problem wird
```

### Für Produktion:
```
1. "Minimap" Layer erstellen
2. Camera nur auf Minimap Layer
3. Separate Minimap-Visualisierung
   (z.B. vereinfachte Meshes, Icons)
4. MinimapIcon/MinimapUnit System verwenden
```

## ?? Debug-Tipps:

### Tip 1: Scene View Camera positionieren
```
1. Scene View: Wählen Sie MinimapCamera
2. GameObject > Align View to Selected (Ctrl+Shift+F)
3. Sehen Sie was die Camera sieht?
   ? JA: RenderTexture/RawImage Problem
   ? NEIN: Camera Position/Layer Problem
```

### Tip 2: RenderTexture direkt prüfen
```
1. Project: Wählen Sie MinimapRT
2. Inspector: Sehen Sie Preview?
   ? JA: RawImage Problem
   ? NEIN: Camera Problem
```

### Tip 3: Frame Debugger verwenden
```
1. Window > Analysis > Frame Debugger
2. Enable
3. Play Mode
4. Suchen Sie nach MinimapCamera Draw Calls
   ? 0 Calls: Nichts zu rendern (Layer/Position Problem)
   ? >0 Calls aber schwarz: RenderTexture/Material Problem
```

### Tip 4: Temporary Test Camera
```
1. Duplicate Main Camera
2. Rename: "TestMinimapCam"
3. Position: (0, 100, 0)
4. Rotation: (90, 0, 0)
5. Target Texture: MinimapRT
6. Culling Mask: Everything
7. Play Mode
   ? Funktioniert? Original Camera ist falsch konfiguriert
```

## ?? Diagnostic Tool Output verstehen:

### Gutes Setup:
```
1. Minimap Camera: ? Found 'MinimapCamera'
   Culling Mask: Everything
   Clear Flags: SolidColor
   Background: Color (0.00, 0.00, 0.00, 1.00)

2. Render Texture: ? Found 'MinimapRT'
   Size: 512x512

3. UI RawImage: ? Found 'MinimapImage'
   Texture: MinimapRT

4. 'Minimap' Layer: ? Exists
   Objects on layer: 15

? Setup ist korrekt!
```

### Problematisches Setup:
```
1. Minimap Camera: ? Found 'MinimapCamera'
   Culling Mask: Minimap  ? Problem!
   ?? Camera only renders 'Minimap' layer
   ? Objects must be on Minimap layer!

4. 'Minimap' Layer: ? Exists
   Objects on layer: 0  ? Problem!
 ?? NO OBJECTS ON MINIMAP LAYER!
   ? This is why minimap is black!

? Entweder Camera ändern ODER Objekte auf Layer!
```

## ?? Quick Wins:

### Win 1: Camera Everything
```
Minimap Camera > Culling Mask: Everything
? 10 Sekunden, funktioniert sofort
```

### Win 2: Auto-Fix Tool
```
Tools > RTS > Minimap Fix & Setup
? Click Complete Auto-Fix
? 30 Sekunden, automatisch
```

### Win 3: Terrain Manual
```
Terrain auswählen > Layer: Minimap
Minimap Camera > Culling Mask: Minimap
? 1 Minute, professionell
```

## ?? Häufige Fehler:

### Fehler 1: Layer verwechseln
```
? Objekt.layer = "Minimap"  // String geht nicht!
? Objekt.layer = LayerMask.NameToLayer("Minimap")
```

### Fehler 2: Culling Mask vs Layer
```
Culling Mask: Was Camera RENDERT
Layer: Wo Objekt IST

Beide müssen passen!
```

### Fehler 3: RenderTexture nicht saved
```
? new RenderTexture() im Script
? RenderTexture als Asset erstellen
? Im Inspector zuweisen
```

### Fehler 4: Canvas Scale
```
Wenn Minimap zu klein/groß:
? Canvas Scaler > UI Scale Mode: Scale With Screen Size
? RawImage Size anpassen
(Nicht RenderTexture Größe!)
```

## ?? Zusammenfassung:

| Problem | Quick Fix | Zeit |
|---------|-----------|------|
| Schwarze Minimap | Camera > Culling Mask: Everything | 10s |
| Kein RenderTexture | Create > Assign | 1min |
| Falsche Position | Position: (0,100,0), Rotation: (90,0,0) | 30s |
| Layer Setup | Tools > Minimap Fix > Auto-Fix | 30s |

**Wichtigste Regel:** 
Camera Culling Mask MUSS zu Objekt Layers passen!

---

**Schnellhilfe:**
1. `Tools > RTS > Minimap Fix & Setup`
2. Click "Complete Auto-Fix"
3. Fertig! ???

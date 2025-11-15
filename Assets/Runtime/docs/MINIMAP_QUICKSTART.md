# Minimap System - Quick Start Guide

## ?? Schnelleinstieg (5 Minuten)

### Option 1: Automatisches Setup (Empfohlen)

1. **Erstellen Sie ein Setup GameObject**
   - Erstellen Sie leeres GameObject ? nennen Sie es "RTSSetup"
   - Add Component ? `RTSSetupHelper`

2. **Führen Sie Auto-Setup aus**
   - Rechtsklick auf RTSSetupHelper Component
   - Wählen Sie: **"Full Auto Setup (with Minimap)"**
   - ? Erstellt Canvas, Minimap UI und Controller

3. **Konfigurieren Sie World Bounds**
   - Wählen Sie MinimapController in Hierarchie
 - Passen Sie "World Size" an Ihr Terrain an
   - Standard: 200x200 (für 100x100 Unity Units)

4. **Fertig!** 
   - Drücken Sie Play
   - Alle Units mit TeamComponent erscheinen automatisch auf der Minimap
   - In Team-Farben (Blau/Rot/Grün/Grau)

---

## ?? Was wurde automatisch erstellt?

### UI Struktur
```
MinimapCanvas
??? MinimapContainer (Bottom-Right, 250x250)
    ??? MinimapImage (RenderTexture Anzeige)
    ??? IconContainer (Icons hier)
    ??? CameraViewIndicator (Zeigt Kamera-Bereich)

MinimapController (Verwaltet alles)
```

### Automatische Features
- ? **Team-Farben**: Units zeigen automatisch ihre Team-Farbe
- ? **Auto-Icons**: Alle TeamComponents bekommen Icons
- ? **Click-Navigation**: Klick auf Minimap ? Kamera springt dorthin
- ? **Camera View**: Zeigt sichtbaren Bereich der Hauptkamera
- ? **Real-time Update**: Icons folgen Units

---

## ?? Minimap anpassen

### Position ändern
```
1. Wählen Sie "MinimapContainer" in Hierarchie
2. RectTransform ? Anchors:
   - Bottom-Right: (1, 0) ? (1, 0)
   - Bottom-Left: (0, 0) ? (0, 0)
   - Top-Right: (1, 1) ? (1, 1)
3. Passen Sie Position und Size Delta an
```

### Größe ändern
```
MinimapContainer ? RectTransform
? Size Delta: (250, 250)   // Standard
  - Größer: (300, 300)
  - Kleiner: (200, 200)
```

### Farben anpassen
```
MinimapContainer ? Image Component
? Background: Dunkelgrau (0.1, 0.1, 0.1, 0.9)
  - Transparent: Alpha auf 0.5 setzen
  - Border hinzufügen: Image Type ? Sliced
```

---

## ?? Icon-Typen für verschiedene Units

### Automatisch durch MinimapUnit Component

```csharp
// Auf Unit GameObject:
MinimapUnit unit = GetComponent<MinimapUnit>();

// Verschiedene Icon Shapes:
unit.SetIconShape(MinimapIcon.IconShape.Circle);   // Normale Units
unit.SetIconShape(MinimapIcon.IconShape.Square);   // Gebäude
unit.SetIconShape(MinimapIcon.IconShape.Triangle); // Militär/Fahrzeuge
unit.SetIconShape(MinimapIcon.IconShape.Diamond);  // Helden/Spezial
unit.SetIconShape(MinimapIcon.IconShape.Cross);    // Objectives/Marker

// Icon-Größe:
unit.SetIconSize(10f);  // Standard
unit.SetIconSize(15f);  // Größer für wichtige Units
unit.SetIconSize(8f);   // Kleiner für unwichtige Units
```

### Manuell Icons erstellen

```csharp
MinimapController controller = FindObjectOfType<MinimapController>();
MinimapIcon icon = controller.CreateIconForUnit(
    unitTransform, 
    MinimapIcon.IconShape.Triangle
);

// Farbe überschreiben (optional)
icon.SetColor(Color.yellow);

// Größe anpassen
icon.SetIconSize(12f);
```

---

## ?? World Bounds konfigurieren

### Automatische Erkennung (Terrain)
```
1. Wählen Sie MinimapController
2. Inspector ? World Bounds Section
3. Click: "Auto-Detect from Terrain"
? Bounds automatisch gesetzt!
```

### Manuelle Konfiguration
```
MinimapController Inspector:

World Center: (0, 0, 0)   // Mittelpunkt der Welt
World Size: (200, 200)      // Breite x Tiefe in Unity Units

Beispiele:
- Kleine Map: (100, 100)
- Standard RTS: (200, 200)
- Große Map: (500, 500)
```

### Bounds testen
```
? Gizmos sind aktiviert (im Scene View)
? Cyan Wireframe Box zeigt World Bounds
? Yellow Sphere zeigt Minimap Camera Position
```

---

## ?? Performance-Optimierung

### Für viele Units (100+)
```
MinimapController:
? Update Interval: 3-5
  (Aktualisiert Icons nur alle N Frames)
  
? Auto Create Icons: ?
  (Manuell Icons für wichtige Units erstellen)

Icon Settings:
? Icon Size: 8-10 (Kleinere Icons)
? Rotate With Unit: ? (Keine Rotation)
```

### Für wenige Units (1-50)
```
MinimapController:
? Update Interval: 0-1
  (Smooth updates jeden Frame)
  
? Auto Create Icons: ?
  (Automatisch für alle Units)

Icon Settings:
? Icon Size: 10-12 (Sichtbare Icons)
? Rotate With Unit: ? (Zeigt Blickrichtung)
```

---

## ?? Interaktion

### Click-to-Navigate
```
MinimapController:
? Enable Click Navigation: ?
? Main Camera: Main Camera (Drag & Drop)
? Camera Height Offset: 20

Funktion:
- Klick auf Minimap
? Hauptkamera springt zu dieser Position
? Behält aktuelle Höhe bei
```

### Camera View Indicator
```
MinimapController:
? Show Camera View Indicator: ?
? View Indicator Color: Weiß mit Alpha 0.3

Funktion:
- Zeigt sichtbaren Bereich der Hauptkamera auf Minimap
- Aktualisiert sich in Echtzeit
- Hilft bei Orientierung
```

---

## ?? Integration mit Team System

### Automatische Integration
```
Units mit TeamComponent:
? Farbe wird automatisch synchronisiert
? Team-Wechsel ? Icon-Farbe aktualisiert sich
? Keine manuelle Konfiguration nötig!
```

### Team-Farben auf Minimap
```
Player (Spieler):   Helles Blau
Enemy (Feind):      Helles Rot
Ally (Verbündeter): Helles Grün
Neutral:            Grau
```

### Runtime Team-Wechsel
```csharp
// Team wechseln
TeamComponent team = unit.GetComponent<TeamComponent>();
team.SetTeam(Team.Ally);
team.SetTeamColor(Color.green);

// Icon aktualisiert sich automatisch!
// Kein manueller Code nötig.
```

---

## ??? Erweiterte Nutzung

### Units zur Laufzeit spawnen
```csharp
// Unit spawnen
GameObject unit = Instantiate(unitPrefab, position, rotation);

// TeamComponent hinzufügen
TeamComponent team = unit.AddComponent<TeamComponent>();
team.SetTeam(Team.Player);

// MinimapUnit hinzufügen (optional, für Auto-Icon)
MinimapUnit minimapUnit = unit.AddComponent<MinimapUnit>();
minimapUnit.SetIconShape(MinimapIcon.IconShape.Circle);
minimapUnit.CreateIcon();

// ODER: Manuell Icon erstellen
MinimapController controller = FindObjectOfType<MinimapController>();
controller.CreateIconForUnit(unit.transform);
```

### Icons dynamisch anpassen
```csharp
MinimapIcon icon = controller.GetIconForUnit(unitTransform);

// Sichtbarkeit ändern (z.B. Fog of War)
icon.SetVisible(false); // Verstecken
icon.SetVisible(true);  // Zeigen

// Farbe ändern (z.B. bei Status-Effekten)
icon.SetColor(Color.yellow); // Warnung
icon.SetColor(Color.red);    // Gefahr

// Größe ändern (z.B. bei Zoom-Level)
icon.SetIconSize(15f); // Größer
icon.SetIconSize(8f);  // Kleiner
```

### Minimap Camera anpassen
```
MinimapController:

Camera Height: 100       // Höhe über Terrain
  - Höher: 150-200 (Mehr Overview)
  - Niedriger: 50-80 (Mehr Detail)

Camera Size: 50      // Orthographic Size
  - Größer: 80-100 (Mehr Welt sichtbar)
  - Kleiner: 30-40 (Mehr Detail, weniger Welt)
```

---

## ?? Troubleshooting

### Problem: "Keine Icons sichtbar"
**Lösung 1:** Prüfen Sie Play Mode (Icons werden zur Laufzeit erstellt)
**Lösung 2:** Check "Auto Create Icons" ist aktiviert
**Lösung 3:** Units haben TeamComponent?

### Problem: "Icons falsche Position"
**Lösung:** World Bounds anpassen
- World Size muss Ihr Terrain abdecken
- World Center sollte Terrain-Mittelpunkt sein
- Nutzen Sie "Auto-Detect from Terrain"

### Problem: "Icons falsche Farbe"
**Lösung 1:** TeamComponent.TeamColor prüfen
**Lösung 2:** MinimapController ? "Refresh All Icon Colors"
**Lösung 3:** Color Override in MinimapIcon auf (0,0,0,0) setzen

### Problem: "Minimap zu klein/groß"
**Lösung:** MinimapContainer ? Size Delta anpassen
- Standard: 250x250
- Klein: 200x200
- Groß: 300x300

### Problem: "Click Navigation funktioniert nicht"
**Lösung 1:** "Enable Click Navigation" aktivieren
**Lösung 2:** Main Camera Reference zuweisen
**Lösung 3:** MinimapContainer braucht GraphicRaycaster auf Canvas

### Problem: "Performance-Einbruch bei vielen Units"
**Lösung:**
```
MinimapController:
? Update Interval: 5
? Icon Size: 8 (Kleinere Icons)

MinimapIcon:
? Rotate With Unit: ?
```

### Problem: "Minimap zeigt schwarzes Bild"
**Lösung 1:** RenderTexture zuweisen
**Lösung 2:** Minimap Camera Target Texture prüfen
**Lösung 3:** Layer "Minimap" erstellen und Units zuweisen

---

## ?? Komponenten-Übersicht

### MinimapController
**Funktion:** Hauptsteuerung des Minimap-Systems
**Pflicht:** Ja (eine pro Szene)
**Setup:** Automatisch via RTSSetupHelper

### MinimapIcon
**Funktion:** Repräsentiert eine Unit auf der Minimap
**Pflicht:** Pro Unit (automatisch erstellt)
**Setup:** Automatisch erstellt vom Controller

### MinimapUnit
**Funktion:** Einfache Auto-Setup Komponente für Units
**Pflicht:** Nein (Alternative: Manuell Icons erstellen)
**Setup:** Add Component ? MinimapUnit

---

## ?? Best Practices

### RTS-Spiele
```
? Icon Shape:
  - Circle: Normale Units
  - Square: Gebäude
  - Triangle: Militär

? Icon Size:
  - Buildings: 12-15
  - Units: 8-10
  - Resources: 6-8

? Update Interval: 2-3
? Click Navigation: ?
? Camera View Indicator: ?
```

### MOBA-Spiele
```
? Icon Shape:
  - Diamond: Heroes
  - Circle: Minions
  - Square: Towers

? Icon Size:
  - Heroes: 15
  - Minions: 8
  - Towers: 12

? Update Interval: 1
? Rotate With Unit: ?
? Click Navigation: ?
```

### Top-Down Shooter
```
? Icon Shape:
  - Triangle: Alle Units (zeigt Richtung)

? Icon Size: 10-12
? Update Interval: 0 (Smooth)
? Rotate With Unit: ?
? Click Navigation: ? (Nicht typisch)
```

---

## ? Schnell-Referenz

### Setup (Neue Szene)
```
1. RTSSetupHelper ? "Full Auto Setup (with Minimap)"
2. MinimapController ? Adjust World Bounds
3. Play ? Fertig!
```

### Unit hinzufügen
```
1. Unit ? Add Component ? TeamComponent
2. Unit ? Add Component ? MinimapUnit (optional)
3. Play ? Icon erscheint automatisch
```

### Icon-Farbe ändern
```csharp
TeamComponent team = GetComponent<TeamComponent>();
team.SetTeamColor(Color.cyan);
// Icon aktualisiert sich automatisch
```

### Icon verstecken
```csharp
MinimapIcon icon = controller.GetIconForUnit(transform);
icon.SetVisible(false);
```

### Minimap verschieben
```
MinimapContainer ? RectTransform
- Bottom-Right: Anchor (1, 0)
- Bottom-Left: Anchor (0, 0)
- Top-Right: Anchor (1, 1)
```

---

## ?? Weitere Informationen

**Team Visual System:** `TEAM_VISUAL_QUICKSTART.md`
**Production System:** `PROGRESS_QUICK_REF.md`
**API Dokumentation:** Siehe Code-Kommentare in Komponenten

---

## ? Quick Tips

?? **Tipp 1:** Minimap funktioniert sofort mit TeamComponent - keine extra Konfiguration!

?? **Tipp 2:** Nutzen Sie verschiedene Icon Shapes für bessere Übersicht

?? **Tipp 3:** Update Interval erhöhen spart Performance bei vielen Units

?? **Tipp 4:** Camera View Indicator hilft Spielern bei Orientierung

?? **Tipp 5:** Auto-Detect World Bounds spart Zeit bei Terrain-Maps

---

**Zeit bis funktionale Minimap: ~2-5 Minuten! ??**

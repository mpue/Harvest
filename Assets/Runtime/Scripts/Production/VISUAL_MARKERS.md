# Visual Markers für Production System

## SpawnPoint Gizmo (Grün)
Der SpawnPoint wird mit folgenden visuellen Elementen dargestellt:

**Normale Ansicht:**
- ?? Grüner Wire Sphere (Hauptmarker)
- ?? Kleiner solider Sphere in der Mitte
- ?? Richtungspfeil (zeigt Spawn-Richtung)
- ? Kreis auf dem Boden

**Bei Selection:**
- ?? Größerer Wire Sphere
- ?? Grid auf dem Boden (4x4 Linien)
- ?? Label mit "Spawn Point" und GameObject-Name

**Konfigurierbar:**
- Farbe (Standard: Grün)
- Größe (Standard: 1.0)
- Label anzeigen (Standard: An)
- Pfeil anzeigen (Standard: An)

## RallyPoint Gizmo (Blau)
Der RallyPoint wird mit folgenden visuellen Elementen dargestellt:

**Normale Ansicht:**
- ?? Blauer Wire Sphere
- ?? Flagge (Pole + Dreiecksflagge)
- ? Kreis auf dem Boden

**Bei Selection:**
- ?? Größerer Wire Sphere
- ?? Größere Flagge (1.5x)
- ?? Grid auf dem Boden (4x4 Linien)
- ?? Label mit "Rally Point" und GameObject-Name

**Konfigurierbar:**
- Farbe (Standard: Blau)
- Größe (Standard: 1.0)
- Label anzeigen (Standard: An)
- Flagge anzeigen (Standard: An)

## Verwendung

### Automatisch
Die Komponenten werden automatisch hinzugefügt, wenn die ProductionComponent ein Spawn Point oder Rally Point erstellt.

### Manuell
1. Erstelle ein leeres GameObject
2. Füge `SpawnPoint.cs` oder `RallyPoint.cs` hinzu
3. Positioniere das GameObject im Scene View
4. Die Gizmos werden automatisch angezeigt

### Im ProductionComponent Editor
Der Custom Inspector bietet Buttons zum:
- Erstellen von Spawn/Rally Points (inkl. Komponenten)
- Hinzufügen der Komponenten zu bestehenden Points

### Anpassung
- Wähle den Spawn/Rally Point im Hierarchy aus
- Im Inspector kannst du Farbe, Größe und Sichtbarkeitsoptionen anpassen
- Änderungen werden sofort im Scene View sichtbar

## Ähnlich wie ShotPoint
Die Implementierung folgt dem gleichen Muster wie `ShotPoint.cs`:
- Verwendet `[ExecuteInEditMode]` für Editor-Visualisierung
- Implementiert `OnDrawGizmos()` und `OnDrawGizmosSelected()`
- Zeigt verschiedene Details basierend auf Selection-Status
- Konfigurierbar über Inspector

## Vorteile
? Sofort sichtbar im Scene View
? Keine zusätzlichen Meshes/Renderer nötig
? Nur im Editor sichtbar, nicht zur Laufzeit
? Einfach zu positionieren und anzupassen
? Klare visuelle Unterscheidung (Grün = Spawn, Blau = Rally)

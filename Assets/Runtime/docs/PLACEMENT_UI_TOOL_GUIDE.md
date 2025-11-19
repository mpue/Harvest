# Building Placement UI Setup Tool

## ?? Automatisches UI-Generierungs-Tool

Dieses Editor-Tool erstellt automatisch ein vollständiges Building Placement UI Panel mit allen benötigten Komponenten.

## Öffnen des Tools

```
Unity Menu Bar > Tools > RTS > Setup Building Placement UI
```

## Was das Tool erstellt

### 1. Canvas (falls nicht vorhanden)
- Screen Space Overlay
- Canvas Scaler mit 1920x1080 Reference Resolution
- Graphic Raycaster
- Event System (falls benötigt)

### 2. Placement Panel
```
BuildingPlacementPanel
?? Image (Background, schwarz mit 80% Alpha)
?? Content
   ?? BuildingName (Text: "Placing: Building Name")
   ?? StatusContainer
   ?  ?? StatusIcon (grün/rot)
   ?  ?? StatusText (Text: "Ready to Place")
   ?? Instructions (Text: "Q/E: Rotate | Right Click/ESC: Cancel")
```

### 3. BuildingPlacementUI Component
- Automatisch verdrahtet mit allen UI-Elementen
- Verbunden mit BuildingPlacement (falls vorhanden)
- Konfigurierte Farben (Grün/Rot)

## Verwendung

### Quick Setup (3 Klicks):

1. **Öffnen:** Tools > RTS > Setup Building Placement UI
2. **Konfigurieren:** (optional, Defaults funktionieren)
3. **Erstellen:** Click "Create Placement UI"

? Fertig!

### Complete Setup (1 Klick):

Klicken Sie auf **"Create Complete Setup (All-in-One)"** für:
- BuildingPlacement System
- Canvas mit UI Panel
- Test-Boden (optional)
- Alle Komponenten verdrahtet

## Tool-Optionen

### Settings

| Option | Beschreibung | Default |
|--------|--------------|---------|
| **Panel Name** | Name des erstellten GameObjects | "BuildingPlacementPanel" |
| **Use TextMeshPro** | TMPro statt Legacy UI Text | ? (empfohlen) |
| **Valid Color** | Farbe für gültige Platzierung | Grün |
| **Invalid Color** | Farbe für ungültige Platzierung | Rot |

### References

| Feld | Beschreibung |
|------|--------------|
| **Target Canvas** | Canvas für UI (wird automatisch gesucht) |
| **Building Placement** | BuildingPlacement Komponente (wird automatisch gesucht) |

### Buttons

| Button | Aktion |
|--------|--------|
| **Auto-Find References** | Sucht Canvas und BuildingPlacement automatisch |
| **Create Placement UI** | Erstellt nur das UI Panel |
| **Create BuildingPlacement System** | Erstellt BuildingPlacement GameObject |
| **Create Complete Setup** | Erstellt alles auf einmal |

## Schritt-für-Schritt Anleitung

### Variante A: Einzelne Schritte

#### 1. BuildingPlacement System erstellen
```
1. Tools > RTS > Setup Building Placement UI
2. Click "Create BuildingPlacement System"
```
? Erstellt: GameObject mit BuildingPlacement + BuildingPlacementDebug

#### 2. UI Panel erstellen
```
3. Click "Auto-Find References" (findet das eben erstellte System)
4. Click "Create Placement UI"
```
? Erstellt: Canvas + BuildingPlacementPanel + alle UI-Elemente

### Variante B: Alles auf einmal

```
1. Tools > RTS > Setup Building Placement UI
2. Click "Create Complete Setup (All-in-One)"
3. Bei Dialog "Create Test Ground?" ? Yes (wenn kein Boden vorhanden)
```
? Fertig - Alles erstellt und verbunden!

## Was danach passiert

### Im Hierarchy:
```
BuildingPlacementSystem
?? BuildingPlacement
?? BuildingPlacementDebug

Canvas
?? BuildingPlacementPanel
?  ?? Content
?   ?? BuildingName
?  ?? StatusContainer
?     ?  ?? StatusIcon
??  ?? StatusText
?     ?? Instructions
?? (andere UI Elemente)

Ground (optional)

EventSystem (falls nicht vorhanden)
```

### Im Inspector (BuildingPlacementPanel):
```
BuildingPlacementUI Component:
  Building Placement: [automatisch zugewiesen]
  Placement Panel: [self]
  Building Name Text: [BuildingName]
  Status Text: [StatusText]
  Instructions Text: [Instructions]
  Status Icon: [StatusIcon]
  Valid Color: Green
  Invalid Color: Red
```

## Anpassungen nach Erstellung

### Position ändern:
```
BuildingPlacementPanel (RectTransform):
Anchor: Top Center (0.5, 1)
  Position: (0, -20)
  Size: (400, 100)
```

### Aussehen anpassen:
```
BuildingPlacementPanel > Image:
  Color: (0, 0, 0, 0.8) [Schwarz, 80% transparent]
  
Kann geändert werden zu:
  - (1, 1, 1, 0.3) für weiß/transparent
  - (0.2, 0.2, 0.2, 0.9) für dunkelgrau
  - Sprite zuweisen für custom Background
```

### Text-Stil anpassen:
```
Alle Text-Elemente können angepasst werden:
  - Font
  - Font Size
  - Color
  - Alignment
  - Etc.
```

### Layout anpassen:
```
Content > VerticalLayoutGroup:
  Spacing: 5 ? ändern für mehr/weniger Abstand
  Padding: Add für Innenabstand
  
Content > RectTransform > OffsetMin/Max:
  (10, 10) ? (-10, -10) für Padding des Containers
```

## Fehlerbehandlung

### "No Canvas found. A new Canvas will be created."
**Info:** Normal - Tool erstellt automatisch einen Canvas

### "No BuildingPlacement found in scene."
**Lösung:** 
- Click "Create BuildingPlacement System" zuerst
- Oder "Create Complete Setup"

### "BuildingPlacement exists"
**Dialog:** Vorhandenes verwenden oder neues erstellen?
- **Select:** Wählt vorhandenes aus
- **Create New:** Erstellt zusätzliches System

## Features

### ? Automatische Verdrahtung
Alle Komponenten werden automatisch miteinander verbunden:
- BuildingPlacementUI ? BuildingPlacement
- BuildingPlacementUI ? UI-Elemente
- Farben konfiguriert

### ? Layout Groups
Automatisches Layout mit:
- VerticalLayoutGroup für Content
- HorizontalLayoutGroup für Status (Icon + Text)
- Proper Spacing und Alignment

### ? Undo Support
Alle Änderungen unterstützen Unity Undo:
- Ctrl+Z um Erstellung rückgängig zu machen

### ? Smart Defaults
- Panel startet versteckt (activeSelf = false)
- Wird automatisch bei Platzierung angezeigt
- Passt sich an Placement-Status an

## Integration mit bestehendem Setup

### Wenn Sie bereits einen Canvas haben:
1. Tool findet ihn automatisch ("Auto-Find References")
2. UI wird als Child des Canvas erstellt

### Wenn Sie bereits BuildingPlacement haben:
1. Tool findet es automatisch
2. Verbindet UI damit
3. Keine Duplikate

### Wenn Sie bereits beides haben:
1. Tool nutzt vorhandene Komponenten
2. Erstellt nur das UI Panel

## Testen

### Nach Erstellung:
```
1. Play Mode starten
2. Headquarter auswählen
3. Gebäude produzieren
4. Nach Fertigstellung ? Panel erscheint automatisch oben
5. Grün = kann platzieren
6. Rot = kann nicht platzieren
```

### Debug-Modus:
```
BuildingPlacementSystem > BuildingPlacementDebug:
  Show Raycast Debug: ?
  Show Ground Hit Point: ?
  
Zeigt zusätzliches Debug-UI links oben
```

## Erweiterte Verwendung

### Multiple Placement Panels:
Wenn Sie verschiedene Stile möchten:
```
1. Erstellen Sie erstes Panel: "BuildingPlacementPanel"
2. Panel Name ändern zu: "BuildingPlacementPanel_Style2"
3. Erstellen Sie zweites Panel
4. Beide Panels können koexistieren
```

### Custom Positionierung:
```csharp
// Nach Erstellung im Code:
GameObject panel = GameObject.Find("BuildingPlacementPanel");
RectTransform rect = panel.GetComponent<RectTransform>();

// Bottom Center
rect.anchorMin = new Vector2(0.5f, 0);
rect.anchorMax = new Vector2(0.5f, 0);
rect.pivot = new Vector2(0.5f, 0);
rect.anchoredPosition = new Vector2(0, 20);

// Top Right
rect.anchorMin = new Vector2(1, 1);
rect.anchorMax = new Vector2(1, 1);
rect.pivot = new Vector2(1, 1);
rect.anchoredPosition = new Vector2(-20, -20);
```

### Icons/Sprites hinzufügen:
```
StatusIcon > Image:
  Sprite: [Ihre Icon Sprite]
  
Oder in BuildingPlacementUI Code:
  public Sprite validIcon;
  public Sprite invalidIcon;
  // Update icon basierend auf Status
```

## Vorteile gegenüber manuellem Setup

| Manual | Mit Tool |
|--------|----------|
| 15-20 Minuten | 30 Sekunden |
| Fehleranfällig | Fehlerfreie Verdrahtung |
| Manuelles Layout | Automatic Layout Groups |
| Vergessene Referenzen | Alle automatisch zugewiesen |
| Keine Undo-Support | Voll Undo-fähig |

## Best Practices

### 1. Nutzen Sie Complete Setup
Für neue Projekte: "Create Complete Setup" spart Zeit

### 2. Verwenden Sie TextMeshPro
Bessere Text-Qualität und Performance

### 3. Behalten Sie Default-Namen
Erleichtert späteres Finden der Elemente

### 4. Testen Sie nach Erstellung
Kurzer Play-Mode Test um zu verifizieren

### 5. Customize nach Bedarf
Tool erstellt funktionierenden Basis - anpassen nach Geschmack

## Troubleshooting

### Panel erscheint nicht im Game View
**Prüfen:**
- Canvas vorhanden?
- EventSystem vorhanden?
- Panel ist Child von Canvas?
- Panel startet versteckt (normal) - wird bei Platzierung angezeigt

### Texte werden nicht angezeigt
**Prüfen:**
- Font zugewiesen? (Arial ist Fallback)
- TextMeshPro package installiert? (falls Use TMP = true)
- Text Color ist nicht transparent?

### Layout ist durcheinander
**Lösung:**
- Content > VerticalLayoutGroup vorhanden?
- RectTransforms korrekt?
- Click "Rebuild" im Layout Group Inspector

### Referenzen fehlen
**Lösung:**
- BuildingPlacementUI Inspector öffnen
- Fehlende Referenzen manuell zuweisen
- Oder Panel löschen und neu erstellen

## Support & Feedback

Das Tool ist erweiterbar! Mögliche Ergänzungen:
- Verschiedene Preset-Styles
- Animation beim Ein-/Ausblenden
- More UI elements (Cancel button, etc.)
- Theme-Support (Hell/Dunkel)

---

**Entwickelt für:** Unity RTS Building System
**Version:** 1.0
**Kompatibel mit:** Unity 2019.4+

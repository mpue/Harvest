# Production System Documentation

## Übersicht
Das Production System ermöglicht es BaseUnits, Entitäten zu produzieren. Wenn eine BaseUnit mit ProductionComponent angeklickt wird, öffnet sich ein ProductionPanel, in dem der Spieler Einheiten zur Produktion auswählen kann.

## Komponenten

### 1. Product (ScriptableObject)
Definiert eine produzierbare Entität.

**Eigenschaften:**
- **Product Name**: Name der Einheit
- **Preview Image**: Vorschaubild für das UI
- **Description**: Beschreibung der Einheit
- **Prefab**: Das GameObject, das produziert wird
- **Production Duration**: Dauer der Produktion in Sekunden
- **Costs**: Ressourcenkosten (Food, Wood, Stone, Gold)

**Erstellen:**
- Rechtsklick im Project Window → Create → RTS → Production → Product
- ScriptableObject konfigurieren
- Prefab zuweisen

### 2. ProductionComponent
Komponente, die einer BaseUnit hinzugefügt wird, um Produktionsfähigkeiten zu erhalten.

**Eigenschaften:**
- **Available Products**: Liste der produzierbaren Produkte
- **Spawn Point**: Wo die Einheiten gespawnt werden (hat automatisch SpawnPoint-Komponente)
- **Max Queue Size**: Maximale Anzahl von Produktionen in der Warteschlange
- **Rally Point**: Wohin die produzierten Einheiten sich bewegen (hat automatisch RallyPoint-Komponente)

**Features:**
- Produktionswarteschlange
- Fortschrittsanzeige
- Produktion abbrechen
- Rally Point setzen
- Automatische Erstellung von Spawn/Rally Points mit visuellen Markern

### 3. SpawnPoint Component
Visuelle Marker-Komponente für Spawn Points im Editor.

**Features:**
- Grüne Gizmos im Scene View (Sphere, Pfeil, Kreis)
- Zeigt Spawn-Richtung an
- Konfigurierbare Farbe und Größe
- Grid-Overlay bei Selection
- Label mit Name

### 4. RallyPoint Component
Visuelle Marker-Komponente für Rally Points im Editor.

**Features:**
- Blaue Gizmos im Scene View (Flagge, Kreis)
- Zeigt Rally Point Position deutlich an
- Konfigurierbare Farbe und Größe
- Grid-Overlay bei Selection
- Label mit Name

### 5. ProductionPanel
UI Panel zur Anzeige und Verwaltung der Produktion.

**Features:**
- Zeigt alle verfügbaren Produkte an
- Zeigt aktuelle Produktionswarteschlange
- Progress Bar für aktuelle Produktion
- Cancel/Clear Queue Buttons

### 6. ProductionSlot
UI Element für ein einzelnes Produkt im Panel.

**Features:**
- Zeigt Produktbild, Name, Kosten und Dauer an
- Progress Bar für Warteschlangen-Items
- Button zum Hinzufügen zur Produktion

### 7. ProductionUIManager
Singleton Manager für das Production UI System.

## Setup

### Schritt 1: Products erstellen
1. Erstelle ScriptableObjects für jede Einheit, die produziert werden soll
2. Konfiguriere Name, Bild, Kosten, Dauer und Prefab

### Schritt 2: BaseUnit mit Production ausstatten
1. Wähle eine BaseUnit (z.B. Barrack, Factory)
2. Füge die `ProductionComponent` hinzu
3. Füge Products zur `Available Products` Liste hinzu
4. Optional: Setze Spawn Point und Rally Point

### Schritt 3: UI Setup
1. Erstelle ein Canvas in der Szene (falls noch nicht vorhanden)
2. Erstelle das ProductionPanel UI:
   ```
   Canvas
   └── ProductionPanel (GameObject mit ProductionPanel.cs)
       ├── Title (TextMeshProUGUI)
       ├── ProductsContainer (mit Grid Layout Group)
   │   └── ProductSlotPrefab (Template)
       ├── QueueContainer (mit Horizontal Layout Group)
  │   └── QueueSlotPrefab (Template)
       ├── QueueCountText (TextMeshProUGUI)
       └── Buttons
           ├── CloseButton
      ├── CancelCurrentButton
         └── ClearQueueButton
   ```

3. Erstelle ProductSlot Prefab:
   ```
   ProductSlot (GameObject mit ProductionSlot.cs)
   ├── ProductImage (Image)
   ├── ProductName (TextMeshProUGUI)
   ├── CostText (TextMeshProUGUI)
   ├── DurationText (TextMeshProUGUI)
   ├── Button (Button)
   └── ProgressBar (GameObject)
       └── ProgressFill (Image mit Fill Amount)
   ```

4. Erstelle ProductionUIManager GameObject:
   - Erstelle leeres GameObject "ProductionUIManager"
   - Füge `ProductionUIManager.cs` Script hinzu
   - Weise ProductionPanel Referenz zu

### Schritt 4: ProductionPanel konfigurieren
1. Weise alle UI Referenzen im Inspector zu
2. Setze ProductSlotPrefab und QueueSlotPrefab
3. Teste das Panel im Editor

## Verwendung

### Zur Laufzeit
1. Klicke auf eine BaseUnit mit ProductionComponent
2. Das ProductionPanel öffnet sich automatisch
3. Klicke auf ein Produkt, um es zur Warteschlange hinzuzufügen
4. Die Produktion startet automatisch
5. Produzierte Einheiten werden am Spawn Point erstellt
6. Einheiten mit Controllable Component bewegen sich zum Rally Point

### Code API

```csharp
// Product zur Warteschlange hinzufügen
productionComponent.AddToQueue(product);

// Aktuelle Produktion abbrechen
productionComponent.CancelCurrentProduction();

// Warteschlange leeren
productionComponent.CancelQueue();

// Rally Point setzen
productionComponent.SetRallyPoint(position);

// Events abonnieren
productionComponent.OnProductionStarted += OnProductionStarted;
productionComponent.OnProductionCompleted += OnProductionCompleted;
productionComponent.OnProductionCancelled += OnProductionCancelled;
productionComponent.OnQueueChanged += OnQueueChanged;
```

## Erweiterungsmöglichkeiten

### Resource System Integration
Aktuell sind TODO-Kommentare im Code, wo das Resource System integriert werden sollte:
- `ProductionComponent.AddToQueue()`: Ressourcen prüfen und abziehen
- `ProductionComponent.CancelCurrentProduction()`: Ressourcen zurückerstatten
- `ProductionPanel.OnProductSelected()`: Ressourcen-Check vor dem Hinzufügen

### Sound Effects
TODO-Kommentare für Sound Integration:
- Beim Hinzufügen zur Warteschlange
- Bei Produktionsabschluss
- Bei Fehlern

### Weitere Features
- Batch Production (mehrere Einheiten auf einmal)
- Production Speed Modifiers
- Tech Tree Requirements
- Unit Limits
- Auto-Rally Point

## Troubleshooting

### Panel öffnet sich nicht
- Prüfe, ob ProductionUIManager in der Szene ist
- Prüfe, ob ProductionPanel korrekt zugewiesen ist
- Prüfe, ob BaseUnit die ProductionComponent hat

### Produkte werden nicht angezeigt
- Prüfe, ob Products in der ProductionComponent zugewiesen sind
- Prüfe, ob ProductSlotPrefab korrekt konfiguriert ist
- Prüfe Console auf Fehler

### Einheiten spawnen nicht
- Prüfe, ob Product.Prefab zugewiesen ist
- Prüfe, ob Spawn Point korrekt gesetzt ist
- Prüfe Console auf Fehler bei Instantiate

### Progress Bar funktioniert nicht
- Prüfe, ob ProgressFill Image-Komponente richtig zugewiesen ist
- Prüfe, ob Image Type auf "Filled" gesetzt ist
- Prüfe, ob Fill Method korrekt ist

## Performance Tipps
- Verwende Object Pooling für häufig produzierte Einheiten
- Limitiere die Max Queue Size
- Verwende Sprite Atlases für Product Images

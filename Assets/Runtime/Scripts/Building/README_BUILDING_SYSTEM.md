# Building System Documentation

## Overview
Das Building System ermöglicht es Spielern, Gebäude zu produzieren und zu platzieren. Nur das Headquarter kann Gebäude produzieren, und Gebäude benötigen Energie zum Betrieb.

## Komponenten

### 1. ResourceManager
Verwaltet Spieler-Ressourcen und Energie.

**Ressourcen:**
- Food (Nahrung)
- Wood (Holz)
- Stone (Stein)
- Gold

**Energie-System:**
- `MaxEnergy`: Maximale verfügbare Energie
- `CurrentEnergy`: Aktuell verbrauchte Energie
- `AvailableEnergy`: Verfügbare Energie (Max - Current)

**Wichtige Methoden:**
- `CanAfford()`: Prüft ob Spieler sich Kosten leisten kann
- `HasAvailableEnergy()`: Prüft ob genug Energie verfügbar ist
- `SpendResources()`: Gibt Ressourcen aus
- `ConsumeEnergy()`: Verbraucht Energie für ein Gebäude
- `ReleaseEnergy()`: Gibt Energie frei wenn Gebäude zerstört wird
- `IncreaseMaxEnergy()`: Erhöht maximale Energie (z.B. durch Energy Blocks)

### 2. Product (erweitert)
Definiert produzierbare Einheiten und Gebäude.

**Neue Eigenschaften:**
- `IsBuilding`: Ist es ein Gebäude?
- `BuildingType`: Typ des Gebäudes (Headquarter, EnergyBlock, etc.)
- `EnergyCost`: Energie-Kosten für den Betrieb
- `EnergyProduction`: Energie-Produktion (für Power Plants)

**Building Types:**
- `None`: Keine Gebäude-Einheit
- `Headquarter`: Hauptgebäude (kann Buildings produzieren)
- `EnergyBlock`: Energie-Kraftwerk (produziert Energie)
- `ProductionFacility`: Produktionsstätte
- `DefenseTower`: Verteidigungsturm
- `ResourceCollector`: Ressourcen-Sammler

### 3. BuildingPlacement
Behandelt die Platzierung von Gebäuden.

**Features:**
- **Platzierungs-Vorschau**: Zeigt Gebäude während der Platzierung
- **Validierung**: Prüft ob Platzierung möglich ist (Kollisionen, Energie)
- **Grid-Snapping**: Optional: Rastet Gebäude am Grid ein
- **Rotation**: Q/E zum Drehen des Gebäudes
- **Visuelles Feedback**: Grün = gültig, Rot = ungültig

**Steuerung:**
- Linke Maustaste: Gebäude platzieren
- Rechte Maustaste / ESC: Abbrechen
- Q: Links drehen
- E: Rechts drehen

**Parameter:**
- `groundLayer`: Layer für Boden-Erkennung
- `placementHeight`: Höhe über dem Boden
- `gridSize`: Größe des Rasters
- `snapToGrid`: Snap-to-Grid aktivieren
- `collisionCheckRadius`: Radius für Kollisionsprüfung

### 4. BuildingComponent
Component für alle Gebäude.

**Funktionen:**
- Verwaltet Energie-Verbrauch/-Produktion
- Behandelt Zerstörung (gibt Energie frei)
- Zeigt Power-Status an

**Eigenschaften:**
- `IsHeadquarter`: Ist es ein Headquarter?
- `IsEnergyProducer`: Produziert es Energie?
- `IsPowered`: Hat es genug Energie?

### 5. ProductionComponent (erweitert)
Erweitert um Building-Produktion.

**Neue Features:**
- Nur Headquarters können Gebäude produzieren
- Prüft Energie-Verfügbarkeit vor Produktion
- Startet Platzierungs-Modus wenn Building fertig ist
- Erstattet Ressourcen bei Abbruch zurück

**Building-Regeln:**
1. Nur Headquarters können Buildings produzieren
2. Gebäude benötigen Energie (außer Energy Blocks)
3. Wenn nicht genug Energie: Nur Energy Blocks baubar
4. Fertige Gebäude müssen platziert werden

### 6. GameManager
Verwaltet Spiel-Initialisierung.

**Funktionen:**
- Erstellt Resource Manager für jeden Spieler
- Spawnt Headquarters für jeden Spieler
- Verwaltet Startpositionen
- Zentrale Spiel-Logik

**Setup:**
- Weist jedem Spieler einen ResourceManager zu
- Erstellt Headquarters an Startpositionen
- Konfiguriert Teams

## Workflow

### Gebäude produzieren:
1. Spieler wählt Headquarter aus
2. Öffnet Production Panel
3. Wählt Gebäude aus Product-Liste
4. System prüft:
 - Ausreichend Ressourcen?
   - Genug Energie verfügbar? (außer Energy Blocks)
   - Queue nicht voll?
5. Bei Erfolg: Gebäude zur Queue hinzufügen
6. Ressourcen werden sofort ausgegeben

### Gebäude platzieren:
1. Wenn Produktion fertig: Platzierungs-Modus startet automatisch
2. Maus bewegen: Vorschau positionieren
3. Q/E: Gebäude drehen
4. Grün = gültige Position, Rot = ungültige Position
5. Linksklick: Platzieren
6. Energie wird beim Platzieren verbraucht
7. Bei Energy Blocks: Max-Energie wird erhöht

### Gebäude zerstören:
1. Wenn Gebäude zerstört wird (Health = 0)
2. Verbrauchte Energie wird freigegeben
3. Bei Energy Blocks: Max-Energie wird reduziert

## Energy-System Details

### Energie-Mechanik:
- **Max Energy**: Gesamte verfügbare Energie (startet bei 20)
- **Current Energy**: Von Gebäuden verbrauchte Energie
- **Available Energy**: MaxEnergy - CurrentEnergy

### Energy Block:
- Erhöht Max-Energie beim Platzieren
- Kann immer gebaut werden (auch bei 0 Energie)
- Reduziert Max-Energie beim Zerstören

### Andere Gebäude:
- Verbrauchen Energie beim Platzieren
- Können nur gebaut werden wenn genug Energie verfügbar
- Geben Energie frei beim Zerstören

## Integration in UI

### ProductionPanel Updates:
- Zeigt Ressourcen an (Food, Wood, Stone, Gold)
- Zeigt Energie an (Available/Max)
- Farb-Kodierung für Energie:
  - Grün: Viel verfügbar (>30%)
  - Gelb: Wenig verfügbar (<30%)
  - Rot: Keine verfügbar (0)

### ProductionSlot:
- Zeigt Kosten an (inkl. Energie)
- Deaktiviert Buttons wenn:
  - Nicht genug Ressourcen
  - Nicht genug Energie
  - Kein Headquarter (für Buildings)

## Setup-Anleitung

### 1. Scene Setup:
```
1. GameManager in Scene platzieren
2. Headquarter Prefab zuweisen
3. Start-Positionen konfigurieren
```

### 2. Headquarter Prefab:
```
Components benötigt:
- BaseUnit
- ProductionComponent
- BuildingComponent
- Health
- TeamComponent
```

### 3. Building Prefabs:
```
Components benötigt:
- BuildingComponent
- Health
- Collider (für Platzierungs-Validierung)
```

### 4. Products erstellen:
```
Create > RTS > Production > Product
- IsBuilding = true
- BuildingType setzen
- EnergyCost für normale Gebäude
- EnergyProduction für Energy Blocks
- Prefab zuweisen
```

### 5. Materials:
```
BuildingPlacement benötigt:
- Valid Placement Material (grün, transparent)
- Invalid Placement Material (rot, transparent)
```

## Best Practices

1. **Energy Blocks zuerst**: Spieler sollten Energy Blocks bauen bevor andere Gebäude
2. **Energy Management**: UI sollte deutlich zeigen wenn Energie knapp wird
3. **Strategische Entscheidungen**: Balance zwischen Gebäude-Produktion und Energie
4. **Platzierungs-Feedback**: Deutliche visuelle Unterscheidung gültig/ungültig
5. **Tutorial**: Spieler über Energie-System informieren

## Erweiterungsmöglichkeiten

- **Upgrade-System**: Gebäude upgraden für bessere Effizienz
- **Energie-Leitungen**: Gebäude müssen mit Stromnetz verbunden sein
- **Energie-Speicher**: Zusätzliche Gebäude zum Speichern von Energie
- **Dynamische Kosten**: Energie-Kosten basierend auf Gebäude-Aktivität
- **Power-Management**: Gebäude ein-/ausschalten um Energie zu sparen

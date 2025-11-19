# ?? Harvesting System - Complete Guide

## Übersicht

Ein vollständiges Resource-Gathering System mit Harvester-Units, die Collectables einsammeln und zu Resource Collectors bringen.

### System-Komponenten:
1. **Collectable** - Ressourcen in der Welt (Gold, Food, Wood, Stone)
2. **HarvesterUnit** - Units die Ressourcen einsammeln
3. **ResourceCollector** - Buildings die Ressourcen annehmen
4. **HarvestCommand** - Command System für Spieler-Kontrolle

### Workflow:
```
1. Spieler befiehlt Harvester
   ? Rechtsklick auf Collectable

2. Harvester geht zu Ressource
   ? State: MovingToResource

3. Harvester sammelt ein
   ? State: Harvesting
   ? Füllt Inventory (0/50 ? 50/50)

4. Harvester ist voll
   ? Findet nächsten Resource Collector
   ? State: MovingToCollector

5. Harvester lädt ab
   ? State: Unloading
 ? Ressourcen werden Spieler gutgeschrieben ?

6. Harvester kehrt zurück
   ? Zu selber Ressource (falls noch vorhanden)
   ? Oder: Idle

Loop!
```

## ? Quick Setup (5 Minuten):

### 1. Collectable erstellen:

```
1. GameObject erstellen (z.B. "Gold Vein")
   3D Model: Cube oder eigenes Model

2. Add Component > Collectable

3. Inspector konfigurieren:
   Resource Settings:
     Resource Type: Gold
     Resource Amount: 100
     Current Amount: 100 (auto)
     
   Harvest Settings:
     Harvest Time: 2  (Sekunden pro Harvest)
     Amount Per Harvest: 10
  
   Visual Settings:
     Visual Model: [Ihr Model]
     Deplete Visually: ?
     Depleted Scale: (0.5, 0.5, 0.5)

4. Collider hinzufügen:
   Box Collider oder Sphere Collider
   
5. Layer setzen (optional):
   Layer: "Collectable"
```

### 2. Harvester Unit erstellen:

```
1. Bestehende Unit nehmen oder neue erstellen

2. Add Components:
   - BaseUnit (falls nicht vorhanden)
   - Controllable (für Movement)
   - TeamComponent (für Team-Zuordnung)
   - HarvesterUnit

3. HarvesterUnit Inspector:
   Harvest Settings:
     Carry Capacity: 50
     Current Carried: 0 (auto)
     Carried Resource Type: Gold
     Harvest Range: 2
     
 References:
     Team Component: [Auto-found]
     Resource Manager: [Auto-found oder zuweisen]

   Visual Feedback:
     Carry Visual: [Optional GameObject das erscheint wenn voll]
     Carry Point: [Transform wo Carry Visual sitzt]

4. Prefab erstellen für Production
```

### 3. Resource Collector Building erstellen:

```
1. Building GameObject erstellen

2. Add Components:
   - BuildingComponent
   - ResourceCollector

3. ResourceCollector Inspector:
   Collector Settings:
     Unload Range: 3
     Unload Time: 2
 Accept All Resources: ?
     Accepted Resources: [Gold] (falls nicht all)
     
   Visual Feedback:
     Unload Effect: [Optional Particle Effect]
  Unload Sound: [Optional Audio Clip]
     Unload Point: [Transform für Effect]

4. BuildingComponent konfigurieren:
   Building Product: [Zuweisen]
   Resource Manager: [Zuweisen]

5. Als Product für Production hinzufügen
```

### 4. Harvest Command Setup:

```
1. GameManager oder leeres GameObject

2. Add Component > HarvestCommand

3. Inspector:
   Settings:
     Collectable Layer: [Layer für Collectables]
     Max Harvest Distance: 100
```

## ?? Verwendung im Spiel:

### Harvester befehligen:

```
1. Harvester Unit(s) selektieren

2. Methode A: Rechtsklick auf Collectable
   ? Harvester geht hin und sammelt

3. Methode B: Taste 'H' drücken
   ? Auto-Harvest: Findet nächste Ressource

4. Harvester arbeitet automatisch:
   ? Sammelt bis voll
   ? Bringt zu Collector
   ? Kehrt zurück
   ? Repeat!
```

### UI Anzeige:

```
Wenn Harvester selektiert:
??????????????????????????????????
? Harvester Commands:?
? Right-Click on Resource: Harvest?
? H: Auto-Harvest nearest resource?
??????????????????????????????????
```

## ?? States im Detail:

### Idle:
```
Harvester wartet auf Befehl
Farbe: Weiß
Verhalten: Steht still
```

### MovingToResource:
```
Harvester bewegt sich zur Ressource
Farbe: Cyan
Verhalten: Läuft zu Collectable
Check: Distance <= Harvest Range?
  ? JA: Wechsel zu Harvesting
```

### Harvesting:
```
Harvester sammelt Ressource
Farbe: Gelb
Verhalten: Steht bei Collectable
Timer: Harvest Time (z.B. 2 Sekunden)
  ? Timer voll: +10 Gold zum Inventory
  ? Inventory voll (50/50)? ? MovingToCollector
  ? Collectable leer? ? MovingToCollector oder Idle
```

### MovingToCollector:
```
Harvester bringt Ressourcen zurück
Farbe: Cyan
Verhalten: Läuft zu Resource Collector
Check: Distance <= Unload Range?
  ? JA: Wechsel zu Unloading
```

### Unloading:
```
Harvester lädt Ressourcen ab
Farbe: Grün
Verhalten: Steht bei Collector
Timer: Unload Time (z.B. 2 Sekunden)
  ? Timer voll: Ressourcen gutgeschrieben ?
  ? Inventory leer (0/50)
  ? Zurück zur Ressource oder Idle
```

## ?? Visual Features:

### Collectable Visuals:
```
Volle Ressource:
  Scale: (1, 1, 1)
  Color: Gold (für Gold Vein)

Halbvolle Ressource:
  Scale: (0.75, 0.75, 0.75)  ? Schrumpft visuell
  Color: Gold

Leere Ressource:
  Scale: (0.5, 0.5, 0.5)
  Effect: Depletion Effect
  Sound: Depletion Sound
  ? Zerstört sich nach 2 Sekunden
```

### Harvester Visuals:
```
Leer (0/50):
  Carry Visual: Inaktiv
  State Color: Weiß/Cyan

Voll (50/50):
  Carry Visual: Aktiv ?
  Position: Carry Point (z.B. auf Rücken)
  State Color: Gelb/Grün
```

### Resource Collector Visuals:
```
Beim Unloading:
  Effect: Unload Effect (Partikel)
  Sound: Unload Sound
  Position: Unload Point
```

## ?? Code-Beispiele:

### Harvester manuell befehlen:
```csharp
HarvesterUnit harvester = GetComponent<HarvesterUnit>();
Collectable collectable = FindObjectOfType<Collectable>();

harvester.GatherFrom(collectable);
```

### Auto-Harvest:
```csharp
HarvesterUnit harvester = GetComponent<HarvesterUnit>();
harvester.AutoGather(); // Findet nächste Ressource automatisch
```

### Status abfragen:
```csharp
HarvesterUnit harvester = GetComponent<HarvesterUnit>();

bool isFull = harvester.IsFull; // 50/50?
bool isEmpty = harvester.IsEmpty; // 0/50?
int carried = harvester.CurrentCarried; // Aktueller Betrag
HarvesterState state = harvester.CurrentState; // Idle, Harvesting, etc.
```

### Ressourcen manuell abliefern:
```csharp
ResourceCollector collector = FindObjectOfType<ResourceCollector>();
ResourceManager resourceManager = FindObjectOfType<ResourceManager>();

collector.DepositResources(ResourceType.Gold, 50, resourceManager);
// ? +50 Gold für Spieler
```

## ?? Customization:

### Verschiedene Ressourcen-Typen:

```
Gold Vein:
  Resource Type: Gold
  Amount: 100
  Color: Gelb
  
Tree:
  Resource Type: Wood
  Amount: 200
  Color: Braun
  
Stone Deposit:
  Resource Type: Stone
  Amount: 150
  Color: Grau
  
Berry Bush:
  Resource Type: Food
  Amount: 50
  Color: Rot
```

### Verschiedene Harvester-Typen:

```
Gold Miner:
  Carry Capacity: 50
  Harvest Range: 2
  ? Nur für Gold optimiert

Lumberjack:
  Carry Capacity: 100  ? Mehr Kapazität für Holz
  Harvest Range: 3     ? Größere Reichweite
  ? Für Holz optimiert

Universal Harvester:
  Carry Capacity: 75
  Harvest Range: 2.5
  ? Sammelt alles
```

### Collector-Spezialisten:

```
Gold Refinery:
  Accept All Resources: ?
  Accepted Resources: [Gold]
  Unload Time: 1.5  ? Schneller für Gold

Warehouse:
  Accept All Resources: ?
  Unload Time: 2.5  ? Langsamer aber flexibel

Lumber Mill:
  Accept All Resources: ?
  Accepted Resources: [Wood]
  Unload Time: 2
```

## ?? Balancing:

### Harvest Speed:
```
Schnell (für häufige Trips):
  Harvest Time: 1.0s
  Amount Per Harvest: 5
  Carry Capacity: 25
  ? Viele kurze Trips

Standard:
  Harvest Time: 2.0s
  Amount Per Harvest: 10
  Carry Capacity: 50
  ? Ausgewogen

Langsam (für wenige große Trips):
  Harvest Time: 3.0s
  Amount Per Harvest: 20
  Carry Capacity: 100
  ? Wenige lange Trips
```

### Resource Amount:
```
Klein (schnell leer):
  Resource Amount: 50
  ? Harvester muss oft neue Ressource finden

Mittel:
  Resource Amount: 100
  ? Standard

Groß (lange haltbar):
  Resource Amount: 500
  ? Mehrere Harvester können daraus sammeln
```

## ?? Debug & Troubleshooting:

### Problem: Harvester bewegt sich nicht

```
1. Controllable Component vorhanden?
   Harvester > Add Component > Controllable

2. NavMeshAgent konfiguriert?
   Controllable > Use Nav Mesh: ?
   NavMesh Agent Component vorhanden?

3. NavMesh in Scene vorhanden?
   Window > AI > Navigation > Bake NavMesh
```

### Problem: Harvester sammelt nicht

```
1. Distance zu Collectable zu groß?
   HarvesterUnit > Harvest Range: 2
   ? Näher kommen lassen

2. Collectable depleted?
   Collectable > Current Amount: > 0

3. Harvester bereits voll?
   HarvesterUnit > Current Carried: < Carry Capacity
```

### Problem: Ressourcen werden nicht gutgeschrieben

```
1. ResourceManager vorhanden?
   HarvesterUnit > Resource Manager: [Zugewiesen?]

2. ResourceCollector findet ResourceManager?
   Console: "Deposited 50 Gold" erscheint?

3. Resource Bar UI updated?
   Siehe Resource Bar Debug Guide
```

### Problem: Harvester findet keinen Collector

```
1. Resource Collector in Scene?
   Mindestens 1 ResourceCollector GameObject

2. Gleiche Team?
   Harvester > Team Component > Team: Player
   Collector > Team Component > Team: Player
   ? Müssen übereinstimmen!

3. Collector in Reichweite?
   Nicht zu weit weg
```

## ?? Pro-Tips:

### Tip 1: Mehrere Harvester auf eine Ressource
```
Großes Gold Vein:
  Resource Amount: 500
  
3 Harvester befehlen:
  ? Alle sammeln gleichzeitig
  ? Ressource wird schneller abgebaut
  ? Effizienter!
```

### Tip 2: Strategische Collector-Platzierung
```
Collector nah an Ressourcen platzieren:
  ? Kürzere Wege
  ? Mehr Ressourcen pro Minute
  ? Weniger Wartezeit
```

### Tip 3: Auto-Harvest für neue Ressourcen
```
Wenn Collectable leer:
  ? Harvester sucht automatisch neue Ressource
  ? Kein manueller Befehl nötig
  ? Taste 'H' für manuelles Trigger
```

### Tip 4: Carry Visual für Feedback
```
Carry Visual GameObject:
  ? Goldbarren-Model auf Rücken
  ? Erscheint wenn Harvester voll
  ? Verschwindet nach Unload
  ? Gibt Spieler visuelles Feedback
```

### Tip 5: Sound Effects
```
Harvest Sound: "Mining" / "Chopping"
Deplete Sound: "Empty" / "Done"
Unload Sound: "Deposit" / "Success"
? Besseres Audio-Feedback
```

## ?? Integration mit Production System:

### Harvester als Product:

```
1. Product erstellen:
   Assets > Create > RTS > Production > Product
   Name: "Harvester"

2. Product konfigurieren:
   Is Building: ?
   Prefab: [Harvester Prefab]
   Production Duration: 15
   Gold Cost: 100

3. Zu ProductionComponent hinzufügen:
   Headquarter > Production Component
   Available Products: [Add Harvester]

4. Harvester produzieren:
   Play Mode ? Headquarter auswählen
   ? Production Panel ? Harvester

5. Fertig!
   ? Unit wird gespawnt
   ? Befehligen mit Rechtsklick auf Ressource
```

### Resource Collector als Building:

```
1. Product erstellen:
   Name: "Gold Refinery"

2. Product konfigurieren:
   Is Building: ?
   Building Type: ResourceCollector
   Prefab: [Collector Prefab]
   Production Duration: 30
   Gold Cost: 200

3. Building platzieren:
   Nah an Gold Veins
   ? Kurze Wege für Harvester

4. Harvester automatisch nutzen:
 ? Finden nächsten Collector automatisch
```

## ?? Performance:

### Optimization:
```
Wenige Harvester (1-5):
  Update Interval: Jedes Frame OK
  
Viele Harvester (10+):
  Update nur wenn State ändert
  Cached Nearest Collector/Collectable
  NavMesh statt Manual Movement
```

### Best Practices:
```
? NavMesh für Movement verwenden
? Collectable Layer setzen (für Raycasts)
? Object Pooling für Harvest Effects
? Update Carry Visual nur bei Änderung
? FindObjectsOfType cachen wo möglich
```

## ?? Zusammenfassung:

**System:** Vollständig funktional ?
**Komponenten:** 4 (Collectable, Harvester, Collector, Command)
**Features:** Auto-pathfinding, Team-Support, Visual Feedback
**Integration:** Production System, Resource Manager, UI

**Setup-Zeit:** 5 Minuten
**Komplexität:** Mittel
**Extensibility:** Hoch (4 Ressourcen-Typen ready)

---

**Quick Start:**
1. Collectable in Scene platzieren
2. Harvester Unit erstellen (mit allen Components)
3. Resource Collector Building bauen
4. Harvester befehligen (Rechtsklick)
5. Zusehen wie Ressourcen eingesammelt werden! ????

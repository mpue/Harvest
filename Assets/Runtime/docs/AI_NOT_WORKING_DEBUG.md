# ?? AI macht nichts - Quick Debug Guide

## Problem: AI Controller macht gar nix

### ? Sofort-Diagnose (1 Minute):

```
1. Play Mode starten

2. Console beobachten:

Erwartete Logs beim Start:
??????????????????????????????
AI (Enemy): Initializing with difficulty Medium
AI (Enemy): Found headquarters
AI (Enemy): Found X units and Y buildings
? AI (Enemy): Successfully initialized!
  - Headquarters: EnemyHQ
  - Available Products: 5
  - Resource Manager: ResourceManager
  - Initial Gold: 500
??????????????????????????????

Alle Logs sichtbar?
  JA ? AI initialisiert OK, siehe "Warum produziert AI nichts?"
  NEIN ? Siehe Probleme unten
```

## ?? Häufigste Probleme:

### Problem 1: "No headquarters found!"

**Console zeigt:**
```
AI (Enemy): No headquarters found! AI cannot function.
Make sure you have a building with TeamComponent (Team=Enemy) 
and BuildingComponent (IsHeadquarter=true).
```

**Ursache:** AI findet keine Basis

**Lösung:**
```
1. Enemy Headquarter Building platzieren:
   - GameObject mit Building Model

2. Components hinzufügen/prüfen:
   ? BuildingComponent
     - Building Product: [Headquarters Product]
     - Is Headquarter: ? (automatisch von Product)
   
   ? TeamComponent
  - Current Team: Enemy  ? WICHTIG!
   
   ? ProductionComponent
     - Available Products: [Liste mit Units & Buildings]

3. Play Mode testen
   ? Console sollte jetzt "Found headquarters" zeigen
```

### Problem 2: "No ProductionComponent!"

**Console zeigt:**
```
AI (Enemy): Headquarters has no ProductionComponent!
```

**Ursache:** Headquarters kann nichts produzieren

**Lösung:**
```
Enemy Headquarter GameObject:
  Add Component > ProductionComponent

Inspector konfigurieren:
  Available Products: [Add]
    - Harvester (Unit)
    - Soldier (Unit)
    - Energy Block (Building)
    - Resource Collector (Building)
    - Defense Tower (Building)

Resource Manager: [ResourceManager GameObject]
Building Placement: [BuildingPlacement GameObject]

? Mindestens 3-5 Products hinzufügen!
```

### Problem 3: "No products available!"

**Console zeigt:**
```
AI (Enemy): No products available in ProductionComponent!
```

**Ursache:** ProductionComponent ist leer

**Lösung:**
```
1. Products erstellen (falls noch nicht vorhanden):
   Assets > Create > RTS > Production > Product
   
   Harvester Product:
     Is Building: ?
     Prefab: [Harvester Prefab]
     Gold Cost: 100
     
   Energy Block Product:
     Is Building: ?
     Prefab: [Energy Block Prefab]
     Gold Cost: 100
     Energy Production: 10

2. Zu ProductionComponent hinzufügen:
   Headquarters > ProductionComponent
   Available Products: [Drag all Products hier rein]

3. Play Mode testen
```

### Problem 4: "Cannot find product containing 'Harvester'"

**Console zeigt:**
```
AI (Enemy): Cannot find product containing 'Harvester'.
Available products: Miner, Soldier, Tank
```

**Ursache:** AI sucht nach "Harvester" aber Product heißt anders

**Lösung A: Product umbenennen**
```
Product Asset:
  Product Name: "Harvester" ? Exakt so!
  
Oder mit "Harvester" im Namen:
  "Gold Harvester" ?
  "Harvester Unit" ?
  "Miner" ? (AI findet das nicht)
```

**Lösung B: AI-Code anpassen**
```csharp
// In AIController.cs, ExecuteEarlyGameStrategy():

// Statt:
ProduceUnit("Harvester");

// Verwenden Sie Ihren Product-Namen:
ProduceUnit("Miner");
ProduceUnit("Gold Harvester");
```

### Problem 5: "No ResourceManager found!"

**Console zeigt:**
```
AI (Enemy): No ResourceManager found! AI cannot function.
```

**Ursache:** Kein ResourceManager in Scene

**Lösung:**
```
1. ResourceManager GameObject erstellen:
 Hierarchy > Create Empty
   Name: "ResourceManager"
   
2. Add Component > ResourceManager

3. Inspector konfigurieren:
   Starting Resources:
     Gold: 500
     Food: 500
  Wood: 500
     Stone: 500
   
   Starting Energy: 20

4. Wichtig für AI:
   ? Jedes Team braucht eigenen ResourceManager!
   ? Oder: Shared ResourceManager (unrealistisch)
```

### Problem 6: "Cannot afford X - Need Y gold, have Z"

**Console zeigt:**
```
AI (Enemy): Cannot afford Harvester - Need 100 gold, have 50
```

**Ursache:** AI hat nicht genug Gold

**Lösung:**
```
ResourceManager (für AI Team):
  Gold: 500 oder mehr ? Starting Gold erhöhen

Oder:
  AI Controller > Min Gold Reserve: 0 ? Senken

Warum hat AI kein Gold?
  ? Braucht eigenen ResourceManager
  ? Oder: Shared Manager mit genug Starting Gold
```

### Problem 7: AI Controller ist disabled

**Console zeigt:**
```
(Gar nichts)
```

**Ursache:** AIController Component ist ausgeschaltet

**Lösung:**
```
Hierarchy > AI Controller GameObject

Inspector:
  ? GameObject Active
  ? AIController Component Enabled (Checkbox)

Play Mode testen
```

## ?? Vollständige Checklist:

```
Setup Checklist:
? Enemy Headquarter in Scene
  ? BuildingComponent ?
  ? TeamComponent > Team: Enemy ?
  ? ProductionComponent ?
  ? Available Products: Mind. 3-5 Products

? AI Controller GameObject in Scene
  ? AIController Component ?
  ? AI Team: Enemy
  ? Difficulty: Medium
  ? Headquarters: [Enemy Headquarter zugewiesen]
  ? Resource Manager: [Zugewiesen]

? ResourceManager in Scene
  ? Starting Gold: >= 500
  ? (Für AI Team oder shared)

? Products erstellt
  ? Harvester (Unit, ~100 Gold)
  ? Soldier (Unit, ~150 Gold)
  ? Energy Block (Building, ~100 Gold)
  ? Resource Collector (Building, ~200 Gold)
  ? Defense Tower (Building, ~200 Gold)

? Product Namen passen
  ? "Harvester" oder enthält "Harvester"
  ? "Energy Block" oder enthält "EnergyBlock"
  ? etc.

? Play Mode Test
  ? Console: "? AI (Enemy): Successfully initialized!"
  ? Console: Periodische Status-Logs alle ~10s
  ? AI produziert Units (nach ein paar Sekunden)
```

## ?? Test-Workflow:

```
1. Play Mode starten

2. Console beobachten (erste 5 Sekunden):
 "AI (Enemy): Initializing..."
   "AI (Enemy): Found headquarters"
   "? AI (Enemy): Successfully initialized!"
   
   Alles OK? ? Weiter zu Schritt 3
   Fehler? ? Siehe Problem oben

3. Warten ~10 Sekunden:
   "AI (Enemy) Status: State=EarlyGame, Gold=500, ..."
   "? AI (Enemy): Producing Harvester (Cost: 100 gold)"
   
   Produziert? ? AI funktioniert! ?
   Nicht? ? Siehe "Warum produziert AI nichts?"

4. Warten weitere 10-20 Sekunden:
   "? AI (Enemy): Producing Harvester" (mehrmals)
   "? AI (Enemy): Building Energy Block"
   
   ? AI baut Wirtschaft auf! ?

5. Nach 1-2 Minuten:
   AI sollte haben:
     - 3-5 Harvesters
     - 1-2 Energy Blocks
     - 1-2 Resource Collectors
     - Evtl. erste Soldiers
```

## ?? Warum produziert AI nichts?

### Szenario A: Initialization OK, aber keine Production

**Console:**
```
? AI (Enemy): Successfully initialized!
  - Available Products: 5
  - Initial Gold: 500

...10 seconds later...

AI (Enemy) Status: State=EarlyGame, Gold=500, Harvesters=0/5
(Keine "Producing..." Logs)
```

**Problem:** AI ruft ProduceUnit() nicht auf

**Ursache & Lösung:**

1. **Update Interval zu hoch?**
```
AI Controller:
  Update Interval: 1.0 ? OK
  
Falls > 5.0:
  ? Zu langsam, auf 1.0 setzen
```

2. **Queue voll?**
```
Console: "Cannot produce X - Queue full (5/5)"

ProductionComponent:
  Max Queue Size: 10 ? Erhöhen
```

3. **Zu wenig Gold?**
```
Console: "Cannot afford X - Need 100, have 50"

ResourceManager:
  Gold: 1000 ? Mehr Starting Gold
```

### Szenario B: Production Logs aber keine Units erscheinen

**Console:**
```
? AI (Enemy): Producing Harvester (Cost: 100 gold)
Completed production of Harvester
(Unit erscheint nicht in Scene)
```

**Problem:** Production klappt, aber Spawn nicht

**Lösung:**
```
Headquarters > ProductionComponent:
  Spawn Point: [Transform zuweisen]
  
Falls fehlt:
  ? Add Child GameObject "SpawnPoint"
  ? Position: Vor HQ
  ? Zuweisen
```

## ?? Advanced Debugging:

### Console Commands (Play Mode):

**Check AI Status:**
```csharp
// In Console Window (wenn möglich):
var ai = FindObjectOfType<AIController>();
Debug.Log($"AI Gold: {ai.ResourceManager.Gold}");
Debug.Log($"AI State: {ai.CurrentState}");
Debug.Log($"HQ Production Queue: {ai.HQProduction.QueueCount}");
```

### Gizmo Debugging:

```
Scene View (während Play Mode):
1. AI Headquarters auswählen

2. Gizmo sichtbar:
   - Rote Wirframe Sphere
   - Label mit Status

Label zeigt:
  AI (Enemy)
  State: EarlyGame
  Strategy: Balanced
  Harvesters: 0/5 ? Sollte hochgehen!
  Army: 0/15
  Gold: 500 ? Sollte runtergehen wenn produziert!

? Live AI-Status sehen!
```

### Manual Testing:

**Force AI Production:**
```csharp
// Temporär in AIController.cs, Update():
if (Input.GetKeyDown(KeyCode.F1))
{
    ProduceUnit("Harvester");
    Debug.Log("Manual: Forced Harvester production");
}

if (Input.GetKeyDown(KeyCode.F2))
{
    BuildStructure("EnergyBlock");
    Debug.Log("Manual: Forced Energy Block construction");
}

// Play Mode ? F1/F2 drücken ? Produziert AI?
```

## ?? Expected vs Actual:

### Expected (AI funktioniert):
```
[0s] AI (Enemy): Initializing...
[0s] AI (Enemy): Found headquarters
[0s] ? AI (Enemy): Successfully initialized!
[0s]   - Available Products: 5
[0s]   - Initial Gold: 500

[2s] ? AI (Enemy): Producing Harvester (Cost: 100 gold)
[5s] ? AI (Enemy): Producing Harvester (Cost: 100 gold)
[8s] ? AI (Enemy): Building Energy Block (Cost: 100 gold)
[10s] AI (Enemy) Status: Gold=200, Harvesters=2/5, Buildings=2

[15s] Completed production of Harvester
[18s] Harvester: Moving to harvest Gold ? AI Harvester arbeitet!
[20s] ? AI (Enemy): Producing Harvester (Cost: 100 gold)
```

### Actual (AI macht nix):
```
[0s] AI (Enemy): Initializing...
[0s] AI (Enemy): No headquarters found! ? PROBLEM!

Oder:

[0s] ? AI (Enemy): Successfully initialized!
[10s] AI (Enemy) Status: Gold=500, Harvesters=0/5 ? Produziert nicht!
[20s] AI (Enemy) Status: Gold=500, Harvesters=0/5 ? Immer noch nicht!

? Siehe Probleme oben!
```

## ?? Quick Fix Zusammenfassung:

```
Problem        | Quick Fix
-------------------------------|----------------------------------
No headquarters found    | TeamComponent > Team: Enemy
No ProductionComponent        | Add Component > ProductionComponent
No products available     | Add Products zu Available Products
Cannot find product "X"       | Product Name: "X" oder "...X..."
No ResourceManager            | Create ResourceManager GameObject
Cannot afford X    | ResourceManager > Gold: 1000
Queue full          | Max Queue Size: 10
AI disabled                 | AIController Component Enable ?
```

---

**Mit diesen Logs sehen Sie GENAU wo das Problem ist!** ??

**Starten Sie Play Mode und prüfen Sie die Console!**

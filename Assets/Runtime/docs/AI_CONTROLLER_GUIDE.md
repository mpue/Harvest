# ?? AI Controller - Complete Guide

## Übersicht

Ein vollständiges KI-System für Computergegner mit verschiedenen Schwierigkeitsgraden, Strategien und intelligentem Spielverhalten.

### Features:
- ? **3 Schwierigkeitsgrade** (Easy, Medium, Hard, Expert)
- ? **3 Strategien** (Economic, Military, Balanced)
- ? **Automatisches Strategy-Switching**
- ? **Intelligente Ressourcenverwaltung**
- ? **Automatische Army-Bildung**
- ? **Defensive Reaktion bei Angriff**
- ? **3 Game-Phasen** (Early, Mid, Late Game)

## ? Quick Setup (2 Minuten):

### 1. AI Headquarters erstellen:

```
1. Enemy Headquarter Building platzieren

2. Components prüfen:
   - BuildingComponent ?
   - ProductionComponent ?
   - TeamComponent > Team: Enemy ?
   - ResourceManager (zugewiesen) ?

3. Products konfigurieren:
   ProductionComponent > Available Products:
   - Harvester Unit
   - Soldier Unit
   - Energy Block
   - Resource Collector
   - Defense Tower
   (Alle Products die AI bauen können soll)
```

### 2. AI Controller Setup:

```
1. Empty GameObject erstellen: "AI Controller"

2. Add Component > AIController

3. Inspector konfigurieren:
   AI Settings:
     AI Team: Enemy
     Difficulty: Medium
 Update Interval: 1

   Strategy Settings:
     Current Strategy: Balanced
     Auto Switch Strategy: ?
     Strategy Change Interval: 60

   Economic Settings:
     Min Gold Reserve: 100
     Ideal Harvester Count: 5
     Max Harvester Count: 10

   Military Settings:
     Min Army Size: 5
     Ideal Army Size: 15
     Attack Threshold: 0.7

   References:
     Resource Manager: [Drag ResourceManager]
     Headquarters: [Drag AI Headquarters]
```

### 3. Initial Resources für AI:

```
ResourceManager muss für beide Teams funktionieren!

Option A: Separate ResourceManager:
  - Erstellen Sie zweiten ResourceManager für AI
  - Nur für AI-Team

Option B: Shared ResourceManager:
  - Ein ResourceManager für alle
  - AI verwendet denselben wie Player
  (Nicht realistisch für Multiplayer)

Empfohlen: Option A für echtes RTS-Gefühl
```

## ?? Wie die AI spielt:

### Early Game (1-3 Buildings):
```
Prioritäten:
1. Harvesters bauen (bis 5x)
2. Energy Blocks bauen (mind. 10 energy)
3. Resource Collector bauen (1-2x)
4. Defense Tower bauen (1-2x)
5. Harvesters zu Ressourcen schicken

Ziel: Wirtschaft aufbauen & Basisdiffense
```

### Mid Game (4-8 Buildings):
```
Strategie-abhängig:

Economic Strategy:
  - Max Harvesters (10x)
  - Mehr Resource Collectors
  - Energy expansion
  - Minimal defense

Military Strategy:
  - Army aufbauen (15-20 Units)
  - Barracks bauen
  - Aggressive Angriffe
  - Minimal economy

Balanced Strategy:
  - 50% Economy / 50% Military
  - 5 Harvesters
  - 10-15 Soldiers
  - Moderate defense
  - Opportunistische Angriffe
```

### Late Game (9+ Buildings):
```
Fokus: Military Dominance

Aktionen:
- Army maximieren (15+ Units)
- Starke Verteidigung (5+ Towers)
- Aggressive Angriffe wenn Vorteil
- Economy maintenance

Attack Condition:
  Army Size >= Enemy Army Size * 0.7
  ? Launch attack!
```

## ?? AI Decision Making:

### Economic Decisions:
```
Harvester produzieren wenn:
  ? Current < Ideal Count
  ? Gold >= 100
  ? Queue not full

Resource Collector bauen wenn:
  ? Count < 2 (early game)
  ? Gold >= 200
  ? Economic strategy

Energy Block bauen wenn:
  ? Available Energy < 10 (early)
  ? Available Energy < 20 (mid/late)
  ? Gold >= 100
```

### Military Decisions:
```
Soldier produzieren wenn:
  ? Army Size < Ideal Size
  ? Gold >= 150
  ? Military or Balanced strategy

Defense Tower bauen wenn:
  ? Count < 2 (early)
  ? Count < 5 (late)
  ? Gold >= 200
  ? Under attack

Angriff starten wenn:
  ? Army Size >= Min Army Size
  ? Strength Ratio >= Attack Threshold
  ? Enemy detected
```

### Strategy Switching:
```
Auto-switch every 60 seconds based on:

? Economic wenn:
  - Harvesters < Ideal / 2
  - Need to rebuild economy

? Military wenn:
  - Gold > 1000 (rich ? aggressive)
  - Army Size < Min Army Size (need defense)

? Balanced wenn:
  - Economy OK
  - Army OK
  - Stable situation
```

## ?? Difficulty Levels:

### Easy:
```
Settings:
  Update Interval: 2.0s (slow reactions)
  Ideal Harvester Count: 3
  Ideal Army Size: 8
  Attack Threshold: 1.0 (only attack when much stronger)
  Min Gold Reserve: 200 (conservative)

Behavior:
- Langsame Entscheidungen
- Kleine Armee
- Defensive playstyle
- Selten angreifen
```

### Medium:
```
Settings:
  Update Interval: 1.0s
  Ideal Harvester Count: 5
  Ideal Army Size: 15
  Attack Threshold: 0.7
  Min Gold Reserve: 100

Behavior:
- Ausgewogene Entscheidungen
- Moderate Armee
- Balanced playstyle
  - Angriffe bei Vorteil
```

### Hard:
```
Settings:
  Update Interval: 0.5s (fast reactions)
  Ideal Harvester Count: 8
  Ideal Army Size: 20
  Attack Threshold: 0.5 (aggressive)
  Min Gold Reserve: 50 (risky)

Behavior:
- Schnelle Entscheidungen
- Große Armee
- Aggressive playstyle
- Häufige Angriffe
```

### Expert:
```
Settings:
  Update Interval: 0.25s (very fast)
  Ideal Harvester Count: 10
Ideal Army Size: 25
  Attack Threshold: 0.4 (very aggressive)
  Min Gold Reserve: 25 (very risky)

Behavior:
- Instant reactions
- Maximale Armee
- Very aggressive
- Constant pressure
```

## ?? AI Building Placer:

### Automatic Placement:

```
Defense Tower:
  - Platziert am Perimeter (7-20m von HQ)
  - Random angle distribution
  - Schützt Basis

Resource Collector:
  - Platziert nah bei Ressourcen (5m)
  - Findet nächste Collectable
  - Optimierte Harvester-Wege

Energy Block:
  - Platziert nah bei HQ (5-7m)
  - Zentral gruppiert
  - Easy zu verteidigen

General Buildings:
  - Random placement (5-30m)
  - Vermeidet Obstacles
  - Ground-aligned
```

## ?? Debug Visualization:

### Gizmos (in Scene View):
```
AI Headquarters:
  - Rote Wirframe Sphere (5m radius)
  
Label zeigt:
  - AI Team
  - Current State (EarlyGame/MidGame/LateGame)
  - Current Strategy
  - Harvester Count
  - Army Size
  - Current Gold

? Sehr hilfreich zum Debuggen!
```

## ?? Troubleshooting:

### Problem: AI produziert nichts

```
Checklist:
? Headquarters gefunden?
  Console: "AI (Enemy): Found headquarters"

? ProductionComponent vorhanden?
  Headquarters > ProductionComponent ?

? Available Products konfiguriert?
  ProductionComponent > Available Products: [Nicht leer]

? ResourceManager zugewiesen?
  AI Controller > Resource Manager: [Zugewiesen]

? Genug Gold?
  ResourceManager > Gold: >= 100

Falls alles OK:
  ? Console Logs prüfen
  ? "AI (Enemy): Producing X" sollte erscheinen
```

### Problem: AI baut keine Buildings

```
Mögliche Ursachen:

1. Products haben Is Building = false
   ? Products Inspector: Is Building ?

2. Building Placement fehlt
   ? AI Controller needs BuildingPlacement reference
   ? Oder: Manual placement in AIBuildingPlacer

3. Keine Energy
   ? AI baut zuerst Energy Blocks
   ? Warten Sie ein paar Sekunden

4. Queue voll
   ? ProductionComponent > Max Queue Size: erhöhen
```

### Problem: AI greift nicht an

```
Checklist:
? Army Size >= Min Army Size?
  Debug Label zeigt: Army: X/15
  ? Warten bis genug Units

? Enemy Units vorhanden?
  ? Spieler muss Units haben

? Attack Threshold zu hoch?
  ? 0.7 = AI braucht 70% der Enemy Stärke
  ? Auf 0.5 senken für aggressivere AI

? Military Units haben Weapons?
  ? WeaponController Component prüfen
```

### Problem: AI Harvesters sammeln nicht

```
1. Collectables in Scene?
 ? Mind. 1 Gold Vein platzieren

2. Resource Collector vorhanden?
   ? AI baut automatisch in Early Game

3. Harvesters haben Components?
   ? HarvesterUnit ?
   ? Controllable ?
   ? TeamComponent (Team: Enemy) ?

4. Console Logs prüfen:
   ? "AI (Enemy): Producing Harvester"
 ? "Harvester: Moving to harvest Gold"
```

## ?? Advanced Configuration:

### Custom Strategies:

```csharp
// In AIController.cs anpassen:

private void ExecuteCustomStrategy()
{
    // Your custom logic
    if (resourceManager.Gold > 500)
    {
  // Build special unit
ProduceUnit("HeavyTank");
    }

    // Custom attack patterns
    if (Time.time % 120 < 1) // Every 2 minutes
    {
 LaunchAttack();
    }
}
```

### Dynamic Difficulty:

```csharp
void Update()
{
    // Adjust difficulty based on player performance
    if (PlayerIsWinning())
    {
   difficulty = AIDifficulty.Hard;
        updateInterval = 0.5f;
 }
 else
    {
        difficulty = AIDifficulty.Medium;
        updateInterval = 1f;
    }
}
```

### Multiple AI Players:

```
Scene Setup:
1. AI Controller 1: Team.Enemy
2. AI Controller 2: Team.Ally
3. AI Controller 3: Team.Neutral

Each with own:
  - ResourceManager
  - Headquarters
  - Initial Units

? 3-player free-for-all!
```

## ?? Performance:

### Optimization:

```
Update Interval:
  Easy: 2.0s ? Very low CPU
  Medium: 1.0s ? Low CPU
  Hard: 0.5s ? Medium CPU
  Expert: 0.25s ? High CPU

Für große Battles:
  ? Update Interval erhöhen
  ? Strategy Change Interval erhöhen
  ? Weniger Units spawnen
```

### Best Practices:

```
? Ein AIController pro AI Player
? Update Interval >= 0.5s (unless Expert)
? Max 50 Units pro AI
? Caching von FindObjectOfType calls
? Deactivate AI wenn Player weit weg (open world)
```

## ?? Zusammenfassung:

**Component:** AIController + AIBuildingPlacer
**Difficulty Levels:** 4 (Easy to Expert)
**Strategies:** 3 (Economic, Military, Balanced)
**Game Phases:** 3 (Early, Mid, Late)

**Features:**
- ? Automatische Produktion
- ? Intelligente Strategie-Wahl
- ? Adaptive Reaktion auf Situationen
- ? Automatische Angriffe
- ? Defensive Behavior
- ? Resource Management
- ? Building Placement

**Setup-Zeit:** 2 Minuten
**Complexity:** High (aber plug-and-play)
**Replayability:** Sehr hoch (durch verschiedene Strategien)

---

**Quick Start:**
1. AI Headquarters platzieren (Team: Enemy)
2. AIController GameObject erstellen
3. Settings konfigurieren
4. Play Mode ? AI spielt automatisch!

**Die AI wird Ihnen ein gutes Match geben!** ????

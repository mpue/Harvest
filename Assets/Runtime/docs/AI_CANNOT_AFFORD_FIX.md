# ?? AI "Cannot afford" Problem - LÖSUNG

## Problem:
```
Console Log:
"Cannot afford EnergyBlock"

AI Status:
- Initial Gold: 500 ?
- ResourceManager: ResourceManagerAI ?
- Headquarters: Found ?
- Available Products: 6 ?

? Aber AI kann nichts bauen! ?
```

## Ursache:

### Das EnergyBlock Product hat wahrscheinlich andere Costs gesetzt!

```
Mögliche Product Configuration:
EnergyBlock Product (ScriptableObject):
  Food Cost: 100    ? AI hat kein Food!
  Wood Cost: 50     ? AI hat kein Wood!
  Stone Cost: 0
  Gold Cost: 100    ? AI hat 500 Gold
  
ResourceManager AI:
  Food: 0 (default)   ? PROBLEM!
  Wood: 0 (default)   ? PROBLEM!
  Stone: 0 (default)
  Gold: 500 ?

CanAfford Check:
  food >= foodCost    ? 0 >= 100 = FALSE ?
  wood >= woodCost    ? 0 >= 50 = FALSE ?
  stone >= stoneCost  ? 0 >= 0 = TRUE ?
  gold >= goldCost    ? 500 >= 100 = TRUE ?
  
Result: Cannot afford! ?
```

## ? LÖSUNG 1: ResourceManager AI richtig konfigurieren

### Unity Editor:
```
1. Scene öffnen
2. "ResourceManagerAI" GameObject finden
3. ResourceManager Component Inspector:

   Resources:
   Food: 500    ? Von 0 auf 500!
     Wood: 500    ? Von 0 auf 500!
     Stone: 500   ? Von 0 auf 500!
     Gold: 500    ? Schon richtig

   Energy System:
     Starting Energy: 20 ?

4. Save Scene
5. Play Mode testen

? AI kann jetzt bauen! ?
```

## ? LÖSUNG 2: Products nur Gold kosten lassen

### Wenn AI nur Gold verwenden soll:

```
Alle Product Assets prüfen:
1. Assets > Products > EnergyBlock
2. Inspector:
   Costs:
     Food Cost: 0    ? Auf 0 setzen!
     Wood Cost: 0    ? Auf 0 setzen!
     Stone Cost: 0   ? Auf 0 setzen!
     Gold Cost: 100  ? Behalten

3. Für ALLE Products wiederholen:
 - Factory
   - Barracks
   - DefenceTower
   - ResourceCollector
   - WallBig
   - etc.

? AI braucht nur Gold! ?
```

## ?? Debug Check:

### Mit neuen Debug Logs wird Console zeigen:

**VORHER (broken):**
```
Checking resources for EnergyBlock: 
  Need Gold=100, Have Gold=500, 
  Food=0, Wood=0, Stone=0

Cannot afford EnergyBlock: 
  Need (Food:100, Wood:50, Stone:0, Gold:100) 
  but have (Food:0, Wood:0, Stone:0, Gold:500)
  
? Food & Wood fehlen! ?
```

**NACHHER (fixed):**
```
Checking resources for EnergyBlock: 
  Need Gold=100, Have Gold=500, 
  Food=500, Wood=500, Stone=500

Added EnergyBlock to production queue. Queue size: 1

? Funktioniert! ?
```

## ?? Empfohlene Setup:

### Option A: Vollständige Resources (realistisch)
```
ResourceManager AI:
  Food: 500
  Wood: 500
  Stone: 500
  Gold: 500
  Starting Energy: 20

Products können alle Resources kosten:
  EnergyBlock: 50 Wood, 100 Gold
  Factory: 100 Wood, 250 Gold
  Barracks: 75 Wood, 200 Gold
  etc.

? Realistisches RTS Gameplay
```

### Option B: Gold-Only (vereinfacht)
```
ResourceManager AI:
  Food: 0 (unused)
  Wood: 0 (unused)
  Stone: 0 (unused)
  Gold: 1000
  Starting Energy: 20

Products kosten nur Gold:
  EnergyBlock: 0 Food, 0 Wood, 0 Stone, 100 Gold
  Factory: 0 Food, 0 Wood, 0 Stone, 250 Gold
  etc.

? Einfacheres System, fokussiert auf Gold
```

### Option C: Per-Team ResourceManager
```
ResourceManager Player:
  Food: 500
  Wood: 500
  Stone: 500
  Gold: 500

ResourceManager AI:
  Food: 500
  Wood: 500
  Stone: 500
  Gold: 500

? Beide Teams unabhängig
? Realistisch für Multiplayer
```

## ?? Quick Fix (Sofort):

### 1. ResourceManager AI fixing:
```bash
Unity Editor:
1. Hierarchy > "ResourceManagerAI"
2. Inspector > Resource Manager:
   - Food: 500
   - Wood: 500
   - Stone: 500
   - Gold: 500
3. Apply
4. Play

? Done! ?
```

### 2. Alternative: Products anpassen:
```bash
Project Window:
1. Assets > Products
2. Für jedes Product:
   - Food Cost: 0
   - Wood Cost: 0
   - Stone Cost: 0
   - Gold Cost: (keep existing)
3. Save All
4. Play

? Done! ?
```

## ?? Testing:

### Nach Fix, Console sollte zeigen:
```
[0s] ? AI (Enemy): Successfully initialized!
  - Initial Gold: 500

[2s] Checking resources for EnergyBlock:
  Need Gold=100, Have Gold=500, 
 Food=500, Wood=500, Stone=500 ?

[2s] Added EnergyBlock to production queue. Queue size: 1

[2s] ? AI (Enemy): Building EnergyBlock (Cost: 100 gold)

[7s] Completed production of EnergyBlock

[7s] AI Building EnergyBlock - placing automatically

[7s] ? AI successfully placed EnergyBlock

? AI baut! ???
```

## ?? Warum es wichtig ist:

### CanAfford ist eine AND-Prüfung:
```csharp
public bool CanAfford(int foodCost, int woodCost, int stoneCost, int goldCost)
{
    return food >= foodCost &&      // ALL must be true!
           wood >= woodCost &&      // If ANY is false ? cannot afford
      stone >= stoneCost &&
           gold >= goldCost;
}
```

### Wenn Product hat:
```
Food Cost: 100
Wood Cost: 50
Stone Cost: 0
Gold Cost: 100
```

### Und ResourceManager hat:
```
Food: 0
Wood: 0
Stone: 500
Gold: 500
```

### Result:
```
0 >= 100 = FALSE  ? FAILS HERE!
(Rest wird nicht gecheckt)

? Cannot afford ?
```

## ? Nach dem Fix:

**ResourceManager AI:**
```
Food: 500 ?
Wood: 500 ?
Stone: 500 ?
Gold: 500 ?
```

**CanAfford Check:**
```
500 >= 100 (Food) = TRUE ?
500 >= 50 (Wood) = TRUE ?
500 >= 0 (Stone) = TRUE ?
500 >= 100 (Gold) = TRUE ?

? Can afford! ?
```

**AI baut:**
```
? EnergyBlock
? Factory
? Barracks
? Everything!
```

---

**Das Problem ist Resource Initialization!**

**Fix ResourceManager AI ? AI funktioniert!** ??

**Oder: Products auf Gold-Only umstellen!** ??

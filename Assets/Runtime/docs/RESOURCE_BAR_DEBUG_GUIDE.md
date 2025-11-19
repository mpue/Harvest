# ?? Resource Bar Not Updating - Debug Guide

## Problem: Resource Bar zeigt keine Änderungen

Wenn Gold oder Energie sich nicht in der Resource Bar ändern, obwohl Buildings gebaut werden:

## ? Quick Debug (30 Sekunden):

### Schritt 1: Debug Helper aktivieren

```
1. Beliebiges GameObject auswählen (z.B. ResourceManager)

2. Add Component > ResourceManagerDebugger

3. Inspector:
   Resource Manager: [Auto-found oder manuell zuweisen]
   Log Resource Changes: ?
   Log Energy Changes: ?
   Show On Screen Debug: ?

4. Play Mode starten
```

### Schritt 2: Was Sie sehen sollten

```
Unten links im Game View:
?????????????????????????????????????
? === Resource Manager Debug ===    ?
? ?
? Current Values:     ?
?   Gold: 500     ?
?   Food: 500              ?
?   Wood: 500     ?
?   Stone: 500   ?
?   Energy: 0/20   ?
?   Available Energy: 20         ?
?     ?
? Event Status:          ?
?   OnResourcesChanged: ? Subscribed?
?   OnEnergyChanged: ? Subscribed   ?
?    ?
? [Test: +100 Gold] [Test: +10 Energy] [Test: Consume 5] ?
?????????????????????????????????????
```

### Schritt 3: Test Buttons verwenden

```
1. Click "Test: +100 Gold"
   ? Gold sollte auf 600 hochgehen
   ? Console: "?? Resources Changed: Gold=600"
   ? Resource Bar sollte updaten!

2. Click "Test: +10 Energy"
   ? Max Energy: 20 ? 30
   ? Console: "? Energy Changed: Max=30"
   ? Resource Bar sollte updaten!

3. Funktioniert Test ? Problem ist woanders
   Funktioniert Test NICHT ? Events Problem
```

## ?? Diagnose-Szenarien:

### Szenario A: Test Buttons funktionieren, aber echte Changes nicht

**Problem:** Events feuern, aber BuildingPlacement/Production sendet keine Updates

**Prüfen:**
```
1. Building bauen

2. Console prüfen:
   ? "Added [Building] to production queue"
   ? "Completed production of [Building]"
   ? "Building [Name] ready for placement"
   ? "Placed [Building] at..."
   
   ABER:
   ? KEIN "?? Resources Changed"
   ? KEIN "? Energy Changed"
```

**Lösung:** ResourceManager wird nicht richtig aufgerufen

```csharp
// In BuildingPlacement.PlaceBuilding():
// FEHLT vermutlich:
buildingComp.Initialize(currentProduct, resourceManager);

// Oder BuildingComponent.Start() wird nicht richtig aufgerufen
```

### Szenario B: Test Buttons funktionieren NICHT

**Problem:** Events werden nicht gefeuert oder nicht subscribed

**Prüfen:**
```
Debug UI zeigt:
  OnResourcesChanged: ? No Listeners  ? PROBLEM!
  OnEnergyChanged: ? No Listeners     ? PROBLEM!
```

**Lösung:** ResourceBarUI startet nicht oder findet ResourceManager nicht

```
1. ResourceBar GameObject auswählen

2. ResourceBarUI Component:
   Resource Manager: [Ist das zugewiesen?]
   
Falls NULL:
   ? Drag ResourceManager GameObject hier rein
   ? Oder: Auto-Find sollte es finden
```

### Szenario C: Nur Gold updated nicht, Energie schon

**Problem:** OnResourcesChanged Event feuert nicht

**Prüfen:**
```
In ResourceManager.cs:

SpendResources():
  ...
  gold -= goldCost;
  NotifyResourcesChanged();? Ist das da?
```

**Lösung:** Fehlender Event-Call

### Szenario D: Nur Energie updated nicht

**Problem:** OnEnergyChanged Event feuert nicht

**Prüfen:**
```
In BuildingComponent.cs Start():

if (IsEnergyProducer && resourceManager != null)
{
    resourceManager.IncreaseMaxEnergy(buildingProduct.EnergyProduction);
    ? Event sollte hier gefeuert werden
}
```

## ?? Häufigste Ursachen:

### 1. ResourceManager nicht zugewiesen

```
ResourceBar > ResourceBarUI:
  Resource Manager: NULL  ? PROBLEM!

Lösung:
  ? Drag ResourceManager GameObject auf das Feld
```

### 2. Events nicht verdrahtet

```csharp
// In ResourceBarUI.Start() fehlt:
resourceManager.OnResourcesChanged += OnResourcesChanged;
resourceManager.OnEnergyChanged += OnEnergyChanged;
```

### 3. ResourceManager.Start() läuft nicht

```
ResourceManager GameObject ist inactive
? Start() wird nie aufgerufen
? Keine initialen Events

Lösung:
  ResourceManager GameObject > Active: ?
```

### 4. Product hat keine Kosten

```
Product Inspector:
  Gold Cost: 0  ? Kein Gold wird abgezogen!
  Energy Cost: 0
  Energy Production: 0  ? Keine Energie-Änderung!

Lösung:
  ? Kosten eintragen!
  Energy Block: Energy Production > 0
  Other Buildings: Gold Cost > 0
```

### 5. Mehrere ResourceManager in Scene

```
Problem: ResourceBarUI findet falschen ResourceManager

Lösung:
  1. Nur EIN ResourceManager in Scene haben
  2. Oder manuell zuweisen welcher gemeint ist
```

## ?? Schritt-für-Schritt Checkliste:

```
? Play Mode starten

? ResourceManagerDebugger aktiv?
  ? Debug UI unten links sichtbar?

? Event Status zeigt "? Subscribed"?
  ? OnResourcesChanged: ?
  ? OnEnergyChanged: ?

? Test Button "Test: +100 Gold" klicken
  ? Console: "TEST: Added 100 Gold"
  ? Console: "?? Resources Changed: Gold=600"
  ? Resource Bar zeigt 600?

? Test Button "Test: +10 Energy" klicken
? Console: "TEST: Increased Max Energy by 10"
  ? Console: "? Energy Changed: Max=30"
  ? Resource Bar zeigt 0/30?

? Falls Tests funktionieren:
  ? Building produzieren
  ? Warten bis fertig
  ? Platzieren
  ? Console prüfen für Events
  
? Falls keine Events in Console:
  ? ProductionComponent verwendet resourceManager?
  ? BuildingComponent initialisiert mit resourceManager?
```

## ?? Debugging-Tipps:

### Tip 1: Console filtern

```
Console Filter: "Resources Changed"
? Sehen Sie nur Resource-Events
? Leichter zu tracken
```

### Tip 2: Breakpoints setzen

```csharp
// In ResourceManager.cs:
private void NotifyResourcesChanged()
{
    OnResourcesChanged?.Invoke(food, wood, stone, gold);  ? BREAKPOINT
}

// Wird das aufgerufen?
// Ist OnResourcesChanged != null?
```

### Tip 3: Manual Event Test

```csharp
// In Console während Play Mode:
ResourceManager rm = GameObject.Find("ResourceManager").GetComponent<ResourceManager>();
rm.AddResources(0, 0, 0, 100);

? Sollte sofort Event feuern
? Resource Bar sollte updaten
```

### Tip 4: Component Reihenfolge

```
Manchmal:
  ResourceBarUI.Start() läuft VOR ResourceManager.Start()
  ? Events werden subscribed bevor Manager bereit ist

Lösung:
  In ResourceManager: Start() ? Awake()
  oder
  In ResourceBarUI: Awake() ? Start()
```

## ?? Quick Fixes:

### Fix 1: ResourceManager manuell zuweisen

```
ResourceBar > ResourceBarUI:
  Resource Manager: [Drag ResourceManager GameObject]
```

### Fix 2: ResourceBarUI neu erstellen

```
1. ResourceBar GameObject löschen
2. Tools > RTS > Create Resource Bar UI
3. Neu erstellen
4. Auto-wired & funktioniert
```

### Fix 3: Events manuell re-subscribe

```csharp
// ResourceBarUI erweitern:
[ContextMenu("Re-Subscribe Events")]
void ReSubscribeEvents()
{
    if (resourceManager != null)
 {
        resourceManager.OnResourcesChanged -= OnResourcesChanged;
        resourceManager.OnEnergyChanged -= OnEnergyChanged;
 resourceManager.OnResourcesChanged += OnResourcesChanged;
        resourceManager.OnEnergyChanged += OnEnergyChanged;
    }
}
```

### Fix 4: Force Update

```csharp
// In ResourceBarUI:
[ContextMenu("Force Update Display")]
void ForceUpdate()
{
    if (resourceManager != null)
    {
        currentGold = resourceManager.Gold;
        targetGold = currentGold;
 currentEnergy = resourceManager.CurrentEnergy;
        targetEnergy = currentEnergy;
        currentMaxEnergy = resourceManager.MaxEnergy;
    targetMaxEnergy = currentMaxEnergy;
        UpdateDisplay();
    }
}
```

## ?? Test-Szenario:

### Kompletter Test-Flow:

```
1. Play Mode
2. Debug UI unten links prüfen
3. Current Gold: 500
4. Test Button "+100 Gold" click
5. Debug UI zeigt jetzt: 600  ?
6. Resource Bar oben zeigt: 600  ?
7. Console zeigt: "?? Resources Changed: Gold=600"  ?

Falls NICHT:
  ? ResourceBarUI ist nicht korrekt subscribed
  ? Manually assign ResourceManager
```

### Building Test:

```
1. Play Mode
2. Headquarter auswählen
3. EnergyBlock produzieren (z.B. kostet 100 Gold)
4. Production startet
   Console: "Added EnergyBlock to production queue"
   Debug UI: Gold 500 ? 400  ?
   Resource Bar: 500 ? 400  ?
   
5. Warten bis fertig
   Console: "Building EnergyBlock ready for placement"
   
6. Building platzieren
   Console: "Placed EnergyBlock at..."
   Console: "EnergyBlock providing 10 energy"
   Debug UI: Energy 0/20 ? 0/30  ?
   Resource Bar: 0/30  ?
   
Falls Schritte fehlen:
  ? Identifizieren Sie welcher Schritt fehlt
  ? Das ist wo das Problem ist
```

## ?? Support-Checkliste:

Wenn Sie Hilfe brauchen, geben Sie an:

```
? Welcher Test funktioniert?
  • Test Buttons: Ja/Nein
  • Building Production: Ja/Nein
  • Building Placement: Ja/Nein

? Console Ausgaben:
  • Copy relevante Logs

? Debug UI Status:
  • OnResourcesChanged: Subscribed?
  • OnEnergyChanged: Subscribed?

? Product Configuration:
  • Gold Cost: [Wert]
  • Energy Production: [Wert]

? ResourceManager Werte:
  • Gold: [Vor/Nach]
  • Energy: [Vor/Nach]
```

---

**Quick Start:**
1. Add Component > ResourceManagerDebugger
2. Play Mode
3. Test Buttons verwenden
4. Siehe sofort ob Events funktionieren!

# KI-Angriffs-Problem - Erweiterte Lösung & Diagnose

## Problem
Der KI-Gegner greift nie an, obwohl der `AIAttackController` vorhanden ist.

## WICHTIG: Schritt-für-Schritt Diagnose

### Schritt 1: Diagnose-Tool verwenden

1. **Füge AIAttackDiagnostics hinzu:**
   - Erstelle ein leeres GameObject in der Szene (z.B. "AI_Diagnostics")
   - Füge die `AIAttackDiagnostics` Komponente hinzu
   - Im Inspector: "Run Diagnostics On Start" aktivieren

2. **Starte das Spiel und beobachte die Console**
   
   Die Diagnose wird automatisch alle 10 Sekunden laufen und folgendes prüfen:
 - ? Ist AIAttackController vorhanden und aktiv?
   - ? Gibt es AI-Militäreinheiten (Soldiers/Tanks)?
   - ? Gibt es Spieler-Ziele (Buildings/Units)?
   - ? Sind WeaponController richtig konfiguriert?
   - ? Sind Teams korrekt zugewiesen?

3. **Manuelle Diagnose-Befehle:**
   - Im Inspector von AIAttackDiagnostics:
   - Rechtsklick ? "Run Full Diagnostics" (jederzeit manuell ausführen)
   - Rechtsklick ? "Force AI Attack Now" (sofortiger Angriff)
   - Rechtsklick ? "List All Units With Weapons"

### Schritt 2: Console-Logs interpretieren

#### ? Erfolgreiche Initialisierung sieht so aus:
```
? SoldierUnit: WeaponController initialized (Team: Enemy, AutoAcquire: True, AutoFire: True)
? Weapon 'MainWeapon' on SoldierUnit initialized successfully (Range: 30, Damage: 25, FireRate: 1)
? Found AIAttackController on: AI_Manager
? Found 5 AI military units
```

#### ? Fehlermeldungen und Lösungen:

**"? NO AIAttackController found in scene!"**
- **Problem:** Kein AIAttackController vorhanden
- **Lösung:** Erstelle ein GameObject und füge `AIAttackController` Komponente hinzu

**"? {Unit}: WeaponController has NO TeamComponent!"**
- **Problem:** Unit hat keine Team-Zuordnung
- **Lösung:** Füge `TeamComponent` zur Unit hinzu und setze Team auf "Enemy"

**"? Weapon '{name}' has NO shot points assigned!"**
- **Problem:** Waffe hat keine Schussposition
- **Lösung:** Erstelle Child-GameObject "ShotPoint" und weise es dem Weapon zu

**"? Weapon '{name}' has NO projectile prefab assigned!"**
- **Problem:** Kein Projektil zugewiesen
- **Lösung:** Erstelle/weise ein Projectile Prefab zu

**"?? NO AI military units found!"**
- **Problem:** AI hat keine Kampfeinheiten produziert
- **Lösung:** Warte länger oder prüfe AI-Produktion (siehe unten)

### Schritt 3: Erwartete Console-Logs während des Spiels

**Bei Angriffsbefehl:**
```
? AIAttackController: Launching attack with 5 units to (x,y,z)!
? SoldierUnit weapon auto-targeting enabled for attack
```

**Bei Zielerkennung:**
```
?? SoldierUnit acquired target: PlayerBuilding (Building, Team: Player)
```

**Bei Schussabgabe:**
```
?? MainWeapon FIRED at PlayerBuilding! (Distance: 25.3m)
```

**Bei Problemen:**
```
?? SoldierUnit: No valid targets found in range 30m. Found 5 colliders.
?? MainWeapon: Target PlayerBuilding out of range (45.2m > 30m)
```

## Ursachen (Erweitert)

### 1. WeaponController konnte keine Gebäude angreifen ? BEHOBEN
Die `IsValidTarget()` Methode akzeptiert jetzt auch `BuildingComponent`.

### 2. Waffen waren nicht aktiviert ? BEHOBEN
`AIAttackController.RallyAndAttack()` aktiviert jetzt explizit die Waffensysteme.

### 3. ownerTeam war NULL ? BEHOBEN
`WeaponController` prüft jetzt ob ownerTeam vorhanden ist und gibt Warnung aus.

### 4. Ziel-Ausrichtung zu streng ? BEHOBEN
Waffen erlauben jetzt Schüsse bis 15° Abweichung (vorher 5°).

### 5. Fehlende Debug-Informationen ? BEHOBEN
Alle kritischen Komponenten geben jetzt aussagekräftige Logs aus.

## Häufige Probleme und Lösungen

### Problem: "No valid targets found"

**Diagnose:**
```
?? SoldierUnit: No valid targets found in range 30m. Found 5 colliders.
```

**Ursachen:**
1. **Layer Mask falsch:** WeaponController > Target Layer Mask umfasst nicht den Layer der Spieler-Einheiten
2. **Teams nicht konfiguriert:** Spieler-Einheiten haben kein TeamComponent
3. **Keine TeamComponent auf Zielen:** Gebäude/Units haben keine TeamComponent

**Lösung:**
1. Prüfe WeaponController Inspector ? Target Layer Mask ? sollte "Everything" sein
2. Prüfe Spieler-Einheiten ? müssen TeamComponent haben mit Team = "Player"
3. Führe "Run Full Diagnostics" aus um Team-Verteilung zu sehen

### Problem: "Target out of range"

**Diagnose:**
```
?? MainWeapon: Target PlayerBuilding out of range (45.2m > 30m)
```

**Lösung:**
- AI-Einheiten sind zu weit vom Ziel entfernt
- Erhöhe Weapon > Range im Inspector (z.B. auf 40-50m)
- Oder warte bis Einheiten näher kommen

### Problem: Einheiten bewegen sich nicht zum Ziel

**Diagnose:**
- AIAttackController sendet Befehl aber Einheiten bleiben stehen

**Ursachen:**
1. **Kein NavMeshAgent:** Einheiten brauchen NavMeshAgent für Bewegung
2. **NavMesh nicht gebacken:** Szene hat kein NavMesh
3. **Controllable Komponente fehlt**

**Lösung:**
1. Füge NavMeshAgent zu Einheiten hinzu
2. Window > AI > Navigation > Bake NavMesh
3. Stelle sicher Einheiten haben Controllable Komponente

### Problem: Waffen schießen nicht obwohl Ziel vorhanden

**Diagnose:**
```
?? SoldierUnit acquired target: PlayerBuilding (Building, Team: Player)
(aber kein "FIRED" Log)
```

**Ursachen:**
1. **Kein Projectile Prefab:** Weapon > Projectile Prefab nicht zugewiesen
2. **Keine Shot Points:** Weapon > Shot Points Array leer
3. **Fire Rate zu niedrig:** Weapon wartet noch auf Cooldown

**Lösung:**
1. Weise Projectile Prefab im Weapon Inspector zu
2. Erstelle ShotPoint GameObject als Child und weise zu Shot Points zu
3. Erhöhe Weapon > Fire Rate (z.B. 1 = 1 Schuss/Sekunde)

### Problem: AI produziert keine Militäreinheiten

**Diagnose:**
```
?? NO AI military units found!
```

**Ursachen:**
1. AI hat nicht genug Ressourcen
2. Barracks/Factory nicht gebaut
3. AI-Strategie fokussiert auf Wirtschaft

**Lösung:**
1. Gib AI mehr Start-Gold im ResourceManager
2. Prüfe ob AI Barracks/Factory gebaut hat
3. Prüfe AIControllerModular > Initial Strategy = "Balanced" oder "Military"
4. Warte länger (erste Einheiten nach ~2-3 Minuten)

## Checkliste: Militäreinheit korrekt einrichten

Für jede militärische Einheit (Soldier, Tank):

### GameObject Hierarchie:
```
SoldierUnit
??? Model (Visual)
??? Turret (optional, für Zielen)
?   ??? Barrel (optional)
?       ??? ShotPoint (WICHTIG!)
??? Components:
```

### Komponenten (Inspector):
- [x] **BaseUnit**
  - Unit Name: "Soldier" oder "MK3 Tank"

- [x] **TeamComponent**
  - Current Team: "Enemy" (für AI-Einheiten)
  
- [x] **Controllable**
  - Use Nav Mesh: ?
  
- [x] **NavMeshAgent**
  - Speed: 5-10
  - Stopping Distance: 2-5
  
- [x] **WeaponController**
  - Auto Acquire Targets: ?
  - Auto Fire: ?
  - Target Layer Mask: "Everything" oder spezifische Layer
  - Weapons: [ Weapon Komponente ] (min. 1)
  
- [x] **Weapon** (kann am selben GameObject oder Child sein)
  - Damage: 25
  - Fire Rate: 1
  - Range: 30-50
  - Projectile Speed: 30-40
  - Projectile Prefab: [Zugewiesen!]
  - Shot Points: [ ShotPoint Transform ] (min. 1)
  - Turret Transform: (optional) [Zuweisen wenn vorhanden]

### Projectile Prefab Einrichtung:
```
ProjectilePrefab
??? Mesh/Visual (Sphere oder Custom)
??? Components:
    - Rigidbody (Use Gravity: OFF, Is Kinematic: OFF)
    - SphereCollider (Is Trigger: ?)
    - Projectile Script
    - TrailRenderer (optional, für Effekt)
```

## AIAttackController Einstellungen (Empfohlen)

```
Attack Interval: 60s
First Attack Delay: 90s
Min Attack Force: 2
Max Attack Force: 6
Use Adaptive Attacks: ?
Adaptive Attack Threshold: 4
Include Tanks: ?
Include Soldiers: ?
Target Buildings: ?
Target Units: ?
Prefer Headquarters: ?
AI Team: Enemy
Target Team: Player
```

## Test-Szenario

1. **Setup:**
   - AI mit AIControllerModular + AIAttackController
   - AI Headquarter mit genug Start-Gold (1000+)
   - Spieler-Gebäude vorhanden
   - AIAttackDiagnostics GameObject hinzugefügt

2. **Start Spiel:**
   - Beobachte Console für Initialisierungs-Logs
   - Alle WeaponController und Weapons sollten ? zeigen
   - AIAttackController sollte gefunden werden

3. **Nach 2-3 Minuten:**
   - AI sollte Factory/Barracks gebaut haben
   - AI sollte 2-3 Militäreinheiten produziert haben
   - Diagnose sollte "Found X AI military units" zeigen

4. **Nach 90 Sekunden (First Attack):**
   - Console: "? AIAttackController: Launching attack with X units"
   - Console: "? {Unit} weapon auto-targeting enabled"

5. **Wenn Einheiten in Reichweite:**
   - Console: "?? {Unit} acquired target: {Target}"
   - Console: "?? {Weapon} FIRED at {Target}!"

6. **Wenn es nicht funktioniert:**
   - Rechtsklick auf AIAttackDiagnostics ? "Run Full Diagnostics"
   - Prüfe ALLE ? Fehlermeldungen
   - Rechtsklick ? "Force AI Attack Now" zum sofortigen Test

## Zusätzliche Debug-Befehle

Im Spiel (Play Mode):

```csharp
// Im Unity-Menü oder per Code:
GameObject.FindObjectOfType<AIAttackDiagnostics>().RunFullDiagnostics();
GameObject.FindObjectOfType<AIAttackDiagnostics>().ForceAIAttackNow();
GameObject.FindObjectOfType<AIAttackDiagnostics>().ListAllUnitsWithWeapons();
```

## Erwartete Timeline

- **0:00** - Spiel startet, AI initialisiert
- **0:10** - AI baut EnergyBlocks
- **0:30** - AI baut Factory
- **1:00** - AI produziert erste Harvester
- **1:30** - AI beginnt MK3 Produktion  
- **1:30** - Erste Attacke (First Attack Delay = 90s)
- **2:30** - Zweite Attacke (Attack Interval = 60s)
- **3:30** - Dritte Attacke
- Oder: **Adaptive Attack** wenn 4+ Einheiten verfügbar

## Letzte Schritte wenn immer noch nichts funktioniert

1. **Erstelle ein Minimal-Test-Szenario:**
   - 1 AI-Soldier mit allen Komponenten
   - 1 Player-Building mit TeamComponent
   - AIAttackDiagnostics
   - Positioniere Soldier in Range vom Building

2. **Manuelle Aktivierung:**
   ```csharp
   // Im Soldier Inspector > WeaponController:
   // Setze manuell im Play Mode:
   Auto Acquire Targets = true
   Auto Fire = true
   ```

3. **Prüfe Layer Collision Matrix:**
   - Edit > Project Settings > Physics
   - Stelle sicher dass Layer der Units mit Layer der Targets kollidieren können

4. **Schicke Console-Log:**
   - Kopiere ALLE Console-Logs nach Spielstart
   - Suche nach ? Fehlern
   - Folge den Lösungsvorschlägen

## Quick Fix Checklist

- [ ] AIAttackDiagnostics hinzugefügt und ausgeführt
- [ ] AIAttackController vorhanden und enabled
- [ ] AI hat Militäreinheiten (Diagnose zeigt > 0)
- [ ] Spieler hat Gebäude/Units mit TeamComponent (Team=Player)
- [ ] WeaponController: Auto Acquire = ?, Auto Fire = ?
- [ ] Weapon: Projectile Prefab zugewiesen
- [ ] Weapon: Shot Points zugewiesen (min. 1)
- [ ] Projectile Prefab: Hat Projectile Script
- [ ] Alle Einheiten haben TeamComponent mit korrektem Team
- [ ] Console zeigt "?" für WeaponController und Weapon Init
- [ ] Console zeigt "? Launching attack" nach 90s
- [ ] Console zeigt "?? acquired target"
- [ ] Console zeigt "?? FIRED"

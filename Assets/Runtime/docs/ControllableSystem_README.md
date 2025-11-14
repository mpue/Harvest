# RTS Controllable System - Dokumentation

## Übersicht
Das `Controllable` Script erweitert das Selection System und ermöglicht es, selektierte Einheiten per Rechtsklick zu bewegen.

## Komponenten

### Controllable.cs
Ermöglicht Einheiten, auf Bewegungsbefehle zu reagieren.

**Features:**
- **NavMesh Support**: Automatische Pfadfindung mit Unity NavMesh
- **Manueller Modus**: Direkte Bewegung ohne NavMesh
- **Formationen**: Mehrere Einheiten bewegen sich in Formation
- **Visuelle Feedback**: Optional Bewegungsziel-Indikator
- **Debug Visualisierung**: Pfade und Ziele im Scene View
- **Überschreibbare Events**: OnMoveCommand, OnReachedDestination, OnStop

## Setup

### 1. Controllable Unit erstellen

```
GameObject (z.B. "Soldier")
??? BaseUnit (Script)
??? Controllable (Script)
??? Collider (für Selektion)
??? NavMeshAgent (optional, wird automatisch hinzugefügt)
```

**Schritte:**
1. Fügen Sie `BaseUnit` zum GameObject hinzu
2. Fügen Sie `Controllable` zum GameObject hinzu
3. Fügen Sie einen Collider hinzu (BoxCollider, CapsuleCollider, etc.)
4. Konfigurieren Sie die Settings im Inspector

### 2. Controllable Settings

**Movement Settings:**
- `moveSpeed`: Geschwindigkeit der Einheit (m/s)
- `rotationSpeed`: Rotationsgeschwindigkeit
- `stoppingDistance`: Abstand zum Ziel, bei dem die Einheit stoppt

**Navigation:**
- `useNavMesh`: NavMesh-Agent verwenden (empfohlen für komplexe Level)
  - ? **An**: Verwendet Unity NavMesh für Pathfinding
  - ? **Aus**: Direkte Bewegung in gerader Linie

**Visual Feedback:**
- `moveTargetIndicator`: GameObject das am Zielort spawnt (z.B. Marker, Ring)
- `indicatorLifetime`: Wie lange der Indikator sichtbar bleibt
- `showPath`: Pfad im Scene View anzeigen
- `pathColor`: Farbe des Pfades

### 3. UnitSelector erweiterte Settings

**Movement Settings:**
- `groundLayer`: Layer für Boden/Terrain (für Rechtsklick)
- `useFormations`: Mehrere Einheiten in Formation bewegen
- `formationSpacing`: Abstand zwischen Einheiten in Formation

## Verwendung

### Steuerung
- **Linksklick**: Einheit(en) auswählen (wie vorher)
- **Rechtsklick auf Boden**: Ausgewählte Einheiten bewegen
- **Shift + Rechtsklick**: *(zukünftig: Attack-Move, Patrol, etc.)*

### NavMesh Setup (Empfohlen)

1. **Ground/Terrain vorbereiten:**
   - Wählen Sie Ihr Terrain/Ground aus
   - Im Inspector: Static ? Navigation Static ?
   
2. **NavMesh backen:**
   - Window ? AI ? Navigation
   - Im "Bake" Tab
   - Agent Radius und Height einstellen
   - "Bake" Button klicken

3. **Units testen:**
   - Units werden automatisch NavMeshAgent erhalten
   - Rechtsklick zum Bewegen

### Layer Setup

**Empfohlene Layer-Konfiguration:**

```
Layer 6: Ground (für Terrain, Straßen, etc.)
Layer 7: Selectable (für Units, Gebäude)
```

**Im UnitSelector:**
- `selectableLayer`: Nur "Selectable" Layer
- `groundLayer`: Nur "Ground" Layer

Das verhindert, dass Rechtsklicks auf Units als Bewegungsbefehle interpretiert werden.

## Code-Beispiele

### Einheit manuell bewegen

```csharp
Controllable unit = GetComponent<Controllable>();

// Zu Position bewegen
Vector3 targetPos = new Vector3(10, 0, 5);
unit.MoveTo(targetPos);

// Mit Formation Offset bewegen
Vector3 offset = new Vector3(2, 0, 0);
unit.MoveTo(targetPos, offset);

// Stoppen
unit.Stop();
```

### Status abfragen

```csharp
Controllable unit = GetComponent<Controllable>();

if (unit.IsMoving)
{
    Debug.Log("Einheit bewegt sich zu: " + unit.TargetPosition);
}

if (unit.HasTarget)
{
    Debug.Log("Einheit hat ein Ziel");
}
```

### Eigene Controllable Units

```csharp
public class Soldier : Controllable
{
    protected override void OnMoveCommand(Vector3 destination)
    {
     base.OnMoveCommand(destination);
  // Spiele "Bewegung bestätigt" Sound
        AudioSource.PlayClipAtPoint(moveSound, transform.position);
    }

 protected override void OnReachedDestination()
    {
 base.OnReachedDestination();
        // Spiele "Angekommen" Animation
        animator.SetTrigger("Idle");
  }

    protected override void OnStop()
    {
        base.OnStop();
        // Sofort stoppen
    animator.SetTrigger("Stop");
    }
}
```

### Formationen anpassen

Im `UnitSelector` können Sie die Formation-Logik erweitern:

```csharp
// Aktuelle Implementation: Grid Formation
// Einheiten werden in einem Raster angeordnet

// Für andere Formationen (Linie, Keil, etc.) 
// können Sie MoveUnitsInFormation() überschreiben
```

## Erweiterte Features

### 1. Bewegungsziel-Indikator erstellen

**Erstellen Sie ein Prefab:**
1. Create ? 3D Object ? Cylinder
2. Scale: (0.5, 0.05, 0.5)
3. Material: Emissive, grüne Farbe
4. Optional: Fügen Sie Animation hinzu (Pulse, Fade)
5. Speichern als Prefab: "MoveTargetIndicator"
6. Weisen Sie es dem `moveTargetIndicator` Feld zu

### 2. NavMesh Obstacles

Für dynamische Hindernisse:

```csharp
// Auf Gebäuden/Objekten:
NavMeshObstacle obstacle = gameObject.AddComponent<NavMeshObstacle>();
obstacle.carving = true;
```

### 3. Verschiedene Unit-Geschwindigkeiten

```csharp
public class FastScout : Controllable
{
    void Start()
    {
 // Überschreibe moveSpeed für diesen Unit-Typ
        var agent = GetComponent<NavMeshAgent>();
  if (agent != null)
        {
     agent.speed = 10f; // Doppelt so schnell
        }
}
}
```

### 4. Patrol System (Beispiel für Erweiterung)

```csharp
public class PatrolController : Controllable
{
    [SerializeField] private Transform[] patrolPoints;
    private int currentPatrolIndex = 0;

    public void StartPatrol()
    {
   if (patrolPoints.Length > 0)
        {
            MoveTo(patrolPoints[0].position);
        }
    }

protected override void OnReachedDestination()
    {
   base.OnReachedDestination();
        
   // Nächster Patrol Point
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        MoveTo(patrolPoints[currentPatrolIndex].position);
    }
}
```

## Troubleshooting

### Problem: Units bewegen sich nicht

**Lösung 1: NavMesh fehlt**
- Stellen Sie sicher, dass NavMesh gebacken ist
- Window ? AI ? Navigation ? Bake

**Lösung 2: Layer falsch**
- `groundLayer` im UnitSelector muss den Boden-Layer enthalten
- Rechtsklick muss auf Ground-Objekt mit Collider treffen

**Lösung 3: NavMeshAgent fehlt**
- Wird normalerweise automatisch hinzugefügt
- Prüfen Sie, ob `useNavMesh` aktiviert ist

### Problem: Units überlappen sich

**Lösung:**
- Erhöhen Sie `formationSpacing` im UnitSelector (z.B. auf 3f)
- Fügen Sie Collider zu Units hinzu für gegenseitige Kollision
- In NavMeshAgent: Erhöhen Sie "Radius" und aktivieren Sie "Obstacle Avoidance"

### Problem: Units bewegen sich durch Wände

**Lösung:**
- NavMesh neu backen mit korrekten Settings
- Agent Radius erhöhen im Navigation Bake Panel
- Wände müssen Navigation Static sein

### Problem: Formation sieht komisch aus

**Lösung:**
- Passen Sie `formationSpacing` an Unit-Größe an
- Für andere Formationen: Ändern Sie `MoveUnitsInFormation()` Logik
- Beispiel für Line Formation:

```csharp
// Im UnitSelector, ersetzen Sie die Grid-Logik:
for (int i = 0; i < units.Count; i++)
{
    float offsetX = (i - units.Count / 2f) * formationSpacing;
    Vector3 offset = new Vector3(offsetX, 0, 0);
    units[i].MoveTo(centerPosition, offset);
}
```

## Performance-Tipps

1. **NavMesh vs Manual:**
   - NavMesh: Besser für komplexe Levels mit Hindernissen
   - Manual: Besser für einfache/flache Levels (performance)

2. **Viele Units:**
   - Verwenden Sie Object Pooling für Move Indicators
   - Reduzieren Sie NavMeshAgent Update-Frequenz
   - Gruppieren Sie Units in "Squads"

3. **NavMesh Optimization:**
   ```csharp
   navAgent.updateRotation = false; // Eigene Rotation
   navAgent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
   ```

## Nächste Schritte

Das System ist bereit für Erweiterungen:
- ? Attack-Move Command
- ? Patrol System
- ? Unit Abilities (Skills)
- ? Formation Types (Line, Wedge, Circle)
- ? Waypoint System (Shift+Rechtsklick für Queue)
- ? Auto-Attack feindliche Einheiten
- ? Unit States (Idle, Moving, Attacking, etc.)

## Integration mit anderen Systemen

### Mit AI/Combat System:

```csharp
public class CombatUnit : Controllable
{
    [SerializeField] private float attackRange = 5f;
    private BaseUnit targetEnemy;

    protected override void OnMoveCommand(Vector3 destination)
    {
        base.OnMoveCommand(destination);
        // Lösche aktuelles Attack-Target
        targetEnemy = null;
    }

    void Update()
    {
        if (targetEnemy != null && !IsMoving)
        {
            // Attack Logic
     if (Vector3.Distance(transform.position, targetEnemy.transform.position) <= attackRange)
   {
          Attack(targetEnemy);
            }
            else
            {
    // Verfolge Feind
                MoveTo(targetEnemy.transform.position);
     }
}
    }
}
```

# ?? DEPLOY ZONE SYSTEM

## Overview
```
DeployZone = Drop-off point for Harvesters
   = ResourceCollector must have DeployZone component
  = Harvesters bring resources here
```

## Components

### DeployZone Component
```csharp
Location: Assets/Runtime/Scripts/Resources/DeployZone.cs

Attached to: ResourceCollector GameObject

Purpose:
  ? Provides drop-off point for Harvesters
  ? Manages queue of waiting Harvesters
  ? Transfers resources to ResourceManager
  ? Visual and audio feedback
```

## Setup

### 1. Add DeployZone to ResourceCollector

```
In Unity Editor:
1. Select ResourceCollector Prefab/GameObject
2. Add Component ? DeployZone
3. Configure settings:
   - Deploy Radius: 5m (how close harvester must be)
   - Max Queue Size: 5 (how many can wait)
   - Deploy Duration: 1s (how long to unload)
```

### 2. DeployZone Inspector Settings

```
???????????????????????????????????????????
? DeployZone Component   ?
???????????????????????????????????????????
? Zone Settings:              ?
?   Deploy Radius: 5 ?
?   Deploy Point: (Auto-created)   ?
?   Show Gizmos: ?               ?
?        ?
? Visual Feedback:  ?
?   Deploy Effect Prefab: (Optional)      ?
?   Deploy Sound: (Optional) ?
?   Deploy Sound Volume: 1       ?
?  ?
? Resource Settings:     ?
?   Resource Manager: (Auto-found)        ?
?   Deploy Duration: 1 ?
?                 ?
? Queue Settings:                 ?
?   Max Queue Size: 5    ?
?   Queue Positions: (Auto-generated)     ?
???????????????????????????????????????????
```

## How It Works

### Harvester Workflow
```
1. Harvester collects resources from Collectable
   ? currentCarried = 50 (full!)
   
2. Harvester finds nearest ResourceCollector
   ? Calls GatherFrom() or auto-finds
   
3. Harvester moves to DeployZone
   ? State: MovingToCollector
   ? Target: ResourceCollector with DeployZone
   
4. Harvester enters DeployZone radius
   ? Checks: CanDeploy(harvester)
   ? Team: Same team? ?
   ? HasResources: true ?
   ? HasSpace: Queue not full? ?
   
5. DeployZone assigns queue position
   ? Position: Circle around deploy point
   ? Harvester moves to queue position
   
6. DeployZone starts deploy
   ? StartDeploy(harvester)
   ? Wait: deployDuration (1 second)
   
7. Resources transferred
   ? resourceManager.AddResources(amount)
   ? harvester.ClearResources()
   ? Play effects + sound
   
8. Harvester returns to gathering
   ? State: Idle
   ? Ready for next gather cycle
```

### Code Flow
```csharp
// In HarvesterUnit.cs:
void UpdateMovingToCollector() {
    if (Vector3.Distance(transform.position, targetCollector.transform.position) < 3f) {
        // Reached collector!
  DeployZone zone = targetCollector.GetComponent<DeployZone>();
  if (zone != null && zone.CanDeploy(this)) {
            zone.StartDeploy(this);
        currentState = HarvesterState.Unloading;
        }
    }
}

// In DeployZone.cs:
public void StartDeploy(HarvesterUnit harvester) {
currentlyDeploying.Add(harvester);
    StartCoroutine(DeployCoroutine(harvester));
}

IEnumerator DeployCoroutine(HarvesterUnit harvester) {
  yield return new WaitForSeconds(deployDuration);
    
    // Transfer resources
    int amount = harvester.GetCarriedAmount();
    ResourceType type = harvester.GetCarriedResourceType();
    
    resourceManager.AddResources(...); // Add to player resources
    harvester.ClearResources(); // Clear harvester
    
    PlayDeployEffects(); // Visual + Audio
}
```

## DeployZone Features

### 1. **Team-Based Access**
```csharp
public bool CanDeploy(HarvesterUnit harvester) {
// Check team
    if (harvesterTeam.CurrentTeam != zoneTeam.CurrentTeam) {
        return false; // Wrong team!
    }
    
    // Player harvesters ? Player ResourceCollector ?
    // AI harvesters ? AI ResourceCollector ?
    // Mixed teams ? ?
}
```

### 2. **Queue Management**
```csharp
Queue System:
  - Max 5 Harvesters can deploy simultaneously
  - Each gets assigned a queue position
  - Positions arranged in circle around deploy point
  - Auto-cleanup of finished/destroyed harvesters
  
Example:
  Harvester #1 ? Position: Front (0°)
  Harvester #2 ? Position: Right (72°)
  Harvester #3 ? Position: Back-Right (144°)
  Harvester #4 ? Position: Back-Left (216°)
  Harvester #5 ? Position: Left (288°)
```

### 3. **Resource Transfer**
```csharp
Resource Types:
  - Food: resourceManager.AddResources(amount, 0, 0, 0)
  - Wood: resourceManager.AddResources(0, amount, 0, 0)
  - Stone: resourceManager.AddResources(0, 0, amount, 0)
  - Gold: resourceManager.AddResources(0, 0, 0, amount)

Example:
  Harvester carries: 50 Gold
  ? resourceManager.AddResources(0, 0, 0, 50)
  ? Player Gold: 500 ? 550 ?
```

### 4. **Visual Feedback**
```csharp
Gizmos (in Scene View):
  - Green Circle: Deploy radius (5m)
  - Green Sphere: Deploy point (center)
  - Yellow Spheres: Queue positions (5 spots)
  - Lines: Connect queue positions to center
  
In-Game:
  - Deploy Effect Prefab: Particle effect when deploying
  - Deploy Sound: Audio clip when resources transferred
- Harvester animation: (if implemented)
```

## Integration with Harvesters

### Required HarvesterUnit Methods
```csharp
// In Assets/Runtime/Scripts/Units/HarvesterUnit.cs

public bool HasResources => currentCarried > 0;

public int GetCarriedAmount() => currentCarried;

public ResourceType GetCarriedResourceType() => carriedResourceType;

public void ClearResources() {
    currentCarried = 0;
    UpdateCarryVisual();
}
```

### Harvester States
```csharp
enum HarvesterState {
    Idle,           // Waiting for orders
    MovingToResource,  // Going to Collectable
    Harvesting,    // Collecting resources
    MovingToCollector, // Going to ResourceCollector ? NEW!
    Unloading     // Deploying at DeployZone ? NEW!
}
```

## Example Scenario

### Player Base
```
[Player HQ]
    ? produces
[Harvester Unit] ???????
          ?
         ? gathers from
          [Gold Collectable]
              ?
           ? carries 50 Gold
  ?
      ? moves to
?????????????????????????????
      ?  [Resource Collector]     ?
      ?  • DeployZone Component   ?
      ?  • Deploy Radius: 5m      ?
      ?  • Team: Player      ?
      ?????????????????????????????
           ?
           ? deploys
     [ResourceManager Player]
               Gold: +50 ?
```

### AI Base (Automatic)
```
[AI HQ]
    ? AI produces
[AI Harvester] ???????
    ?
          ? AI assigns to gather
   [Stone Collectable]
  ?
        ? carries 50 Stone
   ?
            ? AI finds nearest collector
      ????????????????????????????
      ?  [AI Resource Collector] ?
      ?  • DeployZone Component  ?
      ?  • Team: Enemy           ?
 ????????????????????????????
   ?
     ? auto-deploys
  [ResourceManager AI]
         Stone: +50 ?
```

## Debugging

### Console Logs
```
Expected Logs (Success):
  DeployZone: Found Player ResourceManager
  DeployZone: Started deploy from Harvester_01 (Queue: 1/5)
  DeployZone: Harvester deployed 50 Gold
  Harvester_01: Resources cleared

Expected Logs (Fail):
  DeployZone: Full! (5/5)
  DeployZone: Cannot deploy from Harvester_02
```

### Gizmos
```
In Scene View:
  ? Show Gizmos enabled
  
  Green = Deploy zone active
  Red = Deploy zone full
  Yellow = Queue positions
  
Select DeployZone:
    ? Shows larger radius
    ? Shows forward direction (blue arrow)
```

### Testing Checklist
```
? ResourceCollector has DeployZone component
? DeployZone has correct Team (Player/Enemy)
? ResourceManager auto-found or assigned
? Harvester can reach DeployZone (no obstacles)
? Harvester has resources (HasResources = true)
? Queue has space (< maxQueueSize)
? Resources actually transfer to ResourceManager
? Harvester clears resources after deploy
? Harvester returns to Idle/gathering after deploy
```

## Performance Notes

### Optimization
```
? Auto-cleanup of null harvesters (in Update)
? Queue limit (max 5 simultaneous)
? Coroutine-based deploy (non-blocking)
? Minimal physics checks
? Team-based filtering (no cross-team checks)
```

### Resource Usage
```
Per DeployZone:
- 1 GameObject (Deploy Point)
  - 1 Coroutine per deploying harvester (max 5)
  - 1 List<HarvesterUnit> (queue tracking)
  - 1 List<Vector3> (queue positions, cached)
  
Memory: ~1 KB per DeployZone
Performance: Negligible (< 0.1ms per frame)
```

## Troubleshooting

### Problem: Harvester doesn't deploy
```
Check:
1. DeployZone component exists on ResourceCollector
2. Harvester.HasResources = true
3. Same team (Player harvester ? Player collector)
4. Queue not full (< 5 harvesters)
5. In range (distance < deployRadius)

Console Logs:
  - "Cannot deploy" ? Check team or HasResources
  - "Full!" ? Queue is full, wait
  - No logs ? Harvester not trying to deploy
```

### Problem: Resources don't transfer
```
Check:
1. ResourceManager is assigned/found
2. DeployCoroutine completes (wait deployDuration)
3. harvester.GetCarriedAmount() > 0
4. resourceManager.AddResources() called

Debug:
  Add Debug.Log in DeployCoroutine:
  Debug.Log($"Transferring {amount} {type}");
```

### Problem: Multiple harvesters overlap
```
Solution:
  - Increase deployRadius (5m ? 8m)
  - Decrease maxQueueSize (5 ? 3)
  - Check queue positions spacing

Gizmos should show separate positions!
```

## Advanced Features

### Custom Queue Layouts
```csharp
// In Inspector: Manually set queue positions
queuePositions:
  [0] = (3, 0, 0)   // Front
  [1] = (-3, 0, 0)  // Back
  [2] = (0, 0, 3)   // Right
  [3] = (0, 0, -3)  // Left

? Custom layout instead of circle!
```

### Deploy Effects
```csharp
// Assign in Inspector:
deployEffectPrefab = Particle System (gold coins, sparkles, etc.)

? Spawns at Deploy Point when resources transferred
? Auto-destroyed after 2 seconds
```

### Deploy Sound
```csharp
// Assign in Inspector:
deploySound = Audio Clip (coins clink, resource sound)

? Plays at Deploy Point (3D audio)
? Volume: deploySoundVolume (0-1)
```

## Summary

? **DeployZone = Essential for Harvester Economy**
? **Auto-setup (just add component!)**
? **Team-based (no cross-team transfers)**
? **Queue management (up to 5 harvesters)**
? **Visual feedback (Gizmos + Effects)**
? **Performance optimized**

**Add DeployZone to all ResourceCollectors for working harvester economy!** ?????

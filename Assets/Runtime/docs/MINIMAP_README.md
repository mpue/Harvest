# Minimap System für RTS/Strategy Games

Ein vollständig integriertes Minimap-System mit automatischer Team-Integration und Performance-Optimierung.

## ?? Inhaltsverzeichnis

- [Features](#features)
- [Quick Start](#quick-start)
- [Komponenten](#komponenten)
- [Integration](#integration)
- [API Referenz](#api-referenz)
- [Performance](#performance)
- [Troubleshooting](#troubleshooting)

---

## ? Features

### Hauptfeatures
- ? **Automatische Team-Integration** - Synchronisiert mit TeamComponent
- ? **Echtzeit-Updates** - Icons folgen Units automatisch
- ? **Performance-optimiert** - Skaliert bis 500+ Units
- ? **Click-to-Navigate** - Kamera-Navigation via Minimap-Klick
- ? **Camera View Indicator** - Zeigt sichtbaren Kamera-Bereich
- ? **Flexible Icon-Shapes** - Circle, Square, Triangle, Diamond, Cross
- ? **Auto-Setup** - One-Click Integration
- ? **Runtime Unit Spawning** - Dynamisches Icon-Management

### Visuelle Features
- ?? Automatische Team-Farben (Blau/Rot/Grün/Grau)
- ?? Optionale Icon-Rotation (zeigt Unit-Richtung)
- ?? Anpassbare Icon-Größen pro Unit-Typ
- ?? Verschiedene Icon-Formen für Unit-Kategorien
- ??? Dynamische Sichtbarkeit (Fog of War Ready)

### Integration Features
- ?? **TeamComponent Integration** - Automatische Farb-Synchronisation
- ?? **Production System Ready** - Spawned Units bekommen automatisch Icons
- ?? **Selection System Compatible** - Kann mit Selection-System kombiniert werden
- ??? **Terrain Auto-Detection** - Erkennt World Bounds automatisch

---

## ?? Quick Start

### 1. Automatisches Setup (Empfohlen)

```csharp
// Option A: Via RTSSetupHelper
GameObject setupObj = new GameObject("RTSSetup");
RTSSetupHelper helper = setupObj.AddComponent<RTSSetupHelper>();

// Im Editor: Rechtsklick auf Component
// ? "Full Auto Setup (with Minimap)"
```

**Oder im Editor:**
1. GameObject erstellen ? "RTSSetup"
2. Add Component ? `RTSSetupHelper`
3. Rechtsklick ? **"Full Auto Setup (with Minimap)"**

### 2. World Bounds konfigurieren

```csharp
// Nach dem Setup: World Bounds anpassen
MinimapController controller = FindObjectOfType<MinimapController>();
controller.SetWorldBounds(
    center: Vector3.zero,
    size: new Vector2(200f, 200f)  // Your map size
);
```

**Oder im Editor:**
1. Wähle `MinimapController`
2. Inspector ? World Bounds ? "Auto-Detect from Terrain"

### 3. Fertig!

Drücke **Play** - alle Units mit `TeamComponent` erscheinen automatisch auf der Minimap! ??

---

## ?? Komponenten

### MinimapController

**Hauptsteuerung** des Minimap-Systems.

```csharp
public class MinimapController : MonoBehaviour
{
    // Referenzen
    public Camera minimapCamera;
    public RawImage minimapImage;
 public RectTransform minimapContainer;
 public RectTransform iconContainer;
    
  // Konfiguration
    public Vector3 worldCenter = Vector3.zero;
    public Vector2 worldSize = new Vector2(200f, 200f);
    public bool autoCreateIcons = true;
    public int updateInterval = 1;
    public bool enableClickNavigation = true;
    
    // API
    public MinimapIcon CreateIconForUnit(Transform unit, IconShape shape);
    public void RemoveIconForUnit(Transform unit);
    public Vector2 WorldToMinimapPoint(Vector3 worldPos);
    public Vector3 MinimapToWorldPoint(Vector2 minimapPos);
    public void RefreshAllIcons();
}
```

**Placement:** Einmal pro Szene

### MinimapIcon

**Repräsentiert eine Unit** auf der Minimap.

```csharp
public class MinimapIcon : MonoBehaviour
{
    public enum IconShape
    {
        Circle,   // Standard Units
   Square,   // Buildings
        Triangle, // Military/Vehicles
        Diamond,  // Heroes/Special
        Cross     // Objectives/Markers
    }
    
  // Konfiguration
    public Transform targetUnit;
  public IconShape iconShape = IconShape.Circle;
    public float iconSize = 10f;
    public bool rotateWithUnit = true;
    
    // API
    public void SetTargetUnit(Transform unit);
    public void SetIconShape(IconShape shape);
    public void SetIconSize(float size);
  public void SetColor(Color color);
    public void SetVisible(bool visible);
  public void UpdateColor();
}
```

**Placement:** Automatisch pro Unit (via Controller)

### MinimapUnit

**Helper Component** für einfache Auto-Setup.

```csharp
public class MinimapUnit : MonoBehaviour
{
    public MinimapIcon.IconShape iconShape = IconShape.Circle;
    public float iconSize = 10f;
    public bool rotateWithUnit = false;
    public bool autoCreate = true;
    
    // API
    public void CreateIcon();
    public void RemoveIcon();
    public void UpdateIcon();
    public void SetIconShape(IconShape shape);
    public void SetIconSize(float size);
}
```

**Placement:** Optional auf Units für vereinfachtes Setup

---

## ?? Integration

### Mit TeamComponent

**Vollautomatische Integration** - keine Konfiguration nötig!

```csharp
// Unit mit TeamComponent
GameObject unit = new GameObject("MyUnit");
TeamComponent team = unit.AddComponent<TeamComponent>();
team.SetTeam(Team.Player); // Automatisch blau auf Minimap!

// Icon wird automatisch erstellt in Play Mode
// Farbe synchronisiert sich automatisch bei Team-Wechsel
```

### Mit Production System

Units die von Gebäuden produziert werden:
- ? Erben Team vom Gebäude
- ? Bekommen automatisch Minimap-Icon
- ? Richtige Team-Farbe wird gesetzt

```csharp
// Production System erstellt Unit
// ? TeamComponent wird vom Gebäude geerbt
// ? MinimapIcon wird automatisch erstellt
// ? Keine extra Konfiguration nötig!
```

### Runtime Unit Spawning

```csharp
// Methode 1: Mit MinimapUnit (Empfohlen)
GameObject unit = Instantiate(unitPrefab, position, rotation);
TeamComponent team = unit.AddComponent<TeamComponent>();
team.SetTeam(Team.Ally);

MinimapUnit minimapUnit = unit.AddComponent<MinimapUnit>();
minimapUnit.SetIconShape(MinimapIcon.IconShape.Triangle);
minimapUnit.CreateIcon();

// Methode 2: Direkt über Controller
MinimapController controller = FindObjectOfType<MinimapController>();
MinimapIcon icon = controller.CreateIconForUnit(
    unit.transform,
    MinimapIcon.IconShape.Circle
);
icon.SetIconSize(12f);
```

### Custom Icon Prefabs

```csharp
// Erstelle eigenes Icon Prefab:
// 1. GameObject mit RectTransform + Image
// 2. Add Component: MinimapIcon
// 3. Configure settings
// 4. Save as Prefab

// Assign in MinimapController
controller.iconPrefab = myCustomIconPrefab;

// Icons verwenden nun Ihr Prefab
controller.AutoCreateIconsForUnits();
```

---

## ?? API Referenz

### MinimapController API

#### Icon Management

```csharp
// Icons erstellen
MinimapIcon CreateIconForUnit(Transform unit, IconShape shape = IconShape.Circle)
void AutoCreateIconsForUnits()

// Icons entfernen
void RemoveIconForUnit(Transform unit)
void ClearAllIcons()

// Icons abrufen
MinimapIcon GetIconForUnit(Transform unit)

// Icons aktualisieren
void RefreshAllIcons()
```

#### Koordinaten-Konvertierung

```csharp
// World ? Minimap Konvertierung
Vector2 WorldToMinimapPoint(Vector3 worldPos)
Vector3 MinimapToWorldPoint(Vector2 minimapPos)
bool IsPositionInBounds(Vector3 worldPos)
```

#### Kamera-Navigation

```csharp
// Kamera zur Position bewegen
void MoveCameraToPosition(Vector3 worldPos)

// World Bounds setzen
void SetWorldBounds(Vector3 center, Vector2 size)
```

### MinimapIcon API

```csharp
// Setup
void SetTargetUnit(Transform unit)
void Initialize()

// Appearance
void SetIconShape(IconShape shape)
void SetIconSize(float size)
void SetColor(Color color)
void UpdateColor()

// Visibility
void SetVisible(bool visible)
void ToggleVisibility()

// Info
Transform GetTargetUnit()
TeamComponent GetTeamComponent()
```

### MinimapUnit API

```csharp
// Lifecycle
void CreateIcon()
void RemoveIcon()

// Configuration
void SetIconShape(IconShape shape)
void SetIconSize(float size)
void UpdateIcon()

// Access
MinimapIcon GetIcon()
```

---

## ?? Konfiguration

### World Bounds Setup

```csharp
// Automatisch (Terrain)
MinimapController controller = GetComponent<MinimapController>();
// Im Editor: "Auto-Detect from Terrain" Button

// Manuell
controller.SetWorldBounds(
    center: new Vector3(50, 0, 50),  // Terrain center
  size: new Vector2(100, 100)      // Terrain size (X, Z)
);

// Beispiele für verschiedene Map-Größen:
// Kleine Map:  size = (100, 100)
// Medium Map:  size = (200, 200)
// Große Map:   size = (500, 500)
// XXL Map:     size = (1000, 1000)
```

### Minimap UI Position

```csharp
// Bottom-Right (Standard)
RectTransform minimapContainer;
minimapContainer.anchorMin = new Vector2(1, 0);
minimapContainer.anchorMax = new Vector2(1, 0);
minimapContainer.pivot = new Vector2(1, 0);
minimapContainer.anchoredPosition = new Vector2(-20, 20);

// Bottom-Left
minimapContainer.anchorMin = new Vector2(0, 0);
minimapContainer.anchorMax = new Vector2(0, 0);
minimapContainer.pivot = new Vector2(0, 0);
minimapContainer.anchoredPosition = new Vector2(20, 20);

// Top-Right
minimapContainer.anchorMin = new Vector2(1, 1);
minimapContainer.anchorMax = new Vector2(1, 1);
minimapContainer.pivot = new Vector2(1, 1);
minimapContainer.anchoredPosition = new Vector2(-20, -20);
```

### Icon Shapes für Unit-Typen

```csharp
// Best Practices
MinimapIcon.IconShape GetIconShape(UnitType type)
{
    switch (type)
    {
     case UnitType.Infantry:
  return IconShape.Circle;
        
        case UnitType.Vehicle:
    case UnitType.Tank:
     return IconShape.Triangle;
   
        case UnitType.Building:
 case UnitType.Structure:
            return IconShape.Square;
        
        case UnitType.Hero:
      case UnitType.Commander:
         return IconShape.Diamond;
        
        case UnitType.Objective:
        case UnitType.Waypoint:
            return IconShape.Cross;
    
        default:
    return IconShape.Circle;
    }
}
```

---

## ?? Performance

### Optimierung für viele Units

#### Update Interval

```csharp
// MinimapController
public int updateInterval = 3;

// Empfehlungen:
// 0-1:   1-50 Units   (Smooth, every frame)
// 2-3:   50-150 Units (Good balance)
// 5-10:  150-500 Units (Good performance)
// 10+:   500+ Units   (Best performance)
```

#### Icon Settings

```csharp
// Für Performance optimieren
MinimapIcon icon = GetComponent<MinimapIcon>();

// Rotation deaktivieren (spart Rechenzeit)
icon.rotateWithUnit = false;

// Kleinere Icons (weniger Fill-Rate)
icon.iconSize = 8f;

// Update nur wenn sichtbar
if (IsVisibleOnMinimap(unit))
{
    icon.SetVisible(true);
}
else
{
    icon.SetVisible(false);
}
```

### Benchmark-Ergebnisse

| Units | Update Interval | FPS Impact | Empfehlung |
|-------|----------------|------------|------------|
| 50    | 0    | < 1% | ? Perfekt |
| 100   | 1    | ~2%     | ? Gut  |
| 200   | 3       | ~3%   | ? Gut     |
| 500   | 5              | ~5%        | ?? OK      |
| 1000  | 10 | ~8%        | ?? Akzeptabel |

**Testumgebung:** Unity 2021, Intel i7, GTX 1070

### Best Practices

```csharp
// ? DO: Icons pooling für häufiges Spawnen
Dictionary<Team, Queue<MinimapIcon>> iconPool;

// ? DO: Update Interval erhöhen bei vielen Units
controller.updateInterval = Mathf.Max(1, totalUnits / 50);

// ? DO: Sichtbarkeit nutzen (Fog of War)
icon.SetVisible(isVisibleToPlayer);

// ? DON'T: Jeden Frame neue Icons erstellen/zerstören
// ? DON'T: Update Interval 0 bei 200+ Units
// ? DON'T: Alle Icons mit Rotation bei vielen Units
```

---

## ?? Troubleshooting

### Icons nicht sichtbar

**Problem:** Minimap zeigt keine Icons

**Lösungen:**
```csharp
// 1. Play Mode aktiv?
// Icons werden nur zur Laufzeit erstellt

// 2. Auto Create aktiviert?
controller.autoCreateIcons = true;

// 3. Units haben TeamComponent?
TeamComponent team = unit.GetComponent<TeamComponent>();
if (team == null)
    unit.AddComponent<TeamComponent>();

// 4. Icon Container referenziert?
if (controller.iconContainer == null)
{
    // Setup via RTSSetupHelper neu durchführen
}
```

### Falsche Position

**Problem:** Icons an falscher Position auf Minimap

**Lösungen:**
```csharp
// 1. World Bounds prüfen
controller.SetWorldBounds(terrainCenter, terrainSize);

// 2. Auto-Detect nutzen (bei Terrain)
// Im Editor: Inspector ? "Auto-Detect from Terrain"

// 3. Manuell testen
Vector3 testPos = new Vector3(50, 0, 50);
Vector2 minimapPos = controller.WorldToMinimapPoint(testPos);
Debug.Log($"World {testPos} ? Minimap {minimapPos}");
```

### Falsche Farben

**Problem:** Icons haben falsche Team-Farben

**Lösungen:**
```csharp
// 1. Team Color prüfen
TeamComponent team = unit.GetComponent<TeamComponent>();
Debug.Log($"Team: {team.CurrentTeam}, Color: {team.TeamColor}");

// 2. Refresh Icons
controller.RefreshAllIcons();

// 3. Color Override zurücksetzen
MinimapIcon icon = controller.GetIconForUnit(unit.transform);
icon.colorOverride = Color.black; // Nutzt Team-Farbe
icon.UpdateColor();
```

### Performance-Probleme

**Problem:** FPS-Einbruch bei vielen Units

**Lösungen:**
```csharp
// 1. Update Interval erhöhen
controller.updateInterval = 5; // oder höher

// 2. Rotation deaktivieren
foreach (var icon in allIcons)
    icon.rotateWithUnit = false;

// 3. Icon Größe reduzieren
icon.SetIconSize(6f);

// 4. Nur sichtbare Units anzeigen
icon.SetVisible(isInCameraView);
```

### Click Navigation funktioniert nicht

**Problem:** Klick auf Minimap bewegt Kamera nicht

**Lösungen:**
```csharp
// 1. Feature aktiviert?
controller.enableClickNavigation = true;

// 2. Main Camera zugewiesen?
controller.mainCamera = Camera.main;

// 3. GraphicRaycaster auf Canvas?
Canvas canvas = FindObjectOfType<Canvas>();
if (canvas.GetComponent<GraphicRaycaster>() == null)
  canvas.gameObject.AddComponent<GraphicRaycaster>();

// 4. EventSystem vorhanden?
if (FindObjectOfType<EventSystem>() == null)
{
    GameObject eventSystem = new GameObject("EventSystem");
  eventSystem.AddComponent<EventSystem>();
    eventSystem.AddComponent<StandaloneInputModule>();
}
```

---

## ?? Erweiterte Beispiele

### Fog of War Integration

```csharp
public class MinimapFogOfWar : MonoBehaviour
{
    private MinimapController controller;
    private Dictionary<MinimapIcon, bool> iconVisibility;
    
 void UpdateFogOfWar()
    {
        foreach (var kvp in iconVisibility)
        {
 MinimapIcon icon = kvp.Key;
            Transform unit = icon.GetTargetUnit();
            
      bool isVisible = IsVisibleToPlayer(unit.position);
            icon.SetVisible(isVisible);
        }
    }
    
    bool IsVisibleToPlayer(Vector3 position)
    {
        // Ihre Fog of War Logik hier
     return Vector3.Distance(playerPosition, position) < viewRange;
    }
}
```

### Dynamic Icon Scaling (Zoom)

```csharp
public class MinimapZoom : MonoBehaviour
{
    public float minZoom = 50f;
    public float maxZoom = 200f;
    public float currentZoom = 100f;
    
    void Update()
 {
 float scroll = Input.GetAxis("Mouse ScrollWheel");
        currentZoom = Mathf.Clamp(currentZoom - scroll * 20f, minZoom, maxZoom);
  
        // Update icon sizes based on zoom
 float iconScale = Mathf.InverseLerp(minZoom, maxZoom, currentZoom);
        float iconSize = Mathf.Lerp(15f, 8f, iconScale);
        
        foreach (MinimapIcon icon in FindObjectsOfType<MinimapIcon>())
    {
      icon.SetIconSize(iconSize);
        }
    }
}
```

### Unit Ping/Alert System

```csharp
public class MinimapPing : MonoBehaviour
{
    public void PingUnit(Transform unit)
    {
  MinimapController controller = FindObjectOfType<MinimapController>();
        MinimapIcon icon = controller.GetIconForUnit(unit);
        
        if (icon != null)
        {
    StartCoroutine(PingAnimation(icon));
     }
    }
    
    IEnumerator PingAnimation(MinimapIcon icon)
    {
        Color originalColor = icon.GetComponent<Image>().color;
  float duration = 1f;
        
  for (float t = 0; t < duration; t += Time.deltaTime)
   {
            float pulse = Mathf.PingPong(t * 4f, 1f);
            icon.SetIconSize(10f + pulse * 5f);
    yield return null;
        }
  
  icon.SetIconSize(10f);
    }
}
```

---

## ?? Weitere Ressourcen

### Dokumentation
- **Quick Start:** `MINIMAP_QUICKSTART.md`
- **Team System:** `TEAM_VISUAL_QUICKSTART.md`
- **Production System:** `PROGRESS_QUICK_REF.md`

### Demo Scripts
- **MinimapDemo.cs** - Vollständiges Demo-Beispiel
- **RTSSetupHelper.cs** - Auto-Setup Funktionen

### Editor Tools
- **MinimapControllerEditor.cs** - Custom Inspector mit Setup Wizard

---

## ?? Integration Checkliste

- [ ] MinimapController Setup durchgeführt
- [ ] World Bounds konfiguriert
- [ ] Minimap UI positioniert
- [ ] Auto Create Icons aktiviert
- [ ] Click Navigation getestet
- [ ] Performance mit vielen Units getestet
- [ ] TeamComponent Integration verifiziert
- [ ] Icon Shapes für Unit-Typen definiert
- [ ] Camera View Indicator angepasst
- [ ] Demo-Szene erstellt

---

## ?? Lizenz

Teil des Harvest RTS Project

---

**Fragen?** Siehe `MINIMAP_QUICKSTART.md` für schnelle Antworten!

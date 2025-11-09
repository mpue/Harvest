# RTS Unit Selection System - Dokumentation

## Übersicht
Dieses System ermöglicht die Selektion von Einheiten und Gebäuden in einem RTS-Spiel mit Einzelauswahl und Rahmenauswahl.

## Komponenten

### 1. BaseUnit.cs
Basis-Klasse für alle selektierbaren Objekte (Einheiten und Gebäude).

**Features:**
- Visuelle Selektion mit Emission-Effekt
- Optional: Selection Indicator GameObject
- Unterscheidung zwischen Einheiten und Gebäuden
- Ereignisse für Selektion/Deselektion (überschreibbar)

**Setup:**
1. Fügen Sie das `BaseUnit` Script zu jedem selektierbaren Objekt hinzu
2. Konfigurieren Sie:
   - `unitName`: Name der Einheit
   - `isBuilding`: Aktivieren für Gebäude
 - `selectionIndicator`: Optional - GameObject das bei Selektion angezeigt wird (z.B. Ring am Boden)
   - `selectedColor`: Farbe für selektierte Objekte

### 2. UnitSelector.cs
Manager-Klasse für die Selektion. Sollte als Singleton auf einem GameObject in der Szene sein.

**Features:**
- **Einzelauswahl**: Linksklick auf Einheit
- **Mehrfachauswahl**: Shift + Linksklick zum Hinzufügen/Entfernen
- **Rahmenauswahl**: Linke Maustaste halten und ziehen
- **Abwählen**: ESC-Taste oder Klick ins Leere

**Setup:**
1. Erstellen Sie ein leeres GameObject "SelectionManager"
2. Fügen Sie das `UnitSelector` Script hinzu
3. Konfigurieren Sie:
   - `mainCamera`: Referenz zur Kamera (automatisch wenn leer)
   - `selectableLayer`: Layer für selektierbare Objekte
   - `allowMultiSelect`: Mehrfachauswahl erlauben
   - `selectBuildingsWithBox`: Gebäude mit Rahmen auswählbar
   - `boxColor`: Farbe des Auswahlrahmens
   - `boxBorderColor`: Farbe des Rahmens-Rands

## Verwendung

### Steuerung:
- **Linksklick**: Einheit auswählen
- **Shift + Linksklick**: Einheit zur Auswahl hinzufügen/entfernen
- **Linke Maustaste ziehen**: Auswahlrahmen für mehrere Einheiten
- **ESC**: Alle abwählen

### Code-Beispiele:

```csharp
// Zugriff auf den Selector
UnitSelector selector = FindObjectOfType<UnitSelector>();

// Alle ausgewählten Einheiten abrufen
List<BaseUnit> selected = selector.SelectedUnits;

// Spezifische Einheiten-Typen abrufen
List<MyUnitType> units = selector.GetSelectedUnitsOfType<MyUnitType>();

// Alle abwählen
selector.DeselectAll();

// Einzelne Einheit manuell auswählen
selector.SelectUnit(myUnit);
```

### Eigene Unit-Klassen erstellen:

```csharp
public class MyUnit : BaseUnit
{
    protected override void OnSelected()
    {
        base.OnSelected();
        // Eigene Logik bei Selektion
        Debug.Log("Meine Einheit wurde ausgewählt!");
    }

    protected override void OnDeselected()
    {
     base.OnDeselected();
        // Eigene Logik bei Deselektion
    }
}
```

## Empfohlenes Setup

### Layer-Konfiguration:
1. Erstellen Sie einen Layer "Selectable"
2. Weisen Sie allen Units/Gebäuden diesen Layer zu
3. Setzen Sie im UnitSelector: `selectableLayer` auf "Selectable"

### Collider:
- Jede BaseUnit benötigt einen Collider (Box, Sphere, Mesh, etc.)
- Der Collider sollte das gesamte Objekt umfassen

### Visual Feedback:
- **Option 1**: Emission-Material (automatisch)
  - Funktioniert mit Standard/URP/HDRP Materials mit Emission
  
- **Option 2**: Selection Indicator
  - Erstellen Sie einen Ring/Decal unter der Einheit
  - Weisen Sie ihn dem `selectionIndicator` Feld zu
  - Wird automatisch ein/ausgeblendet

## Erweiterte Features

### Selektion-Events in anderen Scripts abfangen:

```csharp
public class MyGameManager : MonoBehaviour
{
    private UnitSelector selector;
    
  void Start()
    {
        selector = FindObjectOfType<UnitSelector>();
    }
    
    void Update()
    {
  if (selector.SelectedCount > 0)
        {
         // Etwas mit ausgewählten Einheiten machen
  foreach (BaseUnit unit in selector.SelectedUnits)
  {
     // ... Logik ...
    }
      }
    }
}
```

### Performance-Tipps:
- Verwenden Sie Layer Masks um unnötige Raycasts zu vermeiden
- Bei vielen Einheiten: Optimieren Sie die Box-Selection mit Spatial Partitioning
- Nutzen Sie Object Pooling für Selection Indicators

## Bekannte Limitierungen
- Box-Selection verwendet `FindObjectsOfType<BaseUnit>()` - kann bei vielen Einheiten langsam sein
- Keine Frustum Culling für Box-Selection (alle Einheiten werden geprüft)

## Zukünftige Erweiterungen
- Gruppen-Verwaltung (Ctrl+1-9 für Gruppen)
- Doppelklick zum Auswählen aller Einheiten des gleichen Typs
- Formation-System für ausgewählte Einheiten
- Selection Priorization (Einheiten vor Gebäuden)

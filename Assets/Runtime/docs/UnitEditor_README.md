# Unit Editor

Ein umfassender Unity Editor für die Verwaltung aller Unit-Aspekte in einem zentralen Fenster.

## Übersicht

Der **Unit Editor** bietet eine einheitliche Oberfläche zur Bearbeitung aller Komponenten einer Unit:

- **BaseUnit** - Grundlegende Unit-Einstellungen
- **TeamComponent** - Team/Fraktion-Zugehörigkeit
- **Health** - Lebenspunkte und Regeneration
- **Controllable** - Bewegungssteuerung (nur für Nicht-Gebäude)
- **NavMeshAgent** - Navigation (nur für Nicht-Gebäude)
- **WeaponController** - Waffen und Kampfsystem
- **Preview** - Visuelle Vorschau der Unit

## Öffnen des Editors

**Menü**: `Tools > Unit Editor`

Der Editor öffnet sich als dockbares Fenster und kann wie jedes andere Unity-Fenster positioniert werden.

## Verwendung

### 1. Unit Auswählen

Es gibt zwei Möglichkeiten, eine Unit auszuwählen:

1. **Im Hierarchy-Fenster**: Wähle ein GameObject mit `BaseUnit`-Komponente aus - der Editor lädt es automatisch
2. **Im Editor-Fenster**: Ziehe ein GameObject in das "Selected Unit" Feld

?? **Wichtig**: Nur GameObjects mit einer `BaseUnit`-Komponente können bearbeitet werden.

### 2. Preview

Am Anfang des Editors wird eine 3D-Vorschau der Unit angezeigt. Diese zeigt:
- Das GameObject in interaktiver 3D-Ansicht
- Material-Preview
- Aktuelle Mesh-Darstellung

Die Preview kann mit der Maus gedreht und gezoomt werden.

### 3. Quick Actions

Der **Quick Actions** Bereich bietet schnelle Zugriffsmöglichkeiten:

#### Component Management

**Hinzufügen/Entfernen von Komponenten**:
- `Add/Remove Controllable` - Bewegungskomponente (nur Nicht-Gebäude)
- `Add/Remove NavMeshAgent` - Navigation (nur Nicht-Gebäude)
- `Add TeamComponent` - Team-Zugehörigkeit
- `Add/Remove Health` - Lebenspunkte
- `Add/Remove WeaponController` - Waffensystem

Rote Buttons = Komponente entfernen
Graue Buttons = Komponente hinzufügen

#### Setup Templates

Schnelle Vorlagen für häufige Unit-Typen:

**Setup Complete Unit** (Grün)
- Fügt alle essentiellen Komponenten hinzu
- TeamComponent
- Health
- Bei Nicht-Gebäuden: Controllable + NavMeshAgent

**Setup Combat Unit** (Grün)
- Wie "Complete Unit"
- + WeaponController für Kampffähigkeit

### 4. Base Unit Settings

Grundlegende Unit-Konfiguration:

```
Unit Name         - Name der Unit (z.B. "Warrior", "Archer")
Is Building       - Ist dies ein Gebäude? (deaktiviert Bewegung)
Selection Indicator - GameObject für Auswahl-Anzeige
Selected Color    - Farbe bei Auswahl
Normal Color      - Standard-Farbe
```

**Hinweis**: `Is Building` bestimmt, ob Controllable/NavMeshAgent benötigt werden.

### 5. Team Settings

Team/Fraktion-Konfiguration:

```
Team      - Player/Enemy/Ally/Neutral
Team Color  - Farbe für Team-Identifikation
```

Teams bestimmen Freund-Feind-Erkennung im Kampfsystem.

### 6. Health Settings

Lebenspunkte und Tod:

**Health**
```
Max Health     - Maximum HP
Can Regenerate        - Automatische Regeneration
  ?? Regeneration Rate   - HP pro Sekunde
  ?? Regeneration Delay  - Verzögerung nach Schaden
```

**Death Settings**
```
Destroy On Death   - Unit bei Tod zerstören
  ?? Destroy Delay- Verzögerung vor Zerstörung
Explosion Prefab   - Explosions-Effekt beim Tod
Explosion Scale    - Größe der Explosion
```

**Audio**
```
Hit Sounds     - Sounds bei Schadennahme (Array)
Death Sounds   - Sounds beim Tod (Array)
Audio Volume   - Lautstärke (0-1)
```

**Visual Feedback**
```
Flash On Hit        - Bei Treffer aufleuchten
  ?? Hit Flash Color   - Farbe des Aufblitzens
  ?? Hit Flash Duration - Dauer in Sekunden
```

### 7. Movement Settings (Controllable)

?? **Nur für Nicht-Gebäude** (`Is Building` = false)

**Movement**
```
Move Speed  - Bewegungsgeschwindigkeit
Rotation Speed   - Drehgeschwindigkeit
Stopping Distance   - Abstand zum Ziel
```

**Navigation**
```
Use NavMesh   - NavMesh-Navigation verwenden
    (false = manuelle Bewegung)
```

**Visual Feedback**
```
Move Target Indicator - Ziel-Marker Prefab
Indicator Lifetime    - Dauer der Anzeige
Show Path          - Pfad zum Ziel anzeigen
  ?? Path Color         - Farbe des Pfades
```

### 8. NavMesh Settings

?? **Nur für Nicht-Gebäude**

Standard Unity NavMeshAgent Einstellungen:

**Agent Settings**
```
Speed        - Bewegungsgeschwindigkeit
Angular Speed     - Drehgeschwindigkeit
Acceleration      - Beschleunigung
Stopping Distance - Abstand zum Ziel
Auto Braking      - Automatisches Abbremsen
```

**Avoidance**
```
Radius      - Kollisionsradius
Height      - Agent-Höhe
Base Offset - Höhen-Offset
```

### 9. Weapon Settings

Waffensystem (optional):

**Weapons**
```
Weapons (Array)   - Liste aller Weapon-Komponenten
```

Button: **Find Weapons in Children** - Findet automatisch alle Weapon-Komponenten in Child-Objekten

**Targeting**
```
Auto Acquire Targets      - Automatische Zielerfassung
Target Scan Interval      - Scan-Intervall (Sekunden)
Target Layer Mask - Layer für Ziele
Prioritize Closest Target - Nächstes Ziel bevorzugen
```

**Firing**
```
Auto Fire      - Automatisches Feuern
Fire Interval  - Zeit zwischen Schüssen
```

## Workflow-Beispiele

### Neuen Kämpfer erstellen

1. GameObject mit BaseUnit-Komponente erstellen
2. Im Unit Editor öffnen
3. **Setup Combat Unit** klicken
4. In **Base Unit Settings**:
   - Unit Name: "Warrior"
   - Is Building: false
5. In **Team Settings**:
   - Team: Player
   - Team Color: Blau
6. In **Health Settings**:
   - Max Health: 100
   - Explosion Prefab zuweisen
7. In **Movement Settings**:
   - Move Speed: 5
   - Use NavMesh: true
8. In **Weapon Settings**:
   - Weapon-Child-Objekte hinzufügen
   - **Find Weapons in Children** klicken
   - Auto Fire: true

### Gebäude erstellen

1. GameObject mit BaseUnit-Komponente erstellen
2. Im Unit Editor öffnen
3. **Setup Complete Unit** klicken
4. In **Base Unit Settings**:
 - Unit Name: "Barracks"
   - Is Building: **true**
5. In **Team Settings**:
 - Team: Player
6. In **Health Settings**:
   - Max Health: 500
   - Can Regenerate: false

?? Hinweis: Bei `Is Building = true` werden Controllable/NavMeshAgent-Bereiche ausgeblendet.

### Nicht-Kämpfer (Worker) erstellen

1. GameObject mit BaseUnit-Komponente erstellen
2. Im Unit Editor öffnen
3. **Setup Complete Unit** klicken (ohne Waffen)
4. In **Base Unit Settings**:
   - Unit Name: "Worker"
   - Is Building: false
5. In **Team Settings**:
   - Team: Player
6. In **Health Settings**:
   - Max Health: 50
7. In **Movement Settings**:
   - Move Speed: 4
   - Use NavMesh: true

Kein WeaponController hinzufügen - Unit kann sich bewegen aber nicht kämpfen.

## Tipps & Tricks

### Component-Abhängigkeiten

Der Editor verwaltet automatisch Abhängigkeiten:

- `Is Building = true` ? Keine Controllable/NavMeshAgent Bereiche
- `Is Building = false` ? Controllable/NavMeshAgent empfohlen
- `WeaponController` ? Benötigt `TeamComponent` für Freund-Feind-Erkennung
- `Controllable` ? Arbeitet optimal mit `NavMeshAgent`

### Vorlagen verwenden

Die Setup-Templates sind ideal für schnelle Prototypen:

1. **Setup Complete Unit** ? Basis-Unit mit allen essentiellen Komponenten
2. **Setup Combat Unit** ? Kampf-fähige Unit
3. Danach individuell anpassen

### Batch-Editing

Um mehrere Units gleichzeitig zu bearbeiten:

1. Erstelle eine Unit als Template
2. Konfiguriere im Unit Editor
3. Erstelle Prefab (Drag & Drop in Project)
4. Instantiere für weitere Units

### Debugging

Der Editor zeigt Warnungen an, wenn:
- Komponenten fehlen
- Gebäude-spezifische Einstellungen bei Nicht-Gebäuden
- Inkonsistente Konfigurationen

Gelbe Info-Boxen geben Hinweise zur korrekten Verwendung.

### Weapon Setup

Empfohlener Workflow:

1. Erstelle Child-GameObjects für Waffen
2. Füge `Weapon`-Komponenten hinzu
3. WeaponController hinzufügen
4. Button "Find Weapons in Children" klicken
5. Automatische Zuweisung aller Waffen

## Integration mit anderen Systemen

### UnitSelector

Der Unit Editor konfiguriert Units für den UnitSelector:
- `BaseUnit` ? Selektierbarkeit
- `Controllable` ? Bewegungsbefehle
- `IsBuilding` ? Selektionsverhalten

### RTSCamera

Zusammenspiel mit Kamera-Steuerung:
- Units mit `Controllable` ? Bewegungsbefehle via Rechtsklick
- `IsBuilding` ? Keine Bewegung, nur Selektion

### Combat System

Für Kampfsystem konfigurieren:
- `TeamComponent` ? Freund-Feind-Erkennung
- `Health` ? Schadenssystem
- `WeaponController` + `Weapon` ? Angriffsfähigkeit

## Troubleshooting

### "No BaseUnit component found"

**Problem**: GameObject hat keine BaseUnit-Komponente

**Lösung**: Füge manuell `BaseUnit`-Komponente hinzu oder wähle anderes GameObject

### Controllable/NavMeshAgent nicht sichtbar

**Problem**: `Is Building` ist auf `true` gesetzt

**Lösung**: In Base Unit Settings `Is Building` auf `false` setzen

### Weapons nicht gefunden

**Problem**: "Find Weapons in Children" findet keine Waffen

**Lösung**: 
1. Stelle sicher, dass Child-Objects `Weapon`-Komponenten haben
2. Waffen müssen Child-Objects sein, nicht auf selber Ebene
3. Manuell im Weapons-Array zuweisen

### Preview zeigt nichts

**Problem**: Keine 3D-Vorschau sichtbar

**Lösung**:
1. GameObject benötigt Renderer/MeshFilter
2. Lade Unit neu (Hierarchy-Selektion ändern)
3. Preview-Foldout ausklappen

## Technische Details

### Editor-Speicherung

Alle Änderungen werden sofort gespeichert:
- Undo/Redo wird unterstützt (Ctrl+Z / Ctrl+Y)
- Änderungen sind persistent im Scene
- Prefab-Änderungen werden markiert

### Performance

Der Editor ist optimiert für:
- Schnelles Umschalten zwischen Units
- Keine Performance-Auswirkungen im Play-Mode
- Minimaler Memory-Footprint

### Kompatibilität

- Unity 2021.3+ (URP)
- .NET Framework 4.7.1
- NavMesh Package erforderlich für Navigation

## Erweiterungen

Der Unit Editor kann erweitert werden:

```csharp
// In UnitEditorWindow.cs

// Neue Sektion hinzufügen:
void DrawCustomSection()
{
    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
    // Deine custom UI hier
    EditorGUILayout.EndVertical();
}

// In OnGUI() aufrufen:
showCustomSettings = EditorGUILayout.Foldout(showCustomSettings, "Custom Settings", true);
if (showCustomSettings)
{
    DrawCustomSection();
}
```

## Siehe auch

- `SelectionSystem_README.md` - Unit-Auswahl-System
- `ControllableSystem_README.md` - Bewegungssystem
- `CombatSystem_README.md` - Kampfsystem
- `WeaponSystem_README.md` - Waffensystem

## Support

Bei Problemen oder Fragen:
1. Prüfe Troubleshooting-Sektion
2. Überprüfe Console auf Fehlermeldungen
3. Siehe verwandte README-Dateien für System-spezifische Infos

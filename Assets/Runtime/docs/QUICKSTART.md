# ?? RTS System - Quick Start Guide

## Schnellstart (5 Minuten Setup)

### Option 1: Automatisches Setup (Empfohlen für Tests)

1. **Setup Helper verwenden:**
   - Erstellen Sie ein leeres GameObject in Ihrer Szene
   - Fügen Sie das `RTSSetupHelper` Script hinzu
   - Im Inspector: Klicken Sie auf "Full Auto Setup" (rechte Maustaste auf Script)
   - Oder klicken Sie im Context Menu (3 Punkte) ? "Full Auto Setup"

2. **Layer konfigurieren:**
   - Edit ? Project Settings ? Tags and Layers
   - Layer 6: "Ground"
   - Layer 7: "Selectable"
   
3. **Layers zuweisen:**
   - Wählen Sie "Ground" GameObject ? Layer: Ground
   - Wählen Sie alle "Test Unit" GameObjects ? Layer: Selectable
   
4. **SelectionManager konfigurieren:**
   - Wählen Sie "SelectionManager" GameObject
   - Im `UnitSelector` Component:
     - Selectable Layer: Nur "Selectable" ?
     - Ground Layer: Nur "Ground" ?

5. **NavMesh backen (Optional, aber empfohlen):**
   - Wählen Sie "Ground" GameObject
   - Im Inspector: Static ? Navigation Static ?
   - Window ? AI ? Navigation (Legacy)
- Bake Tab ? "Bake" Button

6. **Testen:**
   - Play drücken
   - **Linksklick** zum Selektieren
 - **Rechtsklick auf Boden** zum Bewegen
   - **Shift + Linksklick** für Mehrfachauswahl
   - **Ziehen** für Box-Selektion

---

### Option 2: Manuelles Setup (Für Produktion)

#### Schritt 1: Scene Setup

```
Hierarchy:
??? Main Camera (mit RTSCamera Script)
??? SelectionManager (leeres GameObject)
?   ??? UnitSelector (Component)
??? Ground (Plane, Scale: 10, 1, 10)
??? Units
    ??? Unit 1 (Capsule/Custom Model)
    ?   ??? BaseUnit (Component)
    ?   ??? Controllable (Component)
    ?   ??? Collider (für Selektion)
    ?   ??? SelectionIndicator (Child GameObject)
    ??? Unit 2
    ??? ...
```

#### Schritt 2: Unit erstellen

1. **GameObject erstellen:**
   - Create ? 3D Object ? Capsule (oder eigenes Model)
   - Name: "Soldier" (oder beliebig)

2. **Scripts hinzufügen:**
   - Add Component ? BaseUnit
   - Add Component ? Controllable
   - Collider sollte bereits vorhanden sein

3. **BaseUnit konfigurieren:**
   - Unit Name: "Soldier"
   - Is Building: ? (nur für Gebäude)
   - Selected Color: Grün

4. **Controllable konfigurieren:**
   - Move Speed: 5
   - Use NavMesh: ? (empfohlen)
   - Show Path: ? (für Debugging)

5. **Selection Indicator (Optional):**
   - Create ? 3D Object ? Cylinder
   - Name: "SelectionIndicator"
   - Parent: Unit GameObject
   - Position: (0, -0.9, 0)
   - Scale: (1.2, 0.05, 1.2)
   - Material: Grün, Emissive
   - Entfernen Sie den Collider
   - Im BaseUnit: Ziehen Sie dieses GameObject in "Selection Indicator"

#### Schritt 3: Ground/Terrain

1. **Ground erstellen:**
   - Create ? 3D Object ? Plane
   - Name: "Ground"
   - Scale: (10, 1, 10)
   - Layer: Ground

2. **Für NavMesh:**
   - Ground auswählen
   - Static ? Navigation Static ?

#### Schritt 4: SelectionManager

1. **GameObject erstellen:**
   - Create Empty GameObject
   - Name: "SelectionManager"

2. **UnitSelector hinzufügen:**
   - Add Component ? UnitSelector

3. **Konfigurieren:**
   - Selectable Layer: "Selectable"
   - Ground Layer: "Ground"
   - Allow Multi Select: ?
   - Use Formations: ?
   - Formation Spacing: 2

#### Schritt 5: Camera (Optional)

1. **Main Camera auswählen**
2. **RTSCamera Script hinzufügen**
3. **Konfigurieren:**
   - Move Speed: 10
   - Mouse Sensitivity: 3

---

## ?? Steuerung

### Einheiten Selektion:
| Aktion | Beschreibung |
|--------|--------------|
| **Linksklick** | Einheit auswählen |
| **Shift + Linksklick** | Zur Auswahl hinzufügen/entfernen |
| **Maus ziehen** | Box-Selektion (mehrere Einheiten) |
| **ESC** | Alle abwählen |

### Einheiten Bewegung:
| Aktion | Beschreibung |
|--------|--------------|
| **Rechtsklick auf Boden** | Selektierte Einheiten bewegen |
| **Rechtsklick (mehrere Units)** | Formationsbewegung |

### Kamera (RTSCamera):
| Aktion | Beschreibung |
|--------|--------------|
| **W/A/S/D** | Kamera bewegen |
| **Q/E** | Kamera hoch/runter |
| **Shift + WASD** | Schneller bewegen |
| **Rechte Maustaste + Maus** | Free-Look (Kamera drehen) - nur wenn keine Units selektiert (Smart Mode) |
| **Mittlere Maustaste** | Pan (Kamera verschieben) |

**?? Wichtig - Rechtsklick Konflikt gelöst:**
- **Keine Units selektiert** ? Rechtsklick = Kamera Rotation ??
- **Units selektiert** ? Rechtsklick = Units bewegen ??
- Diese "Smart Mode" Logik ist standardmäßig aktiviert!
- Änderbar im RTSCamera Inspector ? `Control Mode`

---

## ? Checkliste

### Basis Setup:
- [ ] Layer "Selectable" erstellt
- [ ] Layer "Ground" erstellt
- [ ] SelectionManager in Szene
- [ ] Ground Plane erstellt und Layer zugewiesen
- [ ] Mindestens eine Unit mit BaseUnit + Controllable
- [ ] Units haben Collider
- [ ] Units haben Layer "Selectable"
- [ ] UnitSelector: Layers konfiguriert

### Optional (aber empfohlen):
- [ ] NavMesh gebacken
- [ ] Selection Indicators erstellt
- [ ] RTSCamera auf Main Camera
- [ ] Move Target Indicator Prefab erstellt
- [ ] Units haben eigene Materials/Farben

---

## ?? Häufige Probleme

### "Units werden nicht selektiert"
**Ursachen:**
- ? Units haben keinen Collider
- ? Units haben falschen Layer (nicht "Selectable")
- ? UnitSelector: `selectableLayer` falsch konfiguriert

**Lösung:**
- Fügen Sie Collider zu Units hinzu
- Setzen Sie Layer auf "Selectable"
- Im UnitSelector: Nur "Selectable" Layer aktivieren

---

### "Units bewegen sich nicht"
**Ursachen:**
- ? Kein Ground mit Collider
- ? Ground hat falschen Layer
- ? UnitSelector: `groundLayer` nicht konfiguriert
- ? NavMesh fehlt (bei useNavMesh = true)

**Lösung:**
- Ground muss Collider haben (Plane hat automatisch MeshCollider)
- Ground Layer auf "Ground" setzen
- Im UnitSelector: "Ground" Layer aktivieren
- NavMesh backen (siehe Anleitung oben)

---

### "NavMeshAgent fehlt"
**Lösung:**
- Wird automatisch hinzugefügt wenn `useNavMesh = true`
- Oder: Manuell hinzufügen: Add Component ? NavMesh Agent

---

### "Units laufen durch Wände"
**Lösung:**
- Wände müssen Navigation Static sein
- NavMesh neu backen
- Agent Radius erhöhen beim Backen

---

### "Box Selection funktioniert nicht"
**Ursachen:**
- ? `allowMultiSelect` ist deaktiviert

**Lösung:**
- Im UnitSelector: `Allow Multi Select` aktivieren ?

---

## ?? Nächste Schritte

### Erweitern Sie das System:

1. **Eigene Unit-Typen:**
   ```csharp
   public class Soldier : Controllable
   {
       // Ihre eigene Logik
   }
   ```

2. **Attack System:**
   - Erkennung von Feinden
   - Attack-Move Command
   - Health System

3. **Gebäude:**
   - BaseUnit mit `isBuilding = true`
   - Produktion von Units
   - Ressourcen-Sammlung

4. **UI:**
   - Unit Info Panel
   - Minimap
   - Command Buttons

5. **Gruppen:**
   - Ctrl+1-9 für Gruppen
   - Doppelklick für "Select All of Type"

---

## ?? Weitere Dokumentation

- `SelectionSystem_README.md` - Details zum Selection System
- `ControllableSystem_README.md` - Details zum Movement System

---

## ?? Fertig!

Sie haben jetzt ein vollständiges RTS Selection & Movement System!

**Test Scenario:**
1. Start Play Mode
2. Klicken Sie auf eine Unit (wird grün)
3. Rechtsklick auf Boden ? Unit bewegt sich!
4. Box-Selektion für mehrere Units ? Alle bewegen sich in Formation!

**Viel Erfolg mit Ihrem RTS Projekt! ??**

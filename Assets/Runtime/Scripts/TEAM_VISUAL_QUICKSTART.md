# Team Visual Indicator - Quick Start Guide

## Schnelleinstieg (5 Minuten)

### Option 1: Automatisches Setup (Empfohlen)

1. **Wählen Sie eine Unit in der Hierarchie aus**

2. **Rechtsklick ? RTS ? Setup As Player Unit**
   - Fügt automatisch TeamComponent hinzu
   - Fügt automatisch TeamVisualIndicator hinzu
   - Setzt Team auf "Player" mit blauer Farbe
   - Konfiguriert Color Ring Indikator

3. **Fertig!** Die Unit zeigt jetzt einen blauen Ring am Boden.

### Option 2: Manuelles Setup

1. **TeamComponent hinzufügen**
   ```
   - Wählen Sie Ihre Unit
   - Add Component ? Team Component
   - Wählen Sie Team: Player, Enemy, Ally oder Neutral
 - Farbe wird automatisch gesetzt (oder anpassen)
   ```

2. **TeamVisualIndicator hinzufügen**
   ```
   - Add Component ? Team Visual Indicator
   - Wählen Sie Indicator Type (z.B. Color Ring)
   - Passen Sie Einstellungen an
   ```

3. **Fertig!**

## Team-Farben

### Standard-Farben (automatisch)
- **Player** (Spieler): Helles Blau
- **Enemy** (Feind): Helles Rot
- **Ally** (Verbündeter): Helles Grün
- **Neutral** (Neutral): Grau

### Farben anpassen
1. Wählen Sie die Unit
2. Gehen Sie zu Team Component
3. Ändern Sie "Team Color"
4. Farbe wird automatisch auf Indikator angewendet

## Indikator-Typen

### 1. Color Ring (Empfohlen für RTS)
- **Beschreibung**: Farbiger Ring am Boden
- **Sichtbarkeit**: Sehr gut
- **Performance**: Ausgezeichnet
- **Best für**: Gruppen von Units, Bodeneinheiten

**Setup:**
```
Indicator Type: Color Ring
Ring Rotation Speed: 30
Pulse Effect: ? (aktiviert)
Pulse Speed: 2
```

### 2. Shield Icon (Gut für Action-Games)
- **Beschreibung**: Icon über der Unit
- **Sichtbarkeit**: Gut
- **Performance**: Gut
- **Best für**: Einzelne Units, dichtes Terrain

**Setup:**
```
Indicator Type: Shield Icon
Billboard Icon: ? (aktiviert)
Indicator Height Offset: 2
```

### 3. Material Tint (Subtil)
- **Beschreibung**: Färbt Unit-Materialien
- **Sichtbarkeit**: Mittel
- **Performance**: Ausgezeichnet
- **Best für**: Kombiniert mit anderen Indikatoren

**Setup:**
```
Indicator Type: Material Tint
Tint Materials: ?
Tint Strength: 0.3
```

### 4. Combined (Maximale Sichtbarkeit)
- **Beschreibung**: Color Ring + Material Tint
- **Sichtbarkeit**: Ausgezeichnet
- **Performance**: Gut
- **Best für**: Wichtige Units, komplexe Szenen

**Setup:**
```
Indicator Type: Combined
(Kombiniert Color Ring und Material Tint Einstellungen)
```

## Schnell-Menü (Rechtsklick)

Wählen Sie eine oder mehrere Units und verwenden Sie:

### Setup-Befehle
- **Setup As Player Unit** - Blaue Markierung (Spieler)
- **Setup As Enemy Unit** - Rote Markierung (Feind)
- **Setup As Ally Unit** - Grüne Markierung (Verbündeter)
- **Setup As Neutral Unit** - Graue Markierung (Neutral)

### Preset-Befehle
- **Apply RTS Preset** - Optimiert für Strategie-Spiele
- **Apply MOBA Preset** - Optimiert für MOBA-Stil
- **Apply Action Preset** - Optimiert für Action-Games

## Häufige Anpassungen

### Größe ändern
```
Team Visual Indicator Component
?? Indicator Scale: 1.0 (Standard)
 - Größer: 1.5-2.0
   - Kleiner: 0.5-0.8
```

### Rotation ändern/ausschalten
```
Color Ring Settings
?? Ring Rotation Speed: 30 (Standard)
   - Schneller: 60-90
   - Langsamer: 10-20
   - Aus: 0
```

### Puls-Effekt anpassen
```
Color Ring Settings
?? Pulse Effect: ?
   ?? Pulse Speed: 2 (Standard)
      - Schneller: 3-5
      - Langsamer: 0.5-1
   ?? Pulse Amount: 0.2 (Standard)
      - Stärker: 0.3-0.5
      - Subtiler: 0.1
```

### Höhe anpassen (für Icons)
```
Visual Indicator Settings
?? Indicator Height Offset: 2 (Standard)
   - Höher: 3-4
   - Niedriger: 1-1.5
```

## Integration mit Production System

Units die von Gebäuden produziert werden:
- Erben automatisch das Team des Gebäudes
- Erhalten automatisch die richtige Team-Farbe
- TeamVisualIndicator wird automatisch hinzugefügt

**Keine zusätzliche Konfiguration nötig!**

## Testen

### Im Editor
1. Starten Sie Play Mode
2. Units zeigen ihre Team-Indikatoren
3. Ändern Sie Team/Farbe im Inspector ? sofortige Aktualisierung

### Zur Laufzeit
```csharp
// Team wechseln
TeamComponent team = unit.GetComponent<TeamComponent>();
team.SetTeam(Team.Enemy);
team.SetTeamColor(Color.red);

// Indikator aktualisieren
TeamVisualIndicator indicator = unit.GetComponent<TeamVisualIndicator>();
indicator.UpdateTeamColor();

// Indikator-Typ ändern
indicator.SetIndicatorType(TeamVisualIndicator.IndicatorType.ShieldIcon);

// Ein-/Ausblenden
indicator.SetIndicatorVisible(false);
```

## Performance-Tipps

### Für viele Units (100+)
```
? Verwenden Sie: Color Ring OHNE Pulse
? Verwenden Sie: Material Tint
? Vermeiden Sie: Rotation bei allen Units
? Vermeiden Sie: Shield Icon für alle
```

### Optimale Einstellungen für große Armeen
```
Indicator Type: Material Tint
Tint Materials: ?
Tint Strength: 0.4
Ring Rotation Speed: 0
Pulse Effect: ? (deaktiviert)
```

### Für wenige Units (1-20)
```
Indicator Type: Combined
Ring Rotation Speed: 30
Pulse Effect: ?
Billboard Icon: ?
```

## Troubleshooting

### Problem: "TeamVisualIndicator requires a TeamComponent"
**Lösung:** Fügen Sie TeamComponent hinzu (Component ? Team Component)

### Problem: Indikator nicht sichtbar
**Lösung 1:** Prüfen Sie "Show Indicator" ist aktiviert
**Lösung 2:** Erhöhen Sie "Indicator Scale" (z.B. 1.5)
**Lösung 3:** Prüfen Sie Team Color ist nicht transparent

### Problem: Ring verschwindet im Boden
**Lösung:** Erhöhen Sie Y-Position in CreateColorRing() von 0.01f auf 0.05f

### Problem: Falsche Farbe
**Lösung:** Drücken Sie "Update Team Color" Button im Inspector (Play Mode)

### Problem: Performance-Einbruch
**Lösung:** Deaktivieren Sie Pulse Effect und Rotation für große Unit-Mengen

## Nächste Schritte

1. **Custom Prefabs erstellen**
   - Eigene Ring-Designs
   - Eigene Icon-Designs
 - Siehe: TEAM_VISUAL_INDICATOR_README.md

2. **Shader anpassen**
   - Outline-Effekte
   - Glow-Effekte
   - Siehe: TEAM_VISUAL_INDICATOR_README.md

3. **Erweiterte Integration**
- Minimap-Integration
   - Selection-System-Integration
   - Combat-System-Integration

## Support

Weitere Informationen:
- **Vollständige Dokumentation**: `TEAM_VISUAL_INDICATOR_README.md`
- **Code-Beispiele**: `TeamVisualIndicatorPresets.cs`
- **Editor-Tools**: `TeamVisualIndicatorEditor.cs`

## Beispiel-Workflow für RTS-Projekt

```
1. Erstellen Sie Player-Hauptgebäude
   ? Rechtsklick ? RTS ? Setup As Player Unit
   ? Indicator Type: Color Ring
   ? Ring Rotation: 30, Pulse: ?

2. Erstellen Sie Enemy-Hauptgebäude
 ? Rechtsklick ? RTS ? Setup As Enemy Unit
   ? Indicator Type: Combined
   ? Ring Rotation: 30, Pulse: ?

3. Produzierte Units erben automatisch:
   ? Team vom Gebäude
   ? Team-Farbe
   ? Visual Indicator

4. Fertig! Spieler können Teams leicht unterscheiden.
```

## Sofort loslegen!

**Schnellster Weg:**
1. Wählen Sie alle Spieler-Units aus
2. Rechtsklick ? RTS ? Setup As Player Unit
3. Wählen Sie alle Feind-Units aus
4. Rechtsklick ? RTS ? Setup As Enemy Unit
5. **Fertig!** Testen Sie im Play Mode.

**Zeit: ~2 Minuten für komplette Team-Visualisierung!**

# ProductionPanel - Verhalten und Interaktion

## Panel Öffnen/Schließen Logik

### ? Panel öffnet sich:
- ? Beim Anklicken einer BaseUnit **mit** ProductionComponent
- ? Beim Umschalten zwischen verschiedenen Produktionsgebäuden
  - Panel wird automatisch auf das neue Gebäude umgeschaltet

### ? Panel bleibt offen:
- ? Wenn eine Produktion gestartet wird (Produkt angeklickt)
- ? Wenn die BaseUnit deselektiert wird (z.B. Klick auf Boden)
- ? Wenn auf andere Orte in der Szene geklickt wird
- ? Während Einheiten produziert werden

### ? Panel schließt sich:
- ? Nur beim Klick auf den **Close-Button**
- ? Beim Anklicken einer Einheit **ohne** ProductionComponent (z.B. Soldat)

## RTS-typisches Verhalten

Dieses Verhalten ist typisch für RTS-Spiele wie:
- **StarCraft II**: Panel bleibt offen, auch wenn Gebäude deselektiert wird
- **Age of Empires**: Panel bleibt offen für schnelle Produktionssteuerung
- **Command & Conquer**: Panel bleibt offen bis manuell geschlossen

### Vorteile:
? **Schnellere Produktion**: Spieler kann mehrere Einheiten hintereinander produzieren
? **Multitasking**: Spieler kann andere Dinge tun, während Panel offen bleibt
? **Übersicht**: Queue bleibt sichtbar, auch wenn Gebäude nicht selektiert ist
? **Weniger Klicks**: Kein ständiges Wiederöffnen des Panels nötig

## Workflow-Beispiele

### Beispiel 1: Einheiten produzieren
```
1. Klick auf Kaserne ? Panel öffnet sich
2. Klick auf "Soldat" ? Soldat wird zur Queue hinzugefügt
3. Klick auf "Soldat" ? Weiterer Soldat zur Queue
4. Klick auf "Bogenschütze" ? Bogenschütze zur Queue
5. Klick auf Boden (Kamera bewegen) ? Panel bleibt offen!
6. Weiter Einheiten hinzufügen möglich
7. Klick auf "Close" ? Panel schließt sich
```

### Beispiel 2: Zwischen Gebäuden wechseln
```
1. Klick auf Kaserne ? Panel zeigt Kaserne-Produkte
2. Klick auf Schmiede ? Panel wechselt zu Schmiede-Produkten
3. Panel bleibt geöffnet, zeigt aber neue Produkte
```

### Beispiel 3: Einheit auswählen
```
1. Klick auf Kaserne ? Panel öffnet sich
2. Produktion läuft
3. Klick auf Soldat (ohne ProductionComponent) ? Panel schließt sich
4. Spieler will jetzt die Einheit steuern, Panel nicht mehr nötig
```

## Code-Änderungen

### BaseUnit.OnDeselected()
**Vorher:**
```csharp
protected virtual void OnDeselected()
{
    Debug.Log($"{unitName} deselected");
    
    // Schloss das Panel automatisch
    if (productionComponent != null)
    {
        ProductionUIManager.Instance.HideProductionPanel();
    }
}
```

**Nachher:**
```csharp
protected virtual void OnDeselected()
{
    Debug.Log($"{unitName} deselected");
 
    // Panel bleibt offen - RTS-typisches Verhalten
    // Schließt sich nur via Close-Button oder bei Auswahl einer Nicht-Produktions-Einheit
}
```

### BaseUnit.OnSelected()
**Neu hinzugefügt:**
```csharp
protected virtual void OnSelected()
{
    Debug.Log($"{unitName} selected");

    if (productionComponent != null)
  {
        // Öffnet oder wechselt das Panel
        ProductionUIManager.Instance.ShowProductionPanel(this);
    }
    else
    {
        // Einheit ohne Produktion ? Panel schließen
        ProductionUIManager.Instance.HideProductionPanel();
    }
}
```

## Alternative Konfiguration

Falls du das **alte Verhalten** (Panel schließt bei Deselection) möchtest, kannst du eine Option hinzufügen:

```csharp
[Header("Production Panel Behavior")]
[SerializeField] private bool closeOnDeselect = false; // false = RTS-Style

protected virtual void OnDeselected()
{
    if (closeOnDeselect && productionComponent != null)
    {
      ProductionUIManager.Instance.HideProductionPanel();
}
}
```

## Zusammenfassung

? **Panel öffnet sich** beim Anklicken einer Produktionseinheit
? **Panel bleibt offen** auch wenn Einheit deselektiert wird
? **Panel wechselt** beim Anklicken anderer Produktionseinheiten
? **Panel schließt sich** nur bei:
- Close-Button
   - Auswahl einer Nicht-Produktions-Einheit
   
Dies ermöglicht flüssiges RTS-Gameplay! ??

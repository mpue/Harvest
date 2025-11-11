# Combat System - QUICKSTART ?

## ?? 5-Minuten Setup

### Step 1: Health zu Unit hinzufügen (30 Sekunden)

1. **Wähle deine Unit** im Hierarchy
2. **Add Component** ? `Health`
3. **Fertig!** Standard-Werte sind bereits konfiguriert

### Step 2: HealthBar hinzufügen (30 Sekunden)

1. **Selbe Unit** noch ausgewählt
2. **Add Component** ? `HealthBarSetup`
3. **Check** `Auto Setup` ??
4. **Fertig!** HealthBar wird automatisch erstellt

### Step 3: Audio hinzufügen (1 Minute)

1. **Health Component** im Inspector
2. **Hit Sounds:**
   - Setze `Size` auf `3`
   - Ziehe 3 verschiedene Hit-Sounds rein
3. **Death Sounds:**
   - Setze `Size` auf `2`
   - Ziehe 2 verschiedene Death-Sounds rein
4. **Fertig!**

### Step 4: Explosion hinzufügen (2 Minuten)

**Option A: Verwende Prefab (empfohlen)**
1. Erstelle Explosion Prefab (siehe unten)
2. Ziehe es in `Explosion Prefab` Field
3. Fertig!

**Option B: Ohne Explosion**
- Lasse `Explosion Prefab` leer
- Unit verschwindet einfach

### Step 5: Teste es! (1 Minute)

1. **Play** drücken
2. In Console eingeben:
   ```csharp
   GameObject.Find("YourUnit").GetComponent<Health>().TakeDamage(50);
   ```
3. Oder: Schieße mit Waffe auf Unit
4. **Beobachte:**
   - ? HealthBar zeigt Schaden
   - ? Hit Sound spielt ab
   - ? Hit Flash Effekt
   - ? Bei 0 HP: Death Sound + Explosion

---

## ?? Explosion Prefab erstellen (3 Minuten)

### Quick Version:

1. **Create Empty** ? Name: `Explosion`

2. **Add Particle System:**
   ```
   Add Component ? Particle System
   ```
   - Start Lifetime: `1`
   - Start Speed: `5`
   - Start Size: `0.5`
   - Start Color: Orange/Red
   - Emission Rate: `50`
   - Shape: Sphere

3. **Add Light:**
   ```
   Add Component ? Light
 ```
   - Type: Point
   - Color: Orange
   - Intensity: `8`
   - Range: `10`

4. **Add Explosion Component:**
 ```
   Add Component ? Explosion
   ```
   - Deals Damage: ??
   - Damage Amount: `50`
   - Damage Radius: `5`
   - Particle Systems: Ziehe Particle System rein
   - Explosion Light: Ziehe Light rein
   - Explosion Sound: Ziehe Sound rein

5. **Save as Prefab** ? Fertig!

---

## ?? Test-Szene Setup (2 Minuten)

```
1. Erstelle 2 Units:
   - Tank (Team 1)
   - Enemy Tank (Team 2)

2. Beide Units:
   - Add BaseUnit
   - Add TeamComponent (verschiedene Teams!)
   - Add Health
   - Add HealthBarSetup
   - Add WeaponController
- Add Weapon als Child

3. Play und beobachte den Kampf!
```

---

## ?? Nur das Minimum

**Absolut minimales Setup:**

```
Unit (GameObject)
??? BaseUnit
??? TeamComponent
??? Health  ? Nur diese 3 Components!
```

**Das reicht für:**
- ? Damage nehmen
- ? Sterben
- ? Events

**HealthBar ist optional!**
**Explosion ist optional!**
**Audio ist optional!**

---

## ?? Quick Tips

### Schnelle Werte:

**Light Unit (Scout):**
- Max Health: `50`
- Regeneration Rate: `10`

**Medium Unit (Infantry):**
- Max Health: `100`
- Regeneration Rate: `5`

**Heavy Unit (Tank):**
- Max Health: `200`
- Regeneration Rate: `2`

**Building:**
- Max Health: `500`
- Regeneration: `false`

### Quick Testing:

**In Unity Console während Play Mode:**
```csharp
// Unit finden
var unit = GameObject.Find("Tank").GetComponent<Health>();

// Schaden
unit.TakeDamage(25);

// Heilen
unit.Heal(50);

// Töten
unit.Kill();

// Status
Debug.Log($"HP: {unit.CurrentHealth}/{unit.MaxHealth}");
```

---

## ?? Häufige Fehler

### "Health Component not found"
**Lösung:** Health Component zur Unit hinzufügen

### "HealthBar zeigt nicht"
**Lösung:** Auto Setup aktivieren in HealthBarSetup

### "Kein Schaden"
**Lösung:** 
- TeamComponent auf Attacker UND Target
- Verschiedene Team IDs!

### "Explosion spawnt nicht"
**Lösung:**
- Explosion Prefab zuweisen
- Oder einfach leer lassen (optional!)

---

## ?? Next Steps

Nach dem Basic Setup:

1. **Tune Values** - Passe Health/Damage an
2. **Add Sounds** - Mehr Audio-Variety
3. **Create Explosions** - Verschiedene Typen
4. **Test Balance** - Kämpfe beobachten
5. **Polish** - VFX verbessern

---

## ?? Mehr Info

Für Details siehe: `CombatSystem_README.md`

---

**?? Los geht's! In 5 Minuten kampfbereit!**

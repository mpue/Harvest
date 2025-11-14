# Combat System - Complete Guide

## ?? Overview

Das Combat System fügt vollständiges Health-Management, visuelle Healthbars, Explosionen und Audio-Feedback hinzu.

## ?? Components

### 1. **Health Component**
Verwaltet die Gesundheit von Units und Buildings.

#### Features:
- ? Health Management (Current/Max)
- ? Damage & Healing
- ? Auto-Regeneration
- ? Death Handling
- ? Audio Feedback (Hit/Death Sounds)
- ? Visual Feedback (Hit Flash)
- ? Explosion on Death
- ? UnityEvents für Custom Logic

#### Inspector Settings:

**Health Settings:**
- `Max Health` - Maximale Gesundheit
- `Can Regenerate` - Automatische Heilung aktivieren
- `Regeneration Rate` - HP pro Sekunde
- `Regeneration Delay` - Wartezeit nach Schaden

**Death Settings:**
- `Destroy On Death` - Unit nach Tod zerstören
- `Destroy Delay` - Verzögerung vor Zerstörung
- `Explosion Prefab` - Explosions-Effekt
- `Explosion Scale` - Größe der Explosion

**Audio:**
- `Hit Sounds[]` - Zufällige Sounds bei Schaden
- `Death Sounds[]` - Zufällige Sounds bei Tod
- `Audio Volume` - Lautstärke

**Visual Feedback:**
- `Flash On Hit` - Rote Aufblitz-Effekt
- `Hit Flash Color` - Farbe des Flash-Effekts
- `Hit Flash Duration` - Dauer des Effekts

#### Public Methods:

```csharp
// Schaden zufügen
health.TakeDamage(50f, attackerTeam);

// Heilen
health.Heal(25f);

// Gesundheit setzen
health.SetHealth(75f);

// Sofort töten
health.Kill(killerTeam);

// Properties
float currentHP = health.CurrentHealth;
float maxHP = health.MaxHealth;
float percent = health.HealthPercentage; // 0-1
bool dead = health.IsDead;
bool full = health.IsFullHealth;
```

#### Events:

```csharp
// Health verändert (current, max)
health.OnHealthChanged.AddListener((current, max) => {
    Debug.Log($"Health: {current}/{max}");
});

// Schaden erhalten (damage amount)
health.OnDamaged.AddListener((damage) => {
    Debug.Log($"Took {damage} damage!");
});

// Tod
health.OnDeath.AddListener(() => {
    Debug.Log("Unit died!");
});

// Voll geheilt
health.OnFullyHealed.AddListener(() => {
    Debug.Log("Fully healed!");
});
```

---

### 2. **HealthBar Component**
World-Space UI Health Bar die der Unit folgt.

#### Features:
- ? Folgt der Unit automatisch
- ? Schaut immer zur Kamera
- ? Farbwechsel basierend auf Health
- ? Smooth Transitions
- ? Auto-Hide bei voller Health
- ? Reagiert auf Health Events

#### Inspector Settings:

**References:**
- `Health Component` - Auto-gefunden wenn leer
- `Fill Image` - UI Image für Health-Anzeige
- `Background Image` - Hintergrund
- `Canvas` - World-Space Canvas

**Display Settings:**
- `Offset` - Position über der Unit
- `Hide When Full` - Bei voller Health verstecken
- `Always Face Camera` - Zur Kamera ausrichten
- `Hidden Health Threshold` - Ab welchem % verstecken

**Colors:**
- `Full Health Color` - Grün (100%)
- `Mid Health Color` - Gelb (50%)
- `Low Health Color` - Rot (<25%)
- `Mid Health Threshold` - Schwellenwert für Gelb
- `Low Health Threshold` - Schwellenwert für Rot

**Animation:**
- `Smooth Transition` - Weiche Übergänge
- `Transition Speed` - Geschwindigkeit

---

### 3. **HealthBarSetup Component**
Automatisches Setup von HealthBars.

#### Features:
- ? Auto-Create HealthBar beim Start
- ? Verwendung von Prefabs oder manuelle Erstellung
- ? Automatische Konfiguration

#### Inspector Settings:

**Auto Setup:**
- `Auto Setup` - Automatisch beim Start erstellen
- `Health Bar Prefab` - Optional: Verwende Prefab

**Health Bar Settings:**
- `Health Bar Offset` - Position über Unit
- `Health Bar Size` - Größe
- `Hide When Full` - Verstecken bei voller Health

**Colors:**
- `Full/Mid/Low Health Color` - Farbeinstellungen

---

### 4. **Explosion Component**
Explosions-Effekt mit Schaden und Physik.

#### Features:
- ? Area Damage (Radius-basiert)
- ? Damage Falloff (Curve)
- ? Physics Force
- ? Visual Effects (Particles, Light)
- ? Audio
- ? Team-Aware (keine Friendly Fire)

#### Inspector Settings:

**Damage Settings:**
- `Deals Damage` - Schaden aktivieren
- `Damage Amount` - Basis-Schaden
- `Damage Radius` - Schadens-Radius
- `Damage Layer Mask` - Welche Layer treffen
- `Damage Falloff` - AnimationCurve für Abnahme

**Force Settings:**
- `Apply Force` - Physik-Kraft aktivieren
- `Explosion Force` - Kraft-Stärke
- `Upwards Modifier` - Nach-oben-Komponente

**Effects:**
- `Particle Systems[]` - Partikel-Effekte
- `Explosion Light` - Licht-Effekt
- `Light Fade Duration` - Licht ausblenden

**Audio:**
- `Explosion Sound` - Sound-Effekt
- `Audio Volume` - Lautstärke

**Lifetime:**
- `Lifetime` - Auto-Destroy nach X Sekunden

#### Usage:

```csharp
// Spawne Explosion
GameObject explosionObj = Instantiate(explosionPrefab, position, Quaternion.identity);
Explosion explosion = explosionObj.GetComponent<Explosion>();
explosion.Initialize(ownerTeam); // Optional: Team setzen
```

---

## ?? Quick Setup

### Setup 1: Basic Unit with Health

1. **Füge Health Component hinzu:**
   ```
   GameObject ? Add Component ? Health
   ```

2. **Konfiguriere Health Settings:**
   - Max Health: `100`
   - Can Regenerate: `true` (optional)
- Regeneration Rate: `5` HP/sec
   - Regeneration Delay: `3` seconds

3. **Audio hinzufügen:**
   - Hit Sounds: Ziehe mehrere AudioClips in Array
   - Death Sounds: Ziehe mehrere AudioClips in Array

4. **Explosion (optional):**
   - Explosion Prefab: Ziehe Explosions-Prefab rein
   - Explosion Scale: `1.0`

### Setup 2: Add HealthBar

**Option A: Automatic Setup**
```
Unit GameObject ? Add Component ? HealthBarSetup
```
- `Auto Setup` ??
- Fertig! HealthBar wird automatisch erstellt

**Option B: Manual Prefab Setup**
```
1. Erstelle HealthBar Prefab:
   - GameObject ? UI ? Canvas (World Space)
   - Add HealthBar Component
   - Configure Fill Image
   
2. Füge zu Unit hinzu:
   - Add Component ? HealthBarSetup
   - Health Bar Prefab: Ziehe Prefab rein
   - Auto Setup ??
```

### Setup 3: Create Explosion Prefab

1. **Erstelle GameObject:**
   ```
   Create Empty ? Name: "Explosion"
   ```

2. **Add Components:**
   - Add Component ? Explosion
   - Add Component ? Particle System(s)
   - Add Component ? Light

3. **Configure Explosion:**
 - Deals Damage: ??
   - Damage Amount: `50`
   - Damage Radius: `5`
   - Damage Falloff: Adjust Curve (1 at center, 0 at edge)

4. **Add Effects:**
   - Particle Systems: Drag particle systems into array
   - Explosion Light: Drag Light component
   - Explosion Sound: Drag AudioClip

5. **Save as Prefab**

---

## ?? Integration mit Weapons

Das Projectile-Script wurde bereits aktualisiert und wendet automatisch Schaden an:

```csharp
// In Projectile.cs - bereits implementiert
void OnHitTarget(BaseUnit target, Vector3 hitPoint)
{
    Health healthComponent = target.GetComponent<Health>();
    if (healthComponent != null)
    {
   healthComponent.TakeDamage(damage, ownerTeam);
    }
}
```

**Kein zusätzlicher Code nötig!** ?

---

## ?? Complete Unit Setup Checklist

- [ ] **BaseUnit Component** - Basis für alle Units
- [ ] **TeamComponent** - Team-Zugehörigkeit
- [ ] **Health Component** - Health Management
  - [ ] Hit Sounds konfiguriert
  - [ ] Death Sounds konfiguriert
  - [ ] Explosion Prefab zugewiesen (optional)
- [ ] **HealthBarSetup** - Automatische HealthBar
  - [ ] Auto Setup aktiviert
- [ ] **Collider** - Für Treffer-Erkennung
- [ ] **Rigidbody** (optional) - Für Physik

### For Controllable Units zusätzlich:
- [ ] **Controllable Component** - Bewegung
- [ ] **NavMeshAgent** - Pathfinding

### For Combat Units zusätzlich:
- [ ] **WeaponController** - Waffen-System
- [ ] **Weapon(s)** - Als Child-Objects

---

## ?? Beispiel: Complete Tank Setup

```
Tank (GameObject)
??? BaseUnit Component
?   ??? Unit Name: "Heavy Tank"
?   ??? Is Building: ?
??? TeamComponent
?   ??? Team ID: 1
?   ??? Team Name: "Blue Team"
??? Health Component
?   ??? Max Health: 200
?   ??? Can Regenerate: ?
?   ??? Regeneration Rate: 10
?   ??? Hit Sounds: [TankHit1, TankHit2, TankHit3]
?   ??? Death Sounds: [TankExplosion1, TankExplosion2]
?   ??? Explosion Prefab: TankExplosion
??? HealthBarSetup Component
?   ??? Auto Setup: ?
?   ??? Offset: (0, 3, 0)
??? Controllable Component
?   ??? Move Speed: 5
??? WeaponController Component
?   ??? Auto Target: ?
??? NavMeshAgent Component
```

---

## ?? Advanced Usage

### Custom Death Logic

```csharp
public class CustomUnit : MonoBehaviour
{
    void Start()
    {
        Health health = GetComponent<Health>();
        health.OnDeath.AddListener(CustomDeathLogic);
    }

    void CustomDeathLogic()
    {
        // Drop items
        DropLoot();
        
        // Update score
        GameManager.Instance.AddScore(100);
        
  // Play custom animation
        GetComponent<Animator>().SetTrigger("Die");
    }
}
```

### Damage Types

```csharp
// Erweitere Health Component für Damage Types
public enum DamageType { Normal, Fire, Ice, Explosive }

public void TakeDamage(float damage, DamageType type)
{
    float finalDamage = damage;
    
    // Apply resistances
    switch(type)
    {
        case DamageType.Fire:
            finalDamage *= fireResistance;
            break;
        case DamageType.Ice:
      finalDamage *= iceResistance;
       break;
    }
    
    TakeDamage(finalDamage);
}
```

### Armor System

```csharp
// Füge Armor-Berechnung hinzu
public float armor = 10f;

public void TakeDamage(float damage)
{
    float damageReduction = armor / (armor + 100f);
    float finalDamage = damage * (1f - damageReduction);
    
    currentHealth -= finalDamage;
  // ... rest of damage logic
}
```

---

## ?? Troubleshooting

### HealthBar zeigt nicht:
- ? Canvas auf `World Space` gesetzt?
- ? Fill Image zugewiesen?
- ? Health Component vorhanden?
- ? Kamera gefunden? (Camera.main)

### Kein Schaden:
- ? Health Component auf Target?
- ? TeamComponent konfiguriert?
- ? Teams sind Enemies?
- ? Collider auf Unit?

### Explosion trifft nicht:
- ? Damage Layer Mask korrekt?
- ? Damage Radius groß genug?
- ? Units haben Collider?
- ? Team konfiguriert? (für Friendly Fire Check)

### Kein Audio:
- ? AudioClips zugewiesen?
- ? Audio Volume > 0?
- ? AudioListener in Szene?
- ? Audio Import Settings korrekt?

---

## ?? Related Systems

- **Weapon System** - `WeaponSystem_README.md`
- **Selection System** - `SelectionSystem_README.md`
- **Controllable System** - `ControllableSystem_README.md`

---

## ?? Best Practices

1. **Performance:**
   - Verwende Object Pooling für Projectiles und Explosionen
   - Limitiere Particle Systems (Max Particles)
   - Verwende LOD für HealthBars bei vielen Units

2. **Balancing:**
   - Teste Damage/Health Verhältnisse
   - Nutze AnimationCurves für Damage Falloff
   - Tweake Regeneration Rates

3. **Audio:**
   - Verwende mehrere Sound-Varianten (Array)
   - Setze Audio Volume angemessen
   - 3D Sound für räumliches Audio

4. **Visual Feedback:**
   - Hit Flash sollte kurz sein (0.1s)
   - HealthBar Farben klar unterscheidbar
   - Explosionen nicht zu groß/aufdringlich

---

**?? Combat System Ready!**

Das System ist vollständig integriert mit:
- ? Weapon System
- ? Projectile System
- ? Team System
- ? Selection System
- ? Controllable System

Viel Erfolg beim Game Development! ??

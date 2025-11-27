# Weapon Animation - Quick Reference

## Schnellstart (5 Minuten)

### 1. Komponenten Setup
```
Unit GameObject
??? Animator ?
??? Animatable ?
??? WeaponController ?
??? Weapon ?
```

### 2. Animator Parameter
```
Parameters:
- IsAiming (bool)
- Fire (trigger)
```

### 3. Weapon Inspector
```
Animation:
  ? Use Animation
  Aim Parameter Name: IsAiming
  Fire Trigger Name: Fire
  ? Use Bool For Aim
  ? Use Trigger For Fire
```

### 4. Fertig!
Animation läuft automatisch:
- Ziel erfasst ? IsAiming = true
- Schuss ? Fire trigger
- Ziel verloren ? IsAiming = false

## Animator States

### Minimum Setup
```
[Idle] ?? [Aiming]
  ? IsAiming
  
[Any State] ? [Firing]
  ? Fire trigger
```

### Mit Movement
```
[Movement Blend] ?? [Aiming Blend]
  ? IsAiming        ? IsAiming
  
[Any State] ? [Firing]
  ? Fire trigger
```

## Code-Beispiele

### Automatisch (kein Code nötig)
```csharp
// Weapon kümmert sich um alles:
weapon.SetTarget(enemy);    // ? IsAiming = true
weapon.TryFire();       // ? Fire trigger
weapon.ClearTarget();       // ? IsAiming = false
```

### Manuell
```csharp
// Manuelle Kontrolle:
weapon.SetAimAnimation(true);
weapon.TriggerFireAnimation();
weapon.SetUseAnimation(false);
```

### Custom Verhalten
```csharp
Animatable anim = weapon.GetAnimatable();
anim.SetTrigger("SpecialAttack");
```

## Häufige Probleme

### Animation spielt nicht
- [ ] Animatable auf **Unit** (Parent), nicht auf Weapon
- [ ] Parameter "IsAiming" existiert
- [ ] Weapon > Use Animation ?
- [ ] Transitions konfiguriert

### Fire triggert nicht
- [ ] Parameter "Fire" (trigger) existiert
- [ ] Transition von Any State ? Firing
- [ ] Exit Time auf Firing State

## Inspector Checkliste

**Weapon Component:**
- [x] Animation > Use Animation
- [x] Aim Parameter Name: "IsAiming"
- [x] Fire Trigger Name: "Fire"
- [x] Use Bool For Aim
- [x] Use Trigger For Fire

**Animatable Component (Parent):**
- [x] Vorhanden auf Unit GameObject
- [x] Animator zugewiesen
- [x] Parameter gecacht

**Animator:**
- [x] Controller zugewiesen
- [x] Parameter "IsAiming" (bool)
- [x] Parameter "Fire" (trigger)
- [x] States: Idle, Aiming, Firing
- [x] Transitions konfiguriert

## Console Logs

### ? Erfolg:
```
? Weapon 'MainGun': Animation support enabled (Aim: IsAiming, Fire: Fire)
?? SoldierUnit: Animation state changed - IsAiming: true
?? MainGun FIRED at Enemy!
```

### ? Problem:
```
?? Weapon 'MainGun': Animation enabled but no Animatable component found on parent
```
? Animatable muss auf Parent GameObject sein!

## Best Practices

1. **Animatable auf Unit** - nicht auf Weapon
2. **Kurze Fire Animationen** - 0.2-0.5s
3. **Bool für Aim** - persistenter State
4. **Trigger für Fire** - einmaliges Event
5. **Exit Time auf Fire State** - automatische Rückkehr

## Siehe auch

- **Vollständige Doku:** `WEAPON_ANIMATION_GUIDE.md`
- **Animation System:** `ANIMATION_SYSTEM_GUIDE.md`
- **Beispiel:** `Examples/AnimatedCombatUnit.cs`

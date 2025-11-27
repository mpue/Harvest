using UnityEngine;

/// <summary>
/// Example script showing how to use the Animatable component for combat units
/// Extends standard behavior with attack and death animations
/// Integrates with Weapon animation system for Aim and Fire states
/// </summary>
[RequireComponent(typeof(Animatable))]
[RequireComponent(typeof(Controllable))]
public class AnimatedCombatUnit : MonoBehaviour
{
    [Header("Animation Triggers")]
    [SerializeField] private string attackTrigger = "Attack";
    [SerializeField] private string dieTrigger = "Die";
  [SerializeField] private string hitTrigger = "Hit";
    
    [Header("Animation States")]
    [SerializeField] private string attackBoolParameter = "IsAttacking";

[Header("Settings")]
    [SerializeField] private bool useAttackBool = false; // Use bool instead of trigger
    [SerializeField] private float attackAnimationDuration = 1f;
    
    [Header("Debug")]
    [SerializeField] private bool debugLogging = false;
    
    private Animatable animatable;
    private Controllable controllable;
    private WeaponController weaponController;
 private Weapon weapon;
    private bool isDead = false;
    private bool isAttacking = false;
    
    void Awake()
    {
   animatable = GetComponent<Animatable>();
        controllable = GetComponent<Controllable>();
        weaponController = GetComponent<WeaponController>();
        weapon = GetComponentInChildren<Weapon>();
 }
    
    void Start()
    {
        // Log setup status
        if (debugLogging)
        {
    Debug.Log($"? AnimatedCombatUnit on {gameObject.name} initialized");
     Debug.Log($"  - Animatable: {animatable != null}");
            Debug.Log($"  - WeaponController: {weaponController != null}");
   Debug.Log($"  - Weapon: {weapon != null}");
     
if (weapon != null)
     {
        Debug.Log($"  - Weapon Animation: Aim={weapon.GetType().GetField("aimParameterName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(weapon)}, Fire={weapon.GetType().GetField("fireTriggerName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(weapon)}");
   }
        }
    }
    
    void Update()
    {
  // Weapon animation (IsAiming, Fire) is handled automatically by Weapon component
        // WeaponController manages target acquisition
    
   // Update attack state based on weapon controller
 if (weaponController != null && animatable != null && !isDead)
   {
    bool hasTarget = weaponController.HasTarget;
      
  if (useAttackBool && hasTarget != isAttacking)
    {
        isAttacking = hasTarget;
      animatable.SetBool(attackBoolParameter, isAttacking);
                
   if (debugLogging)
    {
          Debug.Log($"?? {gameObject.name}: IsAttacking = {isAttacking}");
    }
  }
        }
    }

  /// <summary>
    /// Play attack animation
    /// Note: Fire animation is handled automatically by Weapon component
 /// </summary>
 public void PlayAttackAnimation()
    {
        if (animatable == null || isDead) return;
   
      if (useAttackBool)
  {
     animatable.SetBool(attackBoolParameter, true);
      
        // Auto-reset after duration
  if (attackAnimationDuration > 0)
 {
            Invoke(nameof(ResetAttackAnimation), attackAnimationDuration);
            }
        }
        else
      {
 animatable.SetTrigger(attackTrigger);
        }
      
 if (debugLogging)
        {
     Debug.Log($"?? {gameObject.name}: Playing attack animation");
        }
    }

    /// <summary>
    /// Play hit/damage animation
    /// </summary>
    public void PlayHitAnimation()
    {
   if (animatable == null || isDead) return;
        
  animatable.SetTrigger(hitTrigger);

        if (debugLogging)
   {
      Debug.Log($"?? {gameObject.name}: Playing hit animation");
        }
    }
    
    /// <summary>
    /// Play death animation
 /// </summary>
    public void PlayDeathAnimation()
    {
if (animatable == null || isDead) return;
        
     isDead = true;
        
   // Stop movement
     if (controllable != null)
        {
    controllable.Stop();
controllable.SetUseAnimation(false); // Disable auto-animation
      }
        
// Disable weapon animations
        if (weapon != null)
        {
        weapon.SetUseAnimation(false);
        }
     
        // Play death animation
        animatable.SetTrigger(dieTrigger);
   
 Debug.Log($"?? {gameObject.name}: Playing death animation");
    }
    
    /// <summary>
    /// Reset attack animation (called automatically if using bool)
  /// </summary>
    private void ResetAttackAnimation()
    {
    if (animatable != null && useAttackBool)
     {
     animatable.SetBool(attackBoolParameter, false);
 }
    }
    
    /// <summary>
    /// Enable/disable all animations
    /// </summary>
    public void SetAnimationsEnabled(bool enabled)
    {
 if (animatable != null)
        {
   animatable.enabled = enabled;
        }
    
        if (controllable != null)
        {
controllable.SetUseAnimation(enabled);
        }
  
        if (weapon != null)
        {
   weapon.SetUseAnimation(enabled);
        }
        
        if (debugLogging)
     {
       Debug.Log($"?? {gameObject.name}: Animations {(enabled ? "enabled" : "disabled")}");
        }
    }
    
    /// <summary>
    /// Get current animation states for debugging
    /// </summary>
    public string GetAnimationDebugInfo()
    {
        if (animatable == null) return "No Animatable";
        
 string info = $"AnimatedCombatUnit '{gameObject.name}':\n";
        info += $"  IsMoving: {controllable?.IsMoving ?? false}\n";
        info += $"  IsAttacking: {isAttacking}\n";
        
        if (weaponController != null)
        {
       info += $"  HasTarget: {weaponController.HasTarget}\n";
            info += $"  Target: {weaponController.CurrentTarget?.name ?? "None"}\n";
        }
        
        if (weapon != null)
      {
         info += $"  IsAiming: {weapon.CurrentTarget != null}\n";
        info += $"  IsAimed: {weapon.IsAimed}\n";
        }
        
        info += $"  IsDead: {isDead}\n";
        
  return info;
    }
}

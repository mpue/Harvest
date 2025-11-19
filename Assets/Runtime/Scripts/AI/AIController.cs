using UnityEngine;

/// <summary>
/// OLD AI Controller - DEPRECATED!
/// Use AIControllerModular instead!
/// </summary>
[System.Obsolete("Use AIControllerModular - this is deprecated!")]
public class AIController : MonoBehaviour
{
    void Start()
    {
     Debug.LogError("⚠️ AIController is DEPRECATED!");
        Debug.LogError("Remove this component and use AIControllerModular instead!");
        enabled = false;
    }
}

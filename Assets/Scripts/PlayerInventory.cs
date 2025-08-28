using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory")]
    public bool hasPills = false;
    public bool hasFlashlight = false;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    
    void Start()
    {
        if (showDebugInfo)
        {
            Debug.Log("Player Inventory initialized");
        }
    }
    
    // Add pills to inventory
    public void AddPills()
    {
        hasPills = true;
        if (showDebugInfo)
        {
            Debug.Log("Pills added to inventory");
        }
    }
    
    // Remove pills from inventory
    public void RemovePills()
    {
        hasPills = false;
        if (showDebugInfo)
        {
            Debug.Log("Pills removed from inventory");
        }
    }
    
    // Check if player has pills
    public bool HasPills()
    {
        return hasPills;
    }
    
    // Add flashlight to inventory
    public void AddFlashlight()
    {
        hasFlashlight = true;
        if (showDebugInfo)
        {

        }
    }
    
    // Remove flashlight from inventory
    public void RemoveFlashlight()
    {
        hasFlashlight = false;
        if (showDebugInfo)
        {

        }
    }
    
    // Check if player has flashlight
    public bool HasFlashlight()
    {
        return hasFlashlight;
    }
    
    // Get inventory status for UI
    public string GetInventoryStatus()
    {
        string status = "";
        if (hasPills) status += "Pills ";
        if (hasFlashlight) status += "Flashlight ";
        return status.Length > 0 ? status.Trim() : "Empty";
    }
}

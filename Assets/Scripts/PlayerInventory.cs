using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory")]
    public bool hasPills = false;
    
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
    
    // Get inventory status for UI
    public string GetInventoryStatus()
    {
        return hasPills ? "Has Pills" : "No Pills";
    }
}

using UnityEngine;

public class PillPickup : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionRange = 2f;
    public Transform playerTransform;
    public KeyCode interactionKey = KeyCode.E;
    
    [Header("Visual Feedback")]
    public GameObject visualObject; // The pill cube
    
    [Header("Debug")]
    public bool showGizmos = true;
    
    private bool playerInRange = false;
    private bool isPicked = false;
    
    void Start()
    {
        // Find player if not assigned
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                Debug.Log("PillPickup: Automatically found player with 'Player' tag");
            }
            else
            {
                Debug.LogWarning("PillPickup: No player found! Make sure your player GameObject has the 'Player' tag");
            }
        }
        
        // Use this GameObject as visual if not specified
        if (visualObject == null)
        {
            visualObject = gameObject;
        }
    }
    
    void Update()
    {
        if (isPicked) return;
        
        CheckPlayerDistance();
        HandleKeyboardInput();
    }
    
    void CheckPlayerDistance()
    {
        if (playerTransform == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        bool wasInRange = playerInRange;
        playerInRange = distanceToPlayer <= interactionRange;
        
        if (playerInRange && !wasInRange)
        {
            Debug.Log("Press E to pick up pills");
        }
    }
    
    void HandleKeyboardInput()
    {
        if (playerInRange && Input.GetKeyDown(interactionKey))
        {
            PickupPills();
        }
    }
    
    void PickupPills()
    {
        if (isPicked) return;
        
        isPicked = true;
        
        // Add pills to player inventory
        PlayerInventory inventory = FindFirstObjectByType<PlayerInventory>();
        if (inventory != null)
        {
            inventory.AddPills();
        }
        
        // Hide the visual object
        if (visualObject != null)
        {
            visualObject.SetActive(false);
        }
        
        Debug.Log("Pills picked up!");
    }
    
    // Public method to check if this pickup is still available
    public bool IsAvailable()
    {
        return !isPicked;
    }
    
    void OnDrawGizmosSelected()
    {
        if (!showGizmos || isPicked) return;
        
        // Draw interaction range
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
        
        // Draw line to player if in range
        if (Application.isPlaying && playerInRange && playerTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, playerTransform.position);
        }
    }
}

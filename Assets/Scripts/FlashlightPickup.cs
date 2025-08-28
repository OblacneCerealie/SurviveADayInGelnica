using UnityEngine;

public class FlashlightPickup : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionRange = 3f;
    public Transform playerTransform;
    public KeyCode interactionKey = KeyCode.E;
    
    [Header("Visual Feedback")]
    public GameObject visualObject; // The flashlight cube
    public Light flashlightLight; // The light component
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    public bool showGizmos = true;
    
    private bool playerInRange = false;
    private bool isPicked = false;
    
    void Start()
    {
        InitializeFlashlight();
    }
    
    void InitializeFlashlight()
    {
        // Find player if not assigned
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                if (showDebugInfo)
                {

                }
            }
            else
            {

            }
        }
        
        // Use this GameObject as visual if not specified
        if (visualObject == null)
        {
            visualObject = gameObject;
        }
        
        // Find the light component if not assigned
        if (flashlightLight == null)
        {
            flashlightLight = GetComponentInChildren<Light>();
            if (flashlightLight == null)
            {

            }
        }
        
        // Make sure the light starts OFF
        if (flashlightLight != null)
        {
            flashlightLight.enabled = false;
        }
        
        // Ensure there's a collider for interaction
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            BoxCollider boxCol = gameObject.AddComponent<BoxCollider>();
            boxCol.isTrigger = false;
            if (showDebugInfo)
            {

            }
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
        
        if (playerInRange && !wasInRange && showDebugInfo)
        {

        }
    }
    
    void HandleKeyboardInput()
    {
        if (playerInRange && Input.GetKeyDown(interactionKey))
        {
            PickupFlashlight();
        }
    }
    
    void PickupFlashlight()
    {
        if (isPicked) return;
        
        isPicked = true;
        
        // Add flashlight to player inventory
        PlayerInventory inventory = FindFirstObjectByType<PlayerInventory>();
        if (inventory != null)
        {
            inventory.AddFlashlight();
            
            // Give the flashlight light component to the player's flashlight controller
            FlashlightController controller = FindFirstObjectByType<FlashlightController>();
            if (controller != null)
            {
                controller.SetFlashlightLight(flashlightLight);
            }
        }
        else
        {

        }
        
        // Hide the visual object but don't destroy it yet (light component might be needed)
        if (visualObject != null)
        {
            visualObject.SetActive(false);
        }
        
        if (showDebugInfo)
        {

        }
        
        // Destroy this pickup object after a short delay
        Destroy(gameObject, 0.1f);
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
        Gizmos.color = playerInRange ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
        
        // Draw flashlight indicator
        Gizmos.color = Color.white;
        Gizmos.DrawIcon(transform.position + Vector3.up * 2f, "d_Light Icon", true);
        
        // Draw line to player if in range
        if (Application.isPlaying && playerInRange && playerTransform != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, playerTransform.position);
        }
    }
}

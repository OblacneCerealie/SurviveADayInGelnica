using UnityEngine;

public class BatteryPickup : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionRange = 3f;
    public Transform playerTransform;
    public KeyCode interactionKey = KeyCode.E;
    
    [Header("Battery Settings")]
    [SerializeField] private float batteryRecharge = 25f; // How much battery this pickup restores
    
    [Header("Visual Feedback")]
    public GameObject visualObject; // The battery model
    public Material batteryMaterial;
    public Renderer batteryRenderer;
    
    [Header("Audio")]
    public AudioClip pickupSound;
    public AudioSource audioSource;
    
    [Header("Effects")]
    public ParticleSystem pickupEffect;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    public bool showGizmos = true;
    
    private bool playerInRange = false;
    private bool isPicked = false;
    
    void Start()
    {
        InitializeBattery();
    }
    
    void InitializeBattery()
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
                    Debug.Log("BatteryPickup: Automatically found player with 'Player' tag");
                }
            }
            else
            {
                Debug.LogError("BatteryPickup: No player found! Please assign playerTransform or tag your player with 'Player'");
            }
        }
        
        // Use this GameObject as visual if not specified
        if (visualObject == null)
        {
            visualObject = gameObject;
        }
        
        // Get renderer if not assigned
        if (batteryRenderer == null)
        {
            batteryRenderer = visualObject.GetComponent<Renderer>();
        }
        
        // Setup audio source
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Ensure there's a collider for interaction
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            BoxCollider boxCol = gameObject.AddComponent<BoxCollider>();
            boxCol.isTrigger = false;
            if (showDebugInfo)
            {
                Debug.Log("BatteryPickup: Added BoxCollider component");
            }
        }
        
        // Apply battery material if assigned
        if (batteryMaterial != null && batteryRenderer != null)
        {
            batteryRenderer.material = batteryMaterial;
        }
        
        // Add some visual indication this is a battery (optional color)
        if (batteryRenderer != null && batteryMaterial == null)
        {
            batteryRenderer.material.color = Color.yellow; // Classic battery color
        }
    }
    
    void Update()
    {
        if (isPicked) return;
        
        CheckPlayerDistance();
        HandleKeyboardInput();
        // AnimateBattery(); // Removed automatic animation
    }
    
    void CheckPlayerDistance()
    {
        if (playerTransform == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        bool wasInRange = playerInRange;
        playerInRange = distanceToPlayer <= interactionRange;
        
        if (playerInRange && !wasInRange && showDebugInfo)
        {
            Debug.Log($"Battery nearby - Press {interactionKey} to pick up (+{batteryRecharge}% flashlight battery)");
        }
    }
    
    void HandleKeyboardInput()
    {
        if (playerInRange && Input.GetKeyDown(interactionKey))
        {
            PickupBattery();
        }
    }
    
    // Removed AnimateBattery() method - no more bobbing animation
    
    void PickupBattery()
    {
        if (isPicked) return;
        
        if (showDebugInfo)
        {
            Debug.Log("BatteryPickup: Attempting to pickup battery...");
        }
        
        // Check if player has flashlight
        PlayerInventory inventory = FindFirstObjectByType<PlayerInventory>();
        if (inventory == null)
        {
            if (showDebugInfo)
            {
                Debug.LogError("BatteryPickup: No PlayerInventory found!");
            }
            return;
        }
        
        if (!inventory.HasFlashlight())
        {
            if (showDebugInfo)
            {
                Debug.Log("BatteryPickup: You need a flashlight first!");
            }
            return;
        }
        
        // Check if flashlight battery system exists
        if (FlashlightBattery.Instance == null)
        {
            Debug.LogError("BatteryPickup: No FlashlightBattery system found in scene!");
            return;
        }
        
        if (showDebugInfo)
        {
            Debug.Log("BatteryPickup: All checks passed, applying battery...");
        }
        
        // Check if battery is already full
        if (FlashlightBattery.Instance.GetCurrentBattery() >= FlashlightBattery.Instance.GetMaxBattery())
        {
            if (showDebugInfo)
            {
                Debug.Log("BatteryPickup: Flashlight battery is already full!");
            }
            return;
        }
        
        isPicked = true;
        
        // Apply battery recharge
        FlashlightBattery.Instance.UseBatteryPickup();
        
        // Play pickup sound
        PlayPickupSound();
        
        // Play pickup effect
        PlayPickupEffect();
        
        // Hide visual object
        if (visualObject != null)
        {
            visualObject.SetActive(false);
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"Battery picked up! Recharged flashlight by {batteryRecharge}%");
        }
        
        // Destroy this pickup object after effects
        Destroy(gameObject, 1f);
    }
    
    void PlayPickupSound()
    {
        if (audioSource != null && pickupSound != null)
        {
            audioSource.clip = pickupSound;
            audioSource.Play();
        }
    }
    
    void PlayPickupEffect()
    {
        if (pickupEffect != null)
        {
            pickupEffect.Play();
        }
    }
    
    // Public method to check if this pickup is still available
    public bool IsAvailable()
    {
        return !isPicked;
    }
    
    // Public method to set battery recharge amount
    public void SetBatteryRecharge(float amount)
    {
        batteryRecharge = amount;
    }
    
    void OnDrawGizmosSelected()
    {
        if (!showGizmos || isPicked) return;
        
        // Draw interaction range
        Gizmos.color = playerInRange ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
        
        // Draw battery indicator
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one * 0.3f);
        
        // Draw recharge amount indicator
        Gizmos.color = Color.cyan;
        Vector3 textPos = transform.position + Vector3.up * 2.5f;
        
        // Draw line to player if in range
        if (Application.isPlaying && playerInRange && playerTransform != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, playerTransform.position);
        }
    }
    
    // Context menu methods for testing
    [ContextMenu("Test: Pickup Battery")]
    public void TestPickupBattery()
    {
        PickupBattery();
    }
    
    [ContextMenu("Test: Set Recharge to 50%")]
    public void TestSetHighRecharge()
    {
        SetBatteryRecharge(50f);
        Debug.Log("Battery recharge set to 50%");
    }
}

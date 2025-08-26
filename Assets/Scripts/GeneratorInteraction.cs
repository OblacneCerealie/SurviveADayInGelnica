using UnityEngine;
using System.Collections;

public class GeneratorInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionRange = 3f;
    public Transform playerTransform;
    public KeyCode interactionKey = KeyCode.E;
    
    [Header("Animation Settings")]
    public float shakeDuration = 4f;
    public float shakeIntensity = 0.1f;
    public AnimationCurve shakeIntensityCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    
    [Header("Drop Settings")]
    public GameObject prefabToDrop;
    public Transform dropPoint;
    public float dropForce = 5f;
    public bool useFixedHeight = false;
    public float fixedDropHeight = 0f; // Set this to your floor Y position
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    public bool showGizmos = true;
    
    private bool playerInRange = false;
    private bool isShaking = false;
    private Vector3 originalPosition;
    
    void Start()
    {
        // Store original position for shake animation
        originalPosition = transform.position;
        
        // Find player if not assigned
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                if (showDebugInfo)
                {
                    Debug.Log("CubeInteraction: Automatically found player with 'Player' tag");
                }
            }
            else
            {
                Debug.LogError("CubeInteraction: No player found! Please assign playerTransform or tag your player with 'Player'");
            }
        }
        
        // Set default drop point if not assigned
        if (dropPoint == null)
        {
            dropPoint = transform;
            if (showDebugInfo)
            {
                Debug.Log("CubeInteraction: Using cube transform as drop point");
            }
        }
        
        // Validate prefab
        if (prefabToDrop == null)
        {
            Debug.LogWarning("CubeInteraction: No prefab assigned to drop!");
        }
    }
    
    void Update()
    {
        if (!isShaking)
        {
            CheckPlayerDistance();
            HandleInteractionInput();
        }
    }
    
    void CheckPlayerDistance()
    {
        if (playerTransform == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        bool wasInRange = playerInRange;
        playerInRange = distanceToPlayer <= interactionRange;
        
        if (playerInRange && !wasInRange && showDebugInfo)
        {
            Debug.Log("Cube nearby - Press E to interact");
        }
    }
    
    void HandleInteractionInput()
    {
        if (playerInRange && Input.GetKeyDown(interactionKey))
        {
            StartShakeAndDrop();
        }
    }
    
    void StartShakeAndDrop()
    {
        if (isShaking) return;
        
        if (showDebugInfo)
        {
            Debug.Log("Starting cube shake animation...");
        }
        
        StartCoroutine(ShakeAndDropSequence());
    }
    
    IEnumerator ShakeAndDropSequence()
    {
        isShaking = true;
        float elapsed = 0f;
        
        // Shake animation for specified duration
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / shakeDuration;
            
            // Use animation curve to control shake intensity over time
            float currentIntensity = shakeIntensity * shakeIntensityCurve.Evaluate(progress);
            
            // Generate random shake offset
            Vector3 randomOffset = new Vector3(
                Random.Range(-currentIntensity, currentIntensity),
                Random.Range(-currentIntensity, currentIntensity),
                Random.Range(-currentIntensity, currentIntensity)
            );
            
            // Apply shake
            transform.position = originalPosition + randomOffset;
            
            yield return null;
        }
        
        // Return to original position
        transform.position = originalPosition;
        
        if (showDebugInfo)
        {
            Debug.Log("Shake animation completed. Dropping prefab...");
        }
        
        // Drop the prefab
        DropPrefab();
        
        isShaking = false;
    }
    
    void DropPrefab()
    {
        Debug.Log("DropPrefab() method called!");
        
        if (prefabToDrop == null)
        {
            Debug.LogError("GeneratorInteraction: Cannot drop prefab - no prefab assigned! Please assign a prefab in the inspector.");
            return;
        }
        
        Debug.Log($"About to drop prefab: {prefabToDrop.name}");
        
        Vector3 dropPosition;
        
        if (useFixedHeight)
        {
            // Use manually set height
            dropPosition = new Vector3(
                dropPoint.position.x + 1f, 
                fixedDropHeight, 
                dropPoint.position.z
            );
            Debug.Log($"Using fixed height: {fixedDropHeight}, dropping at: {dropPosition}");
        }
        else
        {
            // Calculate drop position at ground level using raycast
            Vector3 startPosition = dropPoint.position + Vector3.forward * 1f + Vector3.up * 2f;
            
            // Raycast down to find the ground
            if (Physics.Raycast(startPosition, Vector3.down, out RaycastHit hit, 10f))
            {
                // Drop slightly above the ground surface
                dropPosition = hit.point + Vector3.up * 0.2f;
                Debug.Log($"Ground found at: {hit.point}, dropping at: {dropPosition}");
            }
            else
            {
                // Fallback: drop at generator height
                dropPosition = dropPoint.position + Vector3.forward * 1f;
                Debug.Log("No ground found, using generator position as fallback");
            }
        }
        Debug.Log($"Drop position calculated: {dropPosition}");
        
        // Instantiate the prefab
        GameObject droppedObject = Instantiate(prefabToDrop, dropPosition, Quaternion.identity);
        Debug.Log($"Prefab instantiated: {droppedObject.name}");
        
        // Add physics if the prefab doesn't have a Rigidbody
        Rigidbody rb = droppedObject.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = droppedObject.AddComponent<Rigidbody>();
            Debug.Log("Added Rigidbody to dropped object");
        }
        else
        {
            Debug.Log("Dropped object already has Rigidbody");
        }
        
        // Apply a small downward force for realistic drop
        if (rb != null)
        {
            Vector3 dropDirection = Vector3.down + new Vector3(
                Random.Range(-0.1f, 0.1f), 
                0f, 
                Random.Range(-0.1f, 0.1f)
            );
            rb.AddForce(dropDirection * dropForce, ForceMode.Impulse);
            Debug.Log($"Applied force to dropped object: {dropDirection * dropForce}");
        }
        
        Debug.Log($"Successfully dropped prefab: {prefabToDrop.name} at position: {dropPosition}");
    }
    
    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;
        
        // Draw interaction range
        Gizmos.color = playerInRange ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
        
        // Draw drop point
        if (dropPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(dropPoint.position, 0.2f);
            Gizmos.DrawLine(dropPoint.position, dropPoint.position + Vector3.down * 1f);
        }
    }
}

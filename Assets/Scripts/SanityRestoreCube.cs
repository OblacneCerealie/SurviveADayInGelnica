using UnityEngine;

public class SanityRestoreCube : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionRange = 3f;
    public KeyCode interactionKey = KeyCode.Q;
    public int sanityRestoreAmount = 5;
    
    [Header("Testing")]
    public KeyCode testMonsterKey = KeyCode.T; // Press T to test monster activation
    
    [Header("Visual Feedback")]
    public Color normalColor = Color.white;
    public Color highlightColor = Color.green;
    
    [Header("Debug")]
    public bool showGizmos = true;
    public bool debugLogs = true;
    
    private Transform playerTransform;
    private bool playerInRange = false;
    private Renderer cubeRenderer;
    private Material cubeMaterial;
    
    void Start()
    {
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            if (debugLogs)
            {
                Debug.Log("SanityRestoreCube: Found player");
            }
        }
        else
        {
            Debug.LogError("SanityRestoreCube: Could not find player with 'Player' tag!");
        }
        
        // Get renderer for visual feedback
        cubeRenderer = GetComponent<Renderer>();
        if (cubeRenderer != null)
        {
            // Create a copy of the material to avoid modifying the original
            cubeMaterial = new Material(cubeRenderer.material);
            cubeRenderer.material = cubeMaterial;
            cubeMaterial.color = normalColor;
        }
        
        if (debugLogs)
        {
            Debug.Log($"SanityRestoreCube: Initialized. Press {interactionKey} when near to restore {sanityRestoreAmount} sanity.");
        }
    }
    
    void Update()
    {
        CheckPlayerDistance();
        
        if (playerInRange && Input.GetKeyDown(interactionKey))
        {
            RestoreSanity();
        }
        
        if (playerInRange && Input.GetKeyDown(testMonsterKey))
        {
            TestMonsterActivation();
        }
    }
    
    void CheckPlayerDistance()
    {
        if (playerTransform == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        bool wasInRange = playerInRange;
        playerInRange = distanceToPlayer <= interactionRange;
        
        // Update visual feedback
        if (cubeRenderer != null && cubeMaterial != null)
        {
            cubeMaterial.color = playerInRange ? highlightColor : normalColor;
        }
        
        // Show interaction prompt
        if (playerInRange && !wasInRange)
        {
            if (debugLogs)
            {
                Debug.Log($"SanityRestoreCube: Player in range - Press {interactionKey} to restore sanity");
            }
        }
    }
    
    void RestoreSanity()
    {
        // Find SanityManager and restore sanity
        SanityManager sanityManager = SanityManager.Instance;
        if (sanityManager != null)
        {
            int currentSanity = sanityManager.GetCurrentSanity();
            sanityManager.SetSanity(sanityRestoreAmount);
            
            if (debugLogs)
            {
                Debug.Log($"SanityRestoreCube: Restored sanity from {currentSanity} to {sanityRestoreAmount}");
            }
            
            // Visual feedback - flash the cube
            StartCoroutine(FlashCube());
        }
        else
        {
            Debug.LogError("SanityRestoreCube: Could not find SanityManager!");
        }
    }
    
    void TestMonsterActivation()
    {
        // Set sanity to 0 to activate monster
        SanityManager sanityManager = SanityManager.Instance;
        if (sanityManager != null)
        {
            int currentSanity = sanityManager.GetCurrentSanity();
            sanityManager.SetSanity(0);
            
            if (debugLogs)
            {
                Debug.Log($"SanityRestoreCube: TEST - Set sanity from {currentSanity} to 0 (Monster should activate, patients should freeze)");
            }
            
            // Visual feedback - flash red
            StartCoroutine(FlashCubeRed());
        }
        else
        {
            Debug.LogError("SanityRestoreCube: Could not find SanityManager!");
        }
    }
    
    System.Collections.IEnumerator FlashCube()
    {
        if (cubeRenderer == null || cubeMaterial == null) yield break;
        
        // Flash white briefly
        Color originalColor = cubeMaterial.color;
        cubeMaterial.color = Color.white;
        
        yield return new WaitForSeconds(0.1f);
        
        cubeMaterial.color = originalColor;
    }
    
    System.Collections.IEnumerator FlashCubeRed()
    {
        if (cubeRenderer == null || cubeMaterial == null) yield break;
        
        // Flash red briefly
        Color originalColor = cubeMaterial.color;
        cubeMaterial.color = Color.red;
        
        yield return new WaitForSeconds(0.2f);
        
        cubeMaterial.color = originalColor;
    }
    
    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;
        
        // Draw interaction range
        Gizmos.color = playerInRange ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
        
        // Draw line to player if in range
        if (Application.isPlaying && playerInRange && playerTransform != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, playerTransform.position);
        }
    }
    
    void OnDestroy()
    {
        // Clean up material
        if (cubeMaterial != null)
        {
            DestroyImmediate(cubeMaterial);
        }
    }
}

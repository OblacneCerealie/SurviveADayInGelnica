using UnityEngine;
using TMPro;

public class BedInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionRange = 2f;
    public Transform playerTransform;
    public KeyCode interactionKey = KeyCode.E;
    
    [Header("UI Elements")]
    public TextMeshProUGUI interactionText;
    
    [Header("Look Detection")]
    public Camera playerCamera;
    public LayerMask bedLayerMask = 1; // Default layer
    public float maxLookDistance = 5f;
    
    [Header("Game References")]
    public GameTimeUI gameTimeUI;
    
    [Header("Debug")]
    public bool showGizmos = true;
    
    private bool playerInRange = false;
    private bool isInteracting = false;
    private bool hasUsedBedThisVisit = false;
    private bool playerLookingAtBed = false;
    
    void Start()
    {
        // Find player if not assigned
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                Debug.Log("BedInteraction: Automatically found player with 'Player' tag");
            }
            else
            {
                Debug.LogWarning("BedInteraction: No player found! Make sure your player GameObject has the 'Player' tag, or assign Player Transform manually.");
            }
        }
        
        // Find player camera if not assigned
        if (playerCamera == null && playerTransform != null)
        {
            playerCamera = playerTransform.GetComponentInChildren<Camera>();
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }
        }
        
        // Find GameTimeUI if not assigned
        if (gameTimeUI == null)
        {
            gameTimeUI = FindFirstObjectByType<GameTimeUI>();
        }
        
        // Validate required components
        if (interactionText == null)
        {
            Debug.LogError("BedInteraction: No interaction text assigned!");
        }
        
        if (gameTimeUI == null)
        {
            Debug.LogError("BedInteraction: No GameTimeUI found in scene!");
        }
        
        // Hide interaction text initially
        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }
    }
    
    void Update()
    {
        CheckPlayerDistance();
        CheckPlayerLookDirection();
        HandleKeyboardInput();
    }
    
    void CheckPlayerDistance()
    {
        if (playerTransform == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        bool wasInRange = playerInRange;
        playerInRange = distanceToPlayer <= interactionRange;
        
        // Show/hide UI when player enters/exits range
        if (playerInRange && !wasInRange)
        {
            // Player entered the zone - reset bed usage for this visit
            hasUsedBedThisVisit = false;
            UpdateInteractionUI();
        }
        else if (!playerInRange && wasInRange)
        {
            // Player left the zone
            HideInteractionUI();
        }
    }
    
    void CheckPlayerLookDirection()
    {
        if (playerCamera == null) return;
        
        // Cast a ray from camera forward
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        
        bool wasLookingAtBed = playerLookingAtBed;
        playerLookingAtBed = false;
        
        if (Physics.Raycast(ray, out hit, maxLookDistance, bedLayerMask))
        {
            // Check if we hit this bed object
            if (hit.collider.gameObject == gameObject)
            {
                playerLookingAtBed = true;
            }
        }
        
        // Update UI visibility based on look direction
        if (playerLookingAtBed != wasLookingAtBed)
        {
            UpdateInteractionUI();
        }
    }
    
    void HandleKeyboardInput()
    {
        if (playerInRange && playerLookingAtBed && Input.GetKeyDown(interactionKey) && !hasUsedBedThisVisit)
        {
            InteractWithBed();
        }
    }
    
    void UpdateInteractionUI()
    {
        if (interactionText != null)
        {
            // Show text only if: in range, looking at bed, and hasn't used bed this visit
            bool shouldShow = playerInRange && playerLookingAtBed && !hasUsedBedThisVisit;
            interactionText.gameObject.SetActive(shouldShow);
            
            if (shouldShow)
            {
                Debug.Log("Player looking at bed - Press E to advance day");
            }
        }
    }
    
    void HideInteractionUI()
    {
        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }
        
        isInteracting = false;
    }
    
    void InteractWithBed()
    {
        if (isInteracting || hasUsedBedThisVisit) return;
        
        isInteracting = true;
        hasUsedBedThisVisit = true;
        SleepAndAdvanceDay();
    }
    
    void SleepAndAdvanceDay()
    {
        if (gameTimeUI == null)
        {
            Debug.LogError("Cannot advance day - GameTimeUI reference is missing!");
            isInteracting = false;
            return;
        }
        
        // Check if we can advance day
        if (gameTimeUI.currentDay >= gameTimeUI.maxDay)
        {
            Debug.Log("Cannot sleep - Maximum day reached!");
            isInteracting = false;
            return;
        }
        
        // Advance the day
        gameTimeUI.AdvanceDay();
        
        // Hide UI after interaction
        HideInteractionUI();
        
        // Optional: Add sleep effect/animation here
        Debug.Log($"Player used bed! Advanced to day {gameTimeUI.currentDay}. Leave and return to use again.");
        
        // Reset interaction state after a short delay
        Invoke(nameof(ResetInteraction), 1f);
    }
    
    void ResetInteraction()
    {
        isInteracting = false;
    }
    
    // Public methods for external control
    public void SetPlayerTransform(Transform player)
    {
        playerTransform = player;
    }
    
    public void SetGameTimeUI(GameTimeUI timeUI)
    {
        gameTimeUI = timeUI;
    }
    
    public bool IsPlayerInRange()
    {
        return playerInRange;
    }
    
    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;
        
        // Draw interaction range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
        
        // Draw line to player if in range
        if (Application.isPlaying && playerInRange && playerTransform != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, playerTransform.position);
        }
        
        // Draw interaction icon
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position + Vector3.up, Vector3.one * 0.5f);
    }
    
    void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        // Always show interaction range when selected
        Gizmos.color = Color.cyan;
        Gizmos.color = new Color(0, 1, 1, 0.1f); // Transparent cyan
        Gizmos.DrawSphere(transform.position, interactionRange);
    }
}
